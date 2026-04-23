---
標題: web.config 中的設定
uid: zh-Hant/developer/tutorials/description-of-the-web-config-file-in-project
作者: nop.sea
貢獻者: git.RomanovM, git.DmitriyKulagin
---

# web.config 中的設定

## 什麼是 web.config 檔案

`web.config` 檔案是一個基於 XML 的設定檔，用於 ASP.NET 應用程式中，以管理與我們網站設定相關的各種設定。透過這種方式，我們可以將應用程式邏輯與設定邏輯分開。其主要優點在於，如果我們想要更改某些設定，則不需要重新啟動應用程式來套用新的變更，ASP.NET 會自動偵測變更並將其套用到執行中的 ASP.NET 應用程式。

ASP.NET 框架使用分層的設定系統。您可以將 `web.config` 檔案放置在應用程式的任何子目錄中。該檔案隨後會套用到位於相同目錄或任何子目錄中的所有頁面。

## nopCommerce 的 web.config

nopCommerce 使用位於 `Nop.Web` 專案中的 `web.config`，該檔案位於 Presentation 目錄內。在專案目錄的根目錄下，您可以看到一個 web.config 檔案。如果您的解決方案是剛安裝好的 nopCommerce，該檔案內容大致如下：

```xml
<?xml version="1.0" encoding="utf-8"?>
<configuration>
    <system.webServer>
    <modules>
        <!-- Remove WebDAV module so that we can make DELETE requests -->
        <remove name="WebDAVModule" />
    </modules>
    <handlers>
        <!-- Remove WebDAV module so that we can make DELETE requests -->
        <remove name="WebDAV" />
        <add name="aspNetCore" path="*" verb="*" modules="AspNetCoreModuleV2" resourceType="Unspecified" />
    </handlers>
    <!-- When deploying on Azure, make sure that "dotnet" is installed and the path to it is registered in the PATH environment variable or specify the full path to it -->
    <aspNetCore requestTimeout="23:00:00" processPath="%LAUNCHER_PATH%" arguments="%LAUNCHER_ARGS%" forwardWindowsAuthToken="false" stdoutLogEnabled="false" stdoutLogFile=".\logs\stdout" startupTimeLimit="3600" hostingModel="InProcess">
    </aspNetCore>
    <httpProtocol>
        <customHeaders>
        <remove name="X-Powered-By" />
        <!-- Protects against XSS injections. ref.: https://www.veracode.com/blog/2014/03/guidelines-for-setting-security-headers/ -->
        <add name="X-XSS-Protection" value="1; mode=block" />
        <!-- Protects against Clickjacking attacks. ref.: http://stackoverflow.com/a/22105445/1233379 -->
        <add name="X-Frame-Options" value="SAMEORIGIN" />
        <!-- Protects against MIME-type confusion attack. ref.: https://www.veracode.com/blog/2014/03/guidelines-for-setting-security-headers/ -->
        <add name="X-Content-Type-Options" value="nosniff" />
        <!-- Protects against Clickjacking attacks. ref.: https://www.owasp.org/index.php/HTTP_Strict_Transport_Security_Cheat_Sheet -->
        <add name="Strict-Transport-Security" value="max-age=31536000; includeSubDomains" />
        <!-- CSP modern XSS directive-based defence, used since 2014. ref.: http://content-security-policy.com/ -->
        <add name="Content-Security-Policy" value="default-src 'self'; connect-src *; font-src * data:; frame-src *; img-src * data:; media-src *; object-src *; script-src * 'unsafe-inline' 'unsafe-eval'; style-src * 'unsafe-inline';" />
        <!-- Prevents from leaking referrer data over insecure connections. ref.: https://scotthelme.co.uk/a-new-security-header-referrer-policy/ -->
        <add name="Referrer-Policy" value="same-origin" />
        <!-- Permissions-Policy is a new header that allows a site to control which features and APIs can be used in the browser. ref.: https://w3c.github.io/webappsec-permissions-policy/ -->
        <add name="Permissions-Policy" value="accelerometer=(), camera=(), geolocation=(), gyroscope=(), magnetometer=(), microphone=(), payment=*, usb=()" />
      </customHeaders>
    </httpProtocol>
    </system.webServer>
</configuration>
```

```xml
<configuration>
    ...
</configuration>
```

所有的設定規則都放置在 「`<configuration>`」 元素內。

```xml
<system.webServer>
    ...
</system.webServer>
```

`<system.webServer>` 元素為 IIS 的許多網站層級與應用程式層級設定指定了根元素，並包含定義 Web 伺服器引擎與模組所使用設定的設定元素。

```xml
<modules>
    <!-- Remove WebDAV module so that we can make DELETE requests -->
    <remove name="WebDAVModule" />
</modules>
```

`<modules>` 元素定義了為應用程式註冊的原生程式碼模組與受控程式碼模組。我們通常使用模組來實作自訂功能。

`<modules>` 元素包含了一組 `<add>`、`<remove>` 和 `<clear>` 元素。

在此處，nopCommerce 使用 `<remove>` 元素從應用程式中移除 WebDAVModule 模組。

```xml
<handlers>
    <!-- Remove WebDAV module so that we can make DELETE requests -->
    <remove name="WebDAV" />
    <add name="aspNetCore" path="*" verb="*" modules="AspNetCoreModuleV2" resourceType="Unspecified" />
</handlers>
```

處理常式（Handlers）是 IIS 的元件，經設定後可處理特定內容的請求，通常用於為請求的資源產生回應。例如，ASP.NET 網頁就是一種處理常式。您可以使用處理常式來處理任何需要將資訊回傳給使用者（而非靜態檔案）的資源請求。

