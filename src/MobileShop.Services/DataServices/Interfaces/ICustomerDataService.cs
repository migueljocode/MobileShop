namespace MobileShop.Services.DataServices.Interfaces;

/// <summary>
/// Defines the public contract for ICustomerDataService.
/// </summary>
public interface ICustomerDataService : IDataService<Customer>
{
    IReadOnlyList<PartyOptionViewModel> GetPartyOptions();
    IEnumerable<Product> PurchasedProducts(int customerId);
    Task<IEnumerable<Product>> PurchasedProductsAsync(int customerId);
}
