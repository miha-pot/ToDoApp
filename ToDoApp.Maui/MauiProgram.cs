using Microsoft.Extensions.Logging;
using System.Runtime.Versioning;
using ToDoApp.Maui.Extensions;

namespace ToDoApp.Maui;

public static class MauiProgram
{
    [SupportedOSPlatform("MacCatalyst15.0")] // 🟢 Pove prevajalniku, da je varno za Mac 15+
    [SupportedOSPlatform("Android")]         // Podprto na vseh različicah Androida
    [SupportedOSPlatform("iOS")]             // Podprto na vseh različicah iOS
    [SupportedOSPlatform("Windows")]         // Podprto na Windowsih
    public static MauiApp CreateMauiApp()
    {
        var builder = MauiApp.CreateBuilder();
        builder
            .UseMauiApp<App>()
            .ConfigureFonts(fonts =>
            {
                fonts.AddFont("OpenSans-Regular.ttf", "OpenSansRegular");
            });

        builder.Services.AddMauiBlazorWebView();
        builder.Services.AddApplicationServices()
                        .AddInfrastructureServices(builder)
                        .AddValidationServices()
                        .AddSecurityServices();

#if DEBUG
        builder.Services.AddBlazorWebViewDeveloperTools();
        builder.Logging.AddDebug();
#endif

        return builder.Build();
    }
}
