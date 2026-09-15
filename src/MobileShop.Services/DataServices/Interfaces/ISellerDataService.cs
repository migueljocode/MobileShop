namespace MobileShop.Services.DataServices.Interfaces;

public interface ISellerDataService : IDataService<Seller>
{
    IReadOnlyList<PartyOptionViewModel> GetPartyOptions();
    IEnumerable<Product> SoldProducts(int sellerId);
    Task<IEnumerable<Product>> SoldProductsAsync(int sellerId);
    IEnumerable<Product> SoldToShop(int sellerId);
    Task<IEnumerable<Product>> SoldToShopAsync(int sellerId);
}
