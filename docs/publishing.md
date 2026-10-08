# Standalone publishing

Run `dotnet publish` from the repository root with the desired runtime identifier:

```sh
dotnet publish src/MobileShop.Web -c Release -r linux-x64
dotnet publish src/MobileShop.Web -c Release -r osx-arm64
```

The project configures self-contained single-file publishing, compression, and native library extraction. Outputs are written to `Publishes/<RID>/`; no .NET runtime or SDK is needed on the target machine. Extracted platform content includes the app's static web assets, configuration, and a current-schema empty `MobileShop.db` with the initial admin account. Publishing creates that database only when one is not already present, so republishing preserves an existing database. Start the app by running `./MobileShop.Web` from that directory.

The published app uses its current working directory for its database and logs, even when launched from inside the source repository. If `MobileShop.db` is missing or empty, it creates the current schema and a single initial admin account (`admin` / `Admin@123`) on first start; it does not load `sample-data.json`. Change the default password from the Profile page immediately. Back up the database before upgrading an existing installation; if its schema is outdated, run the explicit `--migrate-database` command according to the production database migration instructions.

Use `linux-x64` for Ubuntu x64 and `osx-arm64` for Apple silicon Macs. GitHub currently offers macOS 26 ARM64 runners; the macOS build is not tied to a specific macOS major version and is intended to run on compatible newer releases as well.

Native AOT is not enabled because this application uses Razor Pages/MVC, which is not supported by ASP.NET Core Native AOT.
