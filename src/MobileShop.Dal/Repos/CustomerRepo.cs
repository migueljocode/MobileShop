namespace MobileShop.Dal.Repos;

/// <inheritdoc cref="ICustomerRepo" />
public class CustomerRepo(AppDbContext context) : BaseRepo<Customer>(context), ICustomerRepo
{
    public override Customer? Find(int id)
        =>  Table.Include(x => x.PersonNavigation)
                .FirstOrDefault(x => x.Id == id);
    public override async Task<Customer?> FindAsync(int id)
        => await Table.Include(x => x.PersonNavigation)
                .FirstOrDefaultAsync(x => x.Id == id);

    public IEnumerable<Product>? PurchasedProducts(int customerId)
        => Table
            .Where(c => c.Id == customerId)
            .SelectMany(c => c.Transactions)
            .Where(t => t.Direction == TransactionDirection.Sell)
            .Select(t => t.ProductNavigation)
            .Distinct()
            .ToList();

    public async Task<IEnumerable<Product>?> PurchasedProductsAsync(int customerId)
        => await Table
            .Where(c => c.Id == customerId)
            .SelectMany(c => c.Transactions)
            .Where(t => t.Direction == TransactionDirection.Sell)
            .Select(t => t.ProductNavigation)
            .Distinct()
            .ToListAsync();
}