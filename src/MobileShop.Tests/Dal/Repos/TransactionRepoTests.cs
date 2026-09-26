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
        var product = TestDataHelpers.CreateProduct(Context, 500);
        Context.Sellers.Add(seller);
        Context.Customers.Add(customer);
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

    [Fact]
    public async Task GetEarliestTransactionDateAsync_ReturnsEarliestNonDeletedDate()
    {
        var repo = CreateRepo();

        // Insert a later transaction first, then an earlier one
        var later = CreateValidEntity();
        later.Date = new DateTime(2026, 3, 15);
        await repo.AddAsync(later);

        var earlier = CreateValidEntity();
        earlier.Date = new DateTime(2026, 1, 5);
        await repo.AddAsync(earlier);

        var earliest = await repo.GetEarliestTransactionDateAsync();

        Assert.NotNull(earliest);
        Assert.Equal(new DateTime(2026, 1, 5), earliest!.Value);
    }

    [Fact]
    public async Task GetEarliestTransactionDateAsync_ReturnsNull_WhenTableEmpty()
    {
        var repo = CreateRepo();

        var result = await repo.GetEarliestTransactionDateAsync();

        Assert.Null(result);
    }

    [Fact]
    public async Task GetEarliestTransactionDateAsync_ExcludesSoftDeletedTransactions()
    {
        var repo = CreateRepo();

        var deleted = CreateValidEntity();
        deleted.Date = new DateTime(2025, 1, 1);
        await repo.AddAsync(deleted);
        await repo.DeleteAsync(deleted);

        var active = CreateValidEntity();
        active.Date = new DateTime(2026, 6, 1);
        await repo.AddAsync(active);

        var earliest = await repo.GetEarliestTransactionDateAsync();

        Assert.NotNull(earliest);
        Assert.Equal(new DateTime(2026, 6, 1), earliest!.Value);
    }
}
