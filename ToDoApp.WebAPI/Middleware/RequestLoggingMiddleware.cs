namespace ToDoApp.WebAPI.Middleware;

public class RequestLoggingMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ILogger<RequestLoggingMiddleware> _logger;

    public RequestLoggingMiddleware(RequestDelegate next, ILogger<RequestLoggingMiddleware> logger)
    {
        _next = next;
        _logger = logger;
    }

    public async Task InvokeAsync(HttpContext context)
    {
        var watch = System.Diagnostics.Stopwatch.StartNew();

        _logger.LogInformation("➡️  {Method} {Path} {QueryString}",
            context.Request.Method,
            context.Request.Path,
            context.Request.QueryString);

        await _next(context);

        watch.Stop();

        var level = context.Response.StatusCode >= 400
            ? LogLevel.Warning
            : LogLevel.Information;

        _logger.Log(level, "{StatusCode} {Method} {Path} ({Elapsed}ms)",
            context.Response.StatusCode,
            context.Request.Method,
            context.Request.Path,
            watch.ElapsedMilliseconds);
    }
}
