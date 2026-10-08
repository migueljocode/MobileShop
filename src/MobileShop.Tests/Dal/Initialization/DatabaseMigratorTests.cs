namespace MobileShop.Tests.Dal.Initialization;

using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Migrations;

public sealed class DatabaseMigratorTests : IDisposable
{
    private readonly string _directory =
        Path.Combine(Path.GetTempPath(), $"mobileshop-migrator-{Guid.NewGuid():N}");

    public DatabaseMigratorTests()
    {
        Directory.CreateDirectory(_directory);
    }

    [Fact]
    public void Fresh_database_is_created_and_migrated_without_backup()
    {
        var databaseFile = DatabaseFile();
        using var context = CreateContext(databaseFile);

        var result = DatabaseMigrator.Migrate(context, databaseFile, NullLogger.Instance);

        Assert.Equal(DatabaseMigrationStatus.Created, result.Status);
        Assert.Null(result.BackupPath);
        Assert.Equal(CurrentMigrationCount(context), Scalar<long>(context, "SELECT COUNT(*) FROM \"__EFMigrationsHistory\""));
        Assert.Empty(BackupFiles());
    }

    [Fact]
    public void Empty_database_file_is_created_and_migrated_without_backup()
    {
        var databaseFile = DatabaseFile();
        File.WriteAllBytes(databaseFile, Array.Empty<byte>());
        using var context = CreateContext(databaseFile);
        var result = DatabaseMigrator.Migrate(context, databaseFile, NullLogger.Instance);
        Assert.Equal(DatabaseMigrationStatus.Created, result.Status);
        Assert.Null(result.BackupPath);
        Assert.Equal(CurrentMigrationCount(context), Scalar<long>(context, "SELECT COUNT(*) FROM \"__EFMigrationsHistory\""));
        Assert.Empty(BackupFiles());
    }

    [Fact]
    public void Current_database_is_up_to_date_without_a_new_backup()
    {
        var databaseFile = DatabaseFile();
        using var context = CreateContext(databaseFile);

        DatabaseMigrator.Migrate(context, databaseFile, NullLogger.Instance);
        var result = DatabaseMigrator.Migrate(context, databaseFile, NullLogger.Instance);

        Assert.Equal(DatabaseMigrationStatus.UpToDate, result.Status);
        Assert.Null(result.BackupPath);
        Assert.Empty(BackupFiles());
    }

    [Fact]
    public void Legacy_migration_database_is_backed_up_before_upgrade()
    {
        var databaseFile = DatabaseFile();
        using var context = CreateContext(databaseFile);

        context.Database.Migrate("20261001161450_AddPartNumber");

        var result = DatabaseMigrator.Migrate(context, databaseFile, NullLogger.Instance);

        Assert.Equal(DatabaseMigrationStatus.Upgraded, result.Status);
        Assert.NotNull(result.BackupPath);
        Assert.True(File.Exists(result.BackupPath));
        Assert.Empty(context.Database.GetPendingMigrations());

        using var backup = new Microsoft.Data.Sqlite.SqliteConnection($"Data Source={result.BackupPath};Pooling=False");
        backup.Open();
        Assert.Equal(
            "decimal(18,2)",
            Scalar<string>(backup, "SELECT type FROM pragma_table_info('Products') WHERE name = 'Price'"));
        Assert.Equal(
            5L,
            Scalar<long>(backup, "SELECT COUNT(*) FROM \"__EFMigrationsHistory\""));
        Assert.Equal(CurrentMigrationCount(context), Scalar<long>(context, "SELECT COUNT(*) FROM \"__EFMigrationsHistory\""));
    }

    [Fact]
    public void Ensure_created_database_is_baselined_after_verified_backup()
    {
        var databaseFile = DatabaseFile();
        using var context = CreateContext(databaseFile);

        context.Database.EnsureCreated();
        context.Database.ExecuteSqlRaw(
            "INSERT INTO \"People\" (\"FirstName\", \"LastName\", \"PhoneNumber\", \"Notes\", \"IsDeleted\") VALUES ('Legacy', 'Owner', '000', 'kept', 0)");

        var result = DatabaseMigrator.Migrate(context, databaseFile, NullLogger.Instance);

        Assert.Equal(DatabaseMigrationStatus.Baselined, result.Status);
        Assert.NotNull(result.BackupPath);
        Assert.True(File.Exists(result.BackupPath));
        Assert.Equal(CurrentMigrationCount(context), Scalar<long>(context, "SELECT COUNT(*) FROM \"__EFMigrationsHistory\""));
        Assert.Equal(
            "kept",
            Scalar<string>(context, "SELECT \"Notes\" FROM \"People\" WHERE \"FirstName\" = 'Legacy'"));
    }

