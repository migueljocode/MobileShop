namespace MobileShop.Services.Logging.Configuration;
// extension methods for configuring Serilog In either MobileShop.Api or MobileShop.Web
public static class LoggingsConfiguration
{
    // Console: Colored [LogLevel] first, followed by time, aligned source, and message
    private const string ConsoleOutputTemplate = 
        "[{Level:u3}] {Timestamp:HH:mm:ss} {SourceContext,-35} | {Message:lj}{NewLine}{Exception}";

    // File: Tab-delimited single-line format optimized for Linux tools (grep, cut, awk)
    private const string FileOutputTemplate = 
        "{Timestamp:yyyy-MM-dd HH:mm:ss.fff zzz}\t{Level:u3}\t{SourceContext}\t{Message:lj}{NewLine}{Exception}";

    public static void ConfigureSerilog(this WebApplicationBuilder builder)
    {
        builder.Logging.ClearProviders();
        var config = builder.Configuration;
        // Logging:LogLevel:Default controls Serilog's minimum level; Information is the safe fallback.
        var configuredLevel = config["Logging:LogLevel:Default"];
        var minimumLevel = Enum.TryParse<Serilog.Events.LogEventLevel>(
            configuredLevel,
            ignoreCase: true,
            out var parsedLevel)
            ? parsedLevel
            : Serilog.Events.LogEventLevel.Information;

        var logger = new LoggerConfiguration()
        .MinimumLevel.Is(minimumLevel)
        .Enrich.FromLogContext()
        .WriteTo.Console(
            outputTemplate: ConsoleOutputTemplate, 
            theme: AnsiConsoleTheme.Literate)
        .WriteTo.File(
            path: "logs/app-.log", // this can be change according to json config later
            rollingInterval: RollingInterval.Day,
            outputTemplate: FileOutputTemplate)
        .CreateLogger();
        
        builder.Logging.AddSerilog(logger);
    }
}