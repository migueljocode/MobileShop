# Plan — Stage U — Cleanup sweep

## Requirements
- Functional
  - The phone model dropdown and the `CreatePhoneAsync` model validation agree: only Phone-category models are offered and accepted.
  - Leftovers from earlier stages are removed: the Distribution-tab unit and culture-dependent money formatting on the profit report, `&amp;` in the README, stale ERD money types, stale statements in `.github/copilot-instructions.md`, file-local `using` directives that break the project-wide-usings convention, empty-namespace global usings, and files without a final newline.
  - Final state: clean build with 0 warnings, 0 skipped tests, whole suite and Production smoke green.
- Non-functional: no behaviour change except the dropdown filter, CI evidence for every step, no change to the Api host, authentication, entities, migrations or Development initialization.
- Constraints (project-specific-rules.md): API untouched, no auth changes, Apple ID inventory passwords stay plaintext, EF configuration centralized, project-wide usings live in `GlobalUsings.cs`, no `bin`/`obj` changes.

## Decisions (labelled)
- **D1 (owner unsure → planner decision):** keep the `Phone`-category check in `CreatePhoneAsync` and make `GetModelsAsync` consistent with it. `GetModelsAsync(int manufacturerId, string categoryName = "Phone")` keeps both callers (CreatePhone, CreateGlass: glass fits are phone models) unchanged and lets later product-type stages (tablet, watch, …) pass their own category.
- **D2:** dead-code and unused-`using` detection by IDE analyzers (IDE0051/IDE0052/IDE0005) is not run in CI, so no unprovable removals are made; the build's 0-warning state is the evidence. Adding such analyzers to CI is out of scope (candidate for Stage AC tooling).
- **D3:** the missing final newlines (99 of 318 tracked text files) are fixed mechanically in one isolated step with a new `.editorconfig`; no other whitespace is reformatted.
- **A1:** GitHub Actions is the build/test gate. The actor does not run `dotnet` locally; each step is reported as `Action: #<run_number> — <Success|Failure|Pending>`.
- **A2 (lessons from Stages S–T):** before using a type or extension in a project, check that project's `GlobalUsings.cs`; compile errors are only visible in CI. Compare each report with `git show <hash> --stat` before writing it.

## Reviewer Briefing
- **Step 1 is MEDIUM risk / HIGH confidence:** a signature change across interface, implementation and Api stub; existing page tests seed models and must still use the Phone category (check `CreateGlassModelTests.SeedModel()`).
- **Step 3 is MEDIUM / MEDIUM:** promoting `using` directives to `GlobalUsings.cs` can cause ambiguous-type errors (`CS0104`) that only CI shows; the fallback is to keep that single using file-local and list it as a documented exception.
- **Step 4 is LOW risk but touches ~99 files:** verify it is mechanical — every changed file differs only by the final newline.
- Do not widen scope: no analyzer tooling, no refactors, no style reformatting beyond the listed items.

## [ ] Step 1 — Phone-category consistency in the model dropdown
- Files
  - inspect: `ProductsDataService.cs` (`GetModelsAsync` line ~238, the `CategoryNavigation.Name == "Phone"` check line ~403), `IProductsDataService.cs`, `ApiProductsDataService.cs`, `Pages/Products/CreatePhone.cshtml.cs`, `Pages/Products/CreateGlass.cshtml.cs`, `ProductsDataServiceTests.cs` (the `GetModelsAsync_only_returns_models_for_the_given_manufacturer` test and `SeedCatalog`), `CreatePhoneModelTests.cs`, `CreateGlassModelTests.cs` (`SeedModel()`)
  - modify: `IProductsDataService.cs`, `ProductsDataService.cs`, `ApiProductsDataService.cs`, `ProductsDataServiceTests.cs`, and the two page-model test files only if their seeded models are not in a category named `"Phone"`
  - do not touch: the two Razor pages, entities, migrations, `src/MobileShop.Api`, authentication
- Symbols
  - `Task<IReadOnlyList<DropdownOptionViewModel>> GetModelsAsync(int manufacturerId, string categoryName = "Phone");` in the interface, with the same default repeated on the `ProductsDataService` and `ApiProductsDataService` implementations (the stub still throws).
  - `ProductsDataService`: a private const `PhoneCategoryName = "Phone"` used both by `GetModelsAsync`'s default usage and by the existing check in `CreatePhoneAsync` (replace the literal); `GetModelsAsync` filters `m.ManufacturerId == manufacturerId && m.CategoryNavigation.Name == categoryName` (include the category navigation as the existing check does).
