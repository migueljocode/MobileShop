using MobileShop.Web.Extensions;

namespace MobileShop.Tests.Web.Extensions;

public class VersionInfoTests
{
    [Fact]
    public void Describe_returns_non_empty_version_for_web_assembly()
    {
        var version = VersionInfo.Describe(typeof(VersionInfo).Assembly);
        Assert.False(string.IsNullOrWhiteSpace(version));
    }
}
