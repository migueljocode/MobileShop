namespace MobileShop.Services.DataServices.Dal;

/// <summary>Provides the reporting operations for the Reports area.</summary>
public class ReportsDataService(
    IBaseRepo<Transaction> transactions,
    ITransactionRepo transactionRepo,
    IEmployeeRepo employeeRepo,
    IOptions<DistributionSettings> distributionSettings,
    ILogger<ReportsDataService> logger)
    : IReportsDataService
{
    /// <summary>Gets the structured logger for this reports service.</summary>
    protected ILogger<ReportsDataService> Logger { get; } = logger;

    /// <inheritdoc />
    public async Task<IReadOnlyList<ProfitLossRowViewModel>> GetProfitLossRowsAsync(DateTime? from, DateTime? to)
    {
        // Projected so the product navigation is read inside the query instead of relying on
        // eager-loaded entities.
        var all = await transactions
            .SelectAllAsync(transaction => new
            {
                transaction.ProductId,
                transaction.Date,
                transaction.Direction,
                transaction.FinishedPrice,
                ProductLabel = transaction.ProductNavigation.ModelNavigation.ManufacturerNavigation.Name + " " + transaction.ProductNavigation.ModelNavigation.Name
            })
            .ConfigureAwait(false);

        var filtered = all
            .Where(transaction => (!from.HasValue || transaction.Date.Date >= from.Value.Date) &&
                                  (!to.HasValue || transaction.Date.Date <= to.Value.Date));

        return filtered
            .GroupBy(transaction => transaction.ProductId)
            .Select(group =>
            {
                var first = group.First();
                return new ProfitLossRowViewModel(
                    group.Key,
                    first.ProductLabel,
                    group.Where(t => t.Direction == TransactionDirection.Buy).Sum(t => t.FinishedPrice),
                    group.Where(t => t.Direction == TransactionDirection.Sell).Sum(t => t.FinishedPrice));
            })
            .OrderBy(row => row.ProductId)
            .ToList();
    }

    /// <inheritdoc />
    public async Task<decimal> GetProfitLossTotalAsync(DateTime? from, DateTime? to)
        => (await GetProfitLossRowsAsync(from, to)).Sum(row => row.Profit);

    /// <inheritdoc />
    public async Task<DateTime?> GetEarliestTransactionDateAsync()
    {
        // IBaseRepo<T> has no ordered "first" projection, so the specialized repo query is used here;
        // it applies the same non-deleted filter and returns a date-only value.
        var earliest = await transactionRepo.GetEarliestTransactionDateAsync();
        return earliest?.Date;
    }

    /// <inheritdoc />
    public async Task<IReadOnlyList<DistributionRow>> GetDistributionRowsAsync(decimal totalProfit)
    {
        // The calculator requires Employee entities with PersonNavigation loaded (it matches the
        // required employees by full name), so the repo query that includes the person is used.
        var employees = await employeeRepo.FindAllActiveAsync();
        return DistributionCalculator.Calculate(totalProfit, employees, distributionSettings.Value);
    }
}
