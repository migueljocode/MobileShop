using System.Globalization;

namespace MobileShop.Models.Extensions;

public static class MoneyExtensions
{
    public static string ToGroupedDigits(this long value)
        => value.ToString("N0", CultureInfo.InvariantCulture);

    public static string ToGroupedDigits(this long? value)
        => value.HasValue ? value.Value.ToGroupedDigits() : string.Empty;

    public static string ToIrr(this long value)
        => $"{value.ToGroupedDigits()} IRR";

    public static string ToIrr(this long? value)
        => value.HasValue ? value.Value.ToIrr() : string.Empty;
}
