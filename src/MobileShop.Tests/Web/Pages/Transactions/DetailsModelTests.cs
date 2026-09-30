using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using MobileShop.Models.ViewModels;
using MobileShop.Services.PDF;
using MobileShop.Web.Pages.Transactions;

namespace MobileShop.Tests.Web.Pages.Transactions;

/// <summary>
/// Verifies the factor action on transaction details: generates a factor PDF for exactly that one
/// transaction when found, otherwise returns NotFound.
/// </summary>
public class DetailsModelTests : RepoTestBase
{
    private static readonly byte[] PdfBytes = [0x25, 0x50, 0x44, 0x46];

    private readonly DetailsModel _model;
    private readonly FakePdfGenerator _pdfGenerator = new();

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

    public DetailsModelTests()
    {
        _model = new DetailsModel(new TransactionsDataService(
            new BaseRepo<Transaction>(Context),
            new BaseRepo<Seller>(Context),
            new BaseRepo<Customer>(Context),
            new BaseRepo<Phone>(Context),
            new BaseRepo<AppleId>(Context),
            Context,
            _pdfGenerator,
            NullLogger<TransactionsDataService>.Instance));
    }

    private Transaction SeedTransaction()
    {
        TestDataHelpers.SeedShopSentinels(Context);

        var sellerPerson = new Person { FirstName = "Details", LastName = "Seller", PhoneNumber = "09120000010" };
        Context.People.Add(sellerPerson);
        Context.SaveChanges();

        var seller = new Seller { PersonId = sellerPerson.Id, EntityType = SellerEntityType.Real };
        Context.Sellers.Add(seller);
        Context.SaveChanges();

        var product = TestDataHelpers.CreateProduct(Context, 200m);
        var transaction = new Transaction
        {
            ProductId = product.Id,
            SellerId = seller.Id,
            CustomerId = 1,
            FinishedPrice = 180m,
            Date = new DateTime(2026, 2, 1, 10, 0, 0, DateTimeKind.Utc),
            Direction = TransactionDirection.Buy
        };
        Context.Transactions.Add(transaction);
        Context.SaveChanges();
        return transaction;
    }

    [Fact]
    public async Task OnGetFactor_returns_printable_pdf_for_the_requested_transaction()
    {
        var transaction = SeedTransaction();

        var result = await _model.OnGetFactorAsync(transaction.Id);

        var fileResult = Assert.IsType<FileContentResult>(result);
        Assert.Equal("application/pdf", fileResult.ContentType);
        Assert.True(string.IsNullOrEmpty(fileResult.FileDownloadName)); // inline / printable
        Assert.Equal(PdfBytes, fileResult.FileContents);

        Assert.NotNull(_pdfGenerator.LastFactor);
        Assert.Single(_pdfGenerator.LastFactor!.Rows);
        var row = _pdfGenerator.LastFactor.Rows[0];
        Assert.Equal(transaction.Id, row.TransactionId);
        Assert.Equal(180m, row.FinishedPrice);
        Assert.Equal("Seller", row.PersonRole);
        Assert.Equal("Details Seller", row.PersonLabel);
    }

    [Fact]
    public async Task OnGetFactor_returns_not_found_when_transaction_missing()
    {
        var result = await _model.OnGetFactorAsync(99_999);

        Assert.IsType<NotFoundResult>(result);
        Assert.Null(_pdfGenerator.LastFactor);
    }

    [Fact]
    public async Task OnGet_shows_details_and_returns_not_found_for_missing_transaction()
    {
        var transaction = SeedTransaction();

        var page = await _model.OnGetAsync(transaction.Id);

        Assert.IsType<PageResult>(page);
        Assert.NotNull(_model.Transaction);
        Assert.Equal(180m, _model.Transaction!.FinishedPrice);
        Assert.IsType<NotFoundResult>(await _model.OnGetAsync(99_999));
    }
}
