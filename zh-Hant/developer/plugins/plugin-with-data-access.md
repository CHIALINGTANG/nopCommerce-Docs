---
標題: 具備資料存取的開發外掛
uid: zh-Hant/developer/plugins/plugin-with-data-access
作者: git.AndreiMaz
貢獻者: git.DmitriyKulagin, git.skoshelev, git.cromatido
---

# 具備資料存取的開發外掛

在本教學課程中，我將使用 nopCommerce 的外掛架構來實作一個商品瀏覽追蹤器。在開始開發之前，您必須先閱讀、理解並完成下列列出的教學課程。我會略過一些先前文章中已涵蓋的解釋，但您可以透過提供的連結進行回顧。

- [開發者教學課程](xref:zh-Hant/developer/tutorials/index)
- [更新現有實體。如何新增屬性。](xref:zh-Hant/developer/tutorials/update-existing-entity)
- [如何為 nopCommerce 4.90 編寫外掛](xref:zh-Hant/developer/plugins/how-to-write-plugin-4.90)

我們將從資料存取層（data access layer）開始編寫程式碼，接著進入服務層（service layer），最後完成相依性注入（Dependency Injection）。

## 開始使用

建立一個新的類別庫專案「Nop.Plugin.Misc.ProductViewTracker」。

新增 `plugin.json` 檔案。

> [!TIP]
> 關於 `plugin.json` 檔案的詳細資訊，請參閱 [plugin.json 檔案](xref:zh-Hant/developer/plugins/plugin_json)。

接著，新增對 **Nop.Web.Framework** 專案的參考。這樣就足夠了，因為其他依賴項目（如 **Nop.Core** 和 **Nop.Data**）會自動進行連結。

## 資料存取層（又稱在 nopCommerce 中建立新實體）

在 "*domain*" 命名空間內，我們將建立一個名為 **`ProductViewTrackerRecord`** 的公開類別。此類別繼承自 **`BaseEntity`**，除此之外它是一個非常簡單的檔案。有一點需要銘記，我們不會使用導覽屬性（關聯屬性），因為我們用來處理資料庫的 *Linq2DB* 框架並不支援導覽屬性。

```csharp
namespace Nop.Plugin.Misc.ProductViewTracker.Domain

public class ProductViewTrackerRecord : BaseEntity
{
    public int ProductId { get; set; }
    public string ProductName { get; set; }
    public int CustomerId { get; set; }
    public string IpAddress { get; set; }
    public bool IsRegistered { get; set; }
}
```

下一個要建立的類別是 *FluentMigrator* 實體建構器類別。在映射類別中，我們會映射欄位、資料表關聯以及資料庫資料表。

```csharp
using FluentMigrator.Builders.Create.Table;
using Nop.Core.Domain.Catalog;
using Nop.Core.Domain.Customers;
using Nop.Data.Mapping.Builders;
using Nop.Plugin.Other.ProductViewTracker.Domains;
using Nop.Data.Extensions;
using System.Data;

namespace Nop.Plugin.Other.ProductViewTracker.Mapping.Builders

public class ProductViewTrackerRecordBuilder : NopEntityBuilder<ProductViewTrackerRecord>
{
    /// <summary>
    /// Apply entity configuration
    /// </summary>
    /// <param name="table">Create table expression builder</param>
    public override void MapEntity(CreateTableExpressionBuilder table)
    {
        //map the primary key (not necessary if it is Id field)
        table.WithColumn(nameof(ProductViewTrackerRecord.Id)).AsInt32().PrimaryKey()
        //map the additional properties as foreign keys
        .WithColumn(nameof(ProductViewTrackerRecord.ProductId)).AsInt32().ForeignKey<Product>(onDelete: Rule.Cascade)
        .WithColumn(nameof(ProductViewTrackerRecord.CustomerId)).AsInt32().ForeignKey<Customer>(onDelete: Rule.Cascade)
        //avoiding truncation/failure
        //so we set the same max length used in the product name
        .WithColumn(nameof(ProductViewTrackerRecord.ProductName)).AsString(400)
        //not necessary if we don't specify any rules
        .WithColumn(nameof(ProductViewTrackerRecord.IpAddress)).AsString()
        .WithColumn(nameof(ProductViewTrackerRecord.IsRegistered)).AsInt32();
    }
}
```

