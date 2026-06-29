using Microsoft.AspNetCore.Components.Authorization;
using ToDoApp.Shared.AuthDTO;
using ToDoApp.Shared.AuthDTO.Login;
using ToDoApp.Shared.AuthDTO.Register;
using ToDoApp.Shared.Common;
using ToDoApp.SharedUI.Providers;
using ToDoApp.SharedUI.ServiceContracts;

namespace ToDoApp.SharedUI.Services;

public class AuthService : IAuthService
{
    private readonly ApiService _apiService;
    private readonly IDataStorage _localStorage;

    private readonly AuthenticationStateProvider _authStateProvider;
    private readonly string _endpoint = "auth";

    public AuthService(ApiService apiService,
                       IDataStorage localStorage,
                       AuthenticationStateProvider authStateProvider)
    {
        _apiService = apiService;
        _localStorage = localStorage;
        _authStateProvider = authStateProvider;
    }

    public async Task<ApiResponse<bool>> LoginAsync(LoginRequest request, CancellationToken token)
    {
        var response = await _apiService.PostAsync<LoginRequest, AuthResponse>($"{_endpoint}/login",
                                                                               request,
                                                                               token);

        if (!response.IsSuccess || response.Value is null)
        {
            return ApiResponse<bool>.Failure(response.ErrorTitle,
                                             response.ErrorDetail,
                                             response.StatusCode);
        }

        await SetUserInLocalStorage(response.Value);

        return ApiResponse<bool>.Success(response.IsSuccess, response.StatusCode);
    }

    public async Task<ApiResponse<bool>> RegisterAsync(RegisterRequest request, CancellationToken token)
    {
        var response = await _apiService.PostAsync<RegisterRequest, AuthResponse>($"{_endpoint}/register",
                                                                                  request,
                                                                                  token);

        if (!response.IsSuccess || response.Value is null)
        {
            return ApiResponse<bool>.Failure(response.ErrorTitle,
                                             response.ErrorDetail,
                                             response.StatusCode);
        }

        await SetUserInLocalStorage(response.Value);

        return ApiResponse<bool>.Success(response.IsSuccess, response.StatusCode);

    }

    private async Task SetUserInLocalStorage(AuthResponse response)
    {
        await _localStorage.SetItemAsync("authToken", response.Token);
        await _localStorage.SetItemAsync("refreshToken", response.RefreshToken);

        ((CustomAuthenticationStateProvider)_authStateProvider).NotifyUserLogin();
    }

    public async Task LogoutAsync()
    {
        await _localStorage.RemoveItemAsync("authToken");
        await _localStorage.RemoveItemAsync("refreshToken");

        ((CustomAuthenticationStateProvider)_authStateProvider).NotifyUserLogout();
    }
}
