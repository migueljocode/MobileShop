using MobileShop.Models.ViewModels;
using MobileShop.Services.DataServices.Dal;
using MobileShop.Services.PDF;
using Moq;

namespace MobileShop.Tests.Services.DataServices.Dal;

public class InvoiceDataServiceTests : RepoTestBase
{
    private readonly InvoiceDataService _service;
    private readonly Mock<IPdfGenerator> _pdfGeneratorMock;

    public InvoiceDataServiceTests()
    {
        _pdfGeneratorMock = new Mock<IPdfGenerator>();
        _pdfGeneratorMock.Setup(p => p.Generate(It.IsAny<InvoiceViewModel>())).Returns(new byte[] { 0x25, 0x50, 0x44, 0x46 });
        _service = new InvoiceDataService(Context, _pdfGeneratorMock.Object);
    }

    [Fact]
    public void GetInvoice_returns_sale_invoice_for_sell_transaction()
    {
        var sellerPerson = new Person { FirstName = "Seller", LastName = "One", PhoneNumber = "09120000001" };
        Context.People.Add(sellerPerson);
        Context.SaveChanges();

        var seller = new Seller { PersonId = sellerPerson.Id, EntityType = SellerEntityType.Real };
        Context.Sellers.Add(seller);
        Context.SaveChanges();

        var customerPerson = new Person { FirstName = "Customer", LastName = "One", PhoneNumber = "09120000002" };
        Context.People.Add(customerPerson);
        Context.SaveChanges();

        var customer = new Customer { PersonId = customerPerson.Id, NationalId = "1000000001" };
        Context.Customers.Add(customer);
        Context.SaveChanges();

        var product = TestDataHelpers.CreateProduct(Context, 300m);
        Context.SaveChanges();

        var transaction = new Transaction
        {
            ProductId = product.Id,
            ProductNavigation = product,
            SellerId = seller.Id,
            SellerNavigation = seller,
            CustomerId = customer.Id,
            CustomerNavigation = customer,
            FinishedPrice = 300m,
            Date = DateTime.UtcNow,
            Direction = TransactionDirection.Sell
        };
        Context.Transactions.Add(transaction);
        Context.SaveChanges();

        var invoice = _service.GetInvoice(transaction.Id);

        Assert.NotNull(invoice);
        Assert.Equal("Customer One", invoice!.BuyerName);
        Assert.Equal("1000000001", invoice.BuyerNationalId);
        Assert.Equal("09120000002", invoice.BuyerPhoneNumber);
        Assert.Null(invoice.SellerName);
        Assert.Null(invoice.SellerPhoneNumber);
    }

    [Fact]
    public void GetInvoice_returns_buy_invoice_for_buy_transaction()
    {
        var sellerPerson = new Person { FirstName = "Seller", LastName = "Two", PhoneNumber = "09120000003" };
        Context.People.Add(sellerPerson);
        Context.SaveChanges();

        var seller = new Seller { PersonId = sellerPerson.Id, EntityType = SellerEntityType.Real };
        Context.Sellers.Add(seller);
        Context.SaveChanges();

        var customerPerson = new Person { FirstName = "Customer", LastName = "Two", PhoneNumber = "09120000004" };
        Context.People.Add(customerPerson);
        Context.SaveChanges();

        var customer = new Customer { PersonId = customerPerson.Id, NationalId = "1000000002" };
        Context.Customers.Add(customer);
        Context.SaveChanges();

        var product = TestDataHelpers.CreateProduct(Context, 280m);
        Context.SaveChanges();

        var transaction = new Transaction
        {
            ProductId = product.Id,
            ProductNavigation = product,
            SellerId = seller.Id,
            SellerNavigation = seller,
            CustomerId = customer.Id,
            CustomerNavigation = customer,
            FinishedPrice = 280m,
            Date = DateTime.UtcNow,
            Direction = TransactionDirection.Buy
        };
        Context.Transactions.Add(transaction);
        Context.SaveChanges();

        var invoice = _service.GetInvoice(transaction.Id);

        Assert.NotNull(invoice);
        Assert.Null(invoice!.BuyerName);
        Assert.Null(invoice.BuyerNationalId);
        Assert.Null(invoice.BuyerPhoneNumber);
        Assert.Equal("Seller Two", invoice.SellerName);
        Assert.Equal("09120000003", invoice.SellerPhoneNumber);
    }

    [Fact]
    public void GetInvoice_returns_null_for_nonexistent_transaction()
    {
        var invoice = _service.GetInvoice(99999);
        Assert.Null(invoice);
    }

    [Fact]
    public void GeneratePdf_returns_pdf_bytes_for_valid_transaction()
    {
        var sellerPerson = new Person { FirstName = "Seller", LastName = "Pdf", PhoneNumber = "09120000005" };
        Context.People.Add(sellerPerson);
        Context.SaveChanges();

        var seller = new Seller { PersonId = sellerPerson.Id, EntityType = SellerEntityType.Real };
        Context.Sellers.Add(seller);
        Context.SaveChanges();

        var customerPerson = new Person { FirstName = "Customer", LastName = "Pdf", PhoneNumber = "09120000006" };
        Context.People.Add(customerPerson);
        Context.SaveChanges();

        var customer = new Customer { PersonId = customerPerson.Id, NationalId = "1000000003" };
        Context.Customers.Add(customer);
        Context.SaveChanges();

        var product = TestDataHelpers.CreateProduct(Context, 250m);
        Context.SaveChanges();

        var transaction = new Transaction
        {
            ProductId = product.Id,
            ProductNavigation = product,
            SellerId = seller.Id,
            SellerNavigation = seller,
            CustomerId = customer.Id,
            CustomerNavigation = customer,
            FinishedPrice = 250m,
            Date = DateTime.UtcNow,
            Direction = TransactionDirection.Sell
        };
        Context.Transactions.Add(transaction);
        Context.SaveChanges();

        var pdfBytes = _service.GeneratePdf(transaction.Id);

        Assert.NotNull(pdfBytes);
        Assert.NotEmpty(pdfBytes);
        _pdfGeneratorMock.Verify(p => p.Generate(It.IsAny<InvoiceViewModel>()), Times.Once);
    }

    [Fact]
    public void GeneratePdf_throws_for_nonexistent_transaction()
    {
        Assert.Throws<KeyNotFoundException>(() => _service.GeneratePdf(99999));
    }
}
