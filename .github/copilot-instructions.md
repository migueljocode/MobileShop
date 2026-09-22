# Copilot instructions for MobileShop

This repo is a .NET 10 ASP.NET Core app for a local mobile phone shop inventory/sales system. The solution is organized as a layered app with a web frontend, a REST API host, a shared model layer, a service layer, and an EF Core data access layer.

## Build, test, and validation

Run commands from the repository root unless noted otherwise.

- Full solution build:
  - `dotnet build src/MobileShop.slnx`
- Full test suite:
  - `dotnet test src/MobileShop.slnx --nologo`
- Run a single test or test class:
  - `dotnet test src/MobileShop.slnx --filter "FullyQualifiedName~CustomerRepoTests"`
  - `dotnet test src/MobileShop.slnx --filter "Name~Find_EagerlyLoadsPersonNavigation"`
- Run the web app:
  - `dotnet run --project src/MobileShop.Web`
- Run the API host:
  - `dotnet run --project src/MobileShop.Api`

There is no repository-specific lint command or lint configuration file in the repo. The baseline validation path is `dotnet build` + `dotnet test`.

## High-level architecture

The solution is split into these major projects:

- `src/MobileShop.Web`: Razor Pages UI. `WebApplicationBuilderExtensions` registers the app, Razor Pages, and the shared `AddMobileShop(...)` stack.
- `src/MobileShop.Api`: host for API endpoints; currently a thin host around the same service/data stack.
- `src/MobileShop.Services`: service-layer wiring and business logic access. This project is where DI registration happens via `ServiceCollectionExtensions.AddMobileShop(...)`.
- `src/MobileShop.Dal`: EF Core database access, repositories, and database initialization/seeding logic.
- `src/MobileShop.Models`: shared entities, view models, DTOs, and configuration classes.
- `src/MobileShop.Tests`: xUnit tests using EF Core InMemory and covering DAL/service/model behavior.

The important dependency flow is:

- `Web` and `Api` hosts call `AddMobileShop(...)`.
- `AddMobileShop(...)` registers the EF Core `AppDbContext`, repositories, password hashing, PDF generation, and the selected data-service implementations.
- The default configuration is `UseApi: false`, which means the production-ready DAL data services are used. `UseApi: true` selects the `MobileShop.Services.DataServices.Api` services, which are currently stubs whose members throw `NotImplementedException`.
- `AppDbContext` is the central EF Core model; it exposes the domain sets for users, products, transactions, people, and product-profile tables.
- `DatabaseInitializer.InitializeForDevelopment(...)` wipes and re-creates the SQLite dev database and seeds bundled sample data only in development, while the app startup path is intentionally non-destructive in normal use.

The database is SQLite and is stored next to the solution root via `SolutionPaths.DatabaseFile`, not under a project folder. That matters when creating migrations, seeding logic, or debugging startup issues.

## Key conventions in this repo

- Use the solution file at `src/MobileShop.slnx`; do not assume the repo root contains the solution.
- The app expects a repo-root SQLite database at `MobileShop.db` and resolves it through `SolutionPaths` rather than hard-coded relative paths.
- Development seeding is intentionally destructive: `DatabaseInitializer.InitializeForDevelopment(...)` calls `EnsureDeleted()` + `EnsureCreated()` before seeding. This is a dev-only workflow, not a production pattern.
- Most of the repo uses the repository pattern over EF Core `DbContext` (`*Repo` classes in `MobileShop.Dal/Repos`), with service-layer DTO/data-service abstractions sitting on top.
- Entity configuration is centralized with `ApplyConfigurationsFromAssembly(...)` in `AppDbContext.OnModelCreating(...)`, so entity metadata and relationships are configured in dedicated configuration classes rather than in each model file.
- Project-wide usings live in each project's `GlobalUsings.cs`, and feature files carry no `using` directives (EF migrations, `*.Designer.cs`, and `_ViewImports.cshtml` are the exceptions).
- The QuestPDF licence is set once by `QuestPdfSetup.UseCommunityLicense()`, invoked from the host's `ConfigureBuilder`, not from `Program.cs`.
- Because `QuestPDF.Infrastructure` is global in `MobileShop.Services`, the entity type `Color` must be written as `MobileShop.Models.Entities.Color` inside that project.
- Tests are xUnit-based and use EF Core InMemory. Prefer targeted repository/service tests under `src/MobileShop.Tests` when validating a change.
- The app is a local shop system with strongly typed product families (`Phone`, `Laptop`, `Tablet`, `SmartWatch`, `Case`, `Glass`, `SecondHand`, etc.), so changes affecting product catalogs or transaction flows often touch multiple layers together.

## Working effectively in this repo

- When debugging startup issues, check both the host configuration (`src/MobileShop.Web/Program.cs` and `WebApplicationBuilderExtensions`) and the shared service registration (`src/MobileShop.Services/ServiceCollectionExtensions.cs`).
- When changing database schema or seed data, review the EF Core initialization and sample loader pipeline in `src/MobileShop.Dal/Initialization` before assuming the app can boot with existing data.
- When validating a change, prefer the smallest targeted test command over the full suite if the behavior is isolated to one repo/service area.
- Respect the project layering: models stay in `MobileShop.Models`, EF and repository code stays in `MobileShop.Dal`, and application/service orchestration stays in `MobileShop.Services`.

## Project-specific cues

- The app's `UseApi` flag is a switch used by `AddMobileShop(...)`; when it is set to `true`, it selects the `MobileShop.Services.DataServices.Api` implementations, which are currently stubbed and throw `NotImplementedException` on every data call. The default remains `false`.
- Default admin/login setup is dev-only and depends on the sample seed data; the app now calls `IUserDataService.EnsureAdminUser()` from the Web host's Development-only startup path instead of using a separate `AdminSeeder` class.
- PDF generation is included in the service stack (`QuestPdfGenerator`), so invoice/report changes may require updates across the services and app configuration settings.
