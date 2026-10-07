namespace MobileShop.Services.DataServices.Api;

public class ApiProductsDataService : IProductsDataService
{
    /// <inheritdoc />
    public Task<IReadOnlyList<ProductListItemViewModel>> GetInventoryRowsAsync(string? type = null, int? partNumberId = null, string? availability = null)
        => throw new NotImplementedException("ApiProductsDataService is not implemented yet.");

    /// <inheritdoc />
    public Task<IReadOnlyList<ProductListItemViewModel>> GetSecondHandRowsAsync()
        => throw new NotImplementedException("ApiProductsDataService is not implemented yet.");

    /// <inheritdoc />
    public Task<ProductDetailsViewModel?> GetDetailsAsync(int id, string type)
        => throw new NotImplementedException("ApiProductsDataService is not implemented yet.");

    /// <inheritdoc />
    public Task<IReadOnlyList<DropdownOptionViewModel>> GetManufacturersAsync()
        => throw new NotImplementedException("ApiProductsDataService is not implemented yet.");

    /// <inheritdoc />
    public Task<IReadOnlyList<DropdownOptionViewModel>> GetModelsAsync(int manufacturerId, string categoryName = "Phone")
        => throw new NotImplementedException("ApiProductsDataService is not implemented yet.");

    /// <inheritdoc />
    public Task<IReadOnlyList<DropdownOptionViewModel>> GetColorsAsync()
        => throw new NotImplementedException("ApiProductsDataService is not implemented yet.");

    /// <inheritdoc />
    public Task<IReadOnlyList<DropdownOptionViewModel>> GetStorageCapacitiesAsync()
        => throw new NotImplementedException("ApiProductsDataService is not implemented yet.");

    /// <inheritdoc />
    public Task<IReadOnlyList<string>> GetGuaranteeCorporationsAsync()
        => throw new NotImplementedException("ApiProductsDataService is not implemented yet.");

    /// <inheritdoc />
    public Task<DropdownCreateResult> CreateManufacturerAsync(string name)
        => throw new NotImplementedException("ApiProductsDataService is not implemented yet.");

    /// <inheritdoc />
    public Task<DropdownCreateResult> CreateModelAsync(int manufacturerId, string name, string categoryName = "Phone")
        => throw new NotImplementedException("ApiProductsDataService is not implemented yet.");

    /// <inheritdoc />
    public Task<DropdownCreateResult> CreateColorAsync(string name)
        => throw new NotImplementedException("ApiProductsDataService is not implemented yet.");

    /// <inheritdoc />
    public Task<DropdownCreateResult> CreateStorageCapacityAsync(int gb)
        => throw new NotImplementedException("ApiProductsDataService is not implemented yet.");

    /// <inheritdoc />
    public Task<IReadOnlyList<DropdownOptionViewModel>> GetPartNumbersAsync(int? modelId = null)
        => throw new NotImplementedException("ApiProductsDataService is not implemented yet.");

    /// <inheritdoc />
    public Task<IReadOnlyList<DropdownOptionViewModel>> GetInventoryPartNumbersAsync()
        => throw new NotImplementedException("ApiProductsDataService is not implemented yet.");

    /// <inheritdoc />
    public Task<DropdownCreateResult> CreatePartNumberAsync(int modelId, string code, bool supportsDualSim = false, bool supportsEsim = false)
        => throw new NotImplementedException("ApiProductsDataService is not implemented yet.");

    /// <inheritdoc />
    public Task<ServiceResult> CreatePhoneAsync(CreatePhoneInputModel input)
        => throw new NotImplementedException("ApiProductsDataService is not implemented yet.");

    /// <inheritdoc />
    public Task<ServiceResult> CreateGlassesAsync(CreateGlassInputModel input)
        => throw new NotImplementedException("ApiProductsDataService is not implemented yet.");

    /// <inheritdoc />
    public Task<ServiceResult> CreateAppleIdAsync(CreateAppleIdInputModel input)
        => throw new NotImplementedException("ApiProductsDataService is not implemented yet.");

    public Task<ServiceResult> CreateTabletAsync(CreateTabletInputModel input) => throw new NotImplementedException("ApiProductsDataService is not implemented yet.");
    public Task<ServiceResult> CreateSmartWatchAsync(CreateSmartWatchInputModel input) => throw new NotImplementedException("ApiProductsDataService is not implemented yet.");
    public Task<ServiceResult> CreateLaptopAsync(CreateLaptopInputModel input) => throw new NotImplementedException("ApiProductsDataService is not implemented yet.");
    public Task<ServiceResult> CreateCablesAsync(CreateCableInputModel input) => throw new NotImplementedException("ApiProductsDataService is not implemented yet.");
    public Task<ServiceResult> CreateChargersAsync(CreateChargerInputModel input) => throw new NotImplementedException("ApiProductsDataService is not implemented yet.");
    public Task<ServiceResult> CreatePowerBanksAsync(CreatePowerBankInputModel input) => throw new NotImplementedException("ApiProductsDataService is not implemented yet.");
    public Task<ServiceResult> CreatePortableStoragesAsync(CreatePortableStorageInputModel input) => throw new NotImplementedException("ApiProductsDataService is not implemented yet.");
    public Task<ServiceResult> CreateCasesAsync(CreateCaseInputModel input) => throw new NotImplementedException("ApiProductsDataService is not implemented yet.");
}
