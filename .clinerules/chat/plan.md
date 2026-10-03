# Plan — Stage T — IRR money foundation

## Requirements
- Functional
  - Every money amount in the app is whole IRR (Rials), stored and calculated as `long` (prices can reach 1,000,000,000 IRR and more; `int` caps at 2,147,483,647).
  - A single unit indicator (**IRR**) appears on every page that shows or asks for money; the Persian factor PDF keeps the Persian name of the same unit ("ریال").
  - All seeded sample data is expressed in Rials.
  - Price inputs accept whole numbers only (no decimals, no precision); thousands separators are display-only. Percentages stay decimal.
  - Sums, profit/loss totals, factor totals and the 40/50/10 distribution never overflow.
- Non-functional: no data loss, the Stage S migration/CI harness stays green, CI evidence for every step, no change to the Api host, authentication or Development initialization policy.
- Constraints (project-specific-rules.md): API untouched, no auth changes, Apple ID inventory passwords stay plaintext, EF configuration centralized, project-wide usings in `GlobalUsings.cs`, no `bin`/ `obj` changes.

## Assumptions and decisions (labelled)
- **D1 (owner, decided):** money is `long`. Real overflow risks found while planning: `TransactionFactorViewModel.TotalPrice` uses checked `Enumerable.Sum` on `int` (throws above 2.1B), and `DistributionCalculator` computes `totalProfit * 40 / 100` / `* 50 / 100` in `int` arithmetic, which silently wraps once a period's profit passes about 53.7 million IRR (it is already wrong at Rial scale).
- **D2 (planner):** `MoneyLimits.MaxRials = 10_000_000_000_000` (10^13 IRR). It keeps every client-side calculation exact (10^13 × 100 < 2^53 in JavaScript) and is far above any real price.
- **D3 (planner default, owner can overrule):** the factor PDF keeps "ریال" (Persian for Rial = IRR); web pages show "IRR".
- **D4 (planner assumption — owner please confirm):** existing Production rows are taken to be Rials already (the earlier `UseIntegerRialMoney` stage and the PDF's "ریال" label assume it); no automatic ×10 data conversion is performed. If real data was entered in Toman, a separate, reviewed data-migration step is needed before Production use.
- **D5 (planner):** the seeded values (phones around 45,000,000) are Toman-scale, so the sample data is multiplied by 10.
- **D6 (planner):** SQLite stores `int` and `long` as the same 64-bit `INTEGER`, so widening needs **no table rebuild**. A no-op migration `WidenMoneyToLong` records the CLR type change in the model snapshot; this is what keeps the Stage S snapshot test green.
- **A1:** GitHub Actions is the build/test gate. The actor does not run `dotnet` locally; each step is verified by the pushed commit's run, reported as `Action: #<run_number> — <Success|Failure|Pending>`.

## Reviewer Briefing
- **Step 1 is HIGH risk / MEDIUM confidence:** a wide mechanical type change plus a hand-written migration, Designer and snapshot edit. Lessons from Stage S apply: the Designer needs `using Microsoft.EntityFrameworkCore.Migrations;`, `[DbContext(typeof(AppDbContext))]`, `[Migration("<id>")]` and a `BuildTargetModel` equal to the updated snapshot; the history count assertions change from 6 to 7 in tests and in `production-smoke.sh`.
- **Step 2** adds the boundary tests the plan promised in Stage S; check that each test crosses `int.MaxValue`.
- **Step 4 is MEDIUM / MEDIUM:** Razor and JS output is not unit-testable; evidence is the CI smoke greps plus reading the diff for every `.ToString("N0")` replaced.
- No `int` money may remain: search for `int` near `Price|Profit|Amount|Total|Sold|Bought|FinishedPrice|SuggestedPrice` after Step 1.

## [ ] Step 1 — Widen money to `long` end to end (no behaviour change)
- Files
  - inspect: `src/MobileShop.Models/Entities/{Product,Transaction}.cs`, the view models and bind models listed below, `ProductsDataService.cs` (`ComputeFinishedPrice`, create flows), `TransactionsDataService.cs`, `ReportsDataService.cs`, `IReportsDataService.cs`, `ApiReportsDataService.cs`, `Logging/Settings/DistributionSettings.cs`, `PDF/Configuration/QuestPdfGenerator.cs` (DTOs near lines 498 and 519), `AppDbContextModelSnapshot.cs`, `20261002060000_UseIntegerRialMoney.Designer.cs` (format reference)
  - modify
    - Entities: `Product.Price` and `Transaction.FinishedPrice` → `long`.
    - View models: `InvoiceViewModel`, `ProductTransactionViewModel`, `TransactionFactorRowViewModel`, `TransactionCardViewModel`, `TransactionDetailsViewModel`, `TransactionListItemViewModel` (`FinishedPrice`), `TransactionFactorViewModel.TotalPrice`, `ProfitLossRowViewModel` (`Bought`, `Sold`, `Profit` → `long`; `ProfitPercent` stays `decimal`), `ProductListItemViewModel.SuggestedPrice` → `long?`.
    - Bind models: `CreatePhoneInputModel`, `CreateAppleIdInputModel`, `CreateGlassInputModel` (`Price`, `ProfitAmount` → `long`/`long?`), `BuyInputModel`, `SellInputModel` (`Price` → `long`). Leave all id fields as `int` and all percent fields as `decimal?`.
    - Services: `ProductsDataService.ComputeFinishedPrice(long paid, decimal? percent, long? amount)` (floor to `long`), `ReportsDataService`/`IReportsDataService`/`ApiReportsDataService` (`GetProfitLossTotalAsync` → `Task<long>`, `GetDistributionRowsAsync(long totalProfit)`), `DistributionSettings.cs` (`Calculate(long totalProfit, …)`, `DistributionRow.CalculatedAmount` → `long`), `QuestPdfGenerator` DTOs and mappings, `TransactionsDataService`/`HomeDataService` assignments, `Reports/ProfitLoss.cshtml.cs` (`TotalProfit` → `long`).
    - Dal: `AppDbContextModelSnapshot.cs` (`b.Property<long>("Price")` and `b.Property<long>("FinishedPrice")` only).
    - create: `src/MobileShop.Dal/Migrations/20261004090000_WidenMoneyToLong.cs` and `…Designer.cs`.
    - Tests/scripts: only what CI or the history count requires — `MigrationChainTests` (7 migration ids in order, history rows 7), `DatabaseMigratorTests` (history rows 6 → 7 where the DB is migrated to latest; the 5-row backup assertion for a DB migrated only to `AddPartNumber` stays 5), `.github/scripts/production-smoke.sh` (`HISTORY_COUNT == "7"`), and `int` → `long` literal fixes in tests.
  - do not touch: `src/MobileShop.Api`, authentication, `DatabaseInitializer`, EF configuration (the CHECK constraints stay `>= 0`), the `UseIntegerRialMoney` migration, any other migration
- Symbols
  - `partial class WidenMoneyToLong : Migration` with `Up`/`Down` containing only a comment: SQLite stores `int` and `long` as INTEGER (64-bit), so no SQL is needed and the migration only records the model type change.
  - The Designer carries `using Microsoft.EntityFrameworkCore.Migrations;`, `[DbContext(typeof(AppDbContext))]`, `[Migration("20261004090000_WidenMoneyToLong")]` and `BuildTargetModel` whose body is the **updated** snapshot body (same `ProductVersion` annotation).
- Current → Desired: money is `int`, sums and distribution math overflow at Rial scale → money is `long` everywhere with identical behaviour for values that already fit.
- Edge cases: `Sum` on `long`; remove `(int)` casts; `ProfitPercent` division stays decimal; JSON seed loading maps numbers to `long`; the CI harness (`Snapshot_matches_the_current_model`) must pass with the snapshot and model both `long`; do not regenerate the whole snapshot — change only the two property types. If the snapshot test reports other differences, stop and report them.
- Verify: push and report `Action: #<run_number>`; expect a clean build (0 warnings) and every test passing, including the Stage S migration tests with 7 migrations.
- Done when: no `int` money remains (search recorded in `act.md`), the build and the whole suite are green, and Stage S's tests and smoke still pass.
- Risk: HIGH
- Confidence: MEDIUM

## [ ] Step 2 — Money limit and overflow-boundary tests
- Files
  - create: `src/MobileShop.Models/MoneyLimits.cs`; `src/MobileShop.Tests/Dal/EfStructures/MoneyBoundaryTests.cs`; boundary tests added next to the existing ones in `DistributionCalculatorTests.cs`, `ReportsDataServiceTests.cs`, `TransactionFactorExtensionsTests.cs`, `ProductsDataServiceTests.cs`, `TransactionsDataServiceTests.cs`
  - modify: the six money inputs' `[Range]` attributes, `TransactionsDataService` (`RecordBuyAsync`/`RecordSellAsync`), the three create flows in `ProductsDataService`
  - do not touch: entities, migrations, pages, PDF
- Symbols
  - `public static class MoneyLimits { public const long MaxRials = 10_000_000_000_000L; }`
  - `[Range(0, MoneyLimits.MaxRials)]` on `Price`/`ProfitAmount` of the three create inputs and on `Price` of Buy and Sell (compiles to the `double` overload, exact up to 2^53).
  - Services reject `Price > MoneyLimits.MaxRials` with a friendly message ("The price is too large.") in the existing validation style; a computed finished price above the limit returns a failed `ServiceResult` with `ErrorField` `Price`.
- Tests (each value must cross `int.MaxValue`)
  1. `MoneyBoundaryTests`: persist `Product.Price = 5_000_000_000L` and `Transaction.FinishedPrice = 3_000_000_000L` and read them back exactly (temp-file SQLite, migrated database).
  2. Factor: three rows of 1,500,000,000 give `TotalPrice` 4,500,000,000.
  3. Reports: bought 3,000,000,000 and sold 5,000,000,000 give `Profit` 2,000,000,000, correct `ProfitPercent`, and `GetProfitLossTotalAsync` exact.
  4. Distribution: `Calculate(5_000_000_000L, …)` gives 2,000,000,000 / 2,500,000,000 / 500,000,000; a loss of -3,000,000,000 goes entirely to the shop.
  5. Create flows: paid 2,000,000,000 plus a 25% profit stores 2,500,000,000 (amount-first rule unchanged).
  6. Limits: model validation rejects `MoneyLimits.MaxRials + 1` on the six inputs and accepts `MaxRials`; services reject an over-limit price and write nothing.
- Verify: CI run number; expect all tests passing.
- Done when: the limit exists, all six boundary scenarios pass in CI and no behaviour below the limit changed.
- Risk: MEDIUM
- Confidence: MEDIUM

## [ ] Step 3 — Seed data in Rials
- Files
  - modify: `src/MobileShop.Dal/Initialization/sample-data.json` — **only** `products[].price` (17 rows) and `transactions[].finishedPrice` (26 rows), multiplied by 10; tests that assert seeded money values (for example `SampleDataSeedTests` and any report/home test that sums seeded rows — CI will show them)
  - do not touch: any other JSON field, the loader, entities
- Method: apply the ×10 with a script (python/jq) so no value is hand-edited; the diff must show only those two fields changing.
- Test: `Sample_data_money_is_expressed_in_rials`: every seeded `Price` and `FinishedPrice` is a multiple of 10, at least 1,000,000 and at most `MoneyLimits.MaxRials`.
- Edge cases: the Production smoke seeds a Development database from this file, so its fingerprints change but must stay equal before and after migration within one run.
- Verify: CI run number; expect all tests and the smoke step passing.
- Done when: the seeded data is in Rials, tests updated, CI green.
- Risk: MEDIUM
- Confidence: HIGH

## [ ] Step 4 — IRR indicator on every page and integer-only price inputs
- Files
  - create: `src/MobileShop.Models/Extensions/MoneyExtensions.cs`, `src/MobileShop.Tests/Models/Extensions/MoneyExtensionsTests.cs`
  - modify (Razor/JS/PDF)
    - Displays: `Pages/Index.cshtml` (finished price), `Pages/Reports/ProfitLoss.cshtml` (bought, sold, profit, percent table amounts, distribution amounts, totals), `Pages/Products/Details.cshtml`, `Pages/Transactions/Index.cshtml`, `Pages/Transactions/Details.cshtml`, `Pages/Shared/_ProductPickerOptions.cshtml` (suggested price) and any other `.ToString("N0")` money display.
    - Labels: "Paid price (IRR)", "Profit amount (IRR)" (replacing the stray "Profit $"), Buy/Sell `[Display(Name = "Finished price (IRR)"])`, and "(IRR)" on the money table headers (Price, Bought, Sold, Profit / loss, Amount) and total-profit lines. Percent labels stay as they are.
    - Inputs: the six money inputs get `inputmode="numeric"`, `step="1"`, `min="0"`, `max="@MoneyLimits.MaxRials"`; percent inputs are untouched.
    - JS: `create-product-pricing.js` and `product-picker.js` keep whole-number math (`Math.floor`); fix the stale "profit Rial/$" comments.
    - PDF: `QuestPdfGenerator.cs` formats row and total prices with `ToGroupedDigits()`, keeps "ریال" for the total, adds the unit to the price column header, and adds the unit to the two plain-text fallback lines (`Price: …`, `قیمت : …`); keep the Persian/RTL tests green and update tests that assert the old format.
    - CI smoke: `.github/scripts/production-smoke.sh` — after the route loop, `curl` `/`, `/Transactions` and `/Reports/ProfitLoss` and require `IRR` in each response.
  - do not touch: services, entities, migrations, `src/MobileShop.Api`
- Symbols (`MoneyExtensions`)
  - `ToGroupedDigits(this long value)` → `value.ToString("N0", CultureInfo.InvariantCulture)`
  - `ToIrr(this long value)` → `ToGroupedDigits() + " IRR"`; add `long?` overloads that return an empty string for `null`.
- Tests: `MoneyExtensionsTests` covers 0, 1,234,567, 4,500,000,000, negative -1,500 and `null`; update PDF and page-model tests that asserted the previous formatting.
- Edge cases: use invariant culture so the separator never depends on the server locale; negative profit keeps its sign and the existing success/danger CSS class; Razor is not unit-tested, so verify by the smoke greps and by reading the diff for any remaining `ToString("N0")` on money.
- Verify: CI run number; the smoke step shows `IRR` on the three pages; all tests pass.
- Done when: every money display and money label shows the IRR unit, inputs are integer-only, the PDF keeps its Persian unit, and the smoke greps pass in CI.
- Risk: MEDIUM
- Confidence: MEDIUM

## [ ] Step 5 — Docs and final Stage T validation
- Files: modify `README.md` with a "Money" section: IRR/Rial whole numbers stored as `long`, the `MoneyLimits.MaxRials` cap, the display convention (grouped digits plus IRR; Persian "ریال" on the factor PDF), seeded data in Rials, and the note that existing Production rows are not converted (D4).
- Verify (record all evidence in `act.md`): the final workflow run is green — build with 0 warnings, the full suite including the Stage S migration/legacy/migrator tests with 7 migrations, Bash and PowerShell log tests, and the production smoke with the IRR greps; `git diff --stat <stage-start>..HEAD` shows no change under `src/MobileShop.Api`, authentication, `DatabaseInitializer`/ `SampleDataInitializer` logic or any unrelated file (only the planned money, view-model, service, page, PDF, JSON, test and script files).
- Done when: the README section exists, the final run is `Success` and its number is recorded, and the reviewer signs Stage T off (only the reviewer ticks `to-do.md`).
- Risk: LOW
- Confidence: HIGH

## Global Definition of Done
- Money is `long` in entities, view models, bind models, services, PDF DTOs and the snapshot; a no-op `WidenMoneyToLong` migration is in the chain and every history count is 7.
- No overflow in sums, factor totals, profit/loss or the distribution; the boundary tests cross `int.MaxValue`.
- The IRR unit appears on every money display and label; inputs are integer-only; seeded data is in Rials; the PDF keeps "ریال".
- The Stage S harness and the Production smoke stay green; no Api, auth, entity-shape or Development-initialization changes.

## Carry-over to later stages
- **Stage U:** README still carries the personal "stage 3" note and a literal `&amp;`; the `Phone`-category filter default (keep it and restrict the phone model dropdown to Phone-category models) is recorded in Stage U's line.

## Execution notes
Work compact: chain dependent commands with `&&`, group read-only checks in one `{ ...; }` call, keep reports short, do not restate this plan in chat. One step → one commit → STOP for Job B. Do not run `dotnet build`/`dotnet test` locally; push and report `Action: #<run_number> — <Success|Failure|Pending>` per step. Never touch `to-do.md`, `plan.md` or `audit.md`; never amend.