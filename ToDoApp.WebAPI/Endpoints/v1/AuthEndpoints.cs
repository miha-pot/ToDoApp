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

        return authResult.ToHttpResult();
    }

    [AllowAnonymous]
    public static async Task<IResult> Login(LoginRequest? loginRequest,
                                            IIdentityService identityService,
                                            HttpContext httpContext)
    {
        ServiceResult<AuthResponse> authResult = await identityService.LoginAsync(loginRequest!);

        return authResult.ToHttpResult();
    }

    public static async Task<IResult> Logout(TokenRequest token,
                                             IIdentityService identityService,
                                             HttpContext httpContext)
    {

        await identityService.Logout(token);

        return Results.NoContent();
    }

    public static async Task<IResult> RefreshToken(TokenRequest? tokenRequest,
                                                   IIdentityService identityService,
                                                   HttpContext httpContext)
    {
        var result = await identityService.RefreshTokenAsync(tokenRequest!);

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
}
