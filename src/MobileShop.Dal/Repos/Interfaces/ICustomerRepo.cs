namespace MobileShop.Dal.Repos.Interfaces;

/// <summary>Repository for <see cref="Customer"/> entities.</summary>
public interface ICustomerRepo : IBaseRepo<Customer>
{
    /// <summary>Gets every product this customer has bought (Sell-direction transactions where they're the customer).</summary>
    /// <param name="customerId">The Id of the customer.</param>
    /// <returns>The distinct products purchased, or <see langword="null"/> if the customer doesn't exist.</returns>
    IEnumerable<Product>? PurchasedProducts(int customerId);

    /// <summary>Asynchronous version of <see cref="PurchasedProducts"/>.</summary>
    /// <param name="customerId">The Id of the customer.</param>
    /// <returns>The distinct products purchased, or <see langword="null"/> if the customer doesn't exist.</returns>
    Task<IEnumerable<Product>?> PurchasedProductsAsync(int customerId);
}