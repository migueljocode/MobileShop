namespace MobileShop.Tests.Models.Extensions;

public class MoneyExtensionsTests
{
    [Fact]
    public void ToGroupedDigits_formats_whole_rials_with_invariant_grouping()
    {
        Assert.Equal("0", 0L.ToGroupedDigits());
        Assert.Equal("1 234 567", 1_234_567L.ToGroupedDigits());
        Assert.Equal("4 500 000 000", 4_500_000_000L.ToGroupedDigits());
        Assert.Equal("-1 500", (-1_500L).ToGroupedDigits());
    }

    [Fact]
    public void ToIrr_appends_the_toman_unit()
    {
        Assert.Equal("1 234 567 Toman", 1_234_567L.ToIrr());
    }

    [Fact]
    public void Nullable_money_returns_empty_for_null()
    {
        long? value = null;

        Assert.Equal(string.Empty, value.ToGroupedDigits());
        Assert.Equal(string.Empty, value.ToIrr());
    }

    [Fact]
    public void ToPersianDigits_converts_ascii_digits_to_persian_digits()
    {
        Assert.Equal("۱۲۳", "123".ToPersianDigits());
        Assert.Equal("۱ ۲۳۴ ۵۶۷", "1 234 567".ToPersianDigits());
    }

    [Fact]
    public void ToPersianDigits_keeps_non_digit_characters()
    {
        Assert.Equal("مبلغ ۱۲۳ ریال", "مبلغ 123 ریال".ToPersianDigits());
        Assert.Equal(string.Empty, string.Empty.ToPersianDigits());
    }
}
