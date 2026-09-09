namespace MobileShop.Tests.Models.Extensions;

public class SecondHandExtensionsTests
{
    [Theory]
    [InlineData(0, 0, 0, 0)]
    [InlineData(30, 0, 1, 0)]
    [InlineData(400, 1, 1, 5)]
    public void GetUsedDurationBreakdown_ConvertsDaysCorrectly(int totalDays, int expectedYears, int expectedMonths, int expectedDays)
    {
        var secondHand = new SecondHand { UsedDurationDays = totalDays };

        var (years, months, days) = secondHand.GetUsedDurationBreakdown();

        Assert.Equal(expectedYears, years);
        Assert.Equal(expectedMonths, months);
        Assert.Equal(expectedDays, days);
    }

    [Fact]
    public void GetUsedDurationBreakdown_TreatsNullAsZero()
    {
        var secondHand = new SecondHand { UsedDurationDays = null };

        var (years, months, days) = secondHand.GetUsedDurationBreakdown();

        Assert.Equal(0, years);
        Assert.Equal(0, months);
        Assert.Equal(0, days);
    }
}
