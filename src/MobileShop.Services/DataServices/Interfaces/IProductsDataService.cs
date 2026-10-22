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
    /// <param name="availability">
    /// The availability filter. "available" returns only unsold rows, "sold" returns only sold rows;
    /// null, "all" or an unrecognised value returns both. Pages lowercase before calling.
    /// </param>
    Task<IReadOnlyList<ProductListItemViewModel>> GetInventoryRowsAsync(string? type = null, int? partNumberId = null, string? availability = null);

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

    /// <summary>Gets the storage-capacity dropdown options.</summary>
    Task<IReadOnlyList<DropdownOptionViewModel>> GetStorageCapacitiesAsync();
    Task<IReadOnlyList<DropdownOptionViewModel>> GetCpusAsync();
    Task<IReadOnlyList<DropdownOptionViewModel>> GetGpusAsync();

    /// <summary>Gets all distinct guarantee corporation names.</summary>
    Task<IReadOnlyList<string>> GetGuaranteeCorporationsAsync();

    /// <summary>Creates a manufacturer and returns its dropdown option.</summary>
    /// <param name="name">The manufacturer name.</param>
    Task<DropdownCreateResult> CreateManufacturerAsync(string name);

    /// <summary>Creates a model and returns its dropdown option.</summary>
    /// <param name="manufacturerId">The owning manufacturer identifier.</param>
    /// <param name="name">The model name.</param>
    Task<DropdownCreateResult> CreateModelAsync(int manufacturerId, string name, string categoryName = "Phone");

    /// <summary>Creates a color and returns its dropdown option.</summary>
    /// <param name="name">The color name.</param>
    Task<DropdownCreateResult> CreateColorAsync(string name);

    /// <summary>Creates a storage-capacity option and returns its dropdown choice.</summary>
    /// <param name="gb">The capacity in gigabytes.</param>
    Task<DropdownCreateResult> CreateStorageCapacityAsync(int gb);
    Task<DropdownCreateResult> CreateCpuAsync(string name);
    Task<DropdownCreateResult> CreateGpuAsync(string name);

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
    /// <param name="supportsDualSim">Legacy flag retained for compatibility; the active phone-level SIM capability is set when creating a phone.</param>
    /// <param name="supportsEsim">Legacy flag retained for compatibility; the active phone-level SIM capability is set when creating a phone.</param>
    Task<DropdownCreateResult> CreatePartNumberAsync(int modelId, string code, bool supportsDualSim = false, bool supportsEsim = false);

    /// <summary>Creates a phone from the submitted input.</summary>
    /// <param name="input">The phone input.</param>
    Task<ServiceResult> CreatePhoneAsync(CreatePhoneInputModel input);

    /// <summary>Creates a batch of glass products from the submitted input.</summary>
    /// <param name="input">The glass batch input.</param>
    Task<ServiceResult> CreateGlassesAsync(CreateGlassInputModel input);

    /// <summary>Creates an Apple ID from the submitted input.</summary>
    /// <param name="input">The Apple ID input.</param>
    Task<ServiceResult> CreateAppleIdAsync(CreateAppleIdInputModel input);
    Task<ServiceResult> CreateTabletAsync(CreateTabletInputModel input);
    Task<ServiceResult> CreateSmartWatchAsync(CreateSmartWatchInputModel input);
    Task<ServiceResult> CreateLaptopAsync(CreateLaptopInputModel input);
    Task<ServiceResult> CreateCablesAsync(CreateCableInputModel input);
    Task<ServiceResult> CreateChargersAsync(CreateChargerInputModel input);
    Task<ServiceResult> CreatePowerBanksAsync(CreatePowerBankInputModel input);
    Task<ServiceResult> CreatePortableStoragesAsync(CreatePortableStorageInputModel input);
    Task<ServiceResult> CreateCasesAsync(CreateCaseInputModel input);

    /// <summary>Updates an existing product.</summary>
    /// <param name="input">The product input.</param>
    Task<ServiceResult> UpdateProductAsync(EditProductInputModel input);

    /// <summary>Gets a product entity by ID with all navigation properties loaded for editing.</summary>
    /// <param name="id">The product identifier.</param>
    Task<Product?> GetProductForEditAsync(int id);
}
