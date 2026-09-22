namespace MobileShop.Services.Logging.Settings;

/// <summary>Application logging levels for Serilog sinks.</summary>
public class AppLoggingSettings
{
    /// <summary>Default minimum log level.</summary>
    public LogEventLevel Default { get; set; } = LogEventLevel.Debug;

    /// <summary>Minimum log level for the console sink.</summary>
    public LogEventLevel Console { get; set; } = LogEventLevel.Information;

    /// <summary>Minimum log level for the file sink.</summary>
    public LogEventLevel File { get; set; } = LogEventLevel.Debug;
}
