namespace MobileShop.Services.DataServices.Api;

public class ApiSellerDataService : ISellerDataService
{
    public IReadOnlyList<PartyOptionViewModel> GetPartyOptions()
        => throw new NotImplementedException("ApiSellerDataService is not implemented yet.");

    public IReadOnlyList<SellerListItemViewModel> GetListRows()
        => throw new NotImplementedException("ApiSellerDataService is not implemented yet.");

    public SellerDetailsViewModel? GetDetails(int id)
        => throw new NotImplementedException("ApiSellerDataService is not implemented yet.");

    public IEnumerable<Product> SoldProducts(int sellerId)
        => throw new NotImplementedException("ApiSellerDataService is not implemented yet.");

    public Task<IEnumerable<Product>> SoldProductsAsync(int sellerId)
        => throw new NotImplementedException("ApiSellerDataService is not implemented yet.");

    public IEnumerable<Product> SoldToShop(int sellerId)
        => throw new NotImplementedException("ApiSellerDataService is not implemented yet.");

    public Task<IEnumerable<Product>> SoldToShopAsync(int sellerId)
        => throw new NotImplementedException("ApiSellerDataService is not implemented yet.");

    public IEnumerable<Seller> FindAll(System.Linq.Expressions.Expression<Func<Seller, bool>>? predicate = null)
        => throw new NotImplementedException("ApiSellerDataService is not implemented yet.");

    public Task<IEnumerable<Seller>> FindAllAsync(System.Linq.Expressions.Expression<Func<Seller, bool>>? predicate = null)
        => throw new NotImplementedException("ApiSellerDataService is not implemented yet.");

    public Seller? Find(int id)
        => throw new NotImplementedException("ApiSellerDataService is not implemented yet.");

    public Seller? Find(System.Linq.Expressions.Expression<Func<Seller, bool>> predicate)
        => throw new NotImplementedException("ApiSellerDataService is not implemented yet.");

    public Task<Seller?> FindAsync(int id)
        => throw new NotImplementedException("ApiSellerDataService is not implemented yet.");

    public Task<Seller?> FindAsync(System.Linq.Expressions.Expression<Func<Seller, bool>> predicate)
        => throw new NotImplementedException("ApiSellerDataService is not implemented yet.");

    public bool Add(Seller entity)
        => throw new NotImplementedException("ApiSellerDataService is not implemented yet.");

    public bool Update(Seller entity)
        => throw new NotImplementedException("ApiSellerDataService is not implemented yet.");

    public bool Delete(int id)
        => throw new NotImplementedException("ApiSellerDataService is not implemented yet.");

    public bool Delete(Seller entity)
        => throw new NotImplementedException("ApiSellerDataService is not implemented yet.");

    public Task<bool> AddAsync(Seller entity)
        => throw new NotImplementedException("ApiSellerDataService is not implemented yet.");

    public Task<bool> UpdateAsync(Seller entity)
        => throw new NotImplementedException("ApiSellerDataService is not implemented yet.");

    public Task<bool> DeleteAsync(int id)
        => throw new NotImplementedException("ApiSellerDataService is not implemented yet.");

    public Task<bool> DeleteAsync(Seller entity)
        => throw new NotImplementedException("ApiSellerDataService is not implemented yet.");
}
