using System.CommandLine;
using System.Text;
using System.Text.RegularExpressions;
using Mscc.GenerativeAI;
using Polly;
using Polly.Retry;

// ── CLI 參數定義 ──────────────────────────────────────────────────────────────
var sourceOption = new Option<string>(
    "--source-dir", () => "en", "英文原始目錄");

var targetOption = new Option<string>(
    "--target-dir", () => "zh-Hant", "繁體中文輸出目錄");

var apiKeyOption = new Option<string>(
    "--api-key", () => Environment.GetEnvironmentVariable("GEMINI_API_KEY") ?? "",
    "Gemini API Key（多組以逗號分隔）");

var specificFileOption = new Option<string>(
    "--specific-file", () => "", "只翻譯單一檔案（選填）");

var forceOption = new Option<bool>(
    "--force", () => false, "強制重新翻譯（忽略已存在的 zh-Hant 檔案）");

var sinceHashOption = new Option<string>(
    "--since-hash", () => "", "上游同步基準 commit hash（用於 git diff 偵測異動）");

var rootCommand = new RootCommand("nopCommerce Docs 繁體中文自動翻譯工具");
rootCommand.AddOption(sourceOption);
rootCommand.AddOption(targetOption);
rootCommand.AddOption(apiKeyOption);
rootCommand.AddOption(specificFileOption);
rootCommand.AddOption(forceOption);
rootCommand.AddOption(sinceHashOption);

rootCommand.SetHandler(async (sourceDir, targetDir, apiKey, specificFile, force, sinceHash) =>
{
    if (string.IsNullOrWhiteSpace(apiKey))
    {
        Console.Error.WriteLine("❌ 缺少 GEMINI_API_KEY");
        Environment.Exit(1);
    }

    var translator = new Translator(apiKey, sourceDir, targetDir, force);
    await translator.RunAsync(specificFile, sinceHash);

}, sourceOption, targetOption, apiKeyOption, specificFileOption, forceOption, sinceHashOption);

return await rootCommand.InvokeAsync(args);


// ── PlaceholderContext ─────────────────────────────────────────────────────────
public class PlaceholderContext
{
    private static readonly Regex _reFencedCodeBlock = new(
        @"(?m)^([ \t]*)(```|~~~)[^\n]*\n[\s\S]*?\n\1\2[ \t]*$",
        RegexOptions.Compiled);

    private static readonly Regex _reYamlFrontMatter = new(
        @"\A---\n([\s\S]*?)\n---[ \t]*\n?",
        RegexOptions.Compiled);

    private static readonly Regex _reUidLine = new(
        @"^\s*uid\s*:",
        RegexOptions.Compiled);

    private static readonly Regex _reLiquidTag = new(
        @"\{%-?[\s\S]*?-?%\}|\{\{[\s\S]*?\}\}",
        RegexOptions.Compiled);

    private static readonly Regex _reHtmlTag = new(
        @"<[a-zA-Z/][^>]*?>",
        RegexOptions.Compiled);

    private static readonly Regex _reMarkdownLink = new(
        @"(!?)\[([^\]]*)\]\(([^)]+)\)",
        RegexOptions.Compiled);

    private static readonly HashSet<string> _placeholderLinkWords = new(StringComparer.OrdinalIgnoreCase)
    {
        "", "here", "this", "link", "this link", "click here",
        "read more", "more", "see here", "see"
    };

    private readonly Dictionary<string, string> _map = new();
    private int _counter;

    public string Extract(string content)
    {
        content = ProtectFencedCodeBlocks(content);
        content = ProtectYamlFrontMatter(content);
        content = ProtectLiquidTags(content);
        content = ProtectHtmlTags(content);
        content = ProtectMarkdownUrls(content);
        return content;
    }

    public string Restore(string content)
    {
        var entries = _map.ToList();
        for (int i = entries.Count - 1; i >= 0; i--)
            content = content.Replace(entries[i].Key, entries[i].Value);
        return content;
    }

    private string NextPlaceholder() => $"[[PROTECT_{_counter++:D4}]]";

    private string Store(string original)
    {
        var key = NextPlaceholder();
        _map[key] = original;
        return key;
    }

    private string ProtectFencedCodeBlocks(string content)
        => _reFencedCodeBlock.Replace(content, m => Store(m.Value));

