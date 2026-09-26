using Moq;
using MobileShop.Models.ViewModels;
using MobileShop.Services.PDF;

namespace MobileShop.Tests.Services.DataServices.Dal;

public class TransactionDataServiceTests : RepoTestBase
{
    private readonly TransactionDataService _service;
    private readonly Mock<IPdfGenerator> _pdfGeneratorMock;

    public TransactionDataServiceTests()
    {
        _pdfGeneratorMock = new Mock<IPdfGenerator>();
        _pdfGeneratorMock.Setup(p => p.Generate(It.IsAny<InvoiceViewModel>())).Returns([]);
        _pdfGeneratorMock.Setup(p => p.GenerateTransactionFactor(It.IsAny<TransactionFactorViewModel>())).Returns([0x25, 0x50, 0x44, 0x46]);
        _service = new TransactionDataService(new TransactionRepo(Context), NullLogger<TransactionDataService>.Instance, _pdfGeneratorMock.Object);
    }

    [Fact]
    public async Task RecordBuy_and_RecordSell_and_report_lookups_work()
    {
        var sellerPerson = new Person { FirstName = "Seller", LastName = "Two", PhoneNumber = "09120000003" };
        Context.People.Add(sellerPerson);
        Context.SaveChanges();

        var seller = new Seller { PersonId = sellerPerson.Id, EntityType = SellerEntityType.Real };
        Context.Sellers.Add(seller);
        Context.SaveChanges();

        var customerPerson = new Person { FirstName = "Customer", LastName = "Two", PhoneNumber = "09120000004" };
        Context.People.Add(customerPerson);
        Context.SaveChanges();

        var customer = new Customer { PersonId = customerPerson.Id, NationalId = "1000000002" };
        Context.Customers.Add(customer);
        Context.SaveChanges();

        var product = TestDataHelpers.CreateProduct(Context, 320m);

        var bought = _service.RecordBuy(product.Id, seller.Id, 280m, DateTime.UtcNow);
        Assert.True(bought);

        var sell = _service.RecordSell(product.Id, customer.Id, 360m, DateTime.UtcNow.AddMinutes(1));
        Assert.True(sell);

        var byProduct = _service.GetByProduct(product.Id).ToList();
        Assert.Equal(2, byProduct.Count);

        var recent = _service.GetRecent(10).ToList();
        Assert.NotEmpty(recent);

        var boughtByShop = _service.GetProductsBoughtByShop().ToList();
        var soldByShop = _service.GetProductsSoldByShop().ToList();
        Assert.Contains(boughtByShop, p => p.Id == product.Id);
        Assert.Contains(soldByShop, p => p.Id == product.Id);

        var productsBoughtByShopAsync = (await _service.GetProductsBoughtByShopAsync()).ToList();
        var productsSoldByShopAsync = (await _service.GetProductsSoldByShopAsync()).ToList();
        Assert.Contains(productsBoughtByShopAsync, p => p.Id == product.Id);
        Assert.Contains(productsSoldByShopAsync, p => p.Id == product.Id);

        var recordBuyAsync = await _service.RecordBuyAsync(product.Id, seller.Id, 320m, DateTime.UtcNow.AddMinutes(2));
        var recordSellAsync = await _service.RecordSellAsync(product.Id, customer.Id, 420m, DateTime.UtcNow.AddMinutes(3));
        Assert.False(recordBuyAsync);
        Assert.False(recordSellAsync);
    }

    [Fact]
    public void GenerateTransactionsPdf_passes_real_factor_rows_to_pdf_generator()
    {
        TestDataHelpers.SeedShopSentinels(Context);


        var sellerPerson = new Person { FirstName = "Seller", LastName = "Pdf", PhoneNumber = "09120000003" };
        Context.People.Add(sellerPerson);
        Context.SaveChanges();

        var seller = new Seller { PersonId = sellerPerson.Id, EntityType = SellerEntityType.Real };
        Context.Sellers.Add(seller);
        Context.SaveChanges();

        var product = TestDataHelpers.CreateProduct(Context, 500m);
        _service.RecordBuy(product.Id, seller.Id, 450m, DateTime.UtcNow);

        TransactionFactorViewModel? capturedModel = null;
        _pdfGeneratorMock
            .Setup(p => p.GenerateTransactionFactor(It.IsAny<TransactionFactorViewModel>()))
            .Callback<TransactionFactorViewModel>(m => capturedModel = m)
            .Returns([0x25, 0x50, 0x44, 0x46]);

        var bytes = _service.GenerateTransactionsPdf("buy", 10, "desc");

        Assert.NotEmpty(bytes);
        Assert.NotNull(capturedModel);
        Assert.Single(capturedModel!.Rows);
        var row = capturedModel.Rows[0];
        Assert.Equal(TransactionDirection.Buy, row.Direction);
        Assert.Equal(450m, row.FinishedPrice);
        Assert.Equal("Seller", row.PersonRole);
        Assert.Equal("Seller Pdf", row.PersonLabel);
    }

