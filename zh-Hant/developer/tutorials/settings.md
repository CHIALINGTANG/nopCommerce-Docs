---
標題: 設定 API
uid: zh-Hant/developer/tutorials/settings
作者: git.AndreiMaz
貢獻者: git.exileDev, git.DmitriyKulagin
---

# 設定 API

如同任何其他網站平台，nopCommerce 擁有諸如「商店名稱」或「啟用單頁結帳」等設定。在 nopCommerce 中管理設定有兩種方式。

1. 您可以使用 **ISettingService** 實作中的 **GetSettingByKeyAsync** 和 **SetSettingAsync** 方法來載入及儲存個別設定。

```csharp
var setting = await GetSettingByKeyAsync<string>(key);
...
await _settingService.SetSettingAsync(key, value);
```

2. 在 nopCommerce 中處理設定的建議做法是建立一個新的 **ISettings** 介面實作。每個設定將由一個 C# 屬性來表示，開發者應在需要時透過建構函式注入設定類別。下方是一個設定類別的範例。

```csharp
public partial class MediaSettings : ISettings
    {
        public int AvatarPictureSize { get; set; }
        public int ProductThumbPictureSize { get; set; }
        public int ProductDetailsPictureSize { get; set; }
        public int ProductThumbPictureSizeOnProductDetailsPage { get; set; }
        public int AssociatedProductPictureSize { get; set; }
        public int CategoryThumbPictureSize { get; set; }
        public int ManufacturerThumbPictureSize { get; set; }
        public int VendorThumbPictureSize { get; set; }
        public int CartThumbPictureSize { get; set; }
        public int OrderThumbPictureSize { get; set; }
        public int MiniCartThumbPictureSize { get; set; }
        public int AutoCompleteSearchThumbPictureSize { get; set; }
        public int ImageSquarePictureSize { get; set; }
        public bool DefaultPictureZoomEnabled { get; set; }
        public bool AllowSVGUploads { get; set; }
        public int MaximumImageSize { get; set; }
        public int DefaultImageQuality { get; set; }
        public bool MultipleThumbDirectories { get; set; }
        public bool ImportProductImagesUsingHash { get; set; }
        public string AzureCacheControlHeader { get; set; }
        public bool UseAbsoluteImagePath { get; set; }
        public string VideoIframeAllow { get; set; }
        public int VideoIframeWidth { get; set; }
        public int VideoIframeHeight { get; set; }
        public int ProductDefaultImageId { get; set; }
    }
```