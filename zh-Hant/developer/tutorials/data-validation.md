---
標題: 資料驗證
uid: zh-Hant/developer/tutorials/data-validation
作者: git.AndreiMaz
貢獻者: git.exileDev, git.DmitriyKulagin
---

# 資料驗證

資料驗證是確保程式在乾淨且正確的資料上運作的過程。大多數 .NET 開發者會使用 `Data Annotation Validators`，但 nopCommerce 使用的是 **`Fluent Validation`**。這是一個適用於 .NET 的輕量級驗證函式庫，它使用 Fluent Interface 和 Lambda 表達式來為您的業務物件建立驗證規則。若要在 nopCommerce 中為模型新增驗證，您需要執行以下步驟：

建立一個繼承自 `AbstractValidator` 類別的類別，並將所有必要的邏輯放入其中。

```csharp
public partial class AddressValidator : BaseNopValidator<AddressModel>
{
    public AddressValidator(ILocalizationService localizationService)
    {
        RuleFor(x => x.FirstName)
            .NotEmpty()
            .WithMessageAwait(localizationService.GetResourceAsync("AddressFields.FirstName.Required"));            
    }
}
```

當檢視模型（View model）被傳送到控制器（Controller）時，ASP.NET Core 將會執行相對應的驗證器。