namespace MobileShop.Web;

/// <summary>
/// Startup-time database provisioning for the Web app. Development-only, so a release
/// deploy never wipes a real database.
/// </summary>
public static class DatabaseInitializer
{
    /// <summary>Applies the committed EF Core migration and seeds a fresh database with the bundled sample data.</summary>
    public static void InitializeForDevelopment(WebApplication app)
    {
        using var scope = app.Services.CreateScope();
        var services = scope.ServiceProvider;
        var context = services.GetRequiredService<AppDbContext>();

        context.Database.Migrate();                     // apply the committed Initial migration
        SampleDataInitializer.SeedIfEmpty(context);     // seed only when empty - re-runs are non-destructive
        EnsureDefaultAdminPassword(services, context);
    }

    /// <summary>
    /// First run only: the bundled sample data ships with a placeholder hash, so give the admin
    /// account a real Argon2 hash of a working default password.
    /// </summary>
    private static void EnsureDefaultAdminPassword(IServiceProvider services, AppDbContext context)
    {
        const string Placeholder = "REPLACE_WITH_REAL_HASH";
        const string DefaultPassword = "Admin@123";

        var admin = context.Users.FirstOrDefault(user => user.Username == "admin");
        if (admin is null || admin.PasswordHash != Placeholder)
        {
            return;
        }

        var hasher = services.GetRequiredService<IPasswordHasher>();
        admin.PasswordHash = hasher.Hash(DefaultPassword);
        context.SaveChanges();
    }
}