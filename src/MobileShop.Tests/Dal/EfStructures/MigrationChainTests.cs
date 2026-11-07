namespace MobileShop.Tests.Dal.EfStructures;

public class MigrationChainTests : IDisposable
{
    private readonly string _databaseFile =
        Path.Combine(Path.GetTempPath(), $"mobileshop-migration-chain-{Guid.NewGuid():N}.db");

    private readonly AppDbContext _context;

    public MigrationChainTests()
    {
        var connectionString = $"Data Source={_databaseFile};Pooling=False";
        var options = new DbContextOptionsBuilder<AppDbContext>()
            .UseSqlite(connectionString)
            .Options;

        _context = new AppDbContext(options);
    }

    [Fact]
    public void Migrations_are_discovered_in_order()
    {
        var migrations = _context.GetService<IMigrationsAssembly>().Migrations.Keys.ToArray();

        Assert.Equal(
            new[]
            {
                "20260912135945_Initial",
                "20260914132708_AddPhoneColor",
                "20260920163656_NormalizeCatalog",
                "20260923232224_AddEmployeeEntity",
                "20261001161450_AddPartNumber",
                "20261002060000_UseIntegerRialMoney",
                "20261004090000_WidenMoneyToLong",
                "20261006215821_AddPowerBankPortsAndPhoneSimSupport",
                "20261008023227_AddCpuGpuLookups",
                "20261104100000_AddAppleIdSecurityAnswers",
                "20261107120000_SeedZoneMobileShopParty"
            },
            migrations);
    }

    [Fact]
    public void Snapshot_matches_the_current_model()
    {
        var migrationsAssembly = _context.GetService<IMigrationsAssembly>();
        var snapshot = migrationsAssembly.ModelSnapshot;
        Assert.NotNull(snapshot);

        var runtimeInitializer = _context.GetService<IModelRuntimeInitializer>();
        var snapshotModel = runtimeInitializer.Initialize(snapshot!.Model, designTime: true);
        var snapshotRelationalModel = snapshotModel.GetRelationalModel();

        var currentRelationalModel =
            _context.GetService<IDesignTimeModel>().Model.GetRelationalModel();

        var differ = _context.GetService<IMigrationsModelDiffer>();
        Assert.False(differ.HasDifferences(snapshotRelationalModel, currentRelationalModel));
    }

    [Fact]
    public void Chain_applies_to_an_empty_database()
    {
        _context.Database.Migrate();

        Assert.Empty(_context.Database.GetPendingMigrations());

        var migrationHistoryCount = _context.Database
            .SqlQueryRaw<int>("SELECT COUNT(*) AS Value FROM \"__EFMigrationsHistory\"")
            .Single();

        Assert.Equal(_context.GetService<IMigrationsAssembly>().Migrations.Count, migrationHistoryCount);

        Assert.Equal(
            "INTEGER",
            _context.Database
                .SqlQueryRaw<string>(
                    "SELECT type AS Value FROM pragma_table_info('Products') WHERE name = 'Price'")
                .Single());

        Assert.Equal(
            "INTEGER",
            _context.Database
                .SqlQueryRaw<string>(
                    "SELECT type AS Value FROM pragma_table_info('Transactions') WHERE name = 'FinishedPrice'")
                .Single());

        var constraints = _context.Database
            .SqlQueryRaw<string>(
                "SELECT sql AS Value FROM sqlite_master WHERE type = 'table' AND name IN ('Products', 'Transactions')")
            .ToList();

        Assert.Contains(constraints, sql => sql.Contains("CK_Products_Price_NonNegative", StringComparison.Ordinal));
        Assert.Contains(constraints, sql => sql.Contains("CK_Transactions_FinishedPrice_NonNegative", StringComparison.Ordinal));
    }

    [Fact]
    public void Cpu_and_gpu_names_are_preserved_when_laptop_catalog_is_migrated()
    {
        _context.GetService<IMigrator>().Migrate("20261006215821_AddPowerBankPortsAndPhoneSimSupport");
        _context.Database.ExecuteSqlRaw("""
            INSERT INTO Manufacturers (Id, Name, IsDeleted) VALUES (1, 'Test', 0);
            INSERT INTO Categories (Id, Name, IsDeleted) VALUES (1, 'Laptop', 0);
            INSERT INTO Models (Id, ManufacturerId, CategoryId, Name, IsDeleted) VALUES (1, 1, 1, 'Test Laptop', 0);
            INSERT INTO Products (Id, Price, ModelId, Barcode, IsDeleted) VALUES (1, 100, 1, 'laptop001', 0);
            INSERT INTO Laptops (Id, Cpu, Gpu, DisplaySize, Notes, ProductId, IsDeleted)
                VALUES (1, 'Core i7', 'RTX 4070', 16, NULL, 1, 0);
            """);

        _context.Database.Migrate();

        var laptop = _context.Laptops
            .Include(item => item.CpuNavigation)
            .Include(item => item.GpuNavigation)
            .Single();
        Assert.Equal("Core i7", laptop.CpuNavigation.Name);
        Assert.Equal("RTX 4070", laptop.GpuNavigation!.Name);
    }

    [Fact]
    public void Chain_creates_the_Zone_Mobile_shop_party_on_an_empty_database()
    {
        _context.Database.Migrate();

        Assert.Equal(1, Count("""
            SELECT COUNT(*) AS Value FROM "People"
            WHERE "FirstName" = 'Zone' AND "LastName" = 'Mobile' AND "PhoneNumber" = '09115009109' AND "IsDeleted" = 0
            """));
        Assert.Equal(1, Count("""
            SELECT COUNT(*) AS Value FROM "Sellers" s JOIN "People" p ON p."Id" = s."PersonId"
            WHERE p."FirstName" = 'Zone' AND p."LastName" = 'Mobile' AND s."EntityType" = 1
            """));
        Assert.Equal(1, Count("""
            SELECT COUNT(*) AS Value FROM "Customers" c JOIN "People" p ON p."Id" = c."PersonId"
            WHERE p."FirstName" = 'Zone' AND p."LastName" = 'Mobile' AND c."NationalId" = '0099999999'
            """));
    }

    [Fact]
    public void Zone_Mobile_migration_does_not_duplicate_an_existing_shop_person()
    {
        _context.GetService<IMigrator>().Migrate("20261104100000_AddAppleIdSecurityAnswers");
        _context.Database.ExecuteSqlRaw("""
            INSERT INTO People (Id, FirstName, LastName, PhoneNumber, IsDeleted)
            VALUES (1, 'Zone', 'Mobile', '09120000000', 0);
            """);

        _context.Database.Migrate();

        Assert.Equal(1, Count("""SELECT COUNT(*) AS Value FROM "People" WHERE "FirstName" = 'Zone' AND "LastName" = 'Mobile'"""));
        Assert.Equal(1, Count("""SELECT COUNT(*) AS Value FROM "Sellers" WHERE "PersonId" = 1"""));
        Assert.Equal(1, Count("""SELECT COUNT(*) AS Value FROM "Customers" WHERE "PersonId" = 1"""));
    }

    private int Count(string sql) => _context.Database.SqlQueryRaw<int>(sql).Single();

    public void Dispose()
    {
        _context.Dispose();

        foreach (var suffix in new[] { string.Empty, "-wal", "-shm" })
        {
            var file = _databaseFile + suffix;
            if (!File.Exists(file))
            {
                continue;
            }

            try
            {
                File.Delete(file);
            }
            catch (IOException)
            {
                // Best-effort cleanup only.
            }
        }

        GC.SuppressFinalize(this);
    }
}
