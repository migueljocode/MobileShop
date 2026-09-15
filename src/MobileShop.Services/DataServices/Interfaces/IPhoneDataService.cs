namespace MobileShop.Services.DataServices.Interfaces;

public interface IPhoneDataService : IDataService<Phone>
{
    IReadOnlyList<ProductListItemViewModel> GetInventoryRows();
    IReadOnlyList<ProductListItemViewModel> GetSelectableProducts(TransactionDirection direction);
    IReadOnlyList<ProductListItemViewModel> GetSecondHandRows();
    IReadOnlyList<ProductListItemViewModel> GetAvailableSecondHandRows();
    ProductDetailsViewModel? GetDetails(int id);

    bool ImeiExists(string imei1);
    Task<bool> ImeiExistsAsync(string imei1);
    bool IsSold(int id);
    Task<bool> IsSoldAsync(int id);
    bool IsSecondHand(int id);
    Task<bool> IsSecondHandAsync(int id);
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
    IEnumerable<Phone> GetSecondHand();
    IEnumerable<Phone> GetAvailableSecondHand();
    Task<IEnumerable<Phone>> GetSecondHandAsync();
    Task<IEnumerable<Phone>> GetAvailableSecondHandAsync();
}
