using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.Extensions.Logging;

namespace MobileShop.Dal.Initialization;

public enum DatabaseMigrationStatus
{
    Created,
    UpToDate,
    Upgraded,
    Baselined
}

public sealed record DatabaseMigrationResult(DatabaseMigrationStatus Status, string? BackupPath);

/// <summary>
/// Performs the only supported production schema changes and checks production databases without mutating them.
/// </summary>
public static class DatabaseMigrator
{
    private const string CurrentMigrationMessage =
        "The database schema is not current. Back up MobileShop.db and run: dotnet run --project src/MobileShop.Web -- --migrate-database";

    public static DatabaseMigrationResult Migrate(
        AppDbContext context,
        string databaseFile,
        Microsoft.Extensions.Logging.ILogger logger)
    {
        if (!File.Exists(databaseFile))
        {
            context.Database.Migrate();
            logger.LogInformation("Database was created and all migrations were applied.");
            return new DatabaseMigrationResult(DatabaseMigrationStatus.Created, null);
        }

        var historyExists = context.Database
            .SqlQueryRaw<int>("SELECT COUNT(*) AS Value FROM sqlite_master WHERE type = 'table' AND name = '__EFMigrationsHistory'")
            .Single() > 0;

        if (historyExists)
        {
            if (!context.Database.GetPendingMigrations().Any())
            {
                logger.LogInformation("Database is already up to date.");
                return new DatabaseMigrationResult(DatabaseMigrationStatus.UpToDate, null);
            }

            var backupPath = CreateVerifiedBackup(databaseFile);
            context.Database.Migrate();
            logger.LogInformation("Database upgraded successfully. Verified backup: {BackupPath}", backupPath);
            return new DatabaseMigrationResult(DatabaseMigrationStatus.Upgraded, backupPath);
        }

        var tableCount = context.Database
            .SqlQueryRaw<int>("SELECT COUNT(*) AS Value FROM sqlite_master WHERE type = 'table' AND name NOT LIKE 'sqlite_%'")
            .Single();

        if (tableCount == 0)
        {
            context.Database.Migrate();
            logger.LogInformation("Database was an existing empty file and has been created with all migrations applied.");
            return new DatabaseMigrationResult(DatabaseMigrationStatus.Created, null);
        }

        var differences = CompareWithCurrentSchema(context, databaseFile);
        if (differences.Count > 0)
        {
            var details = string.Join(Environment.NewLine, differences.Take(20).Select(static d => $"- {d}"));
            throw new InvalidOperationException(
                $"The database schema does not match the current model. Refusing to baseline it without changing the database.{Environment.NewLine}{details}");
        }

        var baselineBackup = CreateVerifiedBackup(databaseFile);
        BaselineHistory(context);
        logger.LogInformation("Legacy database baselined successfully. Verified backup: {BackupPath}", baselineBackup);
        return new DatabaseMigrationResult(DatabaseMigrationStatus.Baselined, baselineBackup);
    }

    /// <summary>
    /// Read-only production guard. It never creates, migrates, baselines, or otherwise writes the database.
    /// </summary>
    public static void EnsureCurrent(AppDbContext context)
    {
        var databaseFile = context.Database.GetDbConnection().DataSource;
        if (string.IsNullOrWhiteSpace(databaseFile) || !File.Exists(databaseFile))
        {
            throw new InvalidOperationException(CurrentMigrationMessage);
        }

        var historyExists = context.Database
            .SqlQueryRaw<int>("SELECT COUNT(*) AS Value FROM sqlite_master WHERE type = 'table' AND name = '__EFMigrationsHistory'")
            .Single() > 0;

        if (!historyExists || context.Database.GetPendingMigrations().Any())
        {
            throw new InvalidOperationException(CurrentMigrationMessage);
        }
    }

    private static string CreateVerifiedBackup(string databaseFile)
    {
        var backupPath = $"{databaseFile}.{DateTime.Now:yyyyMMddHHmmss}.bak";
        if (File.Exists(backupPath))
        {
            throw new InvalidOperationException(
                $"Refusing to migrate because the backup target already exists: {backupPath}");
        }

        try
        {
            using (var source = new Microsoft.Data.Sqlite.SqliteConnection($"Data Source={databaseFile};Pooling=False"))
            using (var destination = new Microsoft.Data.Sqlite.SqliteConnection($"Data Source={backupPath};Pooling=False"))
            {
                source.Open();
                destination.Open();
                source.BackupDatabase(destination);
            }

            using var verification = new Microsoft.Data.Sqlite.SqliteConnection($"Data Source={backupPath};Pooling=False");
            verification.Open();
            using var command = verification.CreateCommand();
            command.CommandText = "PRAGMA integrity_check;";
            var result = command.ExecuteScalar()?.ToString();
            if (!string.Equals(result, "ok", StringComparison.OrdinalIgnoreCase))
            {
                throw new InvalidOperationException(
                    $"Backup verification failed for {backupPath}: PRAGMA integrity_check returned '{result ?? "<null>"}'.");
            }

            return backupPath;
        }
        catch
        {
            try
            {
                if (File.Exists(backupPath))
                {
                    File.Delete(backupPath);
                }
            }
            catch
            {
                // Preserve the original backup failure.
            }

            throw;
        }
    }

