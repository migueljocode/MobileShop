namespace MobileShop.Services.DataServices.Interfaces;

/// <summary>Defines the inventory and creation operations provided by the Products area.</summary>
public interface IProductsDataService
{
    /// <summary>Gets all new-product inventory rows.</summary>
    /// <param name="type">The product type filter.</param>
    /// <param name="partNumberId">
    /// The optional part-number filter. A positive value limits the result to phones carrying that part
    /// number (Apple IDs are excluded); null, zero or a negative value applies no part-number filter.
    /// </param>
    Task<IReadOnlyList<ProductListItemViewModel>> GetInventoryRowsAsync(string? type = null, int? partNumberId = null);

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
    Task<IReadOnlyList<DropdownOptionViewModel>> GetModelsAsync(int manufacturerId, string categoryName = "Phone");

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

    /// <summary>Gets the part-number dropdown options for a model.</summary>
    /// <param name="modelId">
    /// The model identifier, or null to return every part number (used by the Create Phone form).
    /// </param>
    Task<IReadOnlyList<DropdownOptionViewModel>> GetPartNumbersAsync(int? modelId = null);

    /// <summary>
    /// Gets the distinct part numbers attached to the current phone inventory, for the Products
    /// list filter. Options come only from phones that exist in inventory (soft-deleted rows are
    /// excluded by the repository filters), so unused catalog part numbers do not appear.
    /// </summary>
    Task<IReadOnlyList<DropdownOptionViewModel>> GetInventoryPartNumbersAsync();

    /// <summary>Creates a part number for a model and returns its dropdown option.</summary>
    /// <param name="modelId">The owning model identifier.</param>
    /// <param name="code">The part-number code.</param>
    /// <param name="supportsDualSim">Whether the part number supports dual SIM.</param>
    /// <param name="supportsEsim">Whether the part number supports eSIM.</param>
    Task<DropdownCreateResult> CreatePartNumberAsync(int modelId, string code, bool supportsDualSim, bool supportsEsim);

    /// <summary>Creates a phone from the submitted input.</summary>
    /// <param name="input">The phone input.</param>
    Task<ServiceResult> CreatePhoneAsync(CreatePhoneInputModel input);

    /// <summary>Creates a batch of glass products from the submitted input.</summary>
    /// <param name="input">The glass batch input.</param>
    Task<ServiceResult> CreateGlassesAsync(CreateGlassInputModel input);

    /// <summary>Creates an Apple ID from the submitted input.</summary>
    /// <param name="input">The Apple ID input.</param>
    Task<ServiceResult> CreateAppleIdAsync(CreateAppleIdInputModel input);
}
