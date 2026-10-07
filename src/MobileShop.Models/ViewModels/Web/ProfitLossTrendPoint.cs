namespace MobileShop.Models.ViewModels.Web;

public enum ProfitLossInterval
{
    Month,
    Week,
    Day,
    Hour
}

public sealed record ProfitLossTrendPoint(DateTime PeriodStart, long Profit);