`<handlers>` 元素包含了一組 `<add>`、`<remove>` 和 `<clear>` 元素，每個元素都定義了應用程式的處理常式對應。`<add>` 元素會將處理常式新增至處理常式集合中，`<remove>` 元素會從集合中移除處理常式的參考，而 `<clear>` 元素則會移除處理常式集合中的所有處理常式參考。在上述程式碼中，「WebDAV」處理常式已被移除，並加入了 `AspNetCoreModuleV2` 模組的處理常式。

```xml
<aspNetCore requestTimeout="23:00:00" processPath="%LAUNCHER_PATH%" arguments="%LAUNCHER_ARGS%" forwardWindowsAuthToken="false" stdoutLogEnabled="false" stdoutLogFile=".\logs\stdout" startupTimeLimit="3600" hostingModel="InProcess"/>
```

```xml
<httpProtocol>
        <customHeaders>
        <remove name="X-Powered-By" />
        <!-- Protects against XSS injections. ref.: https://www.veracode.com/blog/2014/03/guidelines-for-setting-security-headers/ -->
        <add name="X-XSS-Protection" value="1; mode=block" />
        <!-- Protects against Clickjacking attacks. ref.: http://stackoverflow.com/a/22105445/1233379 -->
        <add name="X-Frame-Options" value="SAMEORIGIN" />
        <!-- Protects against MIME-type confusion attack. ref.: https://www.veracode.com/blog/2014/03/guidelines-for-setting-security-headers/ -->
        <add name="X-Content-Type-Options" value="nosniff" />
        <!-- Protects against Clickjacking attacks. ref.: https://www.owasp.org/index.php/HTTP_Strict_Transport_Security_Cheat_Sheet -->
        <add name="Strict-Transport-Security" value="max-age=31536000; includeSubDomains" />
        <!-- CSP modern XSS directive-based defence, used since 2014. ref.: http://content-security-policy.com/ -->
        <add name="Content-Security-Policy" value="default-src 'self'; connect-src *; font-src * data:; frame-src *; img-src * data:; media-src *; object-src *; script-src * 'unsafe-inline' 'unsafe-eval'; style-src * 'unsafe-inline';" />
        <!-- Prevents from leaking referrer data over insecure connections. ref.: https://scotthelme.co.uk/a-new-security-header-referrer-policy/ -->
        <add name="Referrer-Policy" value="same-origin" />
        <!-- Permissions-Policy is a new header that allows a site to control which features and APIs can be used in the browser. ref.: https://w3c.github.io/webappsec-permissions-policy/ -->
        <add name="Permissions-Policy" value="accelerometer=(), camera=(), geolocation=(), gyroscope=(), magnetometer=(), microphone=(), payment=*, usb=()" />
      </customHeaders>
    </httpProtocol>
```

`<httpProtocol>` 元素中的 `<customHeaders>` 元素指定了 IIS 將在 Web 伺服器的 HTTP 回應中回傳的自訂 HTTP 標頭。

HTTP 標頭是從 Web 伺服器回傳回應時包含的名稱與數值對。自訂回應標頭會與預設的 HTTP 標頭一同傳送給用戶端。與僅在發生重新導向時才在回應中回傳的重新導向回應標頭不同，自訂回應標頭會出現在每一次的回應中。

## 在 IIS 中設定重新導向規則

除了上述設定外，我們還可以增加其他設定。在此我們將說明如何在 IIS 中設定重新導向規則。

重新導向規則讓多個 URL 可以指向同一個網頁。您可能基於多種原因需要將請求從一台伺服器重新導向至另一台。例如，若您的公司名稱變更，您可能想要註冊一個新的網域並將網站遷移過去；在這種情況下，您會希望將所有來自舊網域的請求重新導向至新網域。

為了讓我們的網站能夠使用重新導向規則，我們需要安裝「URL rewrite」模組，這是 IIS 的一個延伸模組。

為了演示起見，假設我們必須將請求從舊站台重新導向至新站台，我們需要在 `web.config` 檔案中寫入下列規則。

```xml
<rewrite>
  <rules>
     <rule name="[RULE NAME]" stopProcessing="true">
     <match url="(.*)" />
     <conditions logicalGrouping="MatchAny" trackAllCaptures="false">
        <add input="{HTTP_HOST}{REQUEST_URI}" pattern="[OLD URL]" />
     </conditions>
     <action type="Redirect" url="http://[NEW URL]/{R:1}" redirectType="Permanent"/>
     </rule>
  </rules>
</rewrite>
```

> [!NOTE]
> 透過使用此規則，我們可以將舊網域的所有頁面重新導向至新網域的相同頁面。

在此，我們需要將 [RULE NAME]、[OLD URL] 和 [NEW URL] 取代為適當的資訊。

* [RULE NAME] 可以是任何描述此規則用途的名稱。
* [OLD URL] 是您想要重新導向來源的舊 URL。
* [NEW URL] 是您想要重新導向前往的新 URL。

```xml
<match url="(.*)" />
```

上述元素表示此規則將會符合所有 URL 字串。

```xml
<add input="{HTTP_HOST}{REQUEST_URI}" pattern="[OLD URL]" />
```

上述元素為規則增加了一個條件，該條件透過讀取伺服器變數 HTTP_HOST 和 REQUEST_URI 來取得主機與請求的 Uri 標頭值，並將其與 [OLD URL] 所提供的模式值進行比對。

```xml
<action type="Redirect" url="http://[NEW URL]/{R:1}" redirectType="Permanent"/>
```

此元素會將符合的舊 URL 重新導向至新 URL。