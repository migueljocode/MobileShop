namespace MobileShop.Models.ViewModels.Web;

/// <summary>A customer row for the customers list.</summary>
public sealed record CustomerListItemViewModel(
    int Id,
    string Name,
    string PhoneNumber,
    string NationalId);