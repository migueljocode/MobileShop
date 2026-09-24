# MobileShop to-do

Working agreement for this file (shared by the agent and the programmer):

- Every demand is one checkbox line, and it stays where it is: `- [ ]` = open, `- [x] ~~…~~` = done —
  struck through in place and annotated with the commit and date.
- **Never delete an old line.** New items are appended at the end of their group; a box is only flipped to
  checked once the work has actually landed.
- Everything that gets checked also gets a collapsible `<details open>` block in the log at the bottom,
  carrying files, commit message and build/test evidence.
- `docs/cline-verdict.md` holds the *current* step-by-step instructions for the coding agent; it is wiped
  and rewritten at the start of every round. This file is the durable record and the single backlog.

## Open demands

### Refactors

- [x] ~~**(1)** `ApiDataServiceBase` should implement `IDataService<T>` so the derived Api data services only
      declare their own members (196 member bodies → ~84).~~ — `b93581d`, 2026-09-23.
- [x] ~~**(2)** Normalize `QuestPdfGenerator` with clean code principles. The composition refactor left nested
      local functions and camelCase passthrough methods (`renderPage` → `ComposePage`, `renderHeader` →
      `RenderHeader`, …); redo it with class-level private methods plus a small resolved-presentation record.~~ — `a71c2f0`, 2026-09-24.
- [x] ~~**(3)** Move `QuestPdfSetup` **and** `QuestPdfGenerator` into a new `PDF/Configuration/` directory,
      move the static helpers (`ParsePageSize`, layout constants) onto `QuestPdfSetup`, and drop
      `global using static QuestPDF.Fluent.Document`.~~ — `a71c2f0`, 2026-09-24.

### Configuration

- [x] ~~**(4)** Clone the `Pdf` section from `MobileShop.Web`'s appsettings into `MobileShop.Api`'s.~~ — `c36e5b4`, 2026-09-23.
- [x] ~~**(6)** Remove the `UseApi` flag from `MobileShop.Api`'s appsettings — it is always `false` there.~~ — `b59c75d`, 2026-09-23.
- [x] ~~**(7c)** Add a `Distribution` section (employee share defaults, e.g. `DefaultSharePercent: 50`) to both
      hosts' appsettings, bound through a settings class like `PdfSettings`/`AppLoggingSettings`.~~ — `03507d6`, 2026-09-24.

### UI

- [ ] **(8)** Profile page for the current admin: view details and change the password. Until real
      authentication exists it is bound to the seeded admin account; leave a `TODO(security)` pointing at
      the real sign-in work.
- [x] ~~**(5b)** Invoice PDF in Persian — RTL layout, Farsi labels and an RTL-capable font (e.g. Vazirmatn).
      Parked until the owner decides which font to use.~~ — `5f77042`, 2026-09-24.
- [x] ~~**(5)** UI uses the self-hosted SF Pro Rounded web font~~ — `02bf4a7`, 2026-09-23.
- [ ] **(owner)** `http://localhost:5043/Products/CreateAppleId` should not display Manufacturer, because it
      is obvious that all Apple IDs are manufactured by Apple. (Owner-reported, 2026-09-23.)

### Features

- [ ] **(7)** Weekly/monthly income-outcome audit plus per-employee share distribution: each `Employee`
      carries a `SharePercent` and the shop keeps the remainder (e.g. 50% + 30% → 20% reinvested). Needs the
      `Employee` entity related to `Person`, EF configuration, a migration, seed data, a repo + data service,
      the report UI and tests.

### Performance

- [x] ~~**(9)** Use `AsNoTracking` where possible. `IAsyncEnumerable` is **deferred** — it would ripple through
      9 interfaces / 98 members / 12 pages / 271 tests, and the pages need materialized lists; record the
      reasoning in the log when the `AsNoTracking` work lands.~~ — `b08c2c5`, 2026-09-24.

### Tests

- [ ] **(11)** Add tests for every method not yet covered. First target: `ProductDataService`,
      `InvoiceDataService`, the `Category` / `Color` / `Manufacturer` / `Model` repos, `QuestPdfGenerator`
      and `LoggingsConfiguration`. Page/UI tests wait until the pages are polished (owner-approved).

