namespace MobileShop.Tests.Services.DataServices.Dal;

public class CustomerDataServiceTests : RepoTestBase
{
    private readonly CustomerDataService _service;

    public CustomerDataServiceTests()
    {
        _service = new CustomerDataService(new CustomerRepo(Context), NullLogger<CustomerDataService>.Instance);
    }

    [Fact]
    public async Task PurchasedProducts_work_for_customer_and_are_filterable_async()
    {
        var sellerPerson = new Person { FirstName = "Seller", LastName = "Cust", PhoneNumber = "09120000007" };
        Context.People.Add(sellerPerson);
        Context.SaveChanges();

        var seller = new Seller { PersonId = sellerPerson.Id, EntityType = SellerEntityType.Real };
        Context.Sellers.Add(seller);
        Context.SaveChanges();

        var customerPerson = new Person { FirstName = "Customer", LastName = "Purchases", PhoneNumber = "09120000008" };
        Context.People.Add(customerPerson);
        Context.SaveChanges();

        var customer = new Customer { PersonId = customerPerson.Id, NationalId = "1000000004" };
        Context.Customers.Add(customer);
        Context.SaveChanges();

        var product1 = TestDataHelpers.CreateProduct(Context, 100m);
        var product2 = TestDataHelpers.CreateProduct(Context, 200m);

        Context.Transactions.Add(new Transaction { ProductId = product1.Id, ProductNavigation = product1, SellerId = seller.Id, SellerNavigation = seller, CustomerId = customer.Id, CustomerNavigation = customer, FinishedPrice = 100m, Date = DateTime.UtcNow, Direction = TransactionDirection.Sell });
        Context.Transactions.Add(new Transaction { ProductId = product2.Id, ProductNavigation = product2, SellerId = seller.Id, SellerNavigation = seller, CustomerId = customer.Id, CustomerNavigation = customer, FinishedPrice = 200m, Date = DateTime.UtcNow.AddMinutes(1), Direction = TransactionDirection.Sell });
        Context.SaveChanges();

        var purchased = _service.PurchasedProducts(customer.Id).ToList();
        Assert.Equal(2, purchased.Count);

        var asyncPurchased = (await _service.PurchasedProductsAsync(customer.Id)).ToList();
        Assert.Equal(2, asyncPurchased.Count);
    }

    [Fact]
    public async Task GetListRows_counts_purchased_transactions_and_excludes_soft_deleted_async()
    {
        var customerPerson = new Person { FirstName = "Count", LastName = "Buyer", PhoneNumber = "09120000009" };
        Context.People.Add(customerPerson);
        Context.SaveChanges();

        var customer = new Customer { PersonId = customerPerson.Id, NationalId = "1000000005" };
        Context.Customers.Add(customer);
        Context.SaveChanges();

        var sellerPerson = new Person { FirstName = "Seller", LastName = "ForCount", PhoneNumber = "09120000010" };
        Context.People.Add(sellerPerson);
        Context.SaveChanges();

        var seller = new Seller { PersonId = sellerPerson.Id, EntityType = SellerEntityType.Real };
        Context.Sellers.Add(seller);
        Context.SaveChanges();

        var product = TestDataHelpers.CreateProduct(Context, 100m);

        // two live Sell transactions + one soft-deleted Sell transaction + one Buy transaction (not counted for customers)
        Context.Transactions.Add(new Transaction { ProductId = product.Id, ProductNavigation = product, SellerId = seller.Id, SellerNavigation = seller, CustomerId = customer.Id, CustomerNavigation = customer, FinishedPrice = 100m, Date = DateTime.UtcNow, Direction = TransactionDirection.Sell });
        Context.Transactions.Add(new Transaction { ProductId = product.Id, ProductNavigation = product, SellerId = seller.Id, SellerNavigation = seller, CustomerId = customer.Id, CustomerNavigation = customer, FinishedPrice = 100m, Date = DateTime.UtcNow.AddMinutes(1), Direction = TransactionDirection.Sell });
        Context.Transactions.Add(new Transaction { ProductId = product.Id, ProductNavigation = product, SellerId = seller.Id, SellerNavigation = seller, CustomerId = customer.Id, CustomerNavigation = customer, FinishedPrice = 100m, Date = DateTime.UtcNow.AddMinutes(2), Direction = TransactionDirection.Sell, IsDeleted = true });
        Context.Transactions.Add(new Transaction { ProductId = product.Id, ProductNavigation = product, SellerId = seller.Id, SellerNavigation = seller, CustomerId = customer.Id, CustomerNavigation = customer, FinishedPrice = 100m, Date = DateTime.UtcNow.AddMinutes(3), Direction = TransactionDirection.Buy });
        Context.SaveChanges();

        var rows = _service.GetListRows();
        Assert.Equal(2, rows.Single(r => r.Id == customer.Id).PurchasedCount);

        var asyncRows = await _service.GetListRowsAsync();
        Assert.Equal(2, asyncRows.Single(r => r.Id == customer.Id).PurchasedCount);
    }

