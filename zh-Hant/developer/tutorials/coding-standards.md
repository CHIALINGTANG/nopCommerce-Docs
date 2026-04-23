---
標題: 程式碼撰寫標準
uid: zh-Hant/developer/tutorials/coding-standards
作者: git.AndreiMaz
貢獻者: git.DmitriyKulagin
---

# 程式碼撰寫標準
nopCommerce 擁有特定的程式碼撰寫準則，開發人員在撰寫原始碼時應當遵循。Visual Studio 中的 [.editorconfig](https://github.com/nopSolutions/nopCommerce/blob/develop/.editorconfig) 檔案有助於強制執行所需的程式碼風格。

共有三類受支援的 .NET 程式碼慣例：
- 語言慣例
- 格式慣例
- 命名慣例

## 語言規範

### .NET 程式碼風格設定

#### "this." 限定詞

此風格規則可應用於欄位、屬性、方法或事件。

- 建議程式碼元素「不」使用 `this.` 作為前綴。
- 建議欄位「不」使用 `this.` 作為前綴。

  ```csharp
  //Right
  capacity = 0;
  ```

  ```csharp
  //Wrong
  this.capacity = 0;
  ```

- 建議屬性「不」使用 `this.` 作為前綴。

  ```csharp
  //Right
  ID = 0;
  ```

  ```csharp
  //Wrong
  this.ID = 0;
  ```

- 建議方法「不」使用 `this.` 作為前綴。

  ```csharp
  //Right
  Display();
  ```

  ```csharp
  //Wrong
  this.Display();
  ```

- 建議事件「不」使用 `this.` 作為前綴。

  ```csharp
  //Right
  Elapsed += Handler;
  ```

  ```csharp
  //Wrong
  this.Elapsed += Handler;
  ```

#### 使用語言關鍵字而非框架型別名稱進行型別參考

此風格規則可應用於區域變數、方法參數、類別成員，或作為型別成員存取運算式的獨立規則。

- 對於有對應關鍵字的型別，建議針對區域變數、方法參數及類別成員使用語言關鍵字，而非型別名稱。

  ```csharp
  //Right
  private int _member;
  ```

  ```csharp
  //Wrong
  private Int32 _member;
  ```

- 對於有對應關鍵字的型別，建議針對成員存取運算式使用語言關鍵字，而非型別名稱。

  ```csharp
  //Right
  var local = int.MaxValue;
  ```

  ```csharp
  //Wrong
  var local = Int32.MaxValue;
  ```

#### 修飾詞偏好

本節中的風格規則涉及修飾詞偏好，包括要求存取修飾詞、指定所需的修飾詞排序，以及要求唯讀 (read-only) 修飾詞。

- 建議宣告存取修飾詞，公開介面成員除外。

  ```csharp
  //Right
  class MyClass
  {
      private const string thisFieldIsConst = "constant";
  }
  ```

  ```csharp
  //Wrong
  class MyClass
  {
      const string thisFieldIsConst = "constant";
  }
  ```

- 建議使用指定的排序方式：

    *`public, private, protected, internal, static, extern, new, virtual, abstract, sealed, override, readonly, unsafe, volatile, async:silent`*

  ```csharp
  //Right
  class MyClass
  {
      private static readonly int _daysInYear = 365;
  }
  ```

#### 小括號偏好

本節中的風格規則涉及小括號偏好，包括在算術、關係與其他二元運算子中使用小括號。

- 建議使用小括號來釐清算術運算子 (*, /, %, +, -, <<, >>, &, ^, |) 的優先順序。

  ```csharp
  //Right
  var v = a + (b * c);
  ```

  ```csharp
  //Wrong
  var v = a + b * c;
  ```

- 建議使用小括號來釐清關係運算子 (>, <, <=, >=, is, as, ==, !=) 的優先順序。

  ```csharp
  //Right
  var v = (a < b) == (c > d);
  ```

  ```csharp
  //Wrong
  var v = a < b == c > d;
  ```

- 建議使用小括號來釐清其他二元運算子 (&&, ||, ??) 的優先順序。

  ```csharp
  //Right
  var v = a || (b && c);
  ```

  ```csharp
  //Wrong
  var v = a || b && c;
  ```

- 當運算子優先順序明確時，建議不要使用小括號。

  ```csharp
  //Right
  var v = a.b.Length;
  ```

  ```csharp
  //Wrong
  var v = (a.b).Length;
  ```

#### 運算式層級偏好

本節中的風格規則涉及運算式層級的偏好，包括物件初始設定式、集合初始設定式、明確或推斷的元組 (tuple) 名稱，以及推斷的匿名型別的使用。

- 在可能的情況下，建議使用物件初始設定式來初始化物件。

  ```csharp
  //Right
  var c = new Customer() { Age = 21 };
  ```

  ```csharp
  //Wrong
  var c = new Customer();
  c.Age = 21;
  ```

- 在可能的情況下，建議使用集合初始設定式來初始化集合。

  ```csharp
  //Right
  var list = new List<int> { 1, 2, 3 };
  ```

  ```csharp
  //Wrong
  var list = new List<int>();
  list.Add(1);
  list.Add(2);
  list.Add(3);
  ```

- 建議使用元組名稱而非 ItemX 屬性。

  ```csharp
  //Right
  (string name, int age) customer = GetCustomer();
  var name = customer.name;
  ```

  ```csharp
  //Wrong
  (string name, int age) customer = GetCustomer();
  var name = customer.Item1;
  ```

- 建議使用推斷的元組元素名稱。

  ```csharp
  //Right
  var tuple = (age, name);
  ```

  ```csharp
  //Wrong
  var tuple = (age: age, name: name);
  ```

- 建議使用明確的匿名型別成員名稱。

  ```csharp
  //Right
  var anon = new { age = age, name = name };
  ```

  ```csharp
  //Wrong
  var anon = new { age, name };
  ```

- 建議使用自動屬性 (auto-properties) 而非具有私有備份欄位的屬性。

  ```csharp
  //Right
  private int Age { get; }
  ```

  ```csharp
  //Wrong
  private int age;

  public int Age
  {
      get
      {
          return age;
      }
  }
  ```

- 建議使用帶有模式比對的 null 檢查，而非 *`object.ReferenceEquals`*。

  ```csharp
  //Right
  if (value is null)
      return;
  ```

  ```csharp
  //Wrong
  if (object.ReferenceEquals(value, null))
      return;
  ```

- 建議使用三元條件運算式進行指派，而非使用 if-else 陳述式。

  ```csharp
  //Right
  string s = expr ? "hello" : "world";
  ```

  ```csharp
  //Wrong
  string s;
  if (expr)
  {
      s = "hello";
  }
  else
  {
      s = "world";
  }
  ```

- 建議 return 陳述式使用三元條件運算式，而非使用 if-else 陳述式。

  ```csharp
  //Right
  return expr ? "hello" : "world";
  ```

  ```csharp
  //Wrong
  if (expr)
  {
      return "hello";
  }
  else
  {
      return "world";
  }
  ```

- 建議使用複合指派運算式。

  ```csharp
  //Right
  x += 1;
  ```

  ```csharp
  //Wrong
  x = x + 1;
  ```

#### Null 檢查偏好

本節中的風格規則涉及 Null 檢查的偏好。

- 建議使用 null 合併運算式 (null coalescing expressions)，而非三元運算子檢查。

  ```csharp
  //Right
  var v = x ?? y;
  ```

  ```csharp
  //Wrong
  var v = x != null ? x : y; // or
  var v = x == null ? y : x;
  ```

- 在可能的情況下，建議使用 null 條件運算子 (null-conditional operator)。

  ```csharp
  //Right
  var v = o?.ToString();
  ```

  ```csharp
  //Wrong
  var v = o == null ? null : o.ToString(); // or
  var v = o != null ? o.String() : null;
  ```

### C# 程式碼風格設定

#### 隱式與顯式型別

本節的風格規則涉及在變數宣告中使用 `var` 關鍵字與顯式型別的選擇。此規則可分別應用於內建型別（當型別明確時）及其他情況。

- 當宣告內建系統型別（如 `int`）的變數時，建議使用 *`var`*

  ```csharp
  //Right
  var x = 5;
  ```

  ```csharp
  //Wrong
  int x = 5;
  ```

- 當型別已在宣告表達式的右側提及時，建議使用 *`var`*

  ```csharp
  //Right
  var obj = new Customer();
  ```

  ```csharp
  //Wrong
  Customer obj = new Customer();
  ```

- 除非被其他程式碼風格規則覆寫，否則在所有情況下，建議優先使用 *`var`* 而非顯式型別

  ```csharp
  //Right
  var f = this.Init();
  ```

  ```csharp
  //Wrong
  bool f = this.Init();
  ```

#### 表達式主體成員 (Expression-bodied members)

本節的風格規則涉及在邏輯僅由單一表達式組成時，使用 [表達式主體成員](https://docs.microsoft.com/dotnet/csharp/programming-guide/statements-expressions-operators/expression-bodied-members) 的方式。此規則可應用於方法、建構函式、運算子、屬性、索引子及存取子。

- 針對方法，建議使用區塊主體

  ```csharp
  //Right
  public int GetAge() { return this.Age; }
  ```

  ```csharp
  //Wrong
  public int GetAge() => this.Age;
  ```

- 針對建構函式，建議使用區塊主體

  ```csharp
  //Right
  public Customer(int age) { Age = age; }
  ```

  ```csharp
  //Wrong
  public Customer(int age) => Age = age;
  ```

- 針對運算子，建議使用區塊主體

  ```csharp
  //Right
  public static ComplexNumber operator + (ComplexNumber c1, ComplexNumber c2)
  { return new ComplexNumber(c1.Real + c2.Real, c1.Imaginary + c2.Imaginary); }
  ```

  ```csharp
  //Wrong
  public static ComplexNumber operator + (ComplexNumber c1, ComplexNumber c2)
      => new ComplexNumber(c1.Real + c2.Real, c1.Imaginary + c2.Imaginary);
  ```

- 當屬性僅為單行時，建議使用表達式主體

  ```csharp
  //Right
  public int Age => _age;
  ```

  ```csharp
  //Wrong
  public int Age { get { return _age; }}
  ```

- 針對索引子，建議使用表達式主體

  ```csharp
  //Right
  public T this[int i] => _values[i];
  ```

  ```csharp
  //Wrong
  public T this[int i] { get { return _values[i]; } }
  ```

- 針對存取子，建議使用表達式主體

  ```csharp
  //Right
  public int Age { get => _age; set => _age = value; }
  ```

  ```csharp
  //Wrong
  public int Age { get { return _age; } set { _age = value; } }
  ```

- 針對 Lambda 表達式，建議使用表達式主體

  ```csharp
  //Right
  Func<int, int> square = x => x * x;
  ```

  ```csharp
  //Wrong
  Func<int, int> square = x => { return x * x; };
  ```

#### 模式比對 (Pattern matching)

本節的風格規則涉及在 C# 中使用 [模式比對](https://docs.microsoft.com/dotnet/csharp/pattern-matching)。

- 建議使用模式比對，而非帶有型別轉型的 `is` 表達式

  ```csharp
  //Right
  if (o is int i) {...}
  ```

  ```csharp
  //Wrong
  if (o is int) {var i = (int)o; ... }
  ```

- 建議使用模式比對，而非使用 `as` 表達式配合 null 檢查來判斷某個物件是否為特定型別

  ```csharp
  //Right
  if (o is string s) {...}
  ```

  ```csharp
  //Wrong
  var s = o as string;
  if (s != null) {...}
  ```

#### 內嵌變數宣告

此風格規則涉及 `out` 變數是否應採用內嵌方式宣告。從 C# 7 開始，您可以 [在方法呼叫的引數列表中宣告 `out` 變數](https://docs.microsoft.com/dotnet/csharp/language-reference/keywords/out-parameter-modifier#calling-a-method-with-an-out-argument)，而不必在個別的變數宣告中進行。

- 盡可能在方法呼叫的引數列表中以內嵌方式宣告 *`out`* 變數

  ```csharp
  //Right
  if (int.TryParse(value, out int i)) {...}
  ```

  ```csharp
  //Wrong
  int i;
  if (int.TryParse(value, out i)) {...}
  ```

#### C# 表達式層級偏好設定

此風格規則涉及在編譯器可以推斷表達式型別時，使用 [default 常值作為預設值表達式](https://docs.microsoft.com/dotnet/csharp/programming-guide/statements-expressions-operators/default-value-expressions#default-literal-and-type-inference)。

- 建議使用 *`default`* 而非 *`default(T)`*

  ```csharp
  //Right
  void DoWork(CancellationToken cancellationToken = default) { ... }
  ```

  ```csharp
  //Wrong
  void DoWork(CancellationToken cancellationToken = default(CancellationToken)) {   ... }
  ```

#### C# null 檢查偏好設定

這些風格規則涉及 null 檢查相關的語法，包含是否使用 throw 表達式或 throw 陳述式，以及在呼叫 [Lambda 表達式](https://docs.microsoft.com/dotnet/csharp/lambda-expressions) 時，應執行 null 檢查還是使用條件式合併運算子 (`?.`)。

- 建議使用 throw 表達式，而非 throw 陳述式

  ```csharp
  //Right
  this.s = s ?? throw new ArgumentNullException(nameof(s));
  ```

  ```csharp
  //Wrong
  if (s == null) { throw new ArgumentNullException(nameof(s)); }
  this.s = s;
  ```

- 在呼叫 Lambda 表達式時，建議使用條件式合併運算子 (`?.`)，而非執行 null 檢查

  ```csharp
  //Right
  func?.Invoke(args);
  ```

  ```csharp
  //Wrong
  if (func != null) { func(args); }
  ```

#### 程式碼區塊偏好設定

此風格規則涉及使用大括號 `{ }` 來包覆程式碼區塊。

- 若允許，建議不使用大括號

  ```csharp
  //Right
  if (test) this.Display();
  ```

  ```csharp
  //Wrong
  if (test) { this.Display(); }
  ```

## 格式規範

### .NET 格式化設定

### 組織 using 指示詞

這些格式規則與 *`using`* 指示詞和 *`Imports`* 陳述式的排序及顯示方式有關。

- 請將 `System.*` *`using`* 指示詞按字母順序排列，並置於其他 using 指示詞之前。

  ```csharp
  //Right
  using System.Collections.Generic;
  using System.Threading.Tasks;
  using Octokit;
  ```

  ```csharp
  //Wrong
  using System.Collections.Generic;
  using Octokit;
  using System.Threading.Tasks;
  ```

- 請勿在不同的 using 指示詞群組之間插入空白行。

  ```csharp
  //Right
  using System.Collections.Generic;
  using System.Threading.Tasks;
  using Octokit;
  ```

  ```csharp
  //Wrong
  using System.Collections.Generic;
  using System.Threading.Tasks;

  using Octokit;
  ```

### C# 格式化設定

本節中的格式化規則僅適用於 C# 程式碼。

#### 新行選項

這些格式化規則與使用新行來格式化程式碼有關。

- 要求所有運算式的右大括號必須位於新行（「Allman」風格）。

  ```csharp
  //Right
  void MyMethod()
  {
      if (...)
      {
          ...
      }
  }
  ```

  ```csharp
  //Wrong
  void MyMethod() {
      if (...) {
          ...
      }
  }
  ```

- 將 `else` 陳述式置於新行。

  ```csharp
  //Right
  if (...) 
  {
      ...
  }
  else 
  {
      ...
  }
  ```

  ```csharp
  //Wrong
  if (...) {
      ...
  } else {
      ...
  }
  ```

- 將 `catch` 陳述式置於新行。

  ```csharp
  //Right
  try 
  {
      ...
  }
  catch (Exception e) 
  {
      ...
  }
  ```

  ```csharp
  //Wrong
  try {
      ...
  } catch (Exception e) {
      ...
  }
  ```

- 要求 `finally` 陳述式在右大括號後必須位於新行。

  ```csharp
  //Right
  try 
  {
      ...
  }
  catch (Exception e) 
  {
      ...
  }
  finally 
  {
      ...
  }
  ```

  ```csharp
  //Wrong
  try {
      ...
  } catch (Exception e) {
      ...
  } finally {
      ...
  }
  ```

- 要求物件初始化運算式的成員必須位於獨立的行。

  ```csharp
  //Right
  var z = new B()
  {
      A = 3,
      B = 4
  }
  ```

  ```csharp
  //Wrong
  var z = new B()
  {
      A = 3, B = 4
  }
  ```

- 要求匿名型別的成員必須位於獨立的行。

  ```csharp
  //Right
  var z = new
  {
      A = 3,
      B = 4
  }
  ```

  ```csharp
  //Wrong
  var z = new
  {
      A = 3, B = 4
  }
  ```

- 要求查詢運算式子句的元素必須位於獨立的行。

  ```csharp
  //Right
  var q = from a in e
          from b in e
          select a * b;
  ```

  ```csharp
  //Wrong
  var q = from a in e from b in e
          select a * b;
  ```

#### 縮排選項

這些格式化規則與使用縮排來格式化程式碼有關。

- 縮排 *`switch`* case 的內容。

  ```csharp
  //Right
  switch(c) 
  {
      case Color.Red:
          Console.WriteLine("The color is red");
          break;
      case Color.Blue:
          Console.WriteLine("The color is blue");
          break;
      default:
          Console.WriteLine("The color is unknown.");
          break;
  }
  ```

  ```csharp
  //Wrong
  switch(c) {
      case Color.Red:
      Console.WriteLine("The color is red");
      break;
      case Color.Blue:
      Console.WriteLine("The color is blue");
      break;
      default:
      Console.WriteLine("The color is unknown.");
      break;
  }
  ```

- 縮排 *`switch`* 標籤。

  ```csharp
  //Right
  switch(c) 
  {
      case Color.Red:
          Console.WriteLine("The color is red");
          break;
      case Color.Blue:
          Console.WriteLine("The color is blue");
          break;
      default:
          Console.WriteLine("The color is unknown.");
          break;
  }
  ```

  ```csharp
  //Wrong
  switch(c) {
  case Color.Red:
      Console.WriteLine("The color is red");
      break;
  case Color.Blue:
      Console.WriteLine("The color is blue");
      break;
  default:
      Console.WriteLine("The color is unknown.");
      break;
  }
  ```

- 將標籤放置於與當前內容相同的縮排層級。

  ```csharp
  //Right
  class C
  {
      private string MyMethod(...)
      {          
          if (...) 
          {
              goto error;
          }
          error:
          throw new Exception(...);
      }
  }
  ```

  ```csharp
  //Wrong
  class C
  {
      private string MyMethod(...)
      {
          if (...) {
              goto error;
          }
  error:
          throw new Exception(...);
      }
  }
  ```

  ```csharp
  //Wrong
  class C
  {
      private string MyMethod(...)
      {
          if (...) {
              goto error;
          }
      error:
          throw new Exception(...);
      }
  }
  ```

#### 間距選項

這些格式化規則與使用空格字元來格式化程式碼有關。

- 移除轉型 (cast) 與值之間的空格。

  ```csharp
  //Right
  int y = (int)x;
  ```

  ```csharp
  //Wrong
  int y = (int) x;
  ```

- 在控制流程陳述式（例如 *`for`* 迴圈）的關鍵字後放置一個空格字元。

  ```csharp
  //Right
  for (int i;i<x;i++) { ... }
  ```

  ```csharp
  //Wrong
  for(int i;i<x;i++) { ... }
  ```

- 在型別宣告中，於基底類別或介面冒號前放置一個空格字元。

  ```csharp
  //Right
  interface I
  {

  }

  class C : I
  {

  }
  ```

  ```csharp
  //Wrong
  interface I
  {

  }

  class C: I
  {

  }
  ```

- 在型別宣告中，於基底類別或介面冒號後放置一個空格字元。

  ```csharp
  //Right
  interface I
  {

  }

  class C : I
  {

  }
  ```

  ```csharp
  //Wrong
  interface I
  {

  }

  class C :I
  {

  }
  ```

- 在二元運算子前後插入空格。

  ```csharp
  //Right
  return x * (x - y);
  ```

  ```csharp
  //Wrong
  return x*(x-y);
  ```

  ```csharp
  //Wrong
  return x  *  (x-y);
  ```

- 移除方法宣告參數清單中，左括號後與右括號前的空格字元。

  ```csharp
  //Right
  void Bark(int x) { ... }
  ```

  ```csharp
  //Wrong
  void Bark( int x ) { ... }
  ```

- 移除方法宣告的空白參數清單括號內的空格。

  ```csharp
  //Right
  void Goo()
  {
      Goo(1);
  }

  void Goo(int x)
  {
      Goo();
  }
  ```

  ```csharp
  //Wrong
  void Goo( )
  {
      Goo(1);
  }

  void Goo(int x)
  {
      Goo();
  }

  ```

- 移除方法宣告中，方法名稱與左括號之間的空格字元。

  ```csharp
  //Right
  void M() { }
  ```

  ```csharp
  //Wrong
  void M () { }
  ```

- 移除方法呼叫中，左括號後與右括號前的空格字元。

  ```csharp
  //Right
  MyMethod(argument);
  ```

  ```csharp
  //Wrong
  MyMethod( argument );
  ```

- 移除空白引數清單括號內的空格。

  ```csharp
  //Right
  void Goo()
  {
      Goo(1);
  }

  void Goo(int x)
  {
      Goo();
  }
  ```

  ```csharp
  //Wrong
  void Goo()
  {
      Goo(1);
  }

  void Goo(int x)
  {
      Goo( );
  }
  ```

- 移除方法呼叫名稱與左括號之間的空格。

  ```csharp
  //Right
  void Goo()
  {
      Goo(1);
  }

  void Goo(int x)
  {
      Goo();
  }
  ```

  ```csharp
  //Wrong
  void Goo()
  {
      Goo(1);
  }

  void Goo(int x)
  {
      Goo ();
  }
  ```

- 在逗號後插入空格。

  ```csharp
  //Right
  int[] x = new int[] { 1, 2, 3, 4, 5 };
  ```

  ```csharp
  //Wrong
  int[] x = new int[] { 1,2,3,4,5 };
  ```

- 移除逗號前的空格。

  ```csharp
  //Right
  int[] x = new int[] { 1, 2, 3, 4, 5 };
  ```

  ```csharp
  //Wrong
  int[] x = new int[] { 1 , 2 , 3 , 4 , 5 };
  ```

- 在 `for` 陳述式的每個分號後插入空格。

  ```csharp
  //Right
  for (int i = 0; i < x.Length; i++)
  ```

  ```csharp
  //Wrong
  for (int i = 0;i < x.Length;i++)
  ```

- 移除 `for` 陳述式的每個分號前的空格。

  ```csharp
  //Right
  for (int i = 0; i < x.Length; i++)
  ```

  ```csharp
  //Wrong
  for (int i = 0 ; i < x.Length ; i++)
  ```

- 移除宣告陳述式中多餘的空格字元。

  ```csharp
  //Right
  int x = 0;
  ```

  ```csharp
  //Wrong
  int    x    =    0   ;
  ```

- 移除左方括號 *`[`* 前的空格。

  ```csharp
  //Right
  int[] numbers = new int[] { 1, 2, 3, 4, 5 };
  ```

  ```csharp
  //Wrong
  int [] numbers = new int [] { 1, 2, 3, 4, 5 };
  ```

- 移除空白方括號 *`[]`* 之間的空格。

  ```csharp
  //Right
  int[] numbers = new int[] { 1, 2, 3, 4, 5 };
  ```

  ```csharp
  //Wrong
  int[ ] numbers = new int[ ] { 1, 2, 3, 4, 5 };
  ```

- 移除包含內容的方括號 *`[0]`* 內的空格字元。

  ```csharp
  //Right
  int index = numbers[0];
  ```

  ```csharp
  //Wrong
  int index = numbers[ 0 ];
  ```

#### 換行選項

這些格式化規則與陳述式和程式碼區塊應使用單行還是獨立行有關。

- 保持陳述式與成員宣告位於不同的行。

  ```csharp
  //Right
  int i = 0;
  string name = "John";
  ```

  ```csharp
  //Wrong
  int i = 0; string name = "John";
  ```

- 保持程式碼區塊位於單行。

  ```csharp
  //Right
  public int Foo { get; set; }
  ```

  ```csharp
  //Wrong
  public int MyProperty
  {
      get; set;
  }
  ```

## 命名慣例

- 常數僅能使用大寫字母命名，並以底線 *`_`* 作為分隔符

  ```csharp
  //Right
  const int TEST_CONSTANT = 1;
  ```

  ```csharp
  //Wrong
  const int Test_Constant = 1;
  ```

- 具有 *`public`* 存取層級的欄位應使用 PascalCase 標記法

  ```csharp
  //Right
  public int TestField;
  ```

  ```csharp
  //Wrong
  public int testField;
  ```

- 介面名稱必須使用 PascalCase 標記法，並具有前綴 *`I`*

  ```csharp
  //Right
  public interface ITestInterface;
  ```

  ```csharp
  //Wrong
  public interface testInterface;
  ```

- 類別、結構、方法、列舉、事件、屬性、命名空間與委派的名稱應使用 PascalCase 標記法

  ```csharp
  //Right
  public class SomeClass;
  ```

  ```csharp
  //Wrong
  public class someClass;
  ```

- 泛型型別的參數請使用 PascalCase 的描述性名稱，除非單一字母已足夠且描述性名稱無法增加額外價值。

  ```csharp
  //Right
  public interface ISessionChannel<TSession> { /*...*/ }
  public delegate TOutput Converter<TInput, TOutput>(TInput from);
  public class List<T> { /*...*/ }
  ```

- 若型別僅包含單一字母的型別參數，請使用型別參數 *`T`* 的名稱

  ```csharp
  //Right
  public int IComparer<T>() { return 0; }
  public delegate bool Predicate<T>(T item);
  public struct Nullable<T> where T : struct { /*...*/ }
  ```

- 請使用前綴 *`T`* 作為型別參數的描述性名稱

  ```csharp
  //Right
  public interface ISessionChannel<TSession>
  {
      TSession Session { get; }
  }
  ```

  請在名稱中指定與該型別參數相關的限制。例如，受 *`ISession`* 限制的參數可以命名為 *`TSession`*。

- 私有 (Private) 與受保護 (Protected) 的類別欄位必須以底線 *`_`* 作為前綴

  ```csharp
  //Right
  private int _testField;
  protected int _testField;
  ```

  ```csharp
  //Wrong
  private int testField;
  protected int testField;
  ```

- 所有其他程式碼元素，例如變數、方法參數以及類別欄位（公開欄位除外），皆應使用 camelCase 標記法。

  ```csharp
  //Right
  var testVar = new Object();
  public void Foo(int firstParam, string secondParam)
  ```

  ```csharp
  //Wrong
  var TestVar = new Object();
  public void Foo(int FirstParam, string SecondParam)
  ```