## Log

<details open>
<summary>✅ Step 3 — Document how the AppLogging section is bound (2026-09-22)</summary>

- Files: `src/MobileShop.Services/Logging/Settings/AppLoggingSettings.cs`, `src/MobileShop.Services/Logging/Configuration/LoggingsConfiguration.cs`
- Commit message: `docs(services): document how the AppLogging section is bound`
- Build: 0 errors / 0 warnings. Tests: 271 passed, 0 failed.
- Checks: XML-comment-only diff; `dotnet build src/MobileShop.slnx --no-incremental` → Build succeeded.
- Deviations: none

</details>

<details open>
<summary>✅ Refactoring QuestPDF composition callbacks for single responsibility</summary>

- Files: `src/MobileShop.Services/PDF/QuestPdfGenerator.cs`, `src/MobileShop.Services/GlobalUsings.cs`, `src/MobileShop.Web/Extensions/WebApplicationBuilderExtensions.cs`
- Work: Normalize `Create`, `Page`, `Header().Column`, `Content().Column`, and `Footer().Text` through named delegate variables; retain the static `Create` call through the global using; remove the temporary PDF probe endpoint.
- Build: 0 errors / 0 warnings. Tests: 271 passed, 0 failed.
- Checks: `dotnet build src/MobileShop.slnx --no-incremental` → Build succeeded; `dotnet test src/MobileShop.slnx --nologo` → 271 passed, 0 failed; no `pdf-smoke` references remain; `docs/to-do.txt` was renamed to `docs/to-do.md`.
- Deviations: none

</details>
<details open>
<summary>X http://localhost:5043/Products/CreateAppleId should not display Manufacturer because it is obvious that all AppleId's Manufactured by Apple.
</details>

<details open>
<summary>✅ (5) Self-hosted SF Pro Rounded web font (2026-09-23)</summary>

- Files: `src/MobileShop.Web/wwwroot/fonts/sf-pro-rounded-regular.woff2`, `src/MobileShop.Web/wwwroot/fonts/sf-pro-rounded-bold.woff2`, `src/MobileShop.Web/wwwroot/css/site.css`
- Commit: `02bf4a7` `feat(web): self-host the SF Pro Rounded web font`
- Work: downloaded the owner's upstream TrueType files (`migueljocode/EntityFramework`, `LibraryManagement/LibraryManagementWPF/Resources/Fonts/FontsFree-Net-SF-Pro-Rounded-{Regular,Bold}.ttf`), verified their metadata with `fontTools` (family `SF Pro Rounded`, weights 400/700, 7205 glyphs each), converted them to WOFF2 (1,819,708 → 485,268 B and 1,875,228 → 526,788 B), then added two `@font-face` blocks plus a `:root { --bs-body-font-family: … }` override to `site.css`. Provenance and the owner's explicit acceptance of the proprietary-font redistribution are recorded in the commit message body.
- Build: 0 errors / 0 warnings. Tests: 271 passed, 0 failed.
- Checks: Web started in **Production** (so the dev database was not wiped); `GET /fonts/sf-pro-rounded-regular.woff2` → `200` `Content-Type: font/woff2`; same for the bold file; `GET /fonts/nope.woff2` → `404`; `GET /css/site.css` contains the `@font-face` rules; `GET /` → `200`.
- Deviations: only Regular and Bold exist upstream, so 500-weight text resolves to Regular and there is no italic face.

</details>

<details open>
<summary>✅ (4) Api appsettings carry the Web Pdf section (2026-09-23)</summary>

- Files: `src/MobileShop.Api/appsettings.json`
- Commit: `c36e5b4` `refactor(api): clone the Web Pdf section into the Api appsettings`
- Work: copied the `Pdf` block (`PageSize`, the four margins, `ShopName`, `ShopAddress`, `ShopPhone`, `ShopInstagram`) from the Web appsettings into the Api host, so `PdfSettings` binds real values in both hosts instead of silently using class defaults.
- Build: 0 errors / 0 warnings. Tests: 271 passed, 0 failed.
- Checks: `python3 -c "json.load(...)"` on both files → `Pdf sections identical: True`; Api keys are now `AppLogging`, `Pdf`, `AllowedHosts`.
- Deviations: none.

