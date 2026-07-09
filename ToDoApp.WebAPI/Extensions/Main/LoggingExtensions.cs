using Serilog;
using Serilog.Events;

namespace ToDoApp.WebAPI.Extensions.Main;

public static class LoggingExtensions
{
    public static WebApplicationBuilder AddSerilogLogging(this WebApplicationBuilder builder)
    {
        Log.Logger = new LoggerConfiguration().ReadFrom.Configuration(builder.Configuration)
            .WriteTo.Logger(lc => lc
                .Filter.ByIncludingOnly(evt => evt.Level <= LogEventLevel.Warning)
                .WriteTo.File("logs/InfoLogs/log-.txt", rollingInterval: RollingInterval.Day, retainedFileCountLimit: 21))

            // 2. Zapisovanje Error/Warning logov v svojo mapo
            .WriteTo.Logger(lc => lc
                .Filter.ByIncludingOnly(evt => evt.Level >= LogEventLevel.Error)
                .WriteTo.File("logs/ErrorLogs/log-.txt", rollingInterval: RollingInterval.Day, retainedFileCountLimit: 90))
            .WriteTo.Console().CreateLogger();

        builder.Host.UseSerilog();

        return builder;
    }
}
