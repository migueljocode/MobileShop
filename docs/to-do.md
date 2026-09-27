# MobileShop execution checklist

This file is the authoritative instruction and work-order file for future
development tasks. Completed stages and task entries have been cleared at the
repository owner's request. No tasks are currently pending; add new tasks here
when requested, preserving the instructions and notes below.

## Strict rules for the agent

1. Work only on the first unchecked task whose prerequisites are complete. Do not
   skip ahead, combine unrelated tasks, or redesign completed work.
2. Read the relevant code, tests, configuration, and seed-data structure before
   editing. Follow the repository's naming, layering, global-usings, repository,
   service, Razor Pages, and test conventions.
3. Respect project boundaries. Do not modify `src/MobileShop.Api`, API service
   stubs, API configuration, or API endpoints unless a task explicitly includes
   them.
4. Do not add production authentication, authorization, cookie middleware,
   claims, login security, or other security boilerplate unless explicitly
   requested. The current Profile behavior is development-adapted and must
   remain usable without real authentication.
5. Apple ID inventory passwords are intentionally stored as plaintext because
   the shop must be able to recover them for customers. Do not hash, encrypt,
   hide from the inventory workflow, or otherwise change that requirement. This
   does not change the separate hashed-password behavior for application users.
6. Preserve behavior outside the requested task. Avoid broad refactors,
   unnecessary public-contract changes, database initialization policy changes,
   and edits to generated output, `bin`, or `obj`.
7. Use async repository/service methods when an async equivalent exists. Add
   async methods only when they fit the abstraction and can be used end-to-end.
   Use `IAsyncEnumerable` only for genuinely streaming/deferred collection
   flows; do not force it into Razor handlers or materialized-list APIs.
8. Add focused tests for behavior changes where practical. Run the smallest
   relevant tests first, then:
   - `dotnet build src/MobileShop.slnx --nologo`
   - `dotnet test src/MobileShop.slnx --nologo`

   Do not claim completion when validation fails. For commands that may exceed
   the command window, use an attached background command and keep reading that
   same session until completion; do not use shell `&`, `nohup`, `disown`, or
   detached processes.
9. Never silently swallow errors or return success-shaped fallbacks. Preserve
   repository-standard logging and user-facing error patterns, and report
   missing data explicitly.
10. Do not remove checklist items or completion evidence during ordinary work.
    Cleanup/reset is allowed only when the repository owner explicitly requests
    it. Add future work to the appropriate section.
11. Mark a task complete only after implementation and validation pass. Change
    `- [ ]` to `- [x]`, wrap the complete task text in Markdown strikethrough,
    and add an indented completion note listing changed files, validation, and
    intentional limitations.
12. Never check a parent task while an acceptance criterion or dependent task
    remains unfinished. Do not mark work complete merely because it compiles or
    a UI control renders; verify the requested behavior.
13. If requirements are ambiguous or conflict with these rules, stop before
    editing and report the exact conflict rather than inventing a product
    decision.
14. For multi-task roadmaps, complete and persist each task in a separate Git
    commit before starting the next, using a clear Conventional Commit message.
    Do not add a Co-authored-by trailer; the repository owner explicitly opted
    out of co-author trailers.
15. Do not push, reset, amend, or revert commits unless the owner explicitly
    requests that operation. Do not include unrelated pre-existing changes in a
    task commit.

## Important repository notes

- This is a .NET 10 ASP.NET Core application. The solution is
  `src/MobileShop.slnx`; run build, test, and other commands from the repository
  root.
- The solution is layered into `MobileShop.Web`, `MobileShop.Api`,
  `MobileShop.Services`, `MobileShop.Dal`, `MobileShop.Models`, and
  `MobileShop.Tests`. Keep implementation in the layer responsible for it:
  Razor Pages/UI in Web, orchestration and DI in Services, EF Core and
  repositories in Dal, and shared entities/DTOs/view models in Models.
- `AddMobileShop(...)` registers the shared service/data stack. `UseApi: false`
  selects the DAL implementations. `UseApi: true` selects API data-service
  stubs, which currently throw `NotImplementedException`; avoid enabling or
  changing this path unless the task requires it.
- SQLite development data is stored at the repository root as `MobileShop.db`
  through `SolutionPaths`. `DatabaseInitializer.InitializeForDevelopment(...)`
  intentionally deletes and recreates the database before seeding, and must
  remain a development-only workflow. Normal startup is non-destructive.
- Development sample data is maintained in
  `src/MobileShop.Dal/Initialization/sample-data.json`. Preserve stable entity
  relationships and identifiers when changing seed data; do not place real
  contact details or credentials in it.
- EF entity configuration is centralized through
  `ApplyConfigurationsFromAssembly(...)` in `AppDbContext`. Put entity mapping
  changes in the appropriate configuration class.
- Project-wide usings are in each project's `GlobalUsings.cs`; follow the
  convention of keeping feature files free of redundant `using` directives.
- QuestPDF license setup is centralized through
  `QuestPdfSetup.UseCommunityLicense()` in the host builder configuration.
  Reuse the existing `IPdfGenerator` and PDF models for invoice/factor changes
  rather than introducing another PDF mechanism.
- Tests use xUnit and EF Core InMemory. Prefer focused tests under
  `src/MobileShop.Tests` before running the full suite.

## Current status

No pending tasks. Add new requested work below this heading, with its
requirements and acceptance criteria.
