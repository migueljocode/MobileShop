namespace MobileShop.Dal.Repos;

/// <inheritdoc cref="ITransactionRepo" />
public class TransactionRepo(AppDbContext context) : BaseRepo<Transaction>(context), ITransactionRepo
{
    public override IEnumerable<Transaction> GetAll(Expression<Func<Transaction, bool>>? predicate = null)
    {
        IQueryable<Transaction> query = Table
            .Include(t => t.ProductNavigation)
            .Include(t => t.SellerNavigation).ThenInclude(s => s.PersonNavigation)
            .Include(t => t.CustomerNavigation).ThenInclude(c => c.PersonNavigation);
        return (predicate is null ? query : query.Where(predicate)).ToList();
    }

    public override async Task<IEnumerable<Transaction>> GetAllAsync(
        Expression<Func<Transaction, bool>>? predicate = null)
    {
        IQueryable<Transaction> query = Table
            .Include(t => t.ProductNavigation)
            .Include(t => t.SellerNavigation).ThenInclude(s => s.PersonNavigation)
            .Include(t => t.CustomerNavigation).ThenInclude(c => c.PersonNavigation);
        return await (predicate is null ? query : query.Where(predicate)).ToListAsync();
    }

    /// <inheritdoc />
    public IEnumerable<Transaction> GetByProduct(int productId)
        => GetAll(t => t.ProductId == productId);

    /// <inheritdoc />
    public async Task<IEnumerable<Transaction>> GetByProductAsync(int productId)
        => await GetAllAsync(t => t.ProductId == productId);
}
