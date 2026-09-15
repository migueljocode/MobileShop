namespace MobileShop.Dal.Repos;

/// <inheritdoc cref="IPhoneRepo" />
public class PhoneRepo(AppDbContext context) : BaseRepo<Phone>(context), IPhoneRepo
{
    /// <inheritdoc />
    /// <remarks>Includes the product transactions and second-hand profile.</remarks>
    public override Phone? Find(int id)
        => Table.Include(p => p.ProductNavigation)
                .ThenInclude(p => p.Transactions)
            .Include(p => p.ProductNavigation)
                .ThenInclude(p => p.SecondHandProfile)
            .FirstOrDefault(p => p.Id == id);

    /// <inheritdoc />
    /// <remarks>Includes the product transactions and second-hand profile.</remarks>
    public override async Task<Phone?> FindAsync(int id)
        => await Table.Include(p => p.ProductNavigation)
                .ThenInclude(p => p.Transactions)
            .Include(p => p.ProductNavigation)
                .ThenInclude(p => p.SecondHandProfile)
            .FirstOrDefaultAsync(p => p.Id == id);

    /// <inheritdoc />
    /// <remarks>Includes the product transactions and second-hand profile.</remarks>
    public override IEnumerable<Phone> FindAll(Expression<Func<Phone, bool>>? predicate = null)
    {
        IQueryable<Phone> query = Table
            .Include(p => p.ProductNavigation).ThenInclude(p => p.Transactions)
            .Include(p => p.ProductNavigation).ThenInclude(p => p.SecondHandProfile);
        return (predicate is null ? query : query.Where(predicate)).ToList();
    }

    /// <inheritdoc />
    /// <remarks>Includes the product transactions and second-hand profile.</remarks>
    public override async Task<IEnumerable<Phone>> FindAllAsync(Expression<Func<Phone, bool>>? predicate = null)
    {
        IQueryable<Phone> query = Table
            .Include(p => p.ProductNavigation).ThenInclude(p => p.Transactions)
            .Include(p => p.ProductNavigation).ThenInclude(p => p.SecondHandProfile);
        return await (predicate is null ? query : query.Where(predicate)).ToListAsync();
    }

    /// <inheritdoc />
    public override IEnumerable<Phone> GetAll(Expression<Func<Phone, bool>>? predicate = null) => FindAll(predicate);

    /// <inheritdoc />
    public override Task<IEnumerable<Phone>> GetAllAsync(Expression<Func<Phone, bool>>? predicate = null) => FindAllAsync(predicate);
}