    [Fact]
    public async Task RecordBuyAsync_successfully_persists_buy_transaction()
    {
        TestDataHelpers.SeedShopSentinels(Context);

        var product = TestDataHelpers.CreateProduct(Context, 500m);

        var result = await _service.RecordBuyAsync(product.Id, 1, 450m, new DateTime(2026, 1, 15, 12, 0, 0, DateTimeKind.Utc));

        Assert.True(result);

        var transaction = Context.Transactions.Single(t => t.ProductId == product.Id);
        Assert.Equal(TransactionDirection.Buy, transaction.Direction);
        Assert.Equal(450m, transaction.FinishedPrice);
        Assert.Equal(1, transaction.SellerId);
        Assert.Equal(1, transaction.CustomerId);
        Assert.Equal(new DateTime(2026, 1, 15, 12, 0, 0, DateTimeKind.Utc), transaction.Date);
    }

    [Fact]
    public async Task RecordSellAsync_successfully_persists_sell_transaction_after_buy()
    {
        TestDataHelpers.SeedShopSentinels(Context);

        var product = TestDataHelpers.CreateProduct(Context, 500m);
        await _service.RecordBuyAsync(product.Id, 1, 450m, new DateTime(2026, 1, 15, 12, 0, 0, DateTimeKind.Utc));

        var customerPerson = new Person { FirstName = "Buyer", LastName = "Test", PhoneNumber = "09120000099" };
        Context.People.Add(customerPerson);
        Context.SaveChanges();
        var customer = new Customer { PersonId = customerPerson.Id, NationalId = "9999999999" };
        Context.Customers.Add(customer);
        Context.SaveChanges();

        var result = await _service.RecordSellAsync(product.Id, customer.Id, 600m, new DateTime(2026, 1, 20, 14, 0, 0, DateTimeKind.Utc));

        Assert.True(result);

        var transactions = Context.Transactions.Where(t => t.ProductId == product.Id).OrderBy(t => t.Direction).ToList();
        Assert.Equal(2, transactions.Count);
        Assert.Equal(TransactionDirection.Buy, transactions[0].Direction);
        Assert.Equal(450m, transactions[0].FinishedPrice);
        Assert.Equal(TransactionDirection.Sell, transactions[1].Direction);
        Assert.Equal(600m, transactions[1].FinishedPrice);
        Assert.Equal(customer.Id, transactions[1].CustomerId);
    }

    [Theory]
    [InlineData((double)-100)]
    [InlineData((double)-0.01)]
    public async Task RecordBuyAsync_rejects_negative_price(double price)
    {
        TestDataHelpers.SeedShopSentinels(Context);
        var product = TestDataHelpers.CreateProduct(Context, 500m);

        var result = await _service.RecordBuyAsync(product.Id, 1, (decimal)price);

        Assert.False(result);
        Assert.Empty(Context.Transactions);
    }

    [Theory]
    [InlineData((double)-100)]
    [InlineData((double)-0.01)]
    public async Task RecordSellAsync_rejects_negative_price(double price)
    {
        TestDataHelpers.SeedShopSentinels(Context);
        var product = TestDataHelpers.CreateProduct(Context, 500m);

        var result = await _service.RecordSellAsync(product.Id, 1, (decimal)price);

        Assert.False(result);
        Assert.Empty(Context.Transactions);
    }

