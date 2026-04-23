---
標題: 實體事件系統
uid: zh-Hant/developer/design/entity-events-system
作者: git.nopsg
貢獻者: git.nopsg, git.DmitriyKulagin
---

# 實體事件系統

## 概覽

nopCommerce 實作了事件驅動架構，允許開發人員在執行某些動作或發生事件時，訂閱或使用由事件發布者或事件來源廣播的事件；同時也讓我們能在觸發特定事件時，執行特定的商業邏輯。在 nopCommerce 中，我們可以訂閱或監聽由系統事件所發布的各種事件，甚至可以撰寫邏輯來發出（發布）一個事件，隨後再由其他程式碼進行監聽或訂閱。舉例來說，假設我們想要將顧客詳細資料同步到其他外部系統，那麼我們可以在有新顧客註冊到我們的商店或顧客變更其個人資料時觸發一個事件。我們可以監聽該事件並執行商業邏輯，該邏輯會取出新建立的顧客資料，並將其發送到外部服務進行同步。最棒的是，我們可以在不修改 nopCommerce 原始程式碼的情況下完成這一切。

開發人員可以選擇發布事件或使用（消費）事件：

- 若要發布事件，開發人員需要取得 **IEventPublisher** 的實例，並使用適當的事件資料呼叫 **Publish** 方法。

- 若要監聽事件，開發人員需要為泛型 **IConsumer** 介面建立一個新的實作。一旦建立了新的消費者實作，nopCommerce 就會使用 Reflection（反射）來尋找並註冊該實作以進行事件處理。

有三個用於資料修改事件的事件發布者擴充方法，分別名為 `EntityInsertedAsync`、`EntityUpdatedAsync` 和 `EntityDeletedAsync`，它們與繼承自 **BaseEntity** 的 **IEventPublisher** 介面搭配使用，負責分別廣播實體的插入、更新與刪除事件。

## EntityInsertedAsync

此擴充方法將型別為 `BaseEntity` 的模型實體作為參數。當新增資料時，此擴充方法會用於發佈/廣播型別為 `BaseEntity` 的實體已插入事件。接著，此擴充方法會呼叫 `EntityInsertedEvent` 泛型類別的參數化建構函式，並透過其 `Entity` 屬性公開已插入的實體。開發者可以透過實作型別為 `EntityInsertedEvent`（泛型型別為我們剛插入的實體）的 `IConsumer` 介面來訂閱/處理此事件，例如：`IConsumer<EntyInsertedEvent<BaseEntity>>`。在此處，`BaseEntity` 可以是任何繼承自 `BaseEntity` 類別的模型類別。

### EntityInserted 事件的 Publisher 實作

```cs
public class MyFirstPublisherClass
{
    IEventPublisher _eventPublisher;
    public MyFirstPublisherClass(IEventPublisher eventPublisher)
    {
        this._eventPublisher = eventPublisher;
    }

    public async Task MyFirstProductInsertMethod(Product product)
    {
        //Logic to insert goes here
        await _eventPublisher.EntityInsertedAsync(product);
    }
}
```

在上述範例中，我們透過建構函式相依性注入機制，注入 `IEventPublisher` 介面以取得 EventPublisher 類別的實例。在 `MyFirstProductInsertMethod` 中，完成商品插入邏輯後，我們呼叫了 `EntityInserted` 方法，並傳入泛型型別 `Product`（該型別需繼承自 BaseEntity 類別）以及新建立的商品物件作為參數。當呼叫此擴充方法後，它會針對商品型別廣播「實體已插入（entity inserted）」事件，此時任何訂閱或監聽此事件的物件都將接收到此商品物件作為事件參數。接著，讓我們看看如何使用此事件。

### EntityInserted 事件的消費者實作

```cs
public class MyFirstConsumerClass : IConsumer<EntityInsertedEvent<Product>>
{
    public async Task HandleEventAsync(EntityInsertedEvent<Product> insertEvent)
    {
        //you can access entity using insertEvent.Entity
        var insertedEntity = insertEvent.Entity;

        //Here goes the business logic you want to perform...

    }
}
```

在這裡，我們建立了一個繼承自 `IConsumer<EntityInsertedEvent<Product>>` 的類別。`IConsumer` 介面僅有一個需要實作的方法，即 `HandleEvent` 方法。現在，每當觸發型別為 Product 的 EntityInserted 事件時，此 `HandleEvent` 方法就會被呼叫，並傳入型別為產品實體物件的 `EntityInsertedEvent`。在此類別內部，我們可以執行後續處理該資料所需的商業邏輯。

## EntityUpdatedAsync

