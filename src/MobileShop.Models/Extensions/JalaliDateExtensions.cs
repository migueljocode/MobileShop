using System.Globalization;

namespace MobileShop.Models.Extensions;

/// <summary>
/// Formats <see cref="DateTime"/> values using the Persian (Jalali) calendar
/// without changing the stored value or its <see cref="DateTime.Kind"/>.
/// </summary>
public static class JalaliDateExtensions
{
    private static readonly PersianCalendar Calendar = new();

    public static string ToJalaliDateString(this DateTime value)
        => $"{Calendar.GetYear(value):0000}/{Calendar.GetMonth(value):00}/{Calendar.GetDayOfMonth(value):00}";

    public static string ToJalaliDateTimeString(this DateTime value)
        => $"{value.ToJalaliDateString()} {value:HH:mm}";
}
