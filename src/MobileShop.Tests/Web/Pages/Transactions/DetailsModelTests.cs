using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using MobileShop.Web.Pages.Transactions;
using MobileShop.Services.PDF;
using Moq;

namespace MobileShop.Tests.Web.Pages.Transactions;

/// <summary>
/// Verifies the factor action on transaction details: generates a factor PDF for exactly that one
/// transaction when found, otherwise returns NotFound.
/// </summary>
public class DetailsModelTests : RepoTestBase
{
    private readonly DetailsModel _model;
    private readonly Mock<IPdfGenerator> _pdfGeneratorMock;

    public DetailsModelTests()
    {
        _pdfGeneratorMock = new Mock<IPdfGenerator>();
        _pdfGeneratorMock
            .Setup(p => p.GenerateTransactionFactor(It.IsAny<TransactionFactorViewModel>()))
            .Returns([0x25, 0x50, 0x44, 0x46]);

        var transactionService = new TransactionDataService(
            new TransactionRepo(Context),
            NullLogger<TransactionDataService>.Instance,
            _pdfGeneratorMock.Object);

        _model = new DetailsModel(transactionService, _pdfGeneratorMock.Object);
    }

    [Fact]
    public async Task OnGetFactor_returns_printable_pdf_for_the_requested_transaction()
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

        TransactionFactorViewModel? captured = null;
        _pdfGeneratorMock
            .Setup(p => p.GenerateTransactionFactor(It.IsAny<TransactionFactorViewModel>()))
            .Callback<TransactionFactorViewModel>(m => captured = m)
            .Returns([0x25, 0x50, 0x44, 0x46]);

        var result = await _model.OnGetFactorAsync(transaction.Id);

        var fileResult = Assert.IsType<FileContentResult>(result);
        Assert.Equal("application/pdf", fileResult.ContentType);
        Assert.True(string.IsNullOrEmpty(fileResult.FileDownloadName)); // inline / printable

        Assert.NotNull(captured);
        Assert.Single(captured!.Rows);
        var row = captured.Rows[0];
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
        _pdfGeneratorMock.Verify(p => p.GenerateTransactionFactor(It.IsAny<TransactionFactorViewModel>()), Times.Never);
    }
}