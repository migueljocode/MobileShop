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
    public async Task<IReadOnlyList<DistributionRow>> GetDistributionRowsAsync(decimal totalProfit)
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
