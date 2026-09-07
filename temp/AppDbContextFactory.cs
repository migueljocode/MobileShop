namespace MobileShop.Dal.EfStructures;

// used by EF Core tooling at design time (e.g. `dotnet ef migrations add`), when there's no DI
// container around yet to hand AppDbContext its options the normal way
public class AppDbContextFactory : IDesignTimeDbContextFactory<AppDbContext>
{
    public AppDbContext CreateDbContext(string[] args)
    {
        var optionsBuilder = new DbContextOptionsBuilder<AppDbContext>();
        optionsBuilder.UseSqlite($"Data Source={SolutionPaths.DatabaseFile}");
        return new AppDbContext(optionsBuilder.Options);
    }
}
