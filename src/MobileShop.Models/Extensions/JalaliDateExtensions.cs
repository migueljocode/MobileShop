using System.Globalization;
using System.Text.RegularExpressions;

namespace MobileShop.Models.Extensions;

/// <summary>
/// Converts <see cref="DateTime"/> values to and from the Persian (Jalali/Shamsi) calendar.
/// Values stay Gregorian everywhere else - these helpers only translate the calendar used for
/// display, for parsing what the user types, and for Shamsi period boundaries.
/// </summary>
public static partial class JalaliDateExtensions
{
    private static readonly PersianCalendar Calendar = new();

    // A four-digit year inside this window is read as a Shamsi year. Anything else (for example
    // 2026) is left to the Gregorian parser, so existing ISO values keep working unchanged.
    private const int MinShamsiYear = 1200;
    private const int MaxShamsiYear = 1600;

    /// <summary>Matches <c>1405/07/18</c>, <c>1405-07-18</c> or <c>1405.07.18</c>, optionally followed by a time.</summary>
    [GeneratedRegex(
        @"^\s*(?<year>\d{4})\s*[/\-.]\s*(?<month>\d{1,2})\s*[/\-.]\s*(?<day>\d{1,2})(?:[T\s]+(?<hour>\d{1,2})\s*:\s*(?<minute>\d{1,2})(?:\s*:\s*(?<second>\d{1,2}))?)?\s*$",
        RegexOptions.CultureInvariant)]
    private static partial Regex ShamsiDatePattern();

    /// <summary>Matches a leading four-digit year, used to tell a mistyped Shamsi date from a Gregorian one.</summary>
    [GeneratedRegex(@"^\s*(?<year>\d{4})", RegexOptions.CultureInvariant)]
    private static partial Regex LeadingYearPattern();

    public static string ToJalaliDateString(this DateTime value)
        => $"{Calendar.GetYear(value):0000}/{Calendar.GetMonth(value):00}/{Calendar.GetDayOfMonth(value):00}";

    public static string ToJalaliDateTimeString(this DateTime value)
        => $"{value.ToJalaliDateString()} {value:HH:mm}";

    /// <summary>Formats the Shamsi year and month only, for example <c>1405/07</c>.</summary>
    public static string ToJalaliYearMonthString(this DateTime value)
        => $"{Calendar.GetYear(value):0000}/{Calendar.GetMonth(value):00}";

    /// <summary>Formats the Shamsi year only, for example <c>1405</c>.</summary>
    public static string ToJalaliYearString(this DateTime value)
        => $"{Calendar.GetYear(value):0000}";

    /// <summary>Gets the Shamsi day of the month for <paramref name="value"/>.</summary>
    public static int JalaliDay(this DateTime value) => Calendar.GetDayOfMonth(value);

    /// <summary>Gets the number of days in the Shamsi month that contains <paramref name="value"/>.</summary>
    public static int JalaliMonthLength(this DateTime value)
        => Calendar.GetDaysInMonth(Calendar.GetYear(value), Calendar.GetMonth(value));

    /// <summary>
    /// Parses a Shamsi date such as <c>1405/07/18</c>, <c>1405-07-18</c> or <c>1405/07/18 14:30</c>.
    /// Returns <see langword="false"/> for a year outside the Shamsi window or for an impossible day.
    /// </summary>
    public static bool TryParseJalali(string? text, out DateTime value)
    {
        value = default;
        if (string.IsNullOrWhiteSpace(text))
            return false;

        var match = ShamsiDatePattern().Match(text);
        if (!match.Success)
            return false;

        var year = Parse(match, "year");
        var month = Parse(match, "month");
        var day = Parse(match, "day");
        var hour = match.Groups["hour"].Success ? Parse(match, "hour") : 0;
        var minute = match.Groups["minute"].Success ? Parse(match, "minute") : 0;
        var second = match.Groups["second"].Success ? Parse(match, "second") : 0;

        if (year is < MinShamsiYear or > MaxShamsiYear || month is < 1 or > 12)
            return false;
        if (hour > 23 || minute > 59 || second > 59)
            return false;

        try
        {
            value = Calendar.ToDateTime(year, month, day, hour, minute, second, 0);
            return true;
        }
        catch (ArgumentException)
        {
            // PersianCalendar rejects impossible days such as 1405/12/30 in a non-leap year.
            return false;
        }
    }

    /// <summary>
    /// Reports whether the text leads with a four-digit year that can only be a Shamsi year.
    /// Used to distinguish a mistyped Shamsi value from a Gregorian ISO value.
    /// </summary>
    public static bool LooksLikeJalaliDate(string? text)
    {
        if (string.IsNullOrWhiteSpace(text))
            return false;

        var match = LeadingYearPattern().Match(text);
        if (!match.Success)
            return false;

        var year = int.Parse(match.Groups["year"].Value, CultureInfo.InvariantCulture);
        return year is >= MinShamsiYear and <= MaxShamsiYear;
    }

    /// <summary>Gets the Saturday that starts the Shamsi week containing <paramref name="value"/>.</summary>
    public static DateTime StartOfJalaliWeek(this DateTime value)
    {
        // Shamsi weeks start on Saturday: Saturday shifts by 0, Sunday by 1, ... Friday by 6.
        var daysSinceSaturday = ((int)value.DayOfWeek + 1) % 7;
        return value.Date.AddDays(-daysSinceSaturday);
    }

    /// <summary>Gets the first day of the Shamsi month containing <paramref name="value"/>.</summary>
    public static DateTime StartOfJalaliMonth(this DateTime value)
        => Calendar.ToDateTime(Calendar.GetYear(value), Calendar.GetMonth(value), 1, 0, 0, 0, 0);

    /// <summary>Gets the first day of the Shamsi year containing <paramref name="value"/>.</summary>
    public static DateTime StartOfJalaliYear(this DateTime value)
        => Calendar.ToDateTime(Calendar.GetYear(value), 1, 1, 0, 0, 0, 0);

    /// <summary>Moves to the same day-of-month <paramref name="months"/> Shamsi months away, clamped to the month length.</summary>
    public static DateTime AddJalaliMonths(this DateTime value, int months)
    {
        var year = Calendar.GetYear(value);
        var monthIndex = Calendar.GetMonth(value) - 1 + months;
        var targetYear = year + (int)Math.Floor(monthIndex / 12d);
        var targetMonth = (int)(((monthIndex % 12) + 12) % 12) + 1;
        var day = Math.Min(Calendar.GetDayOfMonth(value), Calendar.GetDaysInMonth(targetYear, targetMonth));
        return Calendar.ToDateTime(targetYear, targetMonth, day, value.Hour, value.Minute, value.Second, value.Millisecond);
    }

    /// <summary>Moves the value <paramref name="years"/> Shamsi years ahead, clamped for leap-day values.</summary>
    public static DateTime AddJalaliYears(this DateTime value, int years)
    {
        var targetYear = Calendar.GetYear(value) + years;
        var month = Calendar.GetMonth(value);
        var day = Math.Min(Calendar.GetDayOfMonth(value), Calendar.GetDaysInMonth(targetYear, month));
        return Calendar.ToDateTime(targetYear, month, day, value.Hour, value.Minute, value.Second, value.Millisecond);
    }

    private static int Parse(Match match, string group)
        => int.Parse(match.Groups[group].Value, CultureInfo.InvariantCulture);
}
