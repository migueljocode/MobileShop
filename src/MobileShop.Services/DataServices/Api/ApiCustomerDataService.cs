namespace MobileShop.Services.DataServices.Api;

public class ApiCustomerDataService : ICustomerDataService
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

    /// <inheritdoc />
    public IEnumerable<Customer> FindAll(Expression<Func<Customer, bool>>? predicate = null)
        => throw new NotImplementedException("ApiCustomerDataService is not implemented yet.");

    /// <inheritdoc />
    public Task<IEnumerable<Customer>> FindAllAsync(Expression<Func<Customer, bool>>? predicate = null)
        => throw new NotImplementedException("ApiCustomerDataService is not implemented yet.");

    /// <inheritdoc />
    public Customer? Find(int id)
        => throw new NotImplementedException("ApiCustomerDataService is not implemented yet.");

    /// <inheritdoc />
    public Customer? Find(Expression<Func<Customer, bool>> predicate)
        => throw new NotImplementedException("ApiCustomerDataService is not implemented yet.");

    /// <inheritdoc />
    public Task<Customer?> FindAsync(int id)
        => throw new NotImplementedException("ApiCustomerDataService is not implemented yet.");

    /// <inheritdoc />
    public Task<Customer?> FindAsync(Expression<Func<Customer, bool>> predicate)
        => throw new NotImplementedException("ApiCustomerDataService is not implemented yet.");

    /// <inheritdoc />
    public bool Add(Customer entity)
        => throw new NotImplementedException("ApiCustomerDataService is not implemented yet.");

    /// <inheritdoc />
    public bool Update(Customer entity)
        => throw new NotImplementedException("ApiCustomerDataService is not implemented yet.");

    /// <inheritdoc />
    public bool Delete(int id)
        => throw new NotImplementedException("ApiCustomerDataService is not implemented yet.");

    /// <inheritdoc />
    public bool Delete(Customer entity)
        => throw new NotImplementedException("ApiCustomerDataService is not implemented yet.");

    /// <inheritdoc />
    public Task<bool> AddAsync(Customer entity)
        => throw new NotImplementedException("ApiCustomerDataService is not implemented yet.");

    /// <inheritdoc />
    public Task<bool> UpdateAsync(Customer entity)
        => throw new NotImplementedException("ApiCustomerDataService is not implemented yet.");

    /// <inheritdoc />
    public Task<bool> DeleteAsync(int id)
        => throw new NotImplementedException("ApiCustomerDataService is not implemented yet.");

    /// <inheritdoc />
    public Task<bool> DeleteAsync(Customer entity)
        => throw new NotImplementedException("ApiCustomerDataService is not implemented yet.");
}
