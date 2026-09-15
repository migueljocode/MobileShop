namespace MobileShop.Services.DataServices.Interfaces;

/// <summary>
/// Defines the public contract for ISellerDataService.
/// </summary>
public interface ISellerDataService : IDataService<Seller>
{
    IReadOnlyList<PartyOptionViewModel> GetPartyOptions();
    IEnumerable<Product> SoldProducts(int sellerId);
    Task<IEnumerable<Product>> SoldProductsAsync(int sellerId);
    IEnumerable<Product> SoldToShop(int sellerId);
    Task<IEnumerable<Product>> SoldToShopAsync(int sellerId);
}
