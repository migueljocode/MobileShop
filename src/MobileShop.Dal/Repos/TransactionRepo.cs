namespace MobileShop.Dal.Repos;

/// <inheritdoc cref="ITransactionRepo" />
public class TransactionRepo(AppDbContext context) : BaseRepo<Transaction>(context), ITransactionRepo
{
    /// <inheritdoc />
    public IEnumerable<Transaction> GetByProduct(int productId)
        => FindAll(t => t.ProductId == productId);

    /// <inheritdoc />
    public async Task<IEnumerable<Transaction>> GetByProductAsync(int productId)
        => await FindAllAsync(t => t.ProductId == productId);
}
