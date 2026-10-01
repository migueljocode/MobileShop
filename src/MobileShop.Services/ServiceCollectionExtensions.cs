namespace MobileShop.Services;

/// <summary>
/// DI registration for the MobileShop data stack - the EF Core context, the repositories,
/// password hashing and the data-service layer.
/// </summary>
/// <remarks>
/// The concrete data services that get registered are chosen by the <c>UseApi</c> flag. Only the Web
/// host sets it in appsettings.json; the Api host does not carry the key and therefore falls back to
/// the default (<see langword="false"/>), which selects the production-ready Dal services.
/// <list type="bullet">
/// <item><see langword="false"/> (default) → the production-ready <c>MobileShop.Services.DataServices.Dal</c> services.</item>
/// <item><see langword="true"/> → the <c>MobileShop.Services.DataServices.Api</c> services, which currently expose stub implementations whose members throw <see cref="NotImplementedException"/>.</item>
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
            .AddMobileShopRepository()
            .AddMobileShopSecurity()
            .AddMobileShopPdf(configuration)
            .AddMobileShopDistribution(configuration)
            .AddMobileShopDataServices(useApi);
    }

    private static IServiceCollection AddMobileShopPdf(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        QuestPdfSetup.UseCommunityLicense();
        services.Configure<PdfSettings>(
            configuration.GetSection("Pdf"));
        services.AddScoped<IPdfGenerator, QuestPdfGenerator>();
        return services;
    }

    private static IServiceCollection AddMobileShopDistribution(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        services.Configure<DistributionSettings>(configuration.GetSection("Distribution"));
        return services;
    }

    /// <summary>Registers the EF Core context over the SQLite database living next to the solution.</summary>
    private static IServiceCollection AddMobileShopDbContext(this IServiceCollection services)
    {
        services.AddDbContext<AppDbContext>(options =>
            options.UseSqlite($"Data Source={SolutionPaths.DatabaseFile}"));
        return services;
    }

    /// <summary>
    /// Registers the repository layer. Only the generic <see cref="IBaseRepo{T}"/> registration
    /// remains; the per-entity repositories were removed in Stage H once their consumers had migrated.
    /// </summary>
    private static IServiceCollection AddMobileShopRepository(this IServiceCollection services)
    {
        services.AddScoped(typeof(IBaseRepo<>), typeof(BaseRepo<>));
        return services;
    }

    /// <summary>Registers the security primitives (Argon2 password hashing).</summary>
    private static IServiceCollection AddMobileShopSecurity(this IServiceCollection services)
    {
        services.AddSingleton<IPasswordHasher, ArgonPasswordHasher>();
        return services;
    }

    /// <summary>
    /// Registers the data services behind their interfaces.
    /// <paramref name="useApi"/> decides between the Dal implementations (production-ready)
    /// and the Api implementations whose members currently throw <see cref="NotImplementedException"/>.
    /// </summary>
    private static IServiceCollection AddMobileShopDataServices(
        this IServiceCollection services,
        bool useApi)
    {
        if (useApi)
        {
            services.AddScoped<IUserDataService, ApiUserDataService>();
            services.AddScoped<ICustomerDataService, ApiCustomerDataService>();
            services.AddScoped<ISellerDataService, ApiSellerDataService>();
            services.AddScoped<ITransactionDataService, ApiTransactionDataService>();
            services.AddScoped<IProductDataService, ApiProductDataService>();
            services.AddScoped<IInvoiceDataService, ApiInvoiceDataService>();
            services.AddScoped<IPhoneDataService, ApiPhoneDataService>();
            services.AddScoped<IAppleIdDataService, ApiAppleIdDataService>();
            services.AddScoped<IHomeDataService, ApiHomeDataService>();
            services.AddScoped<IProductsDataService, ApiProductsDataService>();
            services.AddScoped<IPeopleDataService, ApiPeopleDataService>();
            services.AddScoped<ITransactionsDataService, ApiTransactionsDataService>();
            services.AddScoped<IReportsDataService, ApiReportsDataService>();
            services.AddScoped<IAccountDataService, ApiAccountDataService>();
            return services;
        }

        services.AddScoped<IHomeDataService, HomeDataService>();
        services.AddScoped<IProductsDataService, ProductsDataService>();
        services.AddScoped<IPeopleDataService, PeopleDataService>();
        services.AddScoped<ITransactionsDataService, TransactionsDataService>();
        services.AddScoped<IReportsDataService, ReportsDataService>();
        services.AddScoped<IAccountDataService, AccountDataService>();
        return services;
    }
}