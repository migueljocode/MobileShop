namespace MobileShop.Tests.Services.DataServices.Dal;

public class HomeDataServiceTests : RepoTestBase
{
    private readonly HomeDataService _service;

    public HomeDataServiceTests()
    {
        _service = new HomeDataService(
            new BaseRepo<Phone>(Context),
            new BaseRepo<AppleId>(Context),
            new BaseRepo<Transaction>(Context),
            NullLogger<HomeDataService>.Instance);
    }

    [Fact]
    public async Task GetStockAsync_returns_exact_counts_for_phone_and_apple_id_inventory()
    {
        var product = TestDataHelpers.CreateProduct(Context);
        Context.Phones.Add(new Phone
        {
            ProductId = product.Id,
            ProductNavigation = product,
            IMEI1 = TestDataHelpers.GenerateImei(),
            IMEI2 = TestDataHelpers.GenerateImei(),
            OwnershipTransferred = false
        });
        var secondHandProduct = TestDataHelpers.CreateProduct(Context);
        Context.SecondHands.Add(new SecondHand
        {
            ProductId = secondHandProduct.Id,
            ProductNavigation = secondHandProduct,
            TestPeriodDays = 3,
            UsedDurationDays = 1
        });
        Context.Phones.Add(new Phone
        {
            ProductId = secondHandProduct.Id,
            ProductNavigation = secondHandProduct,
            IMEI1 = TestDataHelpers.GenerateImei(),
            IMEI2 = TestDataHelpers.GenerateImei(),
            OwnershipTransferred = false
        });
        var appleIdProduct = TestDataHelpers.CreateProduct(Context);
        Context.AppleIds.Add(new AppleId
        {
            ProductId = appleIdProduct.Id,
            ProductNavigation = appleIdProduct,
            Email = "stock@example.com",
            Password = "secret"
        });
        Context.SaveChanges();

        var stock = await _service.GetStockAsync();

        Assert.Equal(2, stock.PhonesInStock);
        Assert.Equal(1, stock.PhonesSecondHand);
        Assert.Equal(1, stock.PhonesSecondHandAvailable);
        Assert.Equal(1, stock.AppleIdsInStock);
    }

    [Fact]
    public async Task GetStockAsync_excludes_sold_second_hand_phone_from_available_count()
    {
        TestDataHelpers.SeedShopSentinels(Context);
        var product = TestDataHelpers.CreateProduct(Context);
        Context.SecondHands.Add(new SecondHand
        {
            ProductId = product.Id,
            ProductNavigation = product,
            TestPeriodDays = 3,
            UsedDurationDays = 1
        });
        var phone = new Phone
        {
            ProductId = product.Id,
            ProductNavigation = product,
            IMEI1 = TestDataHelpers.GenerateImei(),
            IMEI2 = TestDataHelpers.GenerateImei(),
            OwnershipTransferred = false
        };
        Context.Phones.Add(phone);
        Context.SaveChanges();
        Context.Transactions.Add(new Transaction
        {
            ProductId = product.Id,
            ProductNavigation = product,
            SellerId = 1,
            CustomerId = 1,
            FinishedPrice = 100m,
            Date = DateTime.UtcNow,
            Direction = TransactionDirection.Sell
        });
        Context.SaveChanges();

        var stock = await _service.GetStockAsync();

        Assert.Equal(1, stock.PhonesInStock);
        Assert.Equal(1, stock.PhonesSecondHand);
        Assert.Equal(0, stock.PhonesSecondHandAvailable);
    }

    [Fact]
    public async Task GetRecentTransactionsAsync_orders_limits_and_projects_cards_with_shop_fallback()
    {
        TestDataHelpers.SeedShopSentinels(Context);
        var olderProduct = TestDataHelpers.CreateProduct(Context);
        var newerProduct = TestDataHelpers.CreateProduct(Context);
        var seller = Context.Sellers.First();
        var customer = Context.Customers.First();

        Context.Transactions.Add(new Transaction
        {
            ProductId = olderProduct.Id,
            ProductNavigation = olderProduct,
            SellerId = seller.Id,
            CustomerId = customer.Id,
            FinishedPrice = 100m,
            Date = new DateTime(2025, 1, 1),
            Direction = TransactionDirection.Buy
        });
        Context.Transactions.Add(new Transaction
        {
            ProductId = newerProduct.Id,
            ProductNavigation = newerProduct,
            SellerId = seller.Id,
            CustomerId = customer.Id,
            FinishedPrice = 250m,
            Date = new DateTime(2025, 1, 2),
            Direction = TransactionDirection.Sell
        });
        Context.SaveChanges();

        var cards = await _service.GetRecentTransactionsAsync(1);

        var card = Assert.Single(cards);
        Assert.Equal(new DateTime(2025, 1, 2), card.Date);
        Assert.Equal(TransactionDirection.Sell, card.Direction);
        Assert.Equal(250m, card.FinishedPrice);
        Assert.StartsWith("To: ", card.PartyLabel);
        Assert.DoesNotContain("From: ", card.PartyLabel);
        Assert.DoesNotContain(cards, card => card.Date == new DateTime(2025, 1, 1));
    }

    [Fact]
    public async Task GetRecentTransactionsAsync_returns_empty_when_no_transactions_exist()
        => Assert.Empty(await _service.GetRecentTransactionsAsync());
}
