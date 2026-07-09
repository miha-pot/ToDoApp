using Microsoft.AspNetCore.Components;
using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using ToDoApp.Shared.AuthDTO;
using ToDoApp.Shared.AuthDTO.Token;
using ToDoApp.SharedUI.ServiceContracts;
using Microsoft.AspNetCore.Components.WebAssembly.Http;

namespace ToDoApp.Web.AuthHandlers;

public class WebAuthorizationHandler : DelegatingHandler
{
    private readonly IDataStorage _dataStorage;
    private readonly NavigationManager _navigationManager;
    private readonly IHttpClientFactory _httpClientFactory;
    private readonly IAuthState _authState;

    public WebAuthorizationHandler(IDataStorage dataStorage,
                                       NavigationManager navigationManager,
                                       IHttpClientFactory httpClientFactory,
                                       IAuthState authState)
    {
        _dataStorage = dataStorage;
        _navigationManager = navigationManager;
        _httpClientFactory = httpClientFactory;
        _authState = authState;
    }

    protected override async Task<HttpResponseMessage> SendAsync(HttpRequestMessage request,
                                                                 CancellationToken cancellationToken)
    {
        var token = await _dataStorage.GetItemAsync<string>("authToken");

        if (!string.IsNullOrWhiteSpace(token))
        {
            request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token);
            request.Headers.Add("X-Client-Type", "web");
        }

        request.SetBrowserRequestCredentials(BrowserRequestCredentials.Include);

        var response = await base.SendAsync(request, cancellationToken);

        if (response.StatusCode != HttpStatusCode.Unauthorized) return response;

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

            _authState.SetLoggingOut();

            await _dataStorage.RemoveItemAsync("authToken");

            _navigationManager.NavigateTo("/login", forceLoad: true);

            return response;
        }
        finally
        {
            _authState.RefreshLock.Release();
        }
    }
}