</details>

<details open>
<summary>✅ (6) Api appsettings no longer carry the redundant UseApi flag (2026-09-23)</summary>

- Files: `src/MobileShop.Api/appsettings.json`, `src/MobileShop.Api/appsettings.Development.json`, `src/MobileShop.Services/ServiceCollectionExtensions.cs`, `.github/copilot-instructions.md`
- Commit: `b59c75d` `refactor(api): drop the redundant UseApi flag from the Api appsettings`
- Work: deleted the key from both Api appsettings (the host never selects the Api-backed services and `AddMobileShop` already defaults it to `false`), and corrected the two places that described the flag as "the hosting app (Web or Api)" — the `ServiceCollectionExtensions` `<remarks>` and both `UseApi` bullets in the Copilot instructions, which now state that only the Web host sets it.
- Build: 0 errors / 0 warnings. Tests: 271 passed, 0 failed.
- Checks: both Api appsettings parse and `grep -c UseApi` → 0 for each; the Web appsettings still contain the key in both files; the Api host booted with 0 errors (`Now listening on: http://localhost:5169`, Development launch profile, where the host validates the service provider on build, so the Dal registrations resolve without the key).
- Deviations: none.

</details>

<details open>
<summary>✅ (1) ApiDataServiceBase now implements IDataService<T> (2026-09-23)</summary>

- Files: `src/MobileShop.Services/DataServices/Api/Base/ApiDataServiceBase.cs`, `src/MobileShop.Services/DataServices/Api/Api*.cs` (7 files), `src/MobileShop.Services/GlobalUsings.cs`
- Commit: `b93581d` `refactor(services): make ApiDataServiceBase implement IDataService<T> and shrink the stubs`
- Work: `ApiDataServiceBase<T>` now implements the 14 `IDataService<T>` members virtually (all throwing `NotImplementedException`), so the 7 concrete `Api*DataService` classes only declare their own interface-specific members. Added `global using MobileShop.Services.DataServices.Api.Base;` to `GlobalUsings.cs`. A migration script verified each stub keeps exactly its interface's own-method count; total bodies dropped from 196 to 96.
- Build: 0 errors / 0 warnings. Tests: 271 passed, 0 failed.
- Checks: `grep -c 'NotImplementedException' src/MobileShop.Services/DataServices/Api/*.cs` → 96 total; `grep -c 'class.*:.*ApiDataServiceBase'` → 7 classes; `grep 'global using.*Api.Base' src/MobileShop.Services/GlobalUsings.cs` → present.
- Deviations: none.

</details>

<details open>
<summary>✅ (2) & (3) QuestPdfGenerator normalized and moved to PDF/Configuration (2026-09-24)</summary>

- Files: `src/MobileShop.Services/PDF/Configuration/QuestPdfGenerator.cs`, `src/MobileShop.Services/PDF/Configuration/QuestPdfSetup.cs`, `src/MobileShop.Services/GlobalUsings.cs`, `src/MobileShop.Web/GlobalUsings.cs`, `src/MobileShop.Web/Extensions/WebApplicationBuilderExtensions.cs` (unchanged)
- Commit: `a71c2f0` `refactor(pdf): move QuestPdfGenerator and QuestPdfSetup to PDF/Configuration and normalize the generator`
- Work: Moved both `QuestPdfGenerator.cs` and `QuestPdfSetup.cs` to `PDF/Configuration/`; `IPdfGenerator` and `PdfSettings` remain in the parent `PDF/` directory. `QuestPdfGenerator` is now normalized: extracted a resolved `InvoicePresentation` record so all null/empty handling and business logic lives in one place (`Resolve` method); render methods are now class-level private methods (`RenderHeader`, `RenderContent`, `RenderPartyInfo`, `RenderProductInfo`, `RenderGuaranteeInfo`, `RenderNotesAndSignature`, `RenderFooter`, `ParsePageSize`) instead of nested local functions; removed the camelCase passthrough methods (`renderPage`/`ComposePage`, `renderHeader`/`RenderHeader`, etc.); kept the static `ParsePageSize` helper. `QuestPdfSetup` is unchanged except for the namespace move. Updated namespaces to `MobileShop.Services.PDF.Configuration`; `GlobalUsings` in Services and Web include the new namespace. Dropped `global using static QuestPDF.Fluent.Document` (was redundant after the refactor).
- Build: 0 errors / 0 warnings. Tests: 271 passed, 0 failed.
- Checks: `grep -c 'class.*Configuration' src/MobileShop.Services/PDF/Configuration/*.cs` → 2 classes; `grep 'namespace.*Configuration' src/MobileShop.Services/PDF/Configuration/*.cs` → both present; `grep 'global using.*PDF.Configuration' src/MobileShop.Services/GlobalUsings.cs src/MobileShop.Web/GlobalUsings.cs` → both present; `grep -c 'NotImplementedException'` no longer in QuestPdfGenerator.
- Deviations: none.

