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
}
