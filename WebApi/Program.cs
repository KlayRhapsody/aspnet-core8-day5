using Microsoft.AspNetCore.Http.Features;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddCors(option =>
{
    option.AddDefaultPolicy(policy =>
    {
        policy.WithOrigins("http://localhost:3000", "https://localhost:3001")
            .AllowAnyHeader()
            .AllowAnyMethod();
            // .AllowAnyOrigin();
    });
});

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
builder.Services.AddExceptionHandler<GlobalExceptionHandler>();

// Add services to the container.
builder.Services.AddControllers(option =>
{
    option.Filters.Add<RecordExectionTimeAttribute>();
});

// Learn more about configuring OpenAPI at https://aka.ms/aspnet/openapi
builder.Services.AddOpenApi("v1", option =>
{
    option.AddDocumentTransformer(new CustomDocumentTransformer(option.DocumentName));
});

builder.Services.AddOpenApi("v2", option =>
{
    option.AddDocumentTransformer(new CustomDocumentTransformer(option.DocumentName));
});

builder.Services.AddApiVersioning(option =>
{
    option.DefaultApiVersion = new ApiVersion(1, 0);
    option.AssumeDefaultVersionWhenUnspecified = true;
    option.ReportApiVersions = true;
    // option.ApiVersionReader = new UrlSegmentApiVersionReader();
    option.ApiVersionReader = ApiVersionReader.Combine(
        new HeaderApiVersionReader("X-ApiVersion"),
        new QueryStringApiVersionReader("api-version"),
        new UrlSegmentApiVersionReader());
})
.AddMvc()
.AddApiExplorer(option =>
{
    option.GroupNameFormat = "'v'V";
    option.SubstituteApiVersionInUrl = true;
});
// builder.Services.ConfigureOptions<ConfigureSwaggerGenOptions>();

builder.Services.AddDbContext<ContosoUniversityContext>(
        options => options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));


var app = builder.Build();

app.UseExceptionHandler();

app.UseCors();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
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
}

// app.UseStatusCodePages();

app.UseHttpsRedirection();

app.UseAuthorization();

app.MapControllers();

app.Run();
