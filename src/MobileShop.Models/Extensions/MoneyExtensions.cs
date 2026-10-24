namespace MobileShop.Models.Extensions;

public static class MoneyExtensions
{
    /// <summary>Formats a money amount with a space separating the digit groups.</summary>
    /// <param name="value">The amount in Rials.</param>
    /// <returns>The grouped amount, for example <c>1 234 567</c>.</returns>
    public static string ToGroupedDigits(this long value)
        => value.ToString("#,0", CultureInfo.InvariantCulture).Replace(',', ' ');

    /// <summary>Formats a nullable money amount with a space separating the digit groups.</summary>
    /// <param name="value">The amount in Rials.</param>
    /// <returns>The grouped amount, or an empty string when the amount is missing.</returns>
    public static string ToGroupedDigits(this long? value)
        => value.HasValue ? value.Value.ToGroupedDigits() : string.Empty;

    /// <summary>Formats a money amount followed by the unit.</summary>
    /// <param name="value">The amount in Rials.</param>
    /// <returns>The grouped amount followed by the unit, for example <c>1 234 567 Toman</c>.</returns>
    public static string ToToman(this long value)
        => $"{value.ToGroupedDigits()} Toman";

    /// <summary>Formats a nullable money amount followed by the unit.</summary>
    /// <param name="value">The amount in Rials.</param>
    /// <returns>The grouped amount followed by the unit, or an empty string when the amount is missing.</returns>
    public static string ToToman(this long? value)
        => value.HasValue ? value.Value.ToToman() : string.Empty;

    /// <summary>Formats a money amount followed by the unit.</summary>
    /// <param name="value">The amount in Rials.</param>
    /// <returns>The grouped amount followed by the unit, for example <c>1 234 567 Toman</c>.</returns>
    public static string ToIrr(this long value)
        => value.ToToman();

    /// <summary>Formats a nullable money amount followed by the unit.</summary>
    /// <param name="value">The amount in Rials.</param>
    /// <returns>The grouped amount followed by unit, or an empty string when the amount is missing.</returns>
    public static string ToIrr(this long? value)
        => value.HasValue ? value.Value.ToIrr() : string.Empty;

    /// <summary>
    /// Converts every ASCII digit in <paramref name="value"/> to its Persian digit
    /// (<c>0</c>-<c>9</c> become <c>۰</c>-<c>۹</c>). All other characters are kept as they are.
    /// </summary>
    /// <param name="value">The text to convert.</param>
    /// <returns>The text with Persian digits, or the original value when it is null or empty.</returns>
    public static string ToPersianDigits(this string value)
    {
        if (string.IsNullOrEmpty(value))
            return value;

        var characters = value.ToCharArray();
        for (var index = 0; index < characters.Length; index++)
        {
            if (characters[index] is >= '0' and <= '9')
                characters[index] = (char)('\u06F0' + (characters[index] - '0'));
        }

        return new string(characters);
    }
}
