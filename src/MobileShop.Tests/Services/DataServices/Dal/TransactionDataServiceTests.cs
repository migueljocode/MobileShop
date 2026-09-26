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
}