對我們來說下一個重要的類別是遷移類別，它會直接在資料庫中建立我們的資料表。您可以在外掛中建立任意數量的遷移，唯一需要留意的是遷移的版本。我們特別建立了 **NopMigration** 屬性來讓您的作業更輕鬆。透過在此處標示最完整且準確的檔案建立日期，您幾乎可以確保遷移編號的唯一性。

```csharp
using FluentMigrator;
using Nop.Data.Extensions;
using Nop.Data.Migrations;
using Nop.Plugin.Other.ProductViewTracker.Domains;

namespace Nop.Plugin.Other.ProductViewTracker.Migrations

[NopSchemaMigration("2020/05/27 08:40:55:1687541", "Other.ProductViewTracker base schema", MigrationProcessType.Installation)]
public class SchemaMigration : ForwardOnlyMigration
{
    public override void Up()
    {
        Create.TableFor<ProductViewTrackerRecord>();            
    }
}
```

## 服務層 (Service layer)

服務層連接了資料存取層與呈現層。由於在程式碼中共享任何類型的職責是不良的習慣，因此每一層都需要被隔離。服務層以商業邏輯包裝資料層，而呈現層則依賴於服務層。由於我們的任務非常簡單，我們的服務層僅負責與 Repository 通訊（在 nopCommerce 中，Repository 扮演著物件內容的 Facade 角色）。

```csharp
using Nop.Data;
using Nop.Plugin.Other.ProductViewTracker.Domains;
using System;

namespace Nop.Plugin.Other.ProductViewTracker.Services

public interface IProductViewTrackerService
{
    /// <summary>
    /// Logs the specified record.
    /// </summary>
    /// <param name="record">The record.</param>
    void Log(ProductViewTrackerRecord record);
}


namespace Nop.Plugin.Misc.ProductViewTracker.Services

public class ProductViewTrackerService : IProductViewTrackerService
{
    private readonly IRepository<ProductViewTrackerRecord> _productViewTrackerRecordRepository;
    public ProductViewTrackerService(IRepository<ProductViewTrackerRecord> productViewTrackerRecordRepository)
    {
        _productViewTrackerRecordRepository = productViewTrackerRecordRepository;
    }
    /// <summary>
    /// Logs the specified record.
    /// </summary>
    /// <param name="record">The record.</param>
    public virtual void Log(ProductViewTrackerRecord record)
    {
        if (record == null)
            throw new ArgumentNullException(nameof(record));
        _productViewTrackerRecordRepository.Insert(record);
    }
}
```

## 相依性注入

