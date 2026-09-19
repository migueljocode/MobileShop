var builder = CreateBuilder(args)
                .ConfigureBuilder();
var app = builder.Build()
                .ConfigureApp();

if (app.Environment.IsDevelopment())
{
    // Dev-only: the freshly seeded sample data ships a placeholder hash, so give the admin a real one.
    using var scope = app.Services.CreateScope();
    scope.ServiceProvider.GetRequiredService<AdminSeeder>().EnsureDefaultAdmin();
}

app.Run();