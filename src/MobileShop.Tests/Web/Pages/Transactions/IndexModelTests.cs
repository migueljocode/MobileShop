using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using MobileShop.Web.Pages.Transactions;
using Moq;

namespace MobileShop.Tests.Web.Pages.Transactions;

/// <summary>
/// Verifies the transaction list page Print Factor behavior:
/// - Manual selection includes exactly the chosen records and enables print mode.
/// - Duplicate selected IDs are de-duplicated.
/// - Non-positive IDs are rejected with a validation error.
/// - Non-existent IDs trigger explicit error and redisplay without print mode.
/// - Empty selection falls back to the direction/count/order filters.
/// - Handler inputs (direction, take, order, selectedIds) are bound.
/// - Selection order is preserved.
/// - Print mode state and rows are composed correctly.
/// - Query-string filters and selectedIds survive validation failure.
/// </summary>
public class IndexModelTests : RepoTestBase
{
    private readonly IndexModel _model;
    private readonly Transaction _tx1;
    private readonly Transaction _tx2;
    private readonly Transaction _tx3;

    public IndexModelTests()
    {
        var sellerPerson = new Person { FirstName = "Ali", LastName = "Seller", PhoneNumber = "09120000011" };
        var customerPerson = new Person { FirstName = "Sara", LastName = "Customer", PhoneNumber = "09120000012" };
        Context.People.AddRange(sellerPerson, customerPerson);
        Context.SaveChanges();

        var seller = new Seller { PersonId = sellerPerson.Id, EntityType = SellerEntityType.Real };
        var customer = new Customer { PersonId = customerPerson.Id, NationalId = "1234567890" };
        Context.Sellers.Add(seller);
        Context.Customers.Add(customer);
        Context.SaveChanges();

        var p1 = TestDataHelpers.CreateProduct(Context, 100m);
        var p2 = TestDataHelpers.CreateProduct(Context, 200m);
        var p3 = TestDataHelpers.CreateProduct(Context, 300m);

        _tx1 = new Transaction
        {
            ProductId = p1.Id,
            SellerId = seller.Id,
            CustomerId = 1,
            FinishedPrice = 90m,
            Date = new DateTime(2026, 1, 1, 0, 0, 0, DateTimeKind.Utc),
            Direction = TransactionDirection.Buy
        };
        _tx2 = new Transaction
        {
            ProductId = p2.Id,
            SellerId = 1,
            CustomerId = customer.Id,
            FinishedPrice = 220m,
            Date = new DateTime(2026, 1, 2, 0, 0, 0, DateTimeKind.Utc),
            Direction = TransactionDirection.Sell
        };
        _tx3 = new Transaction
        {
            ProductId = p3.Id,
            SellerId = seller.Id,
            CustomerId = 1,
            FinishedPrice = 270m,
            Date = new DateTime(2026, 1, 3, 0, 0, 0, DateTimeKind.Utc),
            Direction = TransactionDirection.Buy
        };
        Context.Transactions.AddRange(_tx1, _tx2, _tx3);
        Context.SaveChanges();

        var transactionService = new TransactionDataService(
            new TransactionRepo(Context),
            NullLogger<TransactionDataService>.Instance,
            Mock.Of<MobileShop.Services.PDF.IPdfGenerator>());

        _model = new IndexModel(transactionService);
    }

    [Fact]
    public async Task Manual_selection_enables_print_mode_with_exactly_those_transactions()
    {
        _model.SelectedIds = [_tx1.Id, _tx2.Id];

        var result = await _model.OnGetPrintFactorAsync();

        Assert.IsType<PageResult>(result);
        Assert.True(_model.IsPrintMode);
        Assert.NotNull(_model.PrintFactorRows);
        Assert.Equal(2, _model.PrintFactorRows.Count);
        Assert.Contains(_model.PrintFactorRows, r => r.TransactionId == _tx1.Id && r.PersonRole == "Seller" && r.PersonLabel == "Ali Seller");
        Assert.Contains(_model.PrintFactorRows, r => r.TransactionId == _tx2.Id && r.PersonRole == "Customer" && r.PersonLabel == "Sara Customer");
        Assert.DoesNotContain(_model.PrintFactorRows, r => r.TransactionId == _tx3.Id);
        Assert.Equal(310m, _model.PrintFactorRows.Sum(r => r.FinishedPrice));
    }

    [Fact]
    public async Task Duplicate_selected_identifiers_are_deduplicated_in_print_mode()
    {
        _model.SelectedIds = [_tx1.Id, _tx1.Id, _tx1.Id];

        await _model.OnGetPrintFactorAsync();

        Assert.True(_model.IsPrintMode);
        Assert.Single(_model.PrintFactorRows);
        Assert.Equal(_tx1.Id, _model.PrintFactorRows[0].TransactionId);
    }

    [Fact]
    public async Task Non_positive_selected_ids_are_rejected_without_print_mode()
    {
        _model.SelectedIds = [-1, 0, _tx1.Id];

        var result = await _model.OnGetPrintFactorAsync();

        Assert.IsType<PageResult>(result);
        Assert.False(_model.IsPrintMode);
        Assert.Empty(_model.PrintFactorRows);
        Assert.False(_model.ModelState.IsValid);
        Assert.Contains(
            _model.ModelState[string.Empty]!.Errors,
            e => e.ErrorMessage.Contains("positive"));
    }

