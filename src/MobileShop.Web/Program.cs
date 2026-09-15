var builder = WebApplication.CreateBuilder(args).AddMobileShopWeb();
var app = builder.Build().UseMobileShopWeb();
app.Run();
