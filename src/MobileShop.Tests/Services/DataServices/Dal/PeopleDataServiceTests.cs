
namespace MobileShop.Tests.Services.DataServices.Dal;

public class PeopleDataServiceTests : RepoTestBase
{
    private readonly PeopleDataService _service;

    public PeopleDataServiceTests()
    {
        _service = new PeopleDataService(
            new BaseRepo<Customer>(Context),
            new BaseRepo<Seller>(Context),
            new BaseRepo<Product>(Context),
            NullLogger<PeopleDataService>.Instance);
    }

    private Customer AddCustomer(string firstName, string lastName, string phone, string nationalId)
    {
        var person = new Person { FirstName = firstName, LastName = lastName, PhoneNumber = phone };
        Context.People.Add(person);
        Context.SaveChanges();

        var customer = new Customer { PersonId = person.Id, NationalId = nationalId };
        Context.Customers.Add(customer);
        Context.SaveChanges();
        return customer;
    }

    private Seller AddSeller(string firstName, string lastName, string phone, SellerEntityType entityType = SellerEntityType.Real)
    {
        var person = new Person { FirstName = firstName, LastName = lastName, PhoneNumber = phone };
        Context.People.Add(person);
        Context.SaveChanges();

        var seller = new Seller { PersonId = person.Id, EntityType = entityType };
        Context.Sellers.Add(seller);
        Context.SaveChanges();
        return seller;
    }

    private void SeedTransaction(int productId, int sellerId, int customerId, TransactionDirection direction)
    {
        Context.Transactions.Add(new Transaction
        {
            ProductId = productId,
            SellerId = sellerId,
            CustomerId = customerId,
            FinishedPrice = 100,
            Date = DateTime.UtcNow,
            Direction = direction,
        });
        Context.SaveChanges();
    }

    /// <summary>Seeds a transaction and marks it soft-deleted.</summary>
    private Transaction SeedSoftDeletedTransaction(
        int productId,
        int sellerId,
        int customerId,
        TransactionDirection direction)
    {
        var transaction = new Transaction
        {
            ProductId = productId,
            SellerId = sellerId,
            CustomerId = customerId,
            FinishedPrice = 100,
            Date = DateTime.UtcNow,
            Direction = direction,
        };
        Context.Transactions.Add(transaction);
        Context.SaveChanges();

        transaction.IsDeleted = true;
        Context.SaveChanges();
        return transaction;
    }

    [Fact]
    public async Task GetCustomerRowsAsync_sorts_by_name_phone_and_count_with_invalid_sort_fallback()
    {
        AddCustomer("Ali", "Zed", "09120000003", "1000000001");
        var sara = AddCustomer("Sara", "Ahmadi", "09120000001", "1000000002");
        var aliId = Context.Customers.Single(c => c.NationalId == "1000000001").Id;

        var sellerPerson = new Person { FirstName = "Shop", LastName = "Seller", PhoneNumber = "09120000009" };
        Context.People.Add(sellerPerson);
        Context.SaveChanges();
        var seller = new Seller { PersonId = sellerPerson.Id, EntityType = SellerEntityType.Real };
        Context.Sellers.Add(seller);
        Context.SaveChanges();

        var product = TestDataHelpers.CreateProduct(Context);
        SeedTransaction(product.Id, seller.Id, sara.Id, TransactionDirection.Sell);

        var byName = await _service.GetCustomerRowsAsync("Name", true);
        Assert.Equal("Ali Zed", byName[0].Name);
        Assert.Equal("Sara Ahmadi", byName[1].Name);

        var byNameDesc = await _service.GetCustomerRowsAsync("Name", false);
        Assert.Equal("Sara Ahmadi", byNameDesc[0].Name);

        var byPhone = await _service.GetCustomerRowsAsync("Phone", true);
        Assert.Equal("09120000001", byPhone[0].PhoneNumber);

        var byCountDesc = await _service.GetCustomerRowsAsync("Count", false);
        Assert.Equal(sara.Id, byCountDesc[0].Id);
        Assert.Equal(1, byCountDesc[0].PurchasedCount);
        Assert.Equal(0, byCountDesc.Single(r => r.Id == aliId).PurchasedCount);

        var invalid = await _service.GetCustomerRowsAsync("Bogus", true);
        Assert.Equal("Ali Zed", invalid[0].Name);
    }

