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

namespace ToDoApp.Infrastructure.Services;

public class IdentityService : IIdentityService
{
    private readonly SignInManager<ApplicationUser> _signInManager;
    private readonly UserManager<ApplicationUser> _userManager;

    private readonly IHttpContextAccessor _httpContextAccessor;
    private readonly ITokenService _tokenService;

    public IdentityService(SignInManager<ApplicationUser> signInManager,
                           UserManager<ApplicationUser> userManager,
                           ITokenService tokenService,
                           IHttpContextAccessor httpContext)
    {
        _signInManager = signInManager;
        _userManager = userManager;
        _tokenService = tokenService;
        _httpContextAccessor = httpContext;
    }

    public async Task<ServiceResult<AuthResponse>> RegisterAsync(RegisterRequest registerRequest)
    {
        var isMobile = IsRequestFromMobile(_httpContextAccessor.HttpContext);

        var foundUser = await _userManager.FindByEmailAsync(registerRequest.Email);
        if (foundUser != null)
        {
            return ServiceResult<AuthResponse>.Failure("Mail taken!",
                                                       "User with that email already exists!",
                                                       HttpStatusCode.Conflict);
        }

        ApplicationUser user = registerRequest.ToApplicationUser();

        IdentityResult result = await _userManager.CreateAsync(user, registerRequest.Password);
        if (!result.Succeeded)
        {
            return ServiceResult<AuthResponse>.Failure("Registration error!",
                                                       string.Join(", ", result.Errors),
                                                       HttpStatusCode.BadRequest);
        }

        await _signInManager.SignInAsync(user, isPersistent: true);

        AuthResponse authResponse = await _tokenService.CreateAuthResponseAsync(user);

        if (!isMobile)
        {
            AddRefreshTokenToCookie(_httpContextAccessor.HttpContext, authResponse.RefreshToken);
        }

        return ServiceResult<AuthResponse>.Success(authResponse);
    }

    public async Task<ServiceResult<AuthResponse>> LoginAsync(LoginRequest loginRequest)
    {
        var isMobile = IsRequestFromMobile(_httpContextAccessor.HttpContext);


        var user = await _userManager.FindByEmailAsync(loginRequest.Email);
        if (user is null)
        {
            return ServiceResult<AuthResponse>.Failure("Login error!",
                                                       "Invalid email or password parameters.",
                                                       HttpStatusCode.BadRequest);
        }

        SignInResult result = await _signInManager.CheckPasswordSignInAsync(user,
                                                                            loginRequest.Password,
                                                                            lockoutOnFailure: true);

        if (result.IsLockedOut)
        {
            return ServiceResult<AuthResponse>.Failure("Login error!",
                                                       "This account is temporarily locked out.",
                                                       HttpStatusCode.BadRequest);
        }

        if (!result.Succeeded)
        {
            return ServiceResult<AuthResponse>.Failure("Login error!",
                                                       "Invalid email or password parameters.",
                                                       HttpStatusCode.BadRequest);
        }

        await _userManager.ResetAccessFailedCountAsync(user);

        AuthResponse authResponse = await _tokenService.CreateAuthResponseAsync(user);

        if (!isMobile)
        {
            AddRefreshTokenToCookie(_httpContextAccessor.HttpContext, authResponse.RefreshToken);
        }

        return ServiceResult<AuthResponse>.Success(authResponse);
    }

