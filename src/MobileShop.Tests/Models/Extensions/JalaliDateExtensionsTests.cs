namespace MobileShop.Tests.Models.Extensions;

public class JalaliDateExtensionsTests
{
    [Fact]
    public void ToJalaliDateString_converts_known_gregorian_dates()
    {
        Assert.Equal("1403/01/01", new DateTime(2024, 3, 20).ToJalaliDateString());
        Assert.Equal("1404/01/01", new DateTime(2025, 3, 21).ToJalaliDateString());
        Assert.Equal("1404/07/13", new DateTime(2025, 10, 5).ToJalaliDateString());
    }

    [Fact]
    public void ToJalaliDateTimeString_preserves_time_components()
    {
        var value = new DateTime(2025, 10, 5, 14, 30, 45, DateTimeKind.Utc);

        Assert.Equal("1404/07/13 14:30", value.ToJalaliDateTimeString());
        Assert.Equal(DateTimeKind.Utc, value.Kind);
    }

    [Fact]
    public void ToJalaliDateString_formats_default_date_without_throwing()
    {
        Assert.Equal("0001/10/11", default(DateTime).ToJalaliDateString());
    }
}
