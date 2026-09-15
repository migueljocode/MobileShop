namespace MobileShop.Services.DataServices.Interfaces;

public interface ICustomerDataService : IDataService<Customer>
{
    IReadOnlyList<PartyOptionViewModel> GetPartyOptions();
    IEnumerable<Product> PurchasedProducts(int customerId);
    Task<IEnumerable<Product>> PurchasedProductsAsync(int customerId);
}
