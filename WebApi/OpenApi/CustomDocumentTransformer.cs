namespace WebApi.OpenApi;
public class CustomDocumentTransformer : IOpenApiDocumentTransformer
{
    private readonly string _version;


    public CustomDocumentTransformer(string version)
    {
        _version = version;
    }

    public Task TransformAsync(OpenApiDocument document, OpenApiDocumentTransformerContext context, CancellationToken cancellationToken)
    {
        document.Info = new()
        {
            Title = "Contoso University API",
            Version = _version,
            Description = "API for processing Contoso University data",
        };

        return Task.CompletedTask;
    }
}
