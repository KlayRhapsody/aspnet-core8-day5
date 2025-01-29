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
