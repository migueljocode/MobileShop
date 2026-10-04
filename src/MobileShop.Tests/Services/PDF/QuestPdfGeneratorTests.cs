using MobileShop.Services.PDF.Settings;
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
            FinishedPrice: 1000,
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
            FinishedPrice: 1000,
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
                new TransactionFactorRowViewModel(1, DateTime.UtcNow, TransactionDirection.Buy, "iPhone 13", 42_000_000, "Seller", "Ali"),
                new TransactionFactorRowViewModel(2, DateTime.UtcNow, TransactionDirection.Sell, "iPhone 13", 45_000_000, "Customer", "Sara")
            ],
            DateTime.UtcNow);

        Assert.Equal(87000000, model.TotalPrice);
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
                123_456_789,
                "Seller",
                "Ali Seller"));

        Assert.Equal(123456789, model.TotalPrice);
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
                987_654_322,
                "Customer",
                "سارا احمدی رضایی با نام خانوادگی طولانی و اطلاعات تکمیلی"));

        AssertValidPdf(_generator.GenerateTransactionFactor(model));
    }

    [Fact]
    public void GenerateTransactionFactor_supports_empty_factor()
    {
        var model = Factor();

        Assert.Equal(0, model.TotalPrice);
        AssertValidPdf(_generator.GenerateTransactionFactor(model));
    }

    [Fact]
    public void GenerateTransactionFactor_writes_rendered_inspection_artifacts()
    {
        var outputDirectory = Environment.GetEnvironmentVariable("FACTOR_PDF_INSPECTION_DIR");
        if (string.IsNullOrWhiteSpace(outputDirectory))
            return;

        Directory.CreateDirectory(outputDirectory);

        var single = Factor(
            new TransactionFactorRowViewModel(
                1,
                new(2026, 1, 1, 10, 30, 0, DateTimeKind.Utc),
                TransactionDirection.Sell,
                "اپل آیفون ۱۷ پرو مکس ۵۱۲ گیگابایت Titanium Desert",
                987_654_322,
                "Customer",
                "سارا احمدی رضایی با نام خانوادگی طولانی و اطلاعات تکمیلی"));

        var selected = Factor(
            Enumerable.Range(1, 12)
                .Select(id => new TransactionFactorRowViewModel(
                    id,
                    new(2026, 1, id, 10, 30, 0, DateTimeKind.Utc),
                    id % 2 == 0 ? TransactionDirection.Sell : TransactionDirection.Buy,
                    id % 2 == 0
                        ? "Apple iPhone 17 Pro Max 512GB Titanium Desert"
                        : "سامسونگ گلکسی S26 اولترا با حافظه ۱ ترابایت",
                    120_000_000 + id * 1_234_568,
                    id % 2 == 0 ? "Customer" : "Seller",
                    id % 2 == 0
                        ? "سارا احمدی رضایی"
                        : "علی محمدی فروشنده با نام طولانی"))
                .ToArray());

        File.WriteAllBytes(
            Path.Combine(outputDirectory, "single-persian-factor.pdf"),
            _generator.GenerateTransactionFactor(single));

        File.WriteAllBytes(
            Path.Combine(outputDirectory, "selected-mixed-factor.pdf"),
            _generator.GenerateTransactionFactor(selected));
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