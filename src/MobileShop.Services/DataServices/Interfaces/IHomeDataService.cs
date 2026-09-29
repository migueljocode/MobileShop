namespace MobileShop.Services.DataServices.Interfaces;

/// <summary>Defines the dashboard data operations provided by the Home area.</summary>
public interface IHomeDataService
{
    /// <summary>Gets the current stock summary.</summary>
    Task<DashboardStockSummary> GetStockAsync();

    /// <summary>Gets the most recent transaction cards.</summary>
    /// <param name="count">The maximum number of cards to return.</param>
    Task<IReadOnlyList<TransactionCardViewModel>> GetRecentTransactionsAsync(int count = 20);
}