    [Fact]
    public async Task GetEarliestTransactionDateAsync_returns_date_only_normalized()
    {
        TestDataHelpers.SeedShopSentinels(Context);
        var product1 = TestDataHelpers.CreateProduct(Context, 500m);
        var product2 = TestDataHelpers.CreateProduct(Context, 500m);

        var laterDate = new DateTime(2026, 3, 15, 14, 30, 0);
        var earlierDate = new DateTime(2026, 1, 5, 9, 15, 0);
        await _service.RecordBuyAsync(product1.Id, 1, 300m, laterDate);
        await _service.RecordBuyAsync(product2.Id, 1, 300m, earlierDate);

        var earliest = await _service.GetEarliestTransactionDateAsync();

        Assert.NotNull(earliest);
        Assert.Equal(earlierDate.Date, earliest!.Value);
        Assert.Equal(TimeSpan.Zero, earliest!.Value.TimeOfDay);
    }

    [Fact]
    public async Task GetEarliestTransactionDateAsync_returns_null_when_empty()
    {
        var result = await _service.GetEarliestTransactionDateAsync();

        Assert.Null(result);
    }

    [Fact]
    public async Task GetProfitLossRowsAsync_inclusive_bounds_include_transactions_on_both_From_and_To()
    {
        TestDataHelpers.SeedShopSentinels(Context);

        var product1 = TestDataHelpers.CreateProduct(Context, 100m);
        var product2 = TestDataHelpers.CreateProduct(Context, 200m);
        var product3 = TestDataHelpers.CreateProduct(Context, 300m);

        var fromDate = new DateTime(2025, 6, 15);
        var toDate = new DateTime(2025, 6, 20);
        var beforeFrom = fromDate.AddDays(-1);
        var afterTo = toDate.AddDays(1);

        // Buy transactions (cost) - we only care about sell (revenue) for profit calculation
        // Record buy for each product so we can record sell
        await _service.RecordBuyAsync(product1.Id, 1, 80m, fromDate);       // On From date
        await _service.RecordBuyAsync(product2.Id, 1, 150m, toDate);         // On To date
        await _service.RecordBuyAsync(product3.Id, 1, 250m, beforeFrom);     // Before From

        // Sell transactions (revenue)
        await _service.RecordSellAsync(product1.Id, 1, 120m, fromDate);      // On From date - should be included
        await _service.RecordSellAsync(product2.Id, 1, 250m, toDate);        // On To date - should be included
        await _service.RecordSellAsync(product3.Id, 1, 300m, afterTo);       // After To - should be excluded

        var rows = await _service.GetProfitLossRowsAsync(fromDate, toDate);

        // Only product1 (From date) and product2 (To date) should be in results
        // product3 (after To) should be excluded
        Assert.Equal(2, rows.Count);
        Assert.Contains(rows, r => r.ProductId == product1.Id);
        Assert.Contains(rows, r => r.ProductId == product2.Id);
        Assert.DoesNotContain(rows, r => r.ProductId == product3.Id);

        // Verify profit calculations are correct for included transactions
        var row1 = rows.First(r => r.ProductId == product1.Id);
        Assert.Equal(120m - 80m, row1.Profit); // 40 profit

        var row2 = rows.First(r => r.ProductId == product2.Id);
        Assert.Equal(250m - 150m, row2.Profit); // 100 profit
    }

    [Fact]
    public async Task GetProfitLossTotalAsync_inclusive_bounds_sum_matches_rows()
    {
        TestDataHelpers.SeedShopSentinels(Context);

        var product1 = TestDataHelpers.CreateProduct(Context, 100m);
        var product2 = TestDataHelpers.CreateProduct(Context, 200m);

        var fromDate = new DateTime(2025, 6, 15);
        var toDate = new DateTime(2025, 6, 20);

        await _service.RecordBuyAsync(product1.Id, 1, 80m, fromDate);
        await _service.RecordBuyAsync(product2.Id, 1, 150m, toDate);

        await _service.RecordSellAsync(product1.Id, 1, 120m, fromDate);
        await _service.RecordSellAsync(product2.Id, 1, 250m, toDate);

        var total = await _service.GetProfitLossTotalAsync(fromDate, toDate);
        var rows = await _service.GetProfitLossRowsAsync(fromDate, toDate);

        // Total should equal sum of row profits
        Assert.Equal(rows.Sum(r => r.Profit), total);
        Assert.Equal(40m + 100m, total); // 140 total profit
    }
}
