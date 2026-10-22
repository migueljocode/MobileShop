namespace MobileShop.Tests.Models.Extensions;

public class PhoneNumberExtensionsTests
{
    [Theory]
    [InlineData("۰۹۱۱۵۰۰۹۱۰۹", "۹۱۰۹ ۵۰۰ ۰۹۱۱")]
    [InlineData("09115009109", "9109 500 0911")]
    [InlineData("۰۹۱۱ ۵۰۰ ۹۱۰۹", "۹۱۰۹ ۵۰۰ ۰۹۱۱")]
    [InlineData("  ۰۹۱۱   ۵۰۰\t۹۱۰۹  ", "۹۱۰۹ ۵۰۰ ۰۹۱۱")]
    [InlineData("۱۲۳۴۵", "۱۲۳۴۵")]
    public void ToRtlDisplayPhone_reverses_phone_groups(string input, string expected)
        => Assert.Equal(expected, input.ToRtlDisplayPhone());

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    public void ToRtlDisplayPhone_returns_empty_for_null_or_blank(string? input)
        => Assert.Equal(string.Empty, input.ToRtlDisplayPhone());
}
