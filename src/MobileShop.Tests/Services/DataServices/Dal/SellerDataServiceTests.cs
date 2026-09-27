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

    [Fact]
    public async Task GetListRows_counts_sold_transactions_and_excludes_soft_deleted_async()
    {
        var sellerPerson = new Person { FirstName = "Count", LastName = "Seller", PhoneNumber = "09120000012" };
        Context.People.Add(sellerPerson);
        Context.SaveChanges();

        var seller = new Seller { PersonId = sellerPerson.Id, EntityType = SellerEntityType.Real };
        Context.Sellers.Add(seller);
        Context.SaveChanges();

        var customerPerson = new Person { FirstName = "Buyer", LastName = "ForCount", PhoneNumber = "09120000013" };
        Context.People.Add(customerPerson);
        Context.SaveChanges();

        var customer = new Customer { PersonId = customerPerson.Id, NationalId = "1000000007" };
        Context.Customers.Add(customer);
        Context.SaveChanges();

        var product = TestDataHelpers.CreateProduct(Context, 100m);

        // two live Buy transactions + one soft-deleted Buy transaction + one Sell transaction (not counted for sellers)
        Context.Transactions.Add(new Transaction { ProductId = product.Id, ProductNavigation = product, SellerId = seller.Id, SellerNavigation = seller, CustomerId = customer.Id, CustomerNavigation = customer, FinishedPrice = 100m, Date = DateTime.UtcNow, Direction = TransactionDirection.Buy });
        Context.Transactions.Add(new Transaction { ProductId = product.Id, ProductNavigation = product, SellerId = seller.Id, SellerNavigation = seller, CustomerId = customer.Id, CustomerNavigation = customer, FinishedPrice = 100m, Date = DateTime.UtcNow.AddMinutes(1), Direction = TransactionDirection.Buy });
        Context.Transactions.Add(new Transaction { ProductId = product.Id, ProductNavigation = product, SellerId = seller.Id, SellerNavigation = seller, CustomerId = customer.Id, CustomerNavigation = customer, FinishedPrice = 100m, Date = DateTime.UtcNow.AddMinutes(2), Direction = TransactionDirection.Buy, IsDeleted = true });
        Context.Transactions.Add(new Transaction { ProductId = product.Id, ProductNavigation = product, SellerId = seller.Id, SellerNavigation = seller, CustomerId = customer.Id, CustomerNavigation = customer, FinishedPrice = 100m, Date = DateTime.UtcNow.AddMinutes(3), Direction = TransactionDirection.Sell });
        Context.SaveChanges();

        var rows = _service.GetListRows();
        Assert.Equal(2, rows.Single(r => r.Id == seller.Id).SoldCount);

        var asyncRows = await _service.GetListRowsAsync();
        Assert.Equal(2, asyncRows.Single(r => r.Id == seller.Id).SoldCount);
    }

    [Fact]
    public async Task GetListRows_returns_zero_for_seller_without_transactions_async()
    {
        var sellerPerson = new Person { FirstName = "No", LastName = "Sales", PhoneNumber = "09120000014" };
        Context.People.Add(sellerPerson);
        Context.SaveChanges();

        var seller = new Seller { PersonId = sellerPerson.Id, EntityType = SellerEntityType.Real };
        Context.Sellers.Add(seller);
        Context.SaveChanges();

        var rows = _service.GetListRows();
        Assert.Equal(0, rows.Single(r => r.Id == seller.Id).SoldCount);

        var asyncRows = await _service.GetListRowsAsync();
        Assert.Equal(0, asyncRows.Single(r => r.Id == seller.Id).SoldCount);
    }
}
