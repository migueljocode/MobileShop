namespace MobileShop.Dal.Repos;

/// <inheritdoc cref="ICustomerRepo" />
public class CustomerRepo(AppDbContext context) : BaseRepo<Customer>(context), ICustomerRepo
{
    public override IEnumerable<Customer> GetAll(Expression<Func<Customer, bool>>? predicate = null)
    {
        IQueryable<Customer> query = Table.Include(c => c.PersonNavigation);
        return (predicate is null ? query : query.Where(predicate)).ToList();
    }

    public override async Task<IEnumerable<Customer>> GetAllAsync(Expression<Func<Customer, bool>>? predicate = null)
    {
        IQueryable<Customer> query = Table.Include(c => c.PersonNavigation);
        return await (predicate is null ? query : query.Where(predicate)).ToListAsync();
    }

    /// <inheritdoc />
    /// <remarks>Also eagerly loads <see cref="Customer.PersonNavigation"/>.</remarks>
    public override Customer? Find(int id)
        => Table.Include(x => x.PersonNavigation)
            .FirstOrDefault(x => x.Id == id);

    /// <inheritdoc />
    /// <remarks>Also eagerly loads <see cref="Customer.PersonNavigation"/>.</remarks>
    public override async Task<Customer?> FindAsync(int id)
        => await Table.Include(x => x.PersonNavigation)
            .FirstOrDefaultAsync(x => x.Id == id);

    /// <inheritdoc />
    public IEnumerable<Product>? PurchasedProducts(int customerId)
        => Table
            .Where(c => c.Id == customerId)
            .SelectMany(c => c.Transactions)
            .Where(t => t.Direction == TransactionDirection.Sell)
            .Select(t => t.ProductNavigation)
            .Distinct()
            .ToList();

    /// <inheritdoc />
    public async Task<IEnumerable<Product>?> PurchasedProductsAsync(int customerId)
        => await Table
            .Where(c => c.Id == customerId)
            .SelectMany(c => c.Transactions)
            .Where(t => t.Direction == TransactionDirection.Sell)
            .Select(t => t.ProductNavigation)
            .Distinct()
            .ToListAsync();
}