---
標題: 小工具（設計師指南）
uid: zh-Hant/developer/design/widgets
作者: git.AndreiMaz
貢獻者: git.exileDev, git.DmitriyKulagin
---

# 小工具（設計師指南）

> 小工具（Widget）是一種獨立的應用程式，可以由任何使用者嵌入到第三方網站的頁面中。這是一個小型應用程式，可由終端使用者在網頁內安裝並執行。（節錄自 Wikipedia）。

在 nopCommerce 中，小工具外掛允許您將第三方程式碼或應用程式嵌入到公開商店的特定區域中（例如：head 標籤、body 標籤之後、左側欄區塊以及右側欄區塊）。

目前，預設的 nopCommerce 安裝允許商店管理員嵌入幾個小工具外掛：

1. Google Analytics
2. Swiper
3. Facebook Pixel

## Google Analytics 小工具

Google Analytics 是來自 Google 的免費網站統計工具。它可以追蹤您網站訪客的統計資料以及電子商務轉換率。此小工具區塊可呈現在：

* HTML Header 標籤
* `<body>` 結束 HTML 標籤之後。

若要設定 Google Analytics 小工具，請前往 **後台 → 設定 → 小工具**，點擊 **Google Analytics** 旁邊的 **設定**，並加入您的 Google Analytics 代碼。

## Swiper

Swiper 是一個精美且簡潔的 jQuery 圖片輪播器，適用於您的網站或首頁，可顯示多張具備獨特轉場效果的輪播圖片。

在預設情況下，nopCommerce 內建了 Swiper 小工具整合（預設為啟用），允許您在首頁自動顯示多張輪播圖片。

## Facebook Pixel

*Facebook Pixel* 是一種分析工具，用於追蹤使用者在網站上的行為：他們瀏覽了哪些頁面、點擊了哪些按鈕和連結、填寫了哪些表單以及其他操作。它允許您為 Facebook 廣告活動建立受眾群體。