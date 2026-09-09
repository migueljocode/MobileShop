namespace MobileShop.Tests.Dal.BaseClass;

/// <summary>
/// Shared base for repository tests - gives each test method its own isolated, in-memory
/// <see cref="AppDbContext"/> (xUnit creates a fresh instance of the test class per test method,
/// so the constructor here effectively runs once per test) so tests never interfere with one another.
/// </summary>
public abstract class RepoTestBase : IDisposable
{
    protected AppDbContext Context { get; }

    protected RepoTestBase()
    {
        var options = new DbContextOptionsBuilder<AppDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;
        Context = new AppDbContext(options);
    }

    public void Dispose()
    {
        Context.Dispose();
        GC.SuppressFinalize(this);
    }
}
