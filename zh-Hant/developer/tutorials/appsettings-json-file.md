---
標題: appsettings.json 中的設定
uid: zh-Hant/developer/tutorials/appsettings-json-file
作者: git.nopsg
貢獻者: git.nopsg, git.DmitriyKulagin, git.mariannk
---

# appsettings.json 中的設定

## 概覽

本文包含 *appsettings.json* 檔案的說明。在本文中，我們將解釋此檔案中有哪些可用的設定、它們的用途，以及如何使用這些設定來變更 nopCommerce 專案的功能與行為。

> [!IMPORTANT]
> 所有設定皆可透過環境變數進行覆寫。

## appsettings.json 檔案概覽

如果您之前曾參與過 *ASP.NET Core* 專案，或者您熟悉 `ASP.NET Core`，那麼您可能已經使用過 *appsettings.json* 檔案，並對此檔案的用途有一定程度的了解。

> [!NOTE]
> 您也可以在 **後台 → 設定 → 應用程式設定 (App settings)** 頁面中編輯此檔案。

*appsettings.json* 檔案通常用於儲存應用程式的設定，例如資料庫連接字串、任何應用程式範圍的全域變數，以及許多其他資訊。實際上，在 *ASP.NET Core* 中，應用程式的設定可以儲存在不同的設定來源中，例如 *appsettings.json* 檔案、**`appsettings.{EnvironmentName}.json`** 檔案（其中 `{Environment}` 是應用程式目前的託管環境，如 Development、Staging 或 Production）、`User Secrets`（我們通常用來儲存敏感資訊的地方）等。

## appsettings.json 檔案中的可用設定

### DataConfig

資料庫的連線是透過此區段進行設定。

* **ConnectionString** 此設定需要一個字串值。
  > [!NOTE]
  > 連線字串的格式會根據所選的資料庫提供者而有所不同。
  > 例如，**SqlServer** 的連線字串如下所示：
  >
  > ```powershell
  > Data Source={localhost};Initial Catalog={your_data_base_name}};Integrated >Security=False;Persist Security Info=False;User ID={your_user_id}};Password={your_password}};>Trust Server Certificate=True
  > ```

