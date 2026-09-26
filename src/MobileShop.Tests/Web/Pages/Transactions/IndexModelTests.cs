using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using MobileShop.Web.Pages.Transactions;
using MobileShop.Services.PDF;
using Moq;

namespace MobileShop.Tests.Web.Pages.Transactions;

/// <summary>
/// Verifies the transaction list page factor generation:
/// - Manual selection includes exactly the chosen records.
/// - Duplicate selected IDs are de-duplicated.
/// - Non-existent IDs trigger explicit error and redisplay without generating PDF.
/// - Empty selection falls back to the direction/count/order filters.
/// - Print returns inline/printable response; Download returns named attachment.
/// - Query-string filters and selectedIds survive validation failures.
/// </summary>
public class IndexModelTests : RepoTestBase
{
    private readonly IndexModel _model;
    private readonly Mock<IPdfGenerator> _pdfGeneratorMock;
    private readonly Transaction _tx1;
    private readonly Transaction _tx2;
    private readonly Transaction _tx3;

    public IndexModelTests()
    {
        _pdfGeneratorMock = new Mock<IPdfGenerator>();
        _pdfGeneratorMock
            .Setup(p => p.GenerateTransactionFactor(It.IsAny<TransactionFactorViewModel>()))
            .Returns([0x25, 0x50, 0x44, 0x46]);

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
            _pdfGeneratorMock.Object);

