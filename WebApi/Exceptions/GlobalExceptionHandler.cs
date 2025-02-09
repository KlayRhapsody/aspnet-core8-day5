namespace WebApi.Exceptions;

[Serializable]
public class ProblemException(int statusCode, string message) : Exception(message)
{
    public int StatusCode { get; } = statusCode;
    public string Message { get; } = message;
}

public class CustomValidationException(string message) : ProblemException(StatusCodes.Status400BadRequest, message)
{
}


internal sealed class GlobalExceptionHandler(
    IProblemDetailsService problemDetailsService, 
    IHostEnvironment env
    ) : IExceptionHandler
{
    public async ValueTask<bool> TryHandleAsync(
        HttpContext httpContext, 
        Exception exception, 
        CancellationToken cancellationToken)
    {
        httpContext.Response.StatusCode = exception switch
        {
            ProblemException => StatusCodes.Status400BadRequest,
            _ => StatusCodes.Status500InternalServerError
        };

        bool isDev = env.IsDevelopment();

        return await problemDetailsService.TryWriteAsync(new ProblemDetailsContext
        {
            HttpContext = httpContext,
            Exception = exception,
            ProblemDetails = new ProblemDetails
            {
                Title = isDev ? $"{exception.GetType().Name}" : "error",
                Detail = isDev ? exception.Message : "An unexpected error occurred.",
            }
        });
    }
}