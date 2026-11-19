namespace MobileShop.Web.Extensions;

/// <summary>
/// Applies the default local listening URL when no explicit address or port is configured.
/// </summary>
public static class HostUrlExtensions
{
    /// <summary>Default local URL used when no server URL or port configuration is present.</summary>
    public const string DefaultUrl = "http://localhost:5143";

    /// <summary>
    /// Configures <see cref="DefaultUrl"/> on the host unless <c>urls</c>, <c>HTTP_PORTS</c>,
    /// <c>HTTPS_PORTS</c>, or <c>Kestrel:Endpoints</c> is already configured.
    /// </summary>
    public static WebApplicationBuilder UseDefaultUrlWhenUnset(this WebApplicationBuilder builder)
    {
        if (!HasConfiguredUrl(builder.Configuration))
        {
            builder.WebHost.UseUrls(DefaultUrl);
        }

        return builder;
    }

    private static bool HasConfiguredUrl(IConfiguration configuration)
        => !string.IsNullOrWhiteSpace(configuration["urls"])
            || !string.IsNullOrWhiteSpace(configuration["HTTP_PORTS"])
            || !string.IsNullOrWhiteSpace(configuration["HTTPS_PORTS"])
            || configuration.GetSection("Kestrel:Endpoints").GetChildren().Any();
}