    [Fact]
    public async Task Missing_selected_id_triggers_explicit_error_and_redisplays_without_print_mode()
    {
        _model.SelectedIds = [_tx1.Id, 99_999];

        var result = await _model.OnGetPrintFactorAsync(direction: "all", take: 20, order: "desc");

        Assert.IsType<PageResult>(result);
        Assert.False(_model.IsPrintMode);
        Assert.Empty(_model.PrintFactorRows);
        Assert.False(_model.ModelState.IsValid);
        Assert.Contains(
            _model.ModelState[string.Empty]!.Errors,
            e => e.ErrorMessage.Contains("99999"));

        Assert.Equal([_tx1.Id, 99_999], _model.SelectedIds);
        Assert.Equal("all", _model.Direction);
        Assert.Equal(20, _model.Take);
        Assert.NotEmpty(_model.Transactions);
    }

    [Fact]
    public async Task Empty_selection_uses_direction_and_take_filters_in_print_mode()
    {
        _model.SelectedIds = [];

        await _model.OnGetPrintFactorAsync(direction: "sell", take: 10, order: "desc");

        Assert.True(_model.IsPrintMode);
        Assert.NotNull(_model.PrintFactorRows);
        Assert.Single(_model.PrintFactorRows);
        Assert.Equal(_tx2.Id, _model.PrintFactorRows[0].TransactionId);
        Assert.Equal(TransactionDirection.Sell, _model.PrintFactorRows[0].Direction);
    }

    [Fact]
    public async Task Handler_inputs_direction_take_order_are_bound()
    {
        _model.SelectedIds = [_tx1.Id];

        await _model.OnGetPrintFactorAsync(direction: "buy", take: 7, order: "asc");

        Assert.Equal("buy", _model.Direction);
        Assert.Equal(7, _model.Take);
        Assert.Equal("asc", _model.Order);
        Assert.True(_model.IsPrintMode);
    }

    [Fact]
    public async Task Selected_ids_preserve_submitted_order_in_print_mode()
    {
        _model.SelectedIds = [_tx2.Id, _tx1.Id];

        await _model.OnGetPrintFactorAsync();

        Assert.True(_model.IsPrintMode);
        Assert.Equal(2, _model.PrintFactorRows.Count);
        Assert.Equal(_tx2.Id, _model.PrintFactorRows[0].TransactionId);
        Assert.Equal(_tx1.Id, _model.PrintFactorRows[1].TransactionId);
    }

    [Fact]
    public async Task Print_mode_state_is_set_and_rows_are_composed_from_snapshot()
    {
        _model.SelectedIds = [_tx1.Id, _tx2.Id, _tx3.Id];

        var result = await _model.OnGetPrintFactorAsync(direction: "all", take: 50, order: "desc");

        Assert.True(_model.IsPrintMode);
        Assert.Equal(3, _model.PrintFactorRows.Count);
        foreach (var row in _model.PrintFactorRows)
        {
            Assert.NotEqual(default, row.Date);
            Assert.NotEqual(default, row.ProductLabel);
            Assert.NotEqual(default, row.PersonRole);
            Assert.NotEqual(default, row.PersonLabel);
        }
    }

    [Fact]
    public async Task Selected_ids_from_loaded_snapshot_are_resolved_without_individual_get_details_calls()
    {
        var mockService = new Mock<ITransactionDataService>();
        var tx1 = new TransactionListItemViewModel(
            1, DateTime.UtcNow, TransactionDirection.Buy, "Phone A", 100m, "Shop", "Shop");
        var tx2 = new TransactionListItemViewModel(
            2, DateTime.UtcNow, TransactionDirection.Sell, "Phone B", 200m, "Seller", "Customer");
        mockService.Setup(s => s.GetListAsync("all", 50, false))
            .ReturnsAsync([tx1, tx2]);
        mockService.Setup(s => s.GetDetailsAsync(It.IsAny<int>()))
            .Throws(new InvalidOperationException("GetDetailsAsync should not be called when IDs are in the snapshot"));

        var model = new IndexModel(mockService.Object);
        model.SelectedIds = [1, 2];

        var result = await model.OnGetPrintFactorAsync();

        Assert.True(model.IsPrintMode);
        Assert.Equal(2, model.PrintFactorRows.Count);
        Assert.Contains(model.PrintFactorRows, r => r.TransactionId == 1);
        Assert.Contains(model.PrintFactorRows, r => r.TransactionId == 2);

        mockService.Verify(s => s.GetDetailsAsync(It.IsAny<int>()), Times.Never);
    }

    [Fact]
    public async Task Selected_ids_absent_from_snapshot_are_reported_as_missing_without_get_details_calls()
    {
        var mockService = new Mock<ITransactionDataService>();
        var tx1 = new TransactionListItemViewModel(
            1, DateTime.UtcNow, TransactionDirection.Buy, "Phone A", 100m, "Shop", "Shop");
        var tx2 = new TransactionListItemViewModel(
            2, DateTime.UtcNow, TransactionDirection.Sell, "Phone B", 200m, "Seller", "Customer");
        mockService.Setup(s => s.GetListAsync("all", 50, false))
            .ReturnsAsync([tx1, tx2]);
        mockService.Setup(s => s.GetDetailsAsync(It.IsAny<int>()))
            .Throws(new InvalidOperationException("GetDetailsAsync should not be called"));

        var model = new IndexModel(mockService.Object);
        model.SelectedIds = [1, 999];

        var result = await model.OnGetPrintFactorAsync();

        Assert.IsType<PageResult>(result);
        Assert.False(model.IsPrintMode);
        Assert.Empty(model.PrintFactorRows);
        Assert.False(model.ModelState.IsValid);

        mockService.Verify(s => s.GetDetailsAsync(It.IsAny<int>()), Times.Never);
    }
}
