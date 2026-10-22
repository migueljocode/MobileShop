namespace MobileShop.Tests.Models.Extensions;

public class PhoneNumberExtensionsTests
{
    [Theory]
    [InlineData("0911 500 9109", "0911\u00A0500\u00A09109")]
    [InlineData("۰۹۱۱ ۵۰۰ ۹۱۰۹", "۰۹۱۱\u00A0۵۰۰\u00A0۹۱۰۹")]
    [InlineData("0911   500\t9109", "0911\u00A0500\u00A09109")]
    [InlineData("  0911 500 9109  ", "0911\u00A0500\u00A09109")]
    [InlineData("0911-500-9109", "0911-500-9109")]
    [InlineData("+98 911 500 9109", "+98\u00A0911\u00A0500\u00A09109")]
    [InlineData("09115009109", "09115009109")]
    public void ToBidiSafePhone_joins_parts_with_no_break_spaces(string input, string expected)
        => Assert.Equal(expected, input.ToBidiSafePhone());

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    public void ToBidiSafePhone_returns_empty_for_null_or_blank(string? input)
        => Assert.Equal(string.Empty, input.ToBidiSafePhone());
}
