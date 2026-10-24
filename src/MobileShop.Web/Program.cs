var builder = CreateBuilder(args).ConfigureBuilder();
builder.Build().ConfigureApp(args)?.Run();
