namespace MobileShop.Services.DataServices.Interfaces;

/// <summary>Defines reporting operations provided by the Reports area.</summary>
public interface IReportsDataService
{
    /// <summary>Gets profit and loss rows within an optional date range.</summary>
    /// <param name="from">The inclusive start date.</param>
    /// <param name="to">The inclusive end date.</param>
    Task<IReadOnlyList<ProfitLossRowViewModel>> GetProfitLossRowsAsync(DateTime? from, DateTime? to);

    /// <summary>Gets profit or loss values grouped by the requested interval.</summary>
    /// <param name="from">The inclusive start date.</param>
    /// <param name="to">The inclusive end date.</param>
    /// <param name="interval">The time interval used to group values.</param>
    Task<IReadOnlyList<ProfitLossTrendPoint>> GetProfitLossTrendAsync(DateTime? from, DateTime? to, ProfitLossInterval interval);

    /// <summary>Gets the total profit or loss within an optional date range.</summary>
    /// <param name="from">The inclusive start date.</param>
    /// <param name="to">The inclusive end date.</param>
    Task<long> GetProfitLossTotalAsync(DateTime? from, DateTime? to);

    /// <summary>Gets the earliest transaction date, or <see langword="null"/> when none exists.</summary>
    Task<DateTime?> GetEarliestTransactionDateAsync();

    /// <summary>Gets distribution rows for a total profit amount.</summary>
    /// <param name="totalProfit">The total profit to distribute.</param>
    Task<IReadOnlyList<DistributionRow>> GetDistributionRowsAsync(long totalProfit);
}