- Current → Desired: the form lists every model of a manufacturer (including Apple's AppleId-category "Apple ID" model) but rejects non-Phone models with "Selected model not found for this manufacturer." → the list offers exactly what the validation accepts.
- Edge cases: a manufacturer with no Phone models returns an empty list; an unknown `categoryName` returns an empty list; case-sensitivity follows the existing check.
- Tests
  - Update the existing manufacturer-filter test to the new default behaviour.
  - Add `GetModelsAsync_defaults_to_phone_category_and_excludes_the_apple_id_model` and `GetModelsAsync_can_return_another_category` (use the category names `SeedCatalog` creates).
  - If `CreateGlassModelTests.SeedModel()` or `CreatePhoneModelTests` seed a model whose category is not `"Phone"`, change the seed to the Phone category so the page tests keep passing.
- Verify: push and report `Action: #<run_number>`; expect a clean build (0 warnings) and every test passing.
- Done when: the dropdown and validation agree, the new tests pass in CI, and nothing else changed.
- Risk: MEDIUM
- Confidence: HIGH

## [ ] Step 2 — Display and documentation drift
- Files
  - modify: `src/MobileShop.Web/Pages/Reports/ProfitLoss.cshtml`, `README.md`, `docs/erd_v1.mermaid`, `docs/erd_v2.mermaid`, `.github/copilot-instructions.md`
  - do not touch: any other file
- Changes
  - `ProfitLoss.cshtml` (Distribution tab): change "Total profit:" to "Total profit (IRR):"; format `Model.TotalProfit` (line 103) and `row.CalculatedAmount` (line 110) with `ToGroupedDigits()` instead of `ToString("N0")`, keeping the sign prefix and the success/danger classes exactly as they are.
  - `README.md`: `inventory &amp; sales` → `inventory & sales` (first sentence); nothing else.
  - ERD files: change the money fields `int FinishedPrice` and `int Price` to `long` (four lines).
  - `.github/copilot-instructions.md`: `MobileShop.Dal/Repos` → `MobileShop.Dal/Repo`; `IUserDataService.EnsureAdminUser()` → `IAccountDataService.EnsureAdminUser()`; the test description now reads "EF Core InMemory for most tests, plus temp-file SQLite tests for migrations and the database migrator"; add one bullet for the Production database command (`dotnet run --project src/MobileShop.Web -- --migrate-database`, backup first, normal Production startup only checks the schema) and one for money (whole IRR as `long`, `MoneyLimits.MaxRials`, `ToIrr()`/`ToGroupedDigits()` for display).
- Edge cases: change only the lines named above; do not rewrap or reformat paragraphs.
- Verify: CI run number; build and tests green, Production smoke still green (it requires `IRR` on `/Reports/ProfitLoss`).
- Done when: the listed lines are corrected and CI is green.
- Risk: LOW
- Confidence: HIGH

## [ ] Step 3 — Usings policy and empty-namespace usings
- Files
  - modify
    - Dal: `GlobalUsings.cs` gains `global using Microsoft.EntityFrameworkCore.Infrastructure;` and `global using Microsoft.Extensions.Logging;`; `Initialization/DatabaseMigrator.cs` loses its two file-local usings.
    - Web: `GlobalUsings.cs` gains `global using Microsoft.AspNetCore.Mvc.Rendering;`; `Pages/Products/Index.cshtml.cs` loses its file-local using.
    - Tests: `GlobalUsings.cs` gains the namespaces used by two or more test files — `MobileShop.Models.ViewModels`, `MobileShop.Models.ViewModels.Web.BindModels`, `MobileShop.Services.Logging.Settings`, `MobileShop.Services.PDF`, `MobileShop.Services.PDF.Configuration`, `Microsoft.Extensions.Options` — and the file-local usings for those namespaces are removed from `TransactionFactorExtensionsTests`, `ProfitLossRowViewModelTests`, `DistributionCalculatorTests`, `ReportsDataServiceTests`, `PeopleDataServiceTests`, `TransactionsDataServiceTests`, `ProductsDataServiceTests`, `QuestPdfGeneratorTests`, `ModuleInitializer.cs`; the redundant `using MobileShop.Models.Extensions;` in `MoneyExtensionsTests.cs` is removed (already global). `using Xunit;` in `DistributionCalculatorTests.cs` is removed only if the Tests project already has `Xunit` implicitly available (it does through the test SDK usings; if CI says otherwise, restore it).
    - Empty namespaces: remove `global using MobileShop.Web;` from the Web `GlobalUsings.cs` and `global using MobileShop.Tests.Dal;` from the Tests `GlobalUsings.cs` (neither namespace declares a type). Keep a removal only if the CI build is green without it.
  - `.github/copilot-instructions.md`: keep the statement that feature files carry no `using` directives and list the remaining exceptions precisely (EF migrations, `*.Designer.cs`, `_ViewImports.cshtml`, `ModuleInitializer.cs` if `System.Runtime.CompilerServices` stays file-local, and any using restored under the fallback below).
  - do not touch: production logic, entity files, other files
- Fallback: if a promotion causes an ambiguous-type error (`CS0104`, for example `Color`) or another conflict, revert only that one using to file-local and list it in `act.md` as a documented exception. Do not rename types to resolve it.
- Edge cases: `QuestPDF.Infrastructure` is global in `MobileShop.Services` and makes `Color` ambiguous there; do not add it to other projects' global usings.
- Verify: CI run number; clean build with 0 warnings and every test passing; `grep -rn "^using " --include=*.cs src` outside migrations, Designer files and the recorded exceptions returns nothing.
- Done when: the convention holds (or its documented exceptions are listed) and CI is green.
- Risk: MEDIUM
- Confidence: MEDIUM

## [ ] Step 4 — `.editorconfig` and final newlines
- Files
  - create: `.editorconfig` at the repository root with `root = true`, `[*]`, `charset = utf-8`, `insert_final_newline = true`
  - modify: every tracked text file with extension `cs|cshtml|js|css|sh|ps1|yml|yaml|md|json|csproj|props|slnx` that lacks a final newline (99 at planning time; excluding `src/MobileShop.Dal/Migrations/*` and `.clinerules/chat/*`)
  - do not touch: migrations, binary files, `*.db`, any content of any file
- Method: generate the list with a script (loop over `git ls-files`, test the last byte), append one `\n` to each, and record the list size in `act.md`. No editor reformatting.
- Edge cases: files with CRLF endings (none were found at planning time; if a file has `\r`, append `\r\n`); do not touch empty files.
- Verify: every changed file differs only by the final newline (`git diff --numstat` shows `1 1` per file except `.editorconfig`; `git diff --ignore-space-at-eol --stat` shows no content change); CI run number; build 0 warnings, all tests and the Production smoke green.
- Done when: no tracked text file in scope lacks a final newline, `.editorconfig` exists and CI is green.
- Risk: LOW
- Confidence: HIGH

## [ ] Step 5 — Final Stage U validation
- Files: none expected (validation only).
- Verify (record all evidence in `act.md`)
  - final clean build with 0 warnings and `0 skipped` tests (`Skip` search recorded), all suites and the Production smoke green; run number recorded;
  - `git diff --stat <stage-start>..HEAD` shows no change under `src/MobileShop.Api`, authentication, entities, migrations, `DatabaseInitializer`/`SampleDataInitializer` logic or any unrelated file;
  - a repository search shows no remaining `&amp;`, `Dal/Repos`, `IUserDataService` or `ToString("N0")` on money.
- Done when: all of the above pass and are recorded; the reviewer signs Stage U off (only the reviewer ticks `to-do.md`).
- Risk: LOW
- Confidence: HIGH

## Global Definition of Done
- The phone model dropdown and validation agree; the new tests pass.
- Profit report, README, ERD and `.github/copilot-instructions.md` match the code; the usings convention holds or its exceptions are documented; every text file in scope ends with a newline.
- Clean build with 0 warnings, 0 skipped tests, whole suite and Production smoke green; no Api, auth, entity, migration or Development-initialization change.

## Carry-over to later stages
- **Stage V onward:** later product-type stages call `GetModelsAsync(manufacturerId, "<category>")` for their own models; IDE-analyzer tooling (unused usings and members) is a candidate for Stage AC.

## Execution notes
Work compact: chain dependent commands with `&&`, group read-only checks in one `{ ...; }` call, keep reports short, do not restate this plan in chat. One step → one commit → STOP for Job B. Do not run `dotnet build`/`dotnet test` locally; push and report `Action: #<run_number> — <Success|Failure|Pending>` per step. Never touch `to-do.md`, `plan.md` or `audit.md`; never amend; check each report against `git show <hash> --stat` before recording it.
