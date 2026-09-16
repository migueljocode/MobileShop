namespace MobileShop.Dal.Repos;

/// <inheritdoc cref="ITransactionRepo" />
public class TransactionRepo(AppDbContext context) : BaseRepo<Transaction>(context), ITransactionRepo
{
    /// <inheritdoc />
    /// <remarks>Includes the product and related seller, customer, and person data.</remarks>
    public override IEnumerable<Transaction> FindAll(Expression<Func<Transaction, bool>>? predicate = null)
    {
        IQueryable<Transaction> query = Table
            .Include(t => t.ProductNavigation)
            .Include(t => t.SellerNavigation).ThenInclude(s => s.PersonNavigation)
            .Include(t => t.CustomerNavigation).ThenInclude(c => c.PersonNavigation);
        return (predicate is null ? query : query.Where(predicate)).ToList();
    }

    /// <inheritdoc />
    /// <remarks>Includes the product and related seller, customer, and person data.</remarks>
    public override async Task<IEnumerable<Transaction>> FindAllAsync(
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
        => FindAll(t => t.ProductId == productId);

    /// <inheritdoc />
    public async Task<IEnumerable<Transaction>> GetByProductAsync(int productId)
        => await FindAllAsync(t => t.ProductId == productId);
}
