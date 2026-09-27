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
    public async Task<IReadOnlyList<PartyOptionViewModel>> GetPartyOptionsAsync()
        => (await _sellerRepo
            .SelectAllAsync(seller => new PartyOptionViewModel(
                seller.Id,
                seller.PersonNavigation.FirstName + " " + seller.PersonNavigation.LastName,
                seller.EntityType.ToString())))
            .OrderBy(row => row.Label)
            .ToList();

    /// <inheritdoc />
    public IReadOnlyList<SellerListItemViewModel> GetListRows(string sortBy = "Name", bool ascending = true)
    {
        var rows = _sellerRepo
            .SelectAll(seller => new SellerListItemViewModel(
                seller.Id,
                seller.PersonNavigation.FirstName + " " + seller.PersonNavigation.LastName,
                seller.PersonNavigation.PhoneNumber,
                seller.EntityType.ToString(),
                seller.Transactions.Count(t => t.Direction == TransactionDirection.Buy && !t.IsDeleted)));

        return (sortBy, ascending) switch
        {
            ("Phone", true) => rows.OrderBy(row => row.PhoneNumber).ToList(),
            ("Phone", false) => rows.OrderByDescending(row => row.PhoneNumber).ToList(),
            ("Count", true) => rows.OrderBy(row => row.SoldCount).ToList(),
            ("Count", false) => rows.OrderByDescending(row => row.SoldCount).ToList(),
            (_, false) => rows.OrderByDescending(row => row.Name).ToList(),
            _ => rows.OrderBy(row => row.Name).ToList(),
        };
    }

    /// <inheritdoc />
    public async Task<IReadOnlyList<SellerListItemViewModel>> GetListRowsAsync(string sortBy = "Name", bool ascending = true)
    {
        var rows = await _sellerRepo
            .SelectAllAsync(seller => new SellerListItemViewModel(
                seller.Id,
                seller.PersonNavigation.FirstName + " " + seller.PersonNavigation.LastName,
                seller.PersonNavigation.PhoneNumber,
                seller.EntityType.ToString(),
                seller.Transactions.Count(t => t.Direction == TransactionDirection.Buy && !t.IsDeleted)));

        return (sortBy, ascending) switch
        {
            ("Phone", true) => rows.OrderBy(row => row.PhoneNumber).ToList(),
            ("Phone", false) => rows.OrderByDescending(row => row.PhoneNumber).ToList(),
            ("Count", true) => rows.OrderBy(row => row.SoldCount).ToList(),
            ("Count", false) => rows.OrderByDescending(row => row.SoldCount).ToList(),
            (_, false) => rows.OrderByDescending(row => row.Name).ToList(),
            _ => rows.OrderBy(row => row.Name).ToList(),
        };
    }

    /// <inheritdoc />
    public SellerDetailsViewModel? GetDetails(int id)
        => _sellerRepo.Select(
            id,
            seller => new SellerDetailsViewModel(
                seller.PersonNavigation.FirstName + " " + seller.PersonNavigation.LastName,
                seller.PersonNavigation.PhoneNumber,
                seller.EntityType.ToString()));

    /// <inheritdoc />
    public Task<SellerDetailsViewModel?> GetDetailsAsync(int id)
        => _sellerRepo.SelectAsync(
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