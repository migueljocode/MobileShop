namespace MobileShop.Models.ViewModels.Web;

public sealed record ProductListItemViewModel(
    int EntityId,
    int ProductId,
    string Type,
    string Name,
    string Identifier,
    string? Color,
    bool IsSold,
    bool IsSecondHand);