    [Fact]
    public void Ensure_created_database_with_extra_column_is_refused_without_writes()
    {
        var databaseFile = DatabaseFile();
        using var context = CreateContext(databaseFile);

        context.Database.EnsureCreated();
        context.Database.ExecuteSqlRaw("ALTER TABLE \"People\" ADD COLUMN \"Unexpected\" TEXT");

        var exception = Assert.Throws<InvalidOperationException>(
            () => DatabaseMigrator.Migrate(context, databaseFile, NullLogger.Instance));

        Assert.Contains("does not match the current model", exception.Message, StringComparison.Ordinal);
        Assert.Equal(0L, Scalar<long>(context, "SELECT COUNT(*) FROM sqlite_master WHERE name = '__EFMigrationsHistory'"));
        Assert.Equal(
            1L,
            Scalar<long>(context, "SELECT COUNT(*) FROM pragma_table_info('People') WHERE name = 'Unexpected'"));
        Assert.Empty(BackupFiles());
    }

    [Fact]
    public void Ensure_current_creates_a_missing_database()
    {
        var databaseFile = DatabaseFile();
        using var context = CreateContext(databaseFile);

        var created = DatabaseMigrator.EnsureCurrent(context);

        Assert.True(created);
        Assert.True(File.Exists(databaseFile));
        Assert.Equal(CurrentMigrationCount(context), Scalar<long>(context, "SELECT COUNT(*) FROM \"__EFMigrationsHistory\""));
        Assert.Empty(BackupFiles());
    }

    [Fact]
    public void Ensure_current_migrates_an_existing_empty_database_file()
    {
        var databaseFile = DatabaseFile();
        File.WriteAllBytes(databaseFile, Array.Empty<byte>());
        using var context = CreateContext(databaseFile);

        var created = DatabaseMigrator.EnsureCurrent(context);

        Assert.True(created);
        Assert.Equal(CurrentMigrationCount(context), Scalar<long>(context, "SELECT COUNT(*) FROM \"__EFMigrationsHistory\""));
        Assert.Empty(BackupFiles());
    }

    [Fact]
    public void Ensure_current_refuses_a_database_without_history()
    {
        var databaseFile = DatabaseFile();
        using var context = CreateContext(databaseFile);

        context.Database.EnsureCreated();

        var exception = Assert.Throws<InvalidOperationException>(
            () => DatabaseMigrator.EnsureCurrent(context));

        Assert.Contains("--migrate-database", exception.Message, StringComparison.Ordinal);
        Assert.Equal(
            0L,
            Scalar<long>(context, "SELECT COUNT(*) FROM sqlite_master WHERE name = '__EFMigrationsHistory'"));
    }

    [Fact]
    public void Ensure_current_refuses_pending_migrations()
    {
        var databaseFile = DatabaseFile();
        using var context = CreateContext(databaseFile);

        context.Database.Migrate("20261001161450_AddPartNumber");

        var exception = Assert.Throws<InvalidOperationException>(
            () => DatabaseMigrator.EnsureCurrent(context));

        Assert.Contains("--migrate-database", exception.Message, StringComparison.Ordinal);
    }

    [Fact]
    public void Ensure_current_allows_a_current_database_without_writes()
    {
        var databaseFile = DatabaseFile();
        using var context = CreateContext(databaseFile);

        DatabaseMigrator.Migrate(context, databaseFile, NullLogger.Instance);

        var created = DatabaseMigrator.EnsureCurrent(context);

        Assert.False(created);
        Assert.Empty(BackupFiles());
        Assert.Empty(context.Database.GetPendingMigrations());
    }

    private AppDbContext CreateContext(string databaseFile)
    {
        var options = new DbContextOptionsBuilder<AppDbContext>()
            .UseSqlite($"Data Source={databaseFile};Pooling=False")
            .Options;
        return new AppDbContext(options);
    }

    private static long CurrentMigrationCount(AppDbContext context)
        => context.GetService<IMigrationsAssembly>().Migrations.Count;

    private string DatabaseFile()
        => Path.Combine(_directory, "MobileShop.db");

    private string[] BackupFiles()
        => Directory.GetFiles(_directory, "MobileShop.db.*.bak");

    private static T Scalar<T>(AppDbContext context, string sql)
    {
        var connection = context.Database.GetDbConnection();
        var wasClosed = connection.State == System.Data.ConnectionState.Closed;
        if (wasClosed)
        {
            connection.Open();
        }

        try
        {
            using var command = connection.CreateCommand();
            command.CommandText = sql;
            return (T)Convert.ChangeType(command.ExecuteScalar()!, typeof(T));
        }
        finally
        {
            if (wasClosed)
            {
                connection.Close();
            }
        }
    }

    private static T Scalar<T>(Microsoft.Data.Sqlite.SqliteConnection connection, string sql)
    {
        using var command = connection.CreateCommand();
        command.CommandText = sql;
        return (T)Convert.ChangeType(command.ExecuteScalar()!, typeof(T));
    }

    public void Dispose()
    {
        foreach (var file in Directory.Exists(_directory)
                     ? Directory.GetFiles(_directory)
                     : Array.Empty<string>())
        {
            try
            {
                File.Delete(file);
            }
            catch (IOException)
            {
            }
        }

        try
        {
            Directory.Delete(_directory, recursive: true);
        }
        catch (IOException)
        {
        }

        GC.SuppressFinalize(this);
    }
}
