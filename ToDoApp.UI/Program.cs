using FluentValidation;
using Microsoft.AspNetCore.Components.Authorization;
using Microsoft.AspNetCore.Components.Web;
using Microsoft.AspNetCore.Components.WebAssembly.Hosting;
using MudBlazor.Services;
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
using ToDoApp.UI;
using ToDoApp.UI.Services;

var builder = WebAssemblyHostBuilder.CreateDefault(args);
builder.RootComponents.Add<App>("#app");
builder.RootComponents.Add<HeadOutlet>("head::after");

builder.Services.AddScoped<IToDoService, ToDoService>();
builder.Services.AddScoped<ITagService, TagService>();
builder.Services.AddScoped<IAuthService, AuthService>();

builder.Services.AddSingleton<LanguageState>();
builder.Services.AddSingleton<IDataStorage, LocalStorageService>();

builder.Services.AddTransient<BlazorAuthorizationHandler>();

builder.Services.AddSingleton(TimeProvider.System);
builder.Services.AddMudServices();

builder.Services.AddSingleton<IValidator<LoginRequest>, LoginRequestValidator>();
builder.Services.AddSingleton<IValidator<RegisterRequest>, RegisterRequestValidator>();
builder.Services.AddSingleton<IValidator<ToDoUpdateRequest>, ToDoUpdateRequestValidator>();
builder.Services.AddSingleton<IValidator<ToDoAddRequest>, ToDoAddRequestValidator>();
builder.Services.AddSingleton<IValidator<TagAddRequest>, TagAddRequestValidator>();
builder.Services.AddSingleton<IValidator<TagUpdateRequest>, TagUpdateRequestValidator>();

builder.Services.AddScoped(sp => new HttpClient
{
    BaseAddress = new Uri(builder.HostEnvironment.BaseAddress)
});

builder.Services.AddHttpClient<ApiService>(options =>
{
    options.BaseAddress = new Uri("https://localhost:7186/api/v1/");
    options.Timeout = TimeSpan.FromSeconds(30);
}).AddHttpMessageHandler<BlazorAuthorizationHandler>();

builder.Services.AddCascadingAuthenticationState();

builder.Services.AddAuthorizationCore();
builder.Services.AddScoped<AuthenticationStateProvider, CustomAuthenticationStateProvider>();

var host = builder.Build();

var langState = host.Services.GetRequiredService<LanguageState>();
await langState.InitializeAsync();

await host.RunAsync();