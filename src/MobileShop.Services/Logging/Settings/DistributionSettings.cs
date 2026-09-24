namespace MobileShop.Services.Logging.Settings;

/// <summary>
/// Settings for the profit distribution calculator.
/// </summary>
public class DistributionSettings
{
    /// <summary>
    /// Default share percent for an employee when not explicitly set on the employee.
    /// </summary>
    public int DefaultSharePercent { get; set; } = 50;

    /// <summary>
    /// Maximum total share percent that can be distributed to employees (the rest goes to shop reinvestment).
    /// </summary>
    public int MaxTotalSharePercent { get; set; } = 100;
}

/// <summary>
/// Calculates profit distribution for a given period.
/// </summary>
public static class DistributionCalculator
{
    /// <summary>
    /// Calculates profit distribution for a given period.
    /// </summary>
    /// <param name="totalProfit">Total profit (positive) or loss (negative) for the period.</param>
    /// <param name="employees">Active employees with their share percentages.</param>
    /// <param name="settings">Distribution settings.</param>
    /// <returns>List of distribution rows with employee share and shop reinvestment.</returns>
    public static List<DistributionRow> Calculate(
        decimal totalProfit,
        IEnumerable<Employee> employees,
        DistributionSettings settings)
    {
        var rows = new List<DistributionRow>();
        
        if (totalProfit <= 0)
        {
            // Loss or zero profit - no payouts, shop absorbs everything
            foreach (var emp in employees.Where(e => e.IsActive))
            {
                rows.Add(new DistributionRow
                {
                    EmployeeId = emp.Id,
                    EmployeeName = $"{emp.PersonNavigation.FirstName} {emp.PersonNavigation.LastName}",
                    SharePercent = emp.SharePercent,
                    CalculatedAmount = 0,
                    IsLossPeriod = true
                });
            }
            
            rows.Add(new DistributionRow
            {
                EmployeeId = 0,
                EmployeeName = "Shop (Reinvestment)",
                SharePercent = 100 - employees.Where(e => e.IsActive).Sum(e => e.SharePercent),
                CalculatedAmount = totalProfit,
                IsLossPeriod = true
            });
            
            return rows;
        }

        // Profit case - distribute according to share percentages
        var activeEmployees = employees.Where(e => e.IsActive).ToList();
        var totalAssignedShare = activeEmployees.Sum(e => e.SharePercent);
        var maxAllowedShare = Math.Min(totalAssignedShare, settings.MaxTotalSharePercent);
        var shopSharePercent = Math.Max(0, 100 - maxAllowedShare);
        
        foreach (var emp in activeEmployees)
        {
            var shareRatio = (decimal)emp.SharePercent / 100;
            var amount = Math.Floor(totalProfit * shareRatio); // Round down to avoid fractional currency
            rows.Add(new DistributionRow
            {
                EmployeeId = emp.Id,
                EmployeeName = $"{emp.PersonNavigation.FirstName} {emp.PersonNavigation.LastName}",
                SharePercent = emp.SharePercent,
                CalculatedAmount = amount,
                IsLossPeriod = false
            });
        }

        // Shop gets the remainder (including any rounding differences)
        var totalDistributed = rows.Sum(r => r.CalculatedAmount);
        var shopAmount = totalProfit - totalDistributed;
        
        rows.Add(new DistributionRow
        {
            EmployeeId = 0,
            EmployeeName = "Shop (Reinvestment)",
            SharePercent = shopSharePercent,
            CalculatedAmount = shopAmount,
            IsLossPeriod = false
        });

        return rows;
    }
}

/// <summary>
/// Represents a single row in the profit distribution report.
/// </summary>
public sealed record DistributionRow
{
    public int EmployeeId { get; init; }
    public string EmployeeName { get; init; } = string.Empty;
    public int SharePercent { get; init; }
    public decimal CalculatedAmount { get; init; }
    public bool IsLossPeriod { get; init; }
}