    private string ProtectYamlFrontMatter(string content)
    {
        bool hasCrLf = content.Contains("\r\n");
        content = content.Replace("\r\n", "\n");

        content = _reYamlFrontMatter.Replace(content, m =>
        {
            var body = m.Groups[1].Value;
            var lines = body.Split('\n');
            var processed = lines.Select(line =>
                _reUidLine.IsMatch(line) ? Store(line) : line);
            return $"---\n{string.Join("\n", processed)}\n---\n";
        });

        if (hasCrLf)
            content = content.Replace("\n", "\r\n");

        return content;
    }

    private string ProtectLiquidTags(string content)
        => _reLiquidTag.Replace(content, m => Store(m.Value));

    private string ProtectHtmlTags(string content)
        => _reHtmlTag.Replace(content, m => Store(m.Value));

    private string ProtectMarkdownUrls(string content)
    {
        return _reMarkdownLink.Replace(content, m =>
        {
            var isImage  = m.Groups[1].Value == "!";
            var textPart = m.Groups[2].Value;
            var url      = m.Groups[3].Value;

            if (isImage)
                return Store(m.Value);

            if (_placeholderLinkWords.Contains(textPart.Trim()))
                return Store(m.Value);

            return $"[{textPart}]({Store(url)})";
        });
    }
}


// ── Translator ────────────────────────────────────────────────────────────────
public class Translator
{
    private const int CooldownMs = 8_000;           // ← 改為 8 秒
    private const int ChunkThreshold = 5_000;
    private const string ModelName = "gemini-3.1-flash-lite-preview";
    private const int QuotaWaitMs = 66_000;

    private readonly string _sourceDir;
    private readonly string _targetDir;
    private readonly bool _force;

    private readonly List<string> _apiKeys;
    private int _currentKeyIndex;
    private GenerativeModel _model;

    public Translator(string apiKeyCsv, string sourceDir, string targetDir, bool force)
    {
        _sourceDir = sourceDir;
        _targetDir = targetDir;
        _force = force;

        var allKeys = (apiKeyCsv ?? "")
            .Split(new[] { ',', '|' }, StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries)
            .Distinct()
            .ToList();

        _apiKeys = allKeys.Where(k => k.Length >= 30 && k.Length <= 50).ToList();

        var invalidCount = allKeys.Count - _apiKeys.Count;
        if (invalidCount > 0)
            Console.WriteLine($"  ⚠️  已過濾 {invalidCount} 組長度不合法的 Key（可能含換行符或格式有誤）");

        if (_apiKeys.Count == 0)
        {
            Console.Error.WriteLine("❌ 沒有提供有效的 GEMINI_API_KEY（長度需介於 30-50 字元）");
            Console.Error.WriteLine($"   原始 Key 數量：{allKeys.Count}，Key 長度：{string.Join(", ", allKeys.Select(k => k.Length))}");
            Environment.Exit(1);
        }

        Console.WriteLine($"🔑 已載入 {_apiKeys.Count} 組 API Key");

        _currentKeyIndex = 0;
        _model = CreateModel(_apiKeys[_currentKeyIndex]);
    }

    private static string MaskKey(string key)
    {
        if (string.IsNullOrEmpty(key)) return "(empty)";
        return key.Length <= 10 ? "..." : $"{key[..8]}...{key[^4..]}";
    }

    private static GenerativeModel CreateModel(string apiKey)
        => new GoogleAI(apiKey).GenerativeModel(model: ModelName);

    private bool SwitchToNextKey()
    {
        _currentKeyIndex++;
        if (_currentKeyIndex >= _apiKeys.Count)
            return false;

        Console.WriteLine($"\n  🔄 切換到第 {_currentKeyIndex + 1}/{_apiKeys.Count} 組 API Key（{MaskKey(_apiKeys[_currentKeyIndex])}）");
        _model = CreateModel(_apiKeys[_currentKeyIndex]);
        return true;
    }