    public async Task<ServiceResult<AuthResponse>> RefreshTokenAsync(TokenRequest tokenRequest)
    {
        var httpContext = _httpContextAccessor.HttpContext;
        string? accessToken = tokenRequest.Token;
        string? refreshToken = null;

        // 1. Poskusi prebrati refreshToken iz piškotkov (Web)
        if (httpContext != null)
        {
            httpContext.Request.Cookies.TryGetValue("refreshToken", out refreshToken);
        }

        // 2. Če piškotka ni, vzemi tistega iz telesa (MAUI fallback)
        if (string.IsNullOrWhiteSpace(refreshToken))
        {
            refreshToken = tokenRequest.RefreshToken;
        }

        // 3. Validacija prisotnosti
        if (string.IsNullOrWhiteSpace(accessToken) || string.IsNullOrWhiteSpace(refreshToken))
        {
            return ServiceResult<AuthResponse>.Failure("Missing tokens", "Access token or refresh token was not provided.");
        }

        TokenRequest newTokenRequest = new() { Token = accessToken, RefreshToken = refreshToken };

        // 4. Izvedba dejanske osvežitve (tvoja obstoječa jedrna logika)
        var refreshResult = await _tokenService.RefreshSessionAsync(newTokenRequest);

        if (!refreshResult.IsSuccess || refreshResult.Value is null)
        {
            return refreshResult;
        }

        // 5. Če je zahtevek prišel preko piškotkov (Web), avtomatsko posodobi piškotek
        if (httpContext != null && httpContext.Request.Cookies.ContainsKey("refreshToken"))
        {
            AddRefreshTokenToCookie(httpContext, refreshResult.Value.RefreshToken);
        }

        return refreshResult;
    }

    public async Task<ServiceResult<string>> ForgotPasswordAsync(ForgotPassRequest forgotPassRequest)
    {
        ApplicationUser? user = await _userManager.FindByEmailAsync(forgotPassRequest.Email!);

        if (user == null)
        {
            return ServiceResult<string>.Failure("Mail not found!",
                                                 "Mail was not found in the user database!",
                                                 HttpStatusCode.BadRequest);
        }

        string token = await _userManager.GeneratePasswordResetTokenAsync(user);

        Dictionary<string, string> param = new()
        {
            {"token", token},
            {"email", forgotPassRequest.Email!},
        };

        var callback = QueryHelpers.AddQueryString(forgotPassRequest.ClientUri!, param!);
        return ServiceResult<string>.Success(callback);
    }

    public async Task<ServiceResult<string>> ResetPasswordAsync(ResetPassRequest resetPassRequest)
    {
        ApplicationUser? currentUser;

        if (!string.IsNullOrEmpty(resetPassRequest.Email))
        {
            currentUser = await _userManager.FindByEmailAsync(resetPassRequest.Email);
        }
        else
        {
            currentUser = await _userManager.GetUserAsync(_httpContextAccessor.HttpContext.User);
        }

        if (currentUser == null)
        {
            return ServiceResult<string>.Failure("User not found!",
                                                 "User was not found!",
                                                 HttpStatusCode.BadRequest);
        }

        IdentityResult? result = await _userManager.ResetPasswordAsync(currentUser,
                                                                       resetPassRequest.Token!,
                                                                       resetPassRequest.NewPassword!);

        if (!result.Succeeded)
        {
            var resultErrors = string.Join('|', result.Errors.Select(x => x.Description));

            return ServiceResult<string>.Failure("Error reseting password",
                                                 resultErrors,
                                                 HttpStatusCode.BadRequest);
        }

        return ServiceResult<string>.Success("Your password was successfuly changed!", HttpStatusCode.Accepted);
    }

    public async Task Logout(TokenRequest tokenRequest)
    {
        var isMobile = IsRequestFromMobile(_httpContextAccessor.HttpContext);

        if (isMobile)
        {
            // MAUI — refreshToken pride v body (TokenRequest)
            if (!string.IsNullOrWhiteSpace(tokenRequest.RefreshToken))
            {
                await _tokenService.RevokeRefreshTokenAsync(tokenRequest);

                await _signInManager.SignOutAsync();
            }
        }
        else
        {
            // WASM — refreshToken pride iz cookieja
            if (_httpContextAccessor.HttpContext.Request.Cookies.TryGetValue("refreshToken", out var refreshToken))
            {
                tokenRequest.RefreshToken = refreshToken;
                await _tokenService.RevokeRefreshTokenAsync(tokenRequest);

                await _signInManager.SignOutAsync();
            }

            _httpContextAccessor.HttpContext.Response.Cookies.Delete("refreshToken", new CookieOptions
            {
                Path = "/api/v1/auth"
            });
        }
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

    private bool IsRequestFromMobile(HttpContext httpContext)
    {
        var clientType = httpContext.Request.Headers["X-Client-Type"].ToString();
        return clientType == "mobile";
    }
}
