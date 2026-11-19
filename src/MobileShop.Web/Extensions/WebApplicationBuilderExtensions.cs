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
        builder.UseDefaultUrlWhenUnset();
        QuestPdfSetup.UseCommunityLicense();
        QuestPdfSetup.RegisterFonts();
        builder.ConfigureSerilog();

        builder.Services.AddRazorPages(options =>
        {
            options.Conventions.AuthorizeFolder("/");
            options.Conventions.AllowAnonymousToPage("/Account/Login");
        })
        .AddMvcOptions(options => options.ModelBinderProviders.Insert(0, new JalaliDateTimeModelBinderProvider()));

        builder.Services.AddMobileShopDataProtection();
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
    /// Handles one-off host commands (<c>--version</c>, <c>--backup-database</c>, <c>--migrate-database</c>)
    /// without starting the web host.
    /// </summary>
    public static bool TryRunDatabaseCommand(this WebApplication app, string[] args)
    {
        if (args.Contains("--version", StringComparer.Ordinal))
        {
            Console.WriteLine($"MobileShop {VersionInfo.Describe(typeof(WebApplicationBuilderExtensions).Assembly)}");
            Environment.ExitCode = 0;
            return true;
        }

        var migrateDatabase = args.Contains("--migrate-database", StringComparer.Ordinal);
        var backupDatabase = args.Contains("--backup-database", StringComparer.Ordinal);
        if (!migrateDatabase && !backupDatabase)
        {
            return false;
        }

        try
        {
            using var scope = app.Services.CreateScope();
            var context = scope.ServiceProvider.GetRequiredService<AppDbContext>();

            if (backupDatabase && !migrateDatabase)
            {
                var backupPath = DatabaseMigrator.BackupVerified(context);
                Console.WriteLine($"Verified backup: {backupPath}");
                Environment.ExitCode = 0;
                return true;
            }

            var result = DatabaseMigrator.Migrate(context, SolutionPaths.DatabaseFile, app.Logger);
            if (result.Status == DatabaseMigrationStatus.Created)
                scope.ServiceProvider.GetRequiredService<IAccountDataService>().EnsureAdminUser();
            EnsureShopParty(scope.ServiceProvider);
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
    /// Configures the authenticated web request pipeline. Returns <see langword="null"/> when the
    /// one-off database command ran instead, so the host must not start.
    /// </summary>
    public static WebApplication? ConfigureApp(this WebApplication app, string[] args)
    {
        if (app.TryRunDatabaseCommand(args))
        {
            return null;
        }

        if (app.Environment.IsDevelopment())
        {
            DatabaseInitializer.EnsureDevelopmentIsSafe(SolutionPaths.IsPublishedLayout);
            DatabaseInitializer.InitializeForDevelopment(app.Services);
            using var scope = app.Services.CreateScope();
            scope.ServiceProvider.GetRequiredService<IAccountDataService>().EnsureAdminUser();
            EnsureShopProfile(scope.ServiceProvider);
        }
        else
        {
            using var scope = app.Services.CreateScope();
            var context = scope.ServiceProvider.GetRequiredService<AppDbContext>();
            if (DatabaseMigrator.EnsureCurrent(context, SolutionPaths.IsPublishedLayout))
                scope.ServiceProvider.GetRequiredService<IAccountDataService>().EnsureAdminUser();
            EnsureShopParty(scope.ServiceProvider);
        }

        app.UseRouting();
        app.UseAuthentication();
        app.UseAuthorization();

        app.MapStaticAssets();
        app.MapRazorPages().WithStaticAssets();

        app.PrintRunningAddress();
        return app;
    }

    private static void EnsureShopProfile(IServiceProvider services)
        => services.GetRequiredService<IShopProfileDataService>().EnsureAsync().GetAwaiter().GetResult();

    /// <summary>
    /// Ensures the shop profile row is seeded and synced to <see cref="PdfSettings"/>,
    /// then ensures the Zone Mobile shop party exists with the profile's phone number.
    /// </summary>
    private static void EnsureShopParty(IServiceProvider services)
    {
        EnsureShopProfile(services);
        services.GetRequiredService<IShopPartyDataService>().EnsureAsync().GetAwaiter().GetResult();
    }
}
