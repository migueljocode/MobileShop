namespace MobileShop.Models.ViewModels.Web;

public sealed record TransactionCardViewModel(
    DateTime Date,
    TransactionDirection Direction,
    int FinishedPrice,
    string ProductLabel,
    string PartyLabel);
