namespace MobileShop.Services.DataServices.Api;

public class ApiProductDataService : ApiDataServiceBase<Product>, IProductDataService
{
    /// <inheritdoc />
    public IReadOnlyList<ProductListItemViewModel> GetInventoryRows()
        => throw new NotImplementedException("ApiProductDataService is not implemented yet.");
}
