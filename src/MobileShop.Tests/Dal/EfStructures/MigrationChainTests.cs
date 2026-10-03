using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Migrations;
using Microsoft.EntityFrameworkCore.Storage;
using Microsoft.EntityFrameworkCore.Metadata;

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
                "20261004090000_WidenMoneyToLong"
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

        Assert.Equal(7, migrationHistoryCount);

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