    [Fact]
    public async Task GetListRows_returns_zero_for_customer_without_transactions_async()
    {
        var customerPerson = new Person { FirstName = "No", LastName = "Buys", PhoneNumber = "09120000011" };
        Context.People.Add(customerPerson);
        Context.SaveChanges();

        var customer = new Customer { PersonId = customerPerson.Id, NationalId = "1000000006" };
        Context.Customers.Add(customer);
        Context.SaveChanges();

        var rows = _service.GetListRows();
        Assert.Equal(0, rows.Single(r => r.Id == customer.Id).PurchasedCount);

        var asyncRows = await _service.GetListRowsAsync();
        Assert.Equal(0, asyncRows.Single(r => r.Id == customer.Id).PurchasedCount);
    }

    [Fact]
    public async Task GetListRows_sorts_by_name_phone_and_count_in_both_directions()
    {
        // Alice 0912...11, 0 purchases; Bob 0912...09, 2 purchases; carol 0912...10, 1 purchase
        var alicePerson = new Person { FirstName = "Alice", LastName = "A", PhoneNumber = "09120000021" };
        var bobPerson = new Person { FirstName = "Bob", LastName = "B", PhoneNumber = "09120000019" };
        var carolPerson = new Person { FirstName = "Carol", LastName = "C", PhoneNumber = "09120000020" };
        Context.People.AddRange(alicePerson, bobPerson, carolPerson);
        Context.SaveChanges();

        var alice = new Customer { PersonId = alicePerson.Id, NationalId = "2000000001" };
        var bob = new Customer { PersonId = bobPerson.Id, NationalId = "2000000002" };
        var carol = new Customer { PersonId = carolPerson.Id, NationalId = "2000000003" };
        Context.Customers.AddRange(alice, bob, carol);
        Context.SaveChanges();

        var sellerPerson = new Person { FirstName = "Seller", LastName = "ForSort", PhoneNumber = "09120000022" };
        Context.People.Add(sellerPerson);
        Context.SaveChanges();
        var seller = new Seller { PersonId = sellerPerson.Id, EntityType = SellerEntityType.Real };
        Context.Sellers.Add(seller);
        Context.SaveChanges();

        var product = TestDataHelpers.CreateProduct(Context, 100m);
        Transaction Tx(Customer c, int minutes) => new()
        {
            ProductId = product.Id,
            ProductNavigation = product,
            SellerId = seller.Id,
            SellerNavigation = seller,
            CustomerId = c.Id,
            CustomerNavigation = c,
            FinishedPrice = 100m,
            Date = DateTime.UtcNow.AddMinutes(minutes),
            Direction = TransactionDirection.Sell
        };
        Context.Transactions.Add(Tx(bob, 1));
        Context.Transactions.Add(Tx(bob, 2));
        Context.Transactions.Add(Tx(carol, 3));
        Context.SaveChanges();

        // Name asc: Alice, Bob, Carol
        var byName = _service.GetListRows("Name");
        Assert.Equal(new[] { "Alice A", "Bob B", "Carol C" }, byName.Select(r => r.Name));

        // Name desc: Carol, Bob, Alice
        var byNameDesc = _service.GetListRows("Name", ascending: false);
        Assert.Equal(new[] { "Carol C", "Bob B", "Alice A" }, byNameDesc.Select(r => r.Name));

        // Phone asc: 09120000019 Bob, 09120000020 Carol, 09120000021 Alice
        var byPhone = _service.GetListRows("Phone");
        Assert.Equal(new[] { "Bob B", "Carol C", "Alice A" }, byPhone.Select(r => r.Name));

        // Phone desc
        var byPhoneDesc = _service.GetListRows("Phone", ascending: false);
        Assert.Equal(new[] { "Alice A", "Carol C", "Bob B" }, byPhoneDesc.Select(r => r.Name));

        // Count asc: Alice 0, Carol 1, Bob 2
        var byCount = _service.GetListRows("Count");
        Assert.Equal(new[] { "Alice A", "Carol C", "Bob B" }, byCount.Select(r => r.Name));
        Assert.Equal(new[] { 0, 1, 2 }, byCount.Select(r => r.PurchasedCount));

        // Count desc
        var byCountDesc = _service.GetListRows("Count", ascending: false);
        Assert.Equal(new[] { 2, 1, 0 }, byCountDesc.Select(r => r.PurchasedCount));

        // Invalid sortBy falls back to Name
        var invalid = _service.GetListRows("Bogus");
        Assert.Equal(new[] { "Alice A", "Bob B", "Carol C" }, invalid.Select(r => r.Name));

        // async variants match sync
        var asyncByName = await _service.GetListRowsAsync("Name");
        Assert.Equal(new[] { "Alice A", "Bob B", "Carol C" }, asyncByName.Select(r => r.Name));
        var asyncByCountDesc = await _service.GetListRowsAsync("Count", ascending: false);
        Assert.Equal(new[] { 2, 1, 0 }, asyncByCountDesc.Select(r => r.PurchasedCount));
    }
}
