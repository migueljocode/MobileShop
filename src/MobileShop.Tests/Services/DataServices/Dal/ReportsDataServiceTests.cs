using MobileShop.Services.Logging.Settings;
using Microsoft.Extensions.Options;

namespace MobileShop.Tests.Services.DataServices.Dal;

public class ReportsDataServiceTests : RepoTestBase
{
    private readonly ReportsDataService _service;

    public ReportsDataServiceTests()
    {
        _service = new ReportsDataService(
            new BaseRepo<Transaction>(Context),
            new BaseRepo<Employee>(Context),
            Options.Create(new DistributionSettings()),
            NullLogger<ReportsDataService>.Instance);
    }

    private Seller AddSeller()
    {
        var person = new Person { FirstName = "Ali", LastName = "Zed", PhoneNumber = "09120000031" };
        Context.People.Add(person);
        Context.SaveChanges();

        var seller = new Seller { PersonId = person.Id, EntityType = SellerEntityType.Real };
        Context.Sellers.Add(seller);
        Context.SaveChanges();
        return seller;
    }

    private Customer AddCustomer()
    {
        var person = new Person { FirstName = "Sara", LastName = "Ahmadi", PhoneNumber = "09120000032" };
        Context.People.Add(person);
        Context.SaveChanges();

        var customer = new Customer { PersonId = person.Id, NationalId = "1234567891" };
        Context.Customers.Add(customer);
        Context.SaveChanges();
        return customer;
    }

    private Transaction AddTransaction(
        Product product,
        int sellerId,
        int customerId,
        TransactionDirection direction,
        DateTime date,
        decimal price)
    {
        var transaction = new Transaction
        {
            ProductId = product.Id,
            SellerId = sellerId,
            CustomerId = customerId,
            FinishedPrice = price,
            Date = date,
            Direction = direction,
        };
        Context.Transactions.Add(transaction);
        Context.SaveChanges();
        return transaction;
    }

    private Employee AddEmployee(string firstName, string lastName, bool isActive = true)
    {
        var person = new Person { FirstName = firstName, LastName = lastName, PhoneNumber = "09120000033" };
        Context.People.Add(person);
        Context.SaveChanges();

        var employee = new Employee { PersonId = person.Id, IsActive = isActive };
        Context.Employees.Add(employee);
        Context.SaveChanges();
        return employee;
    }

    [Fact]
    public async Task GetProfitLossRowsAsync_groups_by_product_with_buy_and_sell_sums()
    {
        var seller = AddSeller();
        var customer = AddCustomer();
        var product = TestDataHelpers.CreateProduct(Context);

        AddTransaction(product, seller.Id, customer.Id, TransactionDirection.Buy, new DateTime(2024, 3, 1), 300m);
        AddTransaction(product, seller.Id, customer.Id, TransactionDirection.Sell, new DateTime(2024, 3, 5), 500m);

        var rows = await _service.GetProfitLossRowsAsync(null, null);

        var row = Assert.Single(rows);
        Assert.Equal(product.Id, row.ProductId);
        Assert.Equal(300m, row.Bought);
        Assert.Equal(500m, row.Sold);
        Assert.Equal(200m, row.Profit);
    }

    [Fact]
    public async Task GetProfitLossRowsAsync_uses_glass_paid_price_when_glass_was_sold_without_buy_transaction()
    {
        var seller = AddSeller();
        var customer = AddCustomer();
        var product = TestDataHelpers.CreateProduct(Context);
        product.Price = 300m;
        Context.SaveChanges();

        var glass = new Glass { ProductId = product.Id };
        Context.Glasses.Add(glass);
        Context.SaveChanges();

        AddTransaction(product, seller.Id, customer.Id, TransactionDirection.Sell, new DateTime(2024, 3, 5), 500m);

        var rows = await _service.GetProfitLossRowsAsync(null, null);

        var row = Assert.Single(rows);
        Assert.Equal(300m, row.Bought);
        Assert.Equal(500m, row.Sold);
        Assert.Equal(200m, row.Profit);
    }

    [Fact]
    public async Task GetProfitLossRowsAsync_filters_inclusively_on_date()
    {
        var seller = AddSeller();
        var customer = AddCustomer();
        var inside = TestDataHelpers.CreateProduct(Context);
        var before = TestDataHelpers.CreateProduct(Context);
        var after = TestDataHelpers.CreateProduct(Context);

        AddTransaction(inside, seller.Id, customer.Id, TransactionDirection.Sell, new DateTime(2024, 3, 10), 100m);
        AddTransaction(before, seller.Id, customer.Id, TransactionDirection.Sell, new DateTime(2024, 2, 28), 900m);
        AddTransaction(after, seller.Id, customer.Id, TransactionDirection.Sell, new DateTime(2024, 3, 12), 900m);

        var rows = await _service.GetProfitLossRowsAsync(
            new DateTime(2024, 3, 1),
            new DateTime(2024, 3, 11));

        var row = Assert.Single(rows);
        Assert.Equal(inside.Id, row.ProductId);
        Assert.Equal(100m, row.Sold);
    }

