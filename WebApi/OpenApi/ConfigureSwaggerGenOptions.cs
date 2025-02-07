
// using Asp.Versioning.ApiExplorer;
// using Microsoft.Extensions.Options;
// using Microsoft.OpenApi.Models;
// using Swashbuckle.AspNetCore.SwaggerGen;

// namespace WebApi.OpenApi;

// public class ConfigureSwaggerGenOptions : IConfigureNamedOptions<SwaggerGenOptions>
// {
//     private readonly IApiVersionDescriptionProvider _provider;

//     public ConfigureSwaggerGenOptions(IApiVersionDescriptionProvider provider)
//     {
//         _provider = provider;
//     }

//     public void Configure(string? name, SwaggerGenOptions options)
//     {
//         Configure(options);
//     }

//     public void Configure(SwaggerGenOptions options)
//     {
//         foreach (var description in _provider.ApiVersionDescriptions)
//         {
//             var openApiInfo = new OpenApiInfo
//             {
//                 Title = $"WAHHA WebApi v{description.ApiVersion}",
//                 Version = description.ApiVersion.ToString()
//             };

//             options.SwaggerDoc(description.GroupName, openApiInfo);
//         }
//     }
// }
