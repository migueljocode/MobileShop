namespace MobileShop.Services.DataServices.Dal;

public class AppleIdDataService(
    IAppleIdRepo appleIdRepo,
    ILogger<AppleIdDataService> logger)
    : DataServiceBase<AppleIdDataService, AppleId>(appleIdRepo, logger), IAppleIdDataService
{
    private readonly IAppleIdRepo _appleIdRepo = appleIdRepo;

    // ── Email ─────────────────────────────────────────────

    public AppleId? FindByEmail(string email)
        => _appleIdRepo.Find(email);

    public Task<AppleId?> FindByEmailAsync(string email)
        => _appleIdRepo.FindAsync(email);

    // ── Owner (null ⇒ not sold) ───────────────────────────

    public Customer? GetOwner(int id)
        => _appleIdRepo.GetOwner(id);

    public Customer? GetOwner(Expression<Func<AppleId, bool>> predicate)
    {
        var appleId = _appleIdRepo.Find(predicate);
        return appleId is null ? null : _appleIdRepo.GetOwner(appleId.Id);
    }

    public Task<Customer?> GetOwnerAsync(int id)
        => _appleIdRepo.GetOwnerAsync(id);

    public async Task<Customer?> GetOwnerAsync(Expression<Func<AppleId, bool>> predicate)
    {
        var appleId = await _appleIdRepo.FindAsync(predicate);
        return appleId is null ? null : await _appleIdRepo.GetOwnerAsync(appleId.Id);
    }

    // ── Guarantee ─────────────────────────────────────────

    public Guarantee? GetGuarantee(int id)
        => _appleIdRepo.GetGuarantee(id);

    public Guarantee? GetGuarantee(Expression<Func<AppleId, bool>> predicate)
    {
        var appleId = _appleIdRepo.Find(predicate);
        return appleId is null ? null : _appleIdRepo.GetGuarantee(appleId.Id);
    }

    public Task<Guarantee?> GetGuaranteeAsync(int id)
        => _appleIdRepo.GetGuaranteeAsync(id);

    public async Task<Guarantee?> GetGuaranteeAsync(Expression<Func<AppleId, bool>> predicate)
    {
        var appleId = await _appleIdRepo.FindAsync(predicate);
        return appleId is null ? null : await _appleIdRepo.GetGuaranteeAsync(appleId.Id);
    }

    // ── Second-hand (null ⇒ not second-hand) ──────────────

    public SecondHand? GetSecondHandInfo(int id)
        => _appleIdRepo.GetSecondHandInfo(id);

    public SecondHand? GetSecondHandInfo(Expression<Func<AppleId, bool>> predicate)
    {
        var appleId = _appleIdRepo.Find(predicate);
        return appleId is null ? null : _appleIdRepo.GetSecondHandInfo(appleId.Id);
    }

    public Task<SecondHand?> GetSecondHandInfoAsync(int id)
        => _appleIdRepo.GetSecondHandInfoAsync(id);

    public async Task<SecondHand?> GetSecondHandInfoAsync(Expression<Func<AppleId, bool>> predicate)
    {
        var appleId = await _appleIdRepo.FindAsync(predicate);
        return appleId is null ? null : await _appleIdRepo.GetSecondHandInfoAsync(appleId.Id);
    }

    // ── Quantities ────────────────────────────────────────

    public int Quantity()
        => _appleIdRepo.Quantity();

    public int SecondHandQuantity()
        => _appleIdRepo.GetAll(a => a.ProductNavigation.SecondHandProfile != null).Count();

    public int AvailableSecondHandQuantity()
        => _appleIdRepo.GetAll(a =>
                a.ProductNavigation.SecondHandProfile != null &&
                !a.ProductNavigation.Transactions.Any(t => t.Direction == TransactionDirection.Sell))
            .Count();

    public Task<int> QuantityAsync()
        => _appleIdRepo.QuantityAsync();

    public async Task<int> SecondHandQuantityAsync()
        => (await _appleIdRepo.GetAllAsync(a => a.ProductNavigation.SecondHandProfile != null)).Count();

    public async Task<int> AvailableSecondHandQuantityAsync()
        => (await _appleIdRepo.GetAllAsync(a =>
                a.ProductNavigation.SecondHandProfile != null &&
                !a.ProductNavigation.Transactions.Any(t => t.Direction == TransactionDirection.Sell)))
            .Count();

    // ── Lists ─────────────────────────────────────────────

    public IEnumerable<AppleId> GetSecondHand()
        => _appleIdRepo.GetAll(a => a.ProductNavigation.SecondHandProfile != null);

    public IEnumerable<AppleId> GetAvailableSecondHand()
        => _appleIdRepo.GetAll(a =>
            a.ProductNavigation.SecondHandProfile != null &&
            !a.ProductNavigation.Transactions.Any(t => t.Direction == TransactionDirection.Sell));

    public Task<IEnumerable<AppleId>> GetSecondHandAsync()
        => _appleIdRepo.GetAllAsync(a => a.ProductNavigation.SecondHandProfile != null);

    public Task<IEnumerable<AppleId>> GetAvailableSecondHandAsync()
        => _appleIdRepo.GetAllAsync(a =>
            a.ProductNavigation.SecondHandProfile != null &&
            !a.ProductNavigation.Transactions.Any(t => t.Direction == TransactionDirection.Sell));
}