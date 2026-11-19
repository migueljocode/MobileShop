namespace MobileShop.Tests.Logging;

public class LoggingsConfigurationTests
{
    [Fact]
    public void ConfigureSerilog_returns_builder_for_chaining()
    {
        var builder = WebApplication.CreateBuilder();
        builder.Configuration["AppLogging:Default"] = "Information";
        builder.Configuration["AppLogging:Console"] = "Information";
        builder.Configuration["AppLogging:File"] = "Information";

        var result = builder.ConfigureSerilog();

        Assert.Same(builder, result);
    }

    [Fact]
    public void ConfigureSerilog_uses_default_settings_when_section_missing()
    {
        var builder = WebApplication.CreateBuilder();

        var result = builder.ConfigureSerilog();

        Assert.Same(builder, result);
    }

    [Fact]
    public void LogFilePath_lives_under_LogDirectory_with_rolling_app_log_template()
    {
        Assert.Equal("logs", Path.GetFileName(SolutionPaths.LogDirectory));
        Assert.StartsWith(
            SolutionPaths.LogDirectory + Path.DirectorySeparatorChar,
            LoggingsConfiguration.LogFilePath,
            StringComparison.Ordinal);
        Assert.Equal("app-.log", Path.GetFileName(LoggingsConfiguration.LogFilePath));
    }
}
