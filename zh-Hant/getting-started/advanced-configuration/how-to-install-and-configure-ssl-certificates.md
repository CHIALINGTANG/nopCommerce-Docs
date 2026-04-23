---
標題: 如何安裝與設定 SSL 憑證
uid: zh-Hant/getting-started/advanced-configuration/how-to-install-and-configure-ssl-certificates
作者: git.mariannk
貢獻者: git.DmitriyKulagin
---

# 如何安裝與設定 SSL 憑證

什麼是 SSL 憑證？SSL 代表安全通訊端層（Secure Socket Layer）。SSL 憑證可以驗證您網站的身份，並加密訪客傳送給您網站（或從您網站接收）的資訊。當您擁有保護網站的 SSL 憑證時，您的顧客可以放心，他們在任何安全頁面上輸入的資訊都是隱私的，且不會被網路駭客竊取。

## 如何取得 SSL 憑證

1. 首先，為了在您的網站上實作 SSL，您需要向 SSL 憑證提供者（也稱為憑證授權中心，Certification Authority）取得 SSL 憑證。有許多憑證授權中心可以為您的網站提供 SSL 憑證，例如 SSL.com、Namecheap 或 GoDaddy。

2. 接著，您需要在伺服器上安裝所購買的 SSL 憑證。安裝方式取決於您使用的伺服器。若需更多說明與建議，請參考您的 SSL 憑證提供者指南或伺服器文件。在本文中，我僅提供 GoDaddy 的相關指南連結供您參考：[安裝 SSL 憑證](https://www.godaddy.com/help/install-ssl-certificates-16623)。

3. 最後一步，您需要在 nopCommerce 管理後台設定您的商店。請前往 **設定 → 商店** 頁面。選擇您要設定的商店，並點擊旁邊的 **編輯** 按鈕。此時將顯示「編輯商店詳情」視窗，如下所示：
  ![Store](_static/how-to-install-and-configure-ssl-certificates/store.jpg)

- 輸入包含 `https://` 前綴的 **商店 URL**。
- 勾選 **啟用 SSL** 核取方塊。
    > [!WARNING]
    >
    > 在伺服器上安裝 SSL 憑證之前，請勿啟用此選項。

## 疑難排解

### 管理後台因 SSL 憑證問題而無法存取

常見的問題是伺服器未安裝 SSL 憑證，或 SSL 設定出現問題。同時，商店中已啟用了 **SSL enabled** 設定（如同我們在前一節所做的）。

**受影響的版本**：所有版本

**解決方案**：
執行下列 SQL 查詢：

  ```sql
  UPDATE [dbo].[Store] SET [SslEnabled] = 'False'
  ```

### 混合內容 HTTP 與 HTTPS

當網站透過 SSL 安全協定運作，但部分資源（例如圖片）卻透過不安全的 HTTP 連線載入時，就會發生「混合內容」。這會導致頁面發生錯誤，因為原始請求是透過 HTTPS 進行保護的。

當使用負載平衡器時，由於它與應用程式之間是透過 HTTP 進行通訊，因此也可能產生類似的問題。

**受影響版本**：4.20 以下

**解決方案**：

- 請確保您已啟用以下設定：

  ```json
  securitysettings.forcesslforallpages = true
  ```
  
- 請確保您的網站正在代管伺服器的 443 連接埠上監聽。

**受影響版本**：所有版本

**解決方案**：

- 請求標頭中缺少 `UseHttpXForwardedProto` 欄位。請嘗試在 `appsettings.json` 檔案中啟用 `UseHttpXForwardedProto` 設定並重新啟動網站。

  ```json
  "UseHttpXForwardedProto": true
  ```

- 您可以透過加入 CSP 的「upgrade-insecure-requests」指令來解決此問題。這可以在 `web.config` 檔案中完成；或者，您也可以透過使用 `<meta>` 元素，將相同的內嵌指令嵌入到文件的 `<head>` 區段中：

  ```XML
  <meta http-equiv = "Content-Security-Policy" content = "upgrade-insecure-requests">
  ```

- 若是使用 Cloudflare，請登入您的 Cloudflare 管理後台並點擊 `SSL/TLS app` 來檢查 SSL 設定，確認您的 SSL 設定是否處於 `Full` 或 `Flexible` 模式。

### 無窮重新導向迴圈 (ERR_TOO_MANY_REDIRECTS)

當未授權的使用者嘗試登入或存取購物車時，網站進入了無窮重新導向迴圈。

**受影響版本**：所有版本

**解決方案**：

- 嘗試刪除網站的 cookie；此程序可能會根據您使用的瀏覽器而略有不同。或者，您可以直接以無痕模式開啟頁面，以確認這是否為導致錯誤的原因。
- 清除伺服器、代理伺服器以及瀏覽器的快取。
- 檢查伺服器上的 HTTP 至 HTTPS 重新導向設定。很有可能是您伺服器上的 HTTPS 重新導向規則設定錯誤。您可以在 IIS 中新增從 http 到 https 的重新導向規則。規則模式採用以下形式：

  ```xml
  <configuration>
    <system.webServer>
      <rewrite>
        <rules>
          <rule name="http_to_https" stopProcessing="true">
            <match url="(.*)" />
            <conditions logicalGrouping="MatchAll" trackAllCaptures="false">
              <add input="{HTTPS}" pattern="^OFF$" />
            </conditions>
            <action type="Redirect" url="https://{HTTP_HOST}/{R:1}" redirectType="SeeOther" />
          </rule>
        </rules>
      </rewrite>
    </system.webServer>
  </configuration>
  ```

- ERR_TOO_MANY_REDIRECTS 也經常是由反向代理服務（如 Cloudflare）所引起。當啟用其 Flexible SSL 選項，且您已向虛擬主機供應商安裝 SSL 憑證時，通常會發生這種情況。當選擇 Flexible 模式時，所有對您主機伺服器的請求都會透過 HTTP 發送。您的主機伺服器很可能已經設定了從 HTTP 到 HTTPS 的重新導向，因此導致了重新導向迴圈。若要修正此問題，您需要將 Cloudflare 的 Crypto 設定從 Flexible 變更為 Full 或 Full (strict)。