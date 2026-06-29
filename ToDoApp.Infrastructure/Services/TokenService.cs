using Microsoft.AspNetCore.Identity;
using System.Net;
using System.Security.Claims;
using ToDoApp.Application.ServiceContracts.Identity;
using ToDoApp.Domain.Identity;
using ToDoApp.Shared.AuthDTO;
using ToDoApp.Shared.AuthDTO.Token;
using ToDoApp.Shared.Common;

namespace ToDoApp.Application.Services.Identity;

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

    public async Task<ServiceResult<AuthResponse>> RefreshSessionAsync(TokenRequest tokenDTO)
    {
        ClaimsPrincipal? principal = _jwtService.GetPrincipalFromJwtToken(tokenDTO.Token);

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

        // Move 'IsTokenNotValid' logic inside TokenService too!
        if (user == null || IsTokenNotValid(user, tokenDTO.RefreshToken))
        {
            return ServiceResult<AuthResponse>.Failure("Invalid Session",
                                                       "Token not valid!",
                                                       HttpStatusCode.BadRequest);
        }

        var authResponse = await CreateAuthResponseAsync(user);
        return ServiceResult<AuthResponse>.Success(authResponse);
    }

    private static bool IsTokenNotValid(ApplicationUser? user, string refreshToken) =>
        user == null
        || user.RefreshToken != refreshToken
        || user.RefreshTokenExpirationDateTime <= DateTime.Now;
}
