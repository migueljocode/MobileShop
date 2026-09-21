namespace MobileShop.Tests.Dal.Repos;

public class ProductRepoTests : BaseRepoTests<Product, IProductRepo>
{
    protected override IProductRepo CreateRepo() => new ProductRepo(Context);

    protected override Product CreateValidEntity()
        => TestDataHelpers.CreateProduct(Context, 1_000_000, persist: false);

    [Fact]
    public void IsInStock_ReturnsTrue_WhenNoSellTransactionExists()
    {
        var repo = CreateRepo();
        var product = CreateValidEntity();
        repo.Add(product);

        Assert.True(repo.IsInStock(product.Id));
    }

    [Fact]
    public void IsInStock_ReturnsFalse_AfterSellTransaction()
    {
        var repo = CreateRepo();
        var product = CreateValidEntity();
        repo.Add(product);
        AddSellTransaction(product);

        Assert.False(repo.IsInStock(product.Id));
    }

    [Fact]
    public async Task IsInStockAsync_ReturnsFalse_AfterSellTransaction()
    {
        var repo = CreateRepo();
        var product = CreateValidEntity();
        await repo.AddAsync(product);
        AddSellTransaction(product);

        Assert.False(await repo.IsInStockAsync(product.Id));
    }

    private void AddSellTransaction(Product product)
    {
        var sellerPerson = new Person { FirstName = "S", LastName = "P", PhoneNumber = "09120000001" };
        var customerPerson = new Person { FirstName = "C", LastName = "P", PhoneNumber = "09120000002" };
        Context.People.AddRange(sellerPerson, customerPerson);
        Context.SaveChanges();

        var seller = new Seller { PersonId = sellerPerson.Id, EntityType = SellerEntityType.Real };
        var customer = new Customer { PersonId = customerPerson.Id, NationalId = "1111111111" };
        Context.Sellers.Add(seller);
        Context.Customers.Add(customer);
        Context.SaveChanges();

        Context.Transactions.Add(new Transaction
        {
            Date = DateTime.Today,
            SellerId = seller.Id,
            CustomerId = customer.Id,
            ProductId = product.Id,
            FinishedPrice = product.Price,
            Direction = TransactionDirection.Sell
        });
        Context.SaveChanges();
    }
}
