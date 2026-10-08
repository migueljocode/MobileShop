using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using MobileShop.Models.ViewModels.Web.BindModels;
using MobileShop.Models.ViewModels;
using MobileShop.Services.PDF;
using MobileShop.Web.Pages.Transactions;

namespace MobileShop.Tests.Web.Pages.Transactions;

/// <summary>Verifies the Sell page records transactions through the area data service.</summary>
public class RecordModelTests : RepoTestBase
{
    /// <summary>Thin PDF double - this page never renders a PDF, but the service ctor requires one.</summary>
    private sealed class StubPdfGenerator : IPdfGenerator
    {
        public byte[] Generate(InvoiceViewModel model) => [];

        public byte[] GenerateTransactionFactor(TransactionFactorViewModel model) => [];
    }

    private TransactionsDataService CreateService() => new(
        new BaseRepo<Transaction>(Context),
        new BaseRepo<Seller>(Context),
        new BaseRepo<Customer>(Context),
        new BaseRepo<Phone>(Context),
        new BaseRepo<AppleId>(Context),
        new BaseRepo<Product>(Context),
        Context,
        new StubPdfGenerator(),
        NullLogger<TransactionsDataService>.Instance);

    private Customer AddCustomer(string firstName, string lastName)
    {
        var person = new Person { FirstName = firstName, LastName = lastName, PhoneNumber = "09120000022" };
        Context.People.Add(person);
        Context.SaveChanges();

        var customer = new Customer { PersonId = person.Id, NationalId = Guid.NewGuid().ToString("N")[..10] };
        Context.Customers.Add(customer);
        Context.SaveChanges();
        return customer;
    }

    [Fact]
    public async Task Sell_OnGet_loads_customers_and_selectable_products()
    {
        var customer = AddCustomer("Sara", "Ahmadi");

        var model = new SellModel(CreateService(), null!);
        var result = await model.OnGetAsync();

        Assert.IsType<PageResult>(result);
        Assert.Single(model.Customers);
        Assert.Equal(customer.Id, model.Customers[0].Id);
    }

    [Fact]
    public async Task Sell_OnGet_defaults_date_to_today()
    {
        var before = DateTime.Now;
        var model = new SellModel(CreateService(), null!);
        var result = await model.OnGetAsync();
        var after = DateTime.Now;

        Assert.IsType<PageResult>(result);
        Assert.InRange(model.Input.Date!.Value, before, after);
    }

    [Fact]
    public async Task Sell_OnPost_records_transaction_and_reports_success_message()
    {
        TestDataHelpers.SeedShopSentinels(Context);
        var customer = AddCustomer("Sara", "Ahmadi");
        var product = TestDataHelpers.CreateProduct(Context);
        var before = DateTime.Now;

        var model = new SellModel(CreateService(), null!)
        {
            Input = new SellInputModel { ProductId = product.Id, CustomerId = customer.Id, Price = 250 }
        };

        var result = await model.OnPostAsync();
        var after = DateTime.Now;

        Assert.IsType<PageResult>(result);
        Assert.Equal("Sale recorded successfully.", model.Message);
        var transaction = Context.Transactions.Single();
        Assert.Equal(TransactionDirection.Sell, transaction.Direction);
        Assert.Equal(customer.Id, transaction.CustomerId);
        Assert.InRange(transaction.Date, before, after);
    }

    [Fact]
    public async Task Sell_OnPost_save_and_add_another_redirects_to_a_clean_form()
    {
        TestDataHelpers.SeedShopSentinels(Context);
        var customer = AddCustomer("Sara", "Ahmadi");
        var product = TestDataHelpers.CreateProduct(Context);
        var model = new SellModel(CreateService(), null!)
        {
            Input = new SellInputModel { ProductId = product.Id, CustomerId = customer.Id, Price = 250 }
        };

        var result = await model.OnPostAsync(saveAndAddAnother: true);

        var redirect = Assert.IsType<RedirectToPageResult>(result);
        Assert.Null(redirect.PageName);
        Assert.Equal("Sale recorded successfully. Ready for the next sale.", model.SuccessMessage);
        Assert.Equal(0, model.Input.ProductId);
        Assert.Equal(0, model.Input.CustomerId);
        Assert.Single(Context.Transactions);
        Assert.Equal(TransactionDirection.Sell, Context.Transactions.Single().Direction);
    }

    [Fact]
    public async Task Sell_zero_customer_id_rejected_with_friendly_message()
    {
        TestDataHelpers.SeedShopSentinels(Context);
        var customer = AddCustomer("Sara", "Ahmadi");
        var product = TestDataHelpers.CreateProduct(Context);

        var sellModel = new SellModel(CreateService(), null!)
        {
            Input = new SellInputModel { ProductId = product.Id, CustomerId = 0, Price = 250 }
        };
        ValidateInput(sellModel);
        await sellModel.OnPostAsync();
        Assert.False(sellModel.ModelState.IsValid);
        var error = Assert.Single(sellModel.ModelState["Input.CustomerId"]!.Errors);
        Assert.Equal("The customer should be selected.", error.ErrorMessage);
    }

    [Fact]
    public async Task Sell_zero_product_id_rejected_with_friendly_message()
    {
        TestDataHelpers.SeedShopSentinels(Context);
        var customer = AddCustomer("Sara", "Ahmadi");

        var sellModel = new SellModel(CreateService(), null!)
        {
            Input = new SellInputModel { ProductId = 0, CustomerId = customer.Id, Price = 250 }
        };
        ValidateInput(sellModel);
        await sellModel.OnPostAsync();
        Assert.False(sellModel.ModelState.IsValid);
        var error = Assert.Single(sellModel.ModelState["Input.ProductId"]!.Errors);
        Assert.Equal("The product should be selected.", error.ErrorMessage);
    }

    [Fact]
    public async Task Sell_valid_positive_ids_pass_validation()
    {
        TestDataHelpers.SeedShopSentinels(Context);
        var customer = AddCustomer("Sara", "Ahmadi");
        var product = TestDataHelpers.CreateProduct(Context);

        var sellModel = new SellModel(CreateService(), null!)
        {
            Input = new SellInputModel { ProductId = product.Id, CustomerId = customer.Id, Price = 250 }
        };
        await sellModel.OnPostAsync();
        Assert.True(sellModel.ModelState.IsValid);
        Assert.NotNull(sellModel.Message);
    }

    /// <summary>
    /// Runs data-annotation validation against the Input property the way MVC's model binder
    /// would, populating ModelState so attribute-driven errors can be asserted.
    /// </summary>
    private static void ValidateInput(PageModel pageModel)
    {
        var inputProp = pageModel.GetType().GetProperty("Input")!;
        var input = inputProp.GetValue(pageModel)!;
        var results = new List<ValidationResult>();
        var ctx = new ValidationContext(input!);
        Validator.TryValidateObject(input!, ctx, results, true);
        foreach (var r in results)
        {
            pageModel.ModelState.AddModelError(inputProp.Name + "." + r.MemberNames.FirstOrDefault(), r.ErrorMessage ?? string.Empty);
        }
    }
}
