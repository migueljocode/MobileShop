namespace MobileShop.Services.DataServices.Api;

public class ApiPhoneDataService : IPhoneDataService
{
    public IReadOnlyList<ProductListItemViewModel> GetInventoryRows()
        => throw new NotImplementedException("ApiPhoneDataService is not implemented yet.");

    public IReadOnlyList<ProductListItemViewModel> GetSelectableProducts(TransactionDirection direction)
        => throw new NotImplementedException("ApiPhoneDataService is not implemented yet.");

    public IReadOnlyList<ProductListItemViewModel> GetSecondHandRows()
        => throw new NotImplementedException("ApiPhoneDataService is not implemented yet.");

    public IReadOnlyList<ProductListItemViewModel> GetAvailableSecondHandRows()
        => throw new NotImplementedException("ApiPhoneDataService is not implemented yet.");

    public ProductDetailsViewModel? GetDetails(int id)
        => throw new NotImplementedException("ApiPhoneDataService is not implemented yet.");

    public bool ImeiExists(string imei1)
        => throw new NotImplementedException("ApiPhoneDataService is not implemented yet.");

    public Task<bool> ImeiExistsAsync(string imei1)
        => throw new NotImplementedException("ApiPhoneDataService is not implemented yet.");

    public bool IsSold(int id)
        => throw new NotImplementedException("ApiPhoneDataService is not implemented yet.");

    public Task<bool> IsSoldAsync(int id)
        => throw new NotImplementedException("ApiPhoneDataService is not implemented yet.");

    public bool IsSecondHand(int id)
        => throw new NotImplementedException("ApiPhoneDataService is not implemented yet.");

    public Task<bool> IsSecondHandAsync(int id)
        => throw new NotImplementedException("ApiPhoneDataService is not implemented yet.");

    public Customer? GetOwner(int id)
        => throw new NotImplementedException("ApiPhoneDataService is not implemented yet.");

    public Task<Customer?> GetOwnerAsync(int id)
        => throw new NotImplementedException("ApiPhoneDataService is not implemented yet.");

    public Guarantee? GetGuarantee(int id)
        => throw new NotImplementedException("ApiPhoneDataService is not implemented yet.");

    public Task<Guarantee?> GetGuaranteeAsync(int id)
        => throw new NotImplementedException("ApiPhoneDataService is not implemented yet.");

    public SecondHand? GetSecondHandInfo(int id)
        => throw new NotImplementedException("ApiPhoneDataService is not implemented yet.");

    public Task<SecondHand?> GetSecondHandInfoAsync(int id)
        => throw new NotImplementedException("ApiPhoneDataService is not implemented yet.");

    public int Quantity()
        => throw new NotImplementedException("ApiPhoneDataService is not implemented yet.");

    public int SecondHandQuantity()
        => throw new NotImplementedException("ApiPhoneDataService is not implemented yet.");

    public int AvailableSecondHandQuantity()
        => throw new NotImplementedException("ApiPhoneDataService is not implemented yet.");

    public Task<int> QuantityAsync()
        => throw new NotImplementedException("ApiPhoneDataService is not implemented yet.");

    public Task<int> SecondHandQuantityAsync()
        => throw new NotImplementedException("ApiPhoneDataService is not implemented yet.");

    public Task<int> AvailableSecondHandQuantityAsync()
        => throw new NotImplementedException("ApiPhoneDataService is not implemented yet.");

    public IEnumerable<Phone> GetSecondHand()
        => throw new NotImplementedException("ApiPhoneDataService is not implemented yet.");

    public IEnumerable<Phone> GetAvailableSecondHand()
        => throw new NotImplementedException("ApiPhoneDataService is not implemented yet.");

    public Task<IEnumerable<Phone>> GetSecondHandAsync()
        => throw new NotImplementedException("ApiPhoneDataService is not implemented yet.");

    public Task<IEnumerable<Phone>> GetAvailableSecondHandAsync()
        => throw new NotImplementedException("ApiPhoneDataService is not implemented yet.");

    public IEnumerable<Phone> FindAll(System.Linq.Expressions.Expression<Func<Phone, bool>>? predicate = null)
        => throw new NotImplementedException("ApiPhoneDataService is not implemented yet.");

    public Task<IEnumerable<Phone>> FindAllAsync(System.Linq.Expressions.Expression<Func<Phone, bool>>? predicate = null)
        => throw new NotImplementedException("ApiPhoneDataService is not implemented yet.");

    public Phone? Find(int id)
        => throw new NotImplementedException("ApiPhoneDataService is not implemented yet.");

    public Phone? Find(System.Linq.Expressions.Expression<Func<Phone, bool>> predicate)
        => throw new NotImplementedException("ApiPhoneDataService is not implemented yet.");

    public Task<Phone?> FindAsync(int id)
        => throw new NotImplementedException("ApiPhoneDataService is not implemented yet.");

    public Task<Phone?> FindAsync(System.Linq.Expressions.Expression<Func<Phone, bool>> predicate)
        => throw new NotImplementedException("ApiPhoneDataService is not implemented yet.");

    public bool Add(Phone entity)
        => throw new NotImplementedException("ApiPhoneDataService is not implemented yet.");

    public bool Update(Phone entity)
        => throw new NotImplementedException("ApiPhoneDataService is not implemented yet.");

    public bool Delete(int id)
        => throw new NotImplementedException("ApiPhoneDataService is not implemented yet.");

    public bool Delete(Phone entity)
        => throw new NotImplementedException("ApiPhoneDataService is not implemented yet.");

    public Task<bool> AddAsync(Phone entity)
        => throw new NotImplementedException("ApiPhoneDataService is not implemented yet.");

    public Task<bool> UpdateAsync(Phone entity)
        => throw new NotImplementedException("ApiPhoneDataService is not implemented yet.");

    public Task<bool> DeleteAsync(int id)
        => throw new NotImplementedException("ApiPhoneDataService is not implemented yet.");

    public Task<bool> DeleteAsync(Phone entity)
        => throw new NotImplementedException("ApiPhoneDataService is not implemented yet.");
}
