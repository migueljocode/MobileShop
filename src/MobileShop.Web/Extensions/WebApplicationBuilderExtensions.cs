namespace MobileShop.Web.Extensions;

/// <summary>
/// Configures the MobileShop Razor Pages host.
/// </summary>
public static class WebApplicationBuilderExtensions
{
    /// <summary>
    /// Registers logging, Razor Pages, the database, repositories and data services.
    /// </summary>
    public static WebApplicationBuilder ConfigureBuilder(this WebApplicationBuilder builder)
    {
        QuestPdfSetup.UseCommunityLicense();
        QuestPdfSetup.RegisterFonts();
        builder.ConfigureSerilog();
        builder.Services.AddRazorPages();
        builder.Services.AddMobileShop(builder.Configuration);
        return builder;
    }

    /// <summary>
    /// Handles the explicit production database migration command without starting the web host.
    /// </summary>
    public static bool TryRunDatabaseCommand(this WebApplication app, string[] args)
    {
        if (!args.Contains("--migrate-database", StringComparer.Ordinal))
        {
            return false;
        }

        try
        {
            using var scope = app.Services.CreateScope();
            var context = scope.ServiceProvider.GetRequiredService<AppDbContext>();
            var result = DatabaseMigrator.Migrate(context, SolutionPaths.DatabaseFile, app.Logger);

            Console.WriteLine(result.Status switch
            {
                DatabaseMigrationStatus.Created => "Database created and all migrations applied.",
                DatabaseMigrationStatus.UpToDate => "Database is already up to date.",
                DatabaseMigrationStatus.Upgraded => $"Database upgraded successfully. Verified backup: {result.BackupPath}",
                DatabaseMigrationStatus.Baselined => $"Legacy database baselined successfully. Verified backup: {result.BackupPath}",
                _ => throw new ArgumentOutOfRangeException()
            });

            Environment.ExitCode = 0;
        }
        catch (Exception exception) when (exception is InvalidOperationException
            or IOException
            or Microsoft.Data.Sqlite.SqliteException
            or Microsoft.EntityFrameworkCore.DbUpdateException)
        {
            Console.Error.WriteLine(exception.Message);
            Environment.ExitCode = 1;
        }

        return true;
    }

    /// <summary>
    /// Configures the web request pipeline without enabling authentication.
    /// </summary>
    public static WebApplication ConfigureApp(this WebApplication app)
    {
        if (app.Environment.IsDevelopment())
        {
            DatabaseInitializer.InitializeForDevelopment(app.Services);

            // Dev-only: the freshly seeded sample data ships a placeholder hash, so the admin account gets a real one.
            using var scope = app.Services.CreateScope();
            scope.ServiceProvider.GetRequiredService<IAccountDataService>().EnsureAdminUser();
        }
        else
        {
            using var scope = app.Services.CreateScope();
            var context = scope.ServiceProvider.GetRequiredService<AppDbContext>();
            DatabaseMigrator.EnsureCurrent(context);
        }

        app.UseRouting();
        app.MapStaticAssets();
        app.MapRazorPages().WithStaticAssets();
        return app;
    }
}
