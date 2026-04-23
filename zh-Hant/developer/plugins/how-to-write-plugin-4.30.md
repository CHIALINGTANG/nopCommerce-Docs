---
標題: 如何為 nopCommerce 編寫外掛
uid: zh-Hant/developer/plugins/how-to-write-plugin-4.30
作者: git.AndreiMaz
貢獻者: git.skoshelev, git.cromatido, git.DmitriyKulagin
---

# 如何為 nopCommerce 編寫外掛

外掛用於擴充 nopCommerce 的功能。nopCommerce 擁有幾種類型的外掛，例如：付款方式（如 PayPal）、稅務提供者、運送方式計算方法（如 UPS、USPS、FedEx）、小工具（如「線上客服」區塊）等等。nopCommerce 本身已內建許多不同的外掛。您也可以在 [nopCommerce 官方網站](https://www.nopcommerce.com/marketplace) 搜尋各種外掛，看看是否已經有人開發出符合您需求的外掛。如果沒有，本文將引導您完成建立自訂外掛的過程。

## 外掛結構、必要檔案與位置

1. 首先，您需要在解決方案中建立一個新的「類別庫 (Class Library)」專案。建議將所有外掛放置在解決方案根目錄的 `\Plugins` 目錄中（請勿與位於 `\Nop.Web` 目錄中用於已部署外掛的 `\Plugins` 子目錄混淆）。此外，將所有外掛放置在名為 "Plugins" 的解決方案資料夾中也是良好的實務。

    建議的外掛專案命名為 "Nop.Plugin.{Group}.{Name}"。其中 {Group} 為您的外掛群組（例如 "Payment" 或 "Shipping"），{Name} 為您的外掛名稱（例如 "PayPalStandard"）。例如，PayPal Standard 付款外掛的名稱為：Nop.Plugin.Payments.PayPalStandard。但請注意，這並非強制規定，您可以為外掛選擇任何名稱，例如 "MyGreatPlugin"。

    ![p1](_static/how-to-write-plugin-4.30/write_plugin_4.30_1.jpg)

1. 外掛專案建立完成後，您必須在任何文字編輯器中開啟其 `.csproj` 檔案，並將其內容取代為以下內容：

    ```xml
    <Project Sdk="Microsoft.NET.Sdk">
        <PropertyGroup>
            <TargetFramework>netcoreapp3.1</TargetFramework>
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
    > 其中 PLUGIN_OUTPUT_DIRECTORY 應替換為外掛名稱，例如 Payments.PayPalStandard。
    >
    > 我們這樣做是為了能夠使用 .NET Core 中引入的新方法來新增第三方參考，但這並非強制。此外，已參考之程式庫的參考將會自動載入，非常方便。

1. 下一步是為每個外掛建立一個必要的 `plugin.json` 檔案。此檔案包含描述您外掛的中繼資料。只需從任何其他現有的外掛複製此檔案並依需求進行修改。有關 `plugin.json` 檔案的資訊，請參閱 [plugin.json 檔案](xref:zh-Hant/developer/plugins/plugin_json)。

1. 最後一個必要步驟是建立一個實作 **IPlugin** 介面（Nop.Services.Plugins 命名空間）的類別。nopCommerce 提供了 **BasePlugin** 類別，它已經實作了一些 IPlugin 方法，讓您能避免重複撰寫原始程式碼。nopCommerce 還提供了一些衍生自 IPlugin 的特定介面。例如，我們有 "IPaymentMethod" 介面，用於建立新的付款方式外掛。它包含一些僅適用於付款方式的方法，例如 *ProcessPayment()* 或 *GetAdditionalHandlingFee()*。目前，nopCommerce 擁有以下特定的外掛介面：

   - **IPaymentMethod**：這些外掛用於處理付款。
   - **IShippingRateComputationMethod**：這些外掛用於檢索已接受的運送方式與對應的運費。例如 UPS、FedEx 等。
   - **IPickupPointProvider**：這些外掛用於提供取貨點。
   - **ITaxProvider**：稅務提供者用於取得稅率。
   - **IExchangeRateProvider**：用於取得貨幣匯率。
   - **IDiscountRequirementRule**：允許您建立新的折扣規則，例如「顧客的帳單地址國家/地區應為…」。
   - **IExternalAuthenticationMethod**：用於建立外部驗證方法，例如 Facebook、Twitter、OpenID 等。
   - **IWidgetPlugin**：允許您建立小工具。小工具會呈現在網站的某些部分，例如網站左側欄位的「線上客服」區塊。
   - **IMiscPlugin**：如果您的外掛不符合上述任何介面，請使用此介面。

> [!IMPORTANT]
> 重要提示：每次建置專案後，在進行更改前請先清理解決方案。某些資源會被快取，可能導致開發人員困擾。
>
> 新增外掛後，您可能需要重新建置解決方案。如果您在 Nop.Web\Plugins\PLUGIN_OUTPUT_DIRECTORY 下看不到外掛的 DLL 檔案，則需要重新建置解決方案。如果您的 DLL 檔案不存在於 Nop.Web 的正確資料夾中，nopCommerce 將不會在「本機外掛 (Local Plugins)」頁面中列出您的外掛。

## 處理請求：控制器 (Controllers)、模型 (Models) 與檢視 (Views)

現在您前往 **後台 → 設定 → 本地外掛** 即可看到該外掛。但正如您所料，我們的外掛目前什麼功能都沒有，甚至連設定頁面的使用者介面都還沒建立。讓我們來建立一個外掛的設定頁面。

我們現在需要做的是建立一個控制器、一個模型以及一個檢視。

1. MVC 控制器負責回應對 ASP.NET MVC 網站發出的請求。每一個瀏覽器請求都會對應到特定的控制器。
1. 檢視包含傳送至瀏覽器的 HTML 標記與內容。在 ASP.NET MVC 應用程式中，檢視就等同於一個頁面。
1. MVC 模型包含所有未包含在檢視或控制器中的應用程式邏輯。

您可以透過 [here](https://docs.microsoft.com/aspnet/core/mvc/overview?view=aspnetcore-3.1) 了解更多關於 MVC 模式的資訊。

讓我們開始吧：

- **建立模型**：在新的外掛中加入一個 **Models** 資料夾，然後加入一個符合您需求的模型類別。
- **建立檢視**：在新的外掛中加入一個 **Views** 資料夾，然後新增一個名為 `Configure.cshtml` 的 cshtml 檔案。將該檢視檔案的 **"Build Action"** 屬性設為 **"Content"**，並將 **"Copy to Output Directory"** 屬性設為 **"Copy always"**。請注意，設定頁面應使用 "_ConfigurePlugin" 頁面範本。
- 同時請確保您的 \Views 目錄下擁有 `_ViewImports.cshtml` 檔案。您可以直接從任何其他現有的外掛中複製過來。
- **建立控制器**：在新的外掛中加入一個 **Controllers** 資料夾，然後加入一個新的控制器類別。建議將外掛控制器命名為 `{Group}{Name}Controller.cs`，例如 `PaymentPayPalStandardController`。當然，這並非強制規定，僅為建議。接著，為設定頁面（在後台區域）建立一個對應的動作方法 (Action Method)。讓我們將其命名為 *"Configure"*。準備一個模型類別，並使用實體檢視路徑 `~/Plugins/{PluginOutputDirectory}/Views/Configure.cshtml` 將其傳遞給檢視。
- 請為您的動作方法使用以下屬性：

    ```csharp
    [AuthorizeAdmin] //confirms access to the admin panel
    [Area(AreaNames.Admin)] //specifies the area containing a controller or action
    ```

    例如，開啟 PayPalStandard 付款外掛並查看其 `PaymentPayPalStandardController` 的實作方式。

接著，對於每個具有設定頁面的外掛，您應該指定一個設定 URL。名為 `BasePlugin` 的基底類別提供 `GetConfigurationPageUrl` 方法，該方法會回傳一個設定 URL：

```csharp
public override string GetConfigurationPageUrl()
{
    return $"{_webHelper.GetStoreLocation()}Admin/{CONTROLLER_NAME}/{ACTION_NAME}";
}
```

其中 *{CONTROLLER_NAME}* 是您的控制器名稱，而 *{ACTION_NAME}* 是動作方法的名稱（通常為 "Configure"）。

一旦您安裝了外掛並加入了設定方法，您將會在 **後台 → 設定 → 本地外掛** 下方找到一個設定此外掛的連結。

> [!TIP]
> 完成上述步驟最簡單的方式，就是開啟任何其他外掛，並將這些檔案複製到您的外掛專案中，接著再重新命名對應的類別與目錄即可。

例如，PayPalStandard 外掛的專案結構如下圖所示：

![p3](_static/how-to-write-plugin-4.30/write_plugin_4.30_3.jpg)

## 處理 "Install"、"Uninstall" 與 "Update" 方法

此步驟為選用。有些外掛在安裝過程中可能需要額外的邏輯。例如，外掛可能需要插入新的語言資源。請開啟您的 `IPlugin` 實作（大多數情況下將繼承自 `BasePlugin` 類別）並覆寫下列方法：

1. **Install**。此方法將在安裝外掛時被呼叫。您可以在此初始化任何設定、插入新的語言資源，或建立新的資料庫資料表（若有需要）。
1. **Uninstall**。此方法將在外掛解除安裝時被呼叫。
1. **Update**。此方法將在外掛更新時被呼叫（當 `plugin.json` 檔案中的版本號變更時）。

> [!IMPORTANT]
> 重要提示：如果您覆寫了這些方法中的其中一個，請勿隱藏其基礎實作。

例如，覆寫的 "Install" 方法應包含下列方法呼叫：*base.Install()*。PayPalStandard 外掛的 "Install" 方法如下列程式碼所示：

```csharp
public override void Install()
{
    var settings = new PayPalStandardPaymentSettings()
    {
        UseSandbox = true
    };
    _settingService.SaveSetting(settings);
    ...
    base.Install();
}
```

> [!TIP]
> 已安裝外掛的清單位於 `\App_Data\plugins.json`。該清單會在安裝過程中建立。

## 路由

在這裡，我們將探討如何註冊外掛路由。ASP.NET Core 路由負責將傳入的瀏覽器請求對應到特定的 MVC 控制器動作。您可以參閱 [here](https://docs.microsoft.com/aspnet/core/fundamentals/routing) 以取得更多關於路由的資訊。請按照以下步驟操作：

如果您需要新增自訂路由，請建立 `RouteProvider.cs` 檔案。它會將外掛路由的資訊通知給 nopCommerce 系統。例如，以下的 RouteProvider 類別新增了一個新路由，您可以透過開啟網頁瀏覽器並前往 `http://www.yourStore.com/Plugins/PaymentPayPalStandard/PDTHandler` URL 來存取它（由 PayPal 外掛使用）：

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

部分外掛可能會過時，導致無法在較新版本的 nopCommerce 中運作。如果您在升級到新版本後遇到問題，請刪除該外掛，並前往 nopCommerce 官方網站查看是否有更新的版本可用。許多外掛開發者會更新他們的外掛以適應新版本，然而，有些開發者可能不會這麼做，導致他們的產品隨著 nopCommerce 的改進而變得過時。但在大多數情況下，您只需要開啟適當的 `plugin.json` 檔案並更新 **SupportedVersions** 欄位即可。

## 結論

希望這能協助您開始使用 nopCommerce，並為您開發更複雜的外掛做好準備。

## 外掛範本

您可以為新的 nopCommerce 外掛使用我們的 Visual Studio 範本。這能為開發者節省大量時間，因為開發者不再需要手動完成所有初始步驟，例如建立資料夾（Controllers、Views、Models 等）、建立其他必要檔案（DependencyRegistrar.cs、_ViewImports.cshtml、ObjectContext、plugin.json 等）、設定組態、專案參考等。請前往 [here](https://github.com/nopSolutions/nopCommerce-plugin-template-VS/) 尋找範本及安裝說明。