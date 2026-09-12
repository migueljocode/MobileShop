namespace MobileShop.Services.DataServices.Dal;

public class PhoneDataService(
    IPhoneRepo phoneRepo,
    ILogger<PhoneDataService> logger)
    : DataServiceBase<PhoneDataService, Phone>(phoneRepo, logger), IPhoneDataService
{
    private readonly IPhoneRepo _phoneRepo = phoneRepo;

    // ── IMEI ──────────────────────────────────────────────

    public bool ImeiExists(string imei1)
        => _phoneRepo.ImeiExists(imei1);

    public Task<bool> ImeiExistsAsync(string imei1)
        => _phoneRepo.ImeiExistsAsync(imei1);

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
        => _phoneRepo.GetAll(p => p.ProductNavigation.SecondHandProfile != null).Count();

    public int AvailableSecondHandQuantity()
        => _phoneRepo.GetAll(p =>
                p.ProductNavigation.SecondHandProfile != null &&
                !p.ProductNavigation.Transactions.Any(t => t.Direction == TransactionDirection.Sell))
            .Count();

    public Task<int> QuantityAsync()
        => _phoneRepo.QuantityAsync();

    public async Task<int> SecondHandQuantityAsync()
        => (await _phoneRepo.GetAllAsync(p => p.ProductNavigation.SecondHandProfile != null)).Count();

    public async Task<int> AvailableSecondHandQuantityAsync()
        => (await _phoneRepo.GetAllAsync(p =>
                p.ProductNavigation.SecondHandProfile != null &&
                !p.ProductNavigation.Transactions.Any(t => t.Direction == TransactionDirection.Sell)))
            .Count();

    // ── Lists ─────────────────────────────────────────────

    public IEnumerable<Phone> GetSecondHand()
        => _phoneRepo.GetAll(p => p.ProductNavigation.SecondHandProfile != null);

    public IEnumerable<Phone> GetAvailableSecondHand()
        => _phoneRepo.GetAll(p =>
            p.ProductNavigation.SecondHandProfile != null &&
            !p.ProductNavigation.Transactions.Any(t => t.Direction == TransactionDirection.Sell));

    public Task<IEnumerable<Phone>> GetSecondHandAsync()
        => _phoneRepo.GetAllAsync(p => p.ProductNavigation.SecondHandProfile != null);

    public Task<IEnumerable<Phone>> GetAvailableSecondHandAsync()
        => _phoneRepo.GetAllAsync(p =>
            p.ProductNavigation.SecondHandProfile != null &&
            !p.ProductNavigation.Transactions.Any(t => t.Direction == TransactionDirection.Sell));
}