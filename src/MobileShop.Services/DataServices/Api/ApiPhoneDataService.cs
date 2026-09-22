namespace MobileShop.Services.DataServices.Api;

public class ApiPhoneDataService : IPhoneDataService
{
    /// <inheritdoc />
    public IReadOnlyList<ProductListItemViewModel> GetInventoryRows()
        => throw new NotImplementedException("ApiPhoneDataService is not implemented yet.");

    /// <inheritdoc />
    public IReadOnlyList<ProductListItemViewModel> GetSelectableProducts(TransactionDirection direction)
        => throw new NotImplementedException("ApiPhoneDataService is not implemented yet.");

    /// <inheritdoc />
    public IReadOnlyList<ProductListItemViewModel> GetSecondHandRows()
        => throw new NotImplementedException("ApiPhoneDataService is not implemented yet.");

    /// <inheritdoc />
    public IReadOnlyList<ProductListItemViewModel> GetAvailableSecondHandRows()
        => throw new NotImplementedException("ApiPhoneDataService is not implemented yet.");

    /// <inheritdoc />
    public ProductDetailsViewModel? GetDetails(int id)
        => throw new NotImplementedException("ApiPhoneDataService is not implemented yet.");

    /// <inheritdoc />
    public bool ImeiExists(string imei1)
        => throw new NotImplementedException("ApiPhoneDataService is not implemented yet.");

    /// <inheritdoc />
    public Task<bool> ImeiExistsAsync(string imei1)
        => throw new NotImplementedException("ApiPhoneDataService is not implemented yet.");

    /// <inheritdoc />
    public bool IsSold(int id)
        => throw new NotImplementedException("ApiPhoneDataService is not implemented yet.");

    /// <inheritdoc />
    public Task<bool> IsSoldAsync(int id)
        => throw new NotImplementedException("ApiPhoneDataService is not implemented yet.");

    /// <inheritdoc />
    public bool IsSecondHand(int id)
        => throw new NotImplementedException("ApiPhoneDataService is not implemented yet.");

    /// <inheritdoc />
    public Task<bool> IsSecondHandAsync(int id)
        => throw new NotImplementedException("ApiPhoneDataService is not implemented yet.");

    /// <inheritdoc />
    public Customer? GetOwner(int id)
        => throw new NotImplementedException("ApiPhoneDataService is not implemented yet.");

    /// <inheritdoc />
    public Task<Customer?> GetOwnerAsync(int id)
        => throw new NotImplementedException("ApiPhoneDataService is not implemented yet.");

    /// <inheritdoc />
    public Guarantee? GetGuarantee(int id)
        => throw new NotImplementedException("ApiPhoneDataService is not implemented yet.");

    /// <inheritdoc />
    public Task<Guarantee?> GetGuaranteeAsync(int id)
        => throw new NotImplementedException("ApiPhoneDataService is not implemented yet.");

    /// <inheritdoc />
    public SecondHand? GetSecondHandInfo(int id)
        => throw new NotImplementedException("ApiPhoneDataService is not implemented yet.");

    /// <inheritdoc />
    public Task<SecondHand?> GetSecondHandInfoAsync(int id)
        => throw new NotImplementedException("ApiPhoneDataService is not implemented yet.");

    /// <inheritdoc />
    public int Quantity()
        => throw new NotImplementedException("ApiPhoneDataService is not implemented yet.");

    /// <inheritdoc />
    public int SecondHandQuantity()
        => throw new NotImplementedException("ApiPhoneDataService is not implemented yet.");

    /// <inheritdoc />
    public int AvailableSecondHandQuantity()
        => throw new NotImplementedException("ApiPhoneDataService is not implemented yet.");

    /// <inheritdoc />
    public Task<int> QuantityAsync()
        => throw new NotImplementedException("ApiPhoneDataService is not implemented yet.");

    /// <inheritdoc />
    public Task<int> SecondHandQuantityAsync()
        => throw new NotImplementedException("ApiPhoneDataService is not implemented yet.");

    /// <inheritdoc />
    public Task<int> AvailableSecondHandQuantityAsync()
        => throw new NotImplementedException("ApiPhoneDataService is not implemented yet.");

    /// <inheritdoc />
    public IEnumerable<Phone> GetSecondHand()
        => throw new NotImplementedException("ApiPhoneDataService is not implemented yet.");

    /// <inheritdoc />
    public IEnumerable<Phone> GetAvailableSecondHand()
        => throw new NotImplementedException("ApiPhoneDataService is not implemented yet.");

    /// <inheritdoc />
    public Task<IEnumerable<Phone>> GetSecondHandAsync()
        => throw new NotImplementedException("ApiPhoneDataService is not implemented yet.");

    /// <inheritdoc />
    public Task<IEnumerable<Phone>> GetAvailableSecondHandAsync()
        => throw new NotImplementedException("ApiPhoneDataService is not implemented yet.");

    /// <inheritdoc />
    public IEnumerable<Phone> FindAll(Expression<Func<Phone, bool>>? predicate = null)
        => throw new NotImplementedException("ApiPhoneDataService is not implemented yet.");

    /// <inheritdoc />
    public Task<IEnumerable<Phone>> FindAllAsync(Expression<Func<Phone, bool>>? predicate = null)
        => throw new NotImplementedException("ApiPhoneDataService is not implemented yet.");

    /// <inheritdoc />
    public Phone? Find(int id)
        => throw new NotImplementedException("ApiPhoneDataService is not implemented yet.");

    /// <inheritdoc />
    public Phone? Find(Expression<Func<Phone, bool>> predicate)
        => throw new NotImplementedException("ApiPhoneDataService is not implemented yet.");

    /// <inheritdoc />
    public Task<Phone?> FindAsync(int id)
        => throw new NotImplementedException("ApiPhoneDataService is not implemented yet.");

    /// <inheritdoc />
    public Task<Phone?> FindAsync(Expression<Func<Phone, bool>> predicate)
        => throw new NotImplementedException("ApiPhoneDataService is not implemented yet.");

    /// <inheritdoc />
    public bool Add(Phone entity)
        => throw new NotImplementedException("ApiPhoneDataService is not implemented yet.");

    /// <inheritdoc />
    public bool Update(Phone entity)
        => throw new NotImplementedException("ApiPhoneDataService is not implemented yet.");

    /// <inheritdoc />
    public bool Delete(int id)
        => throw new NotImplementedException("ApiPhoneDataService is not implemented yet.");

    /// <inheritdoc />
    public bool Delete(Phone entity)
        => throw new NotImplementedException("ApiPhoneDataService is not implemented yet.");

    /// <inheritdoc />
    public Task<bool> AddAsync(Phone entity)
        => throw new NotImplementedException("ApiPhoneDataService is not implemented yet.");

    /// <inheritdoc />
    public Task<bool> UpdateAsync(Phone entity)
        => throw new NotImplementedException("ApiPhoneDataService is not implemented yet.");

    /// <inheritdoc />
    public Task<bool> DeleteAsync(int id)
        => throw new NotImplementedException("ApiPhoneDataService is not implemented yet.");

    /// <inheritdoc />
    public Task<bool> DeleteAsync(Phone entity)
        => throw new NotImplementedException("ApiPhoneDataService is not implemented yet.");
}
