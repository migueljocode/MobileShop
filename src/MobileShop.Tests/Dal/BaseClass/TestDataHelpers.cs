namespace MobileShop.Tests.Dal.BaseClass;

internal static class TestDataHelpers
{
    // a syntactically valid 15-digit IMEI - the range guarantees exactly 15 digits every time,
    // and Random.Shared makes repeated calls within a test run unlikely to collide
    internal static string GenerateImei() => Random.Shared.NextInt64(100_000_000_000_000, 999_999_999_999_999).ToString();

    /// <summary>
    /// Creates and persists a product together with the manufacturer/category/model graph it now
    /// requires, so product projections can resolve their navigation paths. Randomised names keep
    /// the unique indexes happy across repeated calls in one test run.
    /// </summary>
    internal static Product CreateProduct(AppDbContext context, decimal price = 100m, bool persist = true)
    {
        var category = new Category { Name = $"Category-{Guid.NewGuid():N}" };
        var manufacturer = new Manufacturer { Name = $"Manufacturer-{Guid.NewGuid():N}" };
        context.Categories.Add(category);
        context.Manufacturers.Add(manufacturer);
        context.SaveChanges();

        var model = new Model
        {
            ManufacturerId = manufacturer.Id,
            CategoryId = category.Id,
            Name = $"Model-{Guid.NewGuid():N}"
        };
        context.Models.Add(model);
        context.SaveChanges();

        var product = new Product
        {
            Price = price,
            ModelId = model.Id,
            Barcode = Guid.NewGuid().ToString("N")[..12]
        };
        if (persist)
        {
            context.Products.Add(product);
            context.SaveChanges();
        }

        return product;
    }

    /// <summary>
    /// Seeds the sentinel Shop person, seller, and customer records (id = 1) expected by
    /// <see cref="TransactionDataService"/> when recording buy/sell operations.
    /// </summary>
    internal static void SeedShopSentinels(AppDbContext context)
    {
        if (context.People.Any(p => p.Id == 1))
            return;

        var shopPerson = new Person
        {
            Id = 1,
            FirstName = "Shop",
            LastName = "Owner",
            PhoneNumber = "09120000001"
        };
        context.People.Add(shopPerson);
        context.SaveChanges();

        if (!context.Sellers.Any(s => s.Id == 1))
        {
            context.Sellers.Add(new Seller
            {
                Id = 1,
                PersonId = 1,
                EntityType = SellerEntityType.Legal
            });
        }

        if (!context.Customers.Any(c => c.Id == 1))
        {
            context.Customers.Add(new Customer
            {
                Id = 1,
                PersonId = 1,
                NationalId = "0000000000"
            });
        }

        context.SaveChanges();
    }

}
