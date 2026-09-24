using Microsoft.Extensions.Options;
using MobileShop.Services.PDF;
using MobileShop.Services.PDF.Configuration;
using MobileShop.Services.PDF.Settings;
using MobileShop.Models.ViewModels;
using Moq;

namespace MobileShop.Tests.PDF;

public class QuestPdfGeneratorTests
{
    private readonly QuestPdfGenerator _generator;
    private readonly Mock<IOptions<PdfSettings>> _optionsMock;

    public QuestPdfGeneratorTests()
    {
        _optionsMock = new Mock<IOptions<PdfSettings>>();
        _optionsMock.Setup(o => o.Value).Returns(new PdfSettings
        {
            ShopName = "Test Shop",
            PageSize = "A4",
            MarginTop = 25,
            MarginRight = 25,
            MarginBottom = 25,
            MarginLeft = 25
        });
        _generator = new QuestPdfGenerator(_optionsMock.Object);
    }

    [Fact]
    public void Generate_returns_pdf_bytes_for_valid_invoice()
    {
        var model = new InvoiceViewModel(
            BuyerName: "John Doe",
            BuyerNationalId: "1234567890",
            BuyerPhoneNumber: "09120000001",
            SellerName: null,
            SellerPhoneNumber: null,
            TransactionDate: DateTime.UtcNow,
            FinishedPrice: 1000m,
            ProductCount: 1,
            ProductInformation: "iPhone 15",
            ProductExtras: [],
            GuaranteeInformation: [],
            OwnershipTransferred: true,
            Notes: "Test invoice"
        );

        var pdfBytes = _generator.Generate(model);

        Assert.NotNull(pdfBytes);
        Assert.NotEmpty(pdfBytes);
        Assert.Equal(0x25, pdfBytes[0]);
        Assert.Equal(0x50, pdfBytes[1]);
        Assert.Equal(0x44, pdfBytes[2]);
        Assert.Equal(0x46, pdfBytes[3]);
    }

    [Fact]
    public void Generate_throws_for_null_model()
    {
        Assert.Throws<ArgumentNullException>(() => _generator.Generate(null!));
    }

    [Fact(Skip = "Persian generation requires Vazirmatn font which may need commercial license")]
    public void GeneratePersian_returns_pdf_bytes_for_valid_invoice()
    {
        var model = new InvoiceViewModel(
            BuyerName: "علی احمدی",
            BuyerNationalId: "1234567890",
            BuyerPhoneNumber: "09120000001",
            SellerName: null,
            SellerPhoneNumber: null,
            TransactionDate: DateTime.UtcNow,
            FinishedPrice: 1000m,
            ProductCount: 1,
            ProductInformation: "آیفون ۱۵",
            ProductExtras: [],
            GuaranteeInformation: [],
            OwnershipTransferred: true,
            Notes: "فاکتور تست"
        );

        var pdfBytes = _generator.GeneratePersian(model);

        Assert.NotNull(pdfBytes);
        Assert.NotEmpty(pdfBytes);
        Assert.Equal(0x25, pdfBytes[0]);
        Assert.Equal(0x50, pdfBytes[1]);
        Assert.Equal(0x44, pdfBytes[2]);
        Assert.Equal(0x46, pdfBytes[3]);
    }

    [Fact(Skip = "Persian generation requires Vazirmatn font which may need commercial license")]
    public void GeneratePersian_throws_for_null_model()
    {
        Assert.Throws<ArgumentNullException>(() => _generator.GeneratePersian(null!));
    }
}
