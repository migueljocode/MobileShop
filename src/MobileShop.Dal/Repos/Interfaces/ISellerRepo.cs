namespace MobileShop.Dal.Repos.Interfaces;

/// <summary>Repository for <see cref="Seller"/> entities.</summary>
public interface ISellerRepo : IBaseRepo<Seller>
{
    IEnumerable<Product>? SoldProducts(int sellerId);
    Task<IEnumerable<Product>?> SoldProductsAsync(int sellerId);
    IEnumerable<Product>? SoldToShop(int sellerId);
    Task<IEnumerable<Product>?> SoldToShopAsync(int sellerId);
}
