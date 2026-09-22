namespace MobileShop.Services.Logging.Settings;

/// <summary>Application logging levels for Serilog sinks.</summary>
/// <remarks>
/// These three levels are bound by name from the <c>AppLogging</c> configuration section. When the
/// section is absent the class defaults apply (<c>Default</c> = Debug, <c>Console</c> = Information,
/// <c>File</c> = Debug). <c>Default</c> is the base minimum level while <c>Console</c> and <c>File</c>
/// are the per-sink restricted minimums. A value that does not name a Serilog.Events.LogEventLevel member
/// causes the configuration binder to throw during startup rather than silently falling back to a default,
/// which is the intended fail-fast behaviour.
/// </remarks>
public class AppLoggingSettings
{
    /// <summary>Default minimum log level.</summary>
    public LogEventLevel Default { get; set; } = LogEventLevel.Debug;

    /// <summary>Minimum log level for the console sink.</summary>
    public LogEventLevel Console { get; set; } = LogEventLevel.Information;

    /// <summary>Minimum log level for the file sink.</summary>
    public LogEventLevel File { get; set; } = LogEventLevel.Debug;
}
