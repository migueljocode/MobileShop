namespace MobileShop.Models.ViewModels.Web;

/// <summary>Flattened customer details, including the person's name and phone.</summary>
public sealed record CustomerDetailsViewModel(
    string Name,
    string PhoneNumber,
    string NationalId);