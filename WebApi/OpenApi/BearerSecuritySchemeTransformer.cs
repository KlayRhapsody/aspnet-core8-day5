using Microsoft.AspNetCore.Authentication.JwtBearer;

namespace WebApi.OpenApi;

public sealed class BearerSecuritySchemeTransformer(IAuthenticationSchemeProvider authenticationSchemeProvider) : IOpenApiDocumentTransformer
{
    public async Task TransformAsync(OpenApiDocument document, OpenApiDocumentTransformerContext context, CancellationToken cancellationToken)
    {
        var authenticationSchemes = await authenticationSchemeProvider.GetAllSchemesAsync();
        if (authenticationSchemes.Any(authScheme => authScheme.Name == "Bearer"))
        {
            var requirements = new Dictionary<string, OpenApiSecurityScheme>
            {
                ["Bearer"] = new OpenApiSecurityScheme
                {
                    Name = "JWT Authentication",
                    Description = "Enter Your JWT token",
                    Type = SecuritySchemeType.Http,
                    Scheme = JwtBearerDefaults.AuthenticationScheme, // "bearer" refers to the header name here
                    In = ParameterLocation.Header,
                    BearerFormat = "Json Web Token",
                }
            };
            document.Components ??= new OpenApiComponents();
            document.Components.SecuritySchemes = requirements;

            // Apply it as a requirement for all operations
            var docPathItems = document.Paths
                .Where(p => p.Key.StartsWith("/api/users", StringComparison.OrdinalIgnoreCase));
            
            foreach (var operation in docPathItems.SelectMany(path => path.Value.Operations))
            {
                OpenApiSecurityRequirement securityRequirement = new()
                {
                    {
                        new OpenApiSecurityScheme
                        {
                            Reference = new OpenApiReference
                            {
                                Type = ReferenceType.SecurityScheme,
                                Id = "Bearer"
                            }
                        },
                        Array.Empty<string>()
                    }
                };
                operation.Value.Security.Add(securityRequirement);
            }

            // foreach (var operation in document.Paths.Values.SelectMany(path => path.Operations))
            // {
            //     OpenApiSecurityRequirement securityRequirement = new()
            //     {
            //         {
            //             new OpenApiSecurityScheme
            //             {
            //                 Reference = new OpenApiReference
            //                 {
            //                     Type = ReferenceType.SecurityScheme,
            //                     Id = "Bearer"
            //                 }
            //             },
            //             Array.Empty<string>()
            //         }
            //     };
            //     operation.Value.Security.Add(securityRequirement);
            //     // operation.Value.Security.Add(new OpenApiSecurityRequirement
            //     // {
            //     //     [new OpenApiSecurityScheme { Reference = new OpenApiReference { Id = "Bearer", Type = ReferenceType.SecurityScheme } }] = Array.Empty<string>()
            //     // });
            // }
        }
    }
}