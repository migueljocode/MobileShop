namespace MobileShop.Services.DataServices.Api;

public class ApiProductDataService : IProductDataService
{
    /// <inheritdoc />
    public IReadOnlyList<ProductListItemViewModel> GetInventoryRows()
        => throw new NotImplementedException("ApiProductDataService is not implemented yet.");

    /// <inheritdoc />
    public IEnumerable<Product> FindAll(Expression<Func<Product, bool>>? predicate = null)
        => throw new NotImplementedException("ApiProductDataService is not implemented yet.");

    /// <inheritdoc />
    public Task<IEnumerable<Product>> FindAllAsync(Expression<Func<Product, bool>>? predicate = null)
        => throw new NotImplementedException("ApiProductDataService is not implemented yet.");

    /// <inheritdoc />
    public Product? Find(int id)
        => throw new NotImplementedException("ApiProductDataService is not implemented yet.");

    /// <inheritdoc />
    public Product? Find(Expression<Func<Product, bool>> predicate)
        => throw new NotImplementedException("ApiProductDataService is not implemented yet.");

    /// <inheritdoc />
    public Task<Product?> FindAsync(int id)
        => throw new NotImplementedException("ApiProductDataService is not implemented yet.");

    /// <inheritdoc />
    public Task<Product?> FindAsync(Expression<Func<Product, bool>> predicate)
        => throw new NotImplementedException("ApiProductDataService is not implemented yet.");

    /// <inheritdoc />
    public bool Add(Product entity)
        => throw new NotImplementedException("ApiProductDataService is not implemented yet.");

    /// <inheritdoc />
    public bool Update(Product entity)
        => throw new NotImplementedException("ApiProductDataService is not implemented yet.");

    /// <inheritdoc />
    public bool Delete(int id)
        => throw new NotImplementedException("ApiProductDataService is not implemented yet.");

    /// <inheritdoc />
    public bool Delete(Product entity)
        => throw new NotImplementedException("ApiProductDataService is not implemented yet.");

    /// <inheritdoc />
    public Task<bool> AddAsync(Product entity)
        => throw new NotImplementedException("ApiProductDataService is not implemented yet.");

    /// <inheritdoc />
    public Task<bool> UpdateAsync(Product entity)
        => throw new NotImplementedException("ApiProductDataService is not implemented yet.");

    /// <inheritdoc />
    public Task<bool> DeleteAsync(int id)
        => throw new NotImplementedException("ApiProductDataService is not implemented yet.");

    /// <inheritdoc />
    public Task<bool> DeleteAsync(Product entity)
        => throw new NotImplementedException("ApiProductDataService is not implemented yet.");
}
