namespace MobileShop.Api.Extensions;

/// <summary>
/// Configures the MobileShop API host.
/// </summary>
public static class ApiApplicationBuilderExtensions
{
    /// <summary>
    /// Registers logging, the OpenAPI document, and the shared MobileShop services stack.
    /// </summary>
    public static WebApplicationBuilder ConfigureBuilder(this WebApplicationBuilder builder)
    {
        builder.ConfigureSerilog();
        builder.Services.AddOpenApi();
        builder.Services.AddMobileShop(builder.Configuration);
        return builder;
    }

    /// <summary>
    /// Configures the API request pipeline.
    /// </summary>
    public static WebApplication ConfigureApp(this WebApplication app)
    {
        if (app.Environment.IsDevelopment())
        {
            app.MapOpenApi();
        }

        return app;
    }
}
