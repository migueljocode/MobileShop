using Microsoft.AspNetCore.Builder;
using MobileShop.Services.Logging.Configuration;

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
}
