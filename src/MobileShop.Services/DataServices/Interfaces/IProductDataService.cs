namespace MobileShop.Services.DataServices.Interfaces;

/// <summary>
/// Defines inventory operations for all catalog products.
/// </summary>
public interface IProductDataService : IDataService<Product>
{
    /// <summary>Gets one projected inventory row for every product.</summary>
    IReadOnlyList<ProductListItemViewModel> GetInventoryRows();
    /// <summary>Gets one projected inventory row for every product asynchronously.</summary>
    Task<IReadOnlyList<ProductListItemViewModel>> GetInventoryRowsAsync() => throw new NotImplementedException("ApiProductDataService is not implemented yet.");
}