    private readonly AsyncRetryPolicy _retryPolicy = Policy
        .Handle<Exception>(ex =>
            ex.Message.Contains("503") ||
            ex.Message.Contains("RESOURCE_EXHAUSTED"))
        .WaitAndRetryAsync(
            retryCount: 2,
            sleepDurationProvider: attempt => TimeSpan.FromSeconds(Math.Pow(2, attempt) * 5),
            onRetry: (ex, wait, attempt, _) =>
                Console.WriteLine($"  ⏳ 第 {attempt} 次重試，等待 {wait.TotalSeconds:F0} 秒... ({ex.Message[..Math.Min(60, ex.Message.Length)]})")
        );

    // ── 主流程 ────────────────────────────────────────────────────────────────

    public async Task RunAsync(string specificFile, string sinceHash)
    {
        // 1. 取得上游異動清單（git diff 偵測 added/modified）
        var upstreamChanged = await GetUpstreamChangedFilesAsync(sinceHash);
        if (upstreamChanged.Count > 0)
            Console.WriteLine($"🔍 上游異動：{upstreamChanged.Count} 個檔案");

        // 2. 非 MD 同步：孤兒刪除 + 複製 + 一次性推送
        await SyncNonMarkdownFilesAsync(upstreamChanged);

        // 3. 刪除 MD 孤兒（en/ 已不存在的 zh-Hant/*.md）
        DeleteMdOrphans();

        // 4. 決定需要翻譯的 MD 清單
        List<string> mdFiles;

        if (!string.IsNullOrWhiteSpace(specificFile))
        {
            if (Directory.Exists(specificFile))
            {
                mdFiles = Directory
                    .EnumerateFiles(specificFile, "*.md", SearchOption.AllDirectories)
                    .OrderBy(f => f)
                    .ToList();
                if (mdFiles.Count == 0)
                {
                    Console.Error.WriteLine($"❌ 指定目錄下沒有找到任何 .md 檔案：{specificFile}");
                    Environment.Exit(1);
                }
                Console.WriteLine($"📂 指定目錄：{specificFile}（共 {mdFiles.Count} 個檔案）");
            }
            else if (File.Exists(specificFile))
            {
                mdFiles = [specificFile];
            }
            else
            {
                Console.Error.WriteLine($"❌ 找不到指定的檔案或目錄：{specificFile}");
                Environment.Exit(1);
                return;
            }
        }
        else if (_force)
        {
            mdFiles = Directory
                .EnumerateFiles(_sourceDir, "*.md", SearchOption.AllDirectories)
                .OrderBy(f => f)
                .ToList();
        }
        else
        {
            // 一般模式：zh-Hant 不存在 OR 上游有異動
            mdFiles = Directory
                .EnumerateFiles(_sourceDir, "*.md", SearchOption.AllDirectories)
                .Where(src =>
                {
                    var rel     = Path.GetRelativePath(_sourceDir, src);
                    var dst     = Path.Combine(_targetDir, rel);
                    var gitPath = $"{_sourceDir}/{rel}".Replace('\\', '/');
                    return !File.Exists(dst) || upstreamChanged.Contains(gitPath);
                })
                .OrderBy(f => f)
                .ToList();
        }

        if (mdFiles.Count == 0)
        {
            Console.WriteLine("✅ 沒有需要翻譯的 .md 檔案");
            return;
        }

        Console.WriteLine($"\n📚 需要翻譯：{mdFiles.Count} 個 .md 檔案\n{new string('─', 55)}");

        // 5. 逐一翻譯，每完成一個就 push（方便中斷後恢復）
        int success = 0, failed = 0;
        int consecutiveFailures = 0;
        const int MaxConsecutiveFailures = 5;
        const int PushEvery = 1;
        bool earlyStop = false;
        var failedFiles = new List<string>();

        for (int i = 0; i < mdFiles.Count; i++)
        {
            var sourcePath = mdFiles[i];
            var relPath    = Path.GetRelativePath(_sourceDir, sourcePath);
            var targetPath = Path.Combine(_targetDir, relPath);

            Console.WriteLine($"\n[{i + 1}/{mdFiles.Count}] {relPath}");

            var result = false;
            try
            {
                result = await TranslateFileAsync(sourcePath, targetPath);
            }
            catch (QuotaExhaustedException ex)
            {
                Console.WriteLine($"\n⛔ {ex.Message}");
                Console.WriteLine($"   今日已成功：{success}（使用了 {_currentKeyIndex + 1}/{_apiKeys.Count} 組 Key）");
                Console.WriteLine($"   已翻好的檔案將會 commit，明天排程會繼續補翻。");
                earlyStop = true;
                break;
            }

            if (result)
            {
                success++;
                consecutiveFailures = 0;
            }
            else
            {
                failed++;
                failedFiles.Add(sourcePath);
                consecutiveFailures++;

                if (consecutiveFailures >= MaxConsecutiveFailures)
                {
                    Console.WriteLine($"\n⛔ 連續失敗 {MaxConsecutiveFailures} 次，提早結束以節省 API 配額。");
                    Console.WriteLine($"   已成功：{success}  失敗：{failed}");
                    earlyStop = true;
                    break;
                }

                var attempted = success + failed;
                if (attempted >= 10 && (double)failed / attempted > 0.5)
                {
                    Console.WriteLine($"\n⛔ 失敗率過高（{failed}/{attempted} = {(double)failed / attempted:P0}），提早結束。");
                    earlyStop = true;
                    break;
                }
            }

            if (i < mdFiles.Count - 1)
            {
                var apiDoneAt = DateTime.UtcNow;

                if (result && success % PushEvery == 0)
                    await PushProgressAsync(success);

                var elapsed   = (int)(DateTime.UtcNow - apiDoneAt).TotalMilliseconds;
                var remaining = CooldownMs - elapsed;
                if (remaining > 0)
                    await Task.Delay(remaining);
            }
        }

        Console.WriteLine($"\n{new string('─', 55)}");
        Console.WriteLine($"✅ 成功：{success}  ❌ 失敗：{failed}");

        var failedListPath = Path.Combine(_targetDir, ".translation-failed.txt");
        if (failedFiles.Count > 0)
        {
            await File.WriteAllLinesAsync(failedListPath, failedFiles, new UTF8Encoding(false));
            Console.WriteLine($"\n📋 失敗清單已寫入：{failedListPath}");
            foreach (var f in failedFiles)
                Console.WriteLine($"   - {f}");
        }
        else if (File.Exists(failedListPath))
        {
            File.Delete(failedListPath);
        }

        if (!earlyStop && failed > 0) Environment.Exit(1);
    }

