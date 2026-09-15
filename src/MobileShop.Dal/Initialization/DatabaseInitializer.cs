using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using MobileShop.Dal.EfStructures;

namespace MobileShop.Dal.Initialization;

/// <summary>
/// Startup-time database provisioning for the Web app. Development-only, so a release
/// deploy never wipes a real database.
/// </summary>
public static class DatabaseInitializer
{
    /// <summary>
    /// Applies the committed EF Core migrations and seeds a fresh database with the bundled
    /// sample data when the database is empty.
    /// </summary>
    public static void InitializeForDevelopment(IServiceProvider services)
    {
        using var scope = services.CreateScope();
        var scopedServices = scope.ServiceProvider;
        var context = scopedServices.GetRequiredService<AppDbContext>();

        context.Database.Migrate();
        SampleDataInitializer.SeedIfEmpty(context);
        EnsureDefaultAdminPassword(scopedServices, context);
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
