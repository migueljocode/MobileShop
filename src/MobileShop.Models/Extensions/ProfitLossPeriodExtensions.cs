using System.Globalization;

namespace MobileShop.Models.Extensions;

/// <summary>
/// Maps profit/loss trend intervals onto Shamsi (Jalali) calendar periods so the report filters,
/// the chart axis and the stored transactions all speak the same calendar.
/// </summary>
public static class ProfitLossPeriodExtensions
{
    /// <summary>Gets the start of the Shamsi period that contains <paramref name="value"/>.</summary>
    public static DateTime ToJalaliPeriodStart(this DateTime value, ProfitLossInterval interval) => interval switch
    {
        ProfitLossInterval.Year => value.StartOfJalaliYear(),
        ProfitLossInterval.Month => value.StartOfJalaliMonth(),
        ProfitLossInterval.Week => value.StartOfJalaliWeek(),
        ProfitLossInterval.Day => value.Date,
        ProfitLossInterval.Hour => new DateTime(value.Year, value.Month, value.Day, value.Hour, 0, 0, value.Kind),
        _ => throw new ArgumentOutOfRangeException(nameof(interval), interval, "Unsupported profit/loss interval.")
    };

    /// <summary>Gets the start of the Shamsi period that follows <paramref name="period"/>.</summary>
    public static DateTime NextJalaliPeriod(this DateTime period, ProfitLossInterval interval) => interval switch
    {
        ProfitLossInterval.Year => period.AddJalaliYears(1),
        ProfitLossInterval.Month => period.AddJalaliMonths(1),
        ProfitLossInterval.Week => period.AddDays(7),
        ProfitLossInterval.Day => period.AddDays(1),
        ProfitLossInterval.Hour => period.AddHours(1),
        _ => throw new ArgumentOutOfRangeException(nameof(interval), interval, "Unsupported profit/loss interval.")
    };

    /// <summary>Formats a Shamsi period start for the profit/loss chart axis.</summary>
    public static string ToJalaliPeriodLabel(this DateTime period, ProfitLossInterval interval) => interval switch
    {
        ProfitLossInterval.Year => period.ToJalaliYearString(),
        ProfitLossInterval.Month => period.ToJalaliYearMonthString(),
        ProfitLossInterval.Week => $"Week of {period.ToJalaliDateString()}",
        ProfitLossInterval.Day => period.ToJalaliDateString(),
        ProfitLossInterval.Hour => period.ToString("h tt", CultureInfo.InvariantCulture),
        _ => period.ToJalaliDateString()
    };
}