此 `IEventPublisher` 介面的擴充方法與 EntityInserted 的實作方式相同。此擴充方法同樣接收 `BaseEntity` 類型的模型實體作為引數/參數。此擴充方法用於在現有實體更新時，發布/廣播 `BaseEntity` 類型的實體更新事件。此擴充方法會呼叫 `EntityUpdatedEvent` 泛型類別的參數化建構函式，並透過其 Entity 屬性公開更新後的實體。開發者接著可以透過實作 `IConsumer` 介面（類型為 `EntityUpdatedEvent`，且泛型參數為我們剛才更新的 `Entity` 類型）來訂閱/處理此事件，例如：`IConsumer<EntityUpdatedEvent<BaseEntity>>`。

### EntityUpdated 事件的發佈者實作

```cs
public class MyFirstPublisherClass
{
    IEventPublisher _eventPublisher;
    public MyFirstPublisherClass(IEventPublisher eventPublisher)
    {
        this._eventPublisher = eventPublisher;
    }

    public async Task MyFirstProductUpdateMethod(Product product)
    {
        //Logic to insert goes here
        await _eventPublisher.EntityUpdatedAsync(product);
    }
}
```

此類別的實作方式與 `EntityInserted` 中的範例大致相同。在此處的 `MyFirstProductInsertMethod` 中，在完成更新商品的邏輯後，我們呼叫了 `EntityUpdatedAsync` 方法，並將最近更新的商品物件作為參數傳入該泛型型別。現在，在呼叫此擴充方法後，它將會廣播商品類型的實體更新事件，而任何訂閱或監聽此事件的物件都將接收到此商品物件作為事件參數。現在讓我們看看如何使用此事件。

### EntityUpdated 事件的消費者實作

```cs
public class MyFirstConsumerClass : IConsumer<EntityUpdatedEvent<Product>>
{
    public async Task HandleEventAsync(EntityUpdatedEvent<Product> updateEvent)
    {
        //you can access entity using updateEvent.Entity
        var updatedEntity = updateEvent.Entity;

        //Here goes the business logic you want to perform...

    }
}
```

同樣地，這與實體插入（entity inserted）事件的消費者類別是一樣的。在此我們建立一個繼承自 `IConsumer<EntityUpdatedEvent<Product>>` 的類別。現在，每當 `Product` 類型的 `EntityUpdated` 事件被觸發時，該類別的 `HandleEvent` 方法就會被呼叫，並接收 `EntityUpdatedEvent` 類型的商品實體物件作為參數。在此類別內部，我們可以執行後續資料處理所需的商業邏輯。

## EntityDeletedAsync

實作此擴充方法的邏輯與 `IEventPublisher` 的 `EntityInsertedAsync` 及 `EntityUpdatedAsync` 擴充方法相同。此擴充方法同樣接收 `BaseEntity` 類型的模型實體作為參數。當現有實體被刪除時，此擴充方法會用於發布或廣播 `BaseEntity` 的實體刪除事件。此擴充方法會呼叫 `EntityDeletedEvent` 泛型類別的建構函式，並透過其 Entity 屬性公開被刪除的實體。開發者接著可以透過實作 `IConsumer<EntityDeletedEvent<BaseEntity>>` 來訂閱或處理此事件。

### 實體刪除 (EntityDeleted) 事件的發佈者實作

```cs
public class MyFirstPublisherClass
{
    IEventPublisher _eventPublisher;
    public ExamplePublisherClass(IEventPublisher eventPublisher)
    {
        this._eventPublisher = eventPublisher;
    }

    public async Task MyFirstProductDeleteMethod(Product product)
    {
        //Logic to insert goes here
        await _eventPublisher.EntityDeletedAsync(product);
    }
}
```

此類別的實作方式與上述範例相同。在 `MyFirstProductDeleteMethod` 中，於完成刪除商品的邏輯後，我們呼叫了 `EntityDeleted` 方法，並將剛剛刪除的商品物件作為參數傳入。現在，當觸發此擴充方法時，它將會針對商品類型廣播實體刪除事件，而任何正在訂閱/監聽此事件的處理器都將接收到該商品物件作為事件參數。接下來，讓我們看看如何處理（消費）此事件。

### EntityDeleted 事件的消費者實作

```cs
public class MyFirstConsumerClass : IConsumer<EntityDeletedEvent<Product>>
{
    public async Task HandleEventAsync(EntityDeletedEvent<Product> deleteEvent)
    {
        //you can access entity using deleteEvent.Entity
        var updatedEntity = deleteEvent.Entity;

        //Here goes the business logic you want to perform...
    }
}
```

這同樣與 entity inserted（實體新增）或 entity updated（實體更新）事件的消費者類別相同。在此，我們建立一個繼承自 `IConsumer<EntityDeletedEvent<Product>>` 的類別。

現在，每當觸發 `Product` 型別的 `EntityDeleted` 事件時，此類別的 `HandleEvent` 方法就會被呼叫，並傳入型別為 `EntityDeletedEvent` 的商品實體物件作為參數。我們可以在這個類別中執行我們的商業邏輯，以進行後續的資料處理。