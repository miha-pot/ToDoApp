using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.WebUtilities;
using System.Net;
using ToDoApp.Application.ServiceContracts.Identity;
using ToDoApp.Domain.Identity;
using ToDoApp.Shared.AuthDTO;
using ToDoApp.Shared.AuthDTO.ForgotPassword;
using ToDoApp.Shared.AuthDTO.Login;
using ToDoApp.Shared.AuthDTO.Register;
using ToDoApp.Shared.AuthDTO.ResetPassword;
using ToDoApp.Shared.AuthDTO.Token;
using ToDoApp.Shared.Common;
using ToDoApp.Application.Mappers;
using ToDoApp.Infrastructure.Extensions;

namespace ToDoApp.Infrastructure.Services;

public class IdentityService : IIdentityService
{
    private readonly UserManager<ApplicationUser> _userManager;
    private readonly IHttpContextAccessor _httpContextAccessor;
    private readonly ITokenService _tokenService;
    private readonly IRefreshTokenCookieService _cookieService;

    public IdentityService(UserManager<ApplicationUser> userManager,
                           ITokenService tokenService,
                           IHttpContextAccessor httpContextAccessor,
                           IRefreshTokenCookieService cookieService)
    {
        _userManager = userManager;
        _tokenService = tokenService;
        _httpContextAccessor = httpContextAccessor;
        _cookieService = cookieService;
    }

    public async Task<ServiceResult<AuthResponse>> RegisterAsync(RegisterRequest registerRequest)
    {
        var existingUser = await _userManager.FindByEmailAsync(registerRequest.Email);
        if (existingUser is not null)
        {
            return ServiceResult<AuthResponse>.Failure("Mail taken!",
                                                       "User with that email already exists!",
                                                       HttpStatusCode.Conflict);
        }

        ApplicationUser user = registerRequest.ToApplicationUser();

        IdentityResult createResult = await _userManager.CreateAsync(user, registerRequest.Password);
        if (!createResult.Succeeded)
        {
            return ServiceResult<AuthResponse>.Failure("Registration error!",
                                                       string.Join(", ", createResult.Errors),
                                                       HttpStatusCode.BadRequest);
        }

        return await IssueAuthResponseAsync(user);
    }

    public async Task<ServiceResult<AuthResponse>> LoginAsync(LoginRequest loginRequest)
    {
        var user = await _userManager.FindByEmailAsync(loginRequest.Email);
        if (user is null)
        {
            return ServiceResult<AuthResponse>.Failure("Login error!",
                                                       "Invalid email or password parameters.",
                                                       HttpStatusCode.BadRequest);
        }

        bool result = await _userManager.CheckPasswordAsync(user, loginRequest.Password);
        if (!result)
        {
            return ServiceResult<AuthResponse>.Failure("Login error!",
                                                       "Invalid email or password parameters.",
                                                       HttpStatusCode.BadRequest);
        }

        await _userManager.ResetAccessFailedCountAsync(user);

        return await IssueAuthResponseAsync(user);
    }

    public async Task<ServiceResult<AuthResponse>> RefreshTokenAsync(TokenRequest tokenRequest)
    {
        var httpContext = _httpContextAccessor.HttpContext;

        string? cookieRefreshToken = null;
        bool hasRefreshCookie = httpContext is not null && _cookieService.TryGet(out cookieRefreshToken);
        var refreshToken = tokenRequest.RefreshToken;
        if (hasRefreshCookie)
        {
            refreshToken = cookieRefreshToken;
        }

        if (string.IsNullOrWhiteSpace(tokenRequest.Token) || string.IsNullOrWhiteSpace(refreshToken))
        {
            return ServiceResult<AuthResponse>.Failure("Missing tokens",
                                                       "Access token or refresh token was not provided.",
                                                       HttpStatusCode.BadRequest);
        }

        var refreshResult = await _tokenService.RefreshSessionAsync(new TokenRequest
        {
            Token = tokenRequest.Token,
            RefreshToken = refreshToken
        });

        if (refreshResult.IsSuccess && hasRefreshCookie)
        {
            _cookieService.Append(refreshResult.Value!.RefreshToken, refreshResult.Value.RefreshTokenExpiration);
        }

        return refreshResult;
    }

    public async Task<ServiceResult<string>> ForgotPasswordAsync(ForgotPassRequest forgotPassRequest)
    {
        ApplicationUser? user = await _userManager.FindByEmailAsync(forgotPassRequest.Email!);
        if (user is null)
        {
            return ServiceResult<string>.Failure("Mail not found!",
                                                 "Mail was not found in the user database!",
                                                 HttpStatusCode.BadRequest);
        }

        string token = await _userManager.GeneratePasswordResetTokenAsync(user);

        Dictionary<string, string> queryParams = new()
        {
            ["token"] = token,
            ["email"] = forgotPassRequest.Email!
        };

        var callbackUrl = QueryHelpers.AddQueryString(forgotPassRequest.ClientUri!, queryParams!);
        return ServiceResult<string>.Success(callbackUrl);
    }

    public async Task<ServiceResult<string>> ResetPasswordAsync(ResetPassRequest resetPassRequest)
    {
        ApplicationUser? user = !string.IsNullOrEmpty(resetPassRequest.Email)
            ? await _userManager.FindByEmailAsync(resetPassRequest.Email)
            : await _userManager.GetUserAsync(_httpContextAccessor.HttpContext?.User!);

        if (user is null)
        {
            return ServiceResult<string>.Failure("User not found!",
                                                 "User was not found!",
                                                 HttpStatusCode.BadRequest);
        }

        IdentityResult result = await _userManager.ResetPasswordAsync(user,
                                                                       resetPassRequest.Token!,
                                                                       resetPassRequest.NewPassword!);

        if (!result.Succeeded)
        {
            var resultErrors = string.Join('|', result.Errors.Select(e => e.Description));

            return ServiceResult<string>.Failure("Error reseting password",
                                                 resultErrors,
                                                 HttpStatusCode.BadRequest);
        }

        return ServiceResult<string>.Success("Your password was successfuly changed!", HttpStatusCode.Accepted);
    }

    public async Task<ServiceResult<object>> LogoutAsync(TokenRequest tokenRequest)
    {
        var httpContext = _httpContextAccessor.HttpContext;
        if (httpContext is null)
        {
            return ServiceResult<object>.Failure("Logout error!", "No active HTTP context.", HttpStatusCode.BadRequest);
        }

        var isMobile = httpContext.IsMobileClient();

        if (!isMobile && _cookieService.TryGet(out var cookieRefreshToken))
        {
            tokenRequest.RefreshToken = cookieRefreshToken;
        }

        if (!string.IsNullOrWhiteSpace(tokenRequest.RefreshToken))
        {
            await _tokenService.RevokeRefreshTokenAsync(tokenRequest);
        }

        if (!isMobile)
        {
            _cookieService.Delete();
        }

        return ServiceResult<object>.Success(new object(), HttpStatusCode.NoContent);
    }

    private async Task<ServiceResult<AuthResponse>> IssueAuthResponseAsync(ApplicationUser user)
    {
        var authResponse = await _tokenService.CreateAuthResponseAsync(user);

        var httpContext = _httpContextAccessor.HttpContext;
        if (httpContext is not null && !httpContext.IsMobileClient())
        {
            _cookieService.Append(authResponse.RefreshToken, authResponse.RefreshTokenExpiration);
        }

        return ServiceResult<AuthResponse>.Success(authResponse);
    }
}
