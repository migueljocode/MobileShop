# Project-Specific Rules — MobileShop
Used alongside planner.md / reviewer.md / actor.md. This file holds only what is specific to this repository; general workflow rules live in those files.

## Non-negotiable
- Apple ID inventory passwords are intentionally stored as plaintext — the shop must be able to recover them for customers. Never hash, encrypt, hide from the inventory workflow, or otherwise "fix" this. (Application-user passwords are separate and stay hashed.)
- Never modify src/MobileShop.Api, API service stubs, API configuration, or API endpoints unless a task explicitly includes them.
- Never add production authentication, authorization, cookie middleware, claims, login security, or other security boilerplate unless explicitly requested. The current Profile behavior is development-adapted and must remain usable without real authentication.
- Never change database initialization policy. DatabaseInitializer.InitializeForDevelopment(...) intentionally deletes and recreates the database before seeding and must stay development-only; normal startup is non-destructive.

## Architecture
- .NET 10 ASP.NET Core. Solution: src/MobileShop.slnx. Run build, test, and all other commands from the repository root.
- Layers — keep implementation in the layer responsible for it:
  - MobileShop.Web: Razor Pages / UI
  - MobileShop.Api: see Non-negotiable
  - MobileShop.Services: orchestration and DI
  - MobileShop.Dal: EF Core and repositories
  - MobileShop.Models: shared entities, DTOs, view models
  - MobileShop.Tests: xUnit tests
- AddMobileShop(...) registers the shared service/data stack. UseApi: false selects the DAL implementations (the normal path). UseApi: true selects API data-service stubs that currently throw NotImplementedException — don't enable or change that path unless a task requires it.
- SQLite development data lives at the repository root as MobileShop.db, resolved through SolutionPaths.
- Development sample data: src/MobileShop.Dal/Initialization/sample-data.json. Preserve stable entity relationships and identifiers when changing it. Never put real contact details or credentials in it.
- EF entity configuration is centralized through ApplyConfigurationsFromAssembly(...) in AppDbContext. Put mapping changes in the matching configuration class.
- Project-wide usings live in each project's GlobalUsings.cs. Keep feature files free of redundant using directives.
- QuestPDF licensing is centralized through QuestPdfSetup.UseCommunityLicense() in the host builder configuration. For invoice/factor changes, reuse the existing IPdfGenerator and PDF models — don't introduce another PDF mechanism.

## Conventions
- Follow the repository's existing naming, layering, repository, service, Razor Pages, and test conventions, and its existing logging and user-facing error patterns.
- Use async repository/service methods whenever an async equivalent exists. Add new async methods only when they fit the abstraction and can be used end-to-end.
- Use IAsyncEnumerable only for genuinely streaming/deferred collection flows — never force it into Razor handlers or materialized-list APIs.
- Tests use xUnit and EF Core InMemory, under src/MobileShop.Tests. Run the smallest relevant tests first, then the full validation as one chained call (--no-build avoids compiling twice):
  - dotnet build src/MobileShop.slnx --nologo && dotnet test src/MobileShop.slnx --nologo --no-build
- Never edit generated output, bin, or obj.
