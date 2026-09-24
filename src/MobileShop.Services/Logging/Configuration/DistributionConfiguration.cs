namespace MobileShop.Services.Logging.Configuration;

/// <summary>
/// Configures the profit distribution settings.
/// </summary>
public static class DistributionConfiguration
{
    /// <summary>
    /// Configures the DistributionSettings from the application configuration.
    /// </summary>
    /// <param name="builder">The web application builder.</param>
    /// <returns>The builder for chaining.</param>
    public static WebApplicationBuilder ConfigureDistribution(this WebApplicationBuilder builder)
    {
        builder.Services.Configure<DistributionSettings>(
            builder.Configuration.GetSection("Distribution"));
        return builder;
    }
}