    [Fact]
    public async Task GetProfitLossRowsAsync_returns_empty_for_range_without_transactions()
    {
        var seller = AddSeller();
        var customer = AddCustomer();
        var product = TestDataHelpers.CreateProduct(Context);
        AddTransaction(product, seller.Id, customer.Id, TransactionDirection.Sell, new DateTime(2024, 3, 10), 100m);

        var rows = await _service.GetProfitLossRowsAsync(
            new DateTime(2025, 1, 1),
            new DateTime(2025, 12, 31));

        Assert.Empty(rows);
        Assert.Equal(0m, await _service.GetProfitLossTotalAsync(
            new DateTime(2025, 1, 1),
            new DateTime(2025, 12, 31)));
    }

    [Fact]
    public async Task GetProfitLossTotalAsync_sums_sold_minus_bought_in_range()
    {
        var seller = AddSeller();
        var customer = AddCustomer();
        var profitable = TestDataHelpers.CreateProduct(Context);
        var losing = TestDataHelpers.CreateProduct(Context);

        AddTransaction(profitable, seller.Id, customer.Id, TransactionDirection.Buy, new DateTime(2024, 4, 1), 100m);
        AddTransaction(profitable, seller.Id, customer.Id, TransactionDirection.Sell, new DateTime(2024, 4, 2), 350m);
        AddTransaction(losing, seller.Id, customer.Id, TransactionDirection.Buy, new DateTime(2024, 4, 3), 200m);

        var total = await _service.GetProfitLossTotalAsync(new DateTime(2024, 4, 1), new DateTime(2024, 4, 30));

        Assert.Equal(50m, total);
    }

    [Fact]
    public async Task GetEarliestTransactionDateAsync_returns_null_when_no_transactions()
    {
        Assert.Null(await _service.GetEarliestTransactionDateAsync());
    }

    [Fact]
    public async Task GetEarliestTransactionDateAsync_returns_date_only_of_earliest_transaction()
    {
        var seller = AddSeller();
        var customer = AddCustomer();
        var product = TestDataHelpers.CreateProduct(Context);
        AddTransaction(product, seller.Id, customer.Id, TransactionDirection.Sell, new DateTime(2024, 6, 15, 14, 30, 0), 100m);
        AddTransaction(product, seller.Id, customer.Id, TransactionDirection.Sell, new DateTime(2024, 7, 1, 9, 0, 0), 100m);

        var earliest = await _service.GetEarliestTransactionDateAsync();

        Assert.NotNull(earliest);
        Assert.Equal(new DateTime(2024, 6, 15), earliest!.Value);
    }

    [Fact]
    public async Task GetDistributionRowsAsync_returns_fixed_40_50_10_split_for_positive_profit()
    {
        var mikaeeil = AddEmployee("Mikaeeil", "Jorjany");
        var anis = AddEmployee("Anis", "Sahabi");

        var rows = await _service.GetDistributionRowsAsync(1000m);

        Assert.Equal(3, rows.Count);
        Assert.Equal(400m, rows.Single(row => row.EmployeeId == mikaeeil.Id).CalculatedAmount);
        Assert.Equal(500m, rows.Single(row => row.EmployeeId == anis.Id).CalculatedAmount);

        var shop = rows.Single(row => row.EmployeeName == "Shop");
        Assert.Equal(100m, shop.CalculatedAmount);
        Assert.Equal(10, shop.SharePercent);
        Assert.All(rows, row => Assert.False(row.IsLossPeriod));
    }

    [Fact]
    public async Task GetDistributionRowsAsync_reports_loss_period_with_zero_payouts()
    {
        AddEmployee("Mikaeeil", "Jorjany");
        AddEmployee("Anis", "Sahabi");

        var rows = await _service.GetDistributionRowsAsync(-250m);

        Assert.Equal(3, rows.Count);
        Assert.All(rows, row => Assert.True(row.IsLossPeriod));
        Assert.Equal(-250m, rows.Single(row => row.EmployeeName == "Shop").CalculatedAmount);
        Assert.Equal(0m, rows.Single(row => row.EmployeeName == "Mikaeeil Jorjany").CalculatedAmount);
    }

    [Fact]
    public async Task GetDistributionRowsAsync_throws_when_required_employee_missing()
    {
        AddEmployee("Anis", "Sahabi");

        await Assert.ThrowsAsync<InvalidOperationException>(
            () => _service.GetDistributionRowsAsync(100m));
    }

    [Fact]
    public async Task GetEarliestTransactionDateAsync_ignores_soft_deleted_transactions()
    {
        var seller = AddSeller();
        var customer = AddCustomer();
        var product = TestDataHelpers.CreateProduct(Context);

        var deleted = AddTransaction(product, seller.Id, customer.Id, TransactionDirection.Sell, new DateTime(2024, 1, 1), 50m);
        deleted.IsDeleted = true;
        Context.SaveChanges();

        AddTransaction(product, seller.Id, customer.Id, TransactionDirection.Sell, new DateTime(2024, 5, 20), 50m);

        var earliest = await _service.GetEarliestTransactionDateAsync();

        Assert.NotNull(earliest);
        Assert.Equal(new DateTime(2024, 5, 20), earliest!.Value);
    }

    [Fact]
    public async Task GetDistributionRowsAsync_ignores_inactive_required_employees()
    {
        AddEmployee("Mikaeeil", "Jorjany", isActive: false);
        AddEmployee("Anis", "Sahabi");

        await Assert.ThrowsAsync<InvalidOperationException>(
            () => _service.GetDistributionRowsAsync(100m));
    }
}
