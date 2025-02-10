# ASP.NET Core 8 開發實戰：應用開發篇 (Web API)


### **開啟 IncludeOpenAPIAnalyzers 檢查 OpenAPI 是否完整**

開啟專案檔案 `*.csproj`，並加入下列程式碼

```xml
<IncludeOpenAPIAnalyzers>true</IncludeOpenAPIAnalyzers>
```

執行 dotnet build 時，會檢查 OpenAPI 是否完整

```cmd
# 範例
warning API1000: Action method returns undeclared status code '404'
```

透過添加 `ProducesResponseType` 屬性，來宣告回應狀態碼

```csharp
[HttpGet("{id}", Name = "取得指定課程Async")]
[ProducesResponseType(StatusCodes.Status200OK)]
[ProducesResponseType(StatusCodes.Status404NotFound)]
[ProducesDefaultResponseType<CourseRead>(StatusCodes.Status200OK)]
public async Task<ActionResult<CourseRead>> GetCourse(int id)
{
}
```

.NET 8 以上可以使用範型，來宣告回應類型

```csharp
[ProducesResponseType<PageCourse>(StatusCodes.Status200OK)]
```

### **.NET 9 以上版本，若想使用 Swagger UI 需要手動安裝回 SwaggerUi**

新增 NuGet 套件並設定 Swagger UI

```bash
dotnet add package Swashbuckle.AspNetCore.SwaggerUi
```

```csharp
if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();

    app.UseSwaggerUI(option =>
    {
        // 指定 OpenAPI 文件的來源（/openapi/v1.json），並將它標記為 "v1"。
        option.SwaggerEndpoint("/openapi/v1.json", "API V1");
    });
}
```

### **安裝 OpenAPI 相關套件、工具**

```bash
dotnet add package NSwag.ApiDescription.Client
dotnet tool install -g Microsoft.dotnet-openapi 
```

### **使用 dotnet-openapi 產生 OpenAPI 文件**

```bash
# Client Project 為 Web API 使用端專案
# Port 為 Web API 服務的 Port
dotnet openapi add url -p {Client Project}.csproj  http://localhost:{Port}/swagger/v1/swagger.json

# 透過檔案方式產生 OpenAPI 文件
dotnet openapi add file -p {Client Project}.csproj swagger.json
```

執行 build 指令重建於 obj 目錄下產生的 `swaggerClient.cs` 檔案

可透過 `ClassName` 屬性指定產生的類別名稱

```xml
<ItemGroup>
    <OpenApiReference Include="swagger.json" SourceUrl="http://localhost:5268/swagger/v1/swagger.json" ClassName="ＷebApi"/>
</ItemGroup>
```

重新下載更新 `swagger.json` 文件

```bash
dotnet openapi refresh -p {Client Project}.csproj http://localhost:{Port}/swagger/v1/swagger.json
```

若使用 .NET 9 預設提供的 Open API 文件，則在客端呼叫時不用指定 URL，因為已包含在 v1.json 檔案中


### **產生 OpenAPI 文件時若有循環參考，則會讓客戶端在建置時失敗**

從錯誤訊息很難看出是循環參考問題 (補充：swagger.json 未遇到此問題)

```bash
 WebApiClient 失敗，有 1 個錯誤 (0.6 秒)
    /Users/xxxxx/.nuget/packages/nswag.apidescription.client/13.0.5/build/NSwag.ApiDescription.Client.targets(28,5): error MSB3073: "dotnet --roll-forward-on-no-candidate-fx 2 /Users/xxxxx/.nuget/packages/nswag.msbuild/13.0.5/build/../tools/NetCore21//dotnet-nswag.dll openapi2csclient /className:v1Client /namespace:WebApiClient /input:/Users/xxxxx/code/github/aspnet-core8-day5/WebApiClient/v1.json /output:obj/v1Client.cs " 命令以返回碼 255 結束。
  WebApiClient 失敗，有 1 個警告 (0.1 秒)
```

