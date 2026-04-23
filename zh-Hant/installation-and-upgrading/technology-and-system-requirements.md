---
標題: 技術與系統需求
uid: zh-Hant/installation-and-upgrading/technology-and-system-requirements
作者: git.AndreiMaz
貢獻者: git.IvanIvanIvanov, git.rajupaladiya, git.exileDev, git.DmitriyKulagin, git.skoshelev
---

# 技術與系統需求

為了執行 nopCommerce，您的伺服器或電腦需要安裝下列軟體與環境：

## 支援的作業系統

* Windows
  * Windows 10 Client (1607 或以上版本)
  * Windows 11 (23H2 或以上版本)
  * Windows Server 2016 或以上版本

* Linux
  * Red Hat Enterprise Linux 8 或以上版本
  * Fedora 41 或以上版本
  * Debian 12 或以上版本
  * Ubuntu 22.04 或以上版本
  * openSUSE Leap 15.6 或以上版本
  * SUSE Enterprise Linux 15.6 或以上版本
  * Alpine Linux 3.20 或以上版本

* macOS
  * macOS 14.0 或以上版本

> [!NOTE]
>
> 請點擊此處查看完整的 [支援作業系統列表](https://github.com/dotnet/core/blob/main/release-notes/9.0/supported-os.md)。

## 支援的網頁伺服器

* Internet Information Service (IIS) 7.0 或更新版本
* 針對 nopCommerce 4.90：安裝 .NET 9 runtime ([下載](https://dotnet.microsoft.com/en-us/download/dotnet/thank-you/runtime-aspnetcore-9.0.9-windows-hosting-bundle-installer))。
* 針對 nopCommerce 4.80：安裝 .NET 9 runtime ([下載](https://dotnet.microsoft.com/en-us/download/dotnet/thank-you/runtime-aspnetcore-9.0.0-windows-hosting-bundle-installer))。
* 針對 nopCommerce 4.70：安裝 .NET 8 runtime ([下載](https://dotnet.microsoft.com/en-us/download/dotnet/thank-you/runtime-aspnetcore-8.0.4-windows-hosting-bundle-installer))。
* 針對 nopCommerce 4.60：安裝 .NET 7 runtime ([下載](https://dotnet.microsoft.com/en-us/download/dotnet/thank-you/runtime-aspnetcore-7.0.5-windows-hosting-bundle-installer))。
* 針對 nopCommerce 4.50：安裝 .NET 6 runtime ([下載](https://dotnet.microsoft.com/en-us/download/dotnet/thank-you/runtime-aspnetcore-6.0.1-windows-hosting-bundle-installer))。
* 針對 nopCommerce 4.40：安裝 .NET 5 runtime ([下載](https://dotnet.microsoft.com/download/dotnet/thank-you/runtime-aspnetcore-5.0.3-windows-hosting-bundle-installer))。
* 針對 nopCommerce 4.30：安裝 .NET Core 3.1 runtime ([下載](https://dotnet.microsoft.com/download/dotnet-core/thank-you/runtime-aspnetcore-3.1.3-windows-hosting-bundle-installer))。
* 針對 nopCommerce 4.20：安裝 .NET Core 2.2 runtime ([下載](https://dotnet.microsoft.com/download))。
* 針對 nopCommerce 4.10：安裝 .NET Core 2.1 runtime ([下載](https://dotnet.microsoft.com/download))。
* 針對 nopCommerce 4.00：安裝 .NET Core Window Server hosting runtime ([下載](https://dotnet.microsoft.com/download))
* 針對 nopCommerce 3.90 或更舊版本：ASP.NET 4.5 (MVC 5.0) 與 Microsoft .NET Framework 4.5.1 或更新版本

## 支援的資料庫

* MS SQL Server 2012 或以上版本
* MySql Server 5.7 或以上版本（從 nopCommerce 4.30 開始）
* PostgreSQL 9.5 或以上版本（從 nopCommerce 4.40 開始）

## 支援的瀏覽器

* Microsoft Edge。Microsoft Internet Explorer 9 以上版本（IE6 和 IE7 在 3.60 以前的版本受支援，IE8 在 4.10 以前的版本受支援）
* Mozilla Firefox 2.0 以上版本
* Google Chrome 1.x 以上版本
* Apple Safari 12.x 以上版本

**針對 nopCommerce 4.90 或以上版本：需使用 MS Visual Studio 2022（17.14 或以上版本）。且別忘了安裝 .NET 9 SDK ([下載](https://dotnet.microsoft.com/en-us/download/dotnet/thank-you/sdk-9.0.305-windows-x64-installer))。此為想要編輯原始程式碼的開發者所必需。**

*針對 nopCommerce 4.80 或以上版本：需使用 MS Visual Studio 2022（17.12 或以上版本）。且別忘了安裝 .NET 9 SDK ([下載](https://dotnet.microsoft.com/en-us/download/dotnet/thank-you/sdk-9.0.100-windows-x64-installer))。此為想要編輯原始程式碼的開發者所必需。*

*針對 nopCommerce 4.70 或以上版本：需使用 MS Visual Studio 2022（17.9 或以上版本）。且別忘了安裝 .NET 8 SDK ([下載](https://dotnet.microsoft.com/en-us/download/dotnet/thank-you/sdk-8.0.204-windows-x64-installer))。此為想要編輯原始程式碼的開發者所必需。*

*針對 nopCommerce 4.60 或以上版本：需使用 MS Visual Studio 2022（17.5 或以上版本）。且別忘了安裝 .NET 7 SDK ([下載](https://dotnet.microsoft.com/en-us/download/dotnet/thank-you/sdk-7.0.302-windows-x64-installer))。此為想要編輯原始程式碼的開發者所必需。*

*針對 nopCommerce 4.60 或以上版本：需使用 MS Visual Studio 2022（17.5 或以上版本）。且別忘了安裝 .NET 7 SDK ([下載](https://dotnet.microsoft.com/en-us/download/dotnet/thank-you/sdk-7.0.302-windows-x64-installer))。此為想要編輯原始程式碼的開發者所必需。*

*針對 nopCommerce 4.50 版本：需使用 MS Visual Studio 2022（17.0 或以上版本）。且別忘了安裝 .NET 6 SDK ([下載](https://dotnet.microsoft.com/en-us/download/dotnet/thank-you/sdk-6.0.406-windows-x64-installer))。此為想要編輯原始程式碼的開發者所必需。*

*針對 nopCommerce 4.40 或以上版本：需使用 MS Visual Studio 2019（16.9 或以上版本）。且別忘了安裝 .NET 5 SDK ([下載](https://dotnet.microsoft.com/download/dotnet/thank-you/sdk-5.0.408-windows-x64-installer))。此為想要編輯原始程式碼的開發者所必需。*

*針對 nopCommerce 4.30 版本：需使用 MS Visual Studio 2019（16.3 或以上版本）。且別忘了安裝 .NET Core SDK ([下載](https://dotnet.microsoft.com/download/dotnet-core/thank-you/sdk-3.1.426-windows-x64-installer))。此為想要編輯原始程式碼的開發者所必需。*

*針對 nopCommerce 4.20 或以上版本：需使用 MS Visual Studio 2017（15.9 或以上版本）。且別忘了安裝 .NET Core SDK ([下載](https://dotnet.microsoft.com/download))。此為想要編輯原始程式碼的開發者所必需。*

*針對 nopCommerce 4.10 版本：需使用 MS Visual Studio 2017（15.7 或以上版本）。且別忘了安裝 .NET Core SDK ([下載](https://dotnet.microsoft.com/download))。此為想要編輯原始程式碼的開發者所必需。*

*針對 nopCommerce 4.00 或以下版本：需使用 MS Visual Studio 2017（15.3 或以上版本）。且別忘了安裝 .NET Core SDK ([下載](https://dotnet.microsoft.com/download))。此為想要編輯原始程式碼的開發者所必需。*

> [!NOTE]
> 如果您要在 Windows 上安裝 nopCommerce 並打算使用具有 SSL 的多重商店功能，則需要 Windows Server 2012 與 IIS 8，因為它支援 SNI（伺服器名稱指示，Server Name Indication）。