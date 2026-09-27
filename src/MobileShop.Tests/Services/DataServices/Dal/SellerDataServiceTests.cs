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

    [Fact]
    public async Task GetListRows_sorts_by_name_phone_and_count_in_both_directions()
    {
        // Ava 09120000031, 0 purchases; Ben 09120000029, 2 purchases; zoe 09120000030, 1 purchase
        var avaPerson = new Person { FirstName = "Ava", LastName = "A", PhoneNumber = "09120000031" };
        var benPerson = new Person { FirstName = "Ben", LastName = "B", PhoneNumber = "09120000029" };
        var zoePerson = new Person { FirstName = "Zoe", LastName = "Z", PhoneNumber = "09120000030" };
        Context.People.AddRange(avaPerson, benPerson, zoePerson);
        Context.SaveChanges();

        var ava = new Seller { PersonId = avaPerson.Id, EntityType = SellerEntityType.Real };
        var ben = new Seller { PersonId = benPerson.Id, EntityType = SellerEntityType.Real };
        var zoe = new Seller { PersonId = zoePerson.Id, EntityType = SellerEntityType.Real };
        Context.Sellers.AddRange(ava, ben, zoe);
        Context.SaveChanges();

        var customerPerson = new Person { FirstName = "Customer", LastName = "ForSort", PhoneNumber = "09120000032" };
        Context.People.Add(customerPerson);
        Context.SaveChanges();
        var customer = new Customer { PersonId = customerPerson.Id, NationalId = "3000000001" };
        Context.Customers.Add(customer);
        Context.SaveChanges();

        var product = TestDataHelpers.CreateProduct(Context, 100m);
        Transaction Tx(Seller s, int minutes) => new()
        {
            ProductId = product.Id,
            ProductNavigation = product,
            SellerId = s.Id,
            SellerNavigation = s,
            CustomerId = customer.Id,
            CustomerNavigation = customer,
            FinishedPrice = 100m,
            Date = DateTime.UtcNow.AddMinutes(minutes),
            Direction = TransactionDirection.Buy
        };
        Context.Transactions.Add(Tx(ben, 1));
        Context.Transactions.Add(Tx(ben, 2));
        Context.Transactions.Add(Tx(zoe, 3));
        Context.SaveChanges();

        // Name asc: Ava, Ben, Zoe
        var byName = _service.GetListRows("Name");
        Assert.Equal(new[] { "Ava A", "Ben B", "Zoe Z" }, byName.Select(r => r.Name));

        // Phone asc: 09120000029 Ben, 09120000030 Zoe, 09120000031 Ava
        var byPhone = _service.GetListRows("Phone");
        Assert.Equal(new[] { "Ben B", "Zoe Z", "Ava A" }, byPhone.Select(r => r.Name));

        // Count asc: Ava 0, Zoe 1, Ben 2
        var byCount = _service.GetListRows("Count");
        Assert.Equal(new[] { 0, 1, 2 }, byCount.Select(r => r.SoldCount));

        // Count desc: Ben 2, Zoe 1, Ava 0
        var byCountDesc = _service.GetListRows("Count", ascending: false);
        Assert.Equal(new[] { 2, 1, 0 }, byCountDesc.Select(r => r.SoldCount));

        // Name desc
        var byNameDesc = _service.GetListRows("Name", ascending: false);
        Assert.Equal(new[] { "Zoe Z", "Ben B", "Ava A" }, byNameDesc.Select(r => r.Name));

        // Invalid sortBy falls back to Name
        var invalid = _service.GetListRows("Bogus");
        Assert.Equal(new[] { "Ava A", "Ben B", "Zoe Z" }, invalid.Select(r => r.Name));

        // async variants match sync
        var asyncByPhone = await _service.GetListRowsAsync("Phone");
        Assert.Equal(new[] { "Ben B", "Zoe Z", "Ava A" }, asyncByPhone.Select(r => r.Name));
        var asyncByCountDesc = await _service.GetListRowsAsync("Count", ascending: false);
        Assert.Equal(new[] { 2, 1, 0 }, asyncByCountDesc.Select(r => r.SoldCount));
    }
}
