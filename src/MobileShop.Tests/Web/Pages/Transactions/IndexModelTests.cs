using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using MobileShop.Services.PDF;
using MobileShop.Web.Pages.Transactions;
using Moq;

namespace MobileShop.Tests.Web.Pages.Transactions;

/// <summary>Verifies downloadable transaction factors from the transaction list.</summary>
public class IndexModelTests : RepoTestBase
{
    private static readonly byte[] PdfBytes = [0x25, 0x50, 0x44, 0x46];

    private readonly IndexModel _model;
    private readonly Mock<IPdfGenerator> _pdfGeneratorMock;
    private readonly Transaction _tx1;
    private readonly Transaction _tx2;
    private readonly Transaction _tx3;
    private TransactionFactorViewModel? _generatedFactor;

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

        _pdfGeneratorMock = new Mock<IPdfGenerator>();
        _pdfGeneratorMock
            .Setup(generator => generator.GenerateTransactionFactor(It.IsAny<TransactionFactorViewModel>()))
            .Callback<TransactionFactorViewModel>(factor => _generatedFactor = factor)
            .Returns(PdfBytes);

        var transactionService = new TransactionDataService(
            new TransactionRepo(Context),
            NullLogger<TransactionDataService>.Instance,
            _pdfGeneratorMock.Object);

