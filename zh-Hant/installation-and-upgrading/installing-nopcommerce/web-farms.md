---
標題: nopCommerce 中的負載平衡與 Web Farm
uid: zh-Hant/installation-and-upgrading/installing-nopcommerce/web-farms
作者: git.exileDev
貢獻者: git.mariannk, git.AndreiMaz
---

# nopCommerce 中的負載平衡與 Web Farm

負載平衡是將工作負載分配到多個節點上的技術。它通常用於將 HTTP 流量分配到多個共同運作的伺服器上，作為 Web 前端使用。

在 nopCommerce 中有幾種設定負載平衡的方法：

1. 使用基於雲端的自動擴展設備，例如 Microsoft 的 Azure Web Apps。請參閱 [here](xref:zh-Hant/installation-and-upgrading/installing-nopcommerce/installing-on-microsoft-azure) 以取得更多資訊。
1. 使用 IIS Web Farm 設定負載平衡。此方法說明如下。

我們強烈建議您在開始為 nopCommerce 設定 Web Farm 之前，先閱讀 Microsoft 的 [此篇教學課程](https://docs.microsoft.com/en-us/iis/web-hosting/scenario-build-a-web-farm-with-iis-servers/overview-build-a-web-farm-with-iis-servers)。Microsoft 建議使用兩種方式來建立基於 IIS 伺服器的 Web Farm：

1. 本地內容基礎架構 (Local Content Infrastructure)
1. 共用網路內容基礎架構 (Shared Network Content Infrastructure)

nopCommerce 支援這兩種方式：如果您使用 **本地內容基礎架構**，它會透過分散式檔案系統複寫 (Distributed File System Replication, DFSR) 來處理內容複寫；如果您使用 **共用網路內容基礎架構**，則會使用一個中心位置來管理內容。

## nopCommerce 設定

首先，您必須在 IIS 中設定網路農場（web farm）的初始設定，並在那裡新增您的 nopCommerce 商店的第一個實例。接著，您必須在 nopCommerce 後台進行一些設定，以允許 nopCommerce 在網路農場環境下運作：

1. 前往 **設定 → 設定 → 所有設定 (進階)**。找到 **mediasettings.useabsoluteimagepath** 設定，並將其值變更為 *false*。

1. 前往 **設定 → 設定 → 應用程式設定**，並找到 *分散式快取設定* 頁籤。勾選 **使用分散式快取** 核取方塊，並選擇您偏好的選項：

   - *Redis*。在此情況下，您只需在下方輸入 Redis 伺服器的 **連接字串**。
   - *SQL Server*。在此情況下，您需要先使用 "sql-cache create" 指令在資料庫中準備一個新資料表。關於此部分的詳細資訊，請參閱 Microsoft 說明文件 [here](https://docs.microsoft.com/en-us/aspnet/core/performance/caching/distributed?view=aspnetcore-5.0#distributed-sql-server-cache)。然後填寫 **連接字串**、**Schema 名稱** 以及 **資料表名稱** 欄位。

1. 由於我們的網路農場利用應用程式請求路由（ARR）透過代理伺服器來控制流量，請勾選 **使用代理伺服器** 核取方塊。
1. 點擊 **儲存** 按鈕。nopCommerce 應用程式將會重新啟動。

## 網頁伺服器陣列（Web farm）設定

### 管理後台重新導向規則

由於 Web Farm 會主控多個應用程式執行個體，您需要選擇其中一個 nopCommerce 執行個體作為主要節點，以避免檔案鎖定問題。此主要節點將負責處理來自管理後台的請求。

此規則應如下所示：

```xml
<rule name="Admin Area" enabled="true" patternSyntax="ECMAScript" stopProcessing="true">
    <match url="^(admin(/.*)?)$|^(lib_npm/.+)$" />
    <conditions logicalGrouping="MatchAll" trackAllCaptures="false">
        <add input="{HTTP_HOST}" pattern="^admin\.wf\.local$" negate="true" />
    </conditions>
    <action type="Rewrite" url="http://admin.wf.local/{R:0}" />
</rule>
```

其中 `admin.wf.local` 是您主要執行個體的位址。

### 負載平衡規則

在設定好網頁伺服器陣列（Web Farm）後，您需要在 **Application Request Routing** 區段中設定負載平衡規則。新增一個條件，以防止處理針對主要節點的請求（在此案例中為管理後台的請求）：

```xml
<rule name="ARR_wf-local_loadbalance" enabled="true" patternSyntax="ECMAScript" stopProcessing="true">
    <match url=".*" ignoreCase="false" />
    <conditions logicalGrouping="MatchAll" trackAllCaptures="false">
        <add input="{PATH_INFO}" pattern="^(/admin(/.*)?)$|^(/lib_npm/.+)$" negate="true" />
    </conditions>
    <action type="Rewrite" url="http://wf-local/{R:0}" />
</rule>
```

其中 `wf-local` 是網頁伺服器陣列的名稱。

您可以透過以下兩種方式新增上述規則：

1. 將其包含在 `applicationHost.config` 檔案中（**system.webServer/rewrite/globalRules** 區段）。
1. 使用 IIS 管理員中的 **URL Rewrite script** 區段。

> [!NOTE]
>
> 在某些情況下，ARR 在處理包含超過一個「.」符號的 JavaScript 檔案 URL 時會出現問題。例如，這可能會影響以 `.min.js` 結尾的壓縮 JS 檔案。為了避免在處理此類檔案時發生錯誤，我們建議直接將這些請求路由至其中一個 nopCommerce 實例。如上述範例所示，我們透過路由至主要實例的方式，對整個 `lib_npm` 目錄執行此操作。

當設定完成後，請將新的實例新增至您的網頁伺服器陣列。

### 檔案複製

當您開始設定檔案複製時，請確保主執行個體的下列資料夾已完成設定，以支援複製到所有其他節點（執行個體）：

- /App_Data
- /Plugins
- /Themes
- /wwwroot

> [!NOTE]
>
> 所有預期會重新啟動 nopCommerce 應用程式的操作（例如：安裝外掛、更新應用程式設定），都需要手動重新啟動與 Web Farm 相關的所有應用程式集區（Application Pools）。