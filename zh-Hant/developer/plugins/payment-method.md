---
標題: 如何編寫自己的付款方式
uid: zh-Hant/developer/plugins/payment-method
作者: git.AndreiMaz
貢獻者: git.Sandeep911, git.exileDev, git.DmitriyKulagin
---

# 如何編寫自己的付款方式

在 nopCommerce 中，付款方式是以「外掛」的形式實作。我們建議您在開始編寫新的付款方式之前，先閱讀 [如何為 nopCommerce 4.90 編寫外掛](xref:zh-Hant/developer/plugins/how-to-write-plugin-4.90)。它會向您說明建立外掛所需的步驟。

實際上，付款方式是一個實作了 **`IPaymentMethod`** 介面（位於 *Nop.Services.Payments* 命名空間）的一般外掛。正如您所料，*IPaymentMethod* 介面是用於建立付款方式外掛的。它包含了一些專屬於付款方式的方法，例如 `ProcessPaymentAsync()` 或 `GetAdditionalHandlingFeeAsync()`。那麼，請將一個新的付款外掛專案（*類別庫*）加入到解決方案中，讓我們開始吧。

## 控制器、檢視、模型

您首先需要做的是建立一個控制器。此控制器負責回應針對 ASP.NET MVC 網站所提出的請求。

1. 在實作新的付款方式時，此控制器應繼承自特殊的 **BasePaymentController** 抽象類別。

1. 接著，實作用於外掛設定的 **Configure** 動作方法（由商店擁有者在管理後台操作）。此方法以及對應的檢視將定義商店擁有者如何在管理後台（*系統 → 設定 → 付款方式*）中查看設定選項。

## Public view component.GetPublicViewComponent

接著，您必須建立一個 View component 以便在外掛中顯示於前台商店。這個 View component 與對應的 view 將決定顧客在結帳期間如何查看付款資訊頁面。首先，讓我們建立一個 View component 類別。它應該放置在 *`/Components`* 資料夾中。請參考 *PayPalCommerce* 外掛的做法：

```csharp
public class PaymentInfoViewComponent : NopViewComponent
{
    public IViewComponentResult Invoke(string widgetZone, object additionalData)
    {
        return View("~/Plugins/Payments.PayPalCommerce/Views/Public/PaymentInfo.cshtml");
    }
}
```

**Invoke** 方法會從外掛的 */Views* 資料夾中回傳適當的 `PaymentInfo` view。請注意，我們是使用自訂的 `NopViewComponent` 類別作為基底類別，而不是使用現有的內建 `ViewComponent`。

接著，讓我們建立顯示付款資訊的 `PaymentInfo` view。在該處，我們只需呈現一段文字，告知顧客將被重新導向至付款頁面。但如有需要，也可以建立更複雜的 View component。例如，如果您想在付款資訊頁面上收集顧客資訊，可以參考 `PayPalDirect` 付款外掛的實作方式。

## 付款處理

現在您需要建立一個實作 **IPaymentMethod** 介面的類別。這個類別將負責與您的金流閘道（payment gateway）進行通訊的所有實際作業。當有人建立訂單時，系統將會呼叫您類別中的 `ProcessPayment` 或 `PostProcessPayment` 方法。以下是 **CheckMoneyOrderPaymentProcessor** 類別的定義方式（以 `CheckMoneyOrder` 付款方式為例）：

```csharp
public class CheckMoneyOrderPaymentProcessor : BasePlugin, IPaymentMethod
```

**IPaymentMethod** 介面包含數個必須實作的方法與屬性。

- **ValidatePaymentFormAsync** 用於在前台網站中驗證顧客的輸入。它會回傳一個警告清單（例如：顧客未輸入信用卡名稱）。如果您的付款方式不需要顧客輸入額外資訊，則 `ValidatePaymentFormAsync` 應回傳一個空清單：

    ```csharp
    public Task<IList<string>> ValidatePaymentFormAsync(IFormCollection form)
    {
        return Task.FromResult<IList<string>>(new List<string>());
    }
    ```

- **GetPaymentInfoAsync** 方法用於在前台網站中解析顧客的輸入，例如信用卡資訊。此方法會回傳一個 ProcessPaymentRequest 物件，其中包含解析後的顧客輸入資訊（例如信用卡資訊）。如果您的付款方式不需要顧客輸入額外資訊，則 `GetPaymentInfoAsync` 將會回傳一個空的 ProcessPaymentRequest 物件：

    ```csharp
    public Task<ProcessPaymentRequest> GetPaymentInfoAsync(IFormCollection form)
    {
        return Task.FromResult(new ProcessPaymentRequest());
    }
    ```

