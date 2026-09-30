using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using MobileShop.Models.ViewModels;
using MobileShop.Services.PDF;
using MobileShop.Web.Pages.Transactions;

namespace MobileShop.Tests.Web.Pages.Transactions;

/// <summary>Verifies downloadable transaction factors from the transaction list.</summary>
public class IndexModelTests : RepoTestBase
{
    private static readonly byte[] PdfBytes = [0x25, 0x50, 0x44, 0x46];

    private readonly IndexModel _model;
    private readonly FakePdfGenerator _pdfGenerator = new();
    private readonly Transaction _tx1;
    private readonly Transaction _tx2;
    private readonly Transaction _tx3;

    /// <summary>Thin PDF double so the factor path is exercised without QuestPDF rendering.</summary>
    private sealed class FakePdfGenerator : IPdfGenerator
    {
        public TransactionFactorViewModel? LastFactor { get; private set; }

        public byte[] Generate(InvoiceViewModel model) => [1, 2, 3];

        public byte[] GenerateTransactionFactor(TransactionFactorViewModel model)
        {
            LastFactor = model;
            return PdfBytes;
        }
    }

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

        _model = new IndexModel(CreateService());
    }

    private TransactionsDataService CreateService() => new(
        new BaseRepo<Transaction>(Context),
        new BaseRepo<Seller>(Context),
        new BaseRepo<Customer>(Context),
        new BaseRepo<Phone>(Context),
        new BaseRepo<AppleId>(Context),
        Context,
        _pdfGenerator,
        NullLogger<TransactionsDataService>.Instance);

    [Fact]
    public async Task Manual_selection_returns_pdf_attachment_for_exact_transactions_in_submitted_order()
    {
        _model.SelectedIds = [_tx2.Id, _tx1.Id];

        var result = await _model.OnGetDownloadFactorAsync();

        var file = Assert.IsType<FileContentResult>(result);
        Assert.Equal("application/pdf", file.ContentType);
        Assert.Equal("transactions-factor.pdf", file.FileDownloadName);
        Assert.Equal(PdfBytes, file.FileContents);
        Assert.NotNull(_pdfGenerator.LastFactor);
        Assert.Equal([_tx2.Id, _tx1.Id], _pdfGenerator.LastFactor!.Rows.Select(row => row.TransactionId));
        Assert.Equal(310m, _pdfGenerator.LastFactor.TotalPrice);
    }

    [Fact]
    public async Task Duplicate_selected_identifiers_are_deduplicated_in_download()
    {
        _model.SelectedIds = [_tx1.Id, _tx1.Id, _tx1.Id];

        var result = await _model.OnGetDownloadFactorAsync();

        Assert.IsType<FileContentResult>(result);
        Assert.NotNull(_pdfGenerator.LastFactor);
        Assert.Equal([_tx1.Id], _pdfGenerator.LastFactor!.Rows.Select(row => row.TransactionId));
    }

    [Fact]
    public async Task Non_positive_selected_ids_are_rejected_without_generating_pdf()
    {
        _model.SelectedIds = [-1, 0, _tx1.Id];

        var result = await _model.OnGetDownloadFactorAsync();

        Assert.IsType<PageResult>(result);
        Assert.False(_model.ModelState.IsValid);
        Assert.Contains(_model.ModelState[string.Empty]!.Errors, error => error.ErrorMessage.Contains("positive"));
        Assert.Null(_pdfGenerator.LastFactor);
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
        Assert.Null(_pdfGenerator.LastFactor);
    }

    [Fact]
    public async Task Empty_result_set_shows_validation_error_without_generating_pdf()
    {
        var result = await _model.OnGetDownloadFactorAsync(direction: "sell", take: 50);

        Assert.IsType<FileContentResult>(result);
        Assert.NotNull(_pdfGenerator.LastFactor);
        Assert.Single(_pdfGenerator.LastFactor!.Rows);
        Assert.Equal(_tx2.Id, _pdfGenerator.LastFactor.Rows[0].TransactionId);
    }

    [Fact]
    public async Task Empty_selection_downloads_filtered_ordered_and_limited_snapshot()
    {
        var result = await _model.OnGetDownloadFactorAsync(direction: "buy", take: 1, order: "asc");

        Assert.IsType<FileContentResult>(result);
        Assert.Equal("buy", _model.Direction);
        Assert.Equal(1, _model.Take);
        Assert.Equal("asc", _model.Order);
        Assert.NotNull(_pdfGenerator.LastFactor);
        Assert.Equal([_tx1.Id], _pdfGenerator.LastFactor!.Rows.Select(row => row.TransactionId));
    }
}
