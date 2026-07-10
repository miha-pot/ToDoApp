using Microsoft.AspNetCore.Http;

namespace ToDoApp.Infrastructure.Extensions;

public static class HttpContextClientExtensions
{
    private const string MobileClientHeaderValue = "mobile";

    public static bool IsMobileClient(this HttpContext httpContext) =>
        httpContext.Request.Headers["X-Client-Type"].ToString() == MobileClientHeaderValue;
}
