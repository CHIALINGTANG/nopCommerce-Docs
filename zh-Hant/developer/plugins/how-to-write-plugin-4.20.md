---
標題: 如何為 nopCommerce 編寫外掛
uid: zh-Hant/developer/plugins/how-to-write-plugin-4.20
作者: git.AndreiMaz
貢獻者: git.Kevat, git.exileDev, git.DmitriyKulagin, git.cromatido
---

# 如何為 nopCommerce 4.20 編寫外掛

外掛用於擴充 nopCommerce 的功能。nopCommerce 擁有多種不同類型的外掛。例如，付款方式（如 PayPal）、稅額計算提供者、運送方式計算方法（如 UPS、USPS、FedEx）、小工具（如「即時聊天」區塊）等等。nopCommerce 本身已經隨附了許多不同的外掛。您也可以在 [nopCommerce 官方網站](https://www.nopcommerce.com/marketplace) 上搜尋各種外掛，看看是否已經有人開發了符合您需求的外掛。如果沒有，本文將引導您完成建立自訂外掛的過程。

## 外掛結構、必要檔案與位置

1. 您首先需要做的是在方案中建立一個新的「類別庫 (Class Library)」專案。將所有外掛放置在方案根目錄的 `\Plugins` 資料夾中是個好習慣（請勿與 `\Nop.Web` 目錄下的 `\Plugins` 子目錄混淆，後者是用於已部署的外掛）。將所有外掛放置在「Plugins」方案資料夾中也是個好習慣（關於方案資料夾，您可以參考 [here](http://msdn.microsoft.com/library/sx2027y2.aspx) 以取得更多資訊）。

    外掛專案的建議命名方式為「Nop.Plugin.{Group}.{Name}」。其中 {Group} 是您的外掛群組（例如「Payment」或「Shipping」），{Name} 則是您的外掛名稱（例如「PayPalStandard」）。例如，PayPal Standard 付款外掛的名稱為：Nop.Plugin.Payments.PayPalStandard。但請注意，這並非強制要求，您可以為外掛選擇任何名稱，例如「MyGreatPlugin」。

    ![p1](_static/how-to-write-plugin-4.20/write_plugin_4.20_1.jpg)

1. 外掛專案建立完成後，您必須在任何文字編輯器中開啟其 `.csproj` 檔案，並將其內容替換為以下內容：

    ```xml
    <Project Sdk="Microsoft.NET.Sdk">
        <PropertyGroup>
            <TargetFramework>netcoreapp2.2</TargetFramework>
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
    > 我們這樣做是為了能夠使用 .NET Core 中引入的新方法來新增第三方參考，但這並非強制。此外，已參考函式庫中的參考項目會自動載入，因此非常方便。

1. 下一個步驟是建立每個外掛都必須具備的 `plugin.json` 檔案。此檔案包含描述您外掛的中繼資訊。您只需從任何其他現有的外掛中複製此檔案，並根據您的需求進行修改即可。關於 `plugin.json` 檔案的資訊，請參閱 [plugin.json file](xref:zh-Hant/developer/plugins/plugin_json)。

1. 最後一個必要步驟是建立一個實作 **IPlugin** 介面（位於 Nop.Services.Plugins 命名空間）的類別。nopCommerce 提供了 **BasePlugin** 類別，該類別已經實作了部分 IPlugin 方法，可協助您避免重複編寫原始程式碼。nopCommerce 也提供了一些衍生自 IPlugin 的特定介面。例如，我們有用於建立新付款方式外掛的「IPaymentMethod」介面。它包含一些僅適用於付款方式的方法，例如 *ProcessPayment()* 或 *GetAdditionalHandlingFee()*。目前，nopCommerce 擁有以下特定的外掛介面：

   - **IPaymentMethod**。這些外掛用於處理付款。
   - **IShippingRateComputationMethod**。這些外掛用於擷取可接受的配送方式與對應的運費費率。例如：UPS、FedEx 等。
   - **IPickupPointProvider**。這些外掛用於提供取貨點。
   - **ITaxProvider**。稅務提供者用於取得稅率。
   - **IExchangeRateProvider**。用於取得貨幣匯率。
   - **IDiscountRequirementRule**。允許您建立新的折扣規則，例如「顧客的帳單國家/地區必須是……」。
   - **IExternalAuthenticationMethod**。用於建立外部驗證方式，例如 Facebook、Twitter、OpenID 等。
   - **IWidgetPlugin**。允許您建立小工具。小工具會呈現在網站的特定區塊。例如，它可以是網站左側欄位中的「線上客服」區塊。
   - **IMiscPlugin**。如果您的外掛不適用於上述任何介面。

> [!IMPORTANT]
> 每次編譯專案後，請在進行更改前先清理方案。某些資源會被快取，這可能會導致開發者抓狂。
>
> 新增外掛後，您可能需要重新建置您的方案。如果您在 Nop.Web\Plugins\PLUGIN_OUTPUT_DIRECTORY 下看不到外掛的 DLL 檔案，則需要重新建置方案。如果您的 DLL 檔案不存在於 Nop.Web 的正確資料夾中，nopCommerce 將不會在「本機外掛 (Local Plugins)」頁面中列出您的外掛。

## 處理請求：控制器 (Controllers)、模型 (Models) 與檢視 (Views)

現在，您可以前往 **後台 → 設定 → 本地外掛** 來查看該外掛。但如您所料，我們的外掛目前什麼功能都沒有，甚至連設定用的使用者介面都沒有。讓我們來建立一個頁面來設定外掛。

我們現在需要做的是建立一個控制器、一個模型以及一個檢視。

1. MVC 控制器負責回應針對 ASP.NET MVC 網站提出的請求。每個瀏覽器請求都會對應到特定的控制器。
1. 檢視包含發送到瀏覽器的 HTML 標記與內容。在開發 ASP.NET MVC 應用程式時，檢視就相當於一個頁面。
1. MVC 模型包含應用程式中所有未包含在檢視或控制器中的邏輯。

您可以找到更多關於 MVC 模式的資訊 [here](http://www.asp.net/mvc/tutorials/older-versions/overview/understanding-models-views-and-controllers-cs)。

那麼讓我們開始吧：

- **建立模型**。在新的外掛中新增一個 **Models** 資料夾，然後新增一個符合您需求的模型類別。
- **建立檢視**。在新的外掛中新增一個 **Views** 資料夾，然後新增一個名為 `Configure.cshtml` 的 cshtml 檔案。將該檢視檔案的 **"Build Action"** 屬性設為 **"Content"**，並將 **"Copy to Output Directory"** 屬性設為 **"Copy always"**。請注意，設定頁面應使用 "_ConfigurePlugin" 頁面範本。
- 同時請確保您的 \Views 目錄下有 `_ViewImports.cshtml` 檔案。您可以直接從任何現有的其他外掛複製一份。
- **建立控制器**。在新的外掛中新增一個 **Controllers** 資料夾，然後新增一個控制器類別。一個好的做法是將外掛控制器命名為 `{Group}{Name}Controller.cs`。例如：PaymentPayPalStandardController。當然，這並非強制規定（僅為建議）。接著，為設定頁面（在管理後台）建立適當的動作方法。讓我們將其命名為 *"Configure"*。準備一個模型類別，並使用實體檢視路徑將其傳遞給檢視：`~/Plugins/{PluginOutputDirectory}/Views/Configure.cshtml`。
- 為您的動作方法使用下列屬性：

    ```csharp
    [AuthorizeAdmin] //confirms access to the admin panel
    [Area(AreaNames.Admin)] //specifies the area containing a controller or action
    ```

    例如，開啟 PayPalStandard 付款外掛，查看其 PaymentPayPalStandardController 的實作方式。

接著，對於每個擁有設定頁面的外掛，您都應該指定一個設定 URL。名為 `BasePlugin` 的基礎類別擁有 `GetConfigurationPageUrl` 方法，該方法會回傳設定 URL：

```csharp
public override string GetConfigurationPageUrl()
{
    return $"{_webHelper.GetStoreLocation()}Admin/{CONTROLLER_NAME}/{ACTION_NAME}";
}
```

其中 *{CONTROLLER_NAME}* 是您的控制器名稱，而 *{ACTION_NAME}* 是動作名稱（通常為 "Configure"）。

一旦您安裝了外掛並新增了設定方法，您就會在 **後台 → 設定 → 本地外掛** 下方找到一個設定外掛的連結。

> [!TIP]
> 完成上述步驟最簡單的方法，就是開啟任何其他外掛，並將這些檔案複製到您的外掛專案中，然後重新命名對應的類別與目錄。

例如，PayPalStandard 外掛的專案結構如下圖所示：

![p3](_static/how-to-write-plugin-4.20/write_plugin_4.20_3.jpg)

## 處理 "Install" 與 "Uninstall" 方法

此步驟為選用。某些外掛在安裝過程中可能需要額外的邏輯。例如，外掛可能需要插入新的語言資源。因此，請開啟您的 `IPlugin` 實作（大多數情況下會繼承自 `BasePlugin` 類別）並覆寫下列方法：

1. **Install**。此方法將在外掛安裝期間被呼叫。您可以在此處初始化任何設定、插入新的語言資源，或建立一些新的資料庫表格（若有需要）。
1. **Uninstall**。此方法將在外掛移除期間被呼叫。

> [!IMPORTANT]
> 如果您覆寫了其中一個方法，請勿隱藏其基礎實作。

例如，覆寫的 "Install" 方法應包含下列方法呼叫：*base.Install()*。PayPalStandard 外掛的 "Install" 方法程式碼如下所示：

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
> 已安裝外掛的列表位於 `\App_Data\Plugins.json`。該列表會在安裝期間建立。

## 路由

在這裡，我們將探討如何註冊外掛路由。ASP.NET Core 路由負責將傳入的瀏覽器請求對應到特定的 MVC 控制器動作。您可以在 [here](https://docs.microsoft.com/aspnet/core/fundamentals/routing) 找到更多關於路由的資訊。請依照下列步驟操作：

1. 如果您需要新增自訂路由，請建立 `RouteProvider.cs` 檔案。它會將外掛路由的資訊通知給 nopCommerce 系統。例如，以下的 RouteProvider 類別新增了一個新路由，您可以透過開啟網頁瀏覽器並導向 `http://www.yourStore.com/Plugins/PaymentPayPalStandard/PDTHandler` URL 來存取（由 PayPal 外掛使用）：

```csharp
public partial class RouteProvider : IRouteProvider
{
    public void RegisterRoutes(IRouteBuilder routeBuilder)
    {
         routeBuilder.MapRoute("Plugin.Payments.PayPalStandard.PDTHandler", "Plugins/PaymentPayPalStandard/PDTHandler",
            new { controller = "PaymentPayPalStandard", action = "PDTHandler" });
    }
    public int Priority => -1;
}
```

## 升級 nopCommerce 可能會導致外掛失效

部分外掛可能會過時，並無法再與較新版本的 nopCommerce 相容。若您在升級至新版本後遇到問題，請刪除該外掛，並前往 nopCommerce 官方網站查看是否有提供更新版本。許多外掛開發者會更新其外掛以適應新版本，然而，部分開發者可能不會這麼做，導致其外掛因 nopCommerce 的改進而變得無法使用。但在大多數情況下，您只需開啟對應的 `plugin.json` 檔案，並更新 **SupportedVersions** 欄位即可。

## 結論

希望這能協助您開始使用 nopCommerce，並為您日後開發更複雜的外掛做好準備。

## 外掛範本

您可以針對新的 nopCommerce 外掛使用我們的 Visual Studio 範本。這可以為開發者節省大量時間，因為他們不再需要手動執行所有初始步驟。例如：建立資料夾（Controllers、Views、Models 等）、建立其他必要檔案（DependencyRegistrar.cs、_ViewImports.cshtml、ObjectContext、plugin.json 等）、進行設定、專案參考等。請前往 [here](https://github.com/nopSolutions/nopCommerce-plugin-template-VS/) 尋找範本與安裝說明。