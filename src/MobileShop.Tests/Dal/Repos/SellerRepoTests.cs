namespace MobileShop.Tests.Dal.Repos;

public class SellerRepoTests : BaseRepoTests<Seller, ISellerRepo>
{
    protected override ISellerRepo CreateRepo() => new SellerRepo(Context);

    protected override Seller CreateValidEntity()
    {
        var person = new Person { FirstName = "Test", LastName = "Seller", PhoneNumber = "09120000000" };
        Context.People.Add(person);
        Context.SaveChanges();

        return new Seller { PersonId = person.Id, EntityType = SellerEntityType.Real };
    }

    [Fact]
    public void SoldProducts_IncludesBothBuyAndSellDirections()
    {
        var repo = CreateRepo();
        var seller = CreateValidEntity();
        repo.Add(seller);
        var supplied = AddTransaction(seller.Id, TransactionDirection.Buy);
        var soldOn = AddTransaction(seller.Id, TransactionDirection.Sell);

        var results = repo.SoldProducts(seller.Id)!.Select(p => p.Id).ToList();

        Assert.Contains(supplied.Id, results);
        Assert.Contains(soldOn.Id, results);
    }

    [Fact]
    public async Task SoldProductsAsync_IncludesBothBuyAndSellDirections()
    {
        var repo = CreateRepo();
        var seller = CreateValidEntity();
        await repo.AddAsync(seller);
        var supplied = AddTransaction(seller.Id, TransactionDirection.Buy);
        var soldOn = AddTransaction(seller.Id, TransactionDirection.Sell);

        var results = (await repo.SoldProductsAsync(seller.Id))!.Select(p => p.Id).ToList();

        Assert.Contains(supplied.Id, results);
        Assert.Contains(soldOn.Id, results);
    }

    [Fact]
    public void SoldToShop_ExcludesSellDirection()
    {
        var repo = CreateRepo();
        var seller = CreateValidEntity();
        repo.Add(seller);
        var supplied = AddTransaction(seller.Id, TransactionDirection.Buy);
        AddTransaction(seller.Id, TransactionDirection.Sell);

        var results = repo.SoldToShop(seller.Id)!.ToList();

        Assert.Single(results);
        Assert.Equal(supplied.Id, results[0].Id);
    }

    [Fact]
    public async Task SoldToShopAsync_ExcludesSellDirection()
    {
        var repo = CreateRepo();
        var seller = CreateValidEntity();
        await repo.AddAsync(seller);
        var supplied = AddTransaction(seller.Id, TransactionDirection.Buy);
        AddTransaction(seller.Id, TransactionDirection.Sell);

        var results = (await repo.SoldToShopAsync(seller.Id))!.ToList();

        Assert.Single(results);
        Assert.Equal(supplied.Id, results[0].Id);
    }

    private Product AddTransaction(int sellerId, TransactionDirection direction)
    {
        var customerPerson = new Person { FirstName = "C", LastName = "P", PhoneNumber = "09122222222" };
        Context.People.Add(customerPerson);
        Context.SaveChanges();

        var customer = new Customer { PersonId = customerPerson.Id, NationalId = "5555555555" };
        var product = new Product { Price = 100, Manufacturer = "TestCo", Model = "X" };
        Context.Customers.Add(customer);
        Context.Products.Add(product);
        Context.SaveChanges();

        Context.Transactions.Add(new Transaction
        {
            Date = DateTime.Today,
            SellerId = sellerId,
            CustomerId = customer.Id,
            ProductId = product.Id,
            FinishedPrice = 100,
            Direction = direction
        });
        Context.SaveChanges();

        return product;
    }
}