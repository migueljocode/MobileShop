namespace MobileShop.Tests.Dal.BaseClass;

/// <summary>
/// Shared test suite for any repo implementing <see cref="IForSale{T}"/>, layered on top of
/// <see cref="BaseRepoTests{TEntity, TRepo}"/>'s CRUD coverage. A concrete test class also
/// implements <see cref="GetProductId"/> to say how to reach the underlying Product's Id from
/// its entity type, since <see cref="IForSale{T}"/>'s methods all operate through that Product.
/// </summary>
public abstract class ForSaleRepoTests<TEntity, TRepo> : BaseRepoTests<TEntity, TRepo>
    where TEntity : BaseEntity
    where TRepo : IBaseRepo<TEntity>, IForSale<TEntity>
{
    /// <summary>Gets the Id of the Product the given entity is attached to.</summary>
    protected abstract int GetProductId(TEntity entity);

    private (Seller Seller, Customer Customer) CreateSellerAndCustomer()
    {
        var sellerPerson = new Person { FirstName = "S", LastName = "P", PhoneNumber = "09121111111" };
        var customerPerson = new Person { FirstName = "C", LastName = "P", PhoneNumber = "09122222222" };
        Context.People.AddRange(sellerPerson, customerPerson);
        Context.SaveChanges();

        var seller = new Seller { PersonId = sellerPerson.Id, EntityType = SellerEntityType.Real };
        var customer = new Customer { PersonId = customerPerson.Id, NationalId = "9999999999" };
        Context.Sellers.Add(seller);
        Context.Customers.Add(customer);
        Context.SaveChanges();

        return (seller, customer);
    }

    private void SellProduct(int productId)
    {
        var (seller, customer) = CreateSellerAndCustomer();
        Context.Transactions.Add(new Transaction
        {
            Date = DateTime.Today,
            SellerId = seller.Id,
            CustomerId = customer.Id,
            ProductId = productId,
            FinishedPrice = 100,
            Direction = TransactionDirection.Sell
        });
        Context.SaveChanges();
    }

    [Fact]
    public void IsSold_ReturnsFalse_WhenNotSold()
    {
        var repo = CreateRepo();
        var entity = CreateValidEntity();
        repo.Add(entity);

        Assert.False(repo.IsSold(entity.Id));
    }

    [Fact]
    public void IsSold_ReturnsTrue_AfterSellTransaction()
    {
        var repo = CreateRepo();
        var entity = CreateValidEntity();
        repo.Add(entity);
        SellProduct(GetProductId(entity));

        Assert.True(repo.IsSold(entity.Id));
    }

    [Fact]
    public async Task IsSoldAsync_ReturnsTrue_AfterSellTransaction()
    {
        var repo = CreateRepo();
        var entity = CreateValidEntity();
        await repo.AddAsync(entity);
        SellProduct(GetProductId(entity));

        Assert.True(await repo.IsSoldAsync(entity.Id));
    }

    [Fact]
    public void Quantity_CountsOnlyUnsoldItems()
    {
        var repo = CreateRepo();
        var inStock = CreateValidEntity();
        var sold = CreateValidEntity();
        repo.Add(inStock);
        repo.Add(sold);
        SellProduct(GetProductId(sold));

        Assert.Equal(1, repo.Quantity());
    }

    [Fact]
    public async Task QuantityAsync_CountsOnlyUnsoldItems()
    {
        var repo = CreateRepo();
        var inStock = CreateValidEntity();
        var sold = CreateValidEntity();
        await repo.AddAsync(inStock);
        await repo.AddAsync(sold);
        SellProduct(GetProductId(sold));

        Assert.Equal(1, await repo.QuantityAsync());
    }

    [Fact]
    public void GetGuarantee_ReturnsNull_WhenProductHasNone()
    {
        var repo = CreateRepo();
        var entity = CreateValidEntity();
        repo.Add(entity);

        Assert.Null(repo.GetGuarantee(entity.Id));
    }

    [Fact]
    public void GetGuarantee_ReturnsIt_WhenProductHasOne()
    {
        var repo = CreateRepo();
        var entity = CreateValidEntity();
        repo.Add(entity);
        Context.Guarantees.Add(new Guarantee
        {
            StartDate = DateTime.Today,
            ExpirationDate = DateTime.Today.AddYears(1),
            Corporation = "TestCorp",
            ProductId = GetProductId(entity)
        });
        Context.SaveChanges();

        var guarantee = repo.GetGuarantee(entity.Id);

        Assert.NotNull(guarantee);
        Assert.Equal("TestCorp", guarantee!.Corporation);
    }

    [Fact]
    public async Task GetGuaranteeAsync_ReturnsIt_WhenProductHasOne()
    {
        var repo = CreateRepo();
        var entity = CreateValidEntity();
        await repo.AddAsync(entity);
        Context.Guarantees.Add(new Guarantee
        {
            StartDate = DateTime.Today,
            ExpirationDate = DateTime.Today.AddYears(1),
            Corporation = "TestCorp",
            ProductId = GetProductId(entity)
        });
        await Context.SaveChangesAsync();

        var guarantee = await repo.GetGuaranteeAsync(entity.Id);

        Assert.NotNull(guarantee);
        Assert.Equal("TestCorp", guarantee!.Corporation);
    }

    [Fact]
    public void GetOwner_ReturnsNull_WhenNotSold()
    {
        var repo = CreateRepo();
        var entity = CreateValidEntity();
        repo.Add(entity);

        Assert.Null(repo.GetOwner(entity.Id));
    }

    [Fact]
    public void GetOwner_ReturnsCustomer_AfterSellTransaction()
    {
        var repo = CreateRepo();
        var entity = CreateValidEntity();
        repo.Add(entity);
        SellProduct(GetProductId(entity));

        Assert.NotNull(repo.GetOwner(entity.Id));
    }

    [Fact]
    public async Task GetOwnerAsync_ReturnsCustomer_AfterSellTransaction()
    {
        var repo = CreateRepo();
        var entity = CreateValidEntity();
        await repo.AddAsync(entity);
        SellProduct(GetProductId(entity));

        Assert.NotNull(await repo.GetOwnerAsync(entity.Id));
    }

    [Fact]
    public void IsSecondHand_ReturnsFalse_WhenProductHasNoSecondHandProfile()
    {
        var repo = CreateRepo();
        var entity = CreateValidEntity();
        repo.Add(entity);

        Assert.False(repo.IsSecondHand(entity.Id));
    }

    [Fact]
    public void IsSecondHand_ReturnsTrue_WhenProductHasSecondHandProfile()
    {
        var repo = CreateRepo();
        var entity = CreateValidEntity();
        repo.Add(entity);
        Context.SecondHands.Add(new SecondHand { TestPeriodDays = 7, ProductId = GetProductId(entity) });
        Context.SaveChanges();

        Assert.True(repo.IsSecondHand(entity.Id));
    }

    [Fact]
    public async Task IsSecondHandAsync_ReturnsTrue_WhenProductHasSecondHandProfile()
    {
        var repo = CreateRepo();
        var entity = CreateValidEntity();
        await repo.AddAsync(entity);
        Context.SecondHands.Add(new SecondHand { TestPeriodDays = 7, ProductId = GetProductId(entity) });
        await Context.SaveChangesAsync();

        Assert.True(await repo.IsSecondHandAsync(entity.Id));
    }
}