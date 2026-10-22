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
}
