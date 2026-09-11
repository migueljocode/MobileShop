namespace MobileShop.Dal.Repos.Interfaces;

/// <summary>Repository for <see cref="Seller"/> entities.</summary>
public interface ISellerRepo : IBaseRepo<Seller>
{
    /// <summary>
    /// Gets every product involved in a transaction where this is the seller, across both
    /// directions. For the shop's own sentinel Seller record, this includes what it has sold on
    /// to customers (Sell) as well as anything recorded under it as a supplier (Buy).
    /// </summary>
    /// <param name="sellerId">The Id of the seller.</param>
    /// <returns>The distinct products, or <see langword="null"/> if the seller doesn't exist.</returns>
    IEnumerable<Product>? SoldProducts(int sellerId);

    /// <summary>Asynchronous version of <see cref="SoldProducts"/>.</summary>
    /// <param name="sellerId">The Id of the seller.</param>
    /// <returns>The distinct products, or <see langword="null"/> if the seller doesn't exist.</returns>
    Task<IEnumerable<Product>?> SoldProductsAsync(int sellerId);

    /// <summary>Gets only the products this seller supplied to the shop (Buy-direction transactions).</summary>
    /// <param name="sellerId">The Id of the seller.</param>
    /// <returns>The distinct products, or <see langword="null"/> if the seller doesn't exist.</returns>
    IEnumerable<Product>? SoldToShop(int sellerId);

    /// <summary>Asynchronous version of <see cref="SoldToShop"/>.</summary>
    /// <param name="sellerId">The Id of the seller.</param>
    /// <returns>The distinct products, or <see langword="null"/> if the seller doesn't exist.</returns>
    Task<IEnumerable<Product>?> SoldToShopAsync(int sellerId);
}