using ToDoApp.Shared.AuthDTO.Login;
using ToDoApp.Shared.AuthDTO.Register;
using ToDoApp.Shared.Common;

namespace ToDoApp.SharedUI.ServiceContracts;

public interface IAuthService
{
    Task<ApiResponse<bool>> LoginAsync(LoginRequest request, CancellationToken token);
    Task<ApiResponse<bool>> RegisterAsync(RegisterRequest request, CancellationToken token);
    Task LogoutAsync(CancellationToken cancellationToken);
}