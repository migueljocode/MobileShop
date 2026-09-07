namespace MobileShop.Dal.Repos.Interfaces;

/// <summary>Repository for <see cref="Product"/> entities.</summary>
public interface IProductRepo : IBaseRepo<Product>
{
    /// <summary>
    /// Checks whether a product is currently in stock. A product is considered sold once it has a
    /// <see cref="TransactionDirection.Sell"/> transaction against it - there is no separate status
    /// field to keep in sync.
    /// </summary>
    /// <param name="productId">The Id of the product to check.</param>
    /// <returns><see langword="true"/> if the product has not been sold yet.</returns>
    bool IsInStock(int productId);

    /// <summary>Asynchronous version of <see cref="IsInStock"/>.</summary>
    /// <param name="productId">The Id of the product to check.</param>
    /// <returns><see langword="true"/> if the product has not been sold yet.</returns>
    Task<bool> IsInStockAsync(int productId);
}
