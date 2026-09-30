namespace MobileShop.Services.DataServices.Api;

public class ApiProductsDataService : IProductsDataService
{
    /// <inheritdoc />
    public Task<IReadOnlyList<ProductListItemViewModel>> GetInventoryRowsAsync()
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
    public Task<IReadOnlyList<DropdownOptionViewModel>> GetModelsAsync(int manufacturerId)
        => throw new NotImplementedException("ApiProductsDataService is not implemented yet.");

    /// <inheritdoc />
    public Task<IReadOnlyList<DropdownOptionViewModel>> GetColorsAsync()
        => throw new NotImplementedException("ApiProductsDataService is not implemented yet.");

    /// <inheritdoc />
    public Task<IReadOnlyList<string>> GetGuaranteeCorporationsAsync()
        => throw new NotImplementedException("ApiProductsDataService is not implemented yet.");

    /// <inheritdoc />
    public Task<DropdownCreateResult> CreateManufacturerAsync(string name)
        => throw new NotImplementedException("ApiProductsDataService is not implemented yet.");

    /// <inheritdoc />
    public Task<DropdownCreateResult> CreateModelAsync(int manufacturerId, string name)
        => throw new NotImplementedException("ApiProductsDataService is not implemented yet.");

    /// <inheritdoc />
    public Task<DropdownCreateResult> CreateColorAsync(string name)
        => throw new NotImplementedException("ApiProductsDataService is not implemented yet.");

    /// <inheritdoc />
    public Task<ServiceResult> CreatePhoneAsync(CreatePhoneInputModel input)
        => throw new NotImplementedException("ApiProductsDataService is not implemented yet.");

    /// <inheritdoc />
    public Task<ServiceResult> CreateAppleIdAsync(CreateAppleIdInputModel input)
        => throw new NotImplementedException("ApiProductsDataService is not implemented yet.");
}
