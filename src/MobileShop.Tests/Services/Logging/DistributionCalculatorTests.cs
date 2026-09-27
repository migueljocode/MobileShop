using MobileShop.Services.Logging.Settings;
using Xunit;

namespace MobileShop.Tests.Services.Logging;

/// <summary>
/// Verifies the fixed three-row profit distribution:
/// Mikaeeil Jorjany (40%), Anis Sahabi (50%), Shop (10%).
/// </summary>
public class DistributionCalculatorTests
{
    private static Employee MakeEmployee(int id, string firstName, string lastName, int share, bool isActive)
        => new Employee
        {
            Id = id,
            PersonNavigation = new Person { Id = id + 100, FirstName = firstName, LastName = lastName },
            SharePercent = share,
            IsActive = isActive,
            HireDate = DateTime.UtcNow
        };

    private static List<Employee> CreateValidEmployees(params (int id, string first, string last, int share, bool active)[] specs)
        => specs.Select(s => MakeEmployee(s.id, s.first, s.last, s.share, s.active)).ToList();

    private static List<Employee> CreateDefaultEmployees()
        => CreateValidEmployees(
            (1, "Mikaeeil", "Jorjany", 40, true),
            (2, "Anis", "Sahabi", 50, true),
            (3, "Ali", "Rezaei", 30, true) // unrelated - must be excluded from output
        );

    private readonly DistributionSettings _settings = new();

    // ── Profit case ────────────────────────────────────────────

    [Fact]
    public void Profit_ReturnsExactlyThreeRowsWithFixedSharesAndFloorRounding()
    {
        var employees = CreateDefaultEmployees();
        // 101 profit: floor(101 * 0.40) = 40, floor(101 * 0.50) = 50, Shop = 101 - 40 - 50 = 11
        var rows = DistributionCalculator.Calculate(101m, employees, _settings);

        Assert.Equal(3, rows.Count);

        // Verify exact row names — sorted by SharePercent desc: Anis (50), Mikaeeil (40), Shop (10)
        Assert.Equal("Anis Sahabi", rows[0].EmployeeName);
        Assert.Equal("Mikaeeil Jorjany", rows[1].EmployeeName);
        Assert.Equal("Shop", rows[2].EmployeeName);

        // Verify fixed shares sum to 100%
        Assert.Equal(50, rows[0].SharePercent);
        Assert.Equal(40, rows[1].SharePercent);
        Assert.Equal(10, rows[2].SharePercent);
        Assert.Equal(100, rows.Sum(r => r.SharePercent));

        // Verify amounts with floor rounding
        Assert.Equal(50m, rows[0].CalculatedAmount);
        Assert.Equal(40m, rows[1].CalculatedAmount);
        Assert.Equal(11m, rows[2].CalculatedAmount);

        // Shop gets remainder
        Assert.Equal(101m, rows[0].CalculatedAmount + rows[1].CalculatedAmount + rows[2].CalculatedAmount);

        // Not a loss period
        Assert.All(rows, r => Assert.False(r.IsLossPeriod));
    }

    [Fact]
    public void Profit_SharePercentAlways40_50_10EvenWhenEmployeesHaveDifferentShares()
    {
        // Employee shares in the data don't matter — calculator overrides to fixed 40/50/10
        var employees = CreateValidEmployees(
            (1, "Mikaeeil", "Jorjany", 90, true),
            (2, "Anis", "Sahabi", 10, true)
        );

        var rows = DistributionCalculator.Calculate(100m, employees, _settings);

        Assert.Equal(3, rows.Count);
        Assert.Equal(50, rows[0].SharePercent);
        Assert.Equal(40, rows[1].SharePercent);
        Assert.Equal(10, rows[2].SharePercent);
        Assert.Equal(100m, rows.Sum(r => r.CalculatedAmount)); // 40 + 50 + 10 = 100
    }

    // ── Zero and loss cases ────────────────────────────────────

    [Fact]
    public void ZeroProfit_ReturnsThreeRowsWithZeroAmountsAndLossFlag()
    {
        var employees = CreateDefaultEmployees();
        var rows = DistributionCalculator.Calculate(0m, employees, _settings);

        Assert.Equal(3, rows.Count);
        // Sorted by SharePercent desc: Anis (50), Mikaeeil (40), Shop (10)
        Assert.Equal("Anis Sahabi", rows[0].EmployeeName);
        Assert.Equal("Mikaeeil Jorjany", rows[1].EmployeeName);
        Assert.Equal("Shop", rows[2].EmployeeName);

        Assert.Equal(50, rows[0].SharePercent);
        Assert.Equal(40, rows[1].SharePercent);
        Assert.Equal(10, rows[2].SharePercent);

        Assert.All(rows, r => Assert.Equal(0m, r.CalculatedAmount));
        Assert.All(rows, r => Assert.True(r.IsLossPeriod));

        // Sum still equals total (0)
        Assert.Equal(0m, rows.Sum(r => r.CalculatedAmount));
    }

