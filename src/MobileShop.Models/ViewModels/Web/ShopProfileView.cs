namespace MobileShop.Models.ViewModels.Web;

/// <summary>Shop identity values loaded from the database for display and editing.</summary>
public sealed record ShopProfileView(
    string Name,
    string Address,
    string Phone,
    string Instagram);
