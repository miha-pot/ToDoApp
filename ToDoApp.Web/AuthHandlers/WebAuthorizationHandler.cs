using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.WebAssembly.Http;
using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text.Json;
using System.Text.Json.Serialization;
using ToDoApp.Shared.AuthDTO;
using ToDoApp.Shared.AuthDTO.Token;
using ToDoApp.Shared.Common;
using ToDoApp.SharedUI.ServiceContracts;

namespace ToDoApp.Web.AuthHandlers;

public class WebAuthorizationHandler : DelegatingHandler
{
    private readonly IDataStorage _dataStorage;
    private readonly NavigationManager _navigationManager;
    private readonly IHttpClientFactory _httpClientFactory;
    private readonly IAuthState _authState;

    private readonly JsonSerializerOptions _options = new()
    {
        PropertyNameCaseInsensitive = true,
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
        ReferenceHandler = ReferenceHandler.IgnoreCycles
    };

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
        }

        request.Headers.TryAddWithoutValidation("X-Client-Type", "web");
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
                refreshRequest.Headers.TryAddWithoutValidation("X-Client-Type", "web");

                var refreshResponse = await refreshClient.SendAsync(refreshRequest, cancellationToken);

                if (refreshResponse.IsSuccessStatusCode)
                {
                    var apiResult = await refreshResponse.Content.ReadFromJsonAsync<ApiResponse<AuthResponse>>(_options, cancellationToken);

                    if (apiResult is not null && apiResult.IsSuccess && apiResult.Value is not null)
                    {
                        var newAuthToken = apiResult.Value.Token;

                        await _dataStorage.SetItemAsync("authToken", newAuthToken);

                        request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", newAuthToken);

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