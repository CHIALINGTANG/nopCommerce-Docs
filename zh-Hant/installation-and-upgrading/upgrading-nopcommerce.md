---
標題: 升級 nopCommerce
uid: zh-Hant/installation-and-upgrading/upgrading-nopcommerce
作者: git.AndreiMaz
貢獻者: git.DmitriyKulagin, git.rajupaladiya, git.exileDev, git.dunaenko
---

# 升級 nopCommerce

本章說明如何將 nopCommerce 升級至 [最新版本](https://www.nopcommerce.com/download-nopcommerce)。當您在管理後台的 nopCommerce 新聞區塊看到有新版本發佈的訊息時，您可能就會想要進行升級。nopCommerce 不支援自動升級，您必須手動執行。

> [!IMPORTANT]
>
> 從 4.40 版本開始，我們不再使用 SQL 升級指令碼。升級會透過移轉（Migrations）自動執行（在應用程式首次啟動期間）。因此，當您從 4.30 升級到 4.40 時，請跳過下方清單中的第 2 個步驟！

請依照下列步驟進行：

1. 備份網站的所有內容，包括資料庫。這一點極為重要，確保無論在移轉過程中發生什麼問題，您都能還原到原本可執行的網站。
1. **[針對升級至 nopCommerce 4.30 及更早版本]** 接著您必須執行 SQL 升級指令碼。您必須分步驟執行。例如，如果您目前的版本是 3.90，而最新可用版本是 4.20，則必須先升級至 4.00，再升級至 4.10，最後才是 4.20。請從 [下載 nopCommerce](https://www.nopcommerce.com/download-nopcommerce) 頁面下載所需的升級指令碼。下載完成後，請針對您的資料庫執行該指令碼。

  > [!NOTE]
  >
  > 請務必閱讀升級指令碼所提供的 `Readme.txt` 檔案。有時它包含有關升級到最新版本的重要注意事項。

1. 除了 'App_Data' 目錄中的 JSON 檔案（例如 `appsettings.json` 和 `plugins.json`）之外，請移除舊版本的所有檔案。請務必保留這些檔案，因為稍後我們會用到它們。針對更早的版本：若存在 `dataSettings.json`、`Settings.txt` 或 `InstalledPlugins.txt` 等檔案，也請一併保存。
1. 上傳新網站的檔案（取得最新版本 [here](https://www.nopcommerce.com/download-nopcommerce)）。
1. 確保一切運作正常。

> [!NOTE]
>
> 在部署時，請確保目標 `appsettings.json` 已根據最新的 nopCommerce 版本進行更新，以便正式環境網站持續指向正式環境資料庫。在較早的 nopCommerce 版本中，這可能是 `dataSettings.json` 和 `Settings.txt` 檔案。此外，請確保 `plugins.json`（`InstalledPlugins.txt`）檔案也已根據最新的 nopCommerce 版本進行更新。
>
> [!NOTE]
>
> 如果您要將 nopCommerce 從舊版本升級至 4.50 版本，請確保您的連線字串包含以下參數之一：`Encrypt=false` 或 `TrustServerCertificate=True`（取決於您的伺服器需求）。您可以手動將這些參數加入 \App_Data\appsettings.json 檔案中的連線字串。此步驟是因為 `Microsoft.Data.SqlClient` 程式庫將 `Encrypt` 選項的預設值從 `false` 改為 `true` 所致。
>
> [!NOTE]
>
> 如果您將圖片儲存在檔案系統中，請務必進行備份（`\wwwroot\Images\`），並在升級後將其複製回來。
>
> [!NOTE]
>
> **(從 3.X 升級至 4.X)**：如果您想從 3.90 版本升級至最新版本，您需要先安裝 4.00（覆蓋現有的資料庫），執行 3.90 至 4.00 的移轉 SQL 指令碼，然後再升級至 4.10、4.20 以及後續版本。

## 疑難排解

如果在升級後遇到問題，您可以隨時還原備份並用舊版本的檔案取代。您也可以隨時在我們的 [論壇](https://www.nopcommerce.com/boards/) 上提出問題。

> [!NOTE]
>
> 如果您在論壇上進行進階搜尋時找不到所需的資訊，請嘗試使用 Google 搜尋並鎖定 nopCommerce 網站：[搜尋關鍵字 **site:[nopcommerce.com](https://www.nopcommerce.com/ "nopcommerce.com")**]。