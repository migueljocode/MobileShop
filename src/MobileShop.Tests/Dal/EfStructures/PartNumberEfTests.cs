using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Migrations;
using Microsoft.Extensions.DependencyInjection;

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
    /// Migration safety: a Phone created under the pre-migration schema (using all
    /// migrations except AddPartNumber) must survive the AddPartNumber migration with
    /// its ID and IMEI intact and PartNumberId = NULL.
    /// </summary>
    [Fact]
    public void Migration_preserves_existing_phone_with_null_part_number()
    {
        var dbFile = Path.Combine(Path.GetTempPath(), $"mobileshop-migration-{Guid.NewGuid():N}.db");
        var connString = $"Data Source={dbFile};Pooling=False";

        try
        {
            // Step 1: Create the pre-migration database using all migrations except AddPartNumber.
            using (var prepContext = new AppDbContext(
                       new DbContextOptionsBuilder<AppDbContext>()
                           .UseSqlite(connString)
                           .Options))
            {
                var target = prepContext.Database.GetService<IMigrator>();
                var preMigrationIds = new[]
                {
                    "20260912135945_Initial",
                    "20260914132708_AddPhoneColor",
                    "20260920163656_NormalizeCatalog",
                    "20260923232224_AddEmployeeEntity"
                };

                foreach (var migrationId in preMigrationIds)
                {
                    target.Migrate(migrationId);
                }

                // Seed a Manufacturer, Category, Model, Product, and Phone under the pre-migration schema.
                prepContext.Database.ExecuteSqlRaw(
                @"
                INSERT INTO Manufacturers (Id, Name, IsDeleted) VALUES (1, 'Apple', 0);
                INSERT INTO Categories (Id, Name, IsDeleted) VALUES (1, 'Mobile', 0);
                INSERT INTO Models (Id, ManufacturerId, CategoryId, Name, IsDeleted) VALUES (1, 1, 1, 'iPhone 17', 0);
                INSERT INTO Products (Id, Price, ModelId, Barcode, IsDeleted) VALUES (1, 100, 1, 'barcode001', 0);
                INSERT INTO Phones (Id, IMEI1, IMEI2, OwnershipTransferred, Notes, ProductId, IsDeleted)
                VALUES (1, '123456789012345', NULL, 0, NULL, 1, 0);
                ");
            }

            // Step 2: Open a new context and apply the remaining (AddPartNumber) migration.
            using var migratedContext = new AppDbContext(
                new DbContextOptionsBuilder<AppDbContext>()
                    .UseSqlite(connString)
                    .Options);

            var pendingMigrations = migratedContext.Database.GetPendingMigrations().ToList();
            Assert.Single(pendingMigrations);
            Assert.Contains("AddPartNumber", pendingMigrations[0]);

            migratedContext.Database.Migrate();

            // Step 3: Reload the phone and verify it survived the migration with NULL PartNumberId.
            var preservedPhone = migratedContext.Phones
                .Include(p => p.PartNumberNavigation)
                .First(p => p.Id == 1);

            Assert.NotNull(preservedPhone);
            Assert.Equal(1, preservedPhone.Id);
            Assert.Equal("123456789012345", preservedPhone.IMEI1);
            Assert.Null(preservedPhone.PartNumberId);
            Assert.Null(preservedPhone.PartNumberNavigation);
        }
        finally
        {
            foreach (var suffix in new[] { string.Empty, "-wal", "-shm" })
            {
                var file = dbFile + suffix;
                if (File.Exists(file))
                {
                    try
                    {
                        File.Delete(file);
                    }
                    catch (IOException)
                    {
                        // Best-effort cleanup only.
                    }
                }
            }
        }
    }
}
