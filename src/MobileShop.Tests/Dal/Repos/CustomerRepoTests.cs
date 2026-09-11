namespace MobileShop.Tests.Dal.Repos;

public class CustomerRepoTests : BaseRepoTests<Customer, ICustomerRepo>
{
    protected override ICustomerRepo CreateRepo() => new CustomerRepo(Context);

    protected override Customer CreateValidEntity()
    {
        var person = new Person { FirstName = "Test", LastName = "Customer", PhoneNumber = "09120000000" };
        Context.People.Add(person);
        Context.SaveChanges();

        return new Customer { PersonId = person.Id, NationalId = "1234567890" };
    }

    [Fact]
    public void Find_EagerlyLoadsPersonNavigation()
    {
        var repo = CreateRepo();
        var customer = CreateValidEntity();
        repo.Add(customer);

        var found = repo.Find(customer.Id);

        Assert.NotNull(found);
        Assert.NotNull(found!.PersonNavigation);
        Assert.Equal("Customer", found.PersonNavigation.LastName);
    }

    [Fact]
    public async Task FindAsync_EagerlyLoadsPersonNavigation()
    {
        var repo = CreateRepo();
        var customer = CreateValidEntity();
        await repo.AddAsync(customer);

        var found = await repo.FindAsync(customer.Id);

        Assert.NotNull(found);
        Assert.NotNull(found!.PersonNavigation);
        Assert.Equal("Customer", found.PersonNavigation.LastName);
    }

    [Fact]
    public void PurchasedProducts_ReturnsOnlyProductsBoughtByThatCustomer()
    {
        var repo = CreateRepo();
        var customer = CreateValidEntity();
        repo.Add(customer);
        var product = BuyProductFor(customer.Id);

        var results = repo.PurchasedProducts(customer.Id)!.ToList();

        Assert.Single(results);
        Assert.Equal(product.Id, results[0].Id);
    }

    [Fact]
    public async Task PurchasedProductsAsync_ReturnsOnlyProductsBoughtByThatCustomer()
    {
        var repo = CreateRepo();
        var customer = CreateValidEntity();
        await repo.AddAsync(customer);
        var product = BuyProductFor(customer.Id);

        var results = (await repo.PurchasedProductsAsync(customer.Id))!.ToList();

        Assert.Single(results);
        Assert.Equal(product.Id, results[0].Id);
    }

    private Product BuyProductFor(int customerId)
    {
        var sellerPerson = new Person { FirstName = "S", LastName = "P", PhoneNumber = "09121111111" };
        Context.People.Add(sellerPerson);
        Context.SaveChanges();

        var seller = new Seller { PersonId = sellerPerson.Id, EntityType = SellerEntityType.Real };
        var product = new Product { Price = 100, Manufacturer = "TestCo", Model = "X" };
        Context.Sellers.Add(seller);
        Context.Products.Add(product);
        Context.SaveChanges();

        Context.Transactions.Add(new Transaction
        {
            Date = DateTime.Today,
            SellerId = seller.Id,
            CustomerId = customerId,
            ProductId = product.Id,
            FinishedPrice = 100,
            Direction = TransactionDirection.Sell
        });
        Context.SaveChanges();

        return product;
    }
}