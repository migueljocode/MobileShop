namespace MobileShop.Tests.Services.DataServices.Dal;

public class SellerDataServiceTests : RepoTestBase
{
    private readonly SellerDataService _service;

    public SellerDataServiceTests()
    {
        _service = new SellerDataService(new SellerRepo(Context), NullLogger<SellerDataService>.Instance);
    }

    [Fact]
    public async Task SoldProducts_and_SoldToShop_are_exposed_from_repo_and_filtered()
    {
        var sellerPerson = new Person { FirstName = "Seller", LastName = "One", PhoneNumber = "09120000001" };
        Context.People.Add(sellerPerson);
        Context.SaveChanges();

        var seller = new Seller { PersonId = sellerPerson.Id, EntityType = SellerEntityType.Real };
        Context.Sellers.Add(seller);
        Context.SaveChanges();

        var customerPerson = new Person { FirstName = "Customer", LastName = "One", PhoneNumber = "09120000002" };
        Context.People.Add(customerPerson);
        Context.SaveChanges();

        var customer = new Customer { PersonId = customerPerson.Id, NationalId = "1000000001" };
        Context.Customers.Add(customer);
        Context.SaveChanges();

        var product = TestDataHelpers.CreateProduct(Context, 500m);

        Context.Transactions.Add(new Transaction { ProductId = product.Id, ProductNavigation = product, SellerId = seller.Id, SellerNavigation = seller, CustomerId = customer.Id, CustomerNavigation = customer, FinishedPrice = 500m, Date = DateTime.UtcNow, Direction = TransactionDirection.Buy });
        Context.Transactions.Add(new Transaction { ProductId = product.Id, ProductNavigation = product, SellerId = seller.Id, SellerNavigation = seller, CustomerId = customer.Id, CustomerNavigation = customer, FinishedPrice = 600m, Date = DateTime.UtcNow.AddMinutes(1), Direction = TransactionDirection.Sell });
        Context.SaveChanges();

        var soldProducts = _service.SoldProducts(seller.Id).ToList();
        Assert.Contains(soldProducts, p => p.Id == product.Id);

        var soldToShop = _service.SoldToShop(seller.Id).ToList();
        Assert.Contains(soldToShop, p => p.Id == product.Id);

        var asyncSoldProducts = (await _service.SoldProductsAsync(seller.Id)).ToList();
        Assert.Contains(asyncSoldProducts, p => p.Id == product.Id);

        var asyncSoldToShop = (await _service.SoldToShopAsync(seller.Id)).ToList();
        Assert.Contains(asyncSoldToShop, p => p.Id == product.Id);
    }
}
