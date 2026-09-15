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
        builder.ConfigureSerilog();
        builder.Services.AddRazorPages();
        builder.Services.AddMobileShop(builder.Configuration);
        return builder;
    }

    /// <summary>
    /// Configures the web request pipeline without enabling authentication.
    /// </summary>
    public static WebApplication ConfigureApp(this WebApplication app)
    {
        if (app.Environment.IsDevelopment())
        {
            DatabaseInitializer.InitializeForDevelopment(app.Services);
        }

        app.UseRouting();
        app.MapStaticAssets();
        app.MapRazorPages().WithStaticAssets();
        return app;
    }
}
