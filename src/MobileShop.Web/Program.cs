var builder = WebApplication.CreateBuilder(args);

// Serilog (console + rolling file) - wired up first so even startup failures get logged.
builder.ConfigureSerilog();

// Register DbContext, repositories, hashing and the data services.
// "UseApi": false in appsettings.json → the production-ready Dal data services are registered.
builder.Services.AddRazorPages();
builder.Services.AddMobileShop(builder.Configuration);

// TODO(security): restore cookie authentication and UseAuthentication/UseAuthorization

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    DatabaseInitializer.InitializeForDevelopment(app);
}
else
{
    // TODO(production): exception handler, HSTS, HTTPS redirection
}

app.UseRouting();

app.MapStaticAssets();
app.MapRazorPages().WithStaticAssets();

app.Run();
