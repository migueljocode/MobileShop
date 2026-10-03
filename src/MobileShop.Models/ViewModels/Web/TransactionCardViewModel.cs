namespace MobileShop.Models.ViewModels.Web;

public sealed record TransactionCardViewModel(
    DateTime Date,
    TransactionDirection Direction,
    long FinishedPrice,
    string ProductLabel,
    string PartyLabel);
