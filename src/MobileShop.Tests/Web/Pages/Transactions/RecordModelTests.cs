using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using MobileShop.Models.ViewModels.Web.BindModels;
using MobileShop.Models.ViewModels;
using MobileShop.Services.PDF;
using MobileShop.Web.Pages.Transactions;

namespace MobileShop.Tests.Web.Pages.Transactions;

/// <summary>Verifies the Buy and Sell pages record transactions through the area data service.</summary>
public class RecordModelTests : RepoTestBase
{
    /// <summary>Thin PDF double - these pages never render a PDF, but the service ctor requires one.</summary>
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
        Context,
        new StubPdfGenerator(),
        NullLogger<TransactionsDataService>.Instance);

    private Seller AddSeller(string firstName, string lastName)
    {
        var person = new Person { FirstName = firstName, LastName = lastName, PhoneNumber = "09120000021" };
        Context.People.Add(person);
        Context.SaveChanges();

        var seller = new Seller { PersonId = person.Id, EntityType = SellerEntityType.Real };
        Context.Sellers.Add(seller);
        Context.SaveChanges();
        return seller;
    }

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
    public async Task Buy_OnGet_loads_sellers_and_selectable_products()
    {
        var seller = AddSeller("Ali", "Zed");
        var product = TestDataHelpers.CreateProduct(Context);
        Context.Phones.Add(new Phone { ProductId = product.Id, IMEI1 = TestDataHelpers.GenerateImei() });
        Context.SaveChanges();

        var model = new BuyModel(CreateService());
        var result = await model.OnGetAsync();

        Assert.IsType<PageResult>(result);
        Assert.Single(model.Sellers);
        Assert.Equal(seller.Id, model.Sellers[0].Id);
        Assert.Single(model.Products);
    }

    [Fact]
    public async Task Buy_OnGet_defaults_date_to_today()
    {
        var model = new BuyModel(CreateService());
        var result = await model.OnGetAsync();

        Assert.IsType<PageResult>(result);
        Assert.Equal(DateTime.Today, model.Input.Date);
    }

    [Fact]
    public async Task Buy_OnPost_records_transaction_and_reports_success_message()
    {
        TestDataHelpers.SeedShopSentinels(Context);
        var seller = AddSeller("Ali", "Zed");
        var product = TestDataHelpers.CreateProduct(Context);

        var model = new BuyModel(CreateService())
        {
            Input = new BuyInputModel { ProductId = product.Id, SellerId = seller.Id, Price = 150m }
        };

        var result = await model.OnPostAsync();

        Assert.IsType<PageResult>(result);
        Assert.Equal("Buy recorded successfully.", model.Message);
        var transaction = Context.Transactions.Single();
        Assert.Equal(TransactionDirection.Buy, transaction.Direction);
        Assert.Equal(150m, transaction.FinishedPrice);
    }

    [Fact]
    public async Task Buy_OnPost_reports_service_failure_as_model_error()
    {
        TestDataHelpers.SeedShopSentinels(Context);
        var seller = AddSeller("Ali", "Zed");
        var product = TestDataHelpers.CreateProduct(Context);

        Context.Transactions.Add(new Transaction
        {
            ProductId = product.Id,
            SellerId = seller.Id,
            CustomerId = 1,
            FinishedPrice = 100m,
            Date = DateTime.UtcNow,
            Direction = TransactionDirection.Buy
        });
        Context.SaveChanges();

        var model = new BuyModel(CreateService())
        {
            Input = new BuyInputModel { ProductId = product.Id, SellerId = seller.Id, Price = 150m }
        };

        var result = await model.OnPostAsync();

        Assert.IsType<PageResult>(result);
        Assert.Null(model.Message);
        Assert.Contains(
            model.ModelState[string.Empty]!.Errors,
            error => error.ErrorMessage.Contains("could not be recorded"));
        Assert.Single(Context.Transactions);
    }

    [Fact]
    public async Task Sell_OnGet_loads_customers_and_selectable_products()
    {
        var customer = AddCustomer("Sara", "Ahmadi");

        var model = new SellModel(CreateService());
        var result = await model.OnGetAsync();

        Assert.IsType<PageResult>(result);
        Assert.Single(model.Customers);
        Assert.Equal(customer.Id, model.Customers[0].Id);
    }

    [Fact]
    public async Task Sell_OnPost_records_transaction_and_reports_success_message()
    {
        TestDataHelpers.SeedShopSentinels(Context);
        var customer = AddCustomer("Sara", "Ahmadi");
        var product = TestDataHelpers.CreateProduct(Context);

        var model = new SellModel(CreateService())
        {
            Input = new SellInputModel { ProductId = product.Id, CustomerId = customer.Id, Price = 250m }
        };

        var result = await model.OnPostAsync();

        Assert.IsType<PageResult>(result);
        Assert.Equal("Sale recorded successfully.", model.Message);
        var transaction = Context.Transactions.Single();
        Assert.Equal(TransactionDirection.Sell, transaction.Direction);
        Assert.Equal(customer.Id, transaction.CustomerId);
    }

    [Theory]
    [InlineData("Buy")]
    [InlineData("Sell")]
    public async Task Zero_selection_rejected_with_friendly_message(string direction)
    {
        TestDataHelpers.SeedShopSentinels(Context);
        var seller = AddSeller("Ali", "Zed");
        var customer = AddCustomer("Sara", "Ahmadi");
        var product = TestDataHelpers.CreateProduct(Context);

        if (direction == "Buy")
        {
            var buyModel = new BuyModel(CreateService())
            {
                Input = new BuyInputModel { ProductId = 0, SellerId = seller.Id, Price = 150m }
            };
            ValidateInput(buyModel);
            await buyModel.OnPostAsync();
            Assert.False(buyModel.ModelState.IsValid);
            var error = Assert.Single(buyModel.ModelState["Input.ProductId"]!.Errors);
            Assert.Equal("The product should be selected.", error.ErrorMessage);
        }
        else
        {
            var sellModel = new SellModel(CreateService())
            {
                Input = new SellInputModel { ProductId = product.Id, CustomerId = 0, Price = 250m }
            };
            ValidateInput(sellModel);
            await sellModel.OnPostAsync();
            Assert.False(sellModel.ModelState.IsValid);
            var error = Assert.Single(sellModel.ModelState["Input.CustomerId"]!.Errors);
            Assert.Equal("The customer should be selected.", error.ErrorMessage);
        }
    }

    [Fact]
    public async Task Buy_zero_seller_id_rejected_with_friendly_message()
    {
        TestDataHelpers.SeedShopSentinels(Context);
        var product = TestDataHelpers.CreateProduct(Context);

        var buyModel = new BuyModel(CreateService())
        {
            Input = new BuyInputModel { ProductId = product.Id, SellerId = 0, Price = 150m }
        };
        ValidateInput(buyModel);
        await buyModel.OnPostAsync();
        Assert.False(buyModel.ModelState.IsValid);
        var error = Assert.Single(buyModel.ModelState["Input.SellerId"]!.Errors);
        Assert.Equal("The seller should be selected.", error.ErrorMessage);
    }

    [Fact]
    public async Task Sell_zero_product_id_rejected_with_friendly_message()
    {
        TestDataHelpers.SeedShopSentinels(Context);
        var customer = AddCustomer("Sara", "Ahmadi");

        var sellModel = new SellModel(CreateService())
        {
            Input = new SellInputModel { ProductId = 0, CustomerId = customer.Id, Price = 250m }
        };
        ValidateInput(sellModel);
        await sellModel.OnPostAsync();
        Assert.False(sellModel.ModelState.IsValid);
        var error = Assert.Single(sellModel.ModelState["Input.ProductId"]!.Errors);
        Assert.Equal("The product should be selected.", error.ErrorMessage);
    }

    [Theory]
    [InlineData("Buy")]
    [InlineData("Sell")]
    public async Task Valid_positive_ids_pass_validation(string direction)
    {
        TestDataHelpers.SeedShopSentinels(Context);
        var seller = AddSeller("Ali", "Zed");
        var customer = AddCustomer("Sara", "Ahmadi");
        var product = TestDataHelpers.CreateProduct(Context);

        if (direction == "Buy")
        {
            var buyModel = new BuyModel(CreateService())
            {
                Input = new BuyInputModel { ProductId = product.Id, SellerId = seller.Id, Price = 150m }
            };
            await buyModel.OnPostAsync();
            Assert.True(buyModel.ModelState.IsValid);
            Assert.NotNull(buyModel.Message);
        }
        else
        {
            var sellModel = new SellModel(CreateService())
            {
                Input = new SellInputModel { ProductId = product.Id, CustomerId = customer.Id, Price = 250m }
            };
            await sellModel.OnPostAsync();
            Assert.True(sellModel.ModelState.IsValid);
            Assert.NotNull(sellModel.Message);
        }
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
