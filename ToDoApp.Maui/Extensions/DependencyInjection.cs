using FluentValidation;
using Microsoft.AspNetCore.Components.Authorization;
using MudBlazor.Services;
using ToDoApp.Maui.Services;
using ToDoApp.Shared.AuthDTO.Login;
using ToDoApp.Shared.AuthDTO.Register;
using ToDoApp.Shared.TagDTO.Commands;
using ToDoApp.Shared.TagDTO.Validators;
using ToDoApp.Shared.ToDoDTO.Commands;
using ToDoApp.Shared.ToDoDTO.Validators;
using ToDoApp.SharedUI.AuthHandlers;
using ToDoApp.SharedUI.Providers;
using ToDoApp.SharedUI.ServiceContracts;
using ToDoApp.SharedUI.Services;
using ToDoApp.SharedUI.States;

namespace ToDoApp.Maui.Extensions;

public static class DependencyInjection
{
    public static IServiceCollection AddApplicationServices(this IServiceCollection services)
    {
        services.AddScoped<IToDoService, ToDoService>();
        services.AddScoped<ITagService, TagService>();
        services.AddScoped<IAuthService, AuthService>();

        return services;
    }

    public static IServiceCollection AddInfrastructureServices(this IServiceCollection services, MauiAppBuilder builder)
    {
        services.AddSingleton<LanguageState>();
        services.AddSingleton<IDataStorage, MauiStorageService>();
        services.AddSingleton(TimeProvider.System);
        services.AddMudServices();

        string apiUrl = string.Empty;

        if (string.IsNullOrEmpty(apiUrl))
            throw new NotImplementedException("Set valid backend url!");

        services.AddHttpClient<ApiService>(options =>
        {
            options.BaseAddress = new Uri(apiUrl);
            options.Timeout = TimeSpan.FromSeconds(30);
        }).AddHttpMessageHandler<BlazorAuthorizationHandler>();

        services.AddHttpClient("RefreshClient", options =>
        {
            options.BaseAddress = new Uri(apiUrl);
            options.Timeout = TimeSpan.FromSeconds(30);
        });

        return services;
    }

    public static IServiceCollection AddValidationServices(this IServiceCollection services)
    {
        services.AddSingleton<IValidator<LoginRequest>, LoginRequestValidator>();
        services.AddSingleton<IValidator<RegisterRequest>, RegisterRequestValidator>();

        services.AddSingleton<IValidator<ToDoAddRequest>, ToDoAddRequestValidator>();
        services.AddSingleton<IValidator<ToDoUpdateRequest>, ToDoUpdateRequestValidator>();

        services.AddSingleton<IValidator<TagAddRequest>, TagAddRequestValidator>();
        services.AddSingleton<IValidator<TagUpdateRequest>, TagUpdateRequestValidator>();

        return services;
    }

    public static IServiceCollection AddSecurityServices(this IServiceCollection services)
    {
        services.AddTransient<BlazorAuthorizationHandler>();
        services.AddCascadingAuthenticationState();
        services.AddAuthorizationCore();
        services.AddScoped<AuthenticationStateProvider, CustomAuthenticationStateProvider>();

        return services;
    }
}
