namespace MobileShop.Services.DataServices.Interfaces;

/// <summary>
/// Defines the public contract for ISellerDataService.
/// </summary>
public interface ISellerDataService : IDataService<Seller>
{
    /// <summary>Gets seller options for transaction selectors.</summary>
    IReadOnlyList<PartyOptionViewModel> GetPartyOptions();
    /// <summary>Gets seller options for transaction selectors asynchronously.</summary>
    Task<IReadOnlyList<PartyOptionViewModel>> GetPartyOptionsAsync() => throw new NotImplementedException("ApiSellerDataService is not implemented yet.");
    /// <summary>Gets seller rows for the sellers list.</summary>
    IReadOnlyList<SellerListItemViewModel> GetListRows();
    /// <summary>Gets seller rows for the sellers list asynchronously.</summary>
    Task<IReadOnlyList<SellerListItemViewModel>> GetListRowsAsync() => throw new NotImplementedException("ApiSellerDataService is not implemented yet.");
    /// <summary>Gets flattened seller details, or <see langword="null"/> when not found.</summary>
    /// <param name="id">The seller identifier.</param>
    SellerDetailsViewModel? GetDetails(int id);
    /// <summary>Gets flattened seller details asynchronously, or <see langword="null"/> when not found.</summary>
    /// <param name="id">The seller identifier.</param>
    Task<SellerDetailsViewModel?> GetDetailsAsync(int id) => throw new NotImplementedException("ApiSellerDataService is not implemented yet.");
    /// <summary>Gets products associated with a seller's transactions.</summary>
    /// <param name="sellerId">The seller identifier.</param>
    IEnumerable<Product> SoldProducts(int sellerId);
    /// <summary>Gets products associated with a seller's transactions asynchronously.</summary>
    /// <param name="sellerId">The seller identifier.</param>
    Task<IEnumerable<Product>> SoldProductsAsync(int sellerId);
    /// <summary>Gets products supplied by a seller to the shop.</summary>
    /// <param name="sellerId">The seller identifier.</param>
    IEnumerable<Product> SoldToShop(int sellerId);
    /// <summary>Gets products supplied by a seller to the shop asynchronously.</summary>
    /// <param name="sellerId">The seller identifier.</param>
    Task<IEnumerable<Product>> SoldToShopAsync(int sellerId);
}
