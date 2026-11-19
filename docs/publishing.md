# Standalone publishing and releases

Run `dotnet publish src/MobileShop.Web` from the repository root. It targets the machine it runs on (the SDK's runtime identifier, such as `linux-x64`, `osx-arm64`, or `win-x64`) and writes a self-contained single-file build to `Publishes/<RID>/`. To publish for another supported platform, pass the runtime identifier explicitly:

```sh
dotnet publish src/MobileShop.Web -r linux-x64
dotnet publish src/MobileShop.Web -r osx-arm64
```

The project configures self-contained single-file publishing, compression, and native library extraction; the output holds the executable, `appsettings.json`, `MobileShop.Web.staticwebassets.endpoints.json`, `PDF/Fonts/`, and `wwwroot/` (no IIS `web.config`, no language folders). No .NET runtime or SDK is needed on the target machine, and the publish output does not include a database. Native AOT is not enabled because ASP.NET Core Razor Pages/MVC does not support it.

## Published runtime behavior

- **Use `mobileshop` for local installs:** The installer (`src/MobileShop.Scripts/Bash/install.sh`) manages versioned directories under `ROOT/app/versions/<TAG>/` behind `ROOT/app/current`, pins `ASPNETCORE_ENVIRONMENT=Production`, `DOTNET_ENVIRONMENT=Production`, `ASPNETCORE_CONTENTROOT=ROOT/app/current`, `MOBILESHOP_DATA_DIRECTORY=ROOT/data`, `ASPNETCORE_URLS=http://localhost:<PORT>`, and `DOTNET_BUNDLE_EXTRACT_BASE_DIR=ROOT/cache/extract`, and switches to `ROOT/data` before executing `MobileShop.Web`.
- **Running `MobileShop.Web` directly from another folder:** If you launch `./MobileShop.Web` manually without the `mobileshop` launcher from a working directory other than the publish folder, set `MOBILESHOP_DATA_DIRECTORY` to your data directory and pin the content root (run from the publish folder or set `ASPNETCORE_CONTENTROOT`) so `appsettings.json`, `wwwroot/`, and PDF fonts resolve reliably.
- **Database and logs follow the data directory:** Both `MobileShop.db` and application logs (`SolutionPaths.LogDirectory`, i.e., `logs/app-.log`) are stored under `SolutionPaths.Root`. Setting `MOBILESHOP_DATA_DIRECTORY` moves both the database and logs together.
- **Default listening URL:** When none of `urls`, `HTTP_PORTS`, `HTTPS_PORTS`, or `Kestrel:Endpoints` is configured (via CLI `--urls`, `ASPNETCORE_URLS`, `ASPNETCORE_HTTP_PORTS`, `ASPNETCORE_HTTPS_PORTS`, or `appsettings.json`), the published host listens on `http://localhost:5143` (instead of `5000`, which collides with AirPlay Receiver on macOS 12+).
- **First start and schema safety in a published layout:** When `MOBILESHOP_DATA_DIRECTORY` is set or `MobileShop.Web.staticwebassets.endpoints.json` is present in the launch directory, normal startup refuses to create a new database automatically, refuses databases holding unknown newer migrations, and refuses to run destructive `ASPNETCORE_ENVIRONMENT=Development` initialization. Always run migrations explicitly first (the `mobileshop` installer does this automatically):
  ```sh
  ./MobileShop.Web --migrate-database
  ```
- **Additional host CLI flags:**
  - `./MobileShop.Web --version` prints `MobileShop <version>` without touching the database or data directory.
  - `./MobileShop.Web --backup-database` writes a verified online SQLite backup (`MobileShop.db.<yyyyMMddHHmmss>.bak`) in the data directory.
- **Shop profile and first-run seeding:** Shop identity (`Name`, `Address`, `Phone`, `Instagram`) lives in the `ShopProfiles` table and is edited in **Account → Shop profile**. `Pdf:Shop*` values in `appsettings.json` are first-run seeds only; `Pdf:ShopPhone` is no longer written back to the database on every startup.
- **Data Protection application name:** The host configures a fixed Data Protection application name (`MobileShop`), keeping login cookies and antiforgery tokens valid across upgrades between `app/versions/<TAG>` folders. Users are signed out once after the first upgrade that includes this change.

Use `linux-x64` for Ubuntu x64 and `osx-arm64` for Apple Silicon Macs. GitHub currently offers macOS ARM64 runners; the macOS build is not tied to a single macOS major version and is intended to run on compatible newer releases as well.

## Release workflow

The GitHub Actions workflow `.github/workflows/release.yml` is triggered manually via `workflow_dispatch` with:

- `version` (required): semantic version such as `0.1.0` or `v0.1.0` (validated and checked to ensure the tag does not already exist on `origin`).
- `prerelease` / `draft` (optional booleans): passed to `gh release create`.
- `dry_run` (optional boolean, default `false`): builds, packages, and runs all publish/installer smoke tests on `linux-x64` and `osx-arm64`, while skipping the `Create GitHub Release` job so no tag or release is created.

Pipeline stages:

1. **Validate and test (`validate`):** Validates the semver tag, checks tag availability, and runs `dotnet restore` / `build` / `test` in `Release` configuration.
2. **Publish and smoke-test (`publish`):** Publishes self-contained single-file binaries for `linux-x64` and `osx-arm64`, verifies macOS `codesign`, packages `MobileShop-<TAG>-<RID>.tar.gz`, runs `.github/scripts/publish-smoke.sh` (end-to-end login, CRUD, and QuestPDF checks), and runs `.github/scripts/installer-smoke.sh` (scenarios `S1`–`S9` against the packaged archive).
3. **Create GitHub Release (`release`, skipped when `dry_run: true`):** Checks out the repository, stamps `__VERSION__` in `src/MobileShop.Scripts/Bash/install.sh` into `dist/install.sh`, computes `dist/checksums.txt` over `MobileShop-*.tar.gz` and `install.sh`, and creates the release with the archives, `install.sh`, and `checksums.txt`.
