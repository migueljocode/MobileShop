namespace MobileShop.Services.DataServices.Api;

public class ApiCustomerDataService : ApiDataServiceBase<Customer>, ICustomerDataService
{
    /// <inheritdoc />
    public IReadOnlyList<PartyOptionViewModel> GetPartyOptions()
        => throw new NotImplementedException("ApiCustomerDataService is not implemented yet.");

    /// <inheritdoc />
    public IReadOnlyList<CustomerListItemViewModel> GetListRows()
        => throw new NotImplementedException("ApiCustomerDataService is not implemented yet.");

    /// <inheritdoc />
    public CustomerDetailsViewModel? GetDetails(int id)
        => throw new NotImplementedException("ApiCustomerDataService is not implemented yet.");

    /// <inheritdoc />
    public IEnumerable<Product> PurchasedProducts(int customerId)
        => throw new NotImplementedException("ApiCustomerDataService is not implemented yet.");

    /// <inheritdoc />
    public Task<IEnumerable<Product>> PurchasedProductsAsync(int customerId)
        => throw new NotImplementedException("ApiCustomerDataService is not implemented yet.");
}
