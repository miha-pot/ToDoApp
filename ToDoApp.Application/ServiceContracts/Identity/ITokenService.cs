using ToDoApp.Domain.Identity;
using ToDoApp.Shared.AuthDTO;
using ToDoApp.Shared.AuthDTO.Token;
using ToDoApp.Shared.Common;

namespace ToDoApp.Application.ServiceContracts.Identity;

public interface ITokenService
{
    Task<AuthResponse> CreateAuthResponseAsync(ApplicationUser? user);
    Task<ServiceResult<AuthResponse>> RefreshSessionAsync(TokenRequest tokenDTO);
    Task<ServiceResult<AuthResponse>> RevokeRefreshTokenAsync(TokenRequest token);
}
