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

    public override IEnumerable<Phone> FindAll(Expression<Func<Phone, bool>>? predicate = null)
    {
        IQueryable<Phone> query = Table
            .Include(p => p.ProductNavigation).ThenInclude(p => p.Transactions)
            .Include(p => p.ProductNavigation).ThenInclude(p => p.SecondHandProfile);
        return (predicate is null ? query : query.Where(predicate)).ToList();
    }

    public override async Task<IEnumerable<Phone>> FindAllAsync(Expression<Func<Phone, bool>>? predicate = null)
    {
        IQueryable<Phone> query = Table
            .Include(p => p.ProductNavigation).ThenInclude(p => p.Transactions)
            .Include(p => p.ProductNavigation).ThenInclude(p => p.SecondHandProfile);
        return await (predicate is null ? query : query.Where(predicate)).ToListAsync();
    }

    public override IEnumerable<Phone> GetAll(Expression<Func<Phone, bool>>? predicate = null) => FindAll(predicate);

    public override Task<IEnumerable<Phone>> GetAllAsync(Expression<Func<Phone, bool>>? predicate = null) => FindAllAsync(predicate);
}
