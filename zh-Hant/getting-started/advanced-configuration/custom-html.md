---
標題: 自訂 HTML
uid: zh-Hant/getting-started/advanced-configuration/custom-html
作者: git.DmitriyKulagin
---

# 自訂 HTML 設定

若要管理 *自訂 HTML* 設定，請前往 **設定 → 設定 → 一般設定**。

此頁面支援多商店設定；這意味著可以為所有商店定義相同的設定，或針對不同商店設定不同的值。如果您想管理特定商店的設定，請從多商店設定下拉式清單中選擇該商店名稱，並勾選左側所需的核取方塊以設定其自訂值。有關更多詳細資訊，請參閱 [多商店](xref:zh-Hant/getting-started/advanced-configuration/multi-store)。

## 自訂 HTML

雖然完整使用 nopCommerce 並不需要具備典型的程式開發知識，但在某些情況下，您可能需要新增頁首 (header) 和頁尾 (footer) 程式碼。例如，您可能想要使用分析工具，這是導致網站檔案遭到駭客入侵的常見原因之一。

許多工具與追蹤指令碼都需要您將程式碼片段新增至網站的頁首或頁尾。在本項目中，我們將示範如何將程式碼加入 nopCommerce 的頁首或頁尾 HTML 中。

標準網站會拆解成幾個不同的組件，就像一份文字文件一樣：

- 頁首 (Header)。您網站的頁首包含許多「預載」元素，以及關於您的 *安全通訊端層 (SSL)* 憑證、加密、任何 JavaScript 等詳細資訊。
- 內文 (Body)。
- 頁尾 (Footer)。其運作方式與頁首類似，但位於頁面的最底部。

伺服器會以線性方式載入頁面：頁首、內文，然後是頁尾。這意味著頁首程式碼會優先載入，而頁尾程式碼會在其他所有內容載入後才執行。

請依照下列方式定義 *自訂 HTML* 設定：
![Security](_static/custom-html/custom-html.jpg)

這將會在全域（即全站範圍）層級新增任何程式碼。