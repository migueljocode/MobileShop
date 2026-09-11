namespace MobileShop.Dal.Repos;

/// <inheritdoc cref="ICustomerRepo" />
public class CustomerRepo(AppDbContext context) : BaseRepo<Customer>(context), ICustomerRepo
{
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