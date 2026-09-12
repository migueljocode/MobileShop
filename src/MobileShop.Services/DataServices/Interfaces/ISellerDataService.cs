namespace MobileShop.Services.DataServices.Interfaces;

public interface ISellerDataService : IDataService<Seller>
{
    // both directions - what this seller supplied to the shop AND what it sold on (shop's own sentinel Seller)
    IEnumerable<Product> SoldProducts(int sellerId);
    IEnumerable<Product> SoldProducts(int sellerId, Expression<Func<Product, bool>> predicate);
    Task<IEnumerable<Product>> SoldProductsAsync(int sellerId);
    Task<IEnumerable<Product>> SoldProductsAsync(int sellerId, Expression<Func<Product, bool>> predicate);

    // Buy-direction only - what this seller supplied to the shop
    IEnumerable<Product> SoldToShop(int sellerId);
    IEnumerable<Product> SoldToShop(int sellerId, Expression<Func<Product, bool>> predicate);
    Task<IEnumerable<Product>> SoldToShopAsync(int sellerId);
    Task<IEnumerable<Product>> SoldToShopAsync(int sellerId, Expression<Func<Product, bool>> predicate);
}