---
標題: 公開與處理事件
uid: zh-Hant/developer/tutorials/events
作者: git.AndreiMaz
貢獻者: git.exileDev, git.DmitriyKulagin
---

# 公開與處理事件

事件是廣播給感興趣對象的通知。當發生資料變更（如新增、更新和刪除）時，系統會觸發事件。nopCommerce 允許開發者「監聽」他們可能感興趣的事件。開發者處理事件的方式主要有兩種：一種是公開事件供監聽者使用，另一種則是訂閱其他開發者以程式方式公開的事件。

1. 若要公開事件，開發者需要取得 `IEventPublisher` 的實例，並呼叫 `PublishAsync` 方法，同時傳入適當的事件資料。

   ```csharp
   await _eventPublisher.PublishAsync(new OrderPlacedEvent(order));
   ```

   該事件類別看起來像這樣：

   ```csharp
   public class OrderPlacedEvent
   {
       public OrderPlacedEvent(Order order)
       {
           Order = order;
       }
       public Order Order { get; }
   }
   ```

1. 若要監聽事件，開發者需要建立泛型 `IConsumer` 介面的新實作。一旦建立了新的消費者實作，nopCommerce 就會使用反射機制來尋找並註冊該實作，以便進行事件處理。

   ```csharp
   public class EventConsumer : IConsumer<OrderPlacedEvent>
   {
       public async Task HandleEventAsync(OrderPlacedEvent eventMessage)
       {
            if (eventMessage?.Order != null)
            {
                //do something
            }
       }
   }
   ```