using Microsoft.AspNetCore.Components.Web;
using Microsoft.AspNetCore.Components.WebAssembly.Hosting;
using ToDoApp.SharedUI.States;
using ToDoApp.Web.Extensions;

var builder = WebAssemblyHostBuilder.CreateDefault(args);
builder.RootComponents.Add<ToDoApp.SharedUI.App>("#app");
builder.RootComponents.Add<HeadOutlet>("head::after");

builder.Services.AddApplicationServices()
                .AddInfrastructureServices(builder)
                .AddValidationServices()
                .AddSecurityServices();

var host = builder.Build();

var langState = host.Services.GetRequiredService<LanguageState>();
await langState.InitializeAsync();

await host.RunAsync();