namespace MobileShop.Dal.Initialization;

/// <summary>
/// Startup-time database provisioning for the Web app. Development-only, so a release
/// deploy never wipes a real database.
/// </summary>
public static class DatabaseInitializer
{
    /// <summary>
    /// Recreates the development database from the current model on every startup (drop and
    /// create - no migrations) and seeds the bundled sample data. Because nothing persists
    /// between runs, seeding and the default admin password are applied unconditionally.
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
        EnsureDefaultAdminPassword(scopedServices, context);
    }

    /// <summary>
    /// Gives the admin account a real Argon2 hash of the default development password. The sample
    /// data ships with a placeholder and the database is recreated on every run, so this is
    /// applied unconditionally.
    /// </summary>
    private static void EnsureDefaultAdminPassword(IServiceProvider services, AppDbContext context)
    {
        const string DefaultPassword = "Admin@123";

        var admin = context.Users.Single(user => user.Username == "admin");

        var hasher = ResolvePasswordHasher(services);
        var hashMethod = hasher.GetType().GetMethod("Hash", new[] { typeof(string) });
        var hashedPassword = hashMethod?.Invoke(hasher, new object[] { DefaultPassword }) as string;

        if (string.IsNullOrWhiteSpace(hashedPassword))
        {
            throw new InvalidOperationException("The configured password hasher could not generate a hash for the default admin account.");
        }

        admin.PasswordHash = hashedPassword;
        context.SaveChanges();
    }

    private static object ResolvePasswordHasher(IServiceProvider services)
    {
        var hasherType = Type.GetType("MobileShop.Services.Security.IPasswordHasher, MobileShop.Services", throwOnError: false);
        if (hasherType is null)
        {
            throw new InvalidOperationException("The MobileShop Services assembly could not be resolved when initializing the default admin password.");
        }

        var hasher = services.GetRequiredService(hasherType);
        return hasher;
    }
}
