namespace MobileShop.Services.Logging.Configuration;

/// <summary>Configures the shared Serilog sinks used by the application hosts.</summary>
public static class LoggingsConfiguration
{
    private const string ConsoleOutputTemplate =
        "[ {Level:u3} ] {Timestamp:HH:mm:ss} {SourceContext,-35} | {Message:lj}{NewLine}{Exception}";

    private const string FileOutputTemplate =
        "{Timestamp:yyyy-MM-dd HH:mm:ss.fff zzz}\t{Level:u3}\t{SourceContext}\t{Message:lj}{NewLine}{Exception}";

    /// <summary>Configures console and rolling-file Serilog sinks from application settings.</summary>
    public static WebApplicationBuilder ConfigureSerilog(this WebApplicationBuilder builder)
    {
        builder.Logging.ClearProviders();

        var settings = builder.Configuration
            .GetSection("AppLogging")
            .Get<AppLoggingSettings>() ?? new AppLoggingSettings();

        var logger = new LoggerConfiguration()
            .MinimumLevel.Is(settings.Default)
            .MinimumLevel.Override("Microsoft.AspNetCore", LogEventLevel.Warning)
            .Enrich.FromLogContext()
            .WriteTo.Console(
                outputTemplate: ConsoleOutputTemplate,
                restrictedToMinimumLevel: settings.Console,
                theme: AnsiConsoleTheme.Literate)
            .WriteTo.File(
                path: "logs/app-.log",
                rollingInterval: RollingInterval.Day,
                restrictedToMinimumLevel: settings.File,
                outputTemplate: FileOutputTemplate)
            .CreateLogger();

        builder.Logging.AddSerilog(logger);
        return builder;
    }
}