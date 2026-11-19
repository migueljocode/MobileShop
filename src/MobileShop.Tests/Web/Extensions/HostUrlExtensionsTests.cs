using Microsoft.AspNetCore.Builder;
using MobileShop.Web.Extensions;

namespace MobileShop.Tests.Web.Extensions;

public class HostUrlExtensionsTests
{
    [Fact]
    public void UseDefaultUrlWhenUnset_sets_default_url_when_nothing_is_configured()
    {
        var builder = WebApplication.CreateBuilder();
        builder.Configuration["urls"] = null;
        builder.Configuration["HTTP_PORTS"] = null;
        builder.Configuration["HTTPS_PORTS"] = null;

        var result = builder.UseDefaultUrlWhenUnset();

        Assert.Same(builder, result);
        Assert.Equal(HostUrlExtensions.DefaultUrl, builder.Configuration["urls"]);
    }

    [Fact]
    public void UseDefaultUrlWhenUnset_preserves_configured_urls()
    {
        var builder = WebApplication.CreateBuilder();
        builder.Configuration["urls"] = "http://localhost:6000";

        builder.UseDefaultUrlWhenUnset();

        Assert.Equal("http://localhost:6000", builder.Configuration["urls"]);
    }

    [Fact]
    public void UseDefaultUrlWhenUnset_preserves_configured_http_ports()
    {
        var builder = WebApplication.CreateBuilder();
        builder.Configuration["urls"] = null;
        builder.Configuration["HTTP_PORTS"] = "8080";

        builder.UseDefaultUrlWhenUnset();

        Assert.Null(builder.Configuration["urls"]);
        Assert.Equal("8080", builder.Configuration["HTTP_PORTS"]);
    }

    [Fact]
    public void UseDefaultUrlWhenUnset_preserves_kestrel_endpoints_section()
    {
        var builder = WebApplication.CreateBuilder();
        builder.Configuration["urls"] = null;
        builder.Configuration["Kestrel:Endpoints:Http:Url"] = "http://localhost:5200";

        builder.UseDefaultUrlWhenUnset();

        Assert.Null(builder.Configuration["urls"]);
    }
}
