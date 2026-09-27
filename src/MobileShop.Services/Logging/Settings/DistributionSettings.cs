namespace MobileShop.Services.Logging.Settings;

/// <summary>
/// Settings for the profit distribution calculator.
/// </summary>
public class DistributionSettings
{
    /// <summary>
    /// Default share percent for an employee when not explicitly set on the employee.
    /// </summary>
    /// <remarks>
    /// Retained for API compatibility; the fixed distribution (Mikaeeil 40%, Anis 50%, Shop 10%)
    /// does not use this value.
    /// </remarks>
    public int DefaultSharePercent { get; set; } = 50;

    /// <summary>
    /// Maximum total share percent that can be distributed to employees (the rest goes to shop reinvestment).
    /// </summary>
    /// <remarks>
    /// Retained for API compatibility; the fixed distribution (Mikaeeil 40%, Anis 50%, Shop 10%)
    /// does not use this value.
    /// </remarks>
    public int MaxTotalSharePercent { get; set; } = 100;
}

/// <summary>
/// Calculates profit distribution for a given period using the fixed three-row distribution:
/// Mikaeeil Jorjany (40%), Anis Sahabi (50%), Shop (10%).
/// </summary>
public static class DistributionCalculator
{
    /// <summary>
    /// Calculates profit distribution for a given period using the fixed three-row distribution:
    /// Mikaeeil Jorjany (40%), Anis Sahabi (50%), Shop (10%).
    /// </summary>
    /// <param name="totalProfit">Total profit (positive) or loss (negative/zero) for the period.</param>
    /// <param name="employees">All employees (active and inactive). Must contain exactly one active
    /// employee named "Mikaeeil Jorjany" and exactly one active employee named "Anis Sahabi".
    /// Inactive matches do not satisfy the requirement.</param>
    /// <param name="settings">Distribution settings (retained for API compatibility; not used by fixed distribution).</param>
    /// <returns>Exactly three distribution rows: Mikaeeil (40%), Anis (50%), Shop (10%).</returns>
    /// <exception cref="InvalidOperationException">Thrown if required employees are missing, duplicated, or inactive.</exception>
    public static List<DistributionRow> Calculate(
        decimal totalProfit,
        IEnumerable<Employee> employees,
        DistributionSettings settings)
    {
        // Materialize once so validation and row creation use the same set
        var employeeList = employees.ToList();

        // Find required active employees by exact full name
        var mikaeeil = employeeList
            .Where(e => e.IsActive && e.PersonNavigation != null && $"{e.PersonNavigation.FirstName} {e.PersonNavigation.LastName}" == "Mikaeeil Jorjany")
            .ToList();
        var anis = employeeList
            .Where(e => e.IsActive && e.PersonNavigation != null && $"{e.PersonNavigation.FirstName} {e.PersonNavigation.LastName}" == "Anis Sahabi")
            .ToList();

        if (mikaeeil.Count == 0)
            throw new InvalidOperationException("Required active employee 'Mikaeeil Jorjany' not found.");
        if (mikaeeil.Count > 1)
            throw new InvalidOperationException("Duplicate active employees named 'Mikaeeil Jorjany' found.");
        if (anis.Count == 0)
            throw new InvalidOperationException("Required active employee 'Anis Sahabi' not found.");
        if (anis.Count > 1)
            throw new InvalidOperationException("Duplicate active employees named 'Anis Sahabi' found.");

        var rows = new List<DistributionRow>();

        if (totalProfit <= 0)
        {
            // Loss or zero profit - no employee payouts, shop absorbs everything
            // Shares still display as 40/50/10
            rows.Add(new DistributionRow
            {
                EmployeeId = mikaeeil[0].Id,
                EmployeeName = "Mikaeeil Jorjany",
                SharePercent = 40,
                CalculatedAmount = 0,
                IsLossPeriod = true
            });
            rows.Add(new DistributionRow
            {
                EmployeeId = anis[0].Id,
                EmployeeName = "Anis Sahabi",
                SharePercent = 50,
                CalculatedAmount = 0,
                IsLossPeriod = true
            });
            rows.Add(new DistributionRow
            {
                EmployeeId = 0,
                EmployeeName = "Shop",
                SharePercent = 10,
                CalculatedAmount = totalProfit, // Entire loss goes to shop
                IsLossPeriod = true
            });
        }

        if (totalProfit > 0)
        {
        // Profit case - fixed shares 40/50/10 with floor rounding for employees
        var mikaeeilAmount = Math.Floor(totalProfit * 0.40m);
        var anisAmount = Math.Floor(totalProfit * 0.50m);
        var shopAmount = totalProfit - mikaeeilAmount - anisAmount; // Remainder including rounding

        rows.Add(new DistributionRow
        {
            EmployeeId = mikaeeil[0].Id,
            EmployeeName = "Mikaeeil Jorjany",
            SharePercent = 40,
            CalculatedAmount = mikaeeilAmount,
            IsLossPeriod = false
        });
        rows.Add(new DistributionRow
        {
            EmployeeId = anis[0].Id,
            EmployeeName = "Anis Sahabi",
            SharePercent = 50,
            CalculatedAmount = anisAmount,
            IsLossPeriod = false
        });
        rows.Add(new DistributionRow
        {
            EmployeeId = 0,
            EmployeeName = "Shop",
            SharePercent = 10,
            CalculatedAmount = shopAmount,
            IsLossPeriod = false
        });
        }

        return rows
            .OrderByDescending(r => r.SharePercent)
            .ThenBy(r => r.EmployeeName)
            .ToList();
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