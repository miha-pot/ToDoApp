using Microsoft.AspNetCore.Components.Authorization;
using System.Security.Claims;
using System.Text.Json;
using ToDoApp.SharedUI.ServiceContracts;

namespace ToDoApp.SharedUI.Providers;

public class CustomAuthenticationStateProvider : AuthenticationStateProvider
{
    private readonly IDataStorage _localStorage;
    private readonly ClaimsPrincipal _anonymous = new(new ClaimsIdentity());

    public CustomAuthenticationStateProvider(IDataStorage localStorage) => _localStorage = localStorage;

    public override async Task<AuthenticationState> GetAuthenticationStateAsync()
    {
        // 1. Try to read the token saved by our AuthService
        var token = await _localStorage.GetItemAsync<string>("authToken");
        if (string.IsNullOrWhiteSpace(token)) return new AuthenticationState(_anonymous);

        List<Claim> claims = []; 

        // Parse internal JWT roles/claims dynamically if needed
        claims.AddRange(ParseClaimsFromJwt(token));

        var identity = new ClaimsIdentity(claims, "JwtAuth");
        return new AuthenticationState(new ClaimsPrincipal(identity));
    }

    // 3. Call this method when the user completes login to dynamically update the UI!
    public void NotifyUserLogin()
    {
        var authState = GetAuthenticationStateAsync();
        NotifyAuthenticationStateChanged(authState);
    }

    private IEnumerable<Claim> ParseClaimsFromJwt(string jwt)
    {
        var payload = jwt.Split('.')[1];
        var jsonBytes = ParseBase64WithoutPadding(payload);
        var keyValuePairs = JsonSerializer.Deserialize<Dictionary<string, object>>(jsonBytes);
        return keyValuePairs!.Select(kvp => new Claim(kvp.Key, kvp.Value.ToString()!));
    }

    private byte[] ParseBase64WithoutPadding(string base64)
    {
        switch (base64.Length % 4)
        {
            case 2: base64 += "=="; break;
            case 3: base64 += "="; break;
        }
        return Convert.FromBase64String(base64);
    }

    public void NotifyUserLogout()
    {
        var anonymousIdentity = new ClaimsIdentity();
        var anonymousUser = new ClaimsPrincipal(anonymousIdentity);

        var anonymousState = Task.FromResult(new AuthenticationState(anonymousUser));

        NotifyAuthenticationStateChanged(anonymousState);
    }
}