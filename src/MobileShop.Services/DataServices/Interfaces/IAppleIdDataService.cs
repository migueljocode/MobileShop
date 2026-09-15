namespace MobileShop.Services.DataServices.Interfaces;

/// <summary>
/// Defines the public contract for IAppleIdDataService.
/// </summary>
public interface IAppleIdDataService : IDataService<AppleId>
{
    IReadOnlyList<ProductListItemViewModel> GetInventoryRows();
    IReadOnlyList<ProductListItemViewModel> GetSelectableProducts(TransactionDirection direction);
    IReadOnlyList<ProductListItemViewModel> GetSecondHandRows();
    IReadOnlyList<ProductListItemViewModel> GetAvailableSecondHandRows();
    ProductDetailsViewModel? GetDetails(int id);
    AppleId? FindByEmail(string email);
    Task<AppleId?> FindByEmailAsync(string email);
    bool IsSold(int id);
    Task<bool> IsSoldAsync(int id);
    Customer? GetOwner(int id);
    Task<Customer?> GetOwnerAsync(int id);
    Guarantee? GetGuarantee(int id);
    Task<Guarantee?> GetGuaranteeAsync(int id);
    SecondHand? GetSecondHandInfo(int id);
    Task<SecondHand?> GetSecondHandInfoAsync(int id);
    int Quantity();
    int SecondHandQuantity();
    int AvailableSecondHandQuantity();
    Task<int> QuantityAsync();
    Task<int> SecondHandQuantityAsync();
    Task<int> AvailableSecondHandQuantityAsync();
    IEnumerable<AppleId> GetSecondHand();
    IEnumerable<AppleId> GetAvailableSecondHand();
    Task<IEnumerable<AppleId>> GetSecondHandAsync();
    Task<IEnumerable<AppleId>> GetAvailableSecondHandAsync();
}
