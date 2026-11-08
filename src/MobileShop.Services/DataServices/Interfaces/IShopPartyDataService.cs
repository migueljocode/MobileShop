namespace MobileShop.Services.DataServices.Interfaces;

/// <summary>Defines the startup operation that keeps the Zone Mobile shop party registered.</summary>
public interface IShopPartyDataService
{
    /// <summary>
    /// Ensures the Zone Mobile shop person, seller, and customer exist, taking the shop phone number
    /// from <c>Pdf:ShopPhone</c> in the application settings.
    /// </summary>
    Task EnsureAsync();
}
