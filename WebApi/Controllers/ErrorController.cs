using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Http.Features;

namespace WebApi.Controllers;

[ApiVersion("1")]
[ApiVersion("2")]
[Route("[controller]")]
[ApiController]
public class ErrorController : ControllerBase
{
    private readonly ILogger<ErrorController> _logger;
    private readonly IHostEnvironment _env;

    public ErrorController(ILogger<ErrorController> logger, IHostEnvironment env)
    {
        _logger = logger;
        _env = env;
    }

    [ApiExplorerSettings(IgnoreApi = true)]
    public IActionResult HandleError()
    {
        var context = HttpContext.Features.Get<IExceptionHandlerPathFeature>();

        if (context == null)
        {
            return Problem(
                type: "tag:example@example.org,2021-09-17:OutOfLuck",
                title: "Unexpected Error",
                detail: "An unexpected error occurred.",
                statusCode: StatusCodes.Status500InternalServerError
            );
        }

        var exception = context.Error;
        var traceId = Activity.Current?.Id ?? HttpContext.TraceIdentifier;
        
        var statusCode = GetStatusCode(exception);
        var httpMethod = HttpContext.Request.Method;
        var httpPath = HttpContext.Request.Path;

        // 加入 狀態碼 與 追蹤識別碼 與 Http Method 與 Http Path 到日誌
        _logger.LogError(exception, 
            "An unexpected error occurred. StatusCode: {StatusCode} TraceId: {TraceId} HttpMethod: {HttpMethod} HttpPath: {HttpPath}", 
            statusCode, traceId, httpMethod, httpPath);

        var problemDetails = new ProblemDetails
        {
            Type = $"https://httpstatuses.com/{statusCode}",
            Title = GetErrorTitle(statusCode),
            Status = statusCode,
            Instance = httpPath,
            Detail = _env.IsDevelopment() ? exception.Message : "An unexpected error occurred.",
        };

        problemDetails.Extensions.Add("traceId", traceId);

        return new ObjectResult(problemDetails)
        {
            StatusCode = statusCode
        };
    }

    private static int GetStatusCode(Exception exception)
    {
        return exception switch
        {
            ValidationException => StatusCodes.Status400BadRequest,
            UnauthorizedAccessException => StatusCodes.Status401Unauthorized,
            KeyNotFoundException => StatusCodes.Status404NotFound,
            BadHttpRequestException => StatusCodes.Status400BadRequest,
            InvalidOperationException => StatusCodes.Status409Conflict,

            _ => StatusCodes.Status500InternalServerError
        };
    }

    private static string GetErrorTitle(int statusCode) => statusCode switch
    {
        400 => "Bad Request",
        401 => "Unauthorized",
        404 => "Not Found",
        500 => "Internal Server Error",
        _ => "Error"
    };
    
}
