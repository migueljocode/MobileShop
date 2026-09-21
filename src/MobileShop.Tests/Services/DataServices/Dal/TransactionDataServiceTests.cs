namespace MobileShop.Tests.Services.DataServices.Dal;

public class TransactionDataServiceTests : RepoTestBase
{
    private readonly TransactionDataService _service;

    public TransactionDataServiceTests()
    {
        _service = new TransactionDataService(new TransactionRepo(Context), NullLogger<TransactionDataService>.Instance);
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
}
