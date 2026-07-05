using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using ToDoApp.Maui.AuthHandlers;
using ToDoApp.Maui.Services;
using ToDoApp.Shared.AuthDTO;
using ToDoApp.Shared.AuthDTO.Token;
using ToDoApp.SharedUI.ServiceContracts;

namespace ToDoApp.MAUI.AuthHandlers;

public class MobileAuthorizationHandler : DelegatingHandler
{
    private readonly IHttpClientFactory _httpClientFactory;
    private readonly IDataStorage _dataStorage;
    private readonly MobileAuthEventService _authEventService;
    private readonly MobileAuthState _authState;

    public MobileAuthorizationHandler(
                                    IHttpClientFactory httpClientFactory,
                                    IDataStorage dataStorage,
                                    MobileAuthEventService authEventService,
                                    MobileAuthState authState)
    {
        _httpClientFactory = httpClientFactory;
        _dataStorage = dataStorage;
        _authEventService = authEventService;
        _authState = authState;
    }

    protected override async Task<HttpResponseMessage> SendAsync(HttpRequestMessage request,
                                                                  CancellationToken cancellationToken)
    {
        var token = await _dataStorage.GetItemAsync<string>("authToken");

        if (!string.IsNullOrWhiteSpace(token))
        {
            request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token);
            request.Headers.Add("X-Client-Type", "mobile");
        }

        var response = await base.SendAsync(request, cancellationToken);

        if (response.StatusCode != HttpStatusCode.Unauthorized)
            return response;

        await _authState.RefreshLock.WaitAsync(cancellationToken);
        try
        {
            if (_authState.IsLoggingOut()) return response;

            var currentToken = await _dataStorage.GetItemAsync<string>("authToken");
            if (!string.IsNullOrWhiteSpace(currentToken) && currentToken != token)
            {
                request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", currentToken);
                response.Dispose();
                return await base.SendAsync(request, cancellationToken);
            }

            var refreshToken = await _dataStorage.GetItemAsync<string>("refreshToken");

            if (!string.IsNullOrWhiteSpace(token) && !string.IsNullOrWhiteSpace(refreshToken))
            {
                using var refreshClient = _httpClientFactory.CreateClient("RefreshClient");

                var tokenRequest = new TokenRequest { Token = token, RefreshToken = refreshToken };
                var refreshResponse = await refreshClient.PostAsJsonAsync("auth/refresh",
                                                                           tokenRequest,
                                                                           cancellationToken);

                if (refreshResponse.IsSuccessStatusCode)
                {
                    var apiResult = await refreshResponse.Content.ReadFromJsonAsync<AuthResponse>(cancellationToken);
                    if (apiResult is not null)
                    {
                        await _dataStorage.SetItemAsync("authToken", apiResult.Token);
                        await _dataStorage.SetItemAsync("refreshToken", apiResult.RefreshToken);

                        request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", apiResult.Token);
                        response.Dispose();
                        return await base.SendAsync(request, cancellationToken);
                    }
                }
            } 

            _authState.SetLoggingOut();

            await _dataStorage.RemoveItemAsync("authToken");
            await _dataStorage.RemoveItemAsync("refreshToken");

            await _authEventService.NotifySessionExpiredAsync();

            return response;
        }
        finally
        {
            _authState.RefreshLock.Release();
        }
    }
}