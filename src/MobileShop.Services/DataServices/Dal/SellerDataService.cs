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

    /// <inheritdoc />
    public IReadOnlyList<SellerListItemViewModel> GetListRows()
        => _sellerRepo
            .SelectAll(seller => new SellerListItemViewModel(
                seller.Id,
                seller.PersonNavigation.FirstName + " " + seller.PersonNavigation.LastName,
                seller.PersonNavigation.PhoneNumber,
                seller.EntityType.ToString()))
            .OrderBy(row => row.Name)
            .ToList();

    /// <inheritdoc />
    public SellerDetailsViewModel? GetDetails(int id)
        => _sellerRepo.Select(
            id,
            seller => new SellerDetailsViewModel(
                seller.PersonNavigation.FirstName + " " + seller.PersonNavigation.LastName,
                seller.PersonNavigation.PhoneNumber,
                seller.EntityType.ToString()));

    // ── Sold products (both directions) ───────────────────

    /// <inheritdoc />
    public IEnumerable<Product> SoldProducts(int sellerId)
        => _sellerRepo
            .SelectAll(
                seller => seller.Id == sellerId,
                seller => seller.Transactions
                    .Select(transaction => transaction.ProductNavigation)
                    .ToList())
            .SelectMany(products => products)
            .Distinct()
            .ToList();

    /// <inheritdoc />
    public async Task<IEnumerable<Product>> SoldProductsAsync(int sellerId)
        => (await _sellerRepo.SelectAllAsync(
                seller => seller.Id == sellerId,
                seller => seller.Transactions
                    .Select(transaction => transaction.ProductNavigation)
                    .ToList()))
            .SelectMany(products => products)
            .Distinct()
            .ToList();

    // ── Supplied to shop (Buy-direction only) ─────────────

    /// <inheritdoc />
    public IEnumerable<Product> SoldToShop(int sellerId)
        => _sellerRepo
            .SelectAll(
                seller => seller.Id == sellerId,
                seller => seller.Transactions
                    .Where(transaction => transaction.Direction == TransactionDirection.Buy)
                    .Select(transaction => transaction.ProductNavigation)
                    .ToList())
            .SelectMany(products => products)
            .Distinct()
            .ToList();

    /// <inheritdoc />
    public async Task<IEnumerable<Product>> SoldToShopAsync(int sellerId)
        => (await _sellerRepo.SelectAllAsync(
                seller => seller.Id == sellerId,
                seller => seller.Transactions
                    .Where(transaction => transaction.Direction == TransactionDirection.Buy)
                    .Select(transaction => transaction.ProductNavigation)
                    .ToList()))
            .SelectMany(products => products)
            .Distinct()
            .ToList();

}