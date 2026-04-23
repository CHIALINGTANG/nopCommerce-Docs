---
標題: ITypeFinder 介面
uid: zh-Hant/developer/tutorials/type-finder
作者: git.skoshelev
貢獻者: git.mariannk, git.DmitriyKulagin
---

# ITypeFinder 介面

## ITypeFinder

這是一個非常簡單的介面，僅包含兩個方法（儘管其中一個有兩個多載）。您可以在下方查看該介面的定義：

  ```csharp
/// <summary>
/// Classes implementing this interface provide information about types 
/// to various services in the Nop engine.
/// </summary>
public interface ITypeFinder
{
    /// <param name="onlyConcreteClasses">A value indicating whether to find only concrete classes</param>
    IEnumerable<Type> FindClassesOfType<T>(bool onlyConcreteClasses = true);

    /// <param name="assignTypeFrom">Assign type from</param>
    /// <param name="onlyConcreteClasses">A value indicating whether to find only concrete classes</param>
    IEnumerable<Type> FindClassesOfType(Type assignTypeFrom, bool onlyConcreteClasses = true);

    /// <summary>
    /// Gets the assemblies related to the current implementation.
    /// </summary>
    IList<Assembly> GetAssemblies();
}
  ```

實作此介面的類別，其主要任務是在組件（assemblies）中搜尋指定類別或介面的型別。我們將透過 nopCommerce 原始碼中的範例來了解它在何處能派上用場。但首先，讓我們看看 ``GetAssemblies`` 方法。此方法的任務僅是傳回執行搜尋時所依據的組件清單。此清單如何形成，取決於該介面的具體實作方式。

## ITypeFinder 的預設實作

此介面的主要預設實作是 ``WebAppTypeFinder`` 類別。而 ``WebAppTypeFinder`` 僅稍微擴充了 ``AppDomainTypeFinder`` 類別，後者基本上完成了所有關於搜尋型別的工作。但我們使用衍生類別，是因為它將搜尋型別的範圍擴充到了 **\Bin** 目錄下的所有組件，而主類別僅處理當前應用程式域（application domain）中的組件。

在不深入探討 ``FindClassesOfType`` 方法的實作細節下（因為兩者最終都歸結為非常簡單的函式，其程式碼可在 [this link](https://github.com/nopSolutions/nopCommerce/blob/develop/src/Libraries/Nop.Core/Infrastructure/AppDomainTypeFinder.cs#L184) 找到），讓我們繼續探討此介面最重要的部分。

## 那麼，為什麼我們需要 ITypeFinder 介面？

此介面被用於 nopCommerce 運作方式中幾個非常重要的面向：

1. 搜尋組件以配置遷移機制（[遷移如何運作？](xref:zh-Hant/developer/tutorials/migrations)）
2. 搜尋網站正確啟動所需的特定介面類別，例如：
    * ``IStartupTask`` - 模組與外掛的初始初始化
    * ``INopStartup`` - 在應用程式啟動時設定服務與中介層
    * ``IOrderedMapperProfile`` - 建立 **AutoMapper** 設定
    * ``IEntityBuilder``, ``INameCompatibility`` - 為 **Linq2Db** 設定資料庫實體建構器，以確保資料表命名向後相容（[nopCommerce 資料存取層](xref:zh-Hant/developer/tutorials/source-code-organization#librariesnopdata)）
    * ``IRouteProvider`` - 註冊路由
    * ``IConsumer<T>`` - 為內部事件註冊處理常式，例如資料庫實體變更
    * ``IExternalAuthenticationRegistrar`` - 註冊並設定外部驗證方法
3. 即時搜尋合適的貨運追蹤器

## 結論

如您所見，該介面雖然小巧但非常有價值。如果沒有使用透過 `ITypeFinder` 介面方法所實作的這種方法，要在 nopCommerce 中實現如此靈活的模組化結構將會非常困難。