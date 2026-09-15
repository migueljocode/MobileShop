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

        var product1 = new Product { Manufacturer = "Apple", Model = "Mac", Price = 100m };
        var product2 = new Product { Manufacturer = "Apple", Model = "iPad", Price = 200m };
        Context.Products.AddRange(product1, product2);
        Context.SaveChanges();

        Context.Transactions.Add(new Transaction { ProductId = product1.Id, ProductNavigation = product1, SellerId = seller.Id, SellerNavigation = seller, CustomerId = customer.Id, CustomerNavigation = customer, FinishedPrice = 100m, Date = DateTime.UtcNow, Direction = TransactionDirection.Sell });
        Context.Transactions.Add(new Transaction { ProductId = product2.Id, ProductNavigation = product2, SellerId = seller.Id, SellerNavigation = seller, CustomerId = customer.Id, CustomerNavigation = customer, FinishedPrice = 200m, Date = DateTime.UtcNow.AddMinutes(1), Direction = TransactionDirection.Sell });
        Context.SaveChanges();

        var purchased = _service.PurchasedProducts(customer.Id).ToList();
        Assert.Equal(2, purchased.Count);

        var asyncPurchased = (await _service.PurchasedProductsAsync(customer.Id)).ToList();
        Assert.Equal(2, asyncPurchased.Count);
    }
}
