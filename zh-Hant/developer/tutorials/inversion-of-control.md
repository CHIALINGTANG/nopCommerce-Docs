---
標題: 控制反轉與相依性注入
uid: zh-Hant/developer/tutorials/inversion-of-control
作者: git.AndreiMaz
貢獻者: git.exileDev, git.DmitriyKulagin
---

# 控制反轉與相依性注入

控制反轉（Inversion of Control）與相依性注入（Dependency Injection）是兩種相關的技術，用於拆解應用程式中的相依關係。[控制反轉 (IoC)](https://en.wikipedia.org/wiki/Inversion_of_control) 指的是物件不會自行建立其運作所需的其他物件，而是從外部來源取得所需的物件。[相依性注入 (DI)](http://en.wikipedia.org/wiki/Dependency_injection) 指的是在物件不進行干預的情況下，通常由框架元件負責傳遞建構函式參數並設定屬性，以達成上述目的。Martin Fowler 針對相依性注入或「控制反轉」撰寫了非常詳盡的說明。我們在此不重複他的論述，您可以透過此連結 [here](https://martinfowler.com/articles/injection.html) 找到他的文章。nopCommerce 使用了 ASP.NET Core 內建的 DI 容器，該容器由 `IServiceProvider` 介面表示。此容器負責將相依性對應至特定型別，並將相依性注入到各個物件中。一旦編寫好服務以及該服務所實作的對應介面，您就應該在任何實作 `INopStartup` 介面（位於 `Nop.Core.Infrastructure` 命名空間）的類別中進行註冊。`ConfigureServices` 方法負責在應用程式中安裝服務。服務會透過 `IServiceCollection` 參數新增至專案中。

```csharp
    public class NopStartup : INopStartup
    {
        public virtual void ConfigureServices(IServiceCollection services, IConfiguration configuration)
        {
                services.AddScoped<IWebHelper, WebHelper>();
            ...
        }

        public void Configure(IApplicationBuilder application)
        {
        }

        public int Order => 2000;
    }
```

您可以視需求建立多個相依性註冊類別。請注意，每個實作 **INopStartup** 介面的類別都具有一個 **Order** 屬性。這允許您取代現有的相依性。若要覆寫 nopCommerce 的相依性，請將 `Order` 屬性設定為大於 0 的數值。nopCommerce 會對相依性類別進行排序，並按遞增順序執行。數值越高，您的物件註冊的時間點就越晚。

透過這種方式，您可以同時註冊內建的 ASP.NET Core 服務與您自己的服務；同樣的註冊機制也適用於在外掛中註冊服務。