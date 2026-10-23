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
    [InlineData(14, 30, "ب.ظ 02:30 1404/07/13")]
    [InlineData(9, 5, "ق.ظ 09:05 1404/07/13")]
    [InlineData(0, 10, "ق.ظ 12:10 1404/07/13")]
    [InlineData(12, 0, "ب.ظ 12:00 1404/07/13")]
    [InlineData(23, 59, "ب.ظ 11:59 1404/07/13")]
    public void ToJalaliDateTimeMeridiemRtlString_lists_marker_time_then_date(int hour, int minute, string expected)
        => Assert.Equal(expected, new DateTime(2025, 10, 5, hour, minute, 0).ToJalaliDateTimeMeridiemRtlString());
}
