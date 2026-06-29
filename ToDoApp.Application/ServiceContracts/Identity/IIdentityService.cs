using ToDoApp.Shared.AuthDTO;
using ToDoApp.Shared.AuthDTO.ForgotPassword;
using ToDoApp.Shared.AuthDTO.Login;
using ToDoApp.Shared.AuthDTO.Register;
using ToDoApp.Shared.AuthDTO.ResetPassword;
using ToDoApp.Shared.AuthDTO.Token;
using ToDoApp.Shared.Common;

namespace ToDoApp.Application.ServiceContracts.Identity;

public interface IIdentityService
{
    Task<ServiceResult<AuthResponse>> RegisterAsync(RegisterRequest registerDTO);
    Task<ServiceResult<AuthResponse>> LoginAsync(LoginRequest loginDTO);
    Task<ServiceResult<AuthResponse>> RefreshTokenAsync(TokenRequest tokenDTO);
    Task<ServiceResult<string>> ForgotPasswordAsync(ForgotPassRequest forgotPassDTO);
    Task<ServiceResult<string>> ResetPasswordAsync(ResetPassRequest resetPassDTO);
    Task Logout();
}
