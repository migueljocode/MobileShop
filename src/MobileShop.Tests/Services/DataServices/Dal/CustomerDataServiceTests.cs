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
}
