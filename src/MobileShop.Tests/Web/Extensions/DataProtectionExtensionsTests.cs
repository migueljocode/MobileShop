using Microsoft.AspNetCore.DataProtection;
using MobileShop.Web.Extensions;

namespace MobileShop.Tests.Web.Extensions;

public class DataProtectionExtensionsTests
{
    [Fact]
    public void AddMobileShopDataProtection_sets_fixed_application_discriminator()
    {
        var builder = WebApplication.CreateBuilder();

        builder.Services.AddMobileShopDataProtection();

        using var provider = builder.Services.BuildServiceProvider();
        var options = provider.GetRequiredService<IOptions<DataProtectionOptions>>().Value;
        Assert.Equal(DataProtectionExtensions.ApplicationName, options.ApplicationDiscriminator);
    }
}