    // ── 上游異動偵測 ──────────────────────────────────────────────────────────

    /// <summary>
    /// 執行 git diff --name-status SINCE HEAD -- en/，回傳 Added/Modified 的路徑集合。
    /// Deleted 由孤兒掃描處理，不在此集合中。
    /// </summary>
    private async Task<HashSet<string>> GetUpstreamChangedFilesAsync(string sinceHash)
    {
        var changed = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
        if (string.IsNullOrWhiteSpace(sinceHash)) return changed;

        try
        {
            var output = await RunGitOutputAsync($"diff --name-status {sinceHash} HEAD -- {_sourceDir}/");
            foreach (var line in output.Split('\n', StringSplitOptions.RemoveEmptyEntries))
            {
                var parts = line.Split('\t', 2);
                if (parts.Length < 2) continue;
                var status = parts[0].Trim();
                if (status == "D") continue; // 刪除由孤兒掃描處理
                var path = parts[1].Trim().Replace('\\', '/');
                changed.Add(path);
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"  ⚠️  無法取得上游異動清單：{ex.Message}（將以檔案存在性判斷）");
        }

        return changed;
    }

    // ── 非 MD 同步 ────────────────────────────────────────────────────────────

    private async Task SyncNonMarkdownFilesAsync(HashSet<string> upstreamChanged)
    {
        // 刪除孤兒非 MD 檔（zh-Hant/ 有但 en/ 已不存在）
        if (Directory.Exists(_targetDir))
        {
            foreach (var targetPath in Directory
                .EnumerateFiles(_targetDir, "*", SearchOption.AllDirectories)
                .Where(f => !f.EndsWith(".md", StringComparison.OrdinalIgnoreCase)
                         && !f.EndsWith(".translation-failed.txt", StringComparison.OrdinalIgnoreCase))
                .ToList())
            {
                var rel     = Path.GetRelativePath(_targetDir, targetPath);
                var srcPath = Path.Combine(_sourceDir, rel);
                if (!File.Exists(srcPath))
                {
                    File.Delete(targetPath);
                    Console.WriteLine($"  🗑️  已刪除孤兒資源：{rel}");
                }
            }
        }

        var nonMdFiles = Directory
            .EnumerateFiles(_sourceDir, "*", SearchOption.AllDirectories)
            .Where(f => !f.EndsWith(".md", StringComparison.OrdinalIgnoreCase))
            .OrderBy(f => f)
            .ToList();

        if (nonMdFiles.Count == 0) return;

        Console.WriteLine("\n📁 同步非 .md 檔案...");
        int copied = 0, skipped = 0;

        foreach (var sourcePath in nonMdFiles)
        {
            var relPath    = Path.GetRelativePath(_sourceDir, sourcePath);
            var targetPath = Path.Combine(_targetDir, relPath);
            var gitPath    = $"{_sourceDir}/{relPath}".Replace('\\', '/');

            bool shouldCopy = _force
                || !File.Exists(targetPath)
                || upstreamChanged.Contains(gitPath);

            if (!shouldCopy) { skipped++; continue; }

            try
            {
                Directory.CreateDirectory(Path.GetDirectoryName(targetPath)!);
                File.Copy(sourcePath, targetPath, overwrite: true);
                copied++;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"  ⚠️  複製失敗 {relPath}：{ex.Message}");
            }
        }

