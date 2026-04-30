---
標題: 論壇
uid: zh-Hant/running-your-store/content-management/forums
作者: git.AndreiMaz
貢獻者: git.DmitriyKulagin, git.exileDev, git.mariannk
---

# 論壇

論壇是一個線上討論網站，人們可以在其中透過發佈訊息的形式進行對話。一個論壇可能包含多個子論壇，每個子論壇又有若干個內容頁面。

> [!NOTE]
>
> 在 nopCommerce 中，論壇功能預設為關閉。若要啟用論壇，請前往 **設定 → 設定 → 論壇設定**，並勾選 **啟用論壇** 核取方塊。「論壇」連結應會顯示在前台網站的選單中（預設佈景主題為頂部選單或頁尾）。

> [!NOTE]
>
> 自 4.90 版本起，啟用論壇後，如有需要，您必須 [手動新增](xref:zh-Hant/running-your-store/content-management/menu) 選單或頁尾項目。

若要管理論壇群組與論壇（論壇群組內），請前往 **內容管理 → 論壇**。

![Manage forums](_static/forums/list.jpg)

## 新增論壇群組

點擊 **新增論壇群組** 按鈕。

![Add a new forum group](_static/forums/forums2.png)

- 定義新的論壇群組 **名稱**。
- 在 **顯示順序** 欄位中，輸入論壇群組的顯示順序。數值 1 代表列表的最上方。

點擊 **儲存**。

## 新增論壇

![Add a new forum](_static/forums/forums3.png)

- 從 **論壇群組** 下拉式選單中，選擇所需的論壇群組。
- 輸入新論壇的 **名稱**。
- 輸入新論壇的 **描述**。
- 選擇論壇群組的 **顯示順序**。數值 1 代表列表的最上方。

點擊 **儲存**。

若要查看論壇運作方式的範例，請前往 <http://www.nopcommerce.com/boards/>。

![nopCommerce forums](_static/forums/example.jpg)

## 論壇設定

若要存取論壇設定，請前往 **設定 → 設定 → 論壇設定**。此頁面提供兩種模式：*進階* 與 *基本*。

此頁面支援多商店設定；這意味著可以為所有商店定義相同的設定，或是針對不同的商店設定不同的值。如果您想要管理特定商店的設定，請從多商店設定下拉式清單中選擇該商店名稱，並勾選左側所需的核取方塊，以便為其設定自訂值。如需進一步詳情，請參閱 [多商店](xref:zh-Hant/getting-started/advanced-configuration/multi-store)。

### 一般

![Common settings](_static/forums/common.jpg)

請在 *一般* 面板中定義以下論壇設定：

- 勾選 **論壇已啟用** 核取方塊以啟用論壇。
- 勾選 **相對日期與時間格式** 核取方塊以啟用相對日期與時間顯示（例如：2 小時前、1 天前）。
- 您可以透過勾選 **啟用簽名** 來允許顧客設定個人簽名。
- 勾選 **顯示顧客發文數** 核取方塊以啟用顯示顧客建立的貼文數量。
- 從 **論壇編輯器** 下拉式清單中，選擇要使用的論壇編輯器類型：
  - 簡易文字方塊。
  - BBCode 編輯器。
  > [!NOTE]
  >
  > 不建議在正式環境中更改論壇編輯器類型。

### 權限

![Permissions settings](_static/forums/permissions.jpg)

在「權限」面板中定義以下論壇設定：

- **允許訪客建立貼文**。
- **允許訪客建立內容頁面**。
- **允許顧客編輯貼文**。
- **允許顧客刪除貼文**。
- **允許顧客管理論壇訂閱**。
- 勾選 **允許使用者對貼文進行投票** 核取方塊以啟用投票功能。
  - **每日最高投票數** 欄位用於設定若已啟用上述設定時，使用者每日可進行的投票次數。
- 勾選 **允許私人訊息** 核取方塊以啟用私人訊息功能。若啟用，將顯示以下兩項設定：
  - 勾選 **顯示私人訊息提醒** 核取方塊，以便在收到新私人訊息時顯示彈出式提醒視窗。
  - 若希望顧客在收到新私人訊息時透過電子郵件收到通知，請勾選 **通知私人訊息**。

### 頁面大小

![Page sizes settings](_static/forums/page-sizes.jpg)

在「頁面大小」面板中定義以下論壇設定：

- **主題頁面大小 (Topics page size)** — 論壇中主題的頁面大小，例如：每頁顯示 '10' 個主題。
- **文章頁面大小 (Posts page size)** — 主題中文章的頁面大小，例如：每頁顯示 '10' 篇文章。
- **搜尋結果頁面大小 (Search results page size)** — 搜尋結果的頁面大小，例如：每頁顯示 '10' 個結果。
- **熱門討論頁面大小 (Active discussions page size)** — 熱門討論頁面的頁面大小，例如：每頁顯示 '10' 個結果。

### Feeds

![Feeds settings](_static/forums/feeds.jpg)

在 *Feeds* 面板中定義下列論壇設定：

- 勾選 **Forum feeds enabled** 核取方塊以啟用各論壇的 RSS feeds。
- 在 **Forum feed count** 欄位中，設定每個 feed 所包含的內容頁面（topics）數量。
- 勾選 **Active discussions feed enabled** 核取方塊以啟用熱門討論內容的 RSS feeds。
- 在 **Active discussions feed count** 欄位中，設定「熱門討論」feed 所包含的討論數量。

## 教學課程

- [管理 nopCommerce 中的論壇](https://www.youtube.com/watch?v=wW2QvC4WA_8)