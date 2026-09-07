namespace MobileShop.Dal.Extensions;

public static class SecondHandExtensions
{
    // display helper - converts the raw day count on demand, using 365/30-day approximations
    // since there's no start date on record to do real calendar arithmetic against
    public static (int Years, int Months, int Days) GetUsedDurationBreakdown(this SecondHand secondHand)
    {
        var totalDays = secondHand.UsedDurationDays ?? 0;
        var years = totalDays / 365;
        var remainder = totalDays % 365;
        var months = remainder / 30;
        var days = remainder % 30;
        return (years, months, days);
    }
}
