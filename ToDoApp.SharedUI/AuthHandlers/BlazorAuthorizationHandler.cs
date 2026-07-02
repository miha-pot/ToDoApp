using Microsoft.AspNetCore.Components;
using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using ToDoApp.Shared.AuthDTO;
using ToDoApp.Shared.AuthDTO.Token;
using ToDoApp.SharedUI.ServiceContracts;
using Microsoft.AspNetCore.Components.WebAssembly.Http;

namespace ToDoApp.SharedUI.AuthHandlers;

public class BlazorAuthorizationHandler : DelegatingHandler
{
    private readonly IDataStorage _dataStorage;
    private readonly SemaphoreSlim _refreshLock = new(1, 1);
    private readonly NavigationManager _navigationManager;
    private readonly IHttpClientFactory _httpClientFactory;
    private bool _isLoggingOut = false;

    public BlazorAuthorizationHandler(IDataStorage dataStorage,
                                       NavigationManager navigationManager,
                                       IHttpClientFactory httpClientFactory)
    {
        _dataStorage = dataStorage;
        _navigationManager = navigationManager;
        _httpClientFactory = httpClientFactory;
    }

    protected override async Task<HttpResponseMessage> SendAsync(HttpRequestMessage request,
                                                                 CancellationToken cancellationToken)
    {
        var token = await _dataStorage.GetItemAsync<string>("authToken");

        if (!string.IsNullOrWhiteSpace(token))
        {
            request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token);
        }

        request.SetBrowserRequestCredentials(BrowserRequestCredentials.Include);

        var response = await base.SendAsync(request, cancellationToken);

        if (response.StatusCode != HttpStatusCode.Unauthorized) return response;

        await _refreshLock.WaitAsync(cancellationToken);
        try
        {
            if (_isLoggingOut) return response;

            var currentToken = await _dataStorage.GetItemAsync<string>("authToken");
            if (!string.IsNullOrWhiteSpace(currentToken) && currentToken != token)
            {
                request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", currentToken);
                response.Dispose();

                return await base.SendAsync(request, cancellationToken);
            }

            if (!string.IsNullOrWhiteSpace(token))
            {
                using HttpClient refreshClient = _httpClientFactory.CreateClient("RefreshClient");
                var tokenRequest = new TokenRequest() { Token = token };
                var refreshRequest = new HttpRequestMessage(HttpMethod.Post, "auth/refresh")
                {
                    Content = JsonContent.Create(tokenRequest)
                };

                refreshRequest.SetBrowserRequestCredentials(BrowserRequestCredentials.Include);

                var refreshResponse = await refreshClient.SendAsync(refreshRequest, cancellationToken);

                if (refreshResponse.IsSuccessStatusCode)
                {
                    AuthResponse? apiResult = await refreshResponse.Content.ReadFromJsonAsync<AuthResponse>(cancellationToken);
                    if (apiResult is not null)
                    {
                        await _dataStorage.SetItemAsync("authToken", apiResult.Token);

                        request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", apiResult.Token);
                        response.Dispose();

                        return await base.SendAsync(request, cancellationToken);
                    }
                }
            }

            _isLoggingOut = true;
            await _dataStorage.RemoveItemAsync("authToken");

            _navigationManager.NavigateTo("/login");

            return response;
        }
        finally
        {
            _refreshLock.Release();
        }
    }

    public void ResetLogoutState() => _isLoggingOut = false;
}