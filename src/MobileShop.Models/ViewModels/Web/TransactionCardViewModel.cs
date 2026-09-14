namespace MobileShop.Models.ViewModels.Web;

public sealed record TransactionCardViewModel(
    DateTime Date,
    TransactionDirection Direction,
    decimal FinishedPrice,
    string ProductLabel,
    string PartyLabel);
