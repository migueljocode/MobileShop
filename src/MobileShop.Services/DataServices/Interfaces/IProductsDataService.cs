namespace MobileShop.Services.DataServices.Interfaces;

/// <summary>Defines the inventory and creation operations provided by the Products area.</summary>
public interface IProductsDataService
{
    /// <summary>Gets all new-product inventory rows.</summary>
    /// <param name="type">The product type filter.</param>
    Task<IReadOnlyList<ProductListItemViewModel>> GetInventoryRowsAsync(string? type = null);

    /// <summary>Gets all second-hand product rows.</summary>
    Task<IReadOnlyList<ProductListItemViewModel>> GetSecondHandRowsAsync();

    /// <summary>Gets product details and its transaction rows.</summary>
    /// <param name="id">The product identifier.</param>
    /// <param name="type">The product type.</param>
    Task<ProductDetailsViewModel?> GetDetailsAsync(int id, string type);

    /// <summary>Gets all manufacturer dropdown options.</summary>
    Task<IReadOnlyList<DropdownOptionViewModel>> GetManufacturersAsync();

    /// <summary>Gets the model dropdown options for a manufacturer.</summary>
    /// <param name="manufacturerId">The manufacturer identifier.</param>
    Task<IReadOnlyList<DropdownOptionViewModel>> GetModelsAsync(int manufacturerId);

    /// <summary>Gets all color dropdown options.</summary>
    Task<IReadOnlyList<DropdownOptionViewModel>> GetColorsAsync();

    /// <summary>Gets all distinct guarantee corporation names.</summary>
    Task<IReadOnlyList<string>> GetGuaranteeCorporationsAsync();

    /// <summary>Creates a manufacturer and returns its dropdown option.</summary>
    /// <param name="name">The manufacturer name.</param>
    Task<DropdownCreateResult> CreateManufacturerAsync(string name);

    /// <summary>Creates a model and returns its dropdown option.</summary>
    /// <param name="manufacturerId">The owning manufacturer identifier.</param>
    /// <param name="name">The model name.</param>
    Task<DropdownCreateResult> CreateModelAsync(int manufacturerId, string name);

    /// <summary>Creates a color and returns its dropdown option.</summary>
    /// <param name="name">The color name.</param>
    Task<DropdownCreateResult> CreateColorAsync(string name);

    /// <summary>Creates a phone from the submitted input.</summary>
    /// <param name="input">The phone input.</param>
    Task<ServiceResult> CreatePhoneAsync(CreatePhoneInputModel input);

    /// <summary>Creates an Apple ID from the submitted input.</summary>
    /// <param name="input">The Apple ID input.</param>
    Task<ServiceResult> CreateAppleIdAsync(CreateAppleIdInputModel input);
}
