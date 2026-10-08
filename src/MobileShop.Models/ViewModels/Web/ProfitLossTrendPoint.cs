namespace MobileShop.Models.ViewModels.Web;

public enum ProfitLossInterval
{
    Year,
    Month,
    Week,
    Day,
    Hour
}

public sealed record ProfitLossTrendPoint(DateTime PeriodStart, long Profit);