Martin Fowler 撰寫了一篇關於相依性注入（Dependency Injection）或控制反轉（Inversion of Control）的精彩文章。我不會重複他的內容，您可以 [在此處閱讀他的文章](https://martinfowler.com/articles/injection.html)。相依性注入管理物件的生命週期，並提供實例供相依物件使用。首先，我們需要設定相依性容器，以便它了解將要控制哪些物件，以及這些物件的建立需要適用哪些規則。

```csharp
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Nop.Core.Infrastructure;
using Nop.Plugin.Other.ProductViewTracker.Services;

namespace Nop.Plugin.Other.ProductViewTracker.Infrastructure

/// <summary>
/// Represents object for the configuring services on application startup
/// </summary>
public class NopStartup : INopStartup
{
    /// <summary>
    /// Add and configure any of the middleware
    /// </summary>
    /// <param name="services">Collection of service descriptors</param>
    /// <param name="configuration">Configuration of the application</param>
    public void ConfigureServices(IServiceCollection services, IConfiguration configuration)
    {
        services.AddScoped<IProductViewTrackerService, ProductViewTrackerService>();
    }
    /// <summary>
    /// Configure the using of added middleware
    /// </summary>
    /// <param name="application">Builder for configuring an application's request pipeline</param>
    public void Configure(IApplicationBuilder application)
    {
    }
    /// <summary>
    /// Gets order of this startup configuration implementation
    /// </summary>
    public int Order => 3000;
}
```

在上面的程式碼中，我們註冊了不同類型的物件，以便稍後可以將它們注入到控制器、服務和 Repository 中。現在我們已經涵蓋了新的主題，我將帶回一些舊有的主題，以便我們完成此外掛。

## 檢視元件 (View component)

讓我們來建立一個檢視元件：

```csharp
using Microsoft.AspNetCore.Mvc;
using Nop.Core;
using Nop.Plugin.Other.ProductViewTracker.Domains;
using Nop.Plugin.Other.ProductViewTracker.Services;
using Nop.Services.Catalog;
using Nop.Services.Customers;
using Nop.Web.Framework.Components;
using Nop.Web.Models.Catalog;

namespace Nop.Plugin.Other.ProductViewTracker.Components

public class ProductViewTrackerViewComponent : NopViewComponent
{
    private readonly ICustomerService _customerService;
    private readonly IProductService _productService;
    private readonly IProductViewTrackerService _productViewTrackerService;
    private readonly IWorkContext _workContext;
    public ProductViewTrackerViewComponent(ICustomerService customerService,
        IProductService productService,
        IProductViewTrackerService productViewTrackerService,
        IWorkContext workContext)
    {
        _customerService = customerService;
        _productService = productService;
        _productViewTrackerService = productViewTrackerService;
        _workContext = workContext;
    }
    public async Task<IViewComponentResult> InvokeAsync(string widgetZone, object additionalData)
    {
        if (!(additionalData is ProductDetailsModel model))
            return Content("");
        //Read from the product service
        var productById = await _productService.GetProductByIdAsync(model.Id);
        //If the product exists we will log it
        if (productById != null)
        {
            var currentCustomer = await _workContext.CurrentCustomerAsync();
            //Setup the product to save
            var record = new ProductViewTrackerRecord
            {
                ProductId = model.Id,
                ProductName = productById.Name,
                CustomerId = currentCustomer.Id,
                IpAddress = currentCustomer.LastIpAddress,
                IsRegistered = await _customerService.Async(currentCustomer)
            };
            //Map the values we're interested in to our new entity
            _productViewTrackerService.Log(record);
        }
        return Content("");
    }
}
```

## 主要的外掛類別

> [!IMPORTANT]
>
> 我們將外掛實作為一個小工具。在這種情況下，我們不需要編輯 `cshtml` 檔案。

```csharp
using Nop.Services.Cms;
using Nop.Services.Plugins;
using Nop.Web.Framework.Infrastructure;
using System.Collections.Generic;

namespace Nop.Plugin.Other.ProductViewTracker

public class ProductViewTrackerPlugin : BasePlugin, IWidgetPlugin
{
    /// <summary>
    /// Gets a value indicating whether to hide this plugin on the widget list page in the admin area
    /// </summary>
    public bool HideInWidgetList => true;
    /// <summary>
    /// Gets a type of a view component for displaying widget
    /// </summary>
    /// <param name="widgetZone">Name of the widget zone</param>
    /// <returns>View component type</returns>
    public Type GetWidgetViewComponent(string widgetZone)
    {
        return typeof(ProductViewTrackerViewComponent);
    }
    /// <summary>
    /// Gets widget zones where this widget should be rendered
    /// </summary>
    /// <returns>Widget zones</returns>
    public Task<IList<string>> GetWidgetZonesAsync()
    {
        return Task.FromResult<IList<string>>(new List<string> { PublicWidgetZones.ProductDetailsTop });
    }
}
```