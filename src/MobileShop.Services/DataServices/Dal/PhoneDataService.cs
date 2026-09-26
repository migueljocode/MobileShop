namespace MobileShop.Services.DataServices.Dal;

public class PhoneDataService(
    IPhoneRepo phoneRepo,
    ILogger<PhoneDataService> logger)
    : DataServiceBase<PhoneDataService, Phone>(phoneRepo, logger), IPhoneDataService
{
    private readonly IPhoneRepo _phoneRepo = phoneRepo;

    /// <inheritdoc />
    public IReadOnlyList<ProductListItemViewModel> GetInventoryRows()
        => _phoneRepo.SelectAll(phone => new ProductListItemViewModel(
                phone.Id,
                phone.ProductId,
                "Phone",
                phone.ProductNavigation.ModelNavigation.ManufacturerNavigation.Name + " " + phone.ProductNavigation.ModelNavigation.Name,
                "IMEI: " + phone.IMEI1,
                (phone.ProductNavigation.ColorNavigation == null ? null : phone.ProductNavigation.ColorNavigation.Name),
                phone.ProductNavigation.Transactions.Any(t => t.Direction == TransactionDirection.Sell),
                phone.ProductNavigation.SecondHandProfile != null))
            .OrderBy(row => row.ProductId)
            .ToList();

    /// <inheritdoc />
    public async Task<IReadOnlyList<ProductListItemViewModel>> GetInventoryRowsAsync()
        => (await _phoneRepo.SelectAllAsync(phone => new ProductListItemViewModel(
                phone.Id,
                phone.ProductId,
                "Phone",
                phone.ProductNavigation.ModelNavigation.ManufacturerNavigation.Name + " " + phone.ProductNavigation.ModelNavigation.Name,
                "IMEI: " + phone.IMEI1,
                (phone.ProductNavigation.ColorNavigation == null ? null : phone.ProductNavigation.ColorNavigation.Name),
                phone.ProductNavigation.Transactions.Any(t => t.Direction == TransactionDirection.Sell),
                phone.ProductNavigation.SecondHandProfile != null)))
            .OrderBy(row => row.ProductId)
            .ToList();

    /// <inheritdoc />
    public IReadOnlyList<ProductListItemViewModel> GetSelectableProducts(TransactionDirection direction)
        => _phoneRepo
            .SelectAll(
                phone => phone.ProductNavigation.Transactions.All(transaction => transaction.Direction != direction),
                phone => new ProductListItemViewModel(
                    phone.Id,
                    phone.ProductId,
                    "Phone",
                    phone.ProductNavigation.ModelNavigation.ManufacturerNavigation.Name + " " + phone.ProductNavigation.ModelNavigation.Name,
                    "IMEI: " + phone.IMEI1,
                    (phone.ProductNavigation.ColorNavigation == null ? null : phone.ProductNavigation.ColorNavigation.Name),
                    phone.ProductNavigation.Transactions.Any(t => t.Direction == TransactionDirection.Sell),
                    phone.ProductNavigation.SecondHandProfile != null))
            .OrderBy(row => row.ProductId)
            .ToList();

    /// <inheritdoc />
    public async Task<IReadOnlyList<ProductListItemViewModel>> GetSelectableProductsAsync(TransactionDirection direction)
        => (await _phoneRepo
            .SelectAllAsync(
                phone => phone.ProductNavigation.Transactions.All(transaction => transaction.Direction != direction),
                phone => new ProductListItemViewModel(
                    phone.Id,
                    phone.ProductId,
                    "Phone",
                    phone.ProductNavigation.ModelNavigation.ManufacturerNavigation.Name + " " + phone.ProductNavigation.ModelNavigation.Name,
                    "IMEI: " + phone.IMEI1,
                    (phone.ProductNavigation.ColorNavigation == null ? null : phone.ProductNavigation.ColorNavigation.Name),
                    phone.ProductNavigation.Transactions.Any(t => t.Direction == TransactionDirection.Sell),
                    phone.ProductNavigation.SecondHandProfile != null)))
            .OrderBy(row => row.ProductId)
            .ToList();

    /// <inheritdoc />
    public IReadOnlyList<ProductListItemViewModel> GetSecondHandRows()
        => _phoneRepo
            .SelectAll(
                phone => phone.ProductNavigation.SecondHandProfile != null,
                phone => new ProductListItemViewModel(
                    phone.Id,
                    phone.ProductId,
                    "Phone",
                    phone.ProductNavigation.ModelNavigation.ManufacturerNavigation.Name + " " + phone.ProductNavigation.ModelNavigation.Name,
                    "IMEI: " + phone.IMEI1,
                    (phone.ProductNavigation.ColorNavigation == null ? null : phone.ProductNavigation.ColorNavigation.Name),
                    phone.ProductNavigation.Transactions.Any(t => t.Direction == TransactionDirection.Sell),
                    phone.ProductNavigation.SecondHandProfile != null))
            .OrderBy(row => row.ProductId)
            .ToList();

    /// <inheritdoc />
    public async Task<IReadOnlyList<ProductListItemViewModel>> GetSecondHandRowsAsync()
        => (await _phoneRepo
            .SelectAllAsync(
                phone => phone.ProductNavigation.SecondHandProfile != null,
                phone => new ProductListItemViewModel(
                    phone.Id,
                    phone.ProductId,
                    "Phone",
                    phone.ProductNavigation.ModelNavigation.ManufacturerNavigation.Name + " " + phone.ProductNavigation.ModelNavigation.Name,
                    "IMEI: " + phone.IMEI1,
                    (phone.ProductNavigation.ColorNavigation == null ? null : phone.ProductNavigation.ColorNavigation.Name),
                    phone.ProductNavigation.Transactions.Any(t => t.Direction == TransactionDirection.Sell),
                    phone.ProductNavigation.SecondHandProfile != null)))
            .OrderBy(row => row.ProductId)
            .ToList();

    /// <inheritdoc />
    public IReadOnlyList<ProductListItemViewModel> GetAvailableSecondHandRows()
        => _phoneRepo
            .SelectAll(
                phone => phone.ProductNavigation.SecondHandProfile != null &&
                         !phone.ProductNavigation.Transactions.Any(transaction => transaction.Direction == TransactionDirection.Sell),
                phone => new ProductListItemViewModel(
                    phone.Id,
                    phone.ProductId,
                    "Phone",
                    phone.ProductNavigation.ModelNavigation.ManufacturerNavigation.Name + " " + phone.ProductNavigation.ModelNavigation.Name,
                    "IMEI: " + phone.IMEI1,
                    (phone.ProductNavigation.ColorNavigation == null ? null : phone.ProductNavigation.ColorNavigation.Name),
                    phone.ProductNavigation.Transactions.Any(t => t.Direction == TransactionDirection.Sell),
                    phone.ProductNavigation.SecondHandProfile != null))
            .OrderBy(row => row.ProductId)
            .ToList();

    /// <inheritdoc />
    public async Task<IReadOnlyList<ProductListItemViewModel>> GetAvailableSecondHandRowsAsync()
        => (await _phoneRepo
            .SelectAllAsync(
                phone => phone.ProductNavigation.SecondHandProfile != null &&
                         !phone.ProductNavigation.Transactions.Any(t => t.Direction == TransactionDirection.Sell),
                phone => new ProductListItemViewModel(
                    phone.Id,
                    phone.ProductId,
                    "Phone",
                    phone.ProductNavigation.ModelNavigation.ManufacturerNavigation.Name + " " + phone.ProductNavigation.ModelNavigation.Name,
                    "IMEI: " + phone.IMEI1,
                    (phone.ProductNavigation.ColorNavigation == null ? null : phone.ProductNavigation.ColorNavigation.Name),
                    phone.ProductNavigation.Transactions.Any(t => t.Direction == TransactionDirection.Sell),
                    phone.ProductNavigation.SecondHandProfile != null)))
            .OrderBy(row => row.ProductId)
            .ToList();

    private static ProductListItemViewModel ToInventoryRow(Phone phone)
        => new(
            phone.Id,
            phone.ProductId,
            "Phone",
            phone.ProductNavigation.ModelNavigation.ManufacturerNavigation.Name + " " + phone.ProductNavigation.ModelNavigation.Name,
            "IMEI: " + phone.IMEI1,
            (phone.ProductNavigation.ColorNavigation == null ? null : phone.ProductNavigation.ColorNavigation.Name),
            phone.ProductNavigation.Transactions.Any(t => t.Direction == TransactionDirection.Sell),
            phone.ProductNavigation.SecondHandProfile != null);

    /// <inheritdoc />
    public ProductDetailsViewModel? GetDetails(int id)
        => _phoneRepo.Select(
            id,
            phone => new ProductDetailsViewModel(
                "Phone",
                phone.ProductId,
                phone.ProductNavigation.ModelNavigation.ManufacturerNavigation.Name,
                phone.ProductNavigation.ModelNavigation.Name,
                string.IsNullOrWhiteSpace(phone.IMEI2)
                    ? "IMEI: " + phone.IMEI1
                    : "IMEI: " + phone.IMEI1 + " / " + phone.IMEI2,
                (phone.ProductNavigation.ColorNavigation == null ? null : phone.ProductNavigation.ColorNavigation.Name),
                phone.ProductNavigation.Transactions
                    .Where(t => t.Direction == TransactionDirection.Sell)
                    .OrderByDescending(t => t.Date)
                    .Select(t => t.CustomerNavigation.PersonNavigation)
                    .Select(person => person.FirstName + " " + person.LastName)
                    .FirstOrDefault() ?? "Not sold",
                phone.ProductNavigation.GuaranteeProfile == null
                    ? "None"
                    : phone.ProductNavigation.GuaranteeProfile.Corporation + " until " + phone.ProductNavigation.GuaranteeProfile.ExpirationDate.ToString("d"),
                phone.ProductNavigation.SecondHandProfile != null));

    /// <inheritdoc />
    public Task<ProductDetailsViewModel?> GetDetailsAsync(int id)
        => _phoneRepo.SelectAsync(
            id,
            phone => new ProductDetailsViewModel(
                "Phone",
                phone.ProductId,
                phone.ProductNavigation.ModelNavigation.ManufacturerNavigation.Name,
                phone.ProductNavigation.ModelNavigation.Name,
                string.IsNullOrWhiteSpace(phone.IMEI2)
                    ? "IMEI: " + phone.IMEI1
                    : "IMEI: " + phone.IMEI1 + " / " + phone.IMEI2,
                (phone.ProductNavigation.ColorNavigation == null ? null : phone.ProductNavigation.ColorNavigation.Name),
                phone.ProductNavigation.Transactions
                    .Where(t => t.Direction == TransactionDirection.Sell)
                    .OrderByDescending(t => t.Date)
                    .Select(t => t.CustomerNavigation.PersonNavigation)
                    .Select(person => person.FirstName + " " + person.LastName)
                    .FirstOrDefault() ?? "Not sold",
                phone.ProductNavigation.GuaranteeProfile == null
                    ? "None"
                    : phone.ProductNavigation.GuaranteeProfile.Corporation + " until " + phone.ProductNavigation.GuaranteeProfile.ExpirationDate.ToString("d"),
                phone.ProductNavigation.SecondHandProfile != null));

    // ── IMEI ──────────────────────────────────────────────

    /// <inheritdoc />
    public bool ImeiExists(string imei1)
        => _phoneRepo.Any(phone => phone.IMEI1 == imei1);

    /// <inheritdoc />
    public Task<bool> ImeiExistsAsync(string imei1)
        => _phoneRepo.AnyAsync(phone => phone.IMEI1 == imei1);

    // ── Status flags ──────────────────────────────────────

    /// <inheritdoc />
    public bool IsSold(int id)
        => _phoneRepo.Any(phone => phone.Id == id &&
            phone.ProductNavigation.Transactions.Any(transaction => transaction.Direction == TransactionDirection.Sell));

    /// <inheritdoc />
    public Task<bool> IsSoldAsync(int id)
        => _phoneRepo.AnyAsync(phone => phone.Id == id &&
            phone.ProductNavigation.Transactions.Any(transaction => transaction.Direction == TransactionDirection.Sell));

    /// <inheritdoc />
    public bool IsSecondHand(int id)
        => _phoneRepo.Any(phone => phone.Id == id && phone.ProductNavigation.SecondHandProfile != null);

    /// <inheritdoc />
    public Task<bool> IsSecondHandAsync(int id)
        => _phoneRepo.AnyAsync(phone => phone.Id == id && phone.ProductNavigation.SecondHandProfile != null);

    // ── Owner (null ⇒ not sold) ───────────────────────────

    /// <inheritdoc />
    public Customer? GetOwner(int id)
        => _phoneRepo.Select(
            phone => phone.Id == id,
            phone => phone.ProductNavigation.Transactions
                .Where(transaction => transaction.Direction == TransactionDirection.Sell)
                .OrderByDescending(transaction => transaction.Date)
                .Select(transaction => transaction.CustomerNavigation)
                .FirstOrDefault());

    /// <inheritdoc />
    public Task<Customer?> GetOwnerAsync(int id)
        => _phoneRepo.SelectAsync(
            phone => phone.Id == id,
            phone => phone.ProductNavigation.Transactions
                .Where(transaction => transaction.Direction == TransactionDirection.Sell)
                .OrderByDescending(transaction => transaction.Date)
                .Select(transaction => transaction.CustomerNavigation)
                .FirstOrDefault());

    // ── Guarantee ─────────────────────────────────────────

    /// <inheritdoc />
    public Guarantee? GetGuarantee(int id)
        => _phoneRepo.Select(phone => phone.Id == id, phone => phone.ProductNavigation.GuaranteeProfile);

    /// <inheritdoc />
    public Task<Guarantee?> GetGuaranteeAsync(int id)
        => _phoneRepo.SelectAsync(phone => phone.Id == id, phone => phone.ProductNavigation.GuaranteeProfile);

    // ── Second-hand (null ⇒ not second-hand) ──────────────

    /// <inheritdoc />
    public SecondHand? GetSecondHandInfo(int id)
        => _phoneRepo.Select(phone => phone.Id == id, phone => phone.ProductNavigation.SecondHandProfile);

    /// <inheritdoc />
    public Task<SecondHand?> GetSecondHandInfoAsync(int id)
        => _phoneRepo.SelectAsync(phone => phone.Id == id, phone => phone.ProductNavigation.SecondHandProfile);

    // ── Quantities ────────────────────────────────────────

    /// <inheritdoc />
    public int Quantity()
        => _phoneRepo.Count();

    /// <inheritdoc />
    public int SecondHandQuantity()
        => _phoneRepo.Count(phone => phone.ProductNavigation.SecondHandProfile != null);

    /// <inheritdoc />
    public int AvailableSecondHandQuantity()
        => _phoneRepo.Count(phone =>
            phone.ProductNavigation.SecondHandProfile != null &&
            !phone.ProductNavigation.Transactions.Any(transaction => transaction.Direction == TransactionDirection.Sell));

    /// <inheritdoc />
    public Task<int> QuantityAsync()
        => _phoneRepo.CountAsync();

    /// <inheritdoc />
    public Task<int> SecondHandQuantityAsync()
        => _phoneRepo.CountAsync(phone => phone.ProductNavigation.SecondHandProfile != null);

    /// <inheritdoc />
    public Task<int> AvailableSecondHandQuantityAsync()
        => _phoneRepo.CountAsync(phone =>
            phone.ProductNavigation.SecondHandProfile != null &&
            !phone.ProductNavigation.Transactions.Any(transaction => transaction.Direction == TransactionDirection.Sell));

    // ── Lists ─────────────────────────────────────────────

    /// <inheritdoc />
    public IEnumerable<Phone> GetSecondHand()
        => _phoneRepo.FindAll(phone => phone.ProductNavigation.SecondHandProfile != null);

    /// <inheritdoc />
    public IEnumerable<Phone> GetAvailableSecondHand()
        => _phoneRepo.FindAll(phone =>
            phone.ProductNavigation.SecondHandProfile != null &&
            !phone.ProductNavigation.Transactions.Any(transaction => transaction.Direction == TransactionDirection.Sell));

    /// <inheritdoc />
    public Task<IEnumerable<Phone>> GetSecondHandAsync()
        => _phoneRepo.FindAllAsync(phone => phone.ProductNavigation.SecondHandProfile != null);

    /// <inheritdoc />
    public Task<IEnumerable<Phone>> GetAvailableSecondHandAsync()
        => _phoneRepo.FindAllAsync(phone =>
            phone.ProductNavigation.SecondHandProfile != null &&
            !phone.ProductNavigation.Transactions.Any(transaction => transaction.Direction == TransactionDirection.Sell));
}