namespace MobileShop.Tests.Dal.Initialization;

public class DatabaseInitializerTests
{
    [Fact]
    public void EnsureDevelopmentIsSafe_throws_when_published_layout()
    {
        var exception = Assert.Throws<InvalidOperationException>(
            () => DatabaseInitializer.EnsureDevelopmentIsSafe(publishedLayout: true));

        Assert.Contains("published data folder", exception.Message, StringComparison.Ordinal);
        Assert.Contains("ASPNETCORE_ENVIRONMENT=Development", exception.Message, StringComparison.Ordinal);
    }

    [Fact]
    public void EnsureDevelopmentIsSafe_does_not_throw_when_not_published_layout()
    {
        DatabaseInitializer.EnsureDevelopmentIsSafe(publishedLayout: false);
    }
}
