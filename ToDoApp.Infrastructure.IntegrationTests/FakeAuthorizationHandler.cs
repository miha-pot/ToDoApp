namespace ToDoApp.Infrastructure.IntegrationTests;

public class FakeAuthorizationHandler : DelegatingHandler
{
    protected override async Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
    {
        // Dodamo lažni Bearer token v glavo zahtevka
        request.Headers.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", "FAKE_TOKEN_ZA_TESTIRANJE");

        return await base.SendAsync(request, cancellationToken);
    }
}
