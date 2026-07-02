using Microsoft.AspNetCore.Authorization;
using System.Net;
using ToDoApp.Application.ServiceContracts.Identity;
using ToDoApp.Shared.AuthDTO;
using ToDoApp.Shared.AuthDTO.ForgotPassword;
using ToDoApp.Shared.AuthDTO.Login;
using ToDoApp.Shared.AuthDTO.Register;
using ToDoApp.Shared.AuthDTO.ResetPassword;
using ToDoApp.Shared.AuthDTO.Token;
using ToDoApp.Shared.Common;
using ToDoApp.WebAPI.Extensions;
using ToDoApp.WebAPI.Filters;

namespace ToDoApp.WebAPI.Endpoints.v1;

public static class AuthEndpoints
{
    public static IEndpointRouteBuilder MapAuthEndpoints(this IEndpointRouteBuilder app)
    {
        var authGroup = app.MapGroup("/auth").MapToApiVersion(1, 0).RequireRateLimiting("api-policy");

        authGroup.MapPost("/register", Register)
            .Produces<AuthResponse>((int)HttpStatusCode.OK)
            .Produces<string>((int)HttpStatusCode.BadRequest)
            .AddEndpointFilter<ValidationFilter<RegisterRequest>>();

        authGroup.MapPost("/login", Login)
            .AddEndpointFilter<ValidationFilter<LoginRequest>>()
            .RequireAuthorization();

        authGroup.MapPost("/refresh", RefreshToken)
            .AddEndpointFilter<ValidationFilter<TokenRequest>>();

        authGroup.MapPost("/logout", Logout)
            .RequireAuthorization();

        authGroup.MapPost("/reset-password", ResetPassword)
            .AddEndpointFilter<ValidationFilter<ResetPassRequest>>();

        authGroup.MapPost("/forgot-password", ForgotPassword)
            .AddEndpointFilter<ValidationFilter<ForgotPassRequest>>();

        return app;
    }

    [AllowAnonymous]
    public static async Task<IResult> Register(RegisterRequest? registerRequest,
                                               IIdentityService identityService,
                                               HttpContext httpContext)
    {
        ServiceResult<AuthResponse> authResult = await identityService.RegisterAsync(registerRequest!);

        if (authResult.IsSuccess && authResult.Value is not null)
        {
            AddRefreshTokenToCookie(httpContext, authResult.Value.RefreshToken);
        }

        return authResult.ToHttpResult();
    }

    [AllowAnonymous]
    public static async Task<IResult> Login(LoginRequest? loginRequest,
                                            IIdentityService identityService,
                                            HttpContext httpContext)
    {
        ServiceResult<AuthResponse> authResult = await identityService.LoginAsync(loginRequest!);

        if (authResult.IsSuccess && authResult.Value is not null)
        {
            AddRefreshTokenToCookie(httpContext, authResult.Value.RefreshToken);
        }

        return authResult.ToHttpResult();
    }

    public static async Task<IResult> Logout(TokenRequest token,
                                             IIdentityService identityService,
                                             HttpContext httpContext)
    {
        if (httpContext.Request.Cookies.TryGetValue("refreshToken", out var refreshToken))
        {
            token.RefreshToken = refreshToken;

            await identityService.Logout(token);
        }

        httpContext.Response.Cookies.Delete("refreshToken", new CookieOptions
        {
            Path = "/api/v1/auth"
        });

        return Results.NoContent();
    }

    public static async Task<IResult> RefreshToken(TokenRequest? tokenRequest,
                                                   IIdentityService identityService,
                                                   HttpContext httpContext)
    {
        if (!httpContext.Request.Cookies.TryGetValue("refreshToken", out var refreshToken))
            return Results.Unauthorized();

        tokenRequest!.RefreshToken = refreshToken;

        ServiceResult<AuthResponse> result = await identityService.RefreshTokenAsync(tokenRequest);

        if (!result.IsSuccess || result.Value is null)
        {
            return Results.Problem(title: result.ErrorTitle,
                                  detail: result.ErrorDetail,
                                  statusCode: (int)result.StatusCode);
        }

        AddRefreshTokenToCookie(httpContext, result.Value.RefreshToken);

        result.Value.RefreshToken = string.Empty;

        return result.ToHttpResult();
    }

    public static async Task<IResult> ForgotPassword(ForgotPassRequest? forgotPassRequest,
                                                     IIdentityService identityService)
    {
        ServiceResult<string> authResult = await identityService.ForgotPasswordAsync(forgotPassRequest!);

        return authResult.ToHttpResult();
    }

    public static async Task<IResult> ResetPassword(ResetPassRequest? resetPassRequest,
                                                    IIdentityService identityService)
    {
        ServiceResult<string> authResult = await identityService.ResetPasswordAsync(resetPassRequest!);

        return authResult.ToHttpResult();
    }

    private static void AddRefreshTokenToCookie(HttpContext httpContext, string refreshToken)
    {
        httpContext.Response.Cookies.Append("refreshToken", refreshToken, new CookieOptions
        {
            HttpOnly = true,
            Secure = true,
            SameSite = SameSiteMode.Strict,
            Path = "/api/v1/auth",
            Expires = DateTimeOffset.UtcNow.AddDays(30)
        });
    }
}
