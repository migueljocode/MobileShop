var builder = CreateBuilder(args).ConfigureBuilder();
var app = builder.Build().ConfigureApp();
app.Run();