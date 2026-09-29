namespace MobileShop.Services.DataServices.Api;

public class ApiPeopleDataService : IPeopleDataService
{
    /// <inheritdoc />
    public Task<IReadOnlyList<CustomerListItemViewModel>> GetCustomerRowsAsync(string sortBy, bool ascending)
        => throw new NotImplementedException("ApiPeopleDataService is not implemented yet.");

    /// <inheritdoc />
    public Task<IReadOnlyList<SellerListItemViewModel>> GetSellerRowsAsync(string sortBy, bool ascending)
        => throw new NotImplementedException("ApiPeopleDataService is not implemented yet.");

    /// <inheritdoc />
    public Task<CustomerDetailsViewModel?> GetCustomerDetailsAsync(int id)
        => throw new NotImplementedException("ApiPeopleDataService is not implemented yet.");

    /// <inheritdoc />
    public Task<SellerDetailsViewModel?> GetSellerDetailsAsync(int id)
        => throw new NotImplementedException("ApiPeopleDataService is not implemented yet.");

    /// <inheritdoc />
    public Task<ServiceResult> CreateCustomerAsync(MobileShop.Models.ViewModels.Web.BindModels.CreateCustomerInputModel input)
        => throw new NotImplementedException("ApiPeopleDataService is not implemented yet.");

    /// <inheritdoc />
    public Task<ServiceResult> CreateSellerAsync(MobileShop.Models.ViewModels.Web.BindModels.CreateSellerInputModel input)
        => throw new NotImplementedException("ApiPeopleDataService is not implemented yet.");
}
