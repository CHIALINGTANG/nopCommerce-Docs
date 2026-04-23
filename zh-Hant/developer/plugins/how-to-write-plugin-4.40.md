---
標題: 如何為 nopCommerce 編寫外掛
uid: zh-Hant/developer/plugins/how-to-write-plugin-4.40
作者: git.AndreiMaz
貢獻者: git.DmitriyKulagin
---

# 如何為 nopCommerce 編寫外掛

外掛用於擴充 nopCommerce 的功能。nopCommerce 擁有多種類型的外掛。例如，付款方式（如 PayPal）、稅額提供者、運送方式計算方法（如 UPS、USPS、FedEx）、小工具（如「線上客服」區塊）以及許多其他類型。nopCommerce 本身已經隨附了許多不同的外掛。您也可以在 [nopCommerce 官方網站](https://www.nopcommerce.com/marketplace) 上搜尋各種外掛，看看是否已經有人開發出符合您需求的外掛。如果沒有，這篇文章將引導您完成建立外掛的流程。

## 外掛結構、必要檔案與位置

1. 您首先需要做的是在方案中建立一個新的 *`Class Library`* 專案。將所有外掛放置在方案根目錄的 `\Plugins` 目錄中是一種良好的實踐（請勿與位於 `\Nop.Web` 目錄中，用於已部署外掛的 `\Plugins` 子目錄混淆）。將所有外掛放置在 `Plugins` 方案資料夾中也是良好的實踐。

    建議的外掛專案命名方式為 **`Nop.Plugin.{Group}.{Name}`**。**`{Group}`** 是您的外掛群組（例如 *Payment* 或 *Shipping*）。**`{Name}`** 是您的外掛名稱（例如 *PayPalStandard*）。例如，PayPal Standard 付款外掛的名稱如下：**`Nop.Plugin.Payments.PayPalStandard`**。但請注意，這並非強制規定，您可以選擇任何名稱作為外掛名稱，例如 `MyGreatPlugin`。

    ![p1](_static/how-to-write-plugin-4.40/write_plugin_4.40_1.jpg)

1. 外掛專案建立完成後，您必須使用任何文字編輯器開啟其 `.csproj` 檔案，並將其內容替換為以下內容：

    ```xml
    <Project Sdk="Microsoft.NET.Sdk">
        <PropertyGroup>
            <TargetFramework>net5.0</TargetFramework>
            <Copyright>SOME_COPYRIGHT</Copyright>
            <Company>YOUR_COMPANY</Company>
            <Authors>SOME_AUTHORS</Authors>
            <PackageLicenseUrl>PACKAGE_LICENSE_URL</PackageLicenseUrl>
            <PackageProjectUrl>PACKAGE_PROJECT_URL</PackageProjectUrl>
            <RepositoryUrl>REPOSITORY_URL</RepositoryUrl>
            <RepositoryType>Git</RepositoryType>
            <OutputPath>..\..\Presentation\Nop.Web\Plugins\PLUGIN_OUTPUT_DIRECTORY</OutputPath>
            <OutDir>$(OutputPath)</OutDir>
            <!--Set this parameter to true to get the dlls copied from the NuGet cache to the output of your    project. You need to set this parameter to true if your plugin has a nuget package to ensure that   the dlls copied from the NuGet cache to the output of your project-->
            <CopyLocalLockFileAssemblies>true</CopyLocalLockFileAssemblies>
        </PropertyGroup>
        <ItemGroup>
            <ProjectReference Include="..\..\Presentation\Nop.Web.Framework\Nop.Web.Framework.csproj" />
            <ClearPluginAssemblies Include="$(MSBuildProjectDirectory)\..\..\Build\ClearPluginAssemblies.proj" />
        </ItemGroup>
        <!-- This target execute after "Build" target -->
        <Target Name="NopTarget" AfterTargets="Build">
            <!-- Delete unnecessary libraries from plugins path -->
            <MSBuild Projects="@(ClearPluginAssemblies)" Properties="PluginPath=$(MSBuildProjectDirectory)\$(OutDir)" Targets="NopClear" />
        </Target>
    </Project>
    ```

    > [!TIP]
    > 其中 **PLUGIN_OUTPUT_DIRECTORY** 應替換為外掛名稱，例如 `Payments.PayPalStandard`。
    >
    > 我們採用這種方式是為了能夠使用 .NET Core 中引入的添加第三方參考的新方法。但這並非強制要求。此外，已經參考的程式庫中的參考項目會自動載入，因此非常方便。

1. 下一個步驟是為每個外掛建立一個必要的 `plugin.json` 檔案。此檔案包含描述您外掛的中繼資料資訊。只需從任何其他現有的外掛複製此檔案，並根據您的需求進行修改。關於 `plugin.json` 檔案的資訊，請參閱 [plugin.json 檔案](xref:zh-Hant/developer/plugins/plugin_json)。

1. 最後一個必要的步驟是建立一個實作 **`IPlugin`** 介面（位於 `Nop.Services.Plugins` 命名空間）的類別。nopCommerce 提供了 **`BasePlugin`** 類別，它已經實作了一些 `IPlugin` 方法，可讓您避免重複撰寫原始程式碼。nopCommerce 也提供了一些衍生自 `IPlugin` 的特定介面。例如，我們有用於建立新付款方式外掛的 `IPaymentMethod` 介面。它包含一些僅針對付款方式特定的方法，例如 *`ProcessPayment()`* 或 *`GetAdditionalHandlingFee()`*。目前，nopCommerce 擁有以下特定的外掛介面：

   - **IPaymentMethod**。這些外掛用於處理付款。
   - **IShippingRateComputationMethod**。這些外掛用於擷取可用的配送方式與對應的運費。例如：UPS、FedEx 等。
   - **IPickupPointProvider**。這些外掛用於提供取貨點。
   - **ITaxProvider**。稅務提供者用於取得稅率。
   - **IExchangeRateProvider**。用於取得貨幣匯率。
   - **IDiscountRequirementRule**。允許您建立新的折扣規則，例如「顧客的帳單國家/地區必須是……」。
   - **IExternalAuthenticationMethod**。用於建立外部驗證方法，例如 Facebook、Twitter、OpenID 等。
   - **IMultiFactorAuthenticationMethod**。用於建立多因素驗證方法，例如 *GoogleAuthenticator* 等。
     >[!NOTE]
     > 這是一個新介面，自 4.40 版本起，我們開箱即用地提供了對應的 MFA 整合基礎架構。

   - **IWidgetPlugin**。它允許您建立小工具。小工具會呈現在您網站的某些區塊中。例如，它可以是您網站左欄中的「即時交談」區塊。
   - **IMiscPlugin**。如果您的外掛不適用於上述任何介面。

> [!IMPORTANT]
> 每次專案編譯後，在進行更改前請先清理方案。某些資源會被快取，這可能會導致開發人員感到崩潰。
>
> 新增外掛後，您可能需要重新建置您的方案。如果您在 `Nop.Web\Plugins\PLUGIN_OUTPUT_DIRECTORY` 下沒有看到外掛的 DLL 檔案，您就需要重新建置方案。如果您的 DLL 檔案不存在於 `Nop.Web` 的正確資料夾中，nopCommerce 將不會在「本機外掛」頁面中列出您的外掛。

## 處理請求：Controller、Model 與 View

現在，您可以前往 **後台 → 設定 → 本地外掛** 查看該外掛。但如您所料，目前我們的外掛什麼也沒做，甚至連用於設定的使用者介面都沒有。讓我們來建立一個頁面來設定此外掛。

我們現在需要做的是建立一個 Controller、一個 Model 以及一個 View。

1. MVC Controller 負責回應針對 ASP.NET Core MVC 網站的請求。每個瀏覽器請求都會對應到特定的 Controller。
1. View 包含發送至瀏覽器的 HTML 標記與內容。在進行 `ASP.NET Core MVC` 應用程式開發時，View 就相當於網頁。
1. MVC Model 包含應用程式中所有未被包含在 View 或 Controller 中的邏輯。

您可以找到更多關於 MVC 模式的資訊 [here](https://docs.microsoft.com/aspnet/core/mvc/overview?view=aspnetcore-5.0)。

讓我們開始吧：

- **建立 Model**：在新的外掛中新增一個 **Models** 資料夾，然後新增一個符合您需求的 Model 類別。
- **建立 View**：在新的外掛中新增一個 **Views** 資料夾，然後新增一個名為 `Configure.cshtml` 的 cshtml 檔案。將該 View 檔案的 **"Build Action"** 屬性設為 **"Content"**，並將 **"Copy to Output Directory"** 屬性設為 **"Copy always"**。請注意，設定頁面應使用 `_ConfigurePlugin` 佈局。
- 此外，請確保您的 \Views 目錄中有 `_ViewImports.cshtml` 檔案。您可以直接從任何現有的其他外掛複製過來。
- **建立 Controller**：在新的外掛中新增一個 **Controllers** 資料夾，然後新增一個 Controller 類別。一個好的習慣是將外掛的 Controller 命名為 `{Group}{Name}Controller.cs`，例如 `PaymentPayPalStandardController`。當然，這並非強制規定（僅是建議）。接著，為設定頁面（在後台區域）建立一個適當的 Action 方法。讓我們將其命名為 `Configure`。準備一個 Model 類別，並使用實體路徑 `~/Plugins/{PluginOutputDirectory}/Views/Configure.cshtml` 將其傳遞至對應的 View。
- 請為您的 Action 方法使用以下屬性：

    ```csharp
    [AutoValidateAntiforgeryToken]
    [AuthorizeAdmin] //confirms access to the admin panel
    [Area(AreaNames.Admin)] //specifies the area containing a controller or action
    ```

    > [!TIP]
    > 您也可以直接將這些屬性加入到 Controller 上。在這種情況下，就不需要為每個方法都加上這些標籤。

    例如，打開 `PayPalStandard` 付款外掛，查看其 `PaymentPayPalStandardController` 的實作方式。

接著，對於每個擁有設定頁面的外掛，您都應該指定一個設定 URL。基底類別 `BasePlugin` 擁有 `GetConfigurationPageUrl` 方法，該方法會回傳一個設定 URL：

```csharp
public override string GetConfigurationPageUrl()
{
    return $"{_webHelper.GetStoreLocation()}Admin/{CONTROLLER_NAME}/{ACTION_NAME}";
}
```

其中 **{CONTROLLER_NAME}** 是您的 Controller 名稱，而 **{ACTION_NAME}** 是 Action 的名稱（通常是 `Configure`）。

一旦安裝了外掛並新增了設定方法，您將會在 **後台 → 設定 → 本地外掛** 下方找到一個用來設定此外掛的連結。

> [!TIP]
> 完成上述步驟最簡單的方法是打開任何其他外掛，並將這些檔案複製到您的外掛專案中，然後重新命名對應的類別與目錄即可。

例如，*PayPalStandard* 外掛的專案結構如下圖所示：

![p3](_static/how-to-write-plugin-4.40/write_plugin_4.40_3.jpg)

## 處理 "InstallAsync"、"UninstallAsync" 與 "UpdateAsync" 方法

此步驟為選用。有些外掛在安裝過程中可能需要額外的邏輯。例如，外掛可能需要插入新的語系資源。因此，請開啟您的 `IPlugin` 實作（通常會繼承自 `BasePlugin` 類別）並覆寫下列方法：

1. **InstallAsync**。此方法將在安裝外掛期間呼叫。您可以在此初始化任何設定、插入新的語系資源，或建立新的資料庫資料表（若有需要）。
1. **UninstallAsync**。此方法將在解除安裝外掛期間呼叫。
1. **UpdateAsync**。此方法將在更新外掛期間呼叫（當 `plugin.json` 檔案中的版本號變更時）。

> [!IMPORTANT]
> 若您覆寫了其中一個方法，請勿隱藏其基礎實作。

例如，覆寫後的 `InstallAsync` 方法應包含以下方法呼叫：*`base.Install()`*。*PayPalStandard* 外掛的 `InstallAsync` 方法看起來如下列程式碼所示：

```csharp
public override async Task InstallAsync()
{
    await _settingService.SaveSettingAsync(new PayPalStandardPaymentSettings
    {
        UseSandbox = true
    });
    
    await _localizationService.AddLocaleResourceAsync(new Dictionary<string, string>
    {
        ...
    });
    await base.InstallAsync();
}
```

> [!TIP]
> 已安裝外掛的列表位於 `\App_Data\plugins.json`。該列表會在安裝過程中建立。

## 路由

在這裡，我們將看看如何註冊外掛路由。ASP.NET Core 路由負責將傳入的瀏覽器請求對應到特定的 MVC 控制器動作。您可以找到關於路由的更多資訊 [here](https://docs.microsoft.com/aspnet/core/fundamentals/routing)。請按照以下步驟操作：

如果您需要新增自訂路由，請建立 `RouteProvider.cs` 檔案。它會告知 nopCommerce 系統有關外掛的路由資訊。例如，以下的 `RouteProvider` 類別新增了一個新路由，可以透過開啟您的網頁瀏覽器並導向 `http://www.yourStore.com/Plugins/PaymentPayPalStandard/PDTHandler` 這個 URL 來存取（由 *PayPal* 外掛使用）：

```csharp
public partial class RouteProvider : IRouteProvider
{
    public void RegisterRoutes(IEndpointRouteBuilder endpointRouteBuilder)
    {
        //PDT
        endpointRouteBuilder.MapControllerRoute("Plugin.Payments.PayPalStandard.PDTHandler", "Plugins/PaymentPayPalStandard/PDTHandler",
                new { controller = "PaymentPayPalStandard", action = "PDTHandler" });
    }
    public int Priority => -1;
}
```

## 升級 nopCommerce 可能會導致外掛失效

部分外掛可能會因為版本過舊，而無法在新版本的 nopCommerce 中運作。如果您在升級至新版本後遇到問題，請刪除該外掛，並前往 nopCommerce 官方網站查看是否有可用的更新版本。許多外掛開發者會更新他們的外掛以適應新版本，但有些外掛則不會，這類外掛可能會因 nopCommerce 的功能改進而變得無法使用。但在大多數情況下，您只需開啟對應的 `plugin.json` 檔案並更新 **SupportedVersions** 欄位即可。

## 結論

希望這些內容能協助您開始使用 nopCommerce，並為您打造更精細的外掛做好準備。

## 外掛範本

您可以為新的 nopCommerce 外掛使用我們的 Visual Studio 範本。它可以為開發者節省大量時間，因為現在不需要手動完成所有的初始步驟。例如建立資料夾（Controllers、Views、Models 等）、其他必要的檔案（DependencyRegistrar.cs、_ViewImports.cshtml、ObjectContext、plugin.json 等）、設定、專案參考等。請前往該處查看安裝說明 [here](https://github.com/nopSolutions/nopCommerce-plugin-template-VS/)