    private static List<string> CompareWithCurrentSchema(AppDbContext context, string databaseFile)
    {
        var referenceFile = Path.Combine(
            Path.GetDirectoryName(databaseFile) ?? Path.GetTempPath(),
            $"mobileshop-schema-reference-{Guid.NewGuid():N}.db");

        try
        {
            var options = new DbContextOptionsBuilder<AppDbContext>()
                .UseSqlite($"Data Source={referenceFile};Pooling=False")
                .Options;

            using var referenceContext = new AppDbContext(options);
            referenceContext.Database.EnsureCreated();

            var actual = ReadSchemaFingerprint(context);
            var expected = ReadSchemaFingerprint(referenceContext);

            return expected
                .Except(actual)
                .Concat(actual.Except(expected))
                .OrderBy(static line => line, StringComparer.Ordinal)
                .ToList();
        }
        finally
        {
            DeleteDatabaseFiles(referenceFile);
        }
    }

    private static SortedSet<string> ReadSchemaFingerprint(AppDbContext context)
    {
        var lines = new SortedSet<string>(StringComparer.Ordinal);

        var tables = context.Database
            .SqlQueryRaw<string>("SELECT name AS Value FROM sqlite_master WHERE type = 'table' AND name NOT LIKE 'sqlite_%' ORDER BY name")
            .ToList();

        foreach (var table in tables)
        {
            var columns = context.Database
                .SqlQueryRaw<SchemaColumn>(
                    "SELECT name AS Name, type AS DeclaredType, \"notnull\" AS NotNullFlag, pk AS PrimaryKey FROM pragma_table_info({0}) ORDER BY cid",
                    table)
                .ToList();

            foreach (var column in columns)
            {
                lines.Add(
                    $"table|{table}|column|{column.Name}|type|{column.DeclaredType}|notnull|{column.NotNullFlag}|pk|{column.PrimaryKey}");
            }
        }

        var indexes = context.Database
            .SqlQueryRaw<string>(
                "SELECT name AS Value FROM sqlite_master WHERE type = 'index' AND name NOT LIKE 'sqlite_%' ORDER BY name")
            .ToList();

        foreach (var index in indexes)
        {
            lines.Add($"index|{index}");
        }

        return lines;
    }

    private static void BaselineHistory(AppDbContext context)
    {
        var history = context.GetService<Microsoft.EntityFrameworkCore.Migrations.IHistoryRepository>();
        var migrations = context.GetService<Microsoft.EntityFrameworkCore.Migrations.IMigrationsAssembly>()
            .Migrations
            .Keys
            .ToArray();
        var productVersion = context.GetService<Microsoft.EntityFrameworkCore.Migrations.IMigrationsAssembly>()
            .ModelSnapshot?.Model["ProductVersion"]?.ToString()
            ?? typeof(Microsoft.EntityFrameworkCore.DbContext).Assembly.GetName().Version?.ToString(3)
            ?? "10.0.0";

        using var transaction = context.Database.BeginTransaction();
        context.Database.ExecuteSqlRaw(history.GetCreateScript());

        foreach (var migrationId in migrations)
        {
            var row = new Microsoft.EntityFrameworkCore.Migrations.HistoryRow(migrationId, productVersion);
            context.Database.ExecuteSqlRaw(history.GetInsertScript(row));
        }

        transaction.Commit();
    }

    internal sealed class SchemaColumn
    {
        public string Name { get; set; } = string.Empty;
        public string DeclaredType { get; set; } = string.Empty;
        public int NotNullFlag { get; set; }
        public int PrimaryKey { get; set; }
    }

    private static void DeleteDatabaseFiles(string databaseFile)
    {
        foreach (var suffix in new[] { string.Empty, "-wal", "-shm" })
        {
            var path = databaseFile + suffix;
            if (File.Exists(path))
            {
                File.Delete(path);
            }
        }
    }
}
