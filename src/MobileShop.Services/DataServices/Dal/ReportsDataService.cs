namespace MobileShop.Services.DataServices.Dal;

/// <summary>Provides the reporting operations for the Reports area.</summary>
public class ReportsDataService(
    IBaseRepo<Transaction> transactions,
    IBaseRepo<Employee> employees,
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
                ProductPrice = transaction.ProductNavigation.Price,
                IsGlass = transaction.ProductNavigation.GlassProfile != null,
                ProductLabel = transaction.ProductNavigation.ModelNavigation.ManufacturerNavigation.Name + " " + transaction.ProductNavigation.ModelNavigation.Name
            })
            .ConfigureAwait(false);

        var filtered = all
            .Where(transaction => (!from.HasValue || transaction.Date.Date >= from.Value.Date) &&
                                  (!to.HasValue || transaction.Date.Date <= to.Value.Date));
        var historyByProduct = all
            .GroupBy(transaction => transaction.ProductId)
            .ToDictionary(group => group.Key, group => group.ToList());

        return filtered
            .GroupBy(transaction => transaction.ProductId)
            .Select(group =>
            {
                var first = group.First();
                var hasSaleInRange = group.Any(transaction => transaction.Direction == TransactionDirection.Sell);
                var purchaseHistory = historyByProduct[group.Key]
                    .Where(transaction => transaction.Direction == TransactionDirection.Buy)
                    .ToList();
                var bought = hasSaleInRange
                    ? purchaseHistory.Count > 0
                        ? purchaseHistory.Sum(transaction => transaction.FinishedPrice)
                        : first.IsGlass ? first.ProductPrice : 0
                    : group.Any(transaction => transaction.Direction == TransactionDirection.Buy)
                        ? group.Where(transaction => transaction.Direction == TransactionDirection.Buy).Sum(transaction => transaction.FinishedPrice)
                        : first.IsGlass ? first.ProductPrice : 0;

                return new ProfitLossRowViewModel(
                    group.Key,
                    first.ProductLabel,
                    bought,
                    group.Where(transaction => transaction.Direction == TransactionDirection.Sell).Sum(transaction => transaction.FinishedPrice));
            })
            .OrderBy(row => row.ProductId)
            .ToList();
    }

    /// <inheritdoc />
    public async Task<IReadOnlyList<ProfitLossTrendPoint>> GetProfitLossTrendAsync(
        DateTime? from,
        DateTime? to,
        ProfitLossInterval interval)
    {
        var transactionsInRange = await transactions
            .SelectAllAsync(transaction => new
            {
                transaction.ProductId,
                transaction.Date,
                transaction.Direction,
                transaction.FinishedPrice,
                ProductPrice = transaction.ProductNavigation.Price,
                IsGlass = transaction.ProductNavigation.GlassProfile != null
            })
            .ConfigureAwait(false);

        var filtered = transactionsInRange
            .Where(transaction => (!from.HasValue || transaction.Date.Date >= from.Value.Date) &&
                                  (!to.HasValue || transaction.Date.Date <= to.Value.Date))
            .ToList();
        var historyByProduct = transactionsInRange
            .GroupBy(transaction => transaction.ProductId)
            .ToDictionary(group => group.Key, group => group.ToList());

        var intervalProfit = filtered
            .GroupBy(transaction => GetPeriodStart(transaction.Date, interval))
            .ToDictionary(
                period => period.Key,
                period => period.Sum(transaction => transaction.Direction == TransactionDirection.Sell
                    ? transaction.FinishedPrice
                    : -transaction.FinishedPrice));

        foreach (var productTransactions in filtered.GroupBy(transaction => transaction.ProductId))
        {
            var productTransactionsInOrder = productTransactions.OrderBy(transaction => transaction.Date).ToList();
            var first = productTransactionsInOrder[0];
            var productHistory = historyByProduct[productTransactions.Key];
            var buys = productHistory.Where(transaction => transaction.Direction == TransactionDirection.Buy).ToList();
            var salesInRange = productTransactionsInOrder
                .Where(transaction => transaction.Direction == TransactionDirection.Sell)
                .ToList();

            if (salesInRange.Count > 0)
            {
                var hasBuyInRange = productTransactionsInOrder.Any(transaction => transaction.Direction == TransactionDirection.Buy);
                if (!hasBuyInRange)
                {
                    var acquisitionCost = buys.Count > 0
                        ? buys.Sum(transaction => transaction.FinishedPrice)
                        : first.IsGlass ? first.ProductPrice : 0;
                    if (acquisitionCost != 0)
                    {
                        var salePeriod = GetPeriodStart(salesInRange[0].Date, interval);
                        intervalProfit[salePeriod] -= acquisitionCost;
                    }
                }
                continue;
            }

            if (!first.IsGlass || buys.Count > 0)
                continue;

            var period = GetPeriodStart(first.Date, interval);
            intervalProfit[period] -= first.ProductPrice;
        }

        return intervalProfit
            .OrderBy(period => period.Key)
            .Select(period => new ProfitLossTrendPoint(period.Key, period.Value))
            .ToList();
    }

    private static DateTime GetPeriodStart(DateTime date, ProfitLossInterval interval)
        => interval switch
        {
            ProfitLossInterval.Year => new DateTime(date.Year, 1, 1),
            ProfitLossInterval.Month => new DateTime(date.Year, date.Month, 1),
            ProfitLossInterval.Week => StartOfWeek(date),
            ProfitLossInterval.Day => date.Date,
            ProfitLossInterval.Hour => new DateTime(date.Year, date.Month, date.Day, date.Hour, 0, 0, date.Kind),
            _ => throw new ArgumentOutOfRangeException(nameof(interval), interval, "Unsupported profit/loss interval.")
        };

    private static DateTime StartOfWeek(DateTime date)
    {
        var daysSinceMonday = ((int)date.DayOfWeek + 6) % 7;
        return date.Date.AddDays(-daysSinceMonday);
    }

    /// <inheritdoc />
    public async Task<long> GetProfitLossTotalAsync(DateTime? from, DateTime? to)
        => (await GetProfitLossRowsAsync(from, to)).Sum(row => row.Profit);

    /// <inheritdoc />
    public async Task<DateTime?> GetEarliestTransactionDateAsync()
    {
        // Uses the generic ordered "first" projection. The explicit non-deleted filter is required
        // here: IBaseRepo<T> applies no soft-delete filter of its own, and the specialized
        // repository query this replaces did filter !IsDeleted.
        var earliest = await transactions.SelectFirstAsync(
            transaction => !transaction.IsDeleted,
            transaction => transaction.Date,
            transaction => (DateTime?)transaction.Date);

        // Truncate to the date, matching the previous implementation's contract.
        return earliest?.Date;
    }

    /// <inheritdoc />
    public async Task<IReadOnlyList<DistributionRow>> GetDistributionRowsAsync(long totalProfit)
    {
        // The calculator matches the required employees by exact full name, so the projection must
        // supply the employee id plus the person's first and last name. Ordering by name mirrors
        // the previous active-employee query.
        var active = await employees.SelectAllAsync(
            employee => employee.IsActive,
            employee => new { employee.Id, employee.PersonNavigation.FirstName, employee.PersonNavigation.LastName });

        var employeeList = active
            .Select(employee => new Employee
            {
                Id = employee.Id,
                IsActive = true,
                PersonNavigation = new Person
                {
                    FirstName = employee.FirstName,
                    LastName = employee.LastName,
                },
            })
            .OrderBy(employee => employee.PersonNavigation.FirstName)
            .ThenBy(employee => employee.PersonNavigation.LastName)
            .ToList();

        return DistributionCalculator.Calculate(totalProfit, employeeList, distributionSettings.Value);
    }
}