    [Fact]
    public void Loss_ReturnsThreeRowsWithEmployeesZeroShopGetsFullLoss()
    {
        var employees = CreateDefaultEmployees();
        var loss = -50000m;

        var rows = DistributionCalculator.Calculate(loss, employees, _settings);

        Assert.Equal(3, rows.Count);
        // Sorted by SharePercent desc: Anis (50), Mikaeeil (40), Shop (10)
        Assert.Equal("Anis Sahabi", rows[0].EmployeeName);
        Assert.Equal("Mikaeeil Jorjany", rows[1].EmployeeName);
        Assert.Equal("Shop", rows[2].EmployeeName);

        Assert.Equal(50, rows[0].SharePercent);
        Assert.Equal(40, rows[1].SharePercent);
        Assert.Equal(10, rows[2].SharePercent);

        // Employees get nothing, shop absorbs entire loss
        Assert.Equal(0m, rows[0].CalculatedAmount);
        Assert.Equal(0m, rows[1].CalculatedAmount);
        Assert.Equal(loss, rows[2].CalculatedAmount);

        Assert.All(rows, r => Assert.True(r.IsLossPeriod));
        Assert.Equal(loss, rows.Sum(r => r.CalculatedAmount));
    }

    // ── Validation: missing required employees ─────────────────

    [Fact]
    public void MissingMikaeeil_ThrowsExplicitException()
    {
        // Only Anis present — Mikaeeil missing entirely
        var employees = CreateValidEmployees(
            (2, "Anis", "Sahabi", 50, true),
            (3, "Ali", "Rezaei", 30, true)
        );

        var ex = Assert.Throws<InvalidOperationException>(() =>
            DistributionCalculator.Calculate(1000m, employees, _settings));
        Assert.Contains("Mikaeeil Jorjany", ex.Message);
    }

    [Fact]
    public void MissingAnis_ThrowsExplicitException()
    {
        // Only Mikaeeil present — Anis missing entirely
        var employees = CreateValidEmployees(
            (1, "Mikaeeil", "Jorjany", 40, true),
            (3, "Ali", "Rezaei", 30, true)
        );

        var ex = Assert.Throws<InvalidOperationException>(() =>
            DistributionCalculator.Calculate(1000m, employees, _settings));
        Assert.Contains("Anis Sahabi", ex.Message);
    }

    // ── Validation: inactive matches do not satisfy requirement ─

    [Fact]
    public void InactiveMikaeeil_DoesNotSatisfyRequirement()
    {
        var employees = CreateValidEmployees(
            (1, "Mikaeeil", "Jorjany", 40, false), // inactive
            (2, "Anis", "Sahabi", 50, true)
        );

        var ex = Assert.Throws<InvalidOperationException>(() =>
            DistributionCalculator.Calculate(1000m, employees, _settings));
        Assert.Contains("Mikaeeil Jorjany", ex.Message);
    }

    [Fact]
    public void InactiveAnis_DoesNotSatisfyRequirement()
    {
        var employees = CreateValidEmployees(
            (1, "Mikaeeil", "Jorjany", 40, true),
            (2, "Anis", "Sahabi", 50, false) // inactive
        );

        var ex = Assert.Throws<InvalidOperationException>(() =>
            DistributionCalculator.Calculate(1000m, employees, _settings));
        Assert.Contains("Anis Sahabi", ex.Message);
    }

    // ── Validation: duplicates ─────────────────────────────────

    [Fact]
    public void DuplicateMikaeeil_ThrowsExplicitException()
    {
        var employees = CreateValidEmployees(
            (1, "Mikaeeil", "Jorjany", 40, true),
            (2, "Mikaeeil", "Jorjany", 30, true), // duplicate active
            (3, "Anis", "Sahabi", 50, true)
        );

        var ex = Assert.Throws<InvalidOperationException>(() =>
            DistributionCalculator.Calculate(1000m, employees, _settings));
        Assert.Contains("Mikaeeil Jorjany", ex.Message);
    }

    [Fact]
    public void DuplicateAnis_ThrowsExplicitException()
    {
        var employees = CreateValidEmployees(
            (1, "Mikaeeil", "Jorjany", 40, true),
            (2, "Anis", "Sahabi", 50, true),
            (3, "Anis", "Sahabi", 20, true) // duplicate active
        );

        var ex = Assert.Throws<InvalidOperationException>(() =>
            DistributionCalculator.Calculate(1000m, employees, _settings));
        Assert.Contains("Anis Sahabi", ex.Message);
    }

    // ── Exclusion ──────────────────────────────────────────────

    [Fact]
    public void UnrelatedActiveEmployeesAreExcludedFromOutput()
    {
        // Many active employees with different names — only Mikaeeil, Anis, and Shop appear
        var employees = CreateValidEmployees(
            (1, "Mikaeeil", "Jorjany", 40, true),
            (2, "Anis", "Sahabi", 50, true),
            (3, "Ali", "Rezaei", 30, true),
            (4, "Sara", "Ahmadi", 25, true),
            (5, "Reza", "Mousavi", 15, true)
        );

        var rows = DistributionCalculator.Calculate(1000m, employees, _settings);

        Assert.Equal(3, rows.Count); // only 3 rows, not 5+
        Assert.Contains(rows, r => r.EmployeeName == "Mikaeeil Jorjany");
        Assert.Contains(rows, r => r.EmployeeName == "Anis Sahabi");
        Assert.Contains(rows, r => r.EmployeeName == "Shop");
        Assert.DoesNotContain(rows, r => r.EmployeeName == "Ali Rezaei");
        Assert.DoesNotContain(rows, r => r.EmployeeName == "Sara Ahmadi");
        Assert.DoesNotContain(rows, r => r.EmployeeName == "Reza Mousavi");
    }
}
