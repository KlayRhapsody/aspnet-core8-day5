
namespace WebApi.OpenApi;
public class CustomDocumentTransformer(string version) : IOpenApiDocumentTransformer
{

    public Task TransformAsync(OpenApiDocument document, OpenApiDocumentTransformerContext context, CancellationToken cancellationToken)
    {
        document.Info = new()
        {
            Title = "Contoso University API",
            Version = version,
            Description = "API for processing Contoso University data",
        };

        return Task.CompletedTask;
    }
}
