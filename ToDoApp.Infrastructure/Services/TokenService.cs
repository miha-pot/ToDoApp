using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;
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
    private readonly IConfiguration _configuration;

    public TokenService(IJwtService jwtService,
                        UserManager<ApplicationUser> userManager,
                        IConfiguration configuration)
    {
        _jwtService = jwtService;
        _userManager = userManager;
        _configuration = configuration;
    }

    public async Task<AuthResponse> CreateAuthResponseAsync(ApplicationUser user)
    {
        var (jwtToken, jwtExpiration) = await _jwtService.CreateJwtTokenAsync(user);

        var refreshToken = RefreshTokenGenerator.Generate();
        var refreshTokenExpiration = DateTime.UtcNow.AddMinutes(
            int.Parse(_configuration["RefreshToken:EXPIRATION_MINUTES"]!));

        user.RefreshToken = refreshToken;
        user.RefreshTokenExpirationDateTime = refreshTokenExpiration;
        await _userManager.UpdateAsync(user);

        return new AuthResponse
        {
            UserId = user.Id,
            Token = jwtToken,
            Email = user.Email!,
            Expiration = jwtExpiration,
            FirstName = user.FirstName!,
            LastName = user.LastName ?? string.Empty,
            RefreshToken = refreshToken,
            RefreshTokenExpiration = refreshTokenExpiration
        };
    }

    public async Task<ServiceResult<AuthResponse>> RefreshSessionAsync(TokenRequest tokenRequest)
    {
        var userResult = await ResolveUserFromAccessTokenAsync(tokenRequest.Token);
        if (!userResult.IsSuccess)
        {
            return ServiceResult<AuthResponse>.Failure(userResult.ErrorTitle!, userResult.ErrorDetail ?? "", userResult.StatusCode);
        }

        var user = userResult.Value!;
        if (IsRefreshTokenInvalid(user, tokenRequest.RefreshToken))
        {
            return ServiceResult<AuthResponse>.Failure("Invalid Session", "Token not valid!", HttpStatusCode.BadRequest);
        }

        var authResponse = await CreateAuthResponseAsync(user);
        return ServiceResult<AuthResponse>.Success(authResponse);
    }

    public async Task<ServiceResult<AuthResponse>> RevokeRefreshTokenAsync(TokenRequest tokenRequest)
    {
        var userResult = await ResolveUserFromAccessTokenAsync(tokenRequest.Token);

        if (userResult.IsSuccess)
        {
            var user = userResult.Value!;
            user.RefreshToken = null;
            user.RefreshTokenExpirationDateTime = DateTime.MinValue;

            await _userManager.UpdateAsync(user);
        }

        return ServiceResult<AuthResponse>.Success(new AuthResponse(), HttpStatusCode.NoContent);
    }

    private async Task<ServiceResult<ApplicationUser>> ResolveUserFromAccessTokenAsync(string? accessToken)
    {
        if (string.IsNullOrWhiteSpace(accessToken))
        {
            return ServiceResult<ApplicationUser>.Failure("Missing token", "Access token was not provided.", HttpStatusCode.BadRequest);
        }

        ClaimsPrincipal principal;
        try
        {
            principal = _jwtService.GetPrincipalFromExpiredToken(accessToken);
        }
        catch (SecurityTokenException)
        {
            return ServiceResult<ApplicationUser>.Failure("Invalid Token", "Token signature is invalid.", HttpStatusCode.BadRequest);
        }

        var email = principal.FindFirstValue(ClaimTypes.Email);
        if (string.IsNullOrEmpty(email))
        {
            return ServiceResult<ApplicationUser>.Failure("Invalid Token", "Token does not contain an email claim.", HttpStatusCode.BadRequest);
        }

        var user = await _userManager.FindByEmailAsync(email);
        if (user is null)
        {
            return ServiceResult<ApplicationUser>.Failure("User not found!", "", HttpStatusCode.NotFound);
        }

        return ServiceResult<ApplicationUser>.Success(user);
    }

    private static bool IsRefreshTokenInvalid(ApplicationUser user, string? refreshToken) =>
        string.IsNullOrEmpty(refreshToken)
        || user.RefreshToken != refreshToken
        || user.RefreshTokenExpirationDateTime <= DateTime.UtcNow;
}
