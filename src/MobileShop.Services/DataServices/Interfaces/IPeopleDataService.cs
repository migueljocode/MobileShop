namespace MobileShop.Services.DataServices.Interfaces;

/// <summary>Defines the customer and seller operations provided by the People area.</summary>
public interface IPeopleDataService
{
    /// <summary>Gets sorted customer list rows.</summary>
    /// <param name="sortBy">The sort property.</param>
    /// <param name="ascending">Whether to sort ascending.</param>
    Task<IReadOnlyList<CustomerListItemViewModel>> GetCustomerRowsAsync(string sortBy, bool ascending);

    /// <summary>Gets sorted seller list rows.</summary>
    /// <param name="sortBy">The sort property.</param>
    /// <param name="ascending">Whether to sort ascending.</param>
    Task<IReadOnlyList<SellerListItemViewModel>> GetSellerRowsAsync(string sortBy, bool ascending);

    /// <summary>Gets customer details and the customer's product rows.</summary>
    /// <param name="id">The customer identifier.</param>
    Task<CustomerDetailsViewModel?> GetCustomerDetailsAsync(int id);

    /// <summary>Gets seller details and the seller's product rows.</summary>
    /// <param name="id">The seller identifier.</param>
    Task<SellerDetailsViewModel?> GetSellerDetailsAsync(int id);

    /// <summary>Searches customer party options by name, phone, or national id.</summary>
    Task<IReadOnlyList<PartyOptionViewModel>> SearchCustomersAsync(string? q, int take = 25);

    /// <summary>Searches seller party options by name or phone.</summary>
    Task<IReadOnlyList<PartyOptionViewModel>> SearchSellersAsync(string? q, int take = 25);

    /// <summary>Creates a customer from the submitted input.</summary>
    /// <param name="input">The customer input.</param>
    Task<ServiceResult> CreateCustomerAsync(MobileShop.Models.ViewModels.Web.BindModels.CreateCustomerInputModel input);

    /// <summary>Creates a seller from the submitted input.</summary>
    /// <param name="input">The seller input.</param>
    Task<ServiceResult> CreateSellerAsync(MobileShop.Models.ViewModels.Web.BindModels.CreateSellerInputModel input);
}
