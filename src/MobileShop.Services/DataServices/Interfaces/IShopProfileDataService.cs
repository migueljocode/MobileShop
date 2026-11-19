namespace MobileShop.Services.DataServices.Interfaces;

/// <summary>Defines startup seeding, retrieval, and update operations for the shop profile.</summary>
public interface IShopProfileDataService
{
    /// <summary>
    /// Ensures a shop profile row exists (seeding once from the existing shop party or <c>Pdf:Shop*</c> settings)
    /// and syncs the row's values into <see cref="PdfSettings"/>.
    /// </summary>
    Task EnsureAsync();

    /// <summary>Returns the current shop profile for display and editing.</summary>
    Task<ShopProfileView> GetAsync();

    /// <summary>Validates and saves the shop profile, refreshes <see cref="PdfSettings"/>, and syncs the shop party.</summary>
    Task<ServiceResult> SaveAsync(ShopProfileInput input);
}
