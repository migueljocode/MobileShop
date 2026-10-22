namespace MobileShop.Models.Extensions;

public static class PhoneNumberExtensions
{
    private const char NoBreakSpace = '\u00A0';

    /// <summary>
    /// Joins the parts of a phone number with no-break spaces so right-to-left text keeps it in typed order.
    /// </summary>
    public static string ToBidiSafePhone(this string? value)
    {
        if (string.IsNullOrWhiteSpace(value))
            return string.Empty;

        return string.Join(NoBreakSpace, value.Split((char[]?)null, StringSplitOptions.RemoveEmptyEntries));
    }

    /// <summary>
    /// Reorders the groups of a phone number (an unspaced 11-digit number is split 4-3-4) from last to first,
    /// so a right-to-left layout displays it in its natural left-to-right order.
    /// </summary>
    public static string ToRtlDisplayPhone(this string? value)
    {
        if (string.IsNullOrWhiteSpace(value))
            return string.Empty;

        var groups = value.Split((char[]?)null, StringSplitOptions.RemoveEmptyEntries);
        if (groups is [{ Length: 11 } number])
            groups = [number[..4], number[4..7], number[7..]];

        Array.Reverse(groups);
        return string.Join(' ', groups);
    }
}
