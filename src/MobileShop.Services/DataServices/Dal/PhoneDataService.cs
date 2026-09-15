namespace MobileShop.Services.DataServices.Dal;

public class PhoneDataService(
    IPhoneRepo phoneRepo,
    ILogger<PhoneDataService> logger)
    : DataServiceBase<PhoneDataService, Phone>(phoneRepo, logger), IPhoneDataService
{
    private readonly IPhoneRepo _phoneRepo = phoneRepo;

    public IReadOnlyList<ProductListItemViewModel> GetInventoryRows()
        => GetAll()
            .OrderBy(phone => phone.ProductId)
            .Select(ToInventoryRow)
            .ToList();

    public IReadOnlyList<ProductListItemViewModel> GetSelectableProducts(TransactionDirection direction)
        => GetAll()
            .Where(phone => phone.ProductNavigation.Transactions.All(transaction => transaction.Direction != direction))
            .OrderBy(phone => phone.ProductId)
            .Select(ToInventoryRow)
            .ToList();

    public IReadOnlyList<ProductListItemViewModel> GetSecondHandRows()
        => GetAll()
            .Where(phone => phone.ProductNavigation.SecondHandProfile is not null)
            .OrderBy(phone => phone.ProductId)
            .Select(ToInventoryRow)
            .ToList();

    public IReadOnlyList<ProductListItemViewModel> GetAvailableSecondHandRows()
        => GetAll()
            .Where(phone => phone.ProductNavigation.SecondHandProfile is not null &&
                           !phone.ProductNavigation.Transactions.Any(transaction => transaction.Direction == TransactionDirection.Sell))
            .OrderBy(phone => phone.ProductId)
            .Select(ToInventoryRow)
            .ToList();

    private static ProductListItemViewModel ToInventoryRow(Phone phone)
        => new(
            phone.Id,
            phone.ProductId,
            "Phone",
            $"{phone.ProductNavigation.Manufacturer} {phone.ProductNavigation.Model}",
            $"IMEI: {phone.IMEI1}",
            phone.Color,
            phone.ProductNavigation.Transactions.Any(t => t.Direction == TransactionDirection.Sell),
            phone.ProductNavigation.SecondHandProfile is not null);

    public ProductDetailsViewModel? GetDetails(int id)
    {
        var phone = Find(id);
        if (phone is null)
            return null;

        var owner = GetOwner(id);
        var guarantee = GetGuarantee(id);
        return new ProductDetailsViewModel(
            "Phone",
            phone.ProductId,
            phone.ProductNavigation?.Manufacturer ?? "Phone",
            phone.ProductNavigation?.Model ?? $"Phone #{id}",
            $"IMEI: {phone.IMEI1}" + (phone.IMEI2 is null ? string.Empty : $" / {phone.IMEI2}"),
            phone.Color,
            owner?.PersonNavigation is { } person ? $"{person.FirstName} {person.LastName}" : "Not sold",
            guarantee is null ? "None" : $"{guarantee.Corporation} until {guarantee.ExpirationDate:d}",
            GetSecondHandInfo(id) is not null);
    }

    // ── IMEI ──────────────────────────────────────────────

    public bool ImeiExists(string imei1)
        => _phoneRepo.ImeiExists(imei1);

    public Task<bool> ImeiExistsAsync(string imei1)
        => _phoneRepo.ImeiExistsAsync(imei1);

    // ── Status flags ──────────────────────────────────────

    public bool IsSold(int id)
        => _phoneRepo.IsSold(id);

    public Task<bool> IsSoldAsync(int id)
        => _phoneRepo.IsSoldAsync(id);

    public bool IsSecondHand(int id)
        => _phoneRepo.IsSecondHand(id);

    public Task<bool> IsSecondHandAsync(int id)
        => _phoneRepo.IsSecondHandAsync(id);

    // ── Owner (null ⇒ not sold) ───────────────────────────

    public Customer? GetOwner(int id)
        => _phoneRepo.GetOwner(id);

    public Customer? GetOwner(Expression<Func<Phone, bool>> predicate)
    {
        var phone = _phoneRepo.Find(predicate);
        return phone is null ? null : _phoneRepo.GetOwner(phone.Id);
    }

    public Task<Customer?> GetOwnerAsync(int id)
        => _phoneRepo.GetOwnerAsync(id);

    public async Task<Customer?> GetOwnerAsync(Expression<Func<Phone, bool>> predicate)
    {
        var phone = await _phoneRepo.FindAsync(predicate);
        return phone is null ? null : await _phoneRepo.GetOwnerAsync(phone.Id);
    }

    // ── Guarantee ─────────────────────────────────────────

    public Guarantee? GetGuarantee(int id)
        => _phoneRepo.GetGuarantee(id);

    public Guarantee? GetGuarantee(Expression<Func<Phone, bool>> predicate)
    {
        var phone = _phoneRepo.Find(predicate);
        return phone is null ? null : _phoneRepo.GetGuarantee(phone.Id);
    }

    public Task<Guarantee?> GetGuaranteeAsync(int id)
        => _phoneRepo.GetGuaranteeAsync(id);

    public async Task<Guarantee?> GetGuaranteeAsync(Expression<Func<Phone, bool>> predicate)
    {
        var phone = await _phoneRepo.FindAsync(predicate);
        return phone is null ? null : await _phoneRepo.GetGuaranteeAsync(phone.Id);
    }

    // ── Second-hand (null ⇒ not second-hand) ──────────────

    public SecondHand? GetSecondHandInfo(int id)
        => _phoneRepo.GetSecondHandInfo(id);

    public SecondHand? GetSecondHandInfo(Expression<Func<Phone, bool>> predicate)
    {
        var phone = _phoneRepo.Find(predicate);
        return phone is null ? null : _phoneRepo.GetSecondHandInfo(phone.Id);
    }

    public Task<SecondHand?> GetSecondHandInfoAsync(int id)
        => _phoneRepo.GetSecondHandInfoAsync(id);

    public async Task<SecondHand?> GetSecondHandInfoAsync(Expression<Func<Phone, bool>> predicate)
    {
        var phone = await _phoneRepo.FindAsync(predicate);
        return phone is null ? null : await _phoneRepo.GetSecondHandInfoAsync(phone.Id);
    }

    // ── Quantities ────────────────────────────────────────

    public int Quantity()
        => _phoneRepo.Quantity();

    public int SecondHandQuantity()
        => _phoneRepo.FindAll(p => p.ProductNavigation.SecondHandProfile != null).Count();

    public int AvailableSecondHandQuantity()
        => _phoneRepo.FindAll(p =>
                p.ProductNavigation.SecondHandProfile != null &&
                !p.ProductNavigation.Transactions.Any(t => t.Direction == TransactionDirection.Sell))
            .Count();

    public Task<int> QuantityAsync()
        => _phoneRepo.QuantityAsync();

    public async Task<int> SecondHandQuantityAsync()
        => (await _phoneRepo.FindAllAsync(p => p.ProductNavigation.SecondHandProfile != null)).Count();

    public async Task<int> AvailableSecondHandQuantityAsync()
        => (await _phoneRepo.FindAllAsync(p =>
                p.ProductNavigation.SecondHandProfile != null &&
                !p.ProductNavigation.Transactions.Any(t => t.Direction == TransactionDirection.Sell)))
            .Count();

    // ── Lists ─────────────────────────────────────────────

    public IEnumerable<Phone> GetSecondHand()
        => _phoneRepo.FindAll(p => p.ProductNavigation.SecondHandProfile != null);

    public IEnumerable<Phone> GetAvailableSecondHand()
        => _phoneRepo.FindAll(p =>
            p.ProductNavigation.SecondHandProfile != null &&
            !p.ProductNavigation.Transactions.Any(t => t.Direction == TransactionDirection.Sell));

    public Task<IEnumerable<Phone>> GetSecondHandAsync()
        => _phoneRepo.FindAllAsync(p => p.ProductNavigation.SecondHandProfile != null);

    public Task<IEnumerable<Phone>> GetAvailableSecondHandAsync()
        => _phoneRepo.FindAllAsync(p =>
            p.ProductNavigation.SecondHandProfile != null &&
            !p.ProductNavigation.Transactions.Any(t => t.Direction == TransactionDirection.Sell));
}