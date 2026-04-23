---
標題: 開發環境系統需求
uid: zh-Hant/developer/tutorials/system-requirements-for-developing
作者: nop.sea
貢獻者: git.RomanovM, git.DmitriyKulagin, git.skoshelev
---

# 開發環境系統需求

## 作業系統

### Windows

| 作業系統 | 版本 |
| ----------------- | ------------- |
| Windows 11 | 25H2, 24H2 (IoT), 24H2 (E), 24H2, 23H2 |
| Windows 10 Client | 21H2 (E), 21H2 (IoT), 1809 (E), 1607 (E) |
| Windows Server | 2025, 23H2, 2022, 2019, 2016 |
| Nano Server | 2025, 2022, 2019 |

### Linux

| 作業系統 | 版本 |
| ---------------------------- | ------------------- |
| Red Hat Enterprise Linux | 10, 9, 8 |
| Fedora | 42, 41 |
| Debian | 13, 12 |
| Ubuntu | 25.10, 24.04, 22.04 |
| openSUSE Leap | 16.0, 15.6 |
| SUSE Enterprise Linux | 16.0, 15.6 |
| Alpine Linux | 3.22, 3.21, 3.20 |

其他發行版本採盡力支援原則，請參閱 [.NET Support and Compatibility for Linux Distributions](https://github.com/dotnet/core/blob/main/linux.md)。

### Apple

| 作業系統 | 版本 |
| -------- | ---------- |
| macOS | 26, 15, 14 |

> [!NOTE]
>
> 查詢受支援作業系統的完整清單 [here](https://github.com/dotnet/core/blob/main/release-notes/9.0/supported-os.md)。
>
> [!IMPORTANT]
>
> 自 .NET 7.0 起，不再支援 **Windows Client 7 SP1, 8.1** 作業系統。
>
> 如需更多關於受支援作業系統版本的資訊，請瀏覽 [此頁面](https://github.com/dotnet/core/blob/main/release-notes/7.0/supported-os.md)。

## 受支援的瀏覽器

* Microsoft Edge。Microsoft Internet Explorer 9 以上版本（IE6 和 IE7 在 3.60 以前的版本受支援，IE8 在 4.10 以前的版本受支援）
* Mozilla Firefox 2.0 以上版本
* Google Chrome 1.x 以上版本
* Apple Safari 12.x 以上版本

## 開發所需工具

由於 nopCommerce 是基於 Microsoft 的 .NET 9，在開始開發之前，我們需要安裝一些工具。

### .NET 9 runtime 與 .NET 9 SDK

由於 nopCommerce 4.90 是基於 .NET 9，我們在開始開發 nopCommerce 之前，必須先安裝 [.NET 9 runtime](https://dotnet.microsoft.com/en-us/download/dotnet/thank-you/runtime-aspnetcore-9.0.0-windows-x64-installer) 和 [.NET 9 SDK](https://dotnet.microsoft.com/en-us/download/dotnet/thank-you/sdk-9.0.100-windows-x64-installer)。

### Visual Studio 2022 或以上版本 / Visual Studio Code

我們知道 nopCommerce 是基於「Microsoft .NET 9」，而 *Visual Studio IDE* 是開發 .NET 應用程式的最佳選擇。由於 .NET Core 是跨平台的，我們可以在任何平台上開發與部署 .NET 應用程式。但在 Windows 或其他平台上進行開發時，也可以使用 *Visual Studio Code* 作為 *Visual Studio* 的替代方案。

> [!NOTE]
>
> 如果您使用 *Visual Studio Code*，則需要安裝 **C# for Visual Studio Code (powered by OmniSharp)** 延伸模組。

### Microsoft SQL Server 2012 或以上版本 / MySql Server 5.7 或以上版本 / PostgreSQL 9.2 或以上版本

從 4.30 版本開始，nopCommerce 使用 *Linq2DB* 作為 ORM 框架。*Linq2DB* 是一個物件關聯映射器（ORM），它使 .NET 開發人員能夠使用 .NET 物件來處理資料庫。它可以將 .NET 物件對應到多種不同的資料庫提供者。您可以選擇使用 MS SQL Server、MySql Server 或 PostgreSQL。

> [!NOTE]
>
> 如需更多關於受支援資料庫的資訊，請瀏覽 [此頁面](https://linq2db.github.io/articles/general/databases.html)。

### Internet Information Service (IIS) 7.0 或以上版本

託管 nopCommerce 應用程式/專案的一個選項是 IIS，這是一項在 Windows 上託管網頁應用程式的 Microsoft 技術。然而，nopCommerce 也可以在不支援 IIS 的 Linux 和 MacOS 上執行。在這種情況下，您可以使用 Apache 或 Nginx 等替代工具在 Linux 伺服器上託管您的應用程式。