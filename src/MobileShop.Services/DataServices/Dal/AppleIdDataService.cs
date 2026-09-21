namespace MobileShop.Services.DataServices.Dal;

public class AppleIdDataService(
    IAppleIdRepo appleIdRepo,
    ILogger<AppleIdDataService> logger)
    : DataServiceBase<AppleIdDataService, AppleId>(appleIdRepo, logger), IAppleIdDataService
{
    private readonly IAppleIdRepo _appleIdRepo = appleIdRepo;

    /// <inheritdoc />
    public IReadOnlyList<ProductListItemViewModel> GetInventoryRows()
        => _appleIdRepo.SelectAll(appleId => new ProductListItemViewModel(
                appleId.Id,
                appleId.ProductId,
                "Apple ID",
                appleId.ProductNavigation.ModelNavigation.ManufacturerNavigation.Name + " " + appleId.ProductNavigation.ModelNavigation.Name,
                appleId.Email,
                null,
                appleId.ProductNavigation.Transactions.Any(t => t.Direction == TransactionDirection.Sell),
                appleId.ProductNavigation.SecondHandProfile != null))
            .OrderBy(row => row.ProductId)
            .ToList();

    /// <inheritdoc />
    public IReadOnlyList<ProductListItemViewModel> GetSelectableProducts(TransactionDirection direction)
        => _appleIdRepo
            .SelectAll(
                appleId => appleId.ProductNavigation.Transactions.All(transaction => transaction.Direction != direction),
                appleId => new ProductListItemViewModel(
                    appleId.Id,
                    appleId.ProductId,
                    "Apple ID",
                    appleId.ProductNavigation.ModelNavigation.ManufacturerNavigation.Name + " " + appleId.ProductNavigation.ModelNavigation.Name,
                    appleId.Email,
                    null,
                    appleId.ProductNavigation.Transactions.Any(t => t.Direction == TransactionDirection.Sell),
                    appleId.ProductNavigation.SecondHandProfile != null))
            .OrderBy(row => row.ProductId)
            .ToList();

    /// <inheritdoc />
    public IReadOnlyList<ProductListItemViewModel> GetSecondHandRows()
        => _appleIdRepo
            .SelectAll(
                appleId => appleId.ProductNavigation.SecondHandProfile != null,
                appleId => new ProductListItemViewModel(
                    appleId.Id,
                    appleId.ProductId,
                    "Apple ID",
                    appleId.ProductNavigation.ModelNavigation.ManufacturerNavigation.Name + " " + appleId.ProductNavigation.ModelNavigation.Name,
                    appleId.Email,
                    null,
                    appleId.ProductNavigation.Transactions.Any(t => t.Direction == TransactionDirection.Sell),
                    appleId.ProductNavigation.SecondHandProfile != null))
            .OrderBy(row => row.ProductId)
            .ToList();

    /// <inheritdoc />
    public IReadOnlyList<ProductListItemViewModel> GetAvailableSecondHandRows()
        => _appleIdRepo
            .SelectAll(
                appleId => appleId.ProductNavigation.SecondHandProfile != null &&
                          !appleId.ProductNavigation.Transactions.Any(transaction => transaction.Direction == TransactionDirection.Sell),
                appleId => new ProductListItemViewModel(
                    appleId.Id,
                    appleId.ProductId,
                    "Apple ID",
                    appleId.ProductNavigation.ModelNavigation.ManufacturerNavigation.Name + " " + appleId.ProductNavigation.ModelNavigation.Name,
                    appleId.Email,
                    null,
                    appleId.ProductNavigation.Transactions.Any(t => t.Direction == TransactionDirection.Sell),
                    appleId.ProductNavigation.SecondHandProfile != null))
            .OrderBy(row => row.ProductId)
            .ToList();

    /// <inheritdoc />
    public ProductDetailsViewModel? GetDetails(int id)
        => _appleIdRepo.Select(
            id,
            appleId => new ProductDetailsViewModel(
                "Apple ID",
                appleId.ProductId,
                appleId.ProductNavigation.ModelNavigation.ManufacturerNavigation.Name,
                appleId.ProductNavigation.ModelNavigation.Name,
                appleId.Email,
                null,
                appleId.ProductNavigation.Transactions
                    .Where(t => t.Direction == TransactionDirection.Sell)
                    .OrderByDescending(t => t.Date)
                    .Select(t => t.CustomerNavigation.PersonNavigation)
                    .Select(person => person.FirstName + " " + person.LastName)
                    .FirstOrDefault() ?? "Not sold",
                appleId.ProductNavigation.GuaranteeProfile == null
                    ? "None"
                    : appleId.ProductNavigation.GuaranteeProfile.Corporation + " until " + appleId.ProductNavigation.GuaranteeProfile.ExpirationDate,
                appleId.ProductNavigation.SecondHandProfile != null));

    // ── Email ─────────────────────────────────────────────

    /// <inheritdoc />
    public AppleId? FindByEmail(string email)
        => _appleIdRepo.Find(email);

    /// <inheritdoc />
    public Task<AppleId?> FindByEmailAsync(string email)
        => _appleIdRepo.FindAsync(email);

    /// <inheritdoc />
    public bool IsSold(int id)
        => _appleIdRepo.Any(appleId => appleId.Id == id &&
            appleId.ProductNavigation.Transactions.Any(transaction => transaction.Direction == TransactionDirection.Sell));

    /// <inheritdoc />
    public Task<bool> IsSoldAsync(int id)
        => _appleIdRepo.AnyAsync(appleId => appleId.Id == id &&
            appleId.ProductNavigation.Transactions.Any(transaction => transaction.Direction == TransactionDirection.Sell));

    // ── Owner (null ⇒ not sold) ───────────────────────────

    /// <inheritdoc />
    public Customer? GetOwner(int id)
        => _appleIdRepo.Select(
            appleId => appleId.Id == id,
            appleId => appleId.ProductNavigation.Transactions
                .Where(transaction => transaction.Direction == TransactionDirection.Sell)
                .OrderByDescending(transaction => transaction.Date)
                .Select(transaction => transaction.CustomerNavigation)
                .FirstOrDefault());

    /// <inheritdoc />
    public Task<Customer?> GetOwnerAsync(int id)
        => _appleIdRepo.SelectAsync(
            appleId => appleId.Id == id,
            appleId => appleId.ProductNavigation.Transactions
                .Where(transaction => transaction.Direction == TransactionDirection.Sell)
                .OrderByDescending(transaction => transaction.Date)
                .Select(transaction => transaction.CustomerNavigation)
                .FirstOrDefault());

    // ── Guarantee ─────────────────────────────────────────

    /// <inheritdoc />
    public Guarantee? GetGuarantee(int id)
        => _appleIdRepo.Select(appleId => appleId.Id == id, appleId => appleId.ProductNavigation.GuaranteeProfile);

    /// <inheritdoc />
    public Task<Guarantee?> GetGuaranteeAsync(int id)
        => _appleIdRepo.SelectAsync(appleId => appleId.Id == id, appleId => appleId.ProductNavigation.GuaranteeProfile);

    // ── Second-hand (null ⇒ not second-hand) ──────────────

    /// <inheritdoc />
    public SecondHand? GetSecondHandInfo(int id)
        => _appleIdRepo.Select(appleId => appleId.Id == id, appleId => appleId.ProductNavigation.SecondHandProfile);

    /// <inheritdoc />
    public Task<SecondHand?> GetSecondHandInfoAsync(int id)
        => _appleIdRepo.SelectAsync(appleId => appleId.Id == id, appleId => appleId.ProductNavigation.SecondHandProfile);

    // ── Quantities ────────────────────────────────────────

    /// <inheritdoc />
    public int Quantity()
        => _appleIdRepo.Count();

    /// <inheritdoc />
    public int SecondHandQuantity()
        => _appleIdRepo.Count(a => a.ProductNavigation.SecondHandProfile != null);

    /// <inheritdoc />
    public int AvailableSecondHandQuantity()
        => _appleIdRepo.Count(a =>
                a.ProductNavigation.SecondHandProfile != null &&
                !a.ProductNavigation.Transactions.Any(t => t.Direction == TransactionDirection.Sell));

    /// <inheritdoc />
    public Task<int> QuantityAsync()
        => _appleIdRepo.CountAsync();

    /// <inheritdoc />
    public async Task<int> SecondHandQuantityAsync()
        => await _appleIdRepo.CountAsync(a => a.ProductNavigation.SecondHandProfile != null);

    /// <inheritdoc />
    public async Task<int> AvailableSecondHandQuantityAsync()
        => await _appleIdRepo.CountAsync(a =>
                a.ProductNavigation.SecondHandProfile != null &&
                !a.ProductNavigation.Transactions.Any(t => t.Direction == TransactionDirection.Sell));

    // ── Lists ─────────────────────────────────────────────

    /// <inheritdoc />
    public IEnumerable<AppleId> GetSecondHand()
        => _appleIdRepo.FindAll(a => a.ProductNavigation.SecondHandProfile != null);

    /// <inheritdoc />
    public IEnumerable<AppleId> GetAvailableSecondHand()
        => _appleIdRepo.FindAll(a =>
            a.ProductNavigation.SecondHandProfile != null &&
            !a.ProductNavigation.Transactions.Any(t => t.Direction == TransactionDirection.Sell));

    /// <inheritdoc />
    public Task<IEnumerable<AppleId>> GetSecondHandAsync()
        => _appleIdRepo.FindAllAsync(a => a.ProductNavigation.SecondHandProfile != null);

    /// <inheritdoc />
    public Task<IEnumerable<AppleId>> GetAvailableSecondHandAsync()
        => _appleIdRepo.FindAllAsync(a =>
            a.ProductNavigation.SecondHandProfile != null &&
            !a.ProductNavigation.Transactions.Any(t => t.Direction == TransactionDirection.Sell));
}