* **DataProvider** 您可以指定其中一個支援的資料提供者：
  * [**SqlServer**](https://www.microsoft.com/sql-server)
  * [**MySql**](https://www.mysql.com/)
  * [**PostgreSQL**](https://www.postgresql.org/)
* **SQLCommandTimeout** 設定執行指令前的等待時間（以秒為單位），若超過該時間將終止嘗試並產生錯誤。預設情況下，不會設定逾時，並會使用該提供者的預設值。設定為 **`0`** 則表示使用無限逾時。
* **WithNoLock** 指示是否要在 SELECT 陳述式中加入 NoLock 提示（僅適用於 SQL Server）。

### AzureBlobConfig

> [!IMPORTANT]
> 此設定區段適用於 4.80 及以下版本。關於 nopCommerce 4.90 及以上版本，請參閱 [Azure 設定說明](xref:zh-Hant/developer/plugins/cloudflare-images)。

我們可以使用 *Azure Blob Storage* 來儲存 Blob 資料。nopCommerce 已經內建此功能，我們只需要正確設定以下資訊即可使用。在建立儲存體帳戶時，您可以從 *Azure* 取得這些設定的值。

* **ConnectionString** 此設定需要一個字串值。您需要在此處加入您的 `AzureBlobStorage` 連接字串。
* **ContainerName** 此設定的值同樣為字串類型。在此設定中，我們設定 *Azure BLOB storage* 的容器名稱。
* **EndPoint** 此設定同樣需要一個字串值。我們需要在此處設定 *Azure BLOB storage* 的端點。
* **AppendContainerName** 此設定需要一個布林值。請根據建構 URL 時是否需要將容器名稱附加到 `EndPoint` 後方，將值設為 **`true`** 或 **`false`**。
* **StoreDataProtectionKeys** 此設定需要一個布林值。若您希望將 *Windows Azure BLOB storage* 用於儲存 *Data Protection Keys*（資料保護金鑰），請將值設為 **`true`**。
* **DataProtectionKeysContainerName** 此設定需要一個字串值。您需要在此處設定一個用於儲存 *Data Protection Keys* 的 Azure 容器名稱（此容器應與用於媒體的容器分開，且應設為私有 Private）。
* **DataProtectionKeysVaultId (選填)** 此設定同樣需要一個字串值。若您需要加密 *Data Protection Keys*，請在此處設定 `Azure key vault ID`。

### CacheConfig

快取設定。

* **DefaultCacheTime** 此設定決定快取資料的存留時間。預設值為 **`60`** 分鐘。
* **LinqDisableQueryCache** 指示是否停用查詢的 LINQ 表達式快取。此快取可減少查詢解析所需的時間，但會產生一些副作用。例如，快取的 LINQ 表達式可能包含對外部物件的參考作為參數；若其他程式碼不再使用這些物件，可能會導致記憶體洩漏。此外，存取快取時的同步處理，其導致的延遲有時可能大於其節省的時間。

### CommonConfig

*CommonConfig* 包含用於設定 nopCommerce 本身行為的設定。它是一個 JSON 物件，包含一些可以調整以變更 nopCommerce 行為的設定。

* **DisplayFullErrorStack** 此設定需要一個布林值。預設值為 **`false`**。如果您希望在正式環境（production environment）中查看完整的錯誤訊息，可以將此值設為 **`true`**。通常我們不建議這樣做，但如果您有充分的理由需要在正式環境中顯示完整錯誤，則可以透過此設定達成。對於開發環境，此設定會被忽略，無論您設定為何，系統都會顯示完整的錯誤訊息。可以說此設定在開發環境中是永遠啟用的。
* **UserAgentStringsPath** 此設定儲存 `Browscap.xml` 檔案的路徑。如其檔名所示，`Browscap.xml` 是一個瀏覽器功能資料庫。它本質上是一個所有已知瀏覽器和機器人的清單，以及它們的預設功能與限制。
  
  ```powershell
  ~/App_Data/browscap.xml
  ```

    >[!NOTE]
    > 在計算領域中，使用者代理（user agent）是一種代表使用者運作的軟體（軟體代理），例如「擷取、呈現並協助終端使用者與 Web 內容互動」的網頁瀏覽器。如需更多資訊，請造訪 [UserAgent](https://en.wikipedia.org/wiki/User_agent)。
* **CrawlerOnlyUserAgentStringsPath** 此設定儲存 `browscap.crawlersonly.xml` 的位置/路徑。它僅儲存「CrawlerOnly」（僅爬蟲）的使用者代理。
  
  ```powershell
  ~/App_Data/browscap.crawlersonly.xml
  ```

* **CrawlerOnlyAdditionalUserAgentStringsPath** 此設定儲存 `additional.crawlers.xml` 的位置/路徑。它僅儲存「Crawlers」（爬蟲）的使用者代理。
  
  ```powershell
  ~/App_Data/additional.crawlers.xml
  ```

* **UseSessionStateTempDataProvider** 此設定需要一個布林值。此設定的預設值為 **`false`**。如果您希望將 `TempData` 儲存在工作階段狀態（session state）中，可能會想將此值設為 **`true`**。預設情況下，會使用基於 cookie 的 `TempData` 提供者將 `TempData` 儲存在 cookie 中。
* **ScheduleTaskRunTimeout** 允許您以毫秒為單位設定排程任務執行的逾時時間。設為 **`null`** 則使用預設值。
* **StaticFilesCacheControl** 指定靜態內容的 'Cache-Control' 標頭值（以秒為單位）。

  ```powershell
  public,max-age=31536000
  ```

* **ServeUnknownFileTypes** 此設定指定是否提供不具備已識別內容類型的檔案。預設值為 **`false`**。
* **UseAutofac** 指定是否使用 *Autofac IoC container* 的值。如果停用，則會使用預設的 *.Net IoC container*。
* **PermitLimit** 在一個時間視窗（1 分鐘）內允許的最大許可計數器數量。當這些選項傳遞給 `FixedWindowRateLimiter` 的建構函式時，必須設定為 `> 0` 的值。如果設為 **`0`**，則關閉限制。
* **QueueCount** 排隊請求的最大累計許可數量。當這些選項傳遞給 `FixedWindowRateLimiter` 的建構函式時，必須設定為 `>= 0` 的值。如果設為 **`0`**，則關閉佇列。
* **RejectionStatusCode** 當請求被拒絕時，在回應中設定的預設狀態碼。

### DistributedCacheConfig

分散式快取是由多個應用程式伺服器共享的快取，通常作為存取這些伺服器的外部服務來維護。分散式快取可以提升 ASP.NET Core 應用程式的效能與擴充性，特別是在應用程式由雲端服務或伺服器陣列託管時。

* **DistributedCacheType** 您可以選擇其中一種實作方式：
  * **Memory** - 這是框架提供的 `IDistributedCache` 實作，將項目儲存在記憶體中。Distributed Memory Cache 並非真正的分散式快取。快取項目是由應用程式執行所在伺服器上的應用程式實例所儲存。
  * **SQL Server** - Distributed SQL Server Cache 實作允許分散式快取使用 SQL Server 資料庫作為其後端儲存。若要在 SQL Server 實例中建立 SQL Server 快取項目資料表，您可以使用 SQL-cache 工具。該工具會使用您指定的名稱與結構描述 (schema) 來建立資料表。建議為此目的使用獨立的資料庫。
  
    ```sh
    dotnet sql-cache create "Data Source=(localdb)\MSSQLLocalDB;Initial Catalog=DistCache;Integrated   Security=True;" dbo nopCache
    ```

  * **Redis** - nopCommerce 內建支援 *Redis*。若要啟用應用程式中的 *Redis*，您必須設定對應的設定。關於 [Redis](https://azure.microsoft.com/documentation/articles/cache-dotnet-how-to-use-azure-redis-cache) 的更多資訊請參考此處。
  * **Redis Synchronized Memory** - nopCommerce 4.70 版本引入了一種新的快取方式，透過 *Redis* 中的訊息機制進行同步。此選項具有極高的效能，因為快取本身儲存在記憶體中，而 *Redis* 僅作為同步器使用。若要啟用此選項，您必須設定與基礎 *Redis* 快取相同的設定。
  
* **Enabled** 此設定需要一個布林值。如果您想啟用 `Distributed cache`，請將值設為 **`true`**。系統會使用 In-Memory 快取，因此此設定用於指示我們是否應該使用 `Distributed Cache` 進行快取，以取代預設的 `in-memory caching`。因此，如果您想使用例如 *Redis* 進行快取，請使用此設定。
* **ConnectionString (optional)** 此設定僅與 *Redis* 或 *SQL Server* 搭配使用。此設定需要一個字串值。此設定的預設值為

  ```powershell
  127.0.0.1:{PORT},ssl=False
  ```

* **SchemaName (optional)** 此設定僅與 `SQL Server` 搭配使用。
* **TableName (optional)** 此設定僅與 `SQL Server` 搭配使用。為 SQL Server 資料庫名稱。
* **InstanceName (optional)** 指定實例名稱（預設為 "nopCommerce"）。
* **PublishIntervalMs (optional)** 指定發布金鑰變更事件的間隔，以毫秒為單位。

### HostingConfig

Hosting 包含用於設定主機行為的設定。它是一個 JSON 物件，包含一些可以調整的屬性設定，用以改變主機的運作行為。

* **UseProxy** 此設定需要一個布林值。啟用此設定可將轉送標頭（forwarded headers）套用到目前 HTTP 請求的對應欄位上。
* **ForwardedProtoHeaderName** 此設定需要一個字串值。指定一個自訂 HTTP 標頭名稱，用以識別用戶端連接到您的 Proxy 或負載平衡器時所使用的通訊協定（HTTP 或 HTTPS）。
* **ForwardedForHeaderName** 此設定需要一個字串值。指定一個自訂 HTTP 標頭名稱，以確定原始來源 IP 位址（例如：**`CF-Connecting-IP`**、**`X-ProxyUser-Ip`**）。
* **KnownProxies** 此設定需要一個字串值。指定一組 IP 位址清單（以逗號分隔）以接受轉送標頭。
* **KnownNetworks** 此設定需要一個字串值。指定一組 IP CIDR 標記法清單（以逗號分隔）以接受轉送標頭。例如：172.64.0.0/13,162.158.0.0/15

### InstallationConfig

它包含了用於設定 nopCommerce 在安裝期間行為的設定。

* **DisableSampleData** 此設定需要一個布林值。此設定用來指出商店管理員是否可以在安裝期間安裝範例資料。如果您不希望商店管理員在安裝期間安裝範例資料，只需將此設定的值設為 **`true`**。
* **DisabledPlugins** 此設定需要一個字串值。指定在安裝期間應忽略的外掛清單（以逗號分隔）。
* **InstallRegionalResources** 此設定需要一個布林值。此設定啟用在安裝期間選擇額外語言資源的功能。選擇國家將決定套用至商店的設定（匯率、稅務、度量單位等區域性功能）。

### PluginConfig

* **UseUnsafeLoadAssembly** 此設定需要一個布林值。如果您希望將組件載入至 load-from 內容中並繞過某些安全性檢查，您可能需要將此值設為 **`true`**。

### WebOptimizer

我們使用 [WebOptimizer](https://github.com/ligershark/WebOptimizer) 工具來進行 *CSS* 和 *JavaScript* 程式碼的壓縮與合併，這是一個 *ASP.NET Core* 中介層。最佳化是在執行時期完成的，並透過伺服器端與用戶端快取來達到高效能表現。

* **EnableJavaScriptBundling** 此設定需要一個布林值。如果您希望啟用 JS 檔案合併與壓縮，可以將其設為 **`true`**。
* **EnableCssBundling** 此設定需要一個布林值。如果您希望啟用 CSS 檔案合併與壓縮，可以將其設為 **`true`**。
* **JavaScriptBundleSuffix** 此設定需要一個字串值。您可以為產生的 bundle 的 js 檔名設定字尾（預設為 **`.scripts`**）。
* **CssBundleSuffix** 此設定需要一個字串值。您可以為產生的 bundle 的 CSS 檔名設定字尾（預設為 **`.styles`**）。
* **EnableCaching** 此設定需要一個布林值。您可以設定一個值來表示是否啟用伺服器端快取（預設為 **`true`**）。
* **EnableMemoryCache** 此設定需要一個布林值。您可以設定一個值來表示是否啟用基於 *Microsoft.Extensions.Caching.Memory.IMemoryCache* 的快取（預設為 **`true`**）。
* **EnableDiskCache** 此設定需要一個布林值。決定是否將管線資產快取至磁碟。這可以透過從磁碟載入管線資產，而非重新執行管線來加速應用程式重啟。在開發模式下停用此選項可能會有所幫助。
* **EnableTagHelperBundling** 此設定需要一個布林值。您可以設定是否啟用 bundle 功能（預設為 **`false`**）。
* **CdnUrl** 此設定需要一個字串值。您可以設定 TagHelpers 使用的 CDN URL（預設為 **`null`**）。
* **CacheDirectory** 此設定需要一個字串值。設定當 **EnableDiskCache** 為 **`true`** 時資產儲存的目錄（預設為 **`{ContentRootPath}\\wwwroot\\bundles`**）。
* **AllowEmptyBundle** 此設定需要一個布林值。您可以設定是否允許產生空的 bundle，而不是拋出例外（預設為 **`true`**）。
* **HttpsCompression** 此設定需要一個整數值。當 Response Compression 中介層可用時，您可以設定一個值，表示是否應針對 HTTPS 請求壓縮檔案。預設值為 **`2`**。您可以選擇下列其中一種實作方式：
  * **1** - 選擇不對 HTTPS 進行壓縮。
  * **2** - 選擇對 HTTPS 進行壓縮。
    > [!NOTE]
    > 在可遠端操作的內容上啟用 HTTPS 請求壓縮可能會引發安全性問題。
* **MemoryCacheTimeToLive** 如果啟用了 *EnableMemoryCache* 記憶體快取，此設定可控制項目在記憶體中的儲存時間。預設為 60 分鐘。