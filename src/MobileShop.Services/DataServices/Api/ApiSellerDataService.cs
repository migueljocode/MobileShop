namespace MobileShop.Services.DataServices.Api;

public class ApiSellerDataService : ApiDataServiceBase<Seller>, ISellerDataService
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
}
