# Standalone publishing

Run `dotnet publish src/MobileShop.Web` from the repository root. It targets the machine it runs on (the SDK's runtime identifier, such as `linux-x64`, `osx-arm64` or `win-x64`) and writes a clean, self-contained single-file build to `Publishes/<RID>/`. To publish for another platform, pass the runtime identifier explicitly:

```sh
dotnet publish src/MobileShop.Web -r linux-x64
dotnet publish src/MobileShop.Web -r osx-arm64
```

The project configures self-contained single-file publishing, compression, and native library extraction; the output holds the executable, `appsettings.json` and `wwwroot/` (no IIS `web.config`, no language folders). No .NET runtime or SDK is needed on the target machine. The publish output does not include a database. Start the app by running `./MobileShop.Web` from that directory.

At runtime, the app uses its current working directory for its database and logs, even when launched from inside the source repository. If `MobileShop.db` is missing or empty, startup creates the current schema and a single initial admin account (`admin` / `Admin@123`); every startup also ensures the Zone Mobile shop party (the seller of every sale and the customer of every purchase) using `Pdf:ShopPhone` from `appsettings.json`; it does not load `sample-data.json`. Change the default password from the Profile page immediately. Back up the database before upgrading an existing installation; if its schema is outdated, run the explicit `--migrate-database` command according to the production database migration instructions. Set `MOBILESHOP_DATA_DIRECTORY` to keep the database and logs in a fixed folder regardless of the launch directory.

Use `linux-x64` for Ubuntu x64 and `osx-arm64` for Apple silicon Macs. GitHub currently offers macOS 26 ARM64 runners; the macOS build is not tied to a specific macOS major version and is intended to run on compatible newer releases as well.

Native AOT is not enabled because this application uses Razor Pages/MVC, which is not supported by ASP.NET Core Native AOT.
