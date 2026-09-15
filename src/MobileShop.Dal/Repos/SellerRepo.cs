namespace MobileShop.Dal.Repos;

/// <inheritdoc cref="ISellerRepo" />
public class SellerRepo(AppDbContext context) : BaseRepo<Seller>(context), ISellerRepo
{
    /// <inheritdoc />
    /// <remarks>Includes the related person.</remarks>
    public override Seller? Find(int id)
        => Table.Include(s => s.PersonNavigation)
            .FirstOrDefault(s => s.Id == id);

    /// <inheritdoc />
    /// <remarks>Includes the related person.</remarks>
    public override async Task<Seller?> FindAsync(int id)
        => await Table.Include(s => s.PersonNavigation)
            .FirstOrDefaultAsync(s => s.Id == id);

    /// <inheritdoc />
    /// <remarks>Includes the related person.</remarks>
    public override IEnumerable<Seller> FindAll(Expression<Func<Seller, bool>>? predicate = null)
    {
        IQueryable<Seller> query = Table.Include(s => s.PersonNavigation);
        return (predicate is null ? query : query.Where(predicate)).ToList();
    }

    /// <inheritdoc />
    /// <remarks>Includes the related person.</remarks>
    public override async Task<IEnumerable<Seller>> FindAllAsync(Expression<Func<Seller, bool>>? predicate = null)
    {
        IQueryable<Seller> query = Table.Include(s => s.PersonNavigation);
        return await (predicate is null ? query : query.Where(predicate)).ToListAsync();
    }

    /// <inheritdoc />
    public override IEnumerable<Seller> GetAll(Expression<Func<Seller, bool>>? predicate = null) => FindAll(predicate);

    /// <inheritdoc />
    public override Task<IEnumerable<Seller>> GetAllAsync(Expression<Func<Seller, bool>>? predicate = null) => FindAllAsync(predicate);
}