        _model = new IndexModel(transactionService, _pdfGeneratorMock.Object);
    }

    [Fact]
    public async Task Manual_selection_generates_factor_for_exactly_those_transactions()
    {
        _model.SelectedIds = [_tx1.Id, _tx2.Id];
        TransactionFactorViewModel? captured = null;
        _pdfGeneratorMock
            .Setup(p => p.GenerateTransactionFactor(It.IsAny<TransactionFactorViewModel>()))
            .Callback<TransactionFactorViewModel>(m => captured = m)
            .Returns([0x25, 0x50, 0x44, 0x46]);

        var result = await _model.OnGetPrintAsync();

        var fileResult = Assert.IsType<FileContentResult>(result);
        Assert.Equal("application/pdf", fileResult.ContentType);
        Assert.True(string.IsNullOrEmpty(fileResult.FileDownloadName)); // inline / printable

        Assert.NotNull(captured);
        Assert.Equal(2, captured!.Rows.Count);
        Assert.Contains(captured.Rows, r => r.TransactionId == _tx1.Id && r.PersonRole == "Seller" && r.PersonLabel == "Ali Seller");
        Assert.Contains(captured.Rows, r => r.TransactionId == _tx2.Id && r.PersonRole == "Customer" && r.PersonLabel == "Sara Customer");
        Assert.DoesNotContain(captured.Rows, r => r.TransactionId == _tx3.Id);
        Assert.Equal(310m, captured.TotalPrice);
    }

    [Fact]
    public async Task Duplicate_selected_identifiers_are_deduplicated()
    {
        _model.SelectedIds = [_tx1.Id, _tx1.Id, _tx1.Id];
        TransactionFactorViewModel? captured = null;
        _pdfGeneratorMock
            .Setup(p => p.GenerateTransactionFactor(It.IsAny<TransactionFactorViewModel>()))
            .Callback<TransactionFactorViewModel>(m => captured = m)
            .Returns([0x25, 0x50, 0x44, 0x46]);

        await _model.OnGetPrintAsync();

        Assert.NotNull(captured);
        Assert.Single(captured!.Rows);
        Assert.Equal(_tx1.Id, captured.Rows[0].TransactionId);
    }

    [Fact]
    public async Task Missing_selected_id_triggers_explicit_error_and_redisplays()
    {
        _model.SelectedIds = [_tx1.Id, 99_999];

        var result = await _model.OnGetPrintAsync(direction: "all", take: 20, order: "desc");

        Assert.IsType<PageResult>(result);
        Assert.False(_model.ModelState.IsValid);
        Assert.Contains(
            _model.ModelState[string.Empty]!.Errors,
            e => e.ErrorMessage.Contains("99999"));

        _pdfGeneratorMock.Verify(p => p.GenerateTransactionFactor(It.IsAny<TransactionFactorViewModel>()), Times.Never);

        Assert.Equal([_tx1.Id, 99_999], _model.SelectedIds);
        Assert.Equal("all", _model.Direction);
        Assert.Equal(20, _model.Take);
        Assert.NotEmpty(_model.Transactions);
    }

    [Fact]
    public async Task Empty_selection_uses_direction_and_take_filters()
    {
        _model.SelectedIds = [];
        TransactionFactorViewModel? captured = null;
        _pdfGeneratorMock
            .Setup(p => p.GenerateTransactionFactor(It.IsAny<TransactionFactorViewModel>()))
            .Callback<TransactionFactorViewModel>(m => captured = m)
            .Returns([0x25, 0x50, 0x44, 0x46]);

        await _model.OnGetPrintAsync(direction: "sell", take: 10, order: "desc");

        Assert.NotNull(captured);
        Assert.Single(captured!.Rows);
        Assert.Equal(_tx2.Id, captured.Rows[0].TransactionId);
        Assert.Equal(TransactionDirection.Sell, captured.Rows[0].Direction);
    }

    [Fact]
    public async Task Download_returns_named_pdf_file_response()
    {
        _model.SelectedIds = [_tx1.Id];

        var result = await _model.OnGetDownloadAsync();

        var fileResult = Assert.IsType<FileContentResult>(result);
        Assert.Equal("application/pdf", fileResult.ContentType);
        Assert.NotNull(fileResult.FileDownloadName);
        Assert.StartsWith("transactions-factor-", fileResult.FileDownloadName);
        Assert.EndsWith(".pdf", fileResult.FileDownloadName);
    }

    [Fact]
    public async Task Selected_ids_from_loaded_snapshot_are_resolved_without_individual_get_details_calls()
    {
        // Arrange: mock service where GetListAsync returns a list with known IDs,
        // and GetDetailsAsync is set up to fail if ever called (proving it is NOT needed
        // when all selected IDs are already in the snapshot).
        var mockService = new Mock<ITransactionDataService>();
        var tx1 = new TransactionListItemViewModel(
            1, DateTime.UtcNow, TransactionDirection.Buy, "Phone A", 100m, "Shop", "Shop");
        var tx2 = new TransactionListItemViewModel(
            2, DateTime.UtcNow, TransactionDirection.Sell, "Phone B", 200m, "Seller", "Customer");
        mockService.Setup(s => s.GetListAsync("all", 50, false))
            .ReturnsAsync([tx1, tx2]);
        mockService.Setup(s => s.GetDetailsAsync(It.IsAny<int>()))
            .Throws(new InvalidOperationException("GetDetailsAsync should not be called when IDs are in the snapshot"));

        var pdfMock = new Mock<IPdfGenerator>();
        TransactionFactorViewModel? captured = null;
        pdfMock.Setup(p => p.GenerateTransactionFactor(It.IsAny<TransactionFactorViewModel>()))
            .Callback<TransactionFactorViewModel>(m => captured = m)
            .Returns([0x25, 0x50, 0x44, 0x46]);

        var model = new IndexModel(mockService.Object, pdfMock.Object);
        model.SelectedIds = [1, 2];

        // Act
        var result = await model.OnGetPrintAsync();

        // Assert: PDF generated from the snapshot, GetDetailsAsync never called
        var fileResult = Assert.IsType<FileContentResult>(result);
        Assert.Equal("application/pdf", fileResult.ContentType);

        Assert.NotNull(captured);
        Assert.Equal(2, captured!.Rows.Count);
        Assert.Contains(captured.Rows, r => r.TransactionId == 1);
        Assert.Contains(captured.Rows, r => r.TransactionId == 2);

        mockService.Verify(s => s.GetDetailsAsync(It.IsAny<int>()), Times.Never);
    }

    [Fact]
    public async Task Selected_ids_absent_from_snapshot_are_reported_as_missing_without_get_details_calls()
    {
        // Arrange: snapshot contains IDs 1 and 2, but the selection also includes
        // ID 999 which is not in the snapshot. The missing ID should be reported
        // via a model error — GetDetailsAsync must never be called.
        var mockService = new Mock<ITransactionDataService>();
        var tx1 = new TransactionListItemViewModel(
            1, DateTime.UtcNow, TransactionDirection.Buy, "Phone A", 100m, "Shop", "Shop");
        var tx2 = new TransactionListItemViewModel(
            2, DateTime.UtcNow, TransactionDirection.Sell, "Phone B", 200m, "Seller", "Customer");
        mockService.Setup(s => s.GetListAsync("all", 50, false))
            .ReturnsAsync([tx1, tx2]);
        mockService.Setup(s => s.GetDetailsAsync(It.IsAny<int>()))
            .Throws(new InvalidOperationException("GetDetailsAsync should not be called"));

        var pdfMock = new Mock<IPdfGenerator>();

        var model = new IndexModel(mockService.Object, pdfMock.Object);
        model.SelectedIds = [1, 999]; // ID 999 is not in the snapshot

        // Act
        var result = await model.OnGetPrintAsync();

        // Assert: PDF is NOT generated; the page is redisplayed with a model error
        Assert.IsType<PageResult>(result);
        Assert.False(model.ModelState.IsValid);

        // GetDetailsAsync must never be called — even for the missing ID
        mockService.Verify(s => s.GetDetailsAsync(It.IsAny<int>()), Times.Never);

        // PDF generator must never be called
        pdfMock.Verify(p => p.GenerateTransactionFactor(It.IsAny<TransactionFactorViewModel>()), Times.Never);
    }
}
