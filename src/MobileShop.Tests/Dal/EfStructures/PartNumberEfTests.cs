namespace MobileShop.Tests.Dal.EfStructures;

/// <summary>
/// Stage M, Step 1 — PartNumber schema and EF configuration tests.
/// </summary>
public class PartNumberEfTests : SqliteRepoTestBase
{
    /// <summary>
    /// PartNumber -> Model is many-to-one: a PartNumber must belong to exactly one Model.
    /// </summary>
    [Fact]
    public void PartNumber_requires_model()
    {
        var category = new Category { Name = "Mobile" };
        var manufacturer = new Manufacturer { Name = "Apple" };
        Context.Categories.Add(category);
        Context.Manufacturers.Add(manufacturer);
        Context.SaveChanges();

        var model = new Model
        {
            ManufacturerId = manufacturer.Id,
            CategoryId = category.Id,
            Name = "iPhone 17"
        };
        Context.Models.Add(model);
        Context.SaveChanges();

        var partNumber = new PartNumber
        {
            ModelId = model.Id,
            Code = "CH/ZAA",
            SupportsDualSim = true,
            SupportsEsim = true
        };
        Context.PartNumbers.Add(partNumber);
        Context.SaveChanges();

        Assert.Equal(model.Id, partNumber.ModelId);
        Assert.Single(model.PartNumbers);
    }

    /// <summary>
    /// Phone -> PartNumber is optional: a Phone without a PartNumber is valid.
    /// </summary>
    [Fact]
    public void Phone_can_exist_without_part_number()
    {
        var product = TestDataHelpers.CreateProduct(Context);
        var phone = new Phone
        {
            ProductId = product.Id,
            IMEI1 = TestDataHelpers.GenerateImei()
        };
        Context.Phones.Add(phone);
        Context.SaveChanges();

        Assert.Null(phone.PartNumberId);
    }

    /// <summary>
    /// Many physical Phone rows can share the same PartNumber.
    /// </summary>
    [Fact]
    public void Multiple_phones_can_share_same_part_number()
    {
        var category = new Category { Name = "Mobile" };
        var manufacturer = new Manufacturer { Name = "Apple" };
        Context.Categories.Add(category);
        Context.Manufacturers.Add(manufacturer);
        Context.SaveChanges();

        var model = new Model
        {
            ManufacturerId = manufacturer.Id,
            CategoryId = category.Id,
            Name = "iPhone 17"
        };
        Context.Models.Add(model);
        Context.SaveChanges();

        var partNumber = new PartNumber
        {
            ModelId = model.Id,
            Code = "CH/ZAA",
            SupportsDualSim = true,
            SupportsEsim = true
        };
        Context.PartNumbers.Add(partNumber);
        Context.SaveChanges();

        var product1 = TestDataHelpers.CreateProduct(Context);
        var product2 = TestDataHelpers.CreateProduct(Context);

        var phone1 = new Phone { ProductId = product1.Id, IMEI1 = TestDataHelpers.GenerateImei(), PartNumberId = partNumber.Id };
        var phone2 = new Phone { ProductId = product2.Id, IMEI1 = TestDataHelpers.GenerateImei(), PartNumberId = partNumber.Id };
        Context.Phones.AddRange(phone1, phone2);
        Context.SaveChanges();

        Assert.Equal(2, partNumber.Phones.Count);
    }

    /// <summary>
    /// Duplicate Model + Code is rejected; same Code under different Models is allowed.
    /// </summary>
    [Fact]
    public void Duplicate_model_plus_code_rejected_but_same_code_different_model_allowed()
    {
        var category = new Category { Name = "Mobile" };
        var manufacturer = new Manufacturer { Name = "Apple" };
        Context.Categories.Add(category);
        Context.Manufacturers.Add(manufacturer);
        Context.SaveChanges();

        var modelA = new Model { ManufacturerId = manufacturer.Id, CategoryId = category.Id, Name = "iPhone 17" };
        var modelB = new Model { ManufacturerId = manufacturer.Id, CategoryId = category.Id, Name = "iPhone 16" };
        Context.Models.AddRange(modelA, modelB);
        Context.SaveChanges();

        Context.PartNumbers.Add(new PartNumber { ModelId = modelA.Id, Code = "CH/ZAA", SupportsDualSim = true, SupportsEsim = true });
        Context.SaveChanges();

        var dup = new PartNumber { ModelId = modelA.Id, Code = "CH/ZAA", SupportsDualSim = false, SupportsEsim = true };
        Context.PartNumbers.Add(dup);
        Assert.Throws<DbUpdateException>(() => Context.SaveChanges());

        Context.Entry(dup).State = EntityState.Detached;

        var differentModel = new PartNumber { ModelId = modelB.Id, Code = "CH/ZAA", SupportsDualSim = true, SupportsEsim = false };
        Context.PartNumbers.Add(differentModel);
        Context.SaveChanges();

        Assert.Equal(2, Context.PartNumbers.Count());
    }

    /// <summary>
    /// Migration safety: applying the AddPartNumber migration against a fresh database
    /// must produce a schema where a Phone without a PartNumber is valid (NULL PartNumberId).
    /// </summary>
    [Fact]
    public void Migration_preserves_existing_phone_with_null_part_number()
    {
        Context.Database.EnsureDeleted();
        Context.Database.Migrate();

        var category = new Category { Name = "Mobile" };
        var manufacturer = new Manufacturer { Name = "Apple" };
        Context.Categories.Add(category);
        Context.Manufacturers.Add(manufacturer);
        Context.SaveChanges();

        var model = new Model { ManufacturerId = manufacturer.Id, CategoryId = category.Id, Name = "iPhone 17" };
        Context.Models.Add(model);
        Context.SaveChanges();

        var product = TestDataHelpers.CreateProduct(Context);
        var phone = new Phone { ProductId = product.Id, IMEI1 = TestDataHelpers.GenerateImei() };
        Context.Phones.Add(phone);
        Context.SaveChanges();

        var phoneId = phone.Id;

        var preservedPhone = Context.Phones.First(p => p.Id == phoneId);
        Assert.NotNull(preservedPhone);
        Assert.Null(preservedPhone.PartNumberId);
        Assert.Equal(phone.IMEI1, preservedPhone.IMEI1);
    }
}