        _model = new IndexModel(transactionService, _pdfGeneratorMock.Object);
    }

    [Fact]
    public async Task Manual_selection_returns_pdf_attachment_for_exact_transactions_in_submitted_order()
    {
        _model.SelectedIds = [_tx2.Id, _tx1.Id];

        var result = await _model.OnGetDownloadFactorAsync();

        var file = Assert.IsType<FileContentResult>(result);
        Assert.Equal("application/pdf", file.ContentType);
        Assert.Equal("transactions-factor.pdf", file.FileDownloadName);
        Assert.Equal(PdfBytes, file.FileContents);
        Assert.NotNull(_generatedFactor);
        Assert.Equal([_tx2.Id, _tx1.Id], _generatedFactor.Rows.Select(row => row.TransactionId));
        Assert.Equal(310m, _generatedFactor.TotalPrice);
        _pdfGeneratorMock.Verify(
            generator => generator.GenerateTransactionFactor(It.IsAny<TransactionFactorViewModel>()),
            Times.Once);
    }

    [Fact]
    public async Task Duplicate_selected_identifiers_are_deduplicated_in_download()
    {
        _model.SelectedIds = [_tx1.Id, _tx1.Id, _tx1.Id];

        var result = await _model.OnGetDownloadFactorAsync();

        Assert.IsType<FileContentResult>(result);
        Assert.NotNull(_generatedFactor);
        Assert.Equal([_tx1.Id], _generatedFactor.Rows.Select(row => row.TransactionId));
    }

    [Fact]
    public async Task Non_positive_selected_ids_are_rejected_without_generating_pdf()
    {
        _model.SelectedIds = [-1, 0, _tx1.Id];

        var result = await _model.OnGetDownloadFactorAsync();

        Assert.IsType<PageResult>(result);
        Assert.False(_model.ModelState.IsValid);
        Assert.Contains(_model.ModelState[string.Empty]!.Errors, error => error.ErrorMessage.Contains("positive"));
        _pdfGeneratorMock.Verify(
            generator => generator.GenerateTransactionFactor(It.IsAny<TransactionFactorViewModel>()),
            Times.Never);
    }

    [Fact]
    public async Task Missing_selected_id_triggers_error_without_generating_partial_pdf()
    {
        _model.SelectedIds = [_tx1.Id, 99_999];

        var result = await _model.OnGetDownloadFactorAsync(direction: "all", take: 20, order: "desc");

        Assert.IsType<PageResult>(result);
        Assert.False(_model.ModelState.IsValid);
        Assert.Contains(_model.ModelState[string.Empty]!.Errors, error => error.ErrorMessage.Contains("99999"));
        Assert.Equal([_tx1.Id, 99_999], _model.SelectedIds);
        Assert.Equal("all", _model.Direction);
        Assert.Equal(20, _model.Take);
        Assert.NotEmpty(_model.Transactions);
        _pdfGeneratorMock.Verify(
            generator => generator.GenerateTransactionFactor(It.IsAny<TransactionFactorViewModel>()),
            Times.Never);
    }

    [Fact]
    public async Task Empty_selection_downloads_filtered_ordered_and_limited_snapshot()
    {
        var result = await _model.OnGetDownloadFactorAsync(direction: "buy", take: 1, order: "asc");

        Assert.IsType<FileContentResult>(result);
        Assert.Equal("buy", _model.Direction);
        Assert.Equal(1, _model.Take);
        Assert.Equal("asc", _model.Order);
        Assert.NotNull(_generatedFactor);
        Assert.Equal([_tx1.Id], _generatedFactor.Rows.Select(row => row.TransactionId));
    }

    [Fact]
    public async Task Empty_result_set_shows_validation_error_without_generating_pdf()
    {
        var transactionService = new Mock<ITransactionDataService>();
        transactionService.Setup(service => service.GetListAsync("sell", 50, false))
            .ReturnsAsync([]);
        var model = new IndexModel(transactionService.Object, _pdfGeneratorMock.Object);

        var result = await model.OnGetDownloadFactorAsync(direction: "sell");

        Assert.IsType<PageResult>(result);
        Assert.False(model.ModelState.IsValid);
        Assert.Contains(
            model.ModelState[string.Empty]!.Errors,
            error => error.ErrorMessage.Contains("No transactions match"));
        _pdfGeneratorMock.Verify(
            generator => generator.GenerateTransactionFactor(It.IsAny<TransactionFactorViewModel>()),
            Times.Never);
    }

    [Fact]
    public async Task Selected_ids_from_loaded_snapshot_are_resolved_without_detail_queries()
    {
        var mockService = new Mock<ITransactionDataService>();
        var tx1 = new TransactionListItemViewModel(
            1, DateTime.UtcNow, TransactionDirection.Buy, "Phone A", 100m, "Shop", "Shop");
        var tx2 = new TransactionListItemViewModel(
            2, DateTime.UtcNow, TransactionDirection.Sell, "Phone B", 200m, "Seller", "Customer");
        mockService.Setup(service => service.GetListAsync("all", 50, false))
            .ReturnsAsync([tx1, tx2]);
        mockService.Setup(service => service.GetDetailsAsync(It.IsAny<int>()))
            .Throws(new InvalidOperationException("GetDetailsAsync should not be called"));

        var model = new IndexModel(mockService.Object, _pdfGeneratorMock.Object);
        model.SelectedIds = [1, 2];

        var result = await model.OnGetDownloadFactorAsync();

        Assert.IsType<FileContentResult>(result);
        Assert.NotNull(_generatedFactor);
        Assert.Equal([1, 2], _generatedFactor.Rows.Select(row => row.TransactionId));
        mockService.Verify(service => service.GetDetailsAsync(It.IsAny<int>()), Times.Never);
    }

    [Fact]
    public async Task Selected_ids_absent_from_snapshot_are_rejected_without_pdf_generation()
    {
        var mockService = new Mock<ITransactionDataService>();
        var tx1 = new TransactionListItemViewModel(
            1, DateTime.UtcNow, TransactionDirection.Buy, "Phone A", 100m, "Shop", "Shop");
        mockService.Setup(service => service.GetListAsync("all", 50, false))
            .ReturnsAsync([tx1]);
        var model = new IndexModel(mockService.Object, _pdfGeneratorMock.Object)
        {
            SelectedIds = [1, 999]
        };

        var result = await model.OnGetDownloadFactorAsync();

        Assert.IsType<PageResult>(result);
        Assert.False(model.ModelState.IsValid);
        _pdfGeneratorMock.Verify(
            generator => generator.GenerateTransactionFactor(It.IsAny<TransactionFactorViewModel>()),
            Times.Never);
    }
}
