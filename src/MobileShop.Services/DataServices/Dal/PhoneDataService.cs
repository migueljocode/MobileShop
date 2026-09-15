namespace MobileShop.Services.DataServices.Dal;

public class PhoneDataService(
    IPhoneRepo phoneRepo,
    ILogger<PhoneDataService> logger)
    : DataServiceBase<PhoneDataService, Phone>(phoneRepo, logger), IPhoneDataService
{
    private readonly IPhoneRepo _phoneRepo = phoneRepo;

    public IReadOnlyList<ProductListItemViewModel> GetInventoryRows()
        => _phoneRepo.SelectAll(phone => new ProductListItemViewModel(
                phone.Id,
                phone.ProductId,
                "Phone",
                phone.ProductNavigation.Manufacturer + " " + phone.ProductNavigation.Model,
                "IMEI: " + phone.IMEI1,
                phone.Color,
                phone.ProductNavigation.Transactions.Any(t => t.Direction == TransactionDirection.Sell),
                phone.ProductNavigation.SecondHandProfile != null))
            .OrderBy(row => row.ProductId)
            .ToList();

    public IReadOnlyList<ProductListItemViewModel> GetSelectableProducts(TransactionDirection direction)
        => _phoneRepo
            .SelectAll(
                phone => phone.ProductNavigation.Transactions.All(transaction => transaction.Direction != direction),
                phone => new ProductListItemViewModel(
                    phone.Id,
                    phone.ProductId,
                    "Phone",
                    phone.ProductNavigation.Manufacturer + " " + phone.ProductNavigation.Model,
                    "IMEI: " + phone.IMEI1,
                    phone.Color,
                    phone.ProductNavigation.Transactions.Any(t => t.Direction == TransactionDirection.Sell),
                    phone.ProductNavigation.SecondHandProfile != null))
            .OrderBy(row => row.ProductId)
            .ToList();

    public IReadOnlyList<ProductListItemViewModel> GetSecondHandRows()
        => _phoneRepo
            .SelectAll(
                phone => phone.ProductNavigation.SecondHandProfile != null,
                phone => new ProductListItemViewModel(
                    phone.Id,
                    phone.ProductId,
                    "Phone",
                    phone.ProductNavigation.Manufacturer + " " + phone.ProductNavigation.Model,
                    "IMEI: " + phone.IMEI1,
                    phone.Color,
                    phone.ProductNavigation.Transactions.Any(t => t.Direction == TransactionDirection.Sell),
                    phone.ProductNavigation.SecondHandProfile != null))
            .OrderBy(row => row.ProductId)
            .ToList();

    public IReadOnlyList<ProductListItemViewModel> GetAvailableSecondHandRows()
        => _phoneRepo
            .SelectAll(
                phone => phone.ProductNavigation.SecondHandProfile != null &&
                         !phone.ProductNavigation.Transactions.Any(transaction => transaction.Direction == TransactionDirection.Sell),
                phone => new ProductListItemViewModel(
                    phone.Id,
                    phone.ProductId,
                    "Phone",
                    phone.ProductNavigation.Manufacturer + " " + phone.ProductNavigation.Model,
                    "IMEI: " + phone.IMEI1,
                    phone.Color,
                    phone.ProductNavigation.Transactions.Any(t => t.Direction == TransactionDirection.Sell),
                    phone.ProductNavigation.SecondHandProfile != null))
            .OrderBy(row => row.ProductId)
            .ToList();

    private static ProductListItemViewModel ToInventoryRow(Phone phone)
        => new(
            phone.Id,
            phone.ProductId,
            "Phone",
            phone.ProductNavigation.Manufacturer + " " + phone.ProductNavigation.Model,
            "IMEI: " + phone.IMEI1,
            phone.Color,
            phone.ProductNavigation.Transactions.Any(t => t.Direction == TransactionDirection.Sell),
            phone.ProductNavigation.SecondHandProfile != null);

    public ProductDetailsViewModel? GetDetails(int id)
        => _phoneRepo.Select(
            id,
            phone => new ProductDetailsViewModel(
                "Phone",
                phone.ProductId,
                phone.ProductNavigation.Manufacturer,
                phone.ProductNavigation.Model,
                string.IsNullOrWhiteSpace(phone.IMEI2)
                    ? "IMEI: " + phone.IMEI1
                    : "IMEI: " + phone.IMEI1 + " / " + phone.IMEI2,
                phone.Color,
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

    public bool ImeiExists(string imei1)
        => _phoneRepo.Any(phone => phone.IMEI1 == imei1);

    public Task<bool> ImeiExistsAsync(string imei1)
        => _phoneRepo.AnyAsync(phone => phone.IMEI1 == imei1);

    // ── Status flags ──────────────────────────────────────

    public bool IsSold(int id)
        => _phoneRepo.Any(phone => phone.Id == id &&
            phone.ProductNavigation.Transactions.Any(transaction => transaction.Direction == TransactionDirection.Sell));

    public Task<bool> IsSoldAsync(int id)
        => _phoneRepo.AnyAsync(phone => phone.Id == id &&
            phone.ProductNavigation.Transactions.Any(transaction => transaction.Direction == TransactionDirection.Sell));

    public bool IsSecondHand(int id)
        => _phoneRepo.Any(phone => phone.Id == id && phone.ProductNavigation.SecondHandProfile != null);

    public Task<bool> IsSecondHandAsync(int id)
        => _phoneRepo.AnyAsync(phone => phone.Id == id && phone.ProductNavigation.SecondHandProfile != null);

    // ── Owner (null ⇒ not sold) ───────────────────────────

    public Customer? GetOwner(int id)
        => _phoneRepo.Select(
            phone => phone.Id == id,
            phone => phone.ProductNavigation.Transactions
                .Where(transaction => transaction.Direction == TransactionDirection.Sell)
                .OrderByDescending(transaction => transaction.Date)
                .Select(transaction => transaction.CustomerNavigation)
                .FirstOrDefault());

    public Customer? GetOwner(Expression<Func<Phone, bool>> predicate)
        => _phoneRepo.Select(predicate, phone => phone.ProductNavigation.Transactions
            .Where(transaction => transaction.Direction == TransactionDirection.Sell)
            .OrderByDescending(transaction => transaction.Date)
            .Select(transaction => transaction.CustomerNavigation)
            .FirstOrDefault());

    public Task<Customer?> GetOwnerAsync(int id)
        => _phoneRepo.SelectAsync(
            phone => phone.Id == id,
            phone => phone.ProductNavigation.Transactions
                .Where(transaction => transaction.Direction == TransactionDirection.Sell)
                .OrderByDescending(transaction => transaction.Date)
                .Select(transaction => transaction.CustomerNavigation)
                .FirstOrDefault());

    public async Task<Customer?> GetOwnerAsync(Expression<Func<Phone, bool>> predicate)
        => await _phoneRepo.SelectAsync(predicate, phone => phone.ProductNavigation.Transactions
            .Where(transaction => transaction.Direction == TransactionDirection.Sell)
            .OrderByDescending(transaction => transaction.Date)
            .Select(transaction => transaction.CustomerNavigation)
            .FirstOrDefault());

    // ── Guarantee ─────────────────────────────────────────

    public Guarantee? GetGuarantee(int id)
        => _phoneRepo.Select(phone => phone.Id == id, phone => phone.ProductNavigation.GuaranteeProfile);

    public Guarantee? GetGuarantee(Expression<Func<Phone, bool>> predicate)
        => _phoneRepo.Select(predicate, phone => phone.ProductNavigation.GuaranteeProfile);

    public Task<Guarantee?> GetGuaranteeAsync(int id)
        => _phoneRepo.SelectAsync(phone => phone.Id == id, phone => phone.ProductNavigation.GuaranteeProfile);

    public async Task<Guarantee?> GetGuaranteeAsync(Expression<Func<Phone, bool>> predicate)
        => await _phoneRepo.SelectAsync(predicate, phone => phone.ProductNavigation.GuaranteeProfile);

    // ── Second-hand (null ⇒ not second-hand) ──────────────

    public SecondHand? GetSecondHandInfo(int id)
        => _phoneRepo.Select(phone => phone.Id == id, phone => phone.ProductNavigation.SecondHandProfile);

    public SecondHand? GetSecondHandInfo(Expression<Func<Phone, bool>> predicate)
        => _phoneRepo.Select(predicate, phone => phone.ProductNavigation.SecondHandProfile);

    public Task<SecondHand?> GetSecondHandInfoAsync(int id)
        => _phoneRepo.SelectAsync(phone => phone.Id == id, phone => phone.ProductNavigation.SecondHandProfile);

    public async Task<SecondHand?> GetSecondHandInfoAsync(Expression<Func<Phone, bool>> predicate)
        => await _phoneRepo.SelectAsync(predicate, phone => phone.ProductNavigation.SecondHandProfile);

    // ── Quantities ────────────────────────────────────────

    public int Quantity()
        => _phoneRepo.Count();

    public int SecondHandQuantity()
        => _phoneRepo.Count(phone => phone.ProductNavigation.SecondHandProfile != null);

    public int AvailableSecondHandQuantity()
        => _phoneRepo.Count(phone =>
            phone.ProductNavigation.SecondHandProfile != null &&
            !phone.ProductNavigation.Transactions.Any(transaction => transaction.Direction == TransactionDirection.Sell));

    public Task<int> QuantityAsync()
        => _phoneRepo.CountAsync();

    public Task<int> SecondHandQuantityAsync()
        => _phoneRepo.CountAsync(phone => phone.ProductNavigation.SecondHandProfile != null);

    public Task<int> AvailableSecondHandQuantityAsync()
        => _phoneRepo.CountAsync(phone =>
            phone.ProductNavigation.SecondHandProfile != null &&
            !phone.ProductNavigation.Transactions.Any(transaction => transaction.Direction == TransactionDirection.Sell));

    // ── Lists ─────────────────────────────────────────────

    public IEnumerable<Phone> GetSecondHand()
        => _phoneRepo.FindAll(phone => phone.ProductNavigation.SecondHandProfile != null);

    public IEnumerable<Phone> GetAvailableSecondHand()
        => _phoneRepo.FindAll(phone =>
            phone.ProductNavigation.SecondHandProfile != null &&
            !phone.ProductNavigation.Transactions.Any(transaction => transaction.Direction == TransactionDirection.Sell));

    public Task<IEnumerable<Phone>> GetSecondHandAsync()
        => _phoneRepo.FindAllAsync(phone => phone.ProductNavigation.SecondHandProfile != null);

    public Task<IEnumerable<Phone>> GetAvailableSecondHandAsync()
        => _phoneRepo.FindAllAsync(phone =>
            phone.ProductNavigation.SecondHandProfile != null &&
            !phone.ProductNavigation.Transactions.Any(transaction => transaction.Direction == TransactionDirection.Sell));
}