var builder = WebApplication.CreateBuilder(args);

// Serilog (console + rolling file) - wired up first so even startup failures get logged.
builder.ConfigureSerilog();

// Register DbContext, repositories, hashing and the data services.
// "UseApi": false in appsettings.json → the production-ready Dal data services are registered.
builder.Services.AddRazorPages();
builder.Services.AddMobileShop(builder.Configuration);

// Cookie-based authentication - the shop's internal users sign in with their User account.
builder.Services
    .AddAuthentication(CookieAuthenticationDefaults.AuthenticationScheme)
    .AddCookie(options =>
    {
        options.LoginPath = "/Account/Login";
        options.AccessDeniedPath = "/Account/Login";
        options.ExpireTimeSpan = TimeSpan.FromHours(8);
        options.SlidingExpiration = true;
    });

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    DatabaseInitializer.InitializeForDevelopment(app);
}
else
{
    app.UseExceptionHandler("/Error");
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}

app.UseHttpsRedirection();

app.UseRouting();

app.UseAuthentication();

app.UseAuthorization();

app.MapStaticAssets();
app.MapRazorPages().WithStaticAssets();

app.Run();
