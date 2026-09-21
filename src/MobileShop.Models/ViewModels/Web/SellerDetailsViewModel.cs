namespace MobileShop.Models.ViewModels.Web;

/// <summary>Flattened seller details, including the person's name and phone.</summary>
public sealed record SellerDetailsViewModel(
    string Name,
    string PhoneNumber,
    string EntityType);