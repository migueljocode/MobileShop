namespace MobileShop.Dal.Initialization;

/// <summary>
/// Startup-time database provisioning for the Web app. Development-only, so a release
/// deploy never wipes a real database.
/// </summary>
public static class DatabaseInitializer
{
    /// <summary>
    /// Recreates the development database from the current model on every startup (drop and
    /// create - no migrations) and seeds the bundled sample data.
    /// </summary>
    public static void InitializeForDevelopment(IServiceProvider services)
    {
        using var scope = services.CreateScope();
        var scopedServices = scope.ServiceProvider;
        var context = scopedServices.GetRequiredService<AppDbContext>();

        // Dev workflow: drop and recreate on every run - there is no persisted state to protect.
        context.Database.EnsureDeleted();
        context.Database.EnsureCreated();
        SampleDataInitializer.SeedIfEmpty(context);
    }
}