    [Fact]
    public async Task GetSellerRowsAsync_sorts_by_name_phone_and_count_with_invalid_sort_fallback()
    {
        AddSeller("Ali", "Zed", "09120000003");
        var sara = AddSeller("Sara", "Ahmadi", "09120000001");
        var aliId = Context.Sellers.Single(s => s.PersonNavigation.FirstName == "Ali").Id;

        var customerPerson = new Person { FirstName = "Shop", LastName = "Customer", PhoneNumber = "09120000010" };
        Context.People.Add(customerPerson);
        Context.SaveChanges();
        var customer = new Customer { PersonId = customerPerson.Id, NationalId = "1000000003" };
        Context.Customers.Add(customer);
        Context.SaveChanges();

        var product = TestDataHelpers.CreateProduct(Context);
        SeedTransaction(product.Id, sara.Id, customer.Id, TransactionDirection.Buy);

        var byName = await _service.GetSellerRowsAsync("Name", true);
        Assert.Equal("Ali Zed", byName[0].Name);

        var byPhone = await _service.GetSellerRowsAsync("Phone", true);
        Assert.Equal("09120000001", byPhone[0].PhoneNumber);

        var byCountDesc = await _service.GetSellerRowsAsync("Count", false);
        Assert.Equal(sara.Id, byCountDesc[0].Id);
        Assert.Equal(1, byCountDesc[0].SoldCount);
        Assert.Equal(0, byCountDesc.Single(r => r.Id == aliId).SoldCount);

        var invalid = await _service.GetSellerRowsAsync("Bogus", true);
        Assert.Equal("Ali Zed", invalid[0].Name);
    }

    [Fact]
    public async Task GetCustomerDetailsAsync_returns_header_with_empty_products_for_known_id()
    {
        var customer = AddCustomer("Ali", "Rezaei", "09120000001", "1000000001");
        Assert.True(customer.Id > 0);

        var details = await _service.GetCustomerDetailsAsync(customer.Id);

        Assert.NotNull(details);
        Assert.Equal("Ali Rezaei", details!.Name);
        Assert.Equal("09120000001", details.PhoneNumber);
        Assert.Equal("1000000001", details.NationalId);
        Assert.NotNull(details.Products);
        Assert.Empty(details.Products);
    }

    [Fact]
    public async Task GetCustomerDetailsAsync_returns_null_for_unknown_id()
    {
        var details = await _service.GetCustomerDetailsAsync(999);

        Assert.Null(details);
    }

    [Fact]
    public async Task GetCustomerDetailsAsync_returns_purchased_sell_direction_products_only()
    {
        var customer = AddCustomer("Ali", "Rezaei", "09120000001", "1000000001");
        var other = AddCustomer("Sara", "Ahmadi", "09120000002", "1000000002");
        var sellerPerson = new Person { FirstName = "Shop", LastName = "Seller", PhoneNumber = "09120000009" };
        Context.People.Add(sellerPerson);
        Context.SaveChanges();
        var seller = new Seller { PersonId = sellerPerson.Id, EntityType = SellerEntityType.Real };
        Context.Sellers.Add(seller);
        Context.SaveChanges();

        var purchased = TestDataHelpers.CreateProduct(Context);
        var notPurchased = TestDataHelpers.CreateProduct(Context);

        // purchased: Sell transaction for this customer
        SeedTransaction(purchased.Id, seller.Id, customer.Id, TransactionDirection.Sell);
        // same product, but a Buy transaction (shop bought from seller) — must not count for the customer
        SeedTransaction(purchased.Id, seller.Id, 1, TransactionDirection.Buy);
        // another customer's purchase — must not leak into this customer's rows
        SeedTransaction(notPurchased.Id, seller.Id, other.Id, TransactionDirection.Sell);

        var details = await _service.GetCustomerDetailsAsync(customer.Id);

        Assert.NotNull(details);
        Assert.Single(details!.Products);
        var row = details.Products[0];
        Assert.Equal(purchased.Id, row.ProductId);
        Assert.Equal(purchased.Id, row.EntityId);
        Assert.Equal(purchased.Barcode, row.Identifier);
        Assert.True(row.IsSold);
        Assert.False(row.IsSecondHand);
    }

    [Fact]
    public async Task GetCustomerDetailsAsync_returns_empty_products_when_no_sell_transactions()
    {
        var customer = AddCustomer("Ali", "Rezaei", "09120000001", "1000000001");

        var details = await _service.GetCustomerDetailsAsync(customer.Id);

        Assert.NotNull(details);
        Assert.Empty(details!.Products);
    }

    [Fact]
    public async Task GetSellerDetailsAsync_returns_header_with_empty_products_for_known_id()
    {
        var seller = AddSeller("Sara", "Karimi", "09120000002", SellerEntityType.Legal);

        var details = await _service.GetSellerDetailsAsync(seller.Id);

        Assert.NotNull(details);
        Assert.Equal("Sara Karimi", details!.Name);
        Assert.Equal("09120000002", details.PhoneNumber);
        Assert.Equal("Legal", details.EntityType);
        Assert.NotNull(details.Products);
        Assert.Empty(details.Products);
    }

    [Fact]
    public async Task GetSellerDetailsAsync_returns_null_for_unknown_id()
    {
        var details = await _service.GetSellerDetailsAsync(999);

        Assert.Null(details);
    }

