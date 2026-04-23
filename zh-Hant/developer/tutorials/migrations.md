---
標題: 資料庫遷移
uid: zh-Hant/developer/tutorials/migrations
作者: git.AndreiMaz
貢獻者: git.skoshelev, git.DmitriyKulagin
---
# 資料庫遷移

## 資料庫處理方式變更的簡述

在 nopCommerce 4.30 版本中，資料庫的處理方式進行了大幅度的重構。第一個可以注意到的變化是完全放棄了導覽屬性 (Navigation properties)。我們可能會針對此方式的實用性進行討論與爭辯，但它確實有幾個正面優點：

1. 簡化了程式碼的理解與維護。
    > [!NOTE]
    > 在程式碼重構過程中，我們發現並修正了幾個影響效能與功能性的不精確之處。
1. 對查詢及其執行時機擁有完全的控制權（這對整體解決方案的效能有正面的影響）。
1. 簡化了遷移至任何資料庫框架的可能性（這點最為重要）。

由於 nopCommerce 完全轉換至 .Net Core (4.10 版本) 並成為跨平台解決方案，支援多種資料庫已變得越來越重要。nopCommerce 團隊進行了大量的研究與分析，決定放棄使用標準的 Entity Framework Core。同時，我們決定不再透過 OOP 方式（這是 C# 開發人員最常用的方式）使用 LINQ 查詢來操作資料庫。最終選擇了 Linq2DB 與 FluentMigrator 的組合。以下我將詳細說明這兩個框架各自的角色。

## Linq2DB

> [!NOTE]
> 從 4.30 版本開始，nopCommerce 使用 Linq2DB 作為 ORM 框架。Linq2DB 是一個物件關聯對映 (ORM) 框架，它讓 .NET 開發人員能夠使用 .NET 物件來操作資料庫。它可以將 .Net 物件對應到多種不同的資料庫提供者。

在 nopCommerce 中，Linq2DB 被用作資料庫存取層。目前，nopCommerce 支援三種最受歡迎的資料庫：MS SQL Server、MySQL 與 PostgreSQL。如果我們分析程式碼，可以輕易看出每個資料庫都有各自實作 `INopDataProvider` 介面的類別。但如果您不打算建立自己的資料庫存取提供者，則可以完全忽略這些實作細節。對於大多數的開發任務，只需理解以下幾點就足夠了：

1. 您需要一個對應到資料庫資料表的物件（POCO 類別）。
1. 所有與資料表資料相關的操作皆透過 `IRepository<TEntity>` 介面執行。您甚至不需要擔心將其放入 IoC，因為它已透過呼叫對應的工廠方法進行註冊。
1. 您需要控制資料庫中資料表的建立。

為了解決最後一個問題，我們需要處理該組合中的第二個框架，即 FluentMigrator。

## FluentMigrator

> [!NOTE]
> Fluent Migrator 是一個 .NET 的遷移框架，與 Ruby on Rails Migrations 非常相似。*遷移* 是變更資料庫綱要 (Schema) 的一種結構化方式，它取代了大量需要由每位參與開發人員手動執行的 SQL 指令稿。遷移解決了為多種資料庫（例如開發人員的本機資料庫、測試資料庫與正式環境資料庫）演進資料庫綱要的問題。資料庫綱要的變更是在以 C# 編寫的類別中描述的。這些類別可以被納入版本控制系統中。

有關新增實體的詳細規劃請參見以下文章：[具備資料存取功能的外掛](xref:zh-Hant/developer/plugins/how-to-write-plugin-4.70)。因此，我們在此僅討論一般的理論觀點：

1. 遷移在 nopCommerce 程式碼層級本身即受到支援。
1. 您可以建立任何繼承自抽象類別 **MigrationBase** 的遷移。
1. 為了簡化遷移的版本控制，我們在程式碼中新增了繼承自 **MigrationAttribute** 的 **NopMigrationAttribute** 屬性。現在，您只需指定建立遷移的日期與時間，而不需要使用傳統的長數字。
1. 我們也新增了 **SkipMigrationOnUpdateAttribute** 屬性，用來指示在更新過程中是否應跳過此遷移。
1. 您可以透過兩種方式在資料庫中建立資料表：
    * 在遷移類別的 **Up** 方法中使用 **Create.Table** 方法，並使用擴充方法指定所有細節。
    * 在遷移類別的 **Up** 方法中使用 **IMigrationManager.BuildTable<T\>** 方法，並在需要時透過實作 **IEntityBuilder** 與 **INameCompatibility** 介面來指定所有細節（在 nopCommerce 中我們使用此種方式）。

> [!IMPORTANT]
>
> 為了執行新的遷移，您必須提高 plugin.json 檔案中的版本號。