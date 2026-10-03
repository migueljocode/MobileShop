var builder = CreateBuilder(args).ConfigureBuilder();
var app = builder.Build();

if (app.TryRunDatabaseCommand(args))
{
    return;
}

app.ConfigureApp().Run();
