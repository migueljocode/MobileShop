namespace MobileShop.Services.Logging.Settings;

/// <summary>
/// Calculates the profit distribution for a period from each active employee's stored share.
/// The shop share is never stored: it is 100 minus the sum of the active employees' shares.
/// </summary>
public static class DistributionCalculator
{
    private const string ShopName = "Shop";

    /// <summary>
    /// Builds one row per active employee plus one shop row.
    /// </summary>
    /// <param name="totalProfit">Total profit (positive) or loss (negative/zero) for the period.</param>
    /// <param name="employees">All employees; only active ones take part.</param>
    /// <returns>Rows ordered by share (descending), then name.</returns>
    /// <exception cref="InvalidOperationException">Thrown when an active share is outside 0–100 or the active shares total more than 100.</exception>
    public static List<DistributionRow> Calculate(long totalProfit, IEnumerable<Employee> employees)
    {
        var activeEmployees = employees.Where(employee => employee.IsActive).ToList();

        foreach (var employee in activeEmployees)
        {
            if (employee.SharePercent is < 0 or > 100)
                throw new InvalidOperationException(
                    $"Share for {EmployeeName(employee)} must be between 0% and 100%.");
        }

        var activeTotal = activeEmployees.Sum(employee => employee.SharePercent);
        if (activeTotal > 100)
            throw new InvalidOperationException(
                $"Active employee shares total {activeTotal}%, which exceeds 100%.");

        var isLossPeriod = totalProfit <= 0;
        var rows = new List<DistributionRow>(activeEmployees.Count + 1);
        long employeesTotal = 0;

        foreach (var employee in activeEmployees)
        {
            var amount = isLossPeriod ? 0 : ShareOf(totalProfit, employee.SharePercent);
            employeesTotal += amount;

            rows.Add(new DistributionRow
            {
                EmployeeId = employee.Id,
                EmployeeName = EmployeeName(employee),
                SharePercent = employee.SharePercent,
                CalculatedAmount = amount,
                IsLossPeriod = isLossPeriod
            });
        }

        // The shop takes the remainder, so a loss is absorbed entirely by the shop.
        rows.Add(new DistributionRow
        {
            EmployeeId = 0,
            EmployeeName = ShopName,
            SharePercent = 100 - activeTotal,
            CalculatedAmount = totalProfit - employeesTotal,
            IsLossPeriod = isLossPeriod
        });

        return rows
            .OrderByDescending(row => row.SharePercent)
            .ThenBy(row => row.EmployeeName)
            .ToList();
    }

    /// <summary>
    /// Returns floor(profit × percent / 100) without overflowing <see cref="long"/> for large profits.
    /// </summary>
    private static long ShareOf(long profit, int percent) =>
        profit / 100 * percent + profit % 100 * percent / 100;

    private static string EmployeeName(Employee employee) =>
        $"{employee.PersonNavigation.FirstName} {employee.PersonNavigation.LastName}";
}

/// <summary>
/// Represents a single row in the profit distribution report.
/// </summary>
public sealed record DistributionRow
{
    public int EmployeeId { get; init; }
    public string EmployeeName { get; init; } = string.Empty;
    public int SharePercent { get; init; }
    public long CalculatedAmount { get; init; }
    public bool IsLossPeriod { get; init; }
}
