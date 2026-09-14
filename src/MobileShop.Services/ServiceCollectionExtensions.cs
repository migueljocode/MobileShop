namespace MobileShop.Services;

/// <summary>
/// DI registration for the MobileShop data stack - the EF Core context, the repositories,
/// password hashing and the data-service layer.
/// </summary>
/// <remarks>
/// The concrete data services that get registered are chosen by the <c>UseApi</c> flag in
/// appsettings.json of the hosting app (Web or Api):
/// <list type="bullet">
/// <item><see langword="false"/> (default) → the production-ready <c>MobileShop.Services.DataServices.Dal</c> services.</item>
/// <item><see langword="true"/> → the <c>MobileShop.Services.DataServices.Api</c> services (implemented later - not yet available).</item>
/// </list>
/// </remarks>
public static class ServiceCollectionExtensions
{
    /// <summary>Registers the whole MobileShop data stack. Reads the <c>UseApi</c> flag from configuration.</summary>
    public static IServiceCollection AddMobileShop(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        var useApi = configuration.GetValue("UseApi", false);

        return services
            .AddMobileShopDbContext()
            .AddMobileShopRepositories()
            .AddMobileShopSecurity()
            .AddMobileShopDataServices(useApi);
    }

    /// <summary>Registers the EF Core context over the SQLite database living next to the solution.</summary>
    private static IServiceCollection AddMobileShopDbContext(this IServiceCollection services)
    {
        services.AddDbContext<AppDbContext>(options =>
            options.UseSqlite($"Data Source={SolutionPaths.DatabaseFile}"));
        return services;
    }

    /// <summary>Registers every repository in the Dal layer.</summary>
    private static IServiceCollection AddMobileShopRepositories(this IServiceCollection services)
    {
        services.AddScoped<IPersonRepo, PersonRepo>();
        services.AddScoped<ISellerRepo, SellerRepo>();
        services.AddScoped<ICustomerRepo, CustomerRepo>();
        services.AddScoped<IUserRepo, UserRepo>();
        services.AddScoped<IProductRepo, ProductRepo>();
        services.AddScoped<ITransactionRepo, TransactionRepo>();
        services.AddScoped<IAppleIdRepo, AppleIdRepo>();
        services.AddScoped<IPhoneRepo, PhoneRepo>();
        services.AddScoped<ISecondHandRepo, SecondHandRepo>();
        services.AddScoped<IGuaranteeRepo, GuaranteeRepo>();
        services.AddScoped<IIPhoneProfileRepo, IPhoneProfileRepo>();
        return services;
    }

    /// <summary>Registers the security primitives (Argon2 password hashing).</summary>
    private static IServiceCollection AddMobileShopSecurity(this IServiceCollection services)
    {
        services.AddSingleton<IPasswordHasher, PasswordHasher>();
        return services;
    }

    /// <summary>
    /// Registers the data services behind their interfaces.
    /// <paramref name="useApi"/> decides between the Dal implementations (production-ready)
    /// and the Api implementations (not implemented yet).
    /// </summary>
    private static IServiceCollection AddMobileShopDataServices(
        this IServiceCollection services,
        bool useApi)
    {
        if (useApi)
        {
            throw new NotSupportedException(
                "\"UseApi\": true selects the Api data services " +
                "(MobileShop.Services.DataServices.Api), but they are not implemented yet. " +
                "Set \"UseApi\": false in appsettings.json to use the production-ready " +
                "Dal data services instead.");
        }

        services.AddScoped<IUserDataService, UserDataService>();
        services.AddScoped<ICustomerDataService, CustomerDataService>();
        services.AddScoped<ISellerDataService, SellerDataService>();
        services.AddScoped<ITransactionDataService, TransactionDataService>();
        services.AddScoped<IPhoneDataService, PhoneDataService>();
        services.AddScoped<IAppleIdDataService, AppleIdDataService>();
        return services;
    }
}