- **ProcessPaymentAsync**。此方法總是在顧客下訂單前立即被觸發。當您需要在訂單儲存至資料庫之前處理付款時，請使用此方法。例如，信用卡請款（capture）或授權（authorize）。通常當顧客不需要重新導向至第三方網站以完成付款，且所有付款都在您的網站上處理時（例如 *PayPalCommerce*），會使用此方法。

    ```csharp
    public Task<ProcessPaymentResult> ProcessPaymentAsync(ProcessPaymentRequest processPaymentRequest)
    {
        return Task.FromResult(new ProcessPaymentResult());
    }
    ```

- **PostProcessPaymentAsync**。此方法在顧客下訂單後立即被觸發。通常當您需要將顧客重新導向至第三方網站以完成付款時（例如 PayPal Standard），會使用此方法。
- **HidePaymentMethodAsync**。您可以在此處放入任何邏輯。例如，如果購物車內所有商品皆為可下載商品，則隱藏此付款方式；或者如果當前顧客來自特定國家/地區，則隱藏此付款方式。
- **GetAdditionalHandlingFeeAsync**。您可以回傳任何將會加總至訂單總額的額外處理費。
- **CaptureAsync**。部分金流閘道允許您在請款前先進行付款授權。這讓商店管理者能在付款完成前審核訂單詳情。在此情況下，您只需在上述的 **ProcessPaymentAsync** 或 **PostProcessPaymentAsync** 方法中授權付款，然後再進行請款。此時，管理後台的訂單詳情頁面將會顯示一個「請款（Capture）」按鈕。請注意，訂單必須已完成授權，且 **SupportCapture** 屬性應回傳 **`true`**。
- **RefundAsync**。此方法允許您進行退款。在此情況下，管理後台的訂單詳情頁面將會顯示一個「退款（Refund）」按鈕。請注意，訂單必須已付款，且 **SupportRefund** 或 **SupportPartiallyRefund** 屬性應回傳 **`true`**。
- **VoidAsync**。此方法允許您取消（void）已授權但未請款的付款。在此情況下，管理後台的訂單詳情頁面將會顯示一個「取消授權（Void）」按鈕。請注意，訂單必須已授權，且 **SupportVoid** 屬性應回傳 **`true`**。
- **ProcessRecurringPaymentAsync**。使用此方法處理週期性付款。
- **CancelRecurringPaymentAsync**。使用此方法取消週期性付款。
- **CanRePostProcessPaymentAsync**。通常此方法用於將顧客重新導向至第三方網站完成付款的情境。如果第三方付款失敗，此選項將允許顧客稍後嘗試重新處理訂單，而無需建立新訂單。**CanRePostProcessPaymentAsync** 應回傳 **`true`** 以啟用此功能。
- **GetConfigurationPageUrl**。如您所記得的，我們在先前的步驟中建立了一個控制器。此方法應回傳其 `Configure` 方法的 URL。例如：

    ```csharp
    public override string GetConfigurationPageUrl()
    {
        return $"{_webHelper.GetStoreLocation()}Admin/PaymentCheckMoneyOrder/Configure";
    }
    ```

- **GetPaymentMethodDescriptionAsync**。此方法取得將會顯示於前台網站結帳頁面上的付款方式描述。

    ```csharp
    public async Task<string> GetPaymentMethodDescriptionAsync()
    {
        return await _localizationService.GetResourceAsync("Plugins.Payment.CheckMoneyOrderPaymentMethodDescription");
    }
    ```

- **SupportCapture、SupportPartiallyRefund、SupportRefund、SupportVoid**。這些屬性指出是否支援付款方式的對應方法。
- **RecurringPaymentType**。此屬性指出是否支援週期性付款。
- **PaymentMethodType**。此屬性指出付款方式的類型。目前共有三種類型。**`Standard`** 用於顧客無需重新導向至第三方網站的付款方式。**`Redirection`** 用於顧客需被重新導向至第三方網站的付款方式。**`Button`** 則類似於 **`Redirection`** 付款方式，唯一的差別在於它會以按鈕形式顯示在購物車頁面上（例如 *Google Checkout*）。
- **SkipPaymentInfo**。指出我們是否應為此外掛顯示付款資訊頁面。

## 結論

希望這些說明能幫助您開始新增付款方式。