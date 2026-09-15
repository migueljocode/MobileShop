namespace MobileShop.Services.DataServices.Dal;

public class AppleIdDataService(
    IAppleIdRepo appleIdRepo,
    ILogger<AppleIdDataService> logger)
    : DataServiceBase<AppleIdDataService, AppleId>(appleIdRepo, logger), IAppleIdDataService
{
    private readonly IAppleIdRepo _appleIdRepo = appleIdRepo;

    public IReadOnlyList<ProductListItemViewModel> GetInventoryRows()
        => _appleIdRepo.SelectAll(appleId => new ProductListItemViewModel(
                appleId.Id,
                appleId.ProductId,
                "Apple ID",
                appleId.ProductNavigation.Manufacturer + " " + appleId.ProductNavigation.Model,
                appleId.Email,
                null,
                appleId.ProductNavigation.Transactions.Any(t => t.Direction == TransactionDirection.Sell),
                appleId.ProductNavigation.SecondHandProfile != null))
            .OrderBy(row => row.ProductId)
            .ToList();

    public IReadOnlyList<ProductListItemViewModel> GetSelectableProducts(TransactionDirection direction)
        => _appleIdRepo
            .SelectAll(
                appleId => appleId.ProductNavigation.Transactions.All(transaction => transaction.Direction != direction),
                appleId => new ProductListItemViewModel(
                    appleId.Id,
                    appleId.ProductId,
                    "Apple ID",
                    appleId.ProductNavigation.Manufacturer + " " + appleId.ProductNavigation.Model,
                    appleId.Email,
                    null,
                    appleId.ProductNavigation.Transactions.Any(t => t.Direction == TransactionDirection.Sell),
                    appleId.ProductNavigation.SecondHandProfile != null))
            .OrderBy(row => row.ProductId)
            .ToList();

    public IReadOnlyList<ProductListItemViewModel> GetSecondHandRows()
        => _appleIdRepo
            .SelectAll(
                appleId => appleId.ProductNavigation.SecondHandProfile != null,
                appleId => new ProductListItemViewModel(
                    appleId.Id,
                    appleId.ProductId,
                    "Apple ID",
                    appleId.ProductNavigation.Manufacturer + " " + appleId.ProductNavigation.Model,
                    appleId.Email,
                    null,
                    appleId.ProductNavigation.Transactions.Any(t => t.Direction == TransactionDirection.Sell),
                    appleId.ProductNavigation.SecondHandProfile != null))
            .OrderBy(row => row.ProductId)
            .ToList();

    public IReadOnlyList<ProductListItemViewModel> GetAvailableSecondHandRows()
        => _appleIdRepo
            .SelectAll(
                appleId => appleId.ProductNavigation.SecondHandProfile != null &&
                          !appleId.ProductNavigation.Transactions.Any(transaction => transaction.Direction == TransactionDirection.Sell),
                appleId => new ProductListItemViewModel(
                    appleId.Id,
                    appleId.ProductId,
                    "Apple ID",
                    appleId.ProductNavigation.Manufacturer + " " + appleId.ProductNavigation.Model,
                    appleId.Email,
                    null,
                    appleId.ProductNavigation.Transactions.Any(t => t.Direction == TransactionDirection.Sell),
                    appleId.ProductNavigation.SecondHandProfile != null))
            .OrderBy(row => row.ProductId)
            .ToList();

    public ProductDetailsViewModel? GetDetails(int id)
        => _appleIdRepo.Select(
            id,
            appleId => new ProductDetailsViewModel(
                "Apple ID",
                appleId.ProductId,
                appleId.ProductNavigation.Manufacturer,
                appleId.ProductNavigation.Model,
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

    public AppleId? FindByEmail(string email)
        => _appleIdRepo.Find(email);

    public Task<AppleId?> FindByEmailAsync(string email)
        => _appleIdRepo.FindAsync(email);

    public bool IsSold(int id)
        => _appleIdRepo.Any(appleId => appleId.Id == id &&
            appleId.ProductNavigation.Transactions.Any(transaction => transaction.Direction == TransactionDirection.Sell));

    public Task<bool> IsSoldAsync(int id)
        => _appleIdRepo.AnyAsync(appleId => appleId.Id == id &&
            appleId.ProductNavigation.Transactions.Any(transaction => transaction.Direction == TransactionDirection.Sell));

    // ── Owner (null ⇒ not sold) ───────────────────────────

    public Customer? GetOwner(int id)
        => _appleIdRepo.Select(
            appleId => appleId.Id == id,
            appleId => appleId.ProductNavigation.Transactions
                .Where(transaction => transaction.Direction == TransactionDirection.Sell)
                .OrderByDescending(transaction => transaction.Date)
                .Select(transaction => transaction.CustomerNavigation)
                .FirstOrDefault());

    public Task<Customer?> GetOwnerAsync(int id)
        => _appleIdRepo.SelectAsync(
            appleId => appleId.Id == id,
            appleId => appleId.ProductNavigation.Transactions
                .Where(transaction => transaction.Direction == TransactionDirection.Sell)
                .OrderByDescending(transaction => transaction.Date)
                .Select(transaction => transaction.CustomerNavigation)
                .FirstOrDefault());

    // ── Guarantee ─────────────────────────────────────────

    public Guarantee? GetGuarantee(int id)
        => _appleIdRepo.Select(appleId => appleId.Id == id, appleId => appleId.ProductNavigation.GuaranteeProfile);

    public Task<Guarantee?> GetGuaranteeAsync(int id)
        => _appleIdRepo.SelectAsync(appleId => appleId.Id == id, appleId => appleId.ProductNavigation.GuaranteeProfile);

    // ── Second-hand (null ⇒ not second-hand) ──────────────

    public SecondHand? GetSecondHandInfo(int id)
        => _appleIdRepo.Select(appleId => appleId.Id == id, appleId => appleId.ProductNavigation.SecondHandProfile);

    public Task<SecondHand?> GetSecondHandInfoAsync(int id)
        => _appleIdRepo.SelectAsync(appleId => appleId.Id == id, appleId => appleId.ProductNavigation.SecondHandProfile);

    // ── Quantities ────────────────────────────────────────

    public int Quantity()
        => _appleIdRepo.Count();

    public int SecondHandQuantity()
        => _appleIdRepo.Count(a => a.ProductNavigation.SecondHandProfile != null);

    public int AvailableSecondHandQuantity()
        => _appleIdRepo.Count(a =>
                a.ProductNavigation.SecondHandProfile != null &&
                !a.ProductNavigation.Transactions.Any(t => t.Direction == TransactionDirection.Sell));

    public Task<int> QuantityAsync()
        => _appleIdRepo.CountAsync();

    public async Task<int> SecondHandQuantityAsync()
        => await _appleIdRepo.CountAsync(a => a.ProductNavigation.SecondHandProfile != null);

    public async Task<int> AvailableSecondHandQuantityAsync()
        => await _appleIdRepo.CountAsync(a =>
                a.ProductNavigation.SecondHandProfile != null &&
                !a.ProductNavigation.Transactions.Any(t => t.Direction == TransactionDirection.Sell));

    // ── Lists ─────────────────────────────────────────────

    public IEnumerable<AppleId> GetSecondHand()
        => _appleIdRepo.FindAll(a => a.ProductNavigation.SecondHandProfile != null);

    public IEnumerable<AppleId> GetAvailableSecondHand()
        => _appleIdRepo.FindAll(a =>
            a.ProductNavigation.SecondHandProfile != null &&
            !a.ProductNavigation.Transactions.Any(t => t.Direction == TransactionDirection.Sell));

    public Task<IEnumerable<AppleId>> GetSecondHandAsync()
        => _appleIdRepo.FindAllAsync(a => a.ProductNavigation.SecondHandProfile != null);

    public Task<IEnumerable<AppleId>> GetAvailableSecondHandAsync()
        => _appleIdRepo.FindAllAsync(a =>
            a.ProductNavigation.SecondHandProfile != null &&
            !a.ProductNavigation.Transactions.Any(t => t.Direction == TransactionDirection.Sell));
}