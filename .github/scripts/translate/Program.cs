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
                Console.WriteLine($"  🚫 已連續 {MaxPushFailures} 次 push 失敗，後續將停止中途推送。");
        }
    }

    // ── Git 工具 ──────────────────────────────────────────────────────────────

    private static async Task RunGitAsync(string args)
    {
        var psi = new System.Diagnostics.ProcessStartInfo("git", args)
        {
            RedirectStandardOutput = true,
            RedirectStandardError  = true,
            UseShellExecute        = false,
        };
        using var proc = System.Diagnostics.Process.Start(psi)!;

        var stdoutTask = proc.StandardOutput.ReadToEndAsync();
        var stderrTask = proc.StandardError.ReadToEndAsync();

        using var cts = new System.Threading.CancellationTokenSource(TimeSpan.FromSeconds(300));
        try
        {
            await proc.WaitForExitAsync(cts.Token);
        }
        catch (OperationCanceledException)
        {
            proc.Kill(entireProcessTree: true);
            throw new Exception($"git {args} 逾時（超過 300 秒）");
        }

        var err = await stderrTask;
        if (proc.ExitCode != 0)
            throw new Exception($"git {args} 失敗：{err.Trim()}");
    }

    private static async Task<string> RunGitOutputAsync(string args)
    {
        var psi = new System.Diagnostics.ProcessStartInfo("git", args)
        {
            RedirectStandardOutput = true,
            RedirectStandardError  = true,
            UseShellExecute        = false,
        };
        using var proc = System.Diagnostics.Process.Start(psi)!;
        var output = await proc.StandardOutput.ReadToEndAsync();
        await proc.WaitForExitAsync();
        return output;
    }

    // ── 翻譯核心 ──────────────────────────────────────────────────────────────

    private async Task<bool> TranslateFileAsync(string sourcePath, string targetPath)
    {
        string content;
        try { content = await File.ReadAllTextAsync(sourcePath, new UTF8Encoding(false)); }
        catch (Exception ex) { Console.WriteLine($"  ❌ 讀取失敗：{ex.Message}"); return false; }
        content = content.TrimStart('\uFEFF');

        if (content.Trim().Length < 10)
        {
            Console.WriteLine("  ⏭️  內容過短，略過");
            Directory.CreateDirectory(Path.GetDirectoryName(targetPath)!);
            await File.WriteAllTextAsync(targetPath, content, new UTF8Encoding(false));
            return true;
        }

        Console.WriteLine($"  🔤 翻譯中（{content.Length:N0} 字元）...");

        string translated;
        try
        {
            var ctx = new PlaceholderContext();
            var protectedContent = ctx.Extract(content);

            string raw;
            if (content.Length > ChunkThreshold)
                raw = await TranslateInChunksAsync(protectedContent);
            else
                raw = await TranslateWithRetryAsync(protectedContent);

            translated = PostProcess(ctx.Restore(raw));
        }
        catch (QuotaExhaustedException)
        {
            throw;
        }
        catch (Exception ex)
        {
            if (ex.Message.Contains("404") || ex.Message.Contains("NOT_FOUND") || ex.Message.Contains("not found for API"))
            {
                Console.Error.WriteLine($"\n⛔ 致命錯誤：模型不存在或 API 版本不支援，請確認模型名稱。");
                Console.Error.WriteLine($"   錯誤訊息：{ex.Message[..Math.Min(200, ex.Message.Length)]}");
                Environment.Exit(2);
            }
            Console.WriteLine($"  ❌ 翻譯失敗：{ex.Message}");
            if (File.Exists(targetPath))
            {
                File.Delete(targetPath);
                Console.WriteLine($"  🗑️  已刪除半成品，下次排程重翻");
            }
            return false;
        }

        Directory.CreateDirectory(Path.GetDirectoryName(targetPath)!);
        await File.WriteAllTextAsync(targetPath, translated, new UTF8Encoding(false));

        Console.WriteLine($"  ✅ → {targetPath}");
        return true;
    }

    // ── PostProcess ───────────────────────────────────────────────────────────

    private static readonly Regex _reHeadFrontMatter = new(
        @"\A---\n[\s\S]*?\n---[ \t]*\n?", RegexOptions.Compiled);
    private static readonly Regex _reFakeFrontMatter = new(
        @"(?m)^---\n(?:[^\n]*:[^\n]*\n)+---[ \t]*\n?", RegexOptions.Compiled);
    private static readonly Regex _reConsecutiveDashes = new(
        @"(?m)^---[ \t]*\n(?=---[ \t]*\n)", RegexOptions.Compiled);
    private static readonly Regex _reDashBeforeHeading = new(
        @"(?m)^---[ \t]*\n(?=#{1,6} )", RegexOptions.Compiled);
    private static readonly Regex _reBlankLineBeforeHeading = new(
        @"(?m)^(?<prev>[^\n].*)\n(?=#{1,6} )", RegexOptions.Compiled);
    private static readonly Regex _reBlankLineBeforeAlert = new(
        @"(?m)^(?<prev>[^>\n].*)\n(?=> \[!)", RegexOptions.Compiled);
    private static readonly Regex _reXrefEn = new(
        @"xref:en/", RegexOptions.Compiled);
    private static readonly Regex _reUidEn = new(
        @"(uid:\s*)en/", RegexOptions.Compiled);

    private static string PostProcess(string content)
    {
        bool hasCrLf = content.Contains("\r\n");
        content = content.Replace("\r\n", "\n");

        string? headFrontMatter = null;
        var headMatch = _reHeadFrontMatter.Match(content);
        if (headMatch.Success)
        {
            headFrontMatter = headMatch.Value;
            content = content[headMatch.Length..];
        }

        content = _reFakeFrontMatter.Replace(content, "");
        content = _reConsecutiveDashes.Replace(content, "");
        content = _reDashBeforeHeading.Replace(content, "");

        content = _reBlankLineBeforeHeading.Replace(content, "${prev}\n\n");
        content = _reBlankLineBeforeAlert.Replace(content, "${prev}\n\n");

        if (headFrontMatter != null)
        {
            headFrontMatter = _reXrefEn.Replace(headFrontMatter, "xref:zh-Hant/");
            headFrontMatter = _reUidEn.Replace(headFrontMatter, "$1zh-Hant/");
            headFrontMatter = Regex.Replace(headFrontMatter, @"(?m)^author:(?=\s|$)", "作者:");
            headFrontMatter = Regex.Replace(headFrontMatter, @"(?m)^contributors:(?=\s|$)", "貢獻者:");
            headFrontMatter = Regex.Replace(headFrontMatter, @"(?m)^title:(?=\s|$)", "標題:");
        }

        content = _reXrefEn.Replace(content, "xref:zh-Hant/");
        content = _reUidEn.Replace(content, "$1zh-Hant/");

        if (headFrontMatter != null)
            content = headFrontMatter + content;

        if (hasCrLf)
            content = content.Replace("\n", "\r\n");

        return content;
    }

    // ── 翻譯重試與分段 ────────────────────────────────────────────────────────

    private static readonly Regex _rePlaceholder = new(
        @"\[\[PROTECT_\d+\]\]", RegexOptions.Compiled);
    private static readonly Regex _reInputMarker = new(
        @"<<<INPUT>>>\s*\n?", RegexOptions.Compiled);
    private static readonly Regex _reEndMarker = new(
        @"<<<END>>>\s*\n?", RegexOptions.Compiled);
    private static readonly Regex _reMarkdownFenceStart = new(
        @"\A\s*```(?:markdown|md)?\s*\n", RegexOptions.Compiled);
    private static readonly Regex _reMarkdownFenceEnd = new(
        @"\n\s*```\s*\z", RegexOptions.Compiled);

    private async Task<string> TranslateWithRetryAsync(string content)
    {
        const int maxQualityRetries = 3;
        for (int attempt = 1; attempt <= maxQualityRetries; attempt++)
        {
            var translated = await _retryPolicy.ExecuteAsync(() => CallGeminiAsyncWithKeyRetry(content));

            var cleanOriginal   = _rePlaceholder.Replace(content,    "").Trim();
            var cleanTranslated = _rePlaceholder.Replace(translated, "").Trim();

            var enChars = cleanOriginal.Count(c => (c >= 'A' && c <= 'Z') || (c >= 'a' && c <= 'z'));
            var zhChars = cleanTranslated.Count(c => c >= 0x4E00 && c <= 0x9FFF);

            if (cleanOriginal.Length > 10 && enChars > 50 && zhChars == 0)
            {
                Console.WriteLine($"  ⚠️ 翻譯不全（譯文無中文，原文英文:{enChars}）" +
                                  (attempt < maxQualityRetries ? $"，第 {attempt} 次重試..." : "，已達重試上限，放棄。"));
                if (attempt < maxQualityRetries) { await Task.Delay(2000); continue; }
                throw new Exception("Translation failed: no Chinese characters in output.");
            }

            return translated;
        }

        throw new Exception("Translation failed: unexpected state.");
    }

    private async Task<string> CallGeminiAsyncWithKeyRetry(string content)
    {
        try
        {
            return await CallGeminiAsync(content);
        }
        catch (Exception ex) when (IsInvalidKeyError(ex))
        {
            // 401 / 403 / API_KEY_INVALID → Key 本身有問題，直接換下一組
            Console.WriteLine($"    ❌ [Key {_currentKeyIndex + 1}/{_apiKeys.Count}] 驗證失敗（401/403）：{ex.Message[..Math.Min(60, ex.Message.Length)]}");
            if (SwitchToNextKey())
                return await CallGeminiAsync(content);
            throw new QuotaExhaustedException($"所有 {_apiKeys.Count} 組 API Key 都無效或已耗盡。");
        }
        catch (Exception ex) when (ex.Message.Contains("429") || ex.Message.Contains("quota"))
        {
            // 429 → 等待 66s 後以同一組 Key 重試一次；若仍 429 才換 Key
            Console.WriteLine($"    ⚠️  [Key {_currentKeyIndex + 1}/{_apiKeys.Count}] 429 配額耗盡，等待 {QuotaWaitMs / 1000}s 後重試...");
            await QuotaWaitWithProgressAsync();
            Console.WriteLine($"    🔁 [Key {_currentKeyIndex + 1}/{_apiKeys.Count}] 重試中...");
            try
            {
                return await CallGeminiAsync(content);
            }
            catch (Exception retryEx) when (retryEx.Message.Contains("429") || retryEx.Message.Contains("quota"))
            {
                Console.WriteLine($"    ⚠️  [Key {_currentKeyIndex + 1}/{_apiKeys.Count}] 重試後仍 429，切換 Key...");
                if (SwitchToNextKey())
                    return await CallGeminiAsync(content);
                throw new QuotaExhaustedException($"所有 {_apiKeys.Count} 組 API Key 的今日配額皆已耗盡。");
            }
        }
    }

    /// <summary>429 等待期間每 15s 印一次進度，讓使用者知道正在倒數</summary>
    private static async Task QuotaWaitWithProgressAsync()
    {
        const int intervalMs = 15_000;
        int waited = 0;
        while (waited < QuotaWaitMs)
        {
            int next = Math.Min(intervalMs, QuotaWaitMs - waited);
            await Task.Delay(next);
            waited += next;
            if (waited < QuotaWaitMs)
                Console.WriteLine($"    ⏳  配額等待中... {waited / 1000}s / {QuotaWaitMs / 1000}s");
        }
    }

    private static bool IsInvalidKeyError(Exception ex)
    {
        var msg = ex.Message;
        return msg.Contains("API_KEY_INVALID")
            || msg.Contains("API key not valid")
            || msg.Contains("PERMISSION_DENIED")
            || msg.Contains("401")
            || msg.Contains("403");
    }

    private async Task<string> CallGeminiAsync(string content)
    {
        var keyLabel = $"Key {_currentKeyIndex + 1}/{_apiKeys.Count}";

        var prompt = $"""
            {SystemPrompt.Text}
            【任務示範】
            即使段落中包含 [[PROTECT_NNNN]]，也必須翻譯其前後的說明。
            輸入：The [[PROTECT_0001]] is a plugin interface.
            輸出：[[PROTECT_0001]] 是一個外掛介面。
            【目前任務內容】（以下 <<<INPUT>>> 與 <<<END>>> 之間為待翻譯內容，這兩個標記本身不是內容，也不要輸出）
            <<<INPUT>>>
            {content}
            <<<END>>>
            【最終提醒】
            1. 請直接輸出繁體中文翻譯後的 Markdown。禁止保留任何原始英文句子。
            2. 不要輸出 <<<INPUT>>>、<<<END>>> 這兩個標記。
            3. 不要自行新增 YAML front matter（即 --- 開頭與結尾的區塊），除非原文本身就有。
            4. 不要在輸出開頭或結尾加上多餘的 --- 分隔線。
            5. 所有英文的 ## 章節標題、段落說明、清單項目都必須翻譯成繁體中文，不得保留英文原文。
            6. 程式碼區塊（``` 包住的部分）以外，絕對不允許出現整段英文句子或英文段落。
            """;

        // 顯示使用哪組 Key 發出請求，並啟動背景計時器（每 15s 提示仍在等待）
        Console.WriteLine($"    📡 [{keyLabel}] 送出請求...");
        var startTime = DateTime.UtcNow;
        using var cts = new System.Threading.CancellationTokenSource();
        var ticker = Task.Run(async () =>
        {
            try
            {
                while (true)
                {
                    await Task.Delay(15_000, cts.Token);
                    var elapsed = (int)(DateTime.UtcNow - startTime).TotalSeconds;
                    Console.WriteLine($"    ⏱️  [{keyLabel}] 等待回應... {elapsed}s");
                }
            }
            catch (OperationCanceledException) { }
        });

        var response = await _model.GenerateContent(prompt);
        cts.Cancel();
        await ticker;

        var totalSec = (int)(DateTime.UtcNow - startTime).TotalSeconds;
        Console.WriteLine($"    ✅ [{keyLabel}] 收到回應（{totalSec}s）");

        var text = response.Text;
        if (string.IsNullOrWhiteSpace(text))
            throw new Exception("Gemini 回傳空內容");

        var finishReason = response.Candidates?.FirstOrDefault()?.FinishReason;
        if (finishReason?.ToString()?.Contains("MaxTokens", StringComparison.OrdinalIgnoreCase) == true ||
            finishReason?.ToString()?.Contains("MAX_TOKENS", StringComparison.OrdinalIgnoreCase) == true)
        {
            throw new Exception(
                $"Gemini 輸出被截斷（finishReason={finishReason}），段落太大。已輸出 {text.Length} 字元。");
        }

        text = _reInputMarker.Replace(text, "");
        text = _reEndMarker.Replace(text, "");
        text = _reMarkdownFenceStart.Replace(text, "");
        text = _reMarkdownFenceEnd.Replace(text, "");

        return text;
    }

    private async Task<string> TranslateInChunksAsync(string content)
    {
        var sections = SplitSafely(content);

        var results = new List<string>();
        for (int i = 0; i < sections.Count; i++)
        {
            Console.WriteLine($"    段落 {i + 1}/{sections.Count}（{sections[i].Length:N0} 字元）...");
            results.Add(await TranslateWithRetryAsync(sections[i]));
            if (i < sections.Count - 1)
                await Task.Delay(CooldownMs);
        }
        return string.Join("\n\n", results);
    }

    private static List<string> SplitSafely(string content)
    {
        var rawSections = Regex
            .Split(content, @"(?=^#{2,3} )", RegexOptions.Multiline)
            .Where(s => s.Trim().Length > 0)
            .ToList();

        var result = new List<string>();
        foreach (var section in rawSections)
        {
            if (section.Length <= ChunkThreshold)
            {
                result.Add(section);
                continue;
            }
            result.AddRange(SplitOnBlankLines(section));
        }
        return result;
    }

    private static List<string> SplitOnBlankLines(string section)
    {
        var chunks = new List<string>();
        var sb = new StringBuilder();
        bool inCodeBlock = false;

        foreach (var line in section.Split('\n'))
        {
            if (Regex.IsMatch(line, @"^(```|~~~)"))
                inCodeBlock = !inCodeBlock;

            sb.AppendLine(line);

            if (!inCodeBlock && line.Trim().Length == 0 && sb.Length >= ChunkThreshold)
            {
                chunks.Add(sb.ToString().TrimEnd());
                sb.Clear();
            }
        }

        if (sb.Length > 0)
            chunks.Add(sb.ToString().TrimEnd());

        return chunks.Where(c => c.Trim().Length > 0).ToList();
    }
}


// ── System Prompt ─────────────────────────────────────────────────────────────
public static class SystemPrompt
{
    public const string Text = """
        你是一位精通 ASP.NET Core 與 nopCommerce 的資深開發者，同時也是專業的技術文件翻譯員。
        你的任務是將 nopCommerce 英文官方文件翻譯成繁體中文（台灣用語）。

        【核心原則】
        1. 使用台灣繁體中文用語，避免中國大陸用語
        2. 技術名詞若台灣業界常用原文（如 API、SDK、cookie），保留原文
        3. 類別名、介面名、方法名、設定鍵、檔案路徑必須原樣保留，不翻譯也不加空格
           例：IPlugin、BasePlugin、INopStartup、Nop.Core、Configure()、plugin.json、App_Data/Install.txt
        4. 保留所有 Markdown 語法（標題、清單、粗體、連結、表格、引用、程式碼區塊）
        5. 程式碼區塊內（``` 包住的部分）一字不改
        6. 遇到 [[PROTECT_NNNN]] 格式的佔位符，原樣保留不動

        【台灣 vs 中國用語對照（嚴格使用台灣用語）】
        軟體（非「软件/软体」）、程式（非「程序」，除非指 Windows「程序」進程）
        資訊（非「信息」）、設定（非「设置/配置」）、預設（非「默认」）
        網路（非「网络/网路」簡體）、介面（非「接口」，除非指程式介面 interface）
        登入 / 登出（非「登陆 / 退出」）、檔案（非「文件」）、資料（非「数据」）
        伺服器（非「服务器」）、註解（非「注释」）、變數（非「变量」）
        函式（非「函数」）、除錯（非「调试」）、部署（非「发布」）
        範本（非「模板」）、驗證（非「验证」）、最佳化（非「优化」）
        使用者（非「用户」，但面向商店的顧客用「顧客」）
        建立（非「创建」）、顯示（非「显示」簡體字型）

        【技術術語對照】
        Provider        → 提供者（非「提供程序」）
        Middleware      → 中介層
        Mapping         → 對應 / 映射
        Dependency Injection → 相依性注入（非「依賴注入」）
        Repository      → Repository（架構層級，保留原文）
        Factory         → 工廠（設計模式）
        Widget          → 小工具（非「小部件」）
        Cache           → 快取（非「緩存」）
        Entity          → 實體
        Service         → 服務

        【nopCommerce 特有術語】
        Topic           → 內容頁面（注意：不是「主題」，Theme 才是佈景主題）
        Theme           → 佈景主題
        Plugin          → 外掛
        Admin area / Admin panel → 後台 / 管理後台
        Storefront      → 前台網站
        Store           → 商店
        Vendor          → 供應商
        Tier Price      → 階梯價格
        Pickup Point    → 取貨點
        Product Attribute → 商品屬性
        Specification Attribute → 規格屬性
        Reward Points   → 紅利點數
        Customer        → 顧客（非「客戶」或「用戶」）
        Message Template → 訊息範本
        Shopping Cart   → 購物車
        Wishlist        → 願望清單
        Checkout        → 結帳
        Gift Card       → 禮品卡
        Coupon Code     → 優惠碼

        【保留原文（不翻譯）】
        - 縮寫：API、SDK、URL、HTTP、HTTPS、JSON、XML、YAML、CSS、HTML、DOM、UI、UX、CLI、IDE、SEO、SKU、JWT、MVC、REST、GDPR、ACL
        - 產品 / 服務名：PayPal、FedEx、UPS、USPS、Swagger、Redis、NuGet、GitHub、Visual Studio、VS Code、Authorize.NET
        - 技術平台：nopCommerce、ASP.NET Core、.NET、Razor、Entity Framework、Blazor、Liquid
        - 版本號、路徑、檔名：3.90、4.60、~/Plugins/、plugin.json、web.config、Description.txt

        【YAML Front Matter 特別處理】
        - uid 行：原樣保留，不翻譯 key 也不翻譯值
        - author / contributors：如果值是 git 使用者名稱（如 git.AndreiMaz），保留原樣
        - title、description 的值可翻譯

        【輸出規則】
        - 直接輸出翻譯後的完整 Markdown，不要加前言、說明或額外的 ``` 包裝
        - 保持原始換行與段落結構
        - 標題、列表、引用區塊（> [!NOTE]、> [!IMPORTANT]、> [!TIP]、> [!WARNING]）的內文也要翻譯
        - 整段逐句翻譯，不可因技術詞多就整段保留英文

        【翻譯範例】

        範例 1 — 類別名混雜敘述：
        原文：The `IPlugin` interface is the base for all plugins. You can create custom widgets by implementing `IWidgetPlugin`.
        ✅ 正確：`IPlugin` 介面是所有外掛的基礎。您可以透過實作 `IWidgetPlugin` 來建立自訂小工具。
        ❌ 錯誤：`IPlugin`介面是所有插件的基础。您可以通过实现`IWidgetPlugin`来创建自定义小部件。
        （錯處：中國用語「插件 / 基础 / 通过 / 实现 / 创建 / 小部件」、類別名前後應有空格分隔）

        範例 2 — UI 路徑翻譯：
        原文：Configure your payment provider in **Admin → Configuration → Payment methods**.
        ✅ 正確：在 **後台 → 設定 → 付款方式** 中設定您的付款提供者。
        ❌ 錯誤：在**Admin → Configuration → Payment methods**中配置您的付款提供程序。
        （錯處：UI 路徑未翻譯、「配置」與「提供程序」是中國用語）

        範例 3 — 引用區塊：
        原文：> [!IMPORTANT]
        > Make sure "Copy local" is set to **False** for third-party assembly references.
        ✅ 正確：> [!IMPORTANT]
        > 請確保第三方組件參考的「Copy local」屬性設為 **False**。
        （注意：`> [!IMPORTANT]` 標記保留；UI 屬性名 "Copy local" 保留原文加中文引號；值 False 保留原文）

        範例 4 — 產品名與連結：
        原文：This plugin allows customers to pay using [PayPal Standard](https://paypal.com).
        ✅ 正確：此外掛允許顧客使用 [PayPal Standard](https://paypal.com) 付款。
        （注意：產品名 PayPal Standard 保留原文、連結結構完整保留、中英文之間加空格）
        """;
}

// ── QuotaExhaustedException ───────────────────────────────────────────────────
public class QuotaExhaustedException(string message) : Exception(message);
