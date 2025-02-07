


namespace WebApi.Filters;

public class RecordExectionTimeAttribute: ActionFilterAttribute
{
    public override void OnActionExecuting(ActionExecutingContext context)
    {
        // 開啟紀錄執行時間
        context.HttpContext.Items["Stopwatch"] = Stopwatch.StartNew();
    }

    public override void OnActionExecuted(ActionExecutedContext context)
    {
        context.HttpContext.Items.TryGetValue("Stopwatch", out var stopwatch);
        if (stopwatch is Stopwatch sw)
        {
            sw.Stop();
            var elapsed = sw.ElapsedMilliseconds;
            Console.WriteLine($"Action {context.ActionDescriptor.DisplayName} executed in {elapsed} ms");
        }
    }
}