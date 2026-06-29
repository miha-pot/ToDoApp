using System.Security.Claims;
using ToDoApp.Domain.Identity;
using ToDoApp.Shared.AuthDTO;

namespace ToDoApp.Application.ServiceContracts.Identity;

public interface IJwtService
{
    AuthResponse CreateJwtToken(ApplicationUser user);
    ClaimsPrincipal? GetPrincipalFromJwtToken(string? token);
}
