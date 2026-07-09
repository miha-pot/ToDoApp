using Microsoft.AspNetCore.Identity;
using System.Net;
using System.Security.Claims;
using ToDoApp.Application.ServiceContracts.Identity;
using ToDoApp.Domain.Identity;
using ToDoApp.Shared.AuthDTO;
using ToDoApp.Shared.AuthDTO.Token;
using ToDoApp.Shared.Common;

namespace ToDoApp.Infrastructure.Services;

public class TokenService : ITokenService
{
    private readonly IJwtService _jwtService;
    private readonly UserManager<ApplicationUser> _userManager;

    public TokenService(IJwtService jwtService,
                        UserManager<ApplicationUser> userManager)
    {
        _jwtService = jwtService;
        _userManager = userManager;
    }

    public async Task<AuthResponse> CreateAuthResponseAsync(ApplicationUser? user)
    {
        AuthResponse authResponse = _jwtService.CreateJwtToken(user!);

        user!.RefreshToken = authResponse.RefreshToken;
        user.RefreshTokenExpirationDateTime = authResponse.RefreshTokenExpiration;

        await _userManager.UpdateAsync(user);

        return authResponse;
    }

    public async Task<ServiceResult<AuthResponse>> RefreshSessionAsync(TokenRequest tokenRequest)
    {
        ClaimsPrincipal? principal = _jwtService.GetPrincipalFromJwtToken(tokenRequest.Token);

        if (principal == null)
        {
            return ServiceResult<AuthResponse>.Failure("User not found!",
                                                       "",
                                                       HttpStatusCode.NotFound);
        }

        var email = principal.FindFirstValue(ClaimTypes.Email);

        if (string.IsNullOrEmpty(email))
            return ServiceResult<AuthResponse>.Failure("Invalid Token",
                                                       "",
                                                       HttpStatusCode.BadRequest);

        var user = await _userManager.FindByEmailAsync(email);

        if (user == null || IsTokenNotValid(user, tokenRequest.RefreshToken!))
        {
            return ServiceResult<AuthResponse>.Failure("Invalid Session",
                                                       "Token not valid!",
                                                       HttpStatusCode.BadRequest);
        }

        var authResponse = await CreateAuthResponseAsync(user);
        return ServiceResult<AuthResponse>.Success(authResponse);
    }

    public async Task<ServiceResult<AuthResponse>> RevokeRefreshTokenAsync(TokenRequest token)
    {
        ClaimsPrincipal? principal = _jwtService.GetPrincipalFromJwtToken(token.Token);

        if (principal == null)
        {
            return ServiceResult<AuthResponse>.Failure("User not found!",
                                                       "",
                                                       HttpStatusCode.NotFound);
        }

        var email = principal.FindFirstValue(ClaimTypes.Email);

        if (string.IsNullOrEmpty(email))
        {
            return ServiceResult<AuthResponse>.Failure("Invalid Token",
                                                       "",
                                                       HttpStatusCode.BadRequest);
        }

        var user = await _userManager.FindByEmailAsync(email);
        if (user is not null)
        {
            user.RefreshToken = null;
            user.RefreshTokenExpirationDateTime = DateTime.MinValue;

            await _userManager.UpdateAsync(user);
        }

        return ServiceResult<AuthResponse>.Success(new AuthResponse(), HttpStatusCode.NoContent);
    }

    private static bool IsTokenNotValid(ApplicationUser? user, string refreshToken) =>
        user == null
        || user.RefreshToken != refreshToken
        || user.RefreshTokenExpirationDateTime <= DateTime.UtcNow;
}
