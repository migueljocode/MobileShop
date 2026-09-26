namespace MobileShop.Services.DataServices.Interfaces;

/// <summary>
/// Defines the public contract for IAppleIdDataService.
/// </summary>
public interface IAppleIdDataService : IDataService<AppleId>
{
    /// <summary>Gets Apple ID inventory rows for the product list.</summary>
    IReadOnlyList<ProductListItemViewModel> GetInventoryRows();
    /// <summary>Gets Apple ID inventory rows for the product list asynchronously.</summary>
    Task<IReadOnlyList<ProductListItemViewModel>> GetInventoryRowsAsync() => throw new NotImplementedException("ApiAppleIdDataService is not implemented yet.");
    /// <summary>Gets Apple IDs eligible for the specified transaction direction.</summary>
    /// <param name="direction">The transaction direction to check.</param>
    IReadOnlyList<ProductListItemViewModel> GetSelectableProducts(TransactionDirection direction);
    /// <summary>Gets Apple IDs eligible for the specified transaction direction asynchronously.</summary>
    /// <param name="direction">The transaction direction to check.</param>
    Task<IReadOnlyList<ProductListItemViewModel>> GetSelectableProductsAsync(TransactionDirection direction) => throw new NotImplementedException("ApiAppleIdDataService is not implemented yet.");
    /// <summary>Gets all second-hand Apple ID inventory rows.</summary>
    IReadOnlyList<ProductListItemViewModel> GetSecondHandRows();
    /// <summary>Gets all second-hand Apple ID inventory rows asynchronously.</summary>
    Task<IReadOnlyList<ProductListItemViewModel>> GetSecondHandRowsAsync() => throw new NotImplementedException("ApiAppleIdDataService is not implemented yet.");
    /// <summary>Gets available second-hand Apple ID inventory rows.</summary>
    IReadOnlyList<ProductListItemViewModel> GetAvailableSecondHandRows();
    /// <summary>Gets available second-hand Apple ID inventory rows asynchronously.</summary>
    Task<IReadOnlyList<ProductListItemViewModel>> GetAvailableSecondHandRowsAsync() => throw new NotImplementedException("ApiAppleIdDataService is not implemented yet.");
    /// <summary>Gets Apple ID details, or <see langword="null"/> when not found.</summary>
    /// <param name="id">The Apple ID identifier.</param>
    ProductDetailsViewModel? GetDetails(int id);
    /// <summary>Gets Apple ID details asynchronously, or <see langword="null"/> when not found.</summary>
    /// <param name="id">The Apple ID identifier.</param>
    Task<ProductDetailsViewModel?> GetDetailsAsync(int id) => throw new NotImplementedException("ApiAppleIdDataService is not implemented yet.");
    /// <summary>Finds an Apple ID by email, or <see langword="null"/> when not found.</summary>
    /// <param name="email">The Apple ID email address.</param>
    AppleId? FindByEmail(string email);
    /// <summary>Finds an Apple ID by email asynchronously.</summary>
    /// <param name="email">The Apple ID email address.</param>
    Task<AppleId?> FindByEmailAsync(string email);
    /// <summary>Determines whether the Apple ID has been sold.</summary>
    /// <param name="id">The Apple ID identifier.</param>
    bool IsSold(int id);
    /// <summary>Determines asynchronously whether the Apple ID has been sold.</summary>
    /// <param name="id">The Apple ID identifier.</param>
    Task<bool> IsSoldAsync(int id);
    /// <summary>Gets the customer owner, or <see langword="null"/> when not sold.</summary>
    /// <param name="id">The Apple ID identifier.</param>
    Customer? GetOwner(int id);
    /// <summary>Gets the customer owner asynchronously, or <see langword="null"/> when not sold.</summary>
    /// <param name="id">The Apple ID identifier.</param>
    Task<Customer?> GetOwnerAsync(int id);
    /// <summary>Gets the guarantee, or <see langword="null"/> when none exists.</summary>
    /// <param name="id">The Apple ID identifier.</param>
    Guarantee? GetGuarantee(int id);
    /// <summary>Gets the guarantee asynchronously, or <see langword="null"/> when none exists.</summary>
    /// <param name="id">The Apple ID identifier.</param>
    Task<Guarantee?> GetGuaranteeAsync(int id);
    /// <summary>Gets second-hand information, or <see langword="null"/> when not second-hand.</summary>
    /// <param name="id">The Apple ID identifier.</param>
    SecondHand? GetSecondHandInfo(int id);
    /// <summary>Gets second-hand information asynchronously, or <see langword="null"/> when not second-hand.</summary>
    /// <param name="id">The Apple ID identifier.</param>
    Task<SecondHand?> GetSecondHandInfoAsync(int id);
    /// <summary>Counts Apple IDs in inventory.</summary>
    int Quantity();
    /// <summary>Counts second-hand Apple IDs.</summary>
    int SecondHandQuantity();
    /// <summary>Counts available second-hand Apple IDs.</summary>
    int AvailableSecondHandQuantity();
    /// <summary>Counts Apple IDs in inventory asynchronously.</summary>
    Task<int> QuantityAsync();
    /// <summary>Counts second-hand Apple IDs asynchronously.</summary>
    Task<int> SecondHandQuantityAsync();
    /// <summary>Counts available second-hand Apple IDs asynchronously.</summary>
    Task<int> AvailableSecondHandQuantityAsync();
    /// <summary>Gets second-hand Apple IDs.</summary>
    IEnumerable<AppleId> GetSecondHand();
    /// <summary>Gets available second-hand Apple IDs.</summary>
    IEnumerable<AppleId> GetAvailableSecondHand();
    /// <summary>Gets second-hand Apple IDs asynchronously.</summary>
    Task<IEnumerable<AppleId>> GetSecondHandAsync();
    /// <summary>Gets available second-hand Apple IDs asynchronously.</summary>
    Task<IEnumerable<AppleId>> GetAvailableSecondHandAsync();
}
