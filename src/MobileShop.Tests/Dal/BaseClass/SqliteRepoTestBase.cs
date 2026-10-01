namespace MobileShop.Tests.Dal.BaseClass;

/// <summary>
/// Shared base for EF Core relational tests that need a real SQLite provider
/// (e.g. for unique-index enforcement, migrations, or query filters not supported
/// by the EF Core InMemory provider). Mirrors RepoTestBase but uses a temp-file SQLite database.
/// </summary>
public abstract class SqliteRepoTestBase : IDisposable
{
    private readonly string _databaseFile =
        Path.Combine(Path.GetTempPath(), $"mobileshop-test-{Guid.NewGuid():N}.db");

    private string ConnectionString => $"Data Source={_databaseFile};Pooling=False";

    protected AppDbContext Context { get; }

    protected SqliteRepoTestBase()
    {
        var options = new DbContextOptionsBuilder<AppDbContext>()
            .UseSqlite(ConnectionString)
            .Options;
        Context = new AppDbContext(options);
        Context.Database.EnsureCreated();
    }

    public void Dispose()
    {
        Context.Dispose();
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
