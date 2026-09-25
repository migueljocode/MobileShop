using MobileShop.Models.ViewModels;

namespace MobileShop.Tests.Models.Extensions;

public class TransactionFactorExtensionsTests
{
    private static readonly DateTime SampleDate = new(2026, 1, 15, 10, 0, 0);

    [Fact]
    public void Sale_row_is_written_to_the_customer()
    {
        var transaction = new TransactionListItemViewModel(
            7, SampleDate, TransactionDirection.Sell, "Apple iPhone 13", 45_000_000m, "Shop", "Sara Ahmadi");

        var row = transaction.ToFactorRow();

        Assert.Equal(7, row.TransactionId);
        Assert.Equal(SampleDate, row.Date);
        Assert.Equal(TransactionDirection.Sell, row.Direction);
        Assert.Equal("Apple iPhone 13", row.ProductLabel);
        Assert.Equal(45_000_000m, row.FinishedPrice);
        Assert.Equal("Customer", row.PersonRole);
        Assert.Equal("Sara Ahmadi", row.PersonLabel);
    }

    [Fact]
    public void Buy_row_is_written_to_the_seller()
    {
        var transaction = new TransactionListItemViewModel(
            8, SampleDate, TransactionDirection.Buy, "Anker 20W Charger", 900_000m, "Ali Rezaei", "Shop");

        var row = transaction.ToFactorRow();

        Assert.Equal("Seller", row.PersonRole);
        Assert.Equal("Ali Rezaei", row.PersonLabel);
    }

    [Fact]
    public void Details_row_keeps_the_supplied_identifier()
    {
        var details = new TransactionDetailsViewModel(
            SampleDate, TransactionDirection.Sell, 1_200_000m, "Anker 20W Charger", "Shop", "Sara Ahmadi");

        var row = details.ToFactorRow(42);

        Assert.Equal(42, row.TransactionId);
        Assert.Equal("Customer", row.PersonRole);
        Assert.Equal("Sara Ahmadi", row.PersonLabel);
    }

    [Theory]
    [InlineData(TransactionDirection.Sell, "Customer")]
    [InlineData(TransactionDirection.Buy, "Seller")]
    public void PersonRole_reflects_the_direction(TransactionDirection direction, string expectedRole)
        => Assert.Equal(expectedRole, TransactionFactorExtensions.PersonRole(direction));

    [Fact]
    public void Factor_total_sums_every_row()
    {
        var model = new TransactionFactorViewModel(
            [
                new TransactionFactorRowViewModel(1, SampleDate, TransactionDirection.Buy, "Apple iPhone 13", 100m, "Seller", "Ali"),
                new TransactionFactorRowViewModel(2, SampleDate, TransactionDirection.Sell, "Apple iPhone 13", 250m, "Customer", "Sara")
            ],
            SampleDate);

        Assert.Equal(350m, model.TotalPrice);
    }
}