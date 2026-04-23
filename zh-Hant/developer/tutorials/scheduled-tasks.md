---
標題: 排程任務
uid: zh-Hant/developer/tutorials/scheduled-tasks
作者: git.AndreiMaz
貢獻者: git.sinaislam, git.DmitriyKulagin, git.exileDev
---

# 排程任務

透過排程任務，您可以設定任務在特定週期執行。例如，nopCommerce 會定期傳送佇列中的郵件。建立新任務的基本步驟如下：

1. 定義一個實作 **IScheduleTask** 介面的類別。該介面僅有一個不帶參數的方法：**ExecuteAsync**。正如您所料，當任務需要執行時，就會呼叫此方法。

    ```csharp
    public partial class KeepAliveTask : IScheduleTask
    {
        private readonly StoreHttpClient _storeHttpClient;
        public KeepAliveTask(StoreHttpClient storeHttpClient)
        {
            _storeHttpClient = storeHttpClient;
        }
        public async System.Threading.Tasks.Task ExecuteAsync()
        {
            await _storeHttpClient.KeepAliveAsync();
        }
    }
    ```

1. 若要排程任務，開發者應在對應的資料庫表中插入一筆新的 **ScheduleTask** 記錄。您可以使用 **IScheduleTaskService** 來插入此類記錄。

    ```csharp
    await _scheduleTaskService.InsertTaskAsync(new ScheduleTask
    {
        Name = "Keep alive",
        Seconds = 300,
        Type = "Nop.Services.Common.KeepAliveTask, Nop.Services",
        Enabled = true,
        LastEnabledUtc = lastEnabledUtc,
        StopOnError = false
    });
    ```

> [!IMPORTANT]
> 當為新排程任務將記錄插入 **ScheduleTask** 資料庫表時，務必保持 **Type** 欄位的格式為 **Namespace.TaskClassName, AssemblyName**。

## 除錯

- 請確保您的商店擁有有效的 URL。
- 新增排程任務後，請重新啟動應用程式。