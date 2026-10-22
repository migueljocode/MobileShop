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
2. Loads sample data from `src/MobileShop.Dal/Initialization/sample-data.json`.
3. Ensures the admin account exists.

**Every Development-mode startup erases the existing database.** Use this mode only with disposable local data.

### Production

Production startup is non-destructive. It checks that the database schema is current and creates the schema and initial admin account if the database is missing or empty. An outdated schema requires an explicit migration.

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
| `Pdf` | Configure shop details such as address and phone before producing customer-facing invoices. |
| `AppLogging` | Configure default, console, and file log levels. |
| `Distribution` | Controls default and maximum share percentages. |
| `AllowedHosts` | Standard ASP.NET Core host filtering; configure it appropriately for your deployment. |

Logs are written under `logs/`. See the [log utilities guide](src/MobileShop.Scripts/README.md) for bash and PowerShell commands.

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
| `src/MobileShop.Scripts` | Bash and PowerShell log-query utilities |

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

The [GitHub Actions workflow](.github/workflows/dotnet.yml) builds and tests the solution on Ubuntu with .NET 10 and tests the log utilities on Ubuntu and Windows. Markdown-only changes do not trigger this workflow.

## Publish a standalone build

Example targets:

```sh
dotnet publish src/MobileShop.Web -c Release -r linux-x64
dotnet publish src/MobileShop.Web -c Release -r osx-arm64
```

Publish output is written under `Publishes/<RID>/`. The build is self-contained and single-file, so the target machine does not need a separately installed .NET runtime. The database is not bundled; first startup creates the schema and initial admin account.

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
