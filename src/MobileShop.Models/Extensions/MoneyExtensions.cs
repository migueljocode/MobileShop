using System.Globalization;

namespace MobileShop.Models.Extensions;

public static class MoneyExtensions
{
    public static string ToGroupedDigits(this long value)
        => value.ToString("#,0", CultureInfo.InvariantCulture).Replace(',', ' ');

    public static string ToGroupedDigits(this long? value)
        => value.HasValue ? value.Value.ToGroupedDigits() : string.Empty;

    public static string ToToman(this long value)
        => $"{value.ToGroupedDigits()} Toman";

    public static string ToToman(this long? value)
        => value.HasValue ? value.Value.ToToman() : string.Empty;

    public static string ToIrr(this long value)
        => value.ToToman();

    public static string ToIrr(this long? value)
        => value.HasValue ? value.Value.ToIrr() : string.Empty;
}
