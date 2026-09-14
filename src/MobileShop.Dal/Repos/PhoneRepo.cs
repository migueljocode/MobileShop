namespace MobileShop.Dal.Repos;

/// <inheritdoc cref="IPhoneRepo" />
public class PhoneRepo(AppDbContext context) : BaseRepo<Phone>(context), IPhoneRepo
{
    public override Phone? Find(int id)
        => Table.Include(p => p.ProductNavigation)
                .ThenInclude(p => p.Transactions)
            .Include(p => p.ProductNavigation)
                .ThenInclude(p => p.SecondHandProfile)
            .FirstOrDefault(p => p.Id == id);

    public override async Task<Phone?> FindAsync(int id)
        => await Table.Include(p => p.ProductNavigation)
                .ThenInclude(p => p.Transactions)
            .Include(p => p.ProductNavigation)
                .ThenInclude(p => p.SecondHandProfile)
            .FirstOrDefaultAsync(p => p.Id == id);

    public override IEnumerable<Phone> GetAll(Expression<Func<Phone, bool>>? predicate = null)
    {
        IQueryable<Phone> query = Table
            .Include(p => p.ProductNavigation).ThenInclude(p => p.Transactions)
            .Include(p => p.ProductNavigation).ThenInclude(p => p.SecondHandProfile);
        return (predicate is null ? query : query.Where(predicate)).ToList();
    }

    public override async Task<IEnumerable<Phone>> GetAllAsync(Expression<Func<Phone, bool>>? predicate = null)
    {
        IQueryable<Phone> query = Table
            .Include(p => p.ProductNavigation).ThenInclude(p => p.Transactions)
            .Include(p => p.ProductNavigation).ThenInclude(p => p.SecondHandProfile);
        return await (predicate is null ? query : query.Where(predicate)).ToListAsync();
    }

    /// <inheritdoc />
    public Guarantee? GetGuarantee(int id)
        => Table
            .Where(p => p.Id == id)
            .Select(p => p.ProductNavigation.GuaranteeProfile)
            .FirstOrDefault();

    /// <inheritdoc />
    public async Task<Guarantee?> GetGuaranteeAsync(int id)
        => await Table
            .Where(p => p.Id == id)
            .Select(p => p.ProductNavigation.GuaranteeProfile)
            .FirstOrDefaultAsync();

    /// <inheritdoc />
    public Customer? GetOwner(int id)
        => Table
            .Include(p => p.ProductNavigation)
                .ThenInclude(p => p.Transactions)
                    .ThenInclude(t => t.CustomerNavigation)
                        .ThenInclude(c => c.PersonNavigation)
            .Where(p => p.Id == id)
            .SelectMany(p => p.ProductNavigation.Transactions)
            .Where(t => t.Direction == TransactionDirection.Sell)
            .OrderByDescending(t => t.Date)
            .Select(t => t.CustomerNavigation)
            .FirstOrDefault();

    /// <inheritdoc />
    public async Task<Customer?> GetOwnerAsync(int id)
        => await Table
            .Include(p => p.ProductNavigation)
                .ThenInclude(p => p.Transactions)
                    .ThenInclude(t => t.CustomerNavigation)
                        .ThenInclude(c => c.PersonNavigation)
            .Where(p => p.Id == id)
            .SelectMany(p => p.ProductNavigation.Transactions)
            .Where(t => t.Direction == TransactionDirection.Sell)
            .OrderByDescending(t => t.Date)
            .Select(t => t.CustomerNavigation)
            .FirstOrDefaultAsync();

    /// <inheritdoc />
    public bool ImeiExists(string imei1)
        => Table.Any(p => p.IMEI1 == imei1);

    /// <inheritdoc />
    public async Task<bool> ImeiExistsAsync(string imei1)
        => await Table.AnyAsync(p => p.IMEI1 == imei1);

    /// <inheritdoc />
    public bool IsSecondHand(int id)
        => Table
            .Where(p => p.Id == id)
            .Select(p => p.ProductNavigation.SecondHandProfile)
            .Any(profile => profile != null);

    /// <inheritdoc />
    public async Task<bool> IsSecondHandAsync(int id)
        => await Table
            .Where(p => p.Id == id)
            .Select(p => p.ProductNavigation.SecondHandProfile)
            .AnyAsync(profile => profile != null);

    /// <inheritdoc />
    public bool IsSold(int id)
        => Table
            .Where(p => p.Id == id)
            .SelectMany(p => p.ProductNavigation.Transactions)
            .Any(t => t.Direction == TransactionDirection.Sell);

    /// <inheritdoc />
    public async Task<bool> IsSoldAsync(int id)
        => await Table
            .Where(p => p.Id == id)
            .SelectMany(p => p.ProductNavigation.Transactions)
            .AnyAsync(t => t.Direction == TransactionDirection.Sell);

    /// <inheritdoc />
    public int Quantity()
        => Table.Count(p => !p.ProductNavigation.Transactions
            .Any(t => t.Direction == TransactionDirection.Sell));

    /// <inheritdoc />
    public async Task<int> QuantityAsync()
        => await Table.CountAsync(p => !p.ProductNavigation.Transactions
            .Any(t => t.Direction == TransactionDirection.Sell));

    // Implementation (same pattern as GetGuarantee)
    public SecondHand? GetSecondHandInfo(int id)
        => Table
            .Where(p => p.Id == id)
            .Select(p => p.ProductNavigation.SecondHandProfile)
            .FirstOrDefault();

    public async Task<SecondHand?> GetSecondHandInfoAsync(int id)
        => await Table
            .Where(p => p.Id == id)
            .Select(p => p.ProductNavigation.SecondHandProfile)
            .FirstOrDefaultAsync();
}