        Console.WriteLine($"  📁 複製：{copied}  略過：{skipped}");

        if (copied == 0) return;

        Console.WriteLine("  💾 推送非 .md 資源...");
        try
        {
            await RunGitAsync("add zh-Hant/");
            try
            {
                await RunGitAsync("commit -m \"📦 同步非文字資源（圖片、PDF 等）\"");
            }
            catch
            {
                Console.WriteLine("  ℹ️  無新變更需要 commit");
                return;
            }
            await RunGitAsync("push origin auto-translate --force");
            Console.WriteLine("  ✅ 推送成功");
        }
        catch (Exception ex)
        {
            Console.WriteLine($"  ⚠️  推送失敗：{ex.Message}（翻譯完成後會一起推送）");
        }
    }

    // ── MD 孤兒清理 ───────────────────────────────────────────────────────────

    private void DeleteMdOrphans()
    {
        if (!Directory.Exists(_targetDir)) return;

        foreach (var targetPath in Directory
            .EnumerateFiles(_targetDir, "*.md", SearchOption.AllDirectories)
            .ToList())
        {
            var rel     = Path.GetRelativePath(_targetDir, targetPath);
            var srcPath = Path.Combine(_sourceDir, rel);
            if (!File.Exists(srcPath))
            {
                File.Delete(targetPath);
                Console.WriteLine($"  🗑️  已刪除孤兒譯文：{rel}");
            }
        }
    }

    // ── 中途推送（MD 翻譯用） ─────────────────────────────────────────────────

    private static int _consecutivePushFailures = 0;
    private const int MaxPushFailures = 3;

    private static async Task PushProgressAsync(int count)
    {
        if (_consecutivePushFailures >= MaxPushFailures)
        {
            if (_consecutivePushFailures == MaxPushFailures)
            {
                Console.WriteLine($"\n  🚫 已連續 {MaxPushFailures} 次 push 失敗，後續不再嘗試中途推送。");
                _consecutivePushFailures++;
            }
            return;
        }

        Console.WriteLine($"\n  💾 中途儲存：已完成 {count} 個，推送至 GitHub...");

        try { await RunGitAsync("add zh-Hant/"); }
        catch (Exception ex)
        {
            Console.WriteLine($"  ⚠️  git add 失敗，跳過本次推送：{ex.Message}");
            _consecutivePushFailures++;
            return;
        }

        try { await RunGitAsync($"commit -m \"🌐 翻譯進度：已完成 {count} 個檔案\""); }
        catch
        {
            Console.WriteLine("  ℹ️  沒有新變更需要 commit");
            return;
        }

        try
        {
            await RunGitAsync("push origin auto-translate --force");
            Console.WriteLine($"  ✅ 中途推送成功");
            _consecutivePushFailures = 0;
        }
        catch (Exception ex)
        {
            Console.WriteLine($"  ⚠️  push 失敗：{ex.Message}");
            _consecutivePushFailures++;
            if (_consecutivePushFailures >= MaxPushFailures)
                Console.WriteLine($"  🚫 已連續 {MaxPush
