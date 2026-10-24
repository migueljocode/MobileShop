namespace MobileShop.Services.DataServices.Dal;

/// <summary>Provides the reporting operations for the Reports area.</summary>
public class ReportsDataService(
    IBaseRepo<Transaction> transactions,
    IBaseRepo<Employee> employees,
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
            .GroupBy(transaction => transaction.Date.ToJalaliPeriodStart(interval))
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
                        var salePeriod = salesInRange[0].Date.ToJalaliPeriodStart(interval);
                        intervalProfit[salePeriod] -= acquisitionCost;
                    }
                }
                continue;
            }

            if (!first.IsGlass || buys.Count > 0)
                continue;

            var period = first.Date.ToJalaliPeriodStart(interval);
            intervalProfit[period] -= first.ProductPrice;
        }

        return intervalProfit
            .OrderBy(period => period.Key)
            .Select(period => new ProfitLossTrendPoint(period.Key, period.Value))
            .ToList();
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
        // The calculator reads each active employee's stored share, so the projection supplies the
        // id, the share and the person's name. Ordering by name mirrors the previous active-employee query.
        var active = await employees.SelectAllAsync(
            employee => employee.IsActive,
            employee => new
            {
                employee.Id,
                employee.SharePercent,
                employee.PersonNavigation.FirstName,
                employee.PersonNavigation.LastName,
            });

        var employeeList = active
            .Select(employee => new Employee
            {
                Id = employee.Id,
                IsActive = true,
                SharePercent = employee.SharePercent,
                PersonNavigation = new Person
                {
                    FirstName = employee.FirstName,
                    LastName = employee.LastName,
                },
            })
            .OrderBy(employee => employee.PersonNavigation.FirstName)
            .ThenBy(employee => employee.PersonNavigation.LastName)
            .ToList();

        return DistributionCalculator.Calculate(totalProfit, employeeList);
    }

    /// <inheritdoc />
    public async Task<IReadOnlyList<EmployeeShareViewModel>> GetActiveEmployeeSharesAsync()
    {
        // Projection gives the person's name without loading navigations; FindAllAsync does not include them.
        var active = await employees.SelectAllAsync(
            employee => employee.IsActive,
            employee => new
            {
                employee.Id,
                employee.SharePercent,
                employee.PersonNavigation.FirstName,
                employee.PersonNavigation.LastName,
            });

        return active
            .Select(employee => new EmployeeShareViewModel(
                employee.Id,
                $"{employee.FirstName} {employee.LastName}",
                employee.SharePercent))
            .OrderByDescending(share => share.SharePercent)
            .ThenBy(share => share.Name)
            .ToList();
    }

    /// <inheritdoc />
    public async Task<EmployeeSharesResult> UpdateEmployeeSharesAsync(IReadOnlyList<EmployeeShareInput> shares)
    {
        if (shares.Count == 0)
            return EmployeeSharesResult.Fail("No employee shares were submitted.");

        if (shares.Select(share => share.EmployeeId).Distinct().Count() != shares.Count)
            return EmployeeSharesResult.Fail("An employee appears more than once.");

        if (shares.Any(share => share.SharePercent is < 0 or > 100))
            return EmployeeSharesResult.Fail("Each share must be between 0% and 100%.");

        var storedShares = (await employees.SelectAllAsync(
            employee => employee.IsActive,
            employee => new { employee.Id, employee.SharePercent })).ToList();
        var activeIds = storedShares.Select(stored => stored.Id).ToHashSet();
        if (shares.Any(share => !activeIds.Contains(share.EmployeeId)))
            return EmployeeSharesResult.Fail("A selected employee is not active.");

        // Check the total against posted values, falling back to stored values for everyone else.
        var postedShares = shares.ToDictionary(share => share.EmployeeId, share => share.SharePercent);
        var total = storedShares.Sum(stored =>
            postedShares.TryGetValue(stored.Id, out var posted) ? posted : stored.SharePercent);
        if (total > 100)
            return EmployeeSharesResult.Fail($"Total share is {total}%; it cannot exceed 100%.");

        // Tracked reads, one per posted employee: they stay consistent with any entity this context already tracks.
        var changed = new List<Employee>(shares.Count);
        foreach (var share in shares)
        {
            var employee = await employees.FindAsync(candidate => candidate.Id == share.EmployeeId)
                ?? throw new InvalidOperationException(
                    $"Active employee {share.EmployeeId} was not found while saving shares.");
            employee.SharePercent = share.SharePercent;
            changed.Add(employee);
        }

        await employees.UpdateRangeAsync(changed);
        Logger.LogInformation("Updated {Count} employee shares; active total is {Total}%.", changed.Count, total);
        return EmployeeSharesResult.Ok;
    }
}
