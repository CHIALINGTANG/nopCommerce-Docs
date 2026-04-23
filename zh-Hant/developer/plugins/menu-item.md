---
標題: 如何從外掛將選單項目新增至管理後台
uid: zh-Hant/developer/plugins/menu-item
作者: git.AndreiMaz
貢獻者: git.Sandeep911, git.DmitriyKulagin, git.exileDev
---

# 如何從外掛將選單項目新增至管理後台（適用於 4.80 以上版本）

若要新增選單項目，您應該使用 **AdminMenuCreatedEvent** 事件。您可以使用下列程式碼範例，將其新增至您的外掛 `*.cs` 檔案中。

此外，您也可以在此方法中加入任何安全性 (ACL) 邏輯。例如，驗證目前的顧客是否擁有「管理外掛」的權限。

```csharp
public class EventConsumer: IConsumer<AdminMenuCreatedEvent>
{
    private readonly IPermissionService _permissionService;

    public EventConsumer(IPermissionService permissionService)
    {
        _permissionService = permissionService;
    }

    public Task HandleEventAsync(AdminMenuCreatedEvent eventMessage)
    {
        if (!await _permissionService.AuthorizeAsync(StandardPermission.Configuration.MANAGE_PLUGINS))
            return;

        eventMessage.RootMenuItem.InsertBefore("Local plugins",
            new AdminMenuItem
            {
                SystemName = "YourCustomSystemName",
                Title = "Plugin Title",
                Url = eventMessage.GetMenuItemUrl("CONTROLLER_NAME", "ACTION_NAME"),
                IconClass = "far fa-dot-circle",
                Visible = true,
            });

        return Task.CompletedTask;
    }
}

```

> [!NOTE]
>
> 在 nopCommerce 4.70 及更早的版本中，管理選單是透過 *sitemap.config* 檔案所建立，該檔案位於 *~/Areas/Admin folder*。
>
> 若要執行相同動作，您可以使用下列範例程式碼，將其加入您的外掛 `*.cs` 檔案中。首先，請在您的外掛主類別中實作 *IAdminMenuPlugin* 介面。
>
>```csharp
> public class CustomPlugin : BasePlugin, IAdminMenuPlugin
> {
>
>    public Task ManageSiteMapAsync(SiteMapNode rootNode)
>    {
>        var menuItem = new SiteMapNode()
>        {
>            SystemName = "YourCustomSystemName",
>            Title = "Plugin Title",
>            ControllerName = "ControllerName",
>            ActionName = "List",
>            IconClass = "far fa-dot-circle",
>            Visible = true,
>            RouteValues = new RouteValueDictionary() { { "area", AreaNames.Admin } },
>        };
>        var pluginNode = rootNode.ChildNodes.FirstOrDefault(x => x.SystemName == "Third party plugins");
>        if(pluginNode != null)
>            pluginNode.ChildNodes.Add(menuItem);
>        else
>            rootNode.ChildNodes.Add(menuItem);
>
>        return Task.CompletedTask;
>    }
>}
>
> ```