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
        private readonly IDataStorage _localStorageService;

        private readonly NavigationManager _navigationManager;

        public BlazorAuthorizationHandler(IDataStorage localStorageService, NavigationManager navigationManager)
        {
            _localStorageService = localStorageService;
            _navigationManager = navigationManager;
        }

        protected override async Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
        {
            // 1. Pridobimo trenutni dostopni žeton iz localStorage
            var token = await _localStorageService.GetItemAsync<string>("authToken");

            if (!string.IsNullOrWhiteSpace(token))
            {
                request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token);
            }

            // 2. Pošljemo zahtevek naprej na API
            var response = await base.SendAsync(request, cancellationToken);

            // 3. 🚀 Prestrežemo 401 Unauthorized (žeton je potekel)
            if (response.StatusCode == HttpStatusCode.Unauthorized)
            {
                var refreshToken = await _localStorageService.GetItemAsync<string>("refreshToken");

                if (!string.IsNullOrWhiteSpace(token) && !string.IsNullOrWhiteSpace(refreshToken))
                {
                    // 4. Izvedemo tihi osvežitveni klic neposredno na tvoj backend endpoint
                    // Ustvarimo začasen čisti HttpClient, da se izognemo neskončni zanki (interceptor zanke)
                    using var refreshClient = new HttpClient { BaseAddress = response.RequestMessage?.RequestUri };

                    TokenRequest tokenRequest = new()
                    {
                        Token = token,
                        RefreshToken = refreshToken
                    };

                    // Predpostavljamo, da tvoj endpoint živi na /api/auth/refresh ali podobno
                    var refreshResponse = await refreshClient.PostAsJsonAsync("auth/refresh", tokenRequest, cancellationToken);

                    if (refreshResponse.IsSuccessStatusCode)
                    {
                        // Tvoj backend ApiResponse<AuthResponse> vrne nov par žetonov
                        var apiResult = await refreshResponse.Content.ReadFromJsonAsync<AuthResponse>(cancellationToken);

                        if (apiResult is not null)
                        {
                            // 5. Shranimo nova žetona nazaj v localStorage
                            await _localStorageService.SetItemAsync("authToken", apiResult.Token);
                            await _localStorageService.SetItemAsync("refreshToken", apiResult.RefreshToken);

                            // 6. Ponovimo prvotni zahtevek z NOVIM delujočim žetonom
                            request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", apiResult.Token);

                            response.Dispose(); // Zapremo staro 401 odzivno telo
                            return await base.SendAsync(request, cancellationToken);
                        }
                    }
                }

                // 7. Če osveževanje spodleti (npr. potekel je tudi Refresh Token), počistimo sejo in delavca vržemo na Login
                await _localStorageService.RemoveItemAsync("authToken");
                await _localStorageService.RemoveItemAsync("refreshToken");

                _navigationManager.NavigateTo("/login");
            }

            return response;
        }
    }
}
