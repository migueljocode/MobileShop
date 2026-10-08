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
        builder.Services.AddRazorPages(options =>
        {
            options.Conventions.AuthorizeFolder("/");
            options.Conventions.AllowAnonymousToPage("/Account/Login");
        });
        builder.Services
            .AddAuthentication(CookieAuthenticationDefaults.AuthenticationScheme)
            .AddCookie(options =>
            {
                options.LoginPath = "/Account/Login";
                options.AccessDeniedPath = "/Account/Login";
                options.Cookie.Name = "MobileShop.Auth";
                options.Cookie.HttpOnly = true;
                options.Cookie.SameSite = SameSiteMode.Lax;
                options.SlidingExpiration = true;
                options.ExpireTimeSpan = TimeSpan.FromHours(8);
            });
        builder.Services.AddAuthorization();
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
    /// Configures the authenticated web request pipeline.
    /// </summary>
    public static WebApplication ConfigureApp(this WebApplication app)
    {
        if (app.Environment.IsDevelopment())
        {
            DatabaseInitializer.InitializeForDevelopment(app.Services);

        }
        else
        {
            using var scope = app.Services.CreateScope();
            var context = scope.ServiceProvider.GetRequiredService<AppDbContext>();
            DatabaseMigrator.EnsureCurrent(context);
        }

        using (var scope = app.Services.CreateScope())
            scope.ServiceProvider.GetRequiredService<IAccountDataService>().EnsureAdminUser();

        app.UseRouting();
        app.UseAuthentication();
        app.UseAuthorization();
        app.MapStaticAssets();
        app.MapRazorPages().WithStaticAssets();
        return app;
    }
}
