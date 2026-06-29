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

    private readonly IHttpContextAccessor _httpContext;
    private readonly ITokenService _tokenService;

    public IdentityService(SignInManager<ApplicationUser> signInManager,
                           UserManager<ApplicationUser> userManager,
                           ITokenService tokenService,
                           IHttpContextAccessor httpContext)
    {
        _signInManager = signInManager;
        _userManager = userManager;
        _tokenService = tokenService;
        _httpContext = httpContext;
    }

    public async Task<ServiceResult<AuthResponse>> RegisterAsync(RegisterRequest registerRequest)
    {
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

        return ServiceResult<AuthResponse>.Success(authResponse);
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

        SignInResult result = await _signInManager.CheckPasswordSignInAsync(user, loginRequest.Password, lockoutOnFailure: true);

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

        return ServiceResult<AuthResponse>.Success(authResponse);
    }

    public async Task<ServiceResult<AuthResponse>> RefreshTokenAsync(TokenRequest tokenRequest)
    {
        return await _tokenService.RefreshSessionAsync(tokenRequest);
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

        //Has password reset been triggered for logged user or not
        if (!string.IsNullOrEmpty(resetPassRequest.Email))
        {
            currentUser = await _userManager.FindByEmailAsync(resetPassRequest.Email);
        }
        else
        {
            currentUser = await _userManager.GetUserAsync(_httpContext.HttpContext.User);
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

    public async Task Logout()
    {
        await _signInManager.SignOutAsync();
    }
}
