
namespace MobileShop.Services.DataServices.Interfaces;

/// <summary>
/// Defines the public contract for IPhoneDataService.
/// </summary>
public interface IPhoneDataService : IDataService<Phone>
{
    /// <summary>Gets phone inventory rows for the product list.</summary>
    IReadOnlyList<ProductListItemViewModel> GetInventoryRows();
    /// <summary>Gets phones eligible for the specified transaction direction.</summary>
    /// <param name="direction">The transaction direction to check.</param>
    IReadOnlyList<ProductListItemViewModel> GetSelectableProducts(TransactionDirection direction);
    /// <summary>Gets all second-hand phone inventory rows.</summary>
    IReadOnlyList<ProductListItemViewModel> GetSecondHandRows();
    /// <summary>Gets available second-hand phone inventory rows.</summary>
    IReadOnlyList<ProductListItemViewModel> GetAvailableSecondHandRows();
    /// <summary>Gets phone details, or <see langword="null"/> when not found.</summary>
    /// <param name="id">The phone identifier.</param>
    ProductDetailsViewModel? GetDetails(int id);

    /// <summary>Determines whether an IMEI is already registered.</summary>
    /// <param name="imei1">The primary IMEI.</param>
    bool ImeiExists(string imei1);
    /// <summary>Determines asynchronously whether an IMEI is already registered.</summary>
    /// <param name="imei1">The primary IMEI.</param>
    Task<bool> ImeiExistsAsync(string imei1);
    /// <summary>Determines whether the phone has been sold.</summary>
    /// <param name="id">The phone identifier.</param>
    bool IsSold(int id);
    /// <summary>Determines asynchronously whether the phone has been sold.</summary>
    /// <param name="id">The phone identifier.</param>
    Task<bool> IsSoldAsync(int id);
    /// <summary>Determines whether the phone is second-hand.</summary>
    /// <param name="id">The phone identifier.</param>
    bool IsSecondHand(int id);
    /// <summary>Determines asynchronously whether the phone is second-hand.</summary>
    /// <param name="id">The phone identifier.</param>
    Task<bool> IsSecondHandAsync(int id);
    /// <summary>Gets the customer owner, or <see langword="null"/> when not sold.</summary>
    /// <param name="id">The phone identifier.</param>
    Customer? GetOwner(int id);
    /// <summary>Gets the customer owner asynchronously, or <see langword="null"/> when not sold.</summary>
    /// <param name="id">The phone identifier.</param>
    Task<Customer?> GetOwnerAsync(int id);
    /// <summary>Gets the guarantee, or <see langword="null"/> when none exists.</summary>
    /// <param name="id">The phone identifier.</param>
    Guarantee? GetGuarantee(int id);
    /// <summary>Gets the guarantee asynchronously, or <see langword="null"/> when none exists.</summary>
    /// <param name="id">The phone identifier.</param>
    Task<Guarantee?> GetGuaranteeAsync(int id);
    /// <summary>Gets second-hand information, or <see langword="null"/> when not second-hand.</summary>
    /// <param name="id">The phone identifier.</param>
    SecondHand? GetSecondHandInfo(int id);
    /// <summary>Gets second-hand information asynchronously, or <see langword="null"/> when not second-hand.</summary>
    /// <param name="id">The phone identifier.</param>
    Task<SecondHand?> GetSecondHandInfoAsync(int id);
    /// <summary>Counts phones in inventory.</summary>
    int Quantity();
    /// <summary>Counts second-hand phones.</summary>
    int SecondHandQuantity();
    /// <summary>Counts available second-hand phones.</summary>
    int AvailableSecondHandQuantity();
    /// <summary>Counts phones in inventory asynchronously.</summary>
    Task<int> QuantityAsync();
    /// <summary>Counts second-hand phones asynchronously.</summary>
    Task<int> SecondHandQuantityAsync();
    /// <summary>Counts available second-hand phones asynchronously.</summary>
    Task<int> AvailableSecondHandQuantityAsync();
    /// <summary>Gets second-hand phones.</summary>
    IEnumerable<Phone> GetSecondHand();
    /// <summary>Gets available second-hand phones.</summary>
    IEnumerable<Phone> GetAvailableSecondHand();
    /// <summary>Gets second-hand phones asynchronously.</summary>
    Task<IEnumerable<Phone>> GetSecondHandAsync();
    /// <summary>Gets available second-hand phones asynchronously.</summary>
    Task<IEnumerable<Phone>> GetAvailableSecondHandAsync();
}
