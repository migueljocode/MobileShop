namespace MobileShop.Tests.Dal.Repos;

public class TransactionRepoTests : BaseRepoTests<Transaction, ITransactionRepo>
{
    protected override ITransactionRepo CreateRepo() => new TransactionRepo(Context);

    protected override Transaction CreateValidEntity()
    {
        var sellerPerson = new Person { FirstName = "S", LastName = "P", PhoneNumber = "09120000003" };
        var customerPerson = new Person { FirstName = "C", LastName = "P", PhoneNumber = "09120000004" };
        Context.People.AddRange(sellerPerson, customerPerson);
        Context.SaveChanges();

        var seller = new Seller { PersonId = sellerPerson.Id, EntityType = SellerEntityType.Real };
        var customer = new Customer { PersonId = customerPerson.Id, NationalId = "2222222222" };
        var product = new Product { Price = 500, Manufacturer = "TestCo", Model = "X" };
        Context.Sellers.Add(seller);
        Context.Customers.Add(customer);
        Context.Products.Add(product);
        Context.SaveChanges();

        return new Transaction
        {
            Date = DateTime.Today,
            SellerId = seller.Id,
            CustomerId = customer.Id,
            ProductId = product.Id,
            FinishedPrice = 500,
            Direction = TransactionDirection.Buy
        };
    }

    [Fact]
    public void GetByProduct_ReturnsOnlyTransactionsForThatProduct()
    {
        var repo = CreateRepo();
        var transaction = CreateValidEntity();
        repo.Add(transaction);
        repo.Add(CreateValidEntity()); // a second, unrelated product + transaction

        var results = repo.GetByProduct(transaction.ProductId).ToList();

        Assert.Single(results);
        Assert.Equal(transaction.Id, results[0].Id);
    }

    [Fact]
    public async Task GetByProductAsync_ReturnsOnlyTransactionsForThatProduct()
    {
        var repo = CreateRepo();
        var transaction = CreateValidEntity();
        await repo.AddAsync(transaction);

        var results = (await repo.GetByProductAsync(transaction.ProductId)).ToList();

        Assert.Single(results);
        Assert.Equal(transaction.Id, results[0].Id);
    }
}
