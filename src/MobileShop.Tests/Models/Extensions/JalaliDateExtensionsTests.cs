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

    [Theory]
    [InlineData(14, 30, "1404/07/13 02:30 ب.ظ")]
    [InlineData(9, 5, "1404/07/13 09:05 ق.ظ")]
    [InlineData(0, 10, "1404/07/13 12:10 ق.ظ")]
    [InlineData(12, 0, "1404/07/13 12:00 ب.ظ")]
    [InlineData(23, 59, "1404/07/13 11:59 ب.ظ")]
    public void ToJalaliDateTimeMeridiemString_uses_12_hour_clock_with_persian_marker(int hour, int minute, string expected)
        => Assert.Equal(expected, new DateTime(2025, 10, 5, hour, minute, 0).ToJalaliDateTimeMeridiemString());
}
