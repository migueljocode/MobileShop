namespace MobileShop.Services.DataServices.Api;

public class ApiAppleIdDataService : IAppleIdDataService
{
    /// <inheritdoc />
    public IReadOnlyList<ProductListItemViewModel> GetInventoryRows()
        => throw new NotImplementedException("ApiAppleIdDataService is not implemented yet.");

    /// <inheritdoc />
    public IReadOnlyList<ProductListItemViewModel> GetSelectableProducts(TransactionDirection direction)
        => throw new NotImplementedException("ApiAppleIdDataService is not implemented yet.");

    /// <inheritdoc />
    public IReadOnlyList<ProductListItemViewModel> GetSecondHandRows()
        => throw new NotImplementedException("ApiAppleIdDataService is not implemented yet.");

    /// <inheritdoc />
    public IReadOnlyList<ProductListItemViewModel> GetAvailableSecondHandRows()
        => throw new NotImplementedException("ApiAppleIdDataService is not implemented yet.");

    /// <inheritdoc />
    public ProductDetailsViewModel? GetDetails(int id)
        => throw new NotImplementedException("ApiAppleIdDataService is not implemented yet.");

    /// <inheritdoc />
    public AppleId? FindByEmail(string email)
        => throw new NotImplementedException("ApiAppleIdDataService is not implemented yet.");

    /// <inheritdoc />
    public Task<AppleId?> FindByEmailAsync(string email)
        => throw new NotImplementedException("ApiAppleIdDataService is not implemented yet.");

    /// <inheritdoc />
    public bool IsSold(int id)
        => throw new NotImplementedException("ApiAppleIdDataService is not implemented yet.");

    /// <inheritdoc />
    public Task<bool> IsSoldAsync(int id)
        => throw new NotImplementedException("ApiAppleIdDataService is not implemented yet.");

    /// <inheritdoc />
    public Customer? GetOwner(int id)
        => throw new NotImplementedException("ApiAppleIdDataService is not implemented yet.");

    /// <inheritdoc />
    public Task<Customer?> GetOwnerAsync(int id)
        => throw new NotImplementedException("ApiAppleIdDataService is not implemented yet.");

    /// <inheritdoc />
    public Guarantee? GetGuarantee(int id)
        => throw new NotImplementedException("ApiAppleIdDataService is not implemented yet.");

    /// <inheritdoc />
    public Task<Guarantee?> GetGuaranteeAsync(int id)
        => throw new NotImplementedException("ApiAppleIdDataService is not implemented yet.");

    /// <inheritdoc />
    public SecondHand? GetSecondHandInfo(int id)
        => throw new NotImplementedException("ApiAppleIdDataService is not implemented yet.");

    /// <inheritdoc />
    public Task<SecondHand?> GetSecondHandInfoAsync(int id)
        => throw new NotImplementedException("ApiAppleIdDataService is not implemented yet.");

    /// <inheritdoc />
    public int Quantity()
        => throw new NotImplementedException("ApiAppleIdDataService is not implemented yet.");

    /// <inheritdoc />
    public int SecondHandQuantity()
        => throw new NotImplementedException("ApiAppleIdDataService is not implemented yet.");

    /// <inheritdoc />
    public int AvailableSecondHandQuantity()
        => throw new NotImplementedException("ApiAppleIdDataService is not implemented yet.");

    /// <inheritdoc />
    public Task<int> QuantityAsync()
        => throw new NotImplementedException("ApiAppleIdDataService is not implemented yet.");

    /// <inheritdoc />
    public Task<int> SecondHandQuantityAsync()
        => throw new NotImplementedException("ApiAppleIdDataService is not implemented yet.");

    /// <inheritdoc />
    public Task<int> AvailableSecondHandQuantityAsync()
        => throw new NotImplementedException("ApiAppleIdDataService is not implemented yet.");

    /// <inheritdoc />
    public IEnumerable<AppleId> GetSecondHand()
        => throw new NotImplementedException("ApiAppleIdDataService is not implemented yet.");

    /// <inheritdoc />
    public IEnumerable<AppleId> GetAvailableSecondHand()
        => throw new NotImplementedException("ApiAppleIdDataService is not implemented yet.");

    /// <inheritdoc />
    public Task<IEnumerable<AppleId>> GetSecondHandAsync()
        => throw new NotImplementedException("ApiAppleIdDataService is not implemented yet.");

    /// <inheritdoc />
    public Task<IEnumerable<AppleId>> GetAvailableSecondHandAsync()
        => throw new NotImplementedException("ApiAppleIdDataService is not implemented yet.");

    /// <inheritdoc />
    public IEnumerable<AppleId> FindAll(Expression<Func<AppleId, bool>>? predicate = null)
        => throw new NotImplementedException("ApiAppleIdDataService is not implemented yet.");

    /// <inheritdoc />
    public Task<IEnumerable<AppleId>> FindAllAsync(Expression<Func<AppleId, bool>>? predicate = null)
        => throw new NotImplementedException("ApiAppleIdDataService is not implemented yet.");

    /// <inheritdoc />
    public AppleId? Find(int id)
        => throw new NotImplementedException("ApiAppleIdDataService is not implemented yet.");

    /// <inheritdoc />
    public AppleId? Find(Expression<Func<AppleId, bool>> predicate)
        => throw new NotImplementedException("ApiAppleIdDataService is not implemented yet.");

    /// <inheritdoc />
    public Task<AppleId?> FindAsync(int id)
        => throw new NotImplementedException("ApiAppleIdDataService is not implemented yet.");

    /// <inheritdoc />
    public Task<AppleId?> FindAsync(Expression<Func<AppleId, bool>> predicate)
        => throw new NotImplementedException("ApiAppleIdDataService is not implemented yet.");

    /// <inheritdoc />
    public bool Add(AppleId entity)
        => throw new NotImplementedException("ApiAppleIdDataService is not implemented yet.");

    /// <inheritdoc />
    public bool Update(AppleId entity)
        => throw new NotImplementedException("ApiAppleIdDataService is not implemented yet.");

    /// <inheritdoc />
    public bool Delete(int id)
        => throw new NotImplementedException("ApiAppleIdDataService is not implemented yet.");

    /// <inheritdoc />
    public bool Delete(AppleId entity)
        => throw new NotImplementedException("ApiAppleIdDataService is not implemented yet.");

    /// <inheritdoc />
    public Task<bool> AddAsync(AppleId entity)
        => throw new NotImplementedException("ApiAppleIdDataService is not implemented yet.");

    /// <inheritdoc />
    public Task<bool> UpdateAsync(AppleId entity)
        => throw new NotImplementedException("ApiAppleIdDataService is not implemented yet.");

    /// <inheritdoc />
    public Task<bool> DeleteAsync(int id)
        => throw new NotImplementedException("ApiAppleIdDataService is not implemented yet.");

    /// <inheritdoc />
    public Task<bool> DeleteAsync(AppleId entity)
        => throw new NotImplementedException("ApiAppleIdDataService is not implemented yet.");
}
