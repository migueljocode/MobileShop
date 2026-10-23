namespace MobileShop.Tests.Models.ViewModels;

public class TransactionFactorViewModelTests
{
    private static readonly DateTime SampleDate = new(2026, 1, 15, 10, 0, 0);

    private static TransactionFactorRowViewModel Row(
        int id,
        long price = 1_000,
        string specKey = "spec-a",
        string specLabel = "Blue",
        string personLabel = "Sara Ahmadi",
        string productLabel = "Apple iPhone 17") => new(
            id,
            SampleDate,
            TransactionDirection.Sell,
            productLabel,
            price,
            "Customer",
            personLabel,
            specKey,
            specLabel);

    private static TransactionFactorViewModel Factor(params TransactionFactorRowViewModel[] rows) =>
        new(rows, SampleDate);

    [Fact]
    public void Identical_rows_merge_into_one_line_with_their_count()
    {
        var factor = Factor(Row(1), Row(2), Row(3));

        var line = Assert.Single(factor.Lines);
        Assert.Equal(3, line.Quantity);
        Assert.Equal(1_000, line.UnitPrice);
        Assert.Equal(3_000, line.PaidAmount);
    }

    [Fact]
    public void Different_spec_keys_stay_on_separate_lines()
    {
        var factor = Factor(Row(1, specKey: "blue", specLabel: "Blue"), Row(2, specKey: "green", specLabel: "Green"));

        Assert.Equal(2, factor.Lines.Count);
        Assert.All(factor.Lines, line => Assert.Equal(1, line.Quantity));
    }

    [Fact]
    public void Same_specs_at_a_different_price_stay_on_separate_lines()
    {
        var factor = Factor(Row(1, price: 1_000), Row(2, price: 1_500));

        Assert.Equal(2, factor.Lines.Count);
    }

    [Fact]
    public void Same_specs_for_a_different_party_stay_on_separate_lines()
    {
        var factor = Factor(Row(1, personLabel: "Sara Ahmadi"), Row(2, personLabel: "Ali Rezaei"));

        Assert.Equal(2, factor.Lines.Count);
    }

    [Fact]
    public void Lines_keep_first_appearance_order()
    {
        var factor = Factor(
            Row(1, specKey: "green", specLabel: "Green"),
            Row(2, specKey: "blue", specLabel: "Blue"),
            Row(3, specKey: "green", specLabel: "Green"));

        Assert.Equal(new[] { "Apple iPhone 17 · Green", "Apple iPhone 17 · Blue" }, factor.Lines.Select(line => line.ProductText).ToArray());
    }

    [Fact]
    public void Product_text_omits_separator_when_spec_label_is_empty()
    {
        var line = Assert.Single(Factor(Row(1, specKey: string.Empty, specLabel: string.Empty)).Lines);

        Assert.Equal("Apple iPhone 17", line.ProductText);
    }

    [Fact]
    public void Total_is_the_sum_of_line_paid_amounts()
    {
        var factor = Factor(Row(1, price: 500), Row(2, price: 500), Row(3, price: 700, specKey: "other", specLabel: "Other"));

        Assert.Equal(factor.TotalPrice, factor.Lines.Sum(line => line.PaidAmount));
        Assert.Equal(1_700, factor.TotalPrice);
    }

    [Fact]
    public void Empty_factor_has_no_lines()
    {
        var factor = Factor();

        Assert.Empty(factor.Lines);
        Assert.Equal(0, factor.TotalPrice);
    }
}
