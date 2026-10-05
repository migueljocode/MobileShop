# Plan — Stage V — Products list: Available / Sold filter

## Requirements
- Functional
  - The Products list gets an **Availability** filter with three choices: All (default, today's behaviour), Available, Sold.
  - It applies live: changing the choice reloads the list immediately (same `onchange="this.form.submit()"` pattern as the existing part-number filter).
  - It works together with the existing type buttons (All / Phones / Apple IDs / Glasses) and the part-number filter; switching type keeps the chosen availability.
  - The filter and the existing status badge can never disagree: both use the same rule.
- Non-functional: no schema change, no migration, no new column; CI evidence for every step; no change to the Api host, authentication, entities or Development initialization.
- Constraints (project-specific-rules.md): API untouched, no auth changes, Apple ID inventory passwords stay plaintext, EF configuration centralized, project-wide usings live in `GlobalUsings.cs`, no `bin`/`obj` changes.

## Decisions (labelled)
- **D1 (single source of truth):** availability reuses the rule behind the existing badge: `ProductListItemViewModel.IsSold` is `Transactions.Any(t => t.Direction == TransactionDirection.Sell)` (soft-deleted transactions are already hidden by the global query filter). The filter is applied on `IsSold` after the rows are built, so badge and filter cannot diverge. Existing behaviour that stays unchanged: a product sold and later bought back still shows as Sold.
- **D2:** values are lowercase strings `"available"` and `"sold"` (same style as the `type` values `"phone"`, `"appleid"`, `"glass"`); `null`, `"all"` and anything unrecognised mean no filter.
- **D3:** the default stays All, and the Second-hand list page (`Products/SecondHand`) is out of scope.
- **D4:** no new column, entity, migration or JavaScript file.
- **A1:** GitHub Actions is the build/test gate. The actor does not run `dotnet` locally; each step is verified by the pushed commit's run, reported as `Action: #<run_number> — <Success|Failure|Pending>`; wait for a green run before merging.
- **A2 (lessons from earlier stages):** check a project's `GlobalUsings.cs` before using a type; compile errors only show in CI; compare each report with `git show <hash> --stat` before writing it. Razor output is not unit-testable here, so Step 2 adds smoke assertions on the rendered badge markup.

## Reviewer Briefing
- **Step 1 is LOW risk / HIGH confidence:** one defaulted parameter threaded through interface, implementation and Api stub, plus a post-filter.
- **Step 2 is MEDIUM / MEDIUM:** Razor markup and the smoke assertions can only be proven by CI. Check that the smoke script asserts both directions (the sold list contains the Sold badge and no Available badge, and vice versa), because the filter's own `<option>` text contains the words "Sold" and "Available".
- The seeded data has 17 products, 12 with a Sell transaction, so both lists are non-empty in the Production smoke.
- No `IndexModel` page tests exist for Products today; Step 2 creates the first one.

## [ ] Step 1 — Availability filter in the service
- Files
  - inspect: `src/MobileShop.Services/DataServices/Dal/ProductsDataService.cs` (`GetInventoryRowsAsync`, line ~35), `Interfaces/IProductsDataService.cs`, `Api/ApiProductsDataService.cs`, `src/MobileShop.Tests/Services/DataServices/Dal/ProductsDataServiceTests.cs` (the existing `GetInventoryRowsAsync_*` tests, `SeedCatalog`), `TransactionsDataServiceTests.cs` (how Transaction rows and the shop/person graph are seeded; `TestDataHelpers` only has `CreateProduct`)
  - modify: `IProductsDataService.cs`, `ProductsDataService.cs`, `ApiProductsDataService.cs`, `ProductsDataServiceTests.cs`
  - do not touch: Razor pages, entities, migrations, `src/MobileShop.Api`, authentication
- Symbols
  - `Task<IReadOnlyList<ProductListItemViewModel>> GetInventoryRowsAsync(string? type = null, int? partNumberId = null, string? availability = null);` on the interface (extend the XML doc: `"available"` returns only unsold rows, `"sold"` only sold rows, null/`"all"`/unrecognised returns both; pages lowercase before calling), the same signature on `ProductsDataService` and on the `ApiProductsDataService` stub (still throws).
  - In `ProductsDataService`, after the phone, Apple ID and glass blocks have filled `rows` and before `return`, filter in one place: `"available"` keeps `!row.IsSold`, `"sold"` keeps `row.IsSold`, anything else keeps all. Do not duplicate the Sell rule in any predicate.
- Current → Desired: the list can only be narrowed by type and part number → it can also be narrowed to available or sold rows, in the same order as before (phones, Apple IDs, glass, each by `ProductId`).
- Edge cases: unrecognised or null availability behaves exactly like today; the type filter and the part-number filter still apply first; ordering is preserved because the filter runs after concatenation; a product whose only Sell transaction is soft-deleted counts as available.
- Tests (in `ProductsDataServiceTests`; seed Sell transactions by following the pattern in `TransactionsDataServiceTests`)
  1. `GetInventoryRowsAsync_availability_available_returns_only_unsold_rows` (a phone, an Apple ID and a glass product, some with a Sell transaction).
  2. `GetInventoryRowsAsync_availability_sold_returns_only_sold_rows`.
  3. `GetInventoryRowsAsync_null_all_and_unrecognised_availability_return_everything` (a `[Theory]` over `null`, `"all"`, `"bogus"`).
  4. `GetInventoryRowsAsync_availability_combines_with_type` (`"phone"` + `"sold"` returns only sold phones).
  5. `GetInventoryRowsAsync_availability_combines_with_part_number_filter`.
  6. `GetInventoryRowsAsync_soft_deleted_sell_transaction_counts_as_available`.
  Existing tests keep passing unchanged (the new parameter is optional).
- Verify: push and report `Action: #<run_number>`; expect a clean build (0 warnings) and every test passing.
- Done when: the filter works for all three product kinds, the six tests pass in CI, and nothing else changed.
- Risk: LOW
- Confidence: HIGH

## ~~[x] Step 2 — Products page: live Availability filter, route carry-over, tests and smoke~~
- Files
  - inspect: `src/MobileShop.Web/Pages/Products/Index.cshtml`, `Index.cshtml.cs`, `.github/scripts/production-smoke.sh` (the route loops near line 144), an existing page-model test such as `CreateGlassModelTests.cs` for the Moq style
  - create: `src/MobileShop.Tests/Web/Pages/Products/IndexModelTests.cs`
  - modify: `Pages/Products/Index.cshtml.cs`, `Pages/Products/Index.cshtml`, `.github/scripts/production-smoke.sh`
  - do not touch: services, entities, migrations, the Second-hand page, `src/MobileShop.Api`, authentication
- Symbols
  - `IndexModel`: `public string Availability { get; private set; } = "all";` and `OnGetAsync(string? type = null, int? partNumberId = null, string? availability = null)`. Normalise with `availability?.Trim().ToLowerInvariant()`: `"available"` and `"sold"` are kept, everything else becomes `"all"`. Call `dataService.GetInventoryRowsAsync(Type, PartNumberId, Availability == "all" ? null : Availability)`.
  - A small read-only helper for links, e.g. `public string? AvailabilityRoute => Availability == "all" ? null : Availability;`.
  - View: render **one** GET form always (not only for phones): hidden `type`, an Availability `<select asp-for="Availability">` with the options All, Available, Sold and `onchange="this.form.submit()"`, and — inside the same form and only when `Model.ShowPartNumberFilter` — the existing part-number select unchanged. The four type links gain `asp-route-availability="@Model.AvailabilityRoute"` so switching type keeps the choice (the existing `partNumberId` behaviour stays as is). Keep the existing badge, table, buttons and empty state.
- Current → Desired: only type buttons and a phones-only part-number form → type buttons plus an always-visible live Availability filter that combines with the part-number filter.
- Edge cases: stale or invalid `availability` values fall back to All; the stale part-number handling stays as is; do not add JavaScript; the form posts `availability=all` when All is selected, which is valid.
- Tests (`IndexModelTests`, Moq on `IProductsDataService`; follow the style of the existing page-model tests)
  1. default request → `Availability` is `"all"` and the service receives `null`.
  2. `"Sold"` and `" sold "` → `"sold"`, service receives `"sold"`.
  3. `"available"` → `"available"`, service receives `"available"`.
  4. `"bogus"` → `"all"`, service receives `null`.
  5. phones view with `partNumberId` and `availability` → the service receives type `"phone"`, the part number and the availability together.
- Smoke (`production-smoke.sh`, after the existing IRR loop and before `stop_app`): request `/Products?availability=sold` and require the Sold badge markup `text-bg-secondary">Sold` and the absence of `text-bg-success">Available`; request `/Products?availability=available` and require the opposite; request `/Products?type=phone&availability=sold` and require HTTP 200. Use the same curl style as the neighbouring loops (`--silent --show-error --fail --max-time 10`); the local-run guard and timeouts stay as they are.
- Verify: push, wait for a green run before merging, report `Action: #<run_number>`; expect a clean build (0 warnings), every test passing and the smoke step green.
- Done when: the filter works live with type and part number, the five page tests and the three smoke assertions pass in CI, and no unrelated file changed.
- Risk: MEDIUM
- Confidence: MEDIUM

## [ ] Step 3 — Final Stage V validation
- Files: none expected (validation only).
- Verify (record all evidence in `act.md`)
  - the final workflow run is green: build with 0 warnings, the full suite with 0 skipped, Bash and PowerShell log tests, and the Production smoke with the new availability assertions; run number recorded;
  - `git diff --stat <stage-start>..HEAD` shows no change under `src/MobileShop.Api`, authentication, entities, migrations or initialization code; only the planned service, interface, page, test and smoke-script files;
  - the Second-hand page and the Transactions pages are unchanged.
- Done when: all of the above pass and are recorded; the reviewer signs Stage V off (only the reviewer ticks `to-do.md`).
- Risk: LOW
- Confidence: HIGH

## Global Definition of Done
- The Products list can be filtered All / Available / Sold, live, together with type and part number; the badge and the filter use the same rule.
- No schema, entity, migration, Api, auth or Development-initialization change; clean build with 0 warnings, 0 skipped tests, whole suite and Production smoke green.

## Carry-over to later stages
- **Stage AA / AB:** new product types extend `GetInventoryRowsAsync` blocks; because the filter runs on the finished `rows`, they inherit the Availability filter as long as their rows carry `IsSold`.
- **Stage W:** Transactions sorting is independent of this stage.

## Execution notes
Work compact: chain dependent commands with `&&`, group read-only checks in one `{ ...; }` call, keep reports short, do not restate this plan in chat. One step → one commit → wait for a green run before merging → STOP for Job B. Do not run `dotnet build`/`dotnet test` locally; push and report `Action: #<run_number> — <Success|Failure|Pending>` per step. Never touch `to-do.md`, `plan.md` or `audit.md`; never amend; check each report against `git show <hash> --stat` before recording it.
