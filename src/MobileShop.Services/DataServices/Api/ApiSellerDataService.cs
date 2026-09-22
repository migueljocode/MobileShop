namespace MobileShop.Services.DataServices.Api;

public class ApiSellerDataService : ISellerDataService
{
    /// <inheritdoc />
    public IReadOnlyList<PartyOptionViewModel> GetPartyOptions()
        => throw new NotImplementedException("ApiSellerDataService is not implemented yet.");

    /// <inheritdoc />
    public IReadOnlyList<SellerListItemViewModel> GetListRows()
        => throw new NotImplementedException("ApiSellerDataService is not implemented yet.");

    /// <inheritdoc />
    public SellerDetailsViewModel? GetDetails(int id)
        => throw new NotImplementedException("ApiSellerDataService is not implemented yet.");

    /// <inheritdoc />
    public IEnumerable<Product> SoldProducts(int sellerId)
        => throw new NotImplementedException("ApiSellerDataService is not implemented yet.");

    /// <inheritdoc />
    public Task<IEnumerable<Product>> SoldProductsAsync(int sellerId)
        => throw new NotImplementedException("ApiSellerDataService is not implemented yet.");

    /// <inheritdoc />
    public IEnumerable<Product> SoldToShop(int sellerId)
        => throw new NotImplementedException("ApiSellerDataService is not implemented yet.");

    /// <inheritdoc />
    public Task<IEnumerable<Product>> SoldToShopAsync(int sellerId)
        => throw new NotImplementedException("ApiSellerDataService is not implemented yet.");

    /// <inheritdoc />
    public IEnumerable<Seller> FindAll(Expression<Func<Seller, bool>>? predicate = null)
        => throw new NotImplementedException("ApiSellerDataService is not implemented yet.");

    /// <inheritdoc />
    public Task<IEnumerable<Seller>> FindAllAsync(Expression<Func<Seller, bool>>? predicate = null)
        => throw new NotImplementedException("ApiSellerDataService is not implemented yet.");

    /// <inheritdoc />
    public Seller? Find(int id)
        => throw new NotImplementedException("ApiSellerDataService is not implemented yet.");

    /// <inheritdoc />
    public Seller? Find(Expression<Func<Seller, bool>> predicate)
        => throw new NotImplementedException("ApiSellerDataService is not implemented yet.");

    /// <inheritdoc />
    public Task<Seller?> FindAsync(int id)
        => throw new NotImplementedException("ApiSellerDataService is not implemented yet.");

    /// <inheritdoc />
    public Task<Seller?> FindAsync(Expression<Func<Seller, bool>> predicate)
        => throw new NotImplementedException("ApiSellerDataService is not implemented yet.");

    /// <inheritdoc />
    public bool Add(Seller entity)
        => throw new NotImplementedException("ApiSellerDataService is not implemented yet.");

    /// <inheritdoc />
    public bool Update(Seller entity)
        => throw new NotImplementedException("ApiSellerDataService is not implemented yet.");

    /// <inheritdoc />
    public bool Delete(int id)
        => throw new NotImplementedException("ApiSellerDataService is not implemented yet.");

    /// <inheritdoc />
    public bool Delete(Seller entity)
        => throw new NotImplementedException("ApiSellerDataService is not implemented yet.");

    /// <inheritdoc />
    public Task<bool> AddAsync(Seller entity)
        => throw new NotImplementedException("ApiSellerDataService is not implemented yet.");

    /// <inheritdoc />
    public Task<bool> UpdateAsync(Seller entity)
        => throw new NotImplementedException("ApiSellerDataService is not implemented yet.");

    /// <inheritdoc />
    public Task<bool> DeleteAsync(int id)
        => throw new NotImplementedException("ApiSellerDataService is not implemented yet.");

    /// <inheritdoc />
    public Task<bool> DeleteAsync(Seller entity)
        => throw new NotImplementedException("ApiSellerDataService is not implemented yet.");
}
