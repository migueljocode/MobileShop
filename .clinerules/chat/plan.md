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

## ~~[x] Step 1 — Phone-category consistency in the model dropdown~~
- **Done** — Job B PASS (`c754e0b`; Action #388). `GetModelsAsync(manufacturerId, categoryName = "Phone")` on interface, service and Api stub; dropdown and `CreatePhoneAsync` validation agree.

## ~~[x] Step 2 — Display and documentation drift~~
- **Done** — Job B PASS (`cdf06c3`; Action #394). ProfitLoss unit and `ToGroupedDigits()`, README `&`, ERD `long` money, `.github/copilot-instructions.md` corrections.

## ~~[x] Step 3 — Usings policy and empty-namespace usings~~
- **Done** — Job B PASS (`e9f7da9`). Planned promotions made; `global using MobileShop.Web;` and `global using MobileShop.Tests.Dal;` removed.

## [ ] Step 4 — `.editorconfig` and final newlines
- **Status:** implemented in PRs #13 and #14 (Action #405 and #406 green) but Job B FAIL: 9 in-scope vendor files under `wwwroot/lib/bootstrap/dist` (`*.rtl.css`, `*.rtl.min.css`, `bootstrap.esm.min.js`) still lack a final newline. The actor appends `\n` to exactly those 9 files in one commit, waits for a green run before merging, records the result, and stops. (Already-touched `.map` and `jquery/LICENSE.txt` stay as they are.)
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
- **Carried from the independent Job B review (MEDIUM — do in this step):** `.github/copilot-instructions.md` line 51 says feature files carry no `using` directives and lists only migrations, Designers, `_ViewImports.cshtml`, `ModuleInitializer.cs` and restored singles, but about 20 test files (page-model, PDF, EF and logging tests) and three production files (`MoneyExtensions.cs`: `System.Globalization`; `ProductsDataService.cs`: `MobileShop.Services.DataServices.Shared`; `QuestPdfGenerator.cs`: `MobileShop.Models.Extensions`) still keep file-local usings. Do not promote more usings. Change only that one sentence so it states the real convention: project-wide usings live in `GlobalUsings.cs`; file-local usings remain only for namespaces used by few files (mainly test files), the three production files above, EF migrations, `*.Designer.cs`, `_ViewImports.cshtml` and `ModuleInitializer.cs`. `.github/copilot-instructions.md` becomes a modified file of this step.
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
