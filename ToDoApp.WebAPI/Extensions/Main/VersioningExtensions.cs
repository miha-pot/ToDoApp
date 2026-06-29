using Asp.Versioning;

namespace ToDoApp.WebAPI.Extensions.Main;

public static class VersioningExtensions
{
    public static IServiceCollection AddSwaggerAndVersioning(this IServiceCollection services)
    {
        services.AddOpenApi();
        services.AddEndpointsApiExplorer();

        services.AddApiVersioning(options =>
        {
            options.AssumeDefaultVersionWhenUnspecified = true;
            options.DefaultApiVersion = new ApiVersion(1, 0);
            options.ReportApiVersions = true;

            // This is what you had in your working second block
            options.ApiVersionReader = new UrlSegmentApiVersionReader();
        })
                .AddApiExplorer(options =>
                {
                    options.GroupNameFormat = "'v'VVV";
                    options.SubstituteApiVersionInUrl = true; // Crucial for URL routing!
                });

        return services;
    }
}