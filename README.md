# MobileShop

ASP.NET Core inventory and sales system for a local mobile phone shop. It tracks stock (including IMEI-tracked devices), records sales, and produces invoices and reports as PDF.

## Features

- **Products:** phones, laptops, tablets, smart watches, power banks, portable storage, cables, chargers, cases, glass protectors, Apple ID inventory, and second-hand items.
- **People:** customers and sellers, each with a details page.
- **Transactions:** selling a product to a customer, with a transaction list, details, and invoice generation.
- **Reports:** profit and loss by period.
- **Dashboard:** a home page with a stock summary.
- **Account:** sign in, sign out, and a profile page for changing the admin username and password.

## Solution structure

| Project | Responsibility |
|---|---|
| `src/MobileShop.Web` | Razor Pages UI. This is the main application. |
| `src/MobileShop.Api` | Thin API host. Not used in normal development (see [Notes](#notes)). |
| `src/MobileShop.Services` | Orchestration, dependency injection (`AddMobileShop(...)`), data services, password hashing, and PDF wiring. |
| `src/MobileShop.Dal` | EF Core `AppDbContext`, repositories (`*Repo`), migrations, database initialization and seeding. |
| `src/MobileShop.Models` | Entities, DTOs, view models, and settings. |
| `src/MobileShop.Tests` | xUnit tests. |
| `src/MobileShop.Scripts` | Log query utilities for bash and PowerShell. |

The solution file is `src/MobileShop.slnx`. Run every command from the repository root.

## Requirements

- .NET 10 SDK. CI uses `10.0.x`.
- No database server is needed. The app uses SQLite through EF Core.
- Developed on Linux. CI builds and tests on Ubuntu 24.04.

## Quick start

```sh
git clone https://github.com/migueljocode/MobileShop.git
cd MobileShop
dotnet build src/MobileShop.slnx
dotnet run --project src/MobileShop.Web
```

The development profile serves the app at `http://localhost:5043`, and the HTTPS profile at `https://localhost:7043`. Sign in as `admin` with the default password described in [Default admin account](#default-admin-account).

## Running modes

### Development

`dotnet run` uses `ASPNETCORE_ENVIRONMENT=Development` from `src/MobileShop.Web/Properties/launchSettings.json`. On every start the app:

1. Deletes and recreates the database from the current model.
2. Loads the sample data from `src/MobileShop.Dal/Initialization/sample-data.json`.
3. Ensures the admin account exists.

**All data is erased on each start.** Use development mode only for local work, never against a real shop database.

### Production

Production startup is non-destructive. The app checks that the database schema is current. If the database is missing or empty, it creates the schema and the initial admin account. If the schema is outdated, startup points you to the migration command.

### Migrating an existing database

1. Back up `MobileShop.db` (in the repository root by default).
2. Run the migration:

   ```sh
   dotnet run --project src/MobileShop.Web -- --migrate-database
   ```

The command verifies a backup and reports one of four results: the database was created, it was already up to date, it was upgraded (with the verified backup path), or a legacy database was baselined (with the verified backup path).

## Configuration

Settings live in `src/MobileShop.Web/appsettings.json`.

| Key | Default | Purpose |
|---|---|---|
| `UseApi` | `false` | Keep `false`. `true` switches data services to the API stubs, which are not implemented yet. |
| `AppLogging` | Default `Debug`, Console `Information`, File `Debug` | Log levels for the default, console and file outputs. |
| `Pdf` | A4, 36 pt margins, shop name, address, phone, Instagram | Page layout and shop details printed on PDFs. Fill in the shop address, phone and Instagram before printing invoices. |
| `Distribution` | `DefaultSharePercent` 50, `MaxTotalSharePercent` 100 | Default and maximum share percentages. |
| `AllowedHosts` | `*` | Standard ASP.NET Core host filter. |

Ports and environment variables are set in `src/MobileShop.Web/Properties/launchSettings.json`.

## Database

- The database is a SQLite file, `MobileShop.db`, in the repository root. Its path comes from `SolutionPaths.DatabaseFile`. Do not hard-code relative paths. The file is git-ignored.
- The schema is managed by EF Core migrations in `src/MobileShop.Dal/Migrations`.
- Entity mappings live in `src/MobileShop.Models/Entities/Configuration` and are applied from `AppDbContext.OnModelCreating`.
- The entity relationship diagram is in [`docs/erd.mermaid`](docs/erd.mermaid).
- Money is stored as whole rials (`long`), bounded by `MoneyLimits.MaxRials`, and displayed with `ToIrr()` or `ToGroupedDigits()`.

## Default admin account

When no admin account exists, the app creates one:

- Username: `admin`
- Password: `Admin@123`

**Change this password from the Profile page immediately**, especially before any production or shared use.

## Logs and log utilities

The app writes logs under `logs/` (for example `logs/app-*.log`). Two equivalent utilities query them:

- `src/MobileShop.Scripts/Bash/log.sh`
- `src/MobileShop.Scripts/PowerShell/log.ps1`

Both support level filtering, line limits, file paths, stdin, and date or date-time ranges. Examples:

```sh
./src/MobileShop.Scripts/Bash/log.sh --level error --above -n 20
./src/MobileShop.Scripts/Bash/log.sh --level warning --date-from 2026-01-01 --date-to 2026-01-31
cat logs/app-*.log | ./src/MobileShop.Scripts/Bash/log.sh --level error
```

Full usage is in [`src/MobileShop.Scripts/README.md`](src/MobileShop.Scripts/README.md).

## Building, testing and CI

```sh
dotnet restore src/MobileShop.slnx
dotnet build src/MobileShop.slnx --no-restore
dotnet test src/MobileShop.slnx --no-build
```

Run a single test with a filter:

```sh
dotnet test src/MobileShop.slnx --no-build --filter "FullyQualifiedName~CustomerRepoTests"
```

Tests use xUnit. Most use EF Core InMemory. Migration and database migrator tests use a temporary SQLite file.

The workflow in [`.github/workflows/dotnet.yml`](.github/workflows/dotnet.yml) runs on pushes to `main` and on pull requests:

- **Build and test** on Ubuntu 24.04 with .NET 10.
- **Log utility tests** on Ubuntu 24.04 (bash) and Windows 2025 (PowerShell).

Pushes that change only Markdown, `.agents/`, `.kilo/` or `docs/` do not trigger the workflow.

## Publishing a standalone build

```sh
dotnet publish src/MobileShop.Web -c Release -r linux-x64
dotnet publish src/MobileShop.Web -c Release -r osx-arm64
```

Output goes to `Publishes/<RID>/`. The build is self-contained single-file with no .NET runtime needed on the target. The publish output does not include a database. On first start the app creates the schema and the default admin account. Start it with `./MobileShop.Web` from that directory. Native AOT is not enabled because Razor Pages and MVC do not support it.

More detail is in [`docs/publishing.md`](docs/publishing.md).

## Architecture and conventions

- **Layers:** `Web` owns Razor Pages and UI. `Services` owns orchestration and dependency injection. `Dal` owns EF Core, repositories and database setup. `Models` owns entities, DTOs, view models and settings. `Api` is a thin host.
- **Dependency injection:** `AddMobileShop(...)` is called by the Web and API hosts. It registers the DbContext, repositories, password hashing, PDF generation and the data services.
- **PDF:** QuestPDF uses its community licence, set once by `QuestPdfSetup.UseCommunityLicense()` from the host's `ConfigureBuilder`. Invoices and reports share one PDF generator (`IPdfGenerator`).
- **Naming:** follow existing layering, repository, service, Razor Pages and test conventions. Project-wide `using`s live in each project's `GlobalUsings.cs`.
- **Async:** prefer async repository and service methods where an async equivalent exists.

The full project rules are in [`.agents/project.md`](.agents/project.md).

## Notes

- **Not production-hardened.** The app has no production-grade authentication or authorization. Treat it as a single-shop tool on a trusted network.
- **Apple ID passwords are stored in plaintext, by design.** The shop must be able to recover them for customers. Application user passwords are hashed.
- **API stubs.** When `UseApi` is `true`, the `MobileShop.Services.DataServices.Api` classes throw `NotImplementedException`. Keep `UseApi` at `false`.
- **Sample data** contains no real contact details or credentials.

## Contributing

Changes follow the multi-agent workflow in [`AGENTS.md`](AGENTS.md):

- Planner, Reviewer and Actor roles are defined in [`.agents/roles`](.agents/roles).
- Project rules are in [`.agents/project.md`](.agents/project.md).
- Commits follow Conventional Commits (`feat`, `fix`, `refactor`, `test`, `docs`, `chore`, `perf`, `build`, `ci`).
- CI is the build and test gate. Make changes minimal and avoid unrelated refactors.

## License

MIT License. Copyright (c) 2026 Mikaeeil. See [`LICENSE`](LICENSE).
