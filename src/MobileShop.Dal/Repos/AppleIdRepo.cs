namespace MobileShop.Dal.Repos;

/// <inheritdoc cref="IAppleIdRepo" />
public class AppleIdRepo(AppDbContext context) : BaseRepo<AppleId>(context), IAppleIdRepo
{
    public override AppleId? Find(int id)
        => Table.Include(a => a.ProductNavigation)
            .FirstOrDefault(a => a.Id == id);

    public override async Task<AppleId?> FindAsync(int id)
        => await Table.Include(a => a.ProductNavigation)
            .FirstOrDefaultAsync(a => a.Id == id);

    public override IEnumerable<AppleId> GetAll(Expression<Func<AppleId, bool>>? predicate = null)
    {
        IQueryable<AppleId> query = Table.Include(a => a.ProductNavigation);
        return (predicate is null ? query : query.Where(predicate)).ToList();
    }

    public override async Task<IEnumerable<AppleId>> GetAllAsync(Expression<Func<AppleId, bool>>? predicate = null)
    {
        IQueryable<AppleId> query = Table.Include(a => a.ProductNavigation);
        return await (predicate is null ? query : query.Where(predicate)).ToListAsync();
    }

    /// <inheritdoc />
    public AppleId? Find(string email)
        => Table.FirstOrDefault(x => x.Email.ToLower() == email.ToLower());

    /// <inheritdoc />
    public async Task<AppleId?> FindAsync(string email)
        => await Table.FirstOrDefaultAsync(x => x.Email.ToLower() == email.ToLower());

    /// <inheritdoc />
    public Guarantee? GetGuarantee(int id)
        => Table
            .Include(x => x.ProductNavigation)
                .ThenInclude(x => x.GuaranteeProfile)
            .FirstOrDefault(x => x.Id == id)?
            .ProductNavigation?
            .GuaranteeProfile;

    /// <inheritdoc />
    public async Task<Guarantee?> GetGuaranteeAsync(int id)
    {
        var appleId = await Table
            .Include(x => x.ProductNavigation)
                .ThenInclude(x => x.GuaranteeProfile)
            .FirstOrDefaultAsync(x => x.Id == id);

        return appleId?
            .ProductNavigation?
            .GuaranteeProfile;
    }

    /// <inheritdoc />
    public Customer? GetOwner(int id)
    {
        var appleId =
        Table.Include(a => a.ProductNavigation)
                .ThenInclude(p => p.Transactions)
                    .ThenInclude(t => t.CustomerNavigation)
                        .ThenInclude(c => c.PersonNavigation)
            .FirstOrDefault(a => a.Id == id);

        return appleId?
                .ProductNavigation?
                .Transactions?
                .OrderByDescending(x => x.Date)
                .FirstOrDefault(x => x.Direction == TransactionDirection.Sell)?
                .CustomerNavigation;
    }

    /// <inheritdoc />
    public async Task<Customer?> GetOwnerAsync(int id)
    {
        var appleId = await Table
            .Include(a => a.ProductNavigation)
                .ThenInclude(p => p.Transactions)
                    .ThenInclude(t => t.CustomerNavigation)
            .FirstOrDefaultAsync(a => a.Id == id);

        return appleId?
            .ProductNavigation?
            .Transactions?
            .OrderByDescending(t => t.Date)
            .FirstOrDefault(t => t.Direction == TransactionDirection.Sell)?
            .CustomerNavigation;
    }

    /// <inheritdoc />
    public bool IsSecondHand(int id)
        => Table
            .Where(x => x.Id == id)
            .Select(x => x.ProductNavigation.SecondHandProfile)
            .Any(profile => profile != null);

    /// <inheritdoc />
    public async Task<bool> IsSecondHandAsync(int id)
        => await Table
            .Where(x => x.Id == id)
            .Select(x => x.ProductNavigation.SecondHandProfile)
            .AnyAsync(profile => profile != null);

    /// <inheritdoc />
    public bool IsSold(int id)
        => Table
            .Where(x => x.Id == id)
            .SelectMany(x => x.ProductNavigation.Transactions)
            .Any(t => t.Direction == TransactionDirection.Sell);

    /// <inheritdoc />
    public async Task<bool> IsSoldAsync(int id)
        => await Table
            .Where(x => x.Id == id)
            .SelectMany(x => x.ProductNavigation.Transactions)
            .AnyAsync(t => t.Direction == TransactionDirection.Sell);

    /// <inheritdoc />
    public int Quantity()
        => Table.Count(a => !a.ProductNavigation.Transactions
            .Any(t => t.Direction == TransactionDirection.Sell));

    /// <inheritdoc />
    public async Task<int> QuantityAsync()
        => await Table.CountAsync(a => !a.ProductNavigation.Transactions
            .Any(t => t.Direction == TransactionDirection.Sell));

    /// <inheritdoc />
    public SecondHand? GetSecondHandInfo(int id)
        => Table
            .Where(a => a.Id == id)
            .Select(a => a.ProductNavigation.SecondHandProfile)
            .FirstOrDefault();

    /// <inheritdoc />
    public async Task<SecondHand?> GetSecondHandInfoAsync(int id)
        => await Table
            .Where(a => a.Id == id)
            .Select(a => a.ProductNavigation.SecondHandProfile)
            .FirstOrDefaultAsync();
}