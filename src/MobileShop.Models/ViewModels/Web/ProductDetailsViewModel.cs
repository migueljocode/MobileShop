namespace MobileShop.Models.ViewModels.Web;

public sealed record ProductDetailsViewModel(
    string Type,
    int ProductId,
    string Manufacturer,
    string Model,
    string Identifier,
    string? Color,
    string OwnerLabel,
    string GuaranteeLabel,
    bool IsSecondHand);
