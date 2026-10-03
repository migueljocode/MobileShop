using Microsoft.EntityFrameworkCore.Diagnostics;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Migrations;

namespace MobileShop.Tests.Dal.EfStructures;

/// <summary>
/// Stage S, Step 2 — verifies that the legacy fractional-money schema upgrades without data loss.
/// </summary>
public class LegacyMoneyUpgradeTests
{
    [Fact]
    public void Legacy_money_upgrade_preserves_rows_and_rounds_fractions()
    {
        var dbFile = Path.Combine(Path.GetTempPath(), $"mobileshop-legacy-money-{Guid.NewGuid():N}.db");
        var connectionString = $"Data Source={dbFile};Pooling=False";

        try
        {
            using (var legacyContext = CreateContext(connectionString))
            {
                var migrator = legacyContext.Database.GetService<IMigrator>();
                migrator.Migrate("20261001161450_AddPartNumber");

                legacyContext.Database.ExecuteSqlRaw(
                    """
                    INSERT INTO Manufacturers (Id, Name, IsDeleted)
                    VALUES (1, 'Apple', 0);

                    INSERT INTO Categories (Id, Name, IsDeleted)
                    VALUES (1, 'Mobile', 0);

                    INSERT INTO Models (Id, ManufacturerId, CategoryId, Name, IsDeleted)
                    VALUES (1, 1, 1, 'iPhone 17', 0);

                    INSERT INTO Products (Id, Price, ModelId, Barcode, IsDeleted)
                    VALUES
                        (1, 800000.75, 1, 'legacy-product-1', 0),
                        (2, 1500000.5, 1, 'legacy-product-2', 0);

                    INSERT INTO People (Id, FirstName, LastName, PhoneNumber, IsDeleted)
                    VALUES
                        (1, 'Legacy', 'Seller', '09120000001', 0),
                        (2, 'Legacy', 'Customer', '09120000002', 0);

                    INSERT INTO Sellers (Id, PersonId, EntityType, IsDeleted)
                    VALUES (1, 1, 0, 0);

                    INSERT INTO Customers (Id, PersonId, NationalId, IsDeleted)
                    VALUES (1, 2, '0012345678', 0);

                    INSERT INTO Transactions (Id, Date, SellerId, CustomerId, ProductId, FinishedPrice, Direction, IsDeleted)
                    VALUES
                        (1, '2026-10-01 10:00:00', 1, 1, 1, 799999.49, 1, 0),
                        (2, '2026-10-01 11:00:00', 1, 1, 2, 1200000.5, 0, 0);
                    """);
            }

            using (var migratedContext = CreateContext(connectionString))
            {
                var beforeCounts = new Dictionary<string, long>
                {
                    ["People"] = Scalar<long>(migratedContext, "SELECT COUNT(*) FROM People;"),
                    ["Products"] = Scalar<long>(migratedContext, "SELECT COUNT(*) FROM Products;"),
                    ["Transactions"] = Scalar<long>(migratedContext, "SELECT COUNT(*) FROM Transactions;")
                };

                migratedContext.Database.Migrate();

                Assert.Empty(migratedContext.Database.GetPendingMigrations());

                Assert.Equal(beforeCounts["People"], Scalar<long>(migratedContext, "SELECT COUNT(*) FROM People;"));
                Assert.Equal(beforeCounts["Products"], Scalar<long>(migratedContext, "SELECT COUNT(*) FROM Products;"));
                Assert.Equal(beforeCounts["Transactions"], Scalar<long>(migratedContext, "SELECT COUNT(*) FROM Transactions;"));

                Assert.Equal(800001L, Scalar<long>(migratedContext, "SELECT Price FROM Products WHERE Id = 1;"));
                Assert.Equal(1500001L, Scalar<long>(migratedContext, "SELECT Price FROM Products WHERE Id = 2;"));
                Assert.Equal(799999L, Scalar<long>(migratedContext, "SELECT FinishedPrice FROM Transactions WHERE Id = 1;"));
                Assert.Equal(1200001L, Scalar<long>(migratedContext, "SELECT FinishedPrice FROM Transactions WHERE Id = 2;"));

                Assert.Equal("INTEGER", Scalar<string>(migratedContext, "SELECT type FROM pragma_table_info('Products') WHERE name = 'Price';"));
                Assert.Equal("INTEGER", Scalar<string>(migratedContext, "SELECT type FROM pragma_table_info('Transactions') WHERE name = 'FinishedPrice';"));

                Assert.Equal(0L, Scalar<long>(migratedContext, "SELECT COUNT(*) FROM pragma_foreign_key_check;"));
                Assert.Equal("ok", Scalar<string>(migratedContext, "PRAGMA integrity_check;"));

                Assert.Equal(1L, Scalar<long>(
                    migratedContext,
                    "SELECT COUNT(*) FROM sqlite_master WHERE type = 'table' AND name = 'Transactions' AND sql LIKE '%CK_Transactions_FinishedPrice_NonNegative%';"));
                Assert.Equal(1L, Scalar<long>(
                    migratedContext,
                    "SELECT COUNT(*) FROM sqlite_master WHERE type = 'table' AND name = 'Products' AND sql LIKE '%CK_Products_Price_NonNegative%';"));
            }
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

    private static AppDbContext CreateContext(string connectionString)
        => new(
            new DbContextOptionsBuilder<AppDbContext>()
                .UseSqlite(connectionString)
                .ConfigureWarnings(w => w.Ignore(RelationalEventId.PendingModelChangesWarning))
                .Options);

    private static T Scalar<T>(AppDbContext context, string sql)
    {
        using var command = context.Database.GetDbConnection().CreateCommand();
        command.CommandText = sql;

        if (command.Connection!.State != System.Data.ConnectionState.Open)
        {
            command.Connection.Open();
        }

        var value = command.ExecuteScalar();
        return (T)Convert.ChangeType(value!, typeof(T));
    }
}
