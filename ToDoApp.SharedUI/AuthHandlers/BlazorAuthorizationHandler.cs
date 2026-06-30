using Microsoft.AspNetCore.Components;
using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using ToDoApp.Shared.AuthDTO;
using ToDoApp.Shared.AuthDTO.Token;
using ToDoApp.SharedUI.ServiceContracts;

namespace ToDoApp.SharedUI.AuthHandlers
{
    public class BlazorAuthorizationHandler : DelegatingHandler
    {
        private readonly IDataStorage _dataStorage;

        private readonly NavigationManager _navigationManager;

        public BlazorAuthorizationHandler(IDataStorage dataStorage, NavigationManager navigationManager)
        {
            _dataStorage = dataStorage;
            _navigationManager = navigationManager;
        }

        protected override async Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
        {
            var token = await _dataStorage.GetItemAsync<string>("authToken");

            if (!string.IsNullOrWhiteSpace(token))
            {
                request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token);
            }

            var response = await base.SendAsync(request, cancellationToken);

            if (response.StatusCode == HttpStatusCode.Unauthorized)
            {
                var refreshToken = await _dataStorage.GetItemAsync<string>("refreshToken");

                if (!string.IsNullOrWhiteSpace(token) && !string.IsNullOrWhiteSpace(refreshToken))
                {
                    using var refreshClient = new HttpClient { BaseAddress = response.RequestMessage?.RequestUri };

                    TokenRequest tokenRequest = new()
                    {
                        Token = token,
                        RefreshToken = refreshToken
                    };

                    var refreshResponse = await refreshClient.PostAsJsonAsync("auth/refresh", tokenRequest, cancellationToken);

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

                await _dataStorage.RemoveItemAsync("authToken");
                await _dataStorage.RemoveItemAsync("refreshToken");

                _navigationManager.NavigateTo("/login");
            }

            return response;
        }
    }
}
