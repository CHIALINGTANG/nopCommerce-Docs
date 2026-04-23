---
標題: 在 Microsoft Azure 上安裝
uid: zh-Hant/installation-and-upgrading/installing-nopcommerce/installing-on-microsoft-azure
作者: git.AndreiMaz
貢獻者: git.skoshelev, git.DmitriyKulagin, git.exileDev, git.ivkadp, git.mariannk
---

# 在 Microsoft Azure 上安裝

在 Microsoft Azure 上部署 nopCommerce 有三種方式：

1. **FTP**。如果您已經有準備好要部署的套件（沒有原始程式碼），請使用此方法。您可以將專案發佈到本機檔案系統，然後透過 FTP 上傳已發佈的檔案。
   如何取得 Azure 的 FTP 憑證？您應該前往 **[Azure.com](https://azure.microsoft.com/) → 我的帳戶 → 管理入口網站 → 選擇您的網站 → 前往儀表板 → 快速概覽**。在這裡，您可以找到 FTP 憑證，或執行「重設您的部署憑證」或「下載發佈設定檔」。
   若為新的 Azure 入口網站，請前往 [portal.azure.com](http://portal.azure.com/) → 瀏覽網站 → 導覽至您的網站 → 屬性。在這裡，您可以找到 FTP 憑證，或執行「重設您的部署憑證」或「下載發佈設定檔」。

1. **Visual Studio** - Web 部署。您也可以直接從 *Visual Studio* 部署到 Azure。使用上述方法從 Azure 下載或取得部署憑證，並在 *Visual Studio* 中設定 Web 部署設定檔。

1. **Web Platform Installer**。nopCommerce 可在 Azure Web Sites 應用程式庫中取得。因此，請前往 Azure 入口網站，點擊「開始、新網站、從應用程式庫」。從可用應用程式清單中選擇 nopCommerce。輸入資料庫連線資訊並點擊 *`OK`* 後，nopCommerce 即可啟動。

    >[!TIP]
    >
    > 如果您收到錯誤「HTTP Error 500.32 - ANCM Failed to Load dll」，可能是因為平台必須變更為 64 位元（透過 Azure：**App Settings - Settings - Configuration - General settings - Platform settings - Platform**）。

網站部署完成後，您必須安裝 nopCommerce。請 [在此處閱讀更多相關資訊](xref:zh-Hant/installation-and-upgrading/installing-nopcommerce/index)。

自 3.70 版本起，Azure 已支援多重執行個體。這對於任何應用程式的擴充性都非常出色。現在，您不必擔心網站是否能處理大量的訪客。那麼，為了支援 Azure 和 Web 伺服器陣列（Web farms）中的多重執行個體，具體做了哪些調整呢？

* **支援 Microsoft Azure 中的 BLOB 儲存體帳戶**。請在此進一步了解 [Azure 中的儲存體帳戶](https://azure.microsoft.com/documentation/articles/storage-introduction/)。*如何設定：*
  * 在 Azure 中設定好 BLOB 儲存體後，開啟您的 `appsettings.json`（或舊版本的 `web.config`）檔案，找到 **AzureBlobStorage** 元素，並指定您的 BLOB 儲存體連接字串、容器（container）和端點（endpoint）。
    >[!IMPORTANT]
    >這些設定適用於 nopCommerce 4.80 及更低版本。
  * 在 Azure 中設定好 BLOB 儲存體後，開啟 **Azure Blob Storage** 外掛的設定頁面，並指定 BLOB 儲存體的連接字串、容器和端點。
    >[!IMPORTANT]
    >這些設定適用於 nopCommerce 4.90 及更高版本。

* **支援分散式快取與工作階段管理**。支援的選項包括 SQL Server 和 Redis。若要設定 SQL Server，請參閱 [DistributedCacheConfig](https://docs.nopcommerce.com/en/developer/tutorials/appsettings-json-file.html#distributedcacheconfig) 章節以取得更多詳細資訊。以下說明假設您已選擇 [Redis](http://redis.io/) 作為快取伺服器（已在 Azure、Amazon 及其他雲端託管公司中提供）。*如何設定：*
  * 首先，您必須安裝並設定 Redis。請在此了解 [如何在 Azure 中使用 Redis](https://azure.microsoft.com/documentation/articles/cache-dotnet-how-to-use-azure-redis-cache/)。
  * 設定完成後，您必須在 nopCommerce 中進行設定。為了在 Redis 中啟用快取，請開啟 `appsettings.json` 檔案。找到 **DistributedCacheConfig** 設定區段。將 **DistributedCacheType** 設定為 Redis，並將 **Enabled** 設定為 *`true`*，然後指定指向您 Redis 伺服器的 **ConnectionString**（於第一步設定）。
  * 對於 3.90（及更低）版本，您還必須啟用 Redis 作為分散式工作階段管理。請開啟 `web.config` 檔案。找到並取消註解 **sessionState** 元素。指定其屬性（例如 *host*、*accessKey* 等），使其指向您的 Redis 伺服器。
* 建議設定 `appsettings.json` 檔案以提升穩定性（適用於 nopCommerce 4.50 及更低版本）：
  * **UsePluginsShadowCopy** - 將其設為 *`false`*，以防止 IIS 應用程式集區回收（pool recycle）與水平擴充時發生的問題。
* **將 Azure 監控代理程式設定為爬蟲程式（nopCommerce 4.70 及更高版本）**。當使用 Azure Traffic Manager 端點監控（例如，當路徑設為 `/`）或 Azure App Service **Always On** 時，它們對根 URL 的請求可能會在每次呼叫時建立新的訪客顧客。為避免此情況，請在 `additional.crawlers.xml` 檔案中將這些使用者代理程式設定為爬蟲程式（請參閱 `appsettings.json` 中的 `CrawlerOnlyAdditionalUserAgentStringsPath` 設定）：

  ```xml
  <browscapitem name="Azure Traffic Manager Endpoint Monitor"> <!-- Azure Traffic Manager -->
    <item name="Crawler" value="true" />
  </browscapitem>

  <browscapitem name="AlwaysOn"> <!-- Azure AppService AlwaysOn -->
    <item name="Crawler" value="true" />
  </browscapitem>
  ```

* 確保 nopCommerce 排程任務每次只在一個執行個體上執行。設定方式如下：
  * 對於 3.90（及更低）版本，開啟 `web.config` 檔案，找到 **WebFarms** 元素，並將其 **MultipleInstancesEnabled** 屬性設為 *`true`*。如果您使用的是 Microsoft Azure Websites（而非雲端服務），則將 **RunOnAzureWebsites** 屬性也設為 *`true`*。
  * 對於較新的版本，無需變更任何設定，因為任務執行程式會使用分散式快取來確保任務每次只會在一個執行個體上執行。

## 安裝程序

nopCommerce 的後續安裝程序與 Windows 上的安裝程序相同；您可以參閱 [this link](xref:zh-Hant/installation-and-upgrading/installing-nopcommerce/installing-on-windows#install-nopcommerce) 中的說明。

## Web farms

您也可以透過 IIS web farms 來設定負載平衡。請 [在此處閱讀更多相關資訊](xref:zh-Hant/installation-and-upgrading/installing-nopcommerce/web-farms)。