    [Fact]
    public async Task GetSellerDetailsAsync_returns_supplied_buy_direction_products_only()
    {
        var seller = AddSeller("Sara", "Karimi", "09120000002");
        var otherSeller = AddSeller("Ali", "Zed", "09120000003");
        var customerPerson = new Person { FirstName = "Shop", LastName = "Customer", PhoneNumber = "09120000010" };
        Context.People.Add(customerPerson);
        Context.SaveChanges();
        var customer = new Customer { PersonId = customerPerson.Id, NationalId = "1000000003" };
        Context.Customers.Add(customer);
        Context.SaveChanges();

        var supplied = TestDataHelpers.CreateProduct(Context);
        var soldAway = TestDataHelpers.CreateProduct(Context);

        // supplied: Buy transaction (shop bought from this seller)
        SeedTransaction(supplied.Id, seller.Id, customer.Id, TransactionDirection.Buy);
        // same product sold by another seller — must not leak in
        SeedTransaction(supplied.Id, otherSeller.Id, customer.Id, TransactionDirection.Buy);
        // this seller's product only ever sold away (Sell direction) — must not count as supplied
        SeedTransaction(soldAway.Id, seller.Id, customer.Id, TransactionDirection.Sell);

        var details = await _service.GetSellerDetailsAsync(seller.Id);

        Assert.NotNull(details);
        Assert.Single(details!.Products);
        var row = details.Products[0];
        Assert.Equal(supplied.Id, row.ProductId);
        Assert.Equal(supplied.Barcode, row.Identifier);
    }

    [Fact]
    public async Task GetSellerDetailsAsync_returns_empty_products_when_no_buy_transactions()
    {
        var seller = AddSeller("Sara", "Karimi", "09120000002");

        var details = await _service.GetSellerDetailsAsync(seller.Id);

        Assert.NotNull(details);
        Assert.Empty(details!.Products);
    }

    [Fact]
    public async Task CreateCustomerAsync_persists_customer_with_person()
    {
        var input = new CreateCustomerInputModel
        {
            FirstName = "Ali",
            LastName = "Rezaei",
            PhoneNumber = "09120000001",
            NationalId = "1000000001",
        };

        var result = await _service.CreateCustomerAsync(input);

        Assert.True(result.Succeeded);
        Assert.NotNull(result.EntityId);
        var customer = Context.Customers.Single(c => c.Id == result.EntityId);
        Assert.Equal("1000000001", customer.NationalId);
        Assert.Equal("Ali", Context.People.Single(p => p.Id == customer.PersonId).FirstName);
    }

    [Fact]
    public async Task CreateSellerAsync_persists_seller_with_person()
    {
        var input = new CreateSellerInputModel
        {
            FirstName = "Sara",
            LastName = "Karimi",
            PhoneNumber = "09120000002",
            EntityType = SellerEntityType.Real,
        };

        var result = await _service.CreateSellerAsync(input);

        Assert.True(result.Succeeded);
        Assert.NotNull(result.EntityId);
        var seller = Context.Sellers.Single(s => s.Id == result.EntityId);
        Assert.Equal(SellerEntityType.Real, seller.EntityType);
        Assert.Equal("Sara", Context.People.Single(p => p.Id == seller.PersonId).FirstName);
    }

    [Fact]
    public async Task GetCustomerRowsAsync_purchased_count_ignores_soft_deleted_transactions()
    {
        var customer = AddCustomer("Sara", "Ahmadi", "09120000001", "1000000001");

        var shopSeller = AddSeller("Shop", "Owner", "09120000009");
        var live = TestDataHelpers.CreateProduct(Context);
        var removed = TestDataHelpers.CreateProduct(Context);

        SeedTransaction(live.Id, shopSeller.Id, customer.Id, TransactionDirection.Sell);
        SeedSoftDeletedTransaction(removed.Id, shopSeller.Id, customer.Id, TransactionDirection.Sell);

        var rows = await _service.GetCustomerRowsAsync("Name", true);
        var row = rows.Single(r => r.Id == customer.Id);

        Assert.Equal(1, row.PurchasedCount);
    }

    [Fact]
    public async Task GetSellerRowsAsync_sold_count_ignores_soft_deleted_transactions()
    {
        var seller = AddSeller("Sara", "Karimi", "09120000002");
        var customer = AddCustomer("Shop", "Buyer", "09120000010", "1000000003");
        var live = TestDataHelpers.CreateProduct(Context);
        var removed = TestDataHelpers.CreateProduct(Context);

        // Both buys belong to the seller under test, so the count is 1 only if the
        // soft-deleted transaction is excluded.
        SeedTransaction(live.Id, seller.Id, customer.Id, TransactionDirection.Buy);
        SeedSoftDeletedTransaction(removed.Id, seller.Id, customer.Id, TransactionDirection.Buy);

        var rows = await _service.GetSellerRowsAsync("Name", true);
        var row = rows.Single(r => r.Id == seller.Id);

        Assert.Equal(1, row.SoldCount);
    }
}