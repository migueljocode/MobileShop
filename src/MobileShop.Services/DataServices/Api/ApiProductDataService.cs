namespace MobileShop.Services.DataServices.Api;

public class ApiProductDataService : IProductDataService
{
    public IReadOnlyList<ProductListItemViewModel> GetInventoryRows()
        => throw new NotImplementedException("ApiProductDataService is not implemented yet.");

    public IEnumerable<Product> FindAll(System.Linq.Expressions.Expression<Func<Product, bool>>? predicate = null)
        => throw new NotImplementedException("ApiProductDataService is not implemented yet.");

    public Task<IEnumerable<Product>> FindAllAsync(System.Linq.Expressions.Expression<Func<Product, bool>>? predicate = null)
        => throw new NotImplementedException("ApiProductDataService is not implemented yet.");

    public Product? Find(int id)
        => throw new NotImplementedException("ApiProductDataService is not implemented yet.");

    public Product? Find(System.Linq.Expressions.Expression<Func<Product, bool>> predicate)
        => throw new NotImplementedException("ApiProductDataService is not implemented yet.");

    public Task<Product?> FindAsync(int id)
        => throw new NotImplementedException("ApiProductDataService is not implemented yet.");

    public Task<Product?> FindAsync(System.Linq.Expressions.Expression<Func<Product, bool>> predicate)
        => throw new NotImplementedException("ApiProductDataService is not implemented yet.");

    public bool Add(Product entity)
        => throw new NotImplementedException("ApiProductDataService is not implemented yet.");

    public bool Update(Product entity)
        => throw new NotImplementedException("ApiProductDataService is not implemented yet.");

    public bool Delete(int id)
        => throw new NotImplementedException("ApiProductDataService is not implemented yet.");

    public bool Delete(Product entity)
        => throw new NotImplementedException("ApiProductDataService is not implemented yet.");

    public Task<bool> AddAsync(Product entity)
        => throw new NotImplementedException("ApiProductDataService is not implemented yet.");

    public Task<bool> UpdateAsync(Product entity)
        => throw new NotImplementedException("ApiProductDataService is not implemented yet.");

    public Task<bool> DeleteAsync(int id)
        => throw new NotImplementedException("ApiProductDataService is not implemented yet.");

    public Task<bool> DeleteAsync(Product entity)
        => throw new NotImplementedException("ApiProductDataService is not implemented yet.");
}