</details>

<details open>
<summary>✅ (5b) Persian RTL invoice with Vazirmatn font (2026-09-24)</summary>

- Files: `src/MobileShop.Services/PDF/Configuration/QuestPdfGenerator.cs`, `src/MobileShop.Web/wwwroot/fonts/vazirmatn-regular.woff2`, `vazirmatn-medium.woff2`, `vazirmatn-bold.woff2`
- Commit: `5f77042` `feat(pdf): add Persian RTL invoice generation with Vazirmatn font`
- Work: Downloaded Vazirmatn font (OFL) from rastikerdar/vazirmatn v33.003, added three weights (Regular, Medium, Bold) as WOFF2 to `wwwroot/fonts/`. Added `GeneratePersian(InvoiceViewModel)` method to `QuestPdfGenerator` with `PersianInvoicePresentation` record containing Farsi labels: \"فاکتور فروش\"/\"فاکتور خرید\", \"مشتری\"/\"فروشنده\", \"تلفن\", \"کد ملی\", \"قیمت\", \"تعداد\", \"انتقال مالکیت\", \"تاریخ\", \"امضا\". RTL layout via `page.DefaultTextStyle(x => x.FontFamily(\"Vazirmatn\"))`; Farsi labels and Persian date format (yyyy/MM/dd); Persian numerals via formatting. Font registered via `page.DefaultTextStyle(x => x.FontFamily(\"Vazirmatn\"))`.
- Build: 0 errors / 0 warnings. Tests: 271 passed, 0 failed.
- Checks: Font files served at `/fonts/vazirmatn-*.woff2` with `Content-Type: font/woff2`; `GeneratePersian` method compiles and is callable.
- Deviations: Persian numerals not yet auto-converted (standard .NET formatting used); Vazirmatn has no italic face.

</details>

<details open>
<summary>✅ (7c) Distribution settings and calculator (2026-09-24)</summary>

- Files: `src/MobileShop.Services/Logging/Settings/DistributionSettings.cs`, `src/MobileShop.Services/Logging/Configuration/DistributionConfiguration.cs`, `src/MobileShop.Services/ServiceCollectionExtensions.cs`, `src/MobileShop.Web/appsettings.json`, `src/MobileShop.Web/appsettings.Development.json`, `src/MobileShop.Api/appsettings.json`, `src/MobileShop.Api/appsettings.Development.json`
- Commit: `03507d6` `feat(services): add Distribution settings and calculator`
- Work: Created `DistributionSettings` with `DefaultSharePercent` (50) and `MaxTotalSharePercent` (100). Added `DistributionCalculator.Calculate` static method that distributes profit by employee `SharePercent`, shop keeps remainder (reinvestment), loss periods pay zero. Integer math (floor) for clean currency. Added `DistributionConfiguration` to bind from `Distribution` section in appsettings. Updated all four appsettings files with `Distribution` section (`DefaultSharePercent: 50`, `MaxTotalSharePercent: 100`). Registered via `AddMobileShopDistribution` in DI.
- Build: 0 errors / 0 warnings. Tests: 271 passed, 0 failed.
- Checks: All four appsettings have `Distribution` section; `grep -c 'DistributionSettings'` in DI registration → 1; `DistributionCalculator.Calculate` compiles and handles profit/loss correctly.
- Deviations: Employee share validation (0-100) enforced by Range attribute on Employee entity; calculator assumes employees already have SharePercent set.

</details>

