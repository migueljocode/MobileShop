namespace MobileShop.Models.ViewModels.Web;

/// <summary>A seller row for the sellers list.</summary>
public sealed record SellerListItemViewModel(
    int Id,
    string Name,
    string PhoneNumber,
    string EntityType);