# ASP.NET Core 8 開發實戰：應用開發篇 (Web API)


### **開啟 IncludeOpenAPIAnalyzers 檢查 OpenAPI 是否完整**

開啟專案檔案 `*.csproj`，並加入下列程式碼

```xml
<IncludeOpenAPIAnalyzers>true</IncludeOpenAPIAnalyzers>
```

執行 dotnet build 時，會檢查 OpenAPI 是否完整

```cmd
warning API1000: Action method returns undeclared status code '404'
```

透過添加 `ProducesResponseType` 屬性，來宣告回應狀態碼

```csharp
[HttpGet("{id}", Name = "取得指定課程Async")]
[ProducesResponseType(StatusCodes.Status200OK)]
[ProducesResponseType(StatusCodes.Status404NotFound)]
[ProducesDefaultResponseType]
public async Task<ActionResult<CourseRead>> GetCourse(int id)
{
}
```

.NET 8 以上可以使用範型，來宣告回應類型

```csharp
[ProducesResponseType<PageCourse>(StatusCodes.Status200OK)]
```

### **.NET 9 以上版本，需要手動安裝回 Swashbuckle 並設定 Swagger**

```bash
dotnet add package Swashbuckle.AspNetCore
```

```csharp
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI(); // 提供可視化的 Swagger UI
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

