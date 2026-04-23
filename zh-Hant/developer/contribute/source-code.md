---
標題: 使用原始程式碼與貢獻
uid: zh-Hant/developer/contribute/source-code
作者: git.AndreiMaz
貢獻者: git.RomanovM, git.exileDev
---

# 使用原始程式碼與貢獻

## 檢出原始程式碼

nopCommerce 在 GitHub 上維護了一個 Repository ([https://github.com/nopSolutions/nopCommerce](https://github.com/nopSolutions/nopCommerce))。因此，您可以隨時檢出最新的原始程式碼！Git SCM（原始程式碼管理）存取是公開的，並允許您即時獲取最新版本的 nopCommerce！這讓您可以追蹤 nopCommerce 每日的開發與改進進度。您無需等待下一個正式版本發佈，即可獲取最新的修補程式與修正。如果您對 Git 不熟悉，這裡有很棒的免費文件 [here](https://git-scm.com/docs) 可供參考。此外，您可以在此 [here](https://opensource.guide/how-to-contribute/) 找到更多關於 GitHub 支援的資訊。請注意，這些版本不應在正式營運環境中使用。我們不保證 SCM（原始程式碼管理）中的任何功能或程式碼都會出現在我們的正式發佈版本中。獲取原始程式碼的最佳方式是複製（clone）此 Repository。Git 內建了用於提交 (git-gui) 與瀏覽 (gitk) 的 GUI 工具，但對於尋求特定平台使用體驗的使用者，也有多種第三方工具可供選擇。請至 [https://git-scm.com/downloads/guis](https://git-scm.com/downloads/guis) 搜尋這些工具（我們使用 [SourceTree](https://www.sourcetreeapp.com/)）。

## 分支描述與命名

最近，我們開始採用 Vincent Driessen 的分支模型（請見此處：[http://nvie.com/posts/a-successful-git-branching-model/](https://nvie.com/posts/a-successful-git-branching-model/)），包括使用功能分支（feature branches）、開發分支（development branch，用於整合）以及主分支（master branch，用於發佈/生產）。在此之前（直到 2016 年 1 月），我們僅有一個 "master" 分支。

* 生產分支：master
* 開發分支：develop
* 工作項目（問題）分支：應以 "issue" 開頭。後接問題 ID（依據我們的 Github issue 清單）以及一些易懂的名稱（例如 "multistore"）。最終命名看起來應像這樣："issue-35-paypal-redirection-bug"
* 發佈分支：應以 "release" 開頭。後接版本號（例如 "3.00"）。最終命名看起來應像這樣："release-3.00"

## 建立分支（Fork）並提交 Pull Request

如果您想為 nopCommerce 核心貢獻原始程式碼（問題修正或新功能），請遵循以下步驟。以下是貢獻流程的簡要清單：

* 首先，您必須建立一個 fork。請至 [https://help.github.com/articles/fork-a-repo/](https://help.github.com/articles/fork-a-repo/) 了解更多關於在 GitHub 上建立 repository fork 的資訊。
* 將其 clone 到本地。
* 從 "develop" 分支建立一個新分支。請務必為每一項貢獻建立一個新分支。您應僅從我們的 "develop" 分支建立新分支。請勿使用 "master"。
* 編寫程式碼並推送到您的 GitHub fork。
* 建立 pull request。請至 [https://help.github.com/articles/using-pull-requests/](https://help.github.com/articles/using-pull-requests/) 閱讀更多相關資訊。在執行此動作之前，請務必與我們的 repository 進行同步。