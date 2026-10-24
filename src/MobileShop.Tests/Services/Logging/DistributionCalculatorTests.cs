namespace MobileShop.Tests.Services.Logging;

/// <summary>
/// Verifies the profit distribution driven by each active employee's stored share.
/// The shop row always takes the remainder: 100 − the sum of the active shares.
/// </summary>
public class DistributionCalculatorTests
{
    private static Employee MakeEmployee(int id, string firstName, string lastName, int share, bool isActive = true)
        => new Employee
        {
            Id = id,
            PersonNavigation = new Person { Id = id + 100, FirstName = firstName, LastName = lastName },
            SharePercent = share,
            IsActive = isActive,
            HireDate = DateTime.UtcNow
        };

    private static List<Employee> Employees(params Employee[] employees) => employees.ToList();

    private static Employee Mikaeeil(int share) => MakeEmployee(1, "Mikaeeil", "Jorjany", share);

    private static Employee Anis(int share) => MakeEmployee(2, "Anis", "Sahabi", share);

    // ── Profit case ────────────────────────────────────────────

    [Fact]
    public void Profit_SplitsByStoredSharesWithFloorRoundingAndShopRemainder()
    {
        // floor(1001 × 40%) = 400, floor(1001 × 50%) = 500, shop = 1001 − 900 = 101 (10%)
        var rows = DistributionCalculator.Calculate(1001, Employees(Mikaeeil(40), Anis(50)));

        Assert.Equal(3, rows.Count);
        Assert.Equal(new[] { "Anis Sahabi", "Mikaeeil Jorjany", "Shop" }, rows.Select(r => r.EmployeeName));
        Assert.Equal(new[] { 50, 40, 10 }, rows.Select(r => r.SharePercent));
        Assert.Equal(new long[] { 500, 400, 101 }, rows.Select(r => r.CalculatedAmount));
        Assert.Equal(1001, rows.Sum(r => r.CalculatedAmount));
        Assert.All(rows, r => Assert.False(r.IsLossPeriod));
    }

    [Fact]
    public void Profit_ShopGetsRemainderWhenActiveSharesLeaveRoom()
    {
        var rows = DistributionCalculator.Calculate(100, Employees(MakeEmployee(1, "Ali", "Rezaei", 30), MakeEmployee(2, "Sara", "Ahmadi", 20)));

        var shop = rows.Single(r => r.EmployeeName == "Shop");
        Assert.Equal(50, shop.SharePercent);
        Assert.Equal(50, shop.CalculatedAmount);
        Assert.Equal(30, rows.Single(r => r.EmployeeName == "Ali Rezaei").CalculatedAmount);
        Assert.Equal(20, rows.Single(r => r.EmployeeName == "Sara Ahmadi").CalculatedAmount);
    }

    [Fact]
    public void Profit_SharesTotallingExactly100_LeaveShopWithZero()
    {
        var rows = DistributionCalculator.Calculate(1000, Employees(Mikaeeil(60), Anis(40)));

        var shop = rows.Single(r => r.EmployeeName == "Shop");
        Assert.Equal(0, shop.SharePercent);
        Assert.Equal(0, shop.CalculatedAmount);
        Assert.Equal(1000, rows.Sum(r => r.CalculatedAmount));
    }

    [Fact]
    public void Profit_SharesAboveOneHundredInTotal_Throws()
    {
        var employees = Employees(Mikaeeil(60), Anis(41));

        var ex = Assert.Throws<InvalidOperationException>(() => DistributionCalculator.Calculate(1000, employees));

        Assert.Equal("Active employee shares total 101%, which exceeds 100%.", ex.Message);
    }

    [Theory]
    [InlineData(-1)]
    [InlineData(101)]
    public void Profit_ShareOutsideZeroToHundred_Throws(int share)
    {
        var employees = Employees(Mikaeeil(share));

        Assert.Throws<InvalidOperationException>(() => DistributionCalculator.Calculate(1000, employees));
    }

