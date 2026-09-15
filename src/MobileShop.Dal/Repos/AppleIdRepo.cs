namespace MobileShop.Dal.Repos;

/// <inheritdoc cref="IAppleIdRepo" />
public class AppleIdRepo(AppDbContext context) : BaseRepo<AppleId>(context), IAppleIdRepo
{
    public override AppleId? Find(int id)
        => Table.Include(a => a.ProductNavigation)
                .ThenInclude(p => p.Transactions)
            .Include(a => a.ProductNavigation)
                .ThenInclude(p => p.SecondHandProfile)
            .FirstOrDefault(a => a.Id == id);

    public override async Task<AppleId?> FindAsync(int id)
        => await Table.Include(a => a.ProductNavigation)
                .ThenInclude(p => p.Transactions)
            .Include(a => a.ProductNavigation)
                .ThenInclude(p => p.SecondHandProfile)
            .FirstOrDefaultAsync(a => a.Id == id);

    public override IEnumerable<AppleId> FindAll(Expression<Func<AppleId, bool>>? predicate = null)
    {
        IQueryable<AppleId> query = Table
            .Include(a => a.ProductNavigation).ThenInclude(p => p.Transactions)
            .Include(a => a.ProductNavigation).ThenInclude(p => p.SecondHandProfile);
        return (predicate is null ? query : query.Where(predicate)).ToList();
    }

    public override async Task<IEnumerable<AppleId>> FindAllAsync(Expression<Func<AppleId, bool>>? predicate = null)
    {
        IQueryable<AppleId> query = Table
            .Include(a => a.ProductNavigation).ThenInclude(p => p.Transactions)
            .Include(a => a.ProductNavigation).ThenInclude(p => p.SecondHandProfile);
        return await (predicate is null ? query : query.Where(predicate)).ToListAsync();
    }

    public override IEnumerable<AppleId> GetAll(Expression<Func<AppleId, bool>>? predicate = null) => FindAll(predicate);

    public override Task<IEnumerable<AppleId>> GetAllAsync(Expression<Func<AppleId, bool>>? predicate = null) => FindAllAsync(predicate);

    public AppleId? Find(string email)
        => Table.FirstOrDefault(x => x.Email.ToLower() == email.ToLower());

    public async Task<AppleId?> FindAsync(string email)
        => await Table.FirstOrDefaultAsync(x => x.Email.ToLower() == email.ToLower());
}
