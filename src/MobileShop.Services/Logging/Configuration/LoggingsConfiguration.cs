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
        var config = builder.Configuration;
        var defaultLevel = ParseLevel(config["Serilog:MinimumLevel:Default"] ?? "Debug");
        var consoleLevel = ParseLevel(config["Serilog:MinimumLevel:Console"] ?? "Information");
        var fileLevel = ParseLevel(config["Serilog:MinimumLevel:File"] ?? "Debug");

        var logger = new LoggerConfiguration()
            .MinimumLevel.Is(defaultLevel)
            .MinimumLevel.Override("Microsoft.AspNetCore", Serilog.Events.LogEventLevel.Warning)
            .Enrich.FromLogContext()
            .WriteTo.Console(
                outputTemplate: ConsoleOutputTemplate,
                restrictedToMinimumLevel: consoleLevel,
                theme: AnsiConsoleTheme.Literate)
            .WriteTo.File(
                path: "logs/app-.log",
                rollingInterval: RollingInterval.Day,
                restrictedToMinimumLevel: fileLevel,
                outputTemplate: FileOutputTemplate)
            .CreateLogger();

        builder.Logging.AddSerilog(logger);
        return builder;
    }

    private static Serilog.Events.LogEventLevel ParseLevel(string? configuredLevel)
    {
        return Enum.TryParse<Serilog.Events.LogEventLevel>(configuredLevel, ignoreCase: true, out var parsedLevel)
            ? parsedLevel
            : Serilog.Events.LogEventLevel.Information;
    }
}