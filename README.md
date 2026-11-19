# MobileShop

A self-hostable inventory and sales management application for a mobile phone shop, built with **ASP.NET Core, .NET 10, EF Core, and SQLite**.

MobileShop helps a shop manage products and IMEI-tracked devices, customers, sellers, sales, PDF invoices, and period-based profit-and-loss reports from one web interface.

> **Important — development data is destructive.** The Development launch profile deletes and recreates the local database on every start, then seeds sample data. Never run Development mode against real shop data.
>
> **Security notice:** this project is not production-hardened and does not provide production-grade authentication or authorization. Review the limitations under [Security and deployment](#security-and-deployment) before exposing it to a network.

## Features

- **Inventory:** phones, laptops, tablets, smart watches, power banks, portable storage, cables, chargers, cases, glass protectors, Apple ID inventory, and second-hand products.
- **Device tracking:** support for IMEI-tracked devices and product-specific details.
- **People management:** customer and seller records with detail pages.
- **Sales:** record transactions, inspect transaction details, and generate PDF invoices.
- **Reporting:** profit-and-loss reports by period and a dashboard with stock summaries.
- **Shop account:** sign in and update the admin username and password.
- **Local-first storage:** SQLite database; no separate database server required.
- **Operational tooling:** bash and PowerShell utilities for querying application logs.
- **Standalone publishing:** self-contained single-file builds for supported target runtimes.

## Technology stack

| Area | Technology |
|---|---|
| Runtime and web framework | .NET 10, ASP.NET Core Razor Pages |
| Data access | Entity Framework Core |
| Database | SQLite |
| PDF generation | QuestPDF |
| Automated tests | xUnit |
| CI | GitHub Actions |

## Getting started

### Prerequisites

- [.NET 10 SDK](https://dotnet.microsoft.com/download/dotnet/10.0)
- Git

SQLite is bundled as a local database file, so no database server is required.

### Run locally

From the repository root:

```sh
git clone https://github.com/migueljocode/MobileShop.git
cd MobileShop
dotnet restore src/MobileShop.slnx
dotnet build src/MobileShop.slnx
dotnet run --project src/MobileShop.Web
```

The Development launch profile serves the application at:

- HTTP: http://localhost:5043
- HTTPS: https://localhost:7043

The default local admin account is `admin` / `Admin@123`. Change the password immediately if you use the account beyond a disposable local environment. See [Default admin account](#default-admin-account).

## Runtime environments and database safety

### Development

The Web project's launch settings select the Development environment. At startup, Development mode:

1. Deletes and recreates the database from the current EF Core model.
2. Loads sample data from `src/MobileShop.Dal/Initialization/sample-data.json` (Person 1, Zone Mobile, is the shop party).
3. Ensures the admin account exists.

**Every Development-mode startup erases the existing database.** Use this mode only with disposable local data.

### Production

Production startup is non-destructive and checks that the database schema matches the running build (refusing missing databases in a published layout, unknown newer migrations, and `ASPNETCORE_ENVIRONMENT=Development` initialization against a published data folder). Run `--migrate-database` before first start and before starting an updated build.

The Zone Mobile shop party is a person named Zone Mobile with a Legal seller and a customer record, and shop details (`Name`, `Address`, `Phone`, `Instagram`) are stored in the database (`ShopProfiles` table) and managed under **Account → Shop profile**. `Pdf:Shop*` settings in `appsettings.json` act only as first-run seeds when no profile row exists yet.

Before migrating an existing database, make a verified backup. Then run:

```sh
dotnet run --project src/MobileShop.Web -- --migrate-database
```

The database is `MobileShop.db` in the repository root by default. Its location is resolved through `SolutionPaths.DatabaseFile`; do not rely on hard-coded relative paths. The database file is git-ignored.

## Configuration

Application settings are in `src/MobileShop.Web/appsettings.json`; environment and local development ports are configured in `src/MobileShop.Web/Properties/launchSettings.json`.

Important settings:

| Setting | Guidance |
|---|---|
| `UseApi` | Keep `false` for normal operation. The alternative API data-service implementations are stubs and throw `NotImplementedException`. |
| `Pdf` | First-run seed values (`Pdf:ShopName`, `Pdf:ShopAddress`, `Pdf:ShopPhone`, `Pdf:ShopInstagram`) for the database shop profile; edit shop details at runtime under **Account → Shop profile**. |
| `AppLogging` | Configure default, console, and file log levels. |
| `AllowedHosts` | Standard ASP.NET Core host filtering; configure it appropriately for your deployment. |

Logs are written under `logs/` inside the configured data directory (`MOBILESHOP_DATA_DIRECTORY/logs` when set, otherwise `logs/` in the solution/publish root). See the [scripts guide](src/MobileShop.Scripts/README.md) for log-query and installer commands.

## Solution structure

The solution file is `src/MobileShop.slnx`. Run commands from the repository root.

| Project | Responsibility |
|---|---|
| `src/MobileShop.Web` | Main web application and Razor Pages UI |
| `src/MobileShop.Api` | Thin API host; not used in the normal application path |
| `src/MobileShop.Services` | Business orchestration, dependency injection, data services, password hashing, and PDF integration |
| `src/MobileShop.Dal` | EF Core context, repositories, migrations, database initialization, and seeding |
| `src/MobileShop.Models` | Entities, DTOs, view models, settings, and entity configuration |
| `src/MobileShop.Tests` | Automated xUnit tests |
| `src/MobileShop.Scripts` | Local installer/launcher, log-query utilities, and shell/PowerShell tests |

Additional implementation references:

- [Entity relationship diagram](docs/erd.mermaid)
- [Publishing guide](docs/publishing.md)
- [Project conventions and architecture rules](.agents/project.md)

### Architecture notes

- The Web and API hosts use `AddMobileShop(...)` for dependency injection.
- EF Core entity mappings are centralized in `src/MobileShop.Models/Entities/Configuration`.
- Schema migrations live in `src/MobileShop.Dal/Migrations`.
- Money is represented as whole Iranian rials (`long`) and bounded by `MoneyLimits.MaxRials`.
- Invoices and reports share the `IPdfGenerator` abstraction.

## Build and test

```sh
dotnet restore src/MobileShop.slnx
dotnet build src/MobileShop.slnx --no-restore
dotnet test src/MobileShop.slnx --no-build
```

Run a focused test by name:

```sh
dotnet test src/MobileShop.slnx --no-build --filter "FullyQualifiedName~CustomerRepoTests"
```

Tests use xUnit, primarily with EF Core InMemory; migration and database-migrator tests use a temporary SQLite database.

The [GitHub Actions workflow](.github/workflows/dotnet.yml) builds and tests the solution on Ubuntu with .NET 10 and runs the script lint/test suite across Ubuntu, Windows, and macOS. Pushes to `main` ignore markdown-only changes; pull requests run all checks.

## Install and update

For **Linux x86_64** (`linux-x64`) and **macOS Apple Silicon** (`osx-arm64`), install or update locally with:

```sh
curl -fsSL https://github.com/migueljocode/MobileShop/releases/latest/download/install.sh | bash
```

> **Note:** The one-liner requires a GitHub release published after the installer asset was added to the release pipeline.

Once installed, open <http://localhost:5143>.

- **Default login:** Sign in with `admin` / `Admin@123` and change the password immediately from the **Profile** page.
- **Updating across the Shop Profile release:** After the first update that adds database-backed shop profiles, open **Account → Shop profile** and re-enter the shop name, address, and Instagram handle once (the shop phone is carried over from the existing Zone Mobile party).

### Directory layout

All files live under a single `ROOT` directory (override with `MOBILESHOP_ROOT`):

- **Linux default:** `${XDG_DATA_HOME:-$HOME/.local/share}/mobileshop`
- **macOS default:** `$HOME/Library/Application Support/MobileShop`

| Path | Contents |
|---|---|
| `ROOT/app/versions/<TAG>/` | Extracted release directories (kept side by side) |
| `ROOT/app/current` | Symlink to `versions/<TAG>` |
| `ROOT/app/bin/mobileshop` | Installed launcher and management CLI (symlinked into `${MOBILESHOP_BIN_DIR:-$HOME/.local/bin}/mobileshop`) |
| `ROOT/data/` | `MobileShop.db` (+ WAL/SHM files), verified `.bak` backups, and `logs/` (mode `700`; never removed by `update`, `prune`, or plain `uninstall`) |
| `ROOT/state/` | `installed.env`, `install.log`, `lock/`, and `app.pid` (when running in pid mode) |
| `ROOT/cache/` | Resumable release downloads (`downloads/`) and `.NET` single-file extraction cache (`extract/`) |

### `mobileshop` commands

| Command | Purpose |
|---|---|
| `mobileshop install [--version TAG] [--archive FILE] [--sha256 HEX] [--force] [--no-service]` | Install a release (online via GitHub Releases or offline via `--archive`), migrate the database, start the app, and verify `/Account/Login` |
| `mobileshop update [--version TAG] [--archive FILE] [--sha256 HEX] [--force] [--no-service]` | Fetch and verify the latest release's `install.sh` (or use `--archive`) and upgrade using the same safe migration + health-check protocol |
| `mobileshop rollback [--yes] [--no-service]` | Switch back to the previous installed version and restore its pre-migration `.bak` (prompts unless `--yes`) |
| `mobileshop prune [--keep N] [--yes]` | Remove older version directories (default keeps `2`: `current` plus the newest other) and clear `cache/downloads` and `cache/extract`; never touches `data/` |
| `mobileshop adopt <dir>` | Copy `MobileShop.db` from an existing manual install into `ROOT/data/` (copy-only; refuses if the target DB exists or the source WAL is non-empty—stop the old app first) |
| `mobileshop backup` | Create a verified online SQLite backup (`MobileShop.db.<timestamp>.bak`) in `ROOT/data/` |
| `mobileshop start \| stop \| restart \| status \| logs` | Control the user service or pid process, print status/URL, or follow the newest `app-*.log` |
| `mobileshop doctor \| version` | Check platform, service/port health, disk space, database/backup status, `PATH`, glibc/libstdc++/ICU (Linux) or `codesign` (macOS); print installer and installed versions |
| `mobileshop uninstall [--purge] [--yes]` | Stop and remove the service, PATH symlink, `app/`, `state/`, and `cache/`, keeping `data/` (`--purge` also deletes `data/` and `ROOT` after confirmation) |
| `mobileshop run [app-args...]` | Launcher used by systemd/launchd/pid mode: pins `Production`, `ASPNETCORE_CONTENTROOT`, `MOBILESHOP_DATA_DIRECTORY`, `ASPNETCORE_URLS`, and `DOTNET_BUNDLE_EXTRACT_BASE_DIR`, then runs `MobileShop.Web` |

### Backups, existing manual installs, and autostart

- **Backups:** When `--migrate-database` has pending migrations (or when you run `mobileshop backup`), the app writes a verified SQLite online backup (`MobileShop.db.<yyyyMMddHHmmss>.bak`) under `ROOT/data/`. Backups are never deleted automatically.
- **Migrating from a manual publish folder:** Stop the old application cleanly so SQLite checkpoints its WAL, run `mobileshop adopt <old-data-dir>`, and then run `mobileshop update` (or `mobileshop install`).
- **Autostart at login / boot:**
  - **macOS:** Installs a LaunchAgent (`~/Library/LaunchAgents/com.migueljocode.mobileshop.plist`) that starts MobileShop at login (logs under `~/Library/Logs/MobileShop/`).
  - **Linux:** Installs a systemd user unit (`${XDG_CONFIG_HOME:-$HOME/.config}/systemd/user/mobileshop.service`) that starts at login. To also start at boot before any interactive login, enable user lingering (may require administrator rights):
    ```sh
    sudo loginctl enable-linger "$USER"
    ```

### Troubleshooting

- **Port `5143` is already in use:** Set `MOBILESHOP_PORT=<port>` before running `mobileshop install`, `update`, `start`, `restart`, `status`, or `doctor`.
- **Why port `5143` (macOS AirPlay Receiver):** On macOS 12+, AirPlay Receiver binds ports `5000` and `7000`, so MobileShop defaults to `5143` instead of `5000`.
- **macOS Gatekeeper blocks a browser-downloaded archive:** Use the `curl ... | bash` one-liner, or install the downloaded archive via `install.sh --archive <file.tar.gz>` (which clears `com.apple.quarantine` and runs `codesign --verify`).
- **`mobileshop: command not found` (`~/.local/bin` not on `PATH`):** Add `$HOME/.local/bin` to your shell `PATH`, invoke `ROOT/app/bin/mobileshop` directly, or set `MOBILESHOP_BIN_DIR` to a directory already on `PATH`.
- **Linux missing `libicu`:** Run `mobileshop doctor`. If it warns that ICU was not detected and the binary fails to start, install your distro's `libicu` package or set `DOTNET_SYSTEM_GLOBALIZATION_INVARIANT=1`.
- **No systemd user bus on Linux:** When `systemctl --user` is unavailable (for example, headless containers or sessions without D-Bus), the installer warns and uses pid mode (`ROOT/state/app.pid` and `ROOT/data/logs/stdout.log`). Passing `--no-service` or `MOBILESHOP_SERVICE=none` selects pid mode explicitly.
- **A failed update rolled itself back:** Check `ROOT/state/install.log` for the `--migrate-database` or health-check error and which `.bak` file was restored.

## Publish a standalone build

Example targets:

```sh
dotnet publish src/MobileShop.Web -c Release -r linux-x64
dotnet publish src/MobileShop.Web -c Release -r osx-arm64
```

Publish output is written under `Publishes/<RID>/`. The build is self-contained and single-file, so the target machine does not need a separately installed .NET runtime. The database is not bundled; in a published layout, run `MobileShop.Web --migrate-database` before first start to create the schema, initial admin account, and first-run seeded shop profile. When no URL or port is configured, the published host listens on `http://localhost:5143`.

See the [publishing guide](docs/publishing.md) for details. Native AOT is not enabled because the Razor Pages/MVC application does not support it.

## Default admin account

When no admin account exists, the application creates:

- **Username:** `admin`
- **Password:** `Admin@123`

Change these credentials immediately in the Profile page before any shared or non-disposable use. Do not treat the default credentials as suitable for a publicly accessible deployment.

## Security and deployment

MobileShop is intended as a local or trusted-network shop tool and is **not production-hardened**.

- It does not implement production-grade authentication or authorization.
- Apple ID inventory passwords are stored in plaintext by design because the shop must be able to retrieve them for customers. Do not enter sensitive credentials unless this storage model is acceptable for your environment.
- Application-user passwords are handled separately and hashed.
- Development mode deletes and recreates the database on every startup; never point it at real data.
- Keep `UseApi` set to `false` unless the API data-service stubs are implemented.
- Configure host filtering, network exposure, backups, shop details, and account credentials for your deployment.

## Contributing

Before changing the code, review [`AGENTS.md`](AGENTS.md) and the [project rules](.agents/project.md). The repository defines a Planner → Reviewer → Actor workflow for scoped changes. Follow existing layering and conventions, keep changes focused, and use [Conventional Commits](https://www.conventionalcommits.org/) (for example, `docs(readme): clarify setup and deployment`).

## License

Distributed under the [MIT License](LICENSE). Copyright © 2026 Mikaeeil.
