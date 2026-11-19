using Microsoft.AspNetCore.DataProtection;

namespace MobileShop.Web.Extensions;

/// <summary>
/// Configures Data Protection for the MobileShop web host.
/// </summary>
public static class DataProtectionExtensions
{
    /// <summary>Fixed application discriminator shared across published release folders.</summary>
    public const string ApplicationName = "MobileShop";

    /// <summary>
    /// Sets a fixed Data Protection application name so cookie sessions and antiforgery tokens
    /// survive moving between versioned release directories.
    /// </summary>
    public static IServiceCollection AddMobileShopDataProtection(this IServiceCollection services)
    {
        services.AddDataProtection().SetApplicationName(ApplicationName);
        return services;
    }
}
