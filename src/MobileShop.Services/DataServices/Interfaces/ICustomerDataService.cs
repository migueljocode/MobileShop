namespace MobileShop.Services.DataServices.Interfaces;

/// <summary>
/// Defines the public contract for ICustomerDataService.
/// </summary>
public interface ICustomerDataService : IDataService<Customer>
{
    /// <summary>Gets customer options for transaction selectors.</summary>
    IReadOnlyList<PartyOptionViewModel> GetPartyOptions();
    /// <summary>Gets customer options for transaction selectors asynchronously.</summary>
    Task<IReadOnlyList<PartyOptionViewModel>> GetPartyOptionsAsync() => throw new NotImplementedException("ApiCustomerDataService is not implemented yet.");
    /// <summary>Gets customer rows for the customers list.</summary>
    /// <param name="sortBy">Sort column: "Name", "Phone", or "Count". Invalid values fall back to "Name".</param>
    /// <param name="ascending">Whether to sort ascending; descending otherwise.</param>
    IReadOnlyList<CustomerListItemViewModel> GetListRows(string sortBy = "Name", bool ascending = true);
    /// <summary>Gets customer rows for the customers list asynchronously.</summary>
    /// <param name="sortBy">Sort column: "Name", "Phone", or "Count". Invalid values fall back to "Name".</param>
    /// <param name="ascending">Whether to sort ascending; descending otherwise.</param>
    Task<IReadOnlyList<CustomerListItemViewModel>> GetListRowsAsync(string sortBy = "Name", bool ascending = true) => throw new NotImplementedException("ApiCustomerDataService is not implemented yet.");
    /// <summary>Gets flattened customer details, or <see langword="null"/> when not found.</summary>
    /// <param name="id">The customer identifier.</param>
    CustomerDetailsViewModel? GetDetails(int id);
    /// <summary>Gets flattened customer details asynchronously, or <see langword="null"/> when not found.</summary>
    /// <param name="id">The customer identifier.</param>
    Task<CustomerDetailsViewModel?> GetDetailsAsync(int id) => throw new NotImplementedException("ApiCustomerDataService is not implemented yet.");
    /// <summary>Gets products purchased by a customer.</summary>
    /// <param name="customerId">The customer identifier.</param>
    IEnumerable<Product> PurchasedProducts(int customerId);
    /// <summary>Gets products purchased by a customer asynchronously.</summary>
    /// <param name="customerId">The customer identifier.</param>
    Task<IEnumerable<Product>> PurchasedProductsAsync(int customerId);
}
