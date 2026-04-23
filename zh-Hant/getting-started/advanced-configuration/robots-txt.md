---
標題: robots.txt
uid: zh-Hant/getting-started/advanced-configuration/robots-txt
作者: git.DmitriyKulagin
---

# robots.txt 設定

若要管理 *robots.txt* 設定，請前往 **設定 → 設定 → 一般設定**。

此頁面支援多商店設定；這意味著可以為所有商店定義相同的設定，或為不同的商店設定不同的值。如果您想管理特定商店的設定，請從多商店設定下拉式清單中選擇該商店名稱，並勾選左側所需的核取方塊，即可為其設定自訂值。有關更多詳細資訊，請參閱 [多商店](xref:zh-Hant/getting-started/advanced-configuration/multi-store)。

## robots.txt

*robots.txt* 檔案會告訴搜尋引擎爬蟲哪些 URL 可以存取您的網站。這主要用於避免您的網站因過多請求而超載；它並非將網頁從 Google 搜尋結果中移除的機制。若要將網頁排除在 Google 搜尋結果之外，請使用 `noindex` 封鎖索引，或為網頁設定密碼保護。

請依下列方式定義 *robots.txt* 設定：
![Security](_static/robots-txt/robots-txt.jpg)

- **允許 sitemap.xml (Allow sitemap.xml)** - 勾選以允許機器人存取 sitemap.xml 檔案。
- **不允許的語言 (Disallow languages)** - 不允許存取的語言清單。如果您不想限制機器人存取特定語言，請將此欄位留空。
- **不允許的路徑 (Disallow paths)** - 不允許存取的路徑清單。
- **在地化不允許的路徑 (Localizable disallow paths)** - 不允許存取的在地化路徑清單。
- **附加規則 (Additions rules)** - 輸入 *robots.txt* 檔案的附加規則。規則是用於指示爬蟲網站中哪些部分可以被檢索的指令。關於每個規則的完整說明，請閱讀此頁面：[Google 對 robots.txt 規範的解讀](https://developers.google.com/search/docs/crawling-indexing/robots/robots_txt)。

> [!NOTE]
>
> 您也可以透過在網站的 wwwroot 目錄中加入 *robots.additions.txt* 檔案，來擴充 *robots.txt* 的資料。