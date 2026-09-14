namespace MobileShop.Models.ViewModels.Web;

public sealed record DashboardStockSummary(
    int PhonesInStock,
    int PhonesSecondHand,
    int PhonesSecondHandAvailable,
    int AppleIdsInStock);
