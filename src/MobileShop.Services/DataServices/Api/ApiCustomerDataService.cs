namespace MobileShop.Services.DataServices.Api;

public class ApiCustomerDataService : ICustomerDataService
{
    public IReadOnlyList<PartyOptionViewModel> GetPartyOptions()
        => throw new NotImplementedException("ApiCustomerDataService is not implemented yet.");

    public IReadOnlyList<CustomerListItemViewModel> GetListRows()
        => throw new NotImplementedException("ApiCustomerDataService is not implemented yet.");

    public CustomerDetailsViewModel? GetDetails(int id)
        => throw new NotImplementedException("ApiCustomerDataService is not implemented yet.");

    public IEnumerable<Product> PurchasedProducts(int customerId)
        => throw new NotImplementedException("ApiCustomerDataService is not implemented yet.");

    public Task<IEnumerable<Product>> PurchasedProductsAsync(int customerId)
        => throw new NotImplementedException("ApiCustomerDataService is not implemented yet.");

    public IEnumerable<Customer> FindAll(System.Linq.Expressions.Expression<Func<Customer, bool>>? predicate = null)
        => throw new NotImplementedException("ApiCustomerDataService is not implemented yet.");

    public Task<IEnumerable<Customer>> FindAllAsync(System.Linq.Expressions.Expression<Func<Customer, bool>>? predicate = null)
        => throw new NotImplementedException("ApiCustomerDataService is not implemented yet.");

    public Customer? Find(int id)
        => throw new NotImplementedException("ApiCustomerDataService is not implemented yet.");

    public Customer? Find(System.Linq.Expressions.Expression<Func<Customer, bool>> predicate)
        => throw new NotImplementedException("ApiCustomerDataService is not implemented yet.");

    public Task<Customer?> FindAsync(int id)
        => throw new NotImplementedException("ApiCustomerDataService is not implemented yet.");

    public Task<Customer?> FindAsync(System.Linq.Expressions.Expression<Func<Customer, bool>> predicate)
        => throw new NotImplementedException("ApiCustomerDataService is not implemented yet.");

    public bool Add(Customer entity)
        => throw new NotImplementedException("ApiCustomerDataService is not implemented yet.");

    public bool Update(Customer entity)
        => throw new NotImplementedException("ApiCustomerDataService is not implemented yet.");

    public bool Delete(int id)
        => throw new NotImplementedException("ApiCustomerDataService is not implemented yet.");

    public bool Delete(Customer entity)
        => throw new NotImplementedException("ApiCustomerDataService is not implemented yet.");

    public Task<bool> AddAsync(Customer entity)
        => throw new NotImplementedException("ApiCustomerDataService is not implemented yet.");

    public Task<bool> UpdateAsync(Customer entity)
        => throw new NotImplementedException("ApiCustomerDataService is not implemented yet.");

    public Task<bool> DeleteAsync(int id)
        => throw new NotImplementedException("ApiCustomerDataService is not implemented yet.");

    public Task<bool> DeleteAsync(Customer entity)
        => throw new NotImplementedException("ApiCustomerDataService is not implemented yet.");
}
