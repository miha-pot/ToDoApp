using System.Threading.RateLimiting;

namespace ToDoApp.WebAPI.Extensions.Main;

public static class RateLimiterExtension
{
    public static IServiceCollection AddCustomRateLimiter(this IServiceCollection services)
    {
        services.AddRateLimiter(options =>
        {
            // Nastavimo standardni HTTP status 429 (Too Many Requests), ko klijent pretirava
            options.RejectionStatusCode = StatusCodes.Status429TooManyRequests;

            // Ustvarimo politiko z imenom "api-policy", ki jo kasneje pripnemo na skupine endpointov
            options.AddPolicy("api-policy", httpContext =>
            {
                // Pridobimo IP naslov naprave (če ga ni, damo privzeto "anonymous")
                var clientIp = httpContext.Connection.RemoteIpAddress?.ToString() ?? "anonymous";

                // Vrnemo Token Bucket limiter, ki je strogo vezan (particioniran) na ta IP naslov
                return RateLimitPartition.GetTokenBucketLimiter(clientIp, _ => new TokenBucketRateLimiterOptions
                {
                    TokenLimit = 60,                               // Maksimalna kapaciteta vedra (60 žetonov hkrati)
                    QueueLimit = 5,                                // Maksimalno 5 zahtevkov čaka v vrsti, ko se vedro sprazni
                    ReplenishmentPeriod = TimeSpan.FromSeconds(10), // Obdobje osveževanja (vsakih 10 sekund)
                    TokensPerPeriod = 20,                          // Koliko žetonov dodamo v vedro vsakih 10 sekund
                    AutoReplenishment = true
                });
            });
        });

        return services;
    }
}
