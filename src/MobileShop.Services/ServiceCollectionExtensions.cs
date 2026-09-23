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
            .AddMobileShopRepositories()
            .AddMobileShopSecurity()
            .AddMobileShopPdf(configuration)
            .AddMobileShopDataServices(useApi);
    }

    private static IServiceCollection AddMobileShopPdf(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        services.Configure<PdfSettings>(
            configuration.GetSection("Pdf"));
        services.AddScoped<IPdfGenerator, QuestPdfGenerator>();
        return services;
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
        services.AddScoped<IManufacturerRepo, ManufacturerRepo>();
        services.AddScoped<IModelRepo, ModelRepo>();
        services.AddScoped<ICategoryRepo, CategoryRepo>();
        services.AddScoped<IColorRepo, ColorRepo>();
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
            return services;
        }

        services.AddScoped<IUserDataService, UserDataService>();
        services.AddScoped<ICustomerDataService, CustomerDataService>();
        services.AddScoped<ISellerDataService, SellerDataService>();
        services.AddScoped<ITransactionDataService, TransactionDataService>();
        services.AddScoped<IProductDataService, ProductDataService>();
        services.AddScoped<IInvoiceDataService, InvoiceDataService>();
        services.AddScoped<IPhoneDataService, PhoneDataService>();
        services.AddScoped<IAppleIdDataService, AppleIdDataService>();
        return services;
    }
}