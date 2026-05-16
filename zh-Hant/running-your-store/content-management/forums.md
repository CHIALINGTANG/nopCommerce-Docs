---
標題: 論壇
uid: zh-Hant/running-your-store/content-management/forums
作者: git.AndreiMaz
貢獻者: git.DmitriyKulagin, git.exileDev, git.mariannk
---

# 論壇

論壇是一個線上討論網站，人們可以在其中透過發佈訊息的形式進行對話。一個論壇可以包含多個子論壇，每個子論壇擁有數個內容頁面。

> [!NOTE]
>
> 在 nopCommerce 中，論壇預設為停用。若要啟用論壇，請前往 **設定 → 設定 → 論壇設定** 並勾選 **論壇已啟用** 核取方塊。在公開商店的選單（預設佈景主題的頂部選單或頁尾）中應會顯示「論壇」連結。

> [!NOTE]
>
> 自 4.90 版本起，若有需要，您必須在啟用論壇後 [手動新增](xref:zh-Hant/running-your-store/content-management/menu) 選單或頁尾項目。

若要管理論壇群組與論壇（位於論壇群組內），請前往 **內容管理 → 論壇**。

![Manage forums](_static/forums/list.jpg)

## 新增論壇群組

點擊 **新增論壇群組** 按鈕。

![Add a new forum group](_static/forums/forums2.png)

- 定義新的論壇群組 **名稱**。
- 在 **顯示順序** 欄位中，輸入該論壇群組的顯示順序。數值 1 代表顯示在列表的最上方。

點擊 **儲存**。

## 新增論壇

![Add a new forum](_static/forums/forums3.png)

- 從 **Forum group** 下拉式選單中，選擇所需的論壇群組。
- 輸入新論壇的 **Name**。
- 輸入新論壇的 **Description**。
- 選擇論壇群組的 **Display order**。數值 1 代表位於列表的最上方。

點擊 **Save**。

若要查看論壇運作方式的範例，請前往 <http://www.nopcommerce.com/boards/>。

![nopCommerce forums](_static/forums/example.jpg)

## 論壇設定

若要存取論壇設定，請前往 **設定 → 設定 → 論壇設定**。此頁面提供兩種模式：*進階 (advanced)* 與 *基本 (basic)*。

此頁面支援多商店設定；這表示您可以為所有商店定義相同的設定，也可以讓各個商店的設定有所不同。如果您想管理特定商店的設定，請從多商店設定下拉式清單中選擇該商店的名稱，並勾選左側所需的核取方塊，以便為其設定自訂值。若需進一步了解，請參閱 [多商店 (Multi-store)](xref:zh-Hant/getting-started/advanced-configuration/multi-store)。

### 一般

![Common settings](_static/forums/common.jpg)

請在 *一般* 面板中定義以下論壇設定：

- 勾選 **論壇已啟用 (Forums enabled)** 核取方塊以啟用論壇。
- 勾選 **相對日期與時間格式 (Relative date and time formatting)** 核取方塊以啟用相對日期與時間顯示（例如：2 小時前、1 天前）。
- 若要讓顧客能夠指定個人簽名，請勾選 **簽名已啟用 (Signature enabled)**。
- 勾選 **顯示顧客貼文數 (Show customers post count)** 核取方塊，以啟用顯示該顧客建立的貼文總數。
- 從 **論壇編輯器 (Forum editor)** 下拉式清單中，選擇要使用的論壇編輯器類型：
  - 簡易文字方塊 (Simple textbox)。
  - BBCode 編輯器 (BBCode editor)。
  > [!NOTE]
  >
  > 不建議在正式營運環境中變更論壇編輯器類型。

### 權限

![Permissions settings](_static/forums/permissions.jpg)

在「權限」面板中定義以下論壇設定：

- **允許訪客建立貼文**。
- **允許訪客建立內容頁面**。
- **允許顧客編輯貼文**。
- **允許顧客刪除貼文**。
- **允許顧客管理論壇訂閱**。
- 勾選 **允許使用者對貼文投票** 核取方塊以啟用投票功能。
  - 若已啟用上述設定，**每日最高投票次數** 欄位可設定使用者每日可進行的投票數。
- 勾選 **允許私人訊息** 核取方塊以啟用私人訊息功能。若啟用，將顯示以下兩項設定：
  - 勾選 **顯示私人訊息提醒** 核取方塊，可在收到新私人訊息時啟用彈出式提醒視窗。
  - 若希望顧客在收到新私人訊息時透過電子郵件收到通知，請勾選 **通知私人訊息**。

### 頁面大小

![Page sizes settings](_static/forums/page-sizes.jpg)

在「頁面大小」面板中定義以下論壇設定：

- **主題頁面大小 (Topics page size)** — 論壇中主題的頁面大小，例如每頁顯示 '10' 個主題。
- **文章頁面大小 (Posts page size)** — 主題中文章的頁面大小，例如每頁顯示 '10' 篇文章。
- **搜尋結果頁面大小 (Search results page size)** — 搜尋結果的頁面大小，例如每頁顯示 '10' 個結果。
- **熱門討論頁面大小 (Active discussions page size)** – 熱門討論頁面的頁面大小，例如每頁顯示 '10' 個結果。

### RSS 摘要

![Feeds settings](_static/forums/feeds.jpg)

請在 *Feeds*（摘要）面板中定義以下論壇設定：

- 勾選 **Forum feeds enabled** 核取方塊以啟用各論壇的 RSS 摘要。
- 在 **Forum feed count** 欄位中，設定每個摘要所包含的內容頁面（Topic）數量。
- 勾選 **Active discussions feed enabled** 核取方塊以啟用熱門討論的 RSS 摘要。
- 在 **Active discussions feed count** 欄位中，設定「熱門討論」摘要中所包含的討論數量。

## 教學

- [在 nopCommerce 中管理論壇](https://www.youtube.com/watch?v=wW2QvC4WA_8)