using ToDoApp.WebAPI.Middleware;

namespace ToDoApp.WebAPI.Extensions;

public static class RequestLoggingExtensions
{
    public static IApplicationBuilder UseRequestLogging(this IApplicationBuilder app)
        => app.UseMiddleware<RequestLoggingMiddleware>();
}
