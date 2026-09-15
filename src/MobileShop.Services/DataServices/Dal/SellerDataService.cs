namespace MobileShop.Services.DataServices.Dal;

public class SellerDataService(
    ISellerRepo sellerRepo,
    ILogger<SellerDataService> logger)
    : DataServiceBase<SellerDataService, Seller>(sellerRepo, logger), ISellerDataService
{
    private readonly ISellerRepo _sellerRepo = sellerRepo;

    /// <inheritdoc />
    public IReadOnlyList<PartyOptionViewModel> GetPartyOptions()
        => _sellerRepo
            .SelectAll(seller => new PartyOptionViewModel(
                seller.Id,
                seller.PersonNavigation.FirstName + " " + seller.PersonNavigation.LastName,
                seller.EntityType.ToString()))
            .OrderBy(row => row.Label)
            .ToList();

    // ── Sold products (both directions) ───────────────────

    /// <inheritdoc />
    public IEnumerable<Product> SoldProducts(int sellerId)
        => _sellerRepo
            .FindAll(seller => seller.Id == sellerId)
            .SelectMany(seller => seller.Transactions)
            .Select(transaction => transaction.ProductNavigation)
            .Distinct()
            .ToList();

    /// <inheritdoc />
    public async Task<IEnumerable<Product>> SoldProductsAsync(int sellerId)
        => (await _sellerRepo.FindAllAsync(seller => seller.Id == sellerId))
            .SelectMany(seller => seller.Transactions)
            .Select(transaction => transaction.ProductNavigation)
            .Distinct()
            .ToList();

    // ── Supplied to shop (Buy-direction only) ─────────────

    /// <inheritdoc />
    public IEnumerable<Product> SoldToShop(int sellerId)
        => _sellerRepo
            .FindAll(seller => seller.Id == sellerId)
            .SelectMany(seller => seller.Transactions)
            .Where(transaction => transaction.Direction == TransactionDirection.Buy)
            .Select(transaction => transaction.ProductNavigation)
            .Distinct()
            .ToList();

    /// <inheritdoc />
    public async Task<IEnumerable<Product>> SoldToShopAsync(int sellerId)
        => (await _sellerRepo.FindAllAsync(seller => seller.Id == sellerId))
            .SelectMany(seller => seller.Transactions)
            .Where(transaction => transaction.Direction == TransactionDirection.Buy)
            .Select(transaction => transaction.ProductNavigation)
            .Distinct()
            .ToList();

}