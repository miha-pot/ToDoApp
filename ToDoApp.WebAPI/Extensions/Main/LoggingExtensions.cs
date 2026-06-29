using Serilog;

namespace ToDoApp.WebAPI.Extensions.Main;

public static class LoggingExtensions
{
    public static WebApplicationBuilder AddSerilogLogging(this WebApplicationBuilder builder)
    {
        Log.Logger = new LoggerConfiguration()
            .ReadFrom.Configuration(builder.Configuration)
            .WriteTo.Console()
            .CreateLogger();


        builder.Host.UseSerilog();


        return builder;
    }
}
