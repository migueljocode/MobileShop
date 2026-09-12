namespace MobileShop.Services.DataServices.Dal;

public class SellerDataService(
    ISellerRepo sellerRepo,
    ILogger<SellerDataService> logger)
    : DataServiceBase<SellerDataService, Seller>(sellerRepo, logger), ISellerDataService
{
    private readonly ISellerRepo _sellerRepo = sellerRepo;

    // ── Sold products (both directions) ───────────────────

    public IEnumerable<Product> SoldProducts(int sellerId)
        => _sellerRepo.SoldProducts(sellerId) ?? [];

    public IEnumerable<Product> SoldProducts(int sellerId, Expression<Func<Product, bool>> predicate)
        => SoldProducts(sellerId).AsQueryable().Where(predicate).ToList();

    public async Task<IEnumerable<Product>> SoldProductsAsync(int sellerId)
        => await _sellerRepo.SoldProductsAsync(sellerId) ?? [];

    public async Task<IEnumerable<Product>> SoldProductsAsync(
        int sellerId,
        Expression<Func<Product, bool>> predicate)
    {
        var products = await SoldProductsAsync(sellerId);
        return products.AsQueryable().Where(predicate).ToList();
    }

    // ── Supplied to shop (Buy-direction only) ─────────────

    public IEnumerable<Product> SoldToShop(int sellerId)
        => _sellerRepo.SoldToShop(sellerId) ?? [];

    public IEnumerable<Product> SoldToShop(int sellerId, Expression<Func<Product, bool>> predicate)
        => SoldToShop(sellerId).AsQueryable().Where(predicate).ToList();

    public async Task<IEnumerable<Product>> SoldToShopAsync(int sellerId)
        => await _sellerRepo.SoldToShopAsync(sellerId) ?? [];

    public async Task<IEnumerable<Product>> SoldToShopAsync(
        int sellerId,
        Expression<Func<Product, bool>> predicate)
    {
        var products = await SoldToShopAsync(sellerId);
        return products.AsQueryable().Where(predicate).ToList();
    }
}