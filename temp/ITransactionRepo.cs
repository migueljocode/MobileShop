namespace MobileShop.Dal.Repos.Interfaces;

/// <summary>Repository for <see cref="Transaction"/> entities.</summary>
public interface ITransactionRepo : IBaseRepo<Transaction>
{
    /// <summary>
    /// Gets every transaction recorded against a product - typically its Buy leg and, if sold,
    /// its Sell leg - useful for profit/loss reporting.
    /// </summary>
    /// <param name="productId">The Id of the product.</param>
    /// <returns>The transactions involving that product.</returns>
    IEnumerable<Transaction> GetByProduct(int productId);

    /// <summary>Asynchronous version of <see cref="GetByProduct"/>.</summary>
    /// <param name="productId">The Id of the product.</param>
    /// <returns>The transactions involving that product.</returns>
    Task<IEnumerable<Transaction>> GetByProductAsync(int productId);
}