可以將 openapi/v1.json 檔案內容貼到 [Swagger Editor](https://editor.swagger.io/) 檢查是否有循環參考問題

範例

```json
{
    "enrollments": {
        "$ref": "#/components/schemas/#/items/properties/department/properties/instructor/properties/enrollments/items/properties/course/properties/enrollments"
    },
    "instructors": {
        "$ref": "#/components/schemas/#/items/properties/department/properties/instructor/properties/enrollments/items/properties/course/properties/instructors"
    }
}
```

解決方式，於請求參數型別和回應型別使用 DTO 替代實體物件


### **以版本切分路由**

安裝 NuGet 套件

```bash
# Microsoft.AspNetCore.Mvc.Versioning
# Microsoft.AspNetCore.Mvc.Versioning.ApiExplorer
# 以上套件為舊版已棄用

dotnet add package Asp.Versioning.Http
# 若使用 Controller 則需安裝該套件 (Minimal API 不需要)
dotnet add package Asp.Versioning.Mvc
# Supper for Swagger
dotnet add package Asp.Versioning.Mvc.ApiExplorer
```

設定版本控制

```csharp
builder.Services.AddApiVersioning(option =>
{
    // 設定預設版本
    option.DefaultApiVersion = new ApiVersion(1, 0);
    // 當未指定版本時，將使用預設版本
    option.AssumeDefaultVersionWhenUnspecified = true;
    // 在回應中報告支援的 API 版本
    option.ReportApiVersions = true;
    // 設定 API 版本讀取器，可使用 Combine 方法來結合多個版本讀取器
    // 使用 HTTP 標頭來讀取 API 版本
    // 使用查詢字串來讀取 API 版本
    // 使用 URL 段來讀取 API 版本
    option.ApiVersionReader = ApiVersionReader.Combine(
        new HeaderApiVersionReader("api-version"),
        new QueryStringApiVersionReader("api-version"),
        new UrlSegmentApiVersionReader());
})
.AddApiExplorer(option =>
{
    // 設定了 API 版本的群組名稱格式。這裡的值 "'v'VVV" 表示群組名稱將以字母 "v" 開頭，後面跟著版本號碼。VVV 是一個佔位符，代表版本號碼的格式。這樣的設定有助於在 API 文件中清晰地標示出不同版本的 API。
    option.GroupNameFormat = "'v'VVV";
    // URL 中會替換 API 版本號碼。這個選項允許在 API 路徑中直接包含版本資訊，使得 API 路徑更加直觀和易於理解。例如，如果 API 版本是 1.0，那麼 URL 可能會變成 /api/v1.0/ 這樣的格式
    option.SubstituteApiVersionInUrl = true;
});
```

使用版本控制，以下為兩個相同的 API 但版本不同，並在各自 Action 上指定對應的版本號碼

```csharp
[ApiVersion("1")]
[ApiVersion("2")]
[ApiController]
[Route("api/v{apiVersion:apiVersion}/[controller]")] // 此為 URL 包含版本號碼
public class CoursesController : ControllerBase
{
    // GET: api/Courses
    [MapToApiVersion("1")]
    [HttpGet(Name = "GetCoursesV1Async")]
    [ProducesResponseType<PageCourse>(StatusCodes.Status200OK)]
    public async Task<ActionResult<IEnumerable<PageCourse>>> GetCoursesV1(
        [Range(1, int.MaxValue, ErrorMessage = "pageIndex 不能小於 1")] 
        int pageIndex = 1, 
        int pageSize = 10)
    {
        // ...
    }

    // GET: api/Courses
    [MapToApiVersion("2")]
    [HttpGet(Name = "GetCoursesV2Async")]
    [ProducesResponseType<PageCourse>(StatusCodes.Status200OK)]
    public async Task<ActionResult<IEnumerable<PageCourse>>> GetCoursesV2(
        [Range(1, int.MaxValue, ErrorMessage = "pageIndex 不能小於 1")] 
        int pageIndex = 1, 
        int pageSize = 5)
    {
        // ...
    }
}
```

若想在 Swagger UI 上針對對應的版本內容產生文件，則需要以下設定

透過 `DescribeApiVersions` 方法取得支援的版本號碼，並透過 `SwaggerEndpoint` 方法來設定 Swagger UI 的路由

支援的版本號會由 Controller 上的 `ApiVersion` 屬性來取得

```csharp
app.UseSwaggerUI(options =>
{
    IReadOnlyList<ApiVersionDescription> descriptions = app.DescribeApiVersions();

    foreach (var description in descriptions)
    {
        string url = $"/openapi/{description.GroupName}.json";
        string name = description.GroupName.ToUpperInvariant();
        options.SwaggerEndpoint(url, name);
    }
});
```

若為 .NET 9 以前的版本可以自行實作 `IConfigureNamedOptions<SwaggerGenOptions>` 來自定義文檔描述

```csharp
public class ConfigureSwaggerGenOptions : IConfigureNamedOptions<SwaggerGenOptions>
{
    private readonly IApiVersionDescriptionProvider _provider;

    public ConfigureSwaggerGenOptions(IApiVersionDescriptionProvider provider)
    {
        _provider = provider;
    }

    public void Configure(string? name, SwaggerGenOptions options)
    {
        Configure(options);
    }

    public void Configure(SwaggerGenOptions options)
    {
        foreach (var description in _provider.ApiVersionDescriptions)
        {
            var openApiInfo = new OpenApiInfo
            {
                Title = $"WebApi v{description.ApiVersion}",
                Version = description.ApiVersion.ToString()
            };

            options.SwaggerDoc(description.GroupName, openApiInfo);
        }
    }
}
```

若為 .NET 9 版本並使用了預設的 OpenAPI 文件則是在 `AddOpenApi()` 方法中透過 `AddDocumentTransformer` 來自定義文檔描述

```csharp
builder.Services.AddOpenApi("v1", option =>
{
    option.AddDocumentTransformer((document, context, cancellationToken) =>
    {
        document.Info = new()
        {
            Title = "Contoso University API",
            Version = "v1",
            Description = "API for processing Contoso University data",
        };

        return Task.CompletedTask;
    });
    option.AddDocumentTransformer(new CustomDocumentTransformer(option.DocumentName));
});
```

若為 .NET 9 版本並使用了預設的 OpenAPI 文件且有多份文件時需將各文件名稱加入 OpenAPI 文件中

```csharp
builder.Services.AddOpenApi("v1");
builder.Services.AddOpenApi("v2");
```

呼叫 API 時，可以透過 URL 段來指定版本號碼

```bash
# 透過 URL 來指定版本號碼
GET {{WebApi_HostAddress}}/api/v1/courses
Accept: application/json

# 透過 Header 標頭來指定版本號碼
GET {{WebApi_HostAddress}}/api/courses
Accept: application/json
api-version: 2.0

# 透過 QueryString 來指定版本號碼
GET {{WebApi_HostAddress}}/api/courses?api-version=2.0
Accept: application/json

# 回傳 Header 會帶有支援的版本號碼
HTTP/1.1 200 OK
Connection: close
Content-Type: application/json; charset=utf-8
Date: Fri, 31 Jan 2025 07:03:34 GMT
Server: Kestrel
Transfer-Encoding: chunked
api-supported-versions: 1, 2
```


### **當公司規定無法使用 `Post` 以外的動詞時，透過 `:update` 等方式來取代**

這種用法通常被稱為「動詞後綴」或「動詞後綴模式」。這種模式在 RESTful API 設計中使用，以便在僅允許使用 POST 方法的情況下，仍能夠表達不同的操作（例如更新、刪除等）。

```bash
PUT {{HostDomain}}/api/courses/1

# 調整為
POST {{HostDomain}}/api/courses/1:Update
```


### **安不安全與 HTTP 動詞無關，動詞只是讓語意更清楚**

開發者沒有資安意思才不安全

早期有個 WebDAV 是一種基於 HTTP 協議的工具，可以用來新增、刪除、修改伺服器端的資料。IIS（Internet Information Services）曾經內建並預設啟用 WebDAV 功能，這確實可能會帶來安全風險，因為駭客可以利用這個功能來植入檔案。然而，HTTP 動詞本身並不是危險的，危險的是未經適當保護和配置的 WebDAV 功能。正確的安全配置和權限管理可以有效防止這類攻擊。


### **`CreatedAtRoute` vs. `CreatedAtAction`**

`CreatedAtRoute(nameof(GetBookById), ...)`：使用路由名稱來建立 Location 標頭，指向新創建的資源。

`CreatedAtAction(nameof(Get), ...)`：使用控制器內的方法名稱來建立 Location 標頭。

### **`CreatedAtRoute` 使用具名路由**
```csharp
return CreatedAtRoute(nameof(GetBookById),
    new { id = product.ProductId },
    book);
```
- **`CreatedAtRoute`** 會根據 `HttpGet("api/books/{id}", Name = nameof(GetBookById))` 指定的名稱 `GetBookById` 來匹配路由。
- **優勢**：即使 `GetBookById` 的路由定義變更（例如 URL 改成 `v2/books/{id}`），只要名稱保持不變，這段程式碼仍然有效。

### **`CreatedAtAction` 使用動作名稱**
```csharp
return CreatedAtAction(nameof(Get),
    new { id = product.ProductId },
    book);
```
- **`CreatedAtAction`** 會根據控制器內部的 `Get` 方法名稱來建立 URL。
- **優勢**：不需要明確設定路由名稱，程式碼更加直觀。

## **何時應該使用哪個？**
| 方法 | 何時使用 | 優勢 |
|------|----------|------|
| `CreatedAtRoute` | **當你希望透過具名路由（route name）來確保路徑穩定時** | 即使方法名稱變更，`Name` 屬性不變，仍然可以匹配 |
| `CreatedAtAction` | **當你只在當前控制器內使用方法名稱時** | 直接使用方法名稱，程式碼更簡潔，但若改變方法名稱則可能影響行為 |


### **路由套用順序**

1. 純字串、無參數、無萬用字元（Literal segments）
2. 比對是否設定「路由順序 Order」
    - 預設 Order=0，數字越小優先權越高
3. 有路由參數，且有「套用限定詞」
4. 有路由參數，且無「套用限定詞」
5. 有「萬用字元參數（Wildcard Parameter）」且有「套用限定詞」
6. 有「萬用字元參數」但無「套用限定詞」


### **套用 `[ApiController]` 屬性**

- 必須使用屬性路由 (Attribute Routing)

- 只要發生模型驗證失敗，就會自動回應 HTTP 400 (Bad Request)
  - 預設以 `ValidationProblemDetails` 型別回應
  - 預設 `BadRequest` 回應

- 自動套用模型繫結的預設規則
  - 複雜型別 預設就會自動套用 `[FromBody]` 屬性
  - 參數型別 如果是 `IFormFile` 或 `IFormFileCollection` 的話
    - 會自動套用 `[FromForm]` 屬性，且該屬性也只能用在這兩個型別
  - 簡單模型或任何其他參數，全部都會自動套用 `[FromQuery]` 屬性
    - 例如「路由參數」預設會自動套用 `[FromQuery]`


### **`[FromBody]` 屬性**

- 在有套用 `[ApiController]` 的 Controller 情況下
  - 預設只會套用「複雜模型」

- 主要特性
  - 只會從 Request Body 取值

- 應用情境
  - 通常用在「簡單模型」的資料類型（`int`、`string`、`DateTime`、...）
  - 當簡單模型內容過大，超出 URL 可允許的長度限制，才會特別這樣用


### **`[FromForm]` 屬性**

- 在有套用 `[ApiController]` 的 Controller 情況下
  - 預設會套用在 `IFormFile` 或 `IFormFileCollection` 複雜模型上

- 主要特性
  - 只會從 Request Body 取值

- 應用情境
  - 雖然也可以用來繫結表單 `POST` 過來的欄位，但通常不會這麼用
  - 實務上大多不會特別套用 `[FromForm]` 屬性，因為在有套用 `[ApiController]` 的控制器 (Controller)，`IFormFile` 或 `IFormFileCollection` 預設就已經會套用上去！


### `[FromQuery]` 屬性

- 在有套用 `[ApiController]` 的 Controller 情況下
  - 預設只會套用「簡單模型」

- 主要特性
  - 只會從 Query String 取值

- 應用情境
  - 通常用在「複雜模型」的資料類型（使用者自定義型別）
  - 當參數過多、資料量不大的情況下，才會特別這樣用

範例

```http
GET https://localhost:5001/api/values?Latitude=25.0436381&Longitude=121.5234672
```


```csharp
public class GeoPoint
{
    public double Latitude { get; set; }
    public double Longitude { get; set; }
}

[Route("api/[controller]")]
[ApiController]
public class ValuesController : ControllerBase
{
    [HttpGet]
    public ActionResult<GeoPoint> GetAll([FromQuery] GeoPoint p)
    {
        return p;
    }
}
```


### `[FromRoute]` 屬性
- 在有套用 `[ApiController]` 的 Controller 情況下
  - 預設只會套用「簡單模型」

- 主要特性
  - 只會從 Route template 取值（Path Info 部分）

- 應用情境
  - 通常用在「簡單模型」的資料類型
  - 當想直接繫結已知的路由參數，才會特別這樣用
  - 實務上大多不會特別套用 `[FromRoute]` 屬性，但可增加程式碼可讀性


### **`[FromHeader]` 屬性**
- 在有套用 `[ApiController]` 的 Controller 情況下
  - 無預設行為，必須套用此屬性才能取值

- 主要特性
  - 只會從 Request header 取值
  - 只能套用「簡單模型」的資料類型

- 應用情境
  - 當 Web API 設計參數來自 Request header 傳遞時才會用上
  - 宣告範例
```csharp
[FromHeader(Name = "X-ClientID")] string clientId
```


### **`[FromServices]` 屬性**
- 在有套用 `[ApiController]` 的 Controller 情況下
  - 無預設行為，必須套用此屬性才能取值

- 主要特性
  - 只會從 `IServiceCollection` 取值（從 DI 容器取得服務物件）

- 應用情境
  - 實務上大多不會特別套用 `[FromServices]` 屬性，而採用建構式注入
  - 宣告範例
```csharp
[FromServices] IDateTime dt
```


### **DTO 驗證**

當有定義 required 時，在做序列化、反序列時，若缺少該屬性，則會回應 400 Bad Request，而不會往下進入到模型驗證的部分

```csharp
public required string Title { get; set; }
```

錯誤訊息

```json
{
  "type": "https://tools.ietf.org/html/rfc9110#section-15.5.1",
  "title": "One or more validation errors occurred.",
  "status": 400,
  "errors": {
    "$": [
      "JSON deserialization for type 'WebApi.Models.Dtos.CourseCreate' was missing required properties including: 'title'."
    ],
    "courseToCreate": [
      "The courseToCreate field is required."
    ]
  },
  "traceId": "00-c6c0ac26f6f9f48c53417bfc8fb1efe8-07ff38f578cceb84-00"
}
```

改為使用 Data Annotations 驗證

```csharp
[Required(ErrorMessage = "The Title field is required.")]
public string Title { get; set; }
```

錯誤訊息

```json
{
  "type": "https://tools.ietf.org/html/rfc9110#section-15.5.1",
  "title": "One or more validation errors occurred.",
  "status": 400,
  "errors": {
    "Title": [
      "The Title field is required."
    ]
  },
  "traceId": "00-cde3489cf40cfe1168d98b6a2bc74daa-e46d13872c34499d-00"
}
```

DTO 類別可繼承 `IValidatableObject` 介面，並實作 `Validate` 方法，來自訂驗證邏輯

```csharp
public class CourseCreate : IValidatableObject
{
    public int CourseId { get; set; }

    [Required(ErrorMessage = "The Title field is required.")]
    public string Title { get; set; }

    [Required(ErrorMessage = "The Credits field is required.")]
    [Range(0, 5, ErrorMessage = "The Credits field must be between 0 and 5.")]
    public int Credits { get; set; }

    public IEnumerable<ValidationResult> Validate(ValidationContext validationContext)
    {
        if (CourseId != 0)
        {
            yield return new ValidationResult("The CourseId field must required 0.", [nameof(CourseId)]);
        }

        yield return ValidationResult.Success!;
    }
}
```


### **Action Filter**

Action Filter 為終端框架的一部分，用於在執行 Action 之前或之後執行程式碼。Action Filter 通常用於執行共用的功能，例如驗證輸入、記錄請求、檢查權限等。

Action Filter 有四種類型：

- Authorization Filter：在執行 Action 之前執行，用於驗證請求。
- Resource Filter：在執行 Action 之前和之後執行，用於執行共用的功能。
- Action Filter：在執行 Action 之前和之後執行，用於執行共用的功能。
- Exception Filter：在發生例外時執行，用於處理例外。

以下是一個簡單的 Action Filter 範例

```csharp
public class XXXXAttribute: ActionFilterAttribute
{
    public override void OnActionExecuting(ActionExecutingContext context)
    {
        // 在執行 Action 之前執行   
    }

    public override void OnActionExecuted(ActionExecutedContext context)
    {
        // 在執行 Action 之後執行
    }
}
```

全域註冊，所有 Controller 都會套用

```csharp
builder.Services.AddControllers(options =>
{
    options.Filters.Add<XXXXAttribute>();
});
```


### **`/Error` 錯誤處理**

透過 `UseExceptionHandler` 中介軟體來處理例外狀況

```csharp
app.UseExceptionHandler("/Error");
```

新增 `ErrorController` 來處理例外狀況

需設定 `ApiExplorerSettings` 屬性，讓 Swagger UI 可以正確顯示

透過 `Problem` 遵循 RFC 7807 (Update RFC 9457) 標準，回應錯誤訊息

這裡取到的 `HttpContext.Request.Path` 會是 `/Error`，而不是原本的路由

```csharp
[Route("[controller]")]
public class ErrorController : ControllerBase
{
    [ApiExplorerSettings(IgnoreApi = true)]
    public IActionResult Error()
    {
        var context = HttpContext.Features.Get<IExceptionHandlerFeature>();
        var exception = context.Error;

        return Problem(
            detail: exception.StackTrace,
            title: exception.Message,
            statusCode: StatusCodes.Status500InternalServerError
        );
    }
}
```


### **實作 `IExceptionFilter` 介面的錯誤處理**

透過實作 `IExceptionFilter` 介面來自訂全域錯誤處理

```csharp
internal sealed class GlobalExceptionHandler : IExceptionHandler
{
    public async ValueTask<bool> TryHandleAsync(
        HttpContext httpContext, 
        Exception exception, 
        CancellationToken cancellationToken)
    {
        // 若回傳 true 表示已處理例外，不會再往下傳遞
        // 若回傳 false 表示未處理例外，會繼續往下傳遞
        return true;
    }
}
```

註冊全域錯誤處理，需帶入 Delegate 但不需要實作

```csharp
app.UseExceptionHandler(o => {});
```

可搭配 `IProblemDetailsService` 介面來標準化錯誤回應的服務

它的目標是提供一個簡單的方式來返回 RFC 7807 標準格式的 ProblemDetails，用於 HTTP API 錯誤回應。

```csharp
internal sealed class GlobalExceptionHandler(IProblemDetailsService problemDetailsService) : IExceptionHandler
{
    public async ValueTask<bool> TryHandleAsync(
        HttpContext httpContext, 
        Exception exception, 
        CancellationToken cancellationToken)
    {

        return await problemDetailsService.TryWriteAsync(new ProblemDetailsContext
        {
            HttpContext = httpContext,
            Exception = exception,
            ProblemDetails = new ProblemDetails
            {
                Title = "error",
                Detail = "An unexpected error occurred.",
                Status = StatusCodes.Status500InternalServerError,
                Type = "https://tools.ietf.org/html/rfc7807#section-3.1"
            }
        });
    }
}
```

### **透過註冊 AddProblemDetails 服務來自訂全域錯誤回應屬性**

```csharp
builder.Services.AddProblemDetails(option =>
{
    option.CustomizeProblemDetails = (context) =>
    {
        context.ProblemDetails.Instance = $"{context.HttpContext.Request.Method} {context.HttpContext.Request.Path}";
        context.ProblemDetails.Extensions.TryAdd("requestId", context.HttpContext.TraceIdentifier);
        
        var activity = context.HttpContext.Features.Get<IHttpActivityFeature>()?.Activity;
        context.ProblemDetails.Extensions.TryAdd("traceId", activity?.Id);
    };
});
```


### **CORS**

不同源的請求在瀏覽器會被擋下，可以透過跨域來源存取來解決此問題

不同源條件

* 主機名稱不同
* 協定不同
* 埠號不同

```csharp
builder.Services.AddCors(option =>
{
    option.AddDefaultPolicy(policy =>
    {
        policy.WithOrigins("http://localhost:3000", "https://localhost:3001")
            .AllowAnyHeader()
            .AllowAnyMethod();
    });
});
```

註冊 Middleware

```csharp
app.UseCors();
```