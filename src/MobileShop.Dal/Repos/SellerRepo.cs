namespace MobileShop.Dal.Repos;

/// <inheritdoc cref="ISellerRepo" />
public class SellerRepo(AppDbContext context) : BaseRepo<Seller>(context), ISellerRepo
{
    public IEnumerable<Product>? SoldProducts(int sellerId)
    {
        throw new NotImplementedException();
    }

    public Task<IEnumerable<Product>?> SoldProductsAsync(int sellerId)
    {
        throw new NotImplementedException();
    }
}
