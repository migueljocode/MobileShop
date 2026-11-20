# MobileShop

[![.NET CI](https://github.com/migueljocode/MobileShop/actions/workflows/dotnet.yml/badge.svg)](https://github.com/migueljocode/MobileShop/actions/workflows/dotnet.yml)
[![.NET 10](https://img.shields.io/badge/.NET-10-512BD4?logo=dotnet)](https://dotnet.microsoft.com/)
[![License: MIT](https://img.shields.io/badge/License-MIT-green.svg)](LICENSE)
[![Platform](https://img.shields.io/badge/platform-linux--x64%20%7C%20osx--arm64-lightgrey)](https://github.com/migueljocode/MobileShop/releases)

**Self-hostable inventory & sales management for mobile phone shops**

Built with ASP.NET Core, .NET 10, Entity Framework Core and SQLite.  
Manage products, IMEI-tracked devices, customers, sellers, sales, PDF invoices and period-based profit-and-loss reports from a single web interface.

> **⚠ Development data is destructive**  
> The Development launch profile deletes and recreates the local database on every start, then seeds sample data.  
> **Never** run Development mode against real shop data.
>
> **Security notice**  
> This project is **not** production-hardened. It does not provide production-grade authentication or authorization.  
> Review the [Security and deployment](#security-and-deployment) section before exposing it to any network.

---

## Quick Install

**Recommended for Linux x86_64 and macOS Apple Silicon**

```sh
curl -fsSL https://github.com/migueljocode/MobileShop/releases/latest/download/install.sh | bash
```

Then open **http://localhost:5143**

| | |
|---|---|
| **Default login** | `admin` / `Admin@123` |
| **First action** | Change the password immediately on the **Profile** page |

> The one-liner requires a GitHub release that includes the installer asset.

For detailed directory layout, commands, backups, autostart and troubleshooting see [Install & Update (advanced)](#install--update-advanced).

---

## Features

- **Inventory** — phones, laptops, tablets, smart watches, power banks, portable storage, headphones, cables, chargers, cases, glass protectors, Apple ID inventory and second-hand products
- **Device tracking** — full IMEI support and product-specific details
- **People** — customer and seller records with dedicated detail pages
- **Sales** — record transactions, inspect details and generate PDF invoices
- **Reporting** — profit-and-loss by period + dashboard with stock summaries
- **Shop account** — sign-in and change admin username / password
- **Local-first** — SQLite database, no separate database server required
- **Operational tooling** — bash & PowerShell utilities for log querying
- **Standalone publishing** — self-contained single-file builds for supported runtimes

---

## Technology Stack

| Area                    | Technology                          |
|-------------------------|-------------------------------------|
| Runtime & web framework | .NET 10, ASP.NET Core Razor Pages   |
| Data access             | Entity Framework Core               |
| Database                | SQLite                              |
| PDF generation          | QuestPDF                            |
| Automated tests         | xUnit                               |
| CI                      | GitHub Actions                      |

---

## Run from Source

### Prerequisites

- [.NET 10 SDK](https://dotnet.microsoft.com/download/dotnet/10.0)
- Git

SQLite is embedded — no external database server is needed.

### Local development

```sh
git clone https://github.com/migueljocode/MobileShop.git
cd MobileShop
dotnet restore src/MobileShop.slnx
dotnet build src/MobileShop.slnx
dotnet run --project src/MobileShop.Web
```

| Environment | URL |
|-------------|-----|
| HTTP        | http://localhost:5043 |
| HTTPS       | https://localhost:7043 |

Default credentials: `admin` / `Admin@123` (change them immediately for any non-disposable use).

---

## Runtime Environments & Database Safety

### Development

The Web project's launch settings select the **Development** environment. On every start it:

1. Deletes and recreates the database from the current EF Core model
2. Loads sample data from `src/MobileShop.Dal/Initialization/sample-data.json`
3. Ensures the admin account exists

**Every Development-mode startup erases the existing database.** Use only with disposable local data.

### Production

Production startup is non-destructive. It verifies that the database schema matches the running build and refuses to start against a missing database (published layout), unknown newer migrations, or when `ASPNETCORE_ENVIRONMENT=Development` is pointed at a published data folder.

Before the first start (or after an update that includes migrations) run:

```sh
dotnet run --project src/MobileShop.Web -- --migrate-database
```

The database file defaults to `MobileShop.db` in the repository root (resolved via `SolutionPaths.DatabaseFile`). It is git-ignored.

Shop profile data (`Name`, `Address`, `Phone`, `Instagram`) lives in the `ShopProfiles` table and is managed under **Account → Shop profile**. The `Pdf:Shop*` settings in `appsettings.json` are used only as first-run seeds.

**Always back up before migrating an existing database.**

---

## Configuration

Settings live in `src/MobileShop.Web/appsettings.json`. Ports and environment are controlled by `src/MobileShop.Web/Properties/launchSettings.json`.

| Setting        | Guidance |
|----------------|----------|
| `UseApi`       | Keep `false`. The alternative API data-service implementations are stubs and throw `NotImplementedException`. |
| `Pdf`          | First-run seed values for the shop profile. Edit live data under **Account → Shop profile**. |
| `AppLogging`   | Default, console and file log levels. |
| `AllowedHosts` | Standard ASP.NET Core host filtering — configure for your deployment. |

Logs are written under `logs/` inside the configured data directory (`MOBILESHOP_DATA_DIRECTORY/logs` when set, otherwise relative to the solution/publish root). See `src/MobileShop.Scripts/README.md` for log-query utilities.

---

## Solution Structure

Solution file: `src/MobileShop.slnx` (always run commands from the repository root).

| Project                 | Responsibility |
|-------------------------|----------------|
| `src/MobileShop.Web`    | Main web application & Razor Pages UI |
| `src/MobileShop.Api`    | Thin API host (not used in the normal path) |
| `src/MobileShop.Services` | Business orchestration, DI, data services, password hashing, PDF |
| `src/MobileShop.Dal`    | EF Core context, repositories, migrations, seeding |
| `src/MobileShop.Models` | Entities, DTOs, view models, settings, entity configuration |
| `src/MobileShop.Tests`  | Automated xUnit tests |
| `src/MobileShop.Scripts`| Installer/launcher, log-query utilities, shell/PowerShell tests |

**Further reading**

- [Entity relationship diagram](docs/erd.mermaid)
- [Publishing guide](docs/publishing.md)
- [Project conventions & architecture rules](.agents/project.md)

### Architecture notes

- Hosts register services via `AddMobileShop(...)`
- EF Core mappings are centralized in `src/MobileShop.Models/Entities/Configuration`
- Schema migrations live in `src/MobileShop.Dal/Migrations`
- Money is stored as whole Iranian rials (`long`) bounded by `MoneyLimits.MaxRials`
- Invoices and reports share the `IPdfGenerator` abstraction

---

## Build & Test

```sh
dotnet restore src/MobileShop.slnx
dotnet build src/MobileShop.slnx --no-restore
dotnet test src/MobileShop.slnx --no-build
```

Run a single test:

```sh
dotnet test src/MobileShop.slnx --no-build --filter "FullyQualifiedName~CustomerRepoTests"
```

Tests use xUnit (mostly EF Core InMemory; migration tests use a temporary SQLite file).

The [GitHub Actions workflow](.github/workflows/dotnet.yml) builds and tests on Ubuntu with .NET 10 and runs the script suite across Ubuntu, Windows and macOS. Markdown-only pushes to `main` are ignored; pull requests always run the full suite.

---

## Install & Update (advanced)

### Directory layout

Everything lives under a single `ROOT` directory (override with `MOBILESHOP_ROOT`):

| Platform | Default `ROOT` |
|----------|----------------|
| Linux    | `${XDG_DATA_HOME:-$HOME/.local/share}/mobileshop` |
| macOS    | `$HOME/Library/Application Support/MobileShop` |

| Path                        | Contents |
|-----------------------------|----------|
| `ROOT/app/versions/<TAG>/`  | Extracted release directories (kept side-by-side) |
| `ROOT/app/current`          | Symlink to the active version |
| `ROOT/app/bin/mobileshop`   | Launcher / management CLI (also linked into `$HOME/.local/bin`) |
| `ROOT/data/`                | `MobileShop.db` (+ WAL/SHM), verified `.bak` backups, `logs/` (mode 700) |
| `ROOT/state/`               | `installed.env`, `install.log`, locks, `app.pid` |
| `ROOT/cache/`               | Resumable downloads and .NET single-file extraction cache |

### `mobileshop` commands

| Command | Purpose |
|---------|---------|
| `mobileshop install [options]` | Install a release, migrate DB, start the app and verify `/Account/Login` |
| `mobileshop update [options]`  | Safe upgrade with migration + health-check protocol |
| `mobileshop rollback [--yes]`  | Switch back to the previous version and restore its pre-migration backup |
| `mobileshop prune [--keep N]`  | Remove older version directories (never touches `data/`) |
| `mobileshop adopt <dir>`       | Import an existing `MobileShop.db` into the managed layout |
| `mobileshop backup`            | Create a verified online SQLite backup |
| `mobileshop start / stop / restart / status / doctor` | Service control & diagnostics |

### Backups, existing installs & autostart

- Before migrations the app writes a verified online backup (`MobileShop.db.<timestamp>.bak`) under `ROOT/data/`. Backups are never deleted automatically.
- To migrate from a manual publish folder: stop the old process cleanly, run `mobileshop adopt <old-data-dir>`, then `mobileshop update` (or `install`).
- **macOS**: installs a LaunchAgent that starts MobileShop at login.
- **Linux**: installs a systemd user unit. For boot-before-login enable lingering:  
  `sudo loginctl enable-linger "$USER"`

### Troubleshooting

| Symptom | Fix |
|---------|-----|
| Port 5143 already in use | Set `MOBILESHOP_PORT=<port>` before install/update/start |
| Why port 5143? | macOS AirPlay Receiver occupies 5000/7000 |
| Gatekeeper blocks archive | Prefer the `curl … \| bash` one-liner or `install.sh --archive …` |
| `mobileshop: command not found` | Add `$HOME/.local/bin` to `PATH` or invoke the binary under `ROOT/app/bin/` |
| Missing `libicu` on Linux | Run `mobileshop doctor` or install the distro package / set `DOTNET_SYSTEM_GLOBALIZATION_INVARIANT=1` |
| No systemd user bus | Installer falls back to pid mode; use `--no-service` explicitly |
| Failed update rolled back | Inspect `ROOT/state/install.log` for the migration/health-check error and the restored `.bak` |

---

## Publish a Standalone Build

```sh
dotnet publish src/MobileShop.Web -c Release -r linux-x64
dotnet publish src/MobileShop.Web -c Release -r osx-arm64
```

Output lands in `Publishes/<RID>/`. The build is self-contained and single-file — the target machine needs no .NET runtime. The database is **not** bundled; run `MobileShop.Web --migrate-database` before first start. Default listen address when none is configured: `http://localhost:5143`.

See the [publishing guide](docs/publishing.md) for full details. Native AOT is not supported for this Razor Pages application.

---

## Default Admin Account

When no admin account exists the application creates:

- **Username:** `admin`
- **Password:** `Admin@123`

Change these credentials immediately on the Profile page before any shared or non-disposable use.

---

## Security and Deployment

MobileShop is intended as a **local or trusted-network** shop tool and is **not production-hardened**.

- No production-grade authentication or authorization is implemented.
- Apple ID inventory passwords are stored in **plaintext** by design (the shop must be able to retrieve them for customers). Do not store sensitive credentials unless this model is acceptable.
- Application-user passwords are hashed separately.
- Development mode always deletes and recreates the database — never point it at real data.
- Keep `UseApi` set to `false` unless the API stubs are fully implemented.
- Configure host filtering, network exposure, backups, shop details and account credentials for your environment.

---

## Contributing

Before changing code, read [`AGENTS.md`](AGENTS.md) and the [project rules](.agents/project.md).  
The repository uses a strict **Planner → Reviewer → Actor** workflow for scoped changes.  
Follow existing layering and conventions, keep changes focused, and use [Conventional Commits](https://www.conventionalcommits.org/).

---

## License

Distributed under the [MIT License](LICENSE).  
Copyright © 2026 Mikaeeil.
