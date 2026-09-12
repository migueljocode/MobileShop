namespace MobileShop.Services.DataServices.Interfaces;

public interface ISellerDataService : IDataService<Seller>
{
    // TODO: eager-load Product when supplier history needs full product info
    IEnumerable<Product> SoldProducts(int sellerId);
    IEnumerable<Product> SoldProducts(int sellerId, Expression<Func<Product, bool>> predicate);
    IEnumerable<Product> SoldToShop(int sellerId);
    IEnumerable<Product> SoldToShop(int sellerId, Expression<Func<Product, bool>> predicate);

    Task<IEnumerable<Product>> SoldProductsAsync(int sellerId);
    Task<IEnumerable<Product>> SoldProductsAsync(int sellerId, Expression<Func<Product, bool>> predicate);
    Task<IEnumerable<Product>> SoldToShopAsync(int sellerId);
    Task<IEnumerable<Product>> SoldToShopAsync(int sellerId, Expression<Func<Product, bool>> predicate);
}