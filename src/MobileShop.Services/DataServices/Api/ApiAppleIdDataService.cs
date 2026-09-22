namespace MobileShop.Services.DataServices.Api;

public class ApiAppleIdDataService : IAppleIdDataService
{
    public IReadOnlyList<ProductListItemViewModel> GetInventoryRows()
        => throw new NotImplementedException("ApiAppleIdDataService is not implemented yet.");

    public IReadOnlyList<ProductListItemViewModel> GetSelectableProducts(TransactionDirection direction)
        => throw new NotImplementedException("ApiAppleIdDataService is not implemented yet.");

    public IReadOnlyList<ProductListItemViewModel> GetSecondHandRows()
        => throw new NotImplementedException("ApiAppleIdDataService is not implemented yet.");

    public IReadOnlyList<ProductListItemViewModel> GetAvailableSecondHandRows()
        => throw new NotImplementedException("ApiAppleIdDataService is not implemented yet.");

    public ProductDetailsViewModel? GetDetails(int id)
        => throw new NotImplementedException("ApiAppleIdDataService is not implemented yet.");

    public AppleId? FindByEmail(string email)
        => throw new NotImplementedException("ApiAppleIdDataService is not implemented yet.");

    public Task<AppleId?> FindByEmailAsync(string email)
        => throw new NotImplementedException("ApiAppleIdDataService is not implemented yet.");

    public bool IsSold(int id)
        => throw new NotImplementedException("ApiAppleIdDataService is not implemented yet.");

    public Task<bool> IsSoldAsync(int id)
        => throw new NotImplementedException("ApiAppleIdDataService is not implemented yet.");

    public Customer? GetOwner(int id)
        => throw new NotImplementedException("ApiAppleIdDataService is not implemented yet.");

    public Task<Customer?> GetOwnerAsync(int id)
        => throw new NotImplementedException("ApiAppleIdDataService is not implemented yet.");

    public Guarantee? GetGuarantee(int id)
        => throw new NotImplementedException("ApiAppleIdDataService is not implemented yet.");

    public Task<Guarantee?> GetGuaranteeAsync(int id)
        => throw new NotImplementedException("ApiAppleIdDataService is not implemented yet.");

    public SecondHand? GetSecondHandInfo(int id)
        => throw new NotImplementedException("ApiAppleIdDataService is not implemented yet.");

    public Task<SecondHand?> GetSecondHandInfoAsync(int id)
        => throw new NotImplementedException("ApiAppleIdDataService is not implemented yet.");

    public int Quantity()
        => throw new NotImplementedException("ApiAppleIdDataService is not implemented yet.");

    public int SecondHandQuantity()
        => throw new NotImplementedException("ApiAppleIdDataService is not implemented yet.");

    public int AvailableSecondHandQuantity()
        => throw new NotImplementedException("ApiAppleIdDataService is not implemented yet.");

    public Task<int> QuantityAsync()
        => throw new NotImplementedException("ApiAppleIdDataService is not implemented yet.");

    public Task<int> SecondHandQuantityAsync()
        => throw new NotImplementedException("ApiAppleIdDataService is not implemented yet.");

    public Task<int> AvailableSecondHandQuantityAsync()
        => throw new NotImplementedException("ApiAppleIdDataService is not implemented yet.");

    public IEnumerable<AppleId> GetSecondHand()
        => throw new NotImplementedException("ApiAppleIdDataService is not implemented yet.");

    public IEnumerable<AppleId> GetAvailableSecondHand()
        => throw new NotImplementedException("ApiAppleIdDataService is not implemented yet.");

    public Task<IEnumerable<AppleId>> GetSecondHandAsync()
        => throw new NotImplementedException("ApiAppleIdDataService is not implemented yet.");

    public Task<IEnumerable<AppleId>> GetAvailableSecondHandAsync()
        => throw new NotImplementedException("ApiAppleIdDataService is not implemented yet.");

    public IEnumerable<AppleId> FindAll(System.Linq.Expressions.Expression<Func<AppleId, bool>>? predicate = null)
        => throw new NotImplementedException("ApiAppleIdDataService is not implemented yet.");

    public Task<IEnumerable<AppleId>> FindAllAsync(System.Linq.Expressions.Expression<Func<AppleId, bool>>? predicate = null)
        => throw new NotImplementedException("ApiAppleIdDataService is not implemented yet.");

    public AppleId? Find(int id)
        => throw new NotImplementedException("ApiAppleIdDataService is not implemented yet.");

    public AppleId? Find(System.Linq.Expressions.Expression<Func<AppleId, bool>> predicate)
        => throw new NotImplementedException("ApiAppleIdDataService is not implemented yet.");

    public Task<AppleId?> FindAsync(int id)
        => throw new NotImplementedException("ApiAppleIdDataService is not implemented yet.");

    public Task<AppleId?> FindAsync(System.Linq.Expressions.Expression<Func<AppleId, bool>> predicate)
        => throw new NotImplementedException("ApiAppleIdDataService is not implemented yet.");

    public bool Add(AppleId entity)
        => throw new NotImplementedException("ApiAppleIdDataService is not implemented yet.");

    public bool Update(AppleId entity)
        => throw new NotImplementedException("ApiAppleIdDataService is not implemented yet.");

    public bool Delete(int id)
        => throw new NotImplementedException("ApiAppleIdDataService is not implemented yet.");

    public bool Delete(AppleId entity)
        => throw new NotImplementedException("ApiAppleIdDataService is not implemented yet.");

    public Task<bool> AddAsync(AppleId entity)
        => throw new NotImplementedException("ApiAppleIdDataService is not implemented yet.");

    public Task<bool> UpdateAsync(AppleId entity)
        => throw new NotImplementedException("ApiAppleIdDataService is not implemented yet.");

    public Task<bool> DeleteAsync(int id)
        => throw new NotImplementedException("ApiAppleIdDataService is not implemented yet.");

    public Task<bool> DeleteAsync(AppleId entity)
        => throw new NotImplementedException("ApiAppleIdDataService is not implemented yet.");
}
