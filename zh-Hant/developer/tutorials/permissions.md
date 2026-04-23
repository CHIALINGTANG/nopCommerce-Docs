---
標題: 擴充權限
uid: zh-Hant/developer/tutorials/permissions
作者: git.DmitriyKulagin
貢獻者: git.DmitriyKulagin
---

# 擴充權限

**權限 (Permissions)** 是應用程式資源安全的重要元素。透過它們，您可以限制對應用程式特定部分的存取，或者反過來，提供讀取或變更特定區域的必要權利。

所有用於控制範圍的可用權限皆位於「存取控制清單 (ACL)」區段中。

與應用程式中的任何其他變更一樣，建議透過外掛來進行。因此，讓我們來看看如何透過抽象外掛來新增權限。本教學旨在向您展示如何管理權限，若要了解如何建立外掛本身，請參閱 [此頁面](xref:zh-Hant/developer/plugins/index)。

## 在 nopCommerce 4.80 及以上版本中新增自訂權限

首先，讓我們建立一個類別來描述新的權限，這就是權限提供者（permission provider）。此類別必須實作 `IPermissionConfigManager` 介面。

```csharp
public partial class WebApiBackendPermissionConfigManager : IPermissionConfigManager
{
    public const string ACCESS_WEB_API = "AccessWebApi";

    /// <summary>
    /// Gets all permission configurations
    /// </summary>
    public IList<PermissionConfig> AllConfigs =>
        new List<PermissionConfig>
        {
            new("Access Web API Backend", ACCESS_WEB_API , nameof(StandardPermission.System), NopCustomerDefaults.AdministratorsRoleName)
        };
}
```

指定的記錄將會安裝到資料庫中，無需任何額外操作。

在此範例中，權限僅設定給 *系統管理員角色（administrator role）*，但如有需要，可以擴充此清單。

別忘了在移除外掛時，也要在 `UninstallAsync()` 方法中一併移除該權限：

```csharp
//delete permission
var permissionRecord = (await _permissionService.GetAllPermissionRecordsAsync())
    .FirstOrDefault(x => x.SystemName == WebApiBackendPermissionConfigManager.ACCESS_WEB_API);

await _permissionService.DeletePermissionRecordAsync(permissionRecord);
```

現在，在任何需要檢查使用者存取特定資源是否合法的程式碼中，我們都需要呼叫 `IPermissionService` 服務的 `AuthorizeAsync()` 方法並傳入該權限：

```csharp
 //check whether current customer has access to Web API
 if (await _permissionService.AuthorizeAsync(WebApiBackendPermissionConfigManager.ACCESS_WEB_API))
     return;
```

另一種將權限作為控制器動作篩選器（controller action filter）屬性的使用方式如下：

```csharp
[CheckPermission(WebApiBackendPermissionConfigManager.ACCESS_WEB_API)]
public virtual async Task<IActionResult> Configure()
{
    ...
}
```

基本上就是這樣，安裝外掛後，您可以在 **ACL** 區段中看到您新增的權限。

> [!NOTE]
>
> 如果您想直接使用預設權限，它們都列在 `StandardPermission` 類別中（位於 `Nop.Services.Security` 命名空間）。
> 例如：
>
> ```csharp
> [CheckPermission(StandardPermission.Configuration.MANAGE_SETTINGS)]
> ```

## 在 nopCommerce 4.70 及更早版本中新增自訂權限

首先，讓我們建立一個描述新權限的類別，這將會是權限提供者。此類別必須實作 `IPermissionProvider` 介面。

```csharp
public partial class WebApiBackendPermissionProvider : IPermissionProvider
{
    public static readonly PermissionRecord AccessWebApiBackend = new ()
    {
        Name = "Access Web API Backend", SystemName = "AccessWebApi", Category= "Standard"
    };
    /// <summary>
    /// Get permissions
    /// </summary>
    /// <returns>Permissions</returns>
    public virtual IEnumerable<PermissionRecord> GetPermissions()
    {
        return new[]
        {
            AccessWebApiBackend
        };
    }
    /// <summary>
    /// Get default permissions
    /// </summary>
    /// <returns>Permissions</returns>
    public virtual HashSet<(string systemRoleName, PermissionRecord[]permissions)> GetDefaultPermissions()
    {
        return new () { (NopCustomerDefaults.AdministratorsRoleName, new [] { AccessWebApiBackend }) };
    }
}
```

在此範例中，該權限僅針對 *管理員角色 (administrator role)* 進行設定，但若有需要，此清單可以擴充。

現在，我們將在外掛安裝時新增此權限。這必須在 `InstallAsync()` 方法中完成：

```csharp
//add permission
await _permissionService.InstallPermissionsAsync(new WebApiBackendPermissionProvider());
```

別忘了在移除外掛時，也要在 `UninstallAsync()` 方法中移除該權限：

```csharp
//delete permission
var permissionRecord = (await _permissionService.GetAllPermissionRecordsAsync())
    .FirstOrDefault(x => x.SystemName == WebApiBackendPermissionProvider.AccessWebApiBackend.SystemName);
var listMappingCustomerRolePermissionRecord = await _permissionService.GetMappingByPermissionRecordIdAsync(permissionRecord.Id);
foreach (var mappingCustomerPermissionRecord in listMappingCustomerRolePermissionRecord)
    await _permissionService.DeletePermissionRecordCustomerRoleMappingAsync(
        mappingCustomerPermissionRecord.PermissionRecordId,
        mappingCustomerPermissionRecord.CustomerRoleId);

await _permissionService.DeletePermissionRecordAsync(permissionRecord);
```

現在，在任何需要檢查使用者是否具有存取特定資源權限的地方，我們都需要呼叫 `IPermissionService` 服務的 `AuthorizeAsync()` 方法，並傳入該權限：

```csharp
//check whether current customer has access to Web API
if (await _permissionService.AuthorizeAsync(WebApiBackendPermissionProvider.AccessWebApiBackend))
    return;
```

這基本上就是所有步驟了，安裝外掛後，您可以在 **ACL** 區段中看到您新增的權限。

## 參閱

- [存取控制清單 (Access control list)](xref:zh-Hant/running-your-store/customer-management/access-control-list)