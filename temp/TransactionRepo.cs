namespace MobileShop.Dal.Repos;

/// <inheritdoc cref="ITransactionRepo" />
public class TransactionRepo(AppDbContext context) : BaseRepo<Transaction>(context), ITransactionRepo
{
    /// <inheritdoc />
    public IEnumerable<Transaction> GetByProduct(int productId)
        => Table.Where(t => t.ProductId == productId).ToList();

    /// <inheritdoc />
    public async Task<IEnumerable<Transaction>> GetByProductAsync(int productId)
        => await Table.Where(t => t.ProductId == productId).ToListAsync();
}
