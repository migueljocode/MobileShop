# MobileShop — Project Rules
.NET 10 ASP.NET Core, layered. Solution: `src/MobileShop.slnx` (not at the repo root). Run every command from the repo root.

## Non-negotiable — unless the approved plan explicitly says otherwise
- Apple ID inventory passwords stay **plaintext** by design (the shop must recover them for customers): never hash, encrypt, hide or "fix" them. Application-user passwords are separate and stay hashed.
- Don't touch `src/MobileShop.Api`, API service stubs, API config or endpoints unless the task includes them.
- No production authentication, authorization, cookie middleware, claims or login-security boilerplate. Profile behavior is development-adapted and must work without real auth.
- `DatabaseInitializer.InitializeForDevelopment(...)` (`EnsureDeleted()` + `EnsureCreated()`, then seed) stays development-only and destructive; normal startup is non-destructive.
- Never edit generated output, `bin` or `obj`.

## Architecture
- Layers — keep code in the layer that owns it: `Web` Razor Pages/UI · `Api` thin host (see above) · `Services` orchestration + DI (`ServiceCollectionExtensions.AddMobileShop(...)`) · `Dal` EF Core, `*Repo` repositories, DB init/seeding · `Models` entities, DTOs, view models, config classes · `Tests` xUnit.
- `AddMobileShop(...)`, called by the Web and Api hosts, registers `AppDbContext`, repositories, password hashing, PDF generation and the data services. `UseApi` is set only in Web's `appsettings.json` (Api omits it → `false`): `false` = DAL data services (normal path); `true` = `MobileShop.Services.DataServices.Api` stubs that throw `NotImplementedException` — don't enable or change unless a task requires it.
- SQLite dev DB: `MobileShop.db` at the repo root, resolved through `SolutionPaths.DatabaseFile` — never hard-code relative paths (matters for migrations, seeding, startup debugging).
- Dev sample data: `src/MobileShop.Dal/Initialization/sample-data.json` — keep relationships and IDs stable; no real contact details or credentials.
- EF configuration is centralized: `ApplyConfigurationsFromAssembly(...)` in `AppDbContext.OnModelCreating` — put mapping changes in the matching configuration class.
- Project-wide usings live in each project's `GlobalUsings.cs`; keep feature files free of redundant `using`s. File-local usings stay only for namespaces few files use (mostly tests), the three production files that intentionally keep theirs, EF migrations, `*.Designer.cs`, `_ViewImports.cshtml` and `ModuleInitializer.cs`. `QuestPDF.Infrastructure` is global in `Services`, so write the entity as `MobileShop.Models.Entities.Color` there.
- QuestPDF licence: set once by `QuestPdfSetup.UseCommunityLicense()` from the host's `ConfigureBuilder` (not `Program.cs`). Invoices/reports reuse `IPdfGenerator` (`QuestPdfGenerator`) and the existing PDF models — no second PDF mechanism.
- Money: whole IRR amounts as `long`, bounded by `MoneyLimits.MaxRials`; display with `ToIrr()` / `ToGroupedDigits()`.
- Dev admin/login depends on the sample seed: the Web host's Development-only startup calls `IAccountDataService.EnsureAdminUser()` (there is no `AdminSeeder`).

## Conventions
- Follow existing naming, layering, repository, service, Razor Pages and test conventions, and the existing logging and user-facing error patterns.
- Prefer async repository/service methods where an async equivalent exists; add new async methods only when they fit the abstraction and are used end-to-end. `IAsyncEnumerable` only for genuinely streaming flows — never in Razor handlers or materialized-list APIs.
- Tests (`src/MobileShop.Tests`): xUnit; EF Core InMemory for most, temp-file SQLite for migrations and the database migrator. Prefer targeted repository/service tests.

## Commands — CI runs build + test; run locally only if the user asks or CI is unavailable
- `dotnet build src/MobileShop.slnx --nologo && dotnet test src/MobileShop.slnx --nologo --no-build` (`--no-build` avoids compiling twice). Single test: `--filter "FullyQualifiedName~CustomerRepoTests"` or `"Name~Find_EagerlyLoadsPersonNavigation"`.
- Run: `dotnet run --project src/MobileShop.Web` (or `src/MobileShop.Api`).
- Production DB change: back up first, then `dotnet run --project src/MobileShop.Web -- --migrate-database`; normal Production startup only checks that the schema is current.
