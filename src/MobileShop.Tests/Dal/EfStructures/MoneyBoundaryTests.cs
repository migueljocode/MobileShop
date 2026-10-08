using System.ComponentModel.DataAnnotations;
using Microsoft.EntityFrameworkCore;
using MobileShop.Models.ViewModels.Web.BindModels;

namespace MobileShop.Tests.Dal.EfStructures;

public class MoneyBoundaryTests : IDisposable
{
    private readonly string _databaseFile = Path.Combine(Path.GetTempPath(), $"mobileshop-money-{Guid.NewGuid():N}.db");
    private string ConnectionString => $"Data Source={_databaseFile};Pooling=False";

    [Fact]
    public void SQLite_round_trips_large_money_values_exactly()
    {
        using var context = CreateContext();
        context.Database.Migrate();

        var category = new Category { Name = "Phone" };
        var manufacturer = new Manufacturer { Name = "Apple" };
        context.Categories.Add(category);
        context.Manufacturers.Add(manufacturer);
        context.SaveChanges();

        var model = new Model
        {
            ManufacturerId = manufacturer.Id,
            CategoryId = category.Id,
            Name = "Boundary"
        };
        context.Models.Add(model);
        context.SaveChanges();

        var product = new Product
        {
            ModelId = model.Id,
            Barcode = "BOUNDARY12345",
            Price = 5_000_000_000L
        };
        context.Products.Add(product);
        context.SaveChanges();

        var seller = new Seller { PersonNavigation = new Person { FirstName = "Seller", LastName = "Boundary" }, EntityType = SellerEntityType.Real };
        var customer = new Customer { PersonNavigation = new Person { FirstName = "Customer", LastName = "Boundary" }, NationalId = "1234567890" };
        context.Sellers.Add(seller);
        context.Customers.Add(customer);
        context.SaveChanges();

        var transaction = new Transaction
        {
            ProductId = product.Id,
            SellerId = seller.Id,
            CustomerId = customer.Id,
            FinishedPrice = 3_000_000_000L,
            Date = DateTime.Today,
            Direction = TransactionDirection.Sell
        };
        context.Transactions.Add(transaction);
        context.SaveChanges();
        context.ChangeTracker.Clear();

        var persistedProduct = context.Products.Single(p => p.Id == product.Id);
        var persistedTransaction = context.Transactions.Single(t => t.Id == transaction.Id);

        Assert.Equal(5_000_000_000L, persistedProduct.Price);
        Assert.Equal(3_000_000_000L, persistedTransaction.FinishedPrice);
    }

    [Fact]
    public void Money_inputs_accept_max_and_reject_above_max()
    {
        var inputs = new object[]
        {
            new CreatePhoneInputModel { SellerId = 1, Price = MoneyLimits.MaxRials, ProfitAmount = MoneyLimits.MaxRials, IMEI1 = "123456789012345" },
            new CreateAppleIdInputModel { SellerId = 1, Price = MoneyLimits.MaxRials, ProfitAmount = MoneyLimits.MaxRials, Email = "limit@example.com", Password = "password" },
            new CreateGlassInputModel { SellerId = 1, CompatibleManufacturerId = 1, CompatibleModelId = 1, GlassManufacturerId = 1, Price = MoneyLimits.MaxRials, ProfitAmount = MoneyLimits.MaxRials },
            new SellInputModel { ProductId = 1, CustomerId = 1, Price = MoneyLimits.MaxRials }
        };

        Assert.All(inputs, input => Assert.Empty(Validate(input)));

        var overLimit = new object[]
        {
            new CreatePhoneInputModel { SellerId = 1, Price = MoneyLimits.MaxRials + 1, ProfitAmount = MoneyLimits.MaxRials + 1, IMEI1 = "123456789012345" },
            new CreateAppleIdInputModel { SellerId = 1, Price = MoneyLimits.MaxRials + 1, ProfitAmount = MoneyLimits.MaxRials + 1, Email = "limit@example.com", Password = "password" },
            new CreateGlassInputModel { SellerId = 1, CompatibleManufacturerId = 1, CompatibleModelId = 1, GlassManufacturerId = 1, Price = MoneyLimits.MaxRials + 1, ProfitAmount = MoneyLimits.MaxRials + 1 },
            new SellInputModel { ProductId = 1, CustomerId = 1, Price = MoneyLimits.MaxRials + 1 }
        };

        Assert.All(overLimit, input => Assert.NotEmpty(Validate(input)));
    }

    private static IReadOnlyList<ValidationResult> Validate(object model)
    {
        var context = new ValidationContext(model);
        var results = new List<ValidationResult>();
        Validator.TryValidateObject(model, context, results, validateAllProperties: true);
        return results;
    }

    private AppDbContext CreateContext()
        => new(new DbContextOptionsBuilder<AppDbContext>().UseSqlite(ConnectionString).Options);

    public void Dispose()
    {
        foreach (var suffix in new[] { string.Empty, "-wal", "-shm" })
        {
            var file = _databaseFile + suffix;
            if (File.Exists(file))
            {
                try { File.Delete(file); } catch (IOException) { }
            }
        }
        GC.SuppressFinalize(this);
    }
}
