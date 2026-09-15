namespace MobileShop.Services.DataServices.Interfaces;

/// <summary>
/// Defines the public contract for ICustomerDataService.
/// </summary>
public interface ICustomerDataService : IDataService<Customer>
{
    /// <summary>Gets customer options for transaction selectors.</summary>
    IReadOnlyList<PartyOptionViewModel> GetPartyOptions();
    /// <summary>Gets products purchased by a customer.</summary>
    /// <param name="customerId">The customer identifier.</param>
    IEnumerable<Product> PurchasedProducts(int customerId);
    /// <summary>Gets products purchased by a customer asynchronously.</summary>
    /// <param name="customerId">The customer identifier.</param>
    Task<IEnumerable<Product>> PurchasedProductsAsync(int customerId);
}
