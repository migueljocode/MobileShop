QuestPDF.Settings.License = QuestPDF.Infrastructure.LicenseType.Community;

var builder = CreateBuilder(args).ConfigureBuilder();
var app = builder.Build().ConfigureApp();
app.Run();