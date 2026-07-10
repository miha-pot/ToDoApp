using System.Security.Claims;
using ToDoApp.Domain.Identity;

namespace ToDoApp.Application.ServiceContracts.Identity;

public interface IJwtService
{
    Task<(string Token, DateTime Expiration)> CreateJwtTokenAsync(ApplicationUser user);
    ClaimsPrincipal GetPrincipalFromExpiredToken(string token);
}
