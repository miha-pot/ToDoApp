using Microsoft.AspNetCore.Http;
using ToDoApp.Application.ServiceContracts.Identity;

namespace ToDoApp.Infrastructure.Services;

/// <summary>
/// Single place that knows how the refresh-token cookie is named, scoped and shaped.
/// HttpContext access is resolved internally via IHttpContextAccessor, so the
/// IRefreshTokenCookieService contract itself stays free of ASP.NET Core Http types
/// (keeps it usable from Application-layer service contracts).
/// </summary>
public class RefreshTokenCookieService : IRefreshTokenCookieService
{
    private const string CookieName = "refreshToken";
    private const string CookiePath = "/api/v1/auth";

    private readonly IHttpContextAccessor _httpContextAccessor;

    public RefreshTokenCookieService(IHttpContextAccessor httpContextAccessor)
    {
        _httpContextAccessor = httpContextAccessor;
    }

    public void Append(string refreshToken, DateTime expiration)
    {
        var httpContext = _httpContextAccessor.HttpContext;
        if (httpContext is null) return;

        httpContext.Response.Cookies.Append(CookieName, refreshToken, new CookieOptions
        {
            HttpOnly = true,
            Secure = true,
            SameSite = SameSiteMode.Strict,
            Path = CookiePath,
            Expires = expiration
        });
    }

    public bool TryGet(out string? refreshToken)
    {
        refreshToken = null;
        var httpContext = _httpContextAccessor.HttpContext;
        return httpContext is not null && httpContext.Request.Cookies.TryGetValue(CookieName, out refreshToken);
    }

    public void Delete()
    {
        var httpContext = _httpContextAccessor.HttpContext;
        httpContext?.Response.Cookies.Delete(CookieName, new CookieOptions { Path = CookiePath });
    }
}