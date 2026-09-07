namespace MobileShop.Dal.Repos;

/// <inheritdoc cref="IProductRepo" />
public class ProductRepo(AppDbContext context) : BaseRepo<Product>(context), IProductRepo
{
    /// <inheritdoc />
    public bool IsInStock(int productId)
        => !Table.Where(p => p.Id == productId)
                 .SelectMany(p => p.Transactions)
                 .Any(t => t.Direction == TransactionDirection.Sell);

    /// <inheritdoc />
    public async Task<bool> IsInStockAsync(int productId)
        => !await Table.Where(p => p.Id == productId)
                       .SelectMany(p => p.Transactions)
                       .AnyAsync(t => t.Direction == TransactionDirection.Sell);
}
