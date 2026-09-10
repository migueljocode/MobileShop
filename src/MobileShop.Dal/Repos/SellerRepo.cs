namespace MobileShop.Dal.Repos;

/// <inheritdoc cref="ISellerRepo" />
public class SellerRepo(AppDbContext context) : BaseRepo<Seller>(context), ISellerRepo
{
    // All products this seller was involved in
    public IEnumerable<Product>? SoldProducts(int sellerId)
    {
        return Table
            .Where(s => s.Id == sellerId)
            .SelectMany(s => s.Transactions)
            .Select(t => t.ProductNavigation)
            .Distinct()
            .ToList();
    }

    public async Task<IEnumerable<Product>?> SoldProductsAsync(int sellerId)
    {
        return await Table
            .Where(s => s.Id == sellerId)
            .SelectMany(s => s.Transactions)
            .Select(t => t.ProductNavigation)
            .Distinct()
            .ToListAsync();
    }

    // Only products this seller sold to the shop
    public IEnumerable<Product>? SoldToShop(int sellerId)
    {
        return Table
            .Where(s => s.Id == sellerId)
            .SelectMany(s => s.Transactions)
            .Where(t => t.Direction == TransactionDirection.Buy)
            .Select(t => t.ProductNavigation)
            .Distinct()
            .ToList();
    }

    public async Task<IEnumerable<Product>?> SoldToShopAsync(int sellerId)
    {
        return await Table
            .Where(s => s.Id == sellerId)
            .SelectMany(s => s.Transactions)
            .Where(t => t.Direction == TransactionDirection.Buy)
            .Select(t => t.ProductNavigation)
            .Distinct()
            .ToListAsync();
    }
}