    [Fact]
    public void Profit_HugeProfitWithoutOverflow_MatchesExactFloor()
    {
        var expected = (long)(new BigInteger(long.MaxValue) * 40 / 100);

        var rows = DistributionCalculator.Calculate(long.MaxValue, Employees(Mikaeeil(40)));

        var employee = rows.Single(r => r.EmployeeName == "Mikaeeil Jorjany");
        Assert.Equal(expected, employee.CalculatedAmount);
        Assert.Equal(long.MaxValue, rows.Sum(r => r.CalculatedAmount));
    }

    // ── Zero and loss cases ────────────────────────────────────

    [Fact]
    public void ZeroProfit_ReturnsZeroAmountsAndLossFlag()
    {
        var rows = DistributionCalculator.Calculate(0, Employees(Mikaeeil(40), Anis(50)));

        Assert.Equal(3, rows.Count);
        Assert.All(rows, r => Assert.Equal(0, r.CalculatedAmount));
        Assert.All(rows, r => Assert.True(r.IsLossPeriod));
        Assert.Equal(new[] { 50, 40, 10 }, rows.Select(r => r.SharePercent));
    }

    [Fact]
    public void Loss_EmployeesGetZeroAndShopAbsorbsWholeLoss()
    {
        var loss = -50000;

        var rows = DistributionCalculator.Calculate(loss, Employees(Mikaeeil(40), Anis(50)));

        Assert.Equal(0, rows.Single(r => r.EmployeeName == "Mikaeeil Jorjany").CalculatedAmount);
        Assert.Equal(0, rows.Single(r => r.EmployeeName == "Anis Sahabi").CalculatedAmount);
        Assert.Equal(loss, rows.Single(r => r.EmployeeName == "Shop").CalculatedAmount);
        Assert.All(rows, r => Assert.True(r.IsLossPeriod));
        Assert.Equal(loss, rows.Sum(r => r.CalculatedAmount));
    }

    // ── Employee selection and ordering ────────────────────────

    [Fact]
    public void NoActiveEmployees_ReturnsSingleShopRowAt100Percent()
    {
        var rows = DistributionCalculator.Calculate(1000, Employees());

        var shop = Assert.Single(rows);
        Assert.Equal("Shop", shop.EmployeeName);
        Assert.Equal(0, shop.EmployeeId);
        Assert.Equal(100, shop.SharePercent);
        Assert.Equal(1000, shop.CalculatedAmount);
    }

    [Fact]
    public void InactiveEmployee_IsIgnoredAndItsShareDoesNotCount()
    {
        var inactive = MakeEmployee(3, "Mikaeeil", "Jorjany", 40, isActive: false);

        var rows = DistributionCalculator.Calculate(1000, Employees(inactive, Anis(50)));

        Assert.Equal(2, rows.Count);
        Assert.DoesNotContain(rows, r => r.EmployeeName == "Mikaeeil Jorjany");
        Assert.Equal(50, rows.Single(r => r.EmployeeName == "Shop").SharePercent);
    }

    [Fact]
    public void ZeroShareEmployee_IsStillListedWithZeroAmount()
    {
        var rows = DistributionCalculator.Calculate(1000, Employees(Mikaeeil(0), Anis(50)));

        var row = rows.Single(r => r.EmployeeName == "Mikaeeil Jorjany");
        Assert.Equal(0, row.SharePercent);
        Assert.Equal(0, row.CalculatedAmount);
    }

    [Fact]
    public void Rows_OrderedBySharePercentDescendingThenName()
    {
        var rows = DistributionCalculator.Calculate(1000, Employees(
            MakeEmployee(1, "Zed", "Zadeh", 20),
            MakeEmployee(2, "Ali", "Zadeh", 20),
            MakeEmployee(3, "Mina", "Karimi", 30)));

        // Shop is 100 − 70 = 30%, tying with Mina; ties break by name.
        Assert.Equal(new[] { "Mina Karimi", "Shop", "Ali Zadeh", "Zed Zadeh" }, rows.Select(r => r.EmployeeName));
    }
}
