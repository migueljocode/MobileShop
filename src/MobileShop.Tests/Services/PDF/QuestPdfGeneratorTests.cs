using Microsoft.Extensions.Options;
using MobileShop.Services.PDF;
using MobileShop.Services.PDF.Configuration;
using MobileShop.Services.PDF.Settings;
using MobileShop.Models.ViewModels;
using Moq;

namespace MobileShop.Tests.Services.PDF;

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
            ShopAddress = "Test Address",
            ShopPhone = "09120000000",
            ShopInstagram = "@testshop",
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

        AssertValidPdf(_generator.Generate(model));
    }

    [Fact]
    public void Generate_throws_for_null_model()
        => Assert.Throws<ArgumentNullException>(() => _generator.Generate(null!));

    [Fact]
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

        AssertValidPdf(_generator.GeneratePersian(model));
    }

    [Fact]
    public void GeneratePersian_throws_for_null_model()
        => Assert.Throws<ArgumentNullException>(() => _generator.GeneratePersian(null!));

    [Fact]
    public void GenerateTransactionFactor_returns_valid_pdf_payload()
    {
        var model = new TransactionFactorViewModel(
            [
                new TransactionFactorRowViewModel(1, DateTime.UtcNow, TransactionDirection.Buy, "iPhone 13", 42_000_000m, "Seller", "Ali"),
                new TransactionFactorRowViewModel(2, DateTime.UtcNow, TransactionDirection.Sell, "iPhone 13", 45_000_000m, "Customer", "Sara")
            ],
            DateTime.UtcNow);

        Assert.Equal(87_000_000m, model.TotalPrice);
        AssertValidPdf(_generator.GenerateTransactionFactor(model));
    }

    [Fact]
    public void GenerateTransactionFactor_supports_one_row_and_exact_total()
    {
        var model = Factor(
            new TransactionFactorRowViewModel(
                1,
                new(2026, 1, 1, 10, 30, 0, DateTimeKind.Utc),
                TransactionDirection.Buy,
                "iPhone 17 Pro",
                123_456_789.45m,
                "Seller",
                "Ali Seller"));

        Assert.Equal(123_456_789.45m, model.TotalPrice);
        AssertValidPdf(_generator.GenerateTransactionFactor(model));
    }

    [Fact]
    public void GenerateTransactionFactor_supports_persian_rtl_and_long_text()
    {
        var model = Factor(
            new TransactionFactorRowViewModel(
                1,
                DateTime.UtcNow,
                TransactionDirection.Sell,
                "اپل آیفون ۱۷ پرو مکس با حافظه ۵۱۲ گیگابایت Titanium Desert",
                987_654_321.99m,
                "Customer",
                "سارا احمدی رضایی با نام خانوادگی طولانی و اطلاعات تکمیلی"));

        AssertValidPdf(_generator.GenerateTransactionFactor(model));
    }

    [Fact]
    public void GenerateTransactionFactor_supports_empty_factor()
    {
        var model = Factor();

        Assert.Equal(0m, model.TotalPrice);
        AssertValidPdf(_generator.GenerateTransactionFactor(model));
    }

    [Fact]
    public void GenerateTransactionFactor_throws_for_null_model()
        => Assert.Throws<ArgumentNullException>(() => _generator.GenerateTransactionFactor(null!));

    private static TransactionFactorViewModel Factor(params TransactionFactorRowViewModel[] rows)
        => new(rows, DateTime.UtcNow);

    private static void AssertValidPdf(byte[] bytes)
    {
        Assert.NotNull(bytes);
        Assert.NotEmpty(bytes);
        Assert.Equal("%PDF"u8.ToArray(), bytes[..4]);
    }
}
