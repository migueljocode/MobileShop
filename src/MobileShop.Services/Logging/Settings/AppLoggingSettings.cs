namespace MobileShop.Services.Logging.Settings;

/// <summary>Application logging levels for Serilog sinks.</summary>
public class AppLoggingSettings
{
    /// <summary>Default minimum log level.</summary>
    public Serilog.Events.LogEventLevel Default { get; set; } = Serilog.Events.LogEventLevel.Debug;

    /// <summary>Minimum log level for the console sink.</summary>
    public Serilog.Events.LogEventLevel Console { get; set; } = Serilog.Events.LogEventLevel.Information;

    /// <summary>Minimum log level for the file sink.</summary>
    public Serilog.Events.LogEventLevel File { get; set; } = Serilog.Events.LogEventLevel.Debug;
}
