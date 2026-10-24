# Plan — Stage 21 — Profit distribution from per-employee DB shares, editable in a Reports modal

## Requirements (user-confirmed)
- Remove the `Distribution` section from **all four** appsettings files (Web and Api, normal and Development). The user explicitly approved touching `src/MobileShop.Api` config for this.
- Each employee's share lives in the database: keep the existing column `Employee.SharePercent` (no rename, **no migration, no schema change**).
- The shop share is never stored: `Shop % = 100 − sum(active employees' SharePercent)`. No max setting exists anywhere.
- Calculation, validation and exceptions live in code and the database, never in appsettings.
- Edit UI: a compact modal on `Reports/ProfitLoss`, **Distribution tab**, opened by a button `Edit shares`. Edits existing **active** employees' percent only. Adding employees, an Active toggle, renames: OUT OF SCOPE.
- The modal must be self-explanatory and update **live** (before saving): see Step 4.
- Must not change: DB schema, entities, migrations, sample data, `UseApi` behaviour, password handling.

## Facts found in the code (verified)
- `Employee.SharePercent` exists (`[Range(0,100)]`, int). Sample data: active Mikaeeil 40 + Anis 50 (sum 90, shop 10); four inactive rows hold 50/30/20/0. `SampleDataSeedTests` asserts the active sum is 90 and stays valid.
- `DistributionCalculator.Calculate(long, IEnumerable<Employee>, DistributionSettings)` ignores the settings and the stored percent: it matches the exact names "Mikaeeil Jorjany" / "Anis Sahabi" and hardcodes 40/50/10. Both `DistributionSettings` properties are unused, and `DistributionConfiguration.ConfigureDistribution` has no caller. `DistributionCalculator` and `DistributionRow` live in `src/MobileShop.Services/Logging/Settings/DistributionSettings.cs`.
- `ReportsDataService(IBaseRepo<Transaction>, IBaseRepo<Employee>, IOptions<DistributionSettings>, ILogger<...>)` projects active employees into fake `Employee` objects (no `SharePercent` copied) and calls the calculator at the end of `GetDistributionRowsAsync(long)`.
- DI: `ServiceCollectionExtensions.AddMobileShopDistribution` (called at line ~30) only registers the settings.
- `IReportsDataService` is implemented by `ReportsDataService` and the stub `ApiReportsDataService` (all methods throw `NotImplementedException`). A new interface method forces the stub to change: that single stub edit is **explicitly in scope**.
- `ProfitLossModel` has `OnGetAsync` only; filter fields `Mode/Preset/From/To` are `[BindProperty(SupportsGet = true)]`. The Distribution tab is `#distribution` / `#distribution-tab` (Bootstrap tabs). Existing modals use plain Bootstrap markup (see `Pages/Products/CreatePhone.cshtml`); JS lives in `wwwroot/js/*.js`.
- `BaseEntity` carries a `[Timestamp] TimeStamp` concurrency token and `IsDeleted` (global query filter). `IBaseRepo<T>` offers `FindAllAsync(predicate)`, `UpdateRangeAsync(entities, persist)`, `SaveChangesAsync`.
- `MoneyLimits.MaxRials` is `long.MaxValue`, so `totalProfit * percent` can overflow `long`; today's `* 40L / 100` already has that latent bug and the new ×100 makes it worse.
- Date filters (`From`/`To`) are Jalali text (`Model.From?.ToJalaliDateString()`) parsed by the global `JalaliDateTimeModelBinder` (registered in `WebApplicationBuilderExtensions`), on GET and POST alike.
- `BaseEntity.TimeStamp` has no configuration in `Dal` and SQLite generates no value, so it is not an effective concurrency check; no service catches `DbUpdateConcurrencyException`.
- Success messages: `Products/Details.cshtml.cs` declares `[TempData] public string? SuccessMessage`; the page renders `<partial name="_ProductCreateSuccess" model="Model.SuccessMessage" />` (a generic `alert-success` in `Pages/Shared`).
- `ToToman(this long)` is in `src/MobileShop.Models/Extensions/MoneyExtensions.cs`; `TotalProfit` and `DistributionRow.CalculatedAmount` are in rials.

## Reviewer Briefing
- Step 1 is MEDIUM risk (and fixes a latent `long` overflow in the amount maths): it deletes config + a DI registration across 4 appsettings files, the README and rewrites the calculator tests. In both `appsettings.Development.json` files `Distribution` is the **last key**, so the preceding comma must go too.
- Step 2 is MEDIUM risk: first write path on `Employees`; the ≤100 invariant matters (no effective concurrency token exists).
- Step 3 is MEDIUM risk: first POST handler on `ProfitLoss` (a GET-bound page); the filter dates are Jalali strings (global binder) and must round-trip as strings through the hidden fields and the redirect; a validation error must re-render the page without losing the user's typed values.
- Step 4 is MEDIUM risk / MEDIUM confidence: live JS preview must reproduce the server's floor rounding exactly; only manual rendering proves the modal layout on a phone width.
- Production data: no migration is needed, but live `Employees.SharePercent` values may differ from 40/50, so the report numbers can change after deploy. Recorded as a manual check in the Global Definition of Done.
- Assumptions below are isolated and cheap to change.

## Assumptions (labelled, not facts)
- A: Only **active** employees take part (as today). A row for an inactive employee never appears in the report or the modal.
- B: The shop row's percent is `100 − sum`. If the stored active sum is already above 100 (legacy data), the calculator throws `InvalidOperationException` and the page shows an inline message while the `Edit shares` button still works, so the user can repair it.
- C: Employee amounts keep today's rounding (floor, `long`), computed overflow-safe as `totalProfit / 100 * percent + totalProfit % 100 * percent / 100` (exactly equal to floor(profit × percent / 100) for profit > 0); the shop gets the remainder. Loss or zero profit: employees get 0, the shop absorbs `totalProfit`, percentages still display.
- D: Rows order stays `SharePercent` descending, then name. Employees with 0 % still show a row.

## ~~[x] Step 1 — Calculator driven by DB shares; remove the Distribution config~~
- Files: inspect: `src/MobileShop.Services/Logging/Settings/DistributionSettings.cs`, `.../Configuration/DistributionConfiguration.cs`, `src/MobileShop.Services/ServiceCollectionExtensions.cs`, `src/MobileShop.Services/DataServices/Dal/ReportsDataService.cs`, `src/MobileShop.Models/Entities/Employee.cs`, `src/MobileShop.Models/Extensions/MoneyExtensions.cs` + `MoneyLimits`; modify: the first four files above, `src/MobileShop.Web/appsettings.json`, `src/MobileShop.Web/appsettings.Development.json`, `src/MobileShop.Api/appsettings.json`, `src/MobileShop.Api/appsettings.Development.json`, `README.md`, `src/MobileShop.Tests/Services/Logging/DistributionCalculatorTests.cs`, `src/MobileShop.Tests/Services/DataServices/Dal/ReportsDataServiceTests.cs`; delete: `DistributionConfiguration.cs`; rename (git mv, same folder and namespace): `DistributionSettings.cs` → `DistributionCalculator.cs`; do not touch: entities, sample data, `Web` pages, `Api` code other than the two JSON files.
- Symbols: `DistributionCalculator.Calculate(long totalProfit, IEnumerable<Employee> employees)` (the `settings` parameter removed); `DistributionSettings` (delete class); `DistributionConfiguration.ConfigureDistribution` (delete); `ServiceCollectionExtensions.AddMobileShopDistribution` and its call in `AddMobileShop` (delete); `ReportsDataService` primary constructor (drop `IOptions<DistributionSettings>`); `ReportsDataService.GetDistributionRowsAsync(long)`.
- Current -> Desired: hardcoded names and 40/50/10 → rows built from each **active** employee's `SharePercent` (by `Id`, name = `"{FirstName} {LastName}"`), plus one shop row (`EmployeeId = 0`, name `"Shop"`, percent `100 − sum`).
- Change:
  1. Calculator: keep only active employees; `sum = Σ SharePercent`; if `sum > 100` throw `InvalidOperationException("Active employee shares total {sum}%, which exceeds 100%.")`. Any active percent outside 0–100 also throws `InvalidOperationException`. Profit > 0: each employee amount via the overflow-safe formula of Assumption C (never `totalProfit * percent`); shop = remainder (`totalProfit − Σ employee amounts`). Loss/zero: employees 0, shop = `totalProfit`, `IsLossPeriod = true`. Keep the existing ordering. Remove the duplicate-name exception. Keep `DistributionRow` unchanged.
  2. `ReportsDataService`: project `employee.SharePercent` too and copy it into the temporary `Employee`; pass no settings. Remove the options parameter and its `using` if it becomes unused.
  3. Delete the `Distribution` object from the four JSON files (valid JSON afterwards: in both Development files remove the comma after `AppLogging`'s closing brace; in the two normal files the section sits before `AllowedHosts`, so no comma problem). Remove the `Distribution` row from the README configuration table.
- Depends on: none.
- Edge cases / error handling: no active employees → one shop row at 100 %; zero-percent employee still listed; `totalProfit = long.MaxValue` must not overflow.
- Tests: rewrite `DistributionCalculatorTests` for the new signature and the new rules: shares 40/50 → 40/50/10 split with floor rounding and remainder to shop; shares 30/20 → shop 50 %; sum exactly 100 → shop row 0 % / 0 amount; sum 101 → throws; a percent of −1 or 101 → throws; `long.MaxValue` profit with 40 % does not throw and equals the exact floor (compare with `System.Numerics.BigInteger`); no active employees → single shop row; inactive employee ignored; loss period → employees 0, shop = loss; ordering. Update `ReportsDataServiceTests` (constructor without options; the five existing `GetDistributionRowsAsync_*` tests (~lines 400–480) now seed employees with explicit `SharePercent` and assert by those values; remove the "missing/unlinked employee" expectations that no longer apply).
- Verify: CI green; `grep -rn "DistributionSettings\|ConfigureDistribution\|\"Distribution\"" src README.md` returns nothing; the four JSON files still parse.
- Done when: all distribution amounts come from `Employee.SharePercent` and no `Distribution` config remains anywhere.
- Risk: MEDIUM
- Confidence: HIGH

## ~~[x] Step 2 — Service: read and update employee shares~~
- Files: inspect: `src/MobileShop.Services/DataServices/Interfaces/IReportsDataService.cs`, `Dal/ReportsDataService.cs`, `src/MobileShop.Dal/Repo/BaseRepo.cs` (tracking behaviour of `FindAllAsync`/`UpdateRangeAsync`), `src/MobileShop.Models/ViewModels/Web/` (placement convention); modify: `IReportsDataService.cs`, `ReportsDataService.cs`, `src/MobileShop.Services/DataServices/Api/ApiReportsDataService.cs` (stubs only: both new methods throw `NotImplementedException`, same message as its siblings); create: `EmployeeShareViewModel.cs`, `EmployeeShareInput.cs`, `EmployeeSharesResult.cs` in the Models view-model folder found above, tests in `ReportsDataServiceTests.cs`; do not touch: other Api stubs.
- Symbols: `IReportsDataService.GetActiveEmployeeSharesAsync()` → `Task<IReadOnlyList<EmployeeShareViewModel>>`; `IReportsDataService.UpdateEmployeeSharesAsync(IReadOnlyList<EmployeeShareInput>)` → `Task<EmployeeSharesResult>`; `EmployeeShareViewModel(int EmployeeId, string Name, int SharePercent)`; `EmployeeShareInput { int EmployeeId; int SharePercent }` (class with `[Range(0, 100)]` on the percent); `EmployeeSharesResult(bool Success, string? Error)` with static `Ok` and `Fail(string)` helpers.
- Current -> Desired: no way to change shares. Desired: `Get…` returns active employees ordered like the report (percent desc, name); `Update…` validates then saves everything in one `SaveChanges`.
- Change: `Update…` loads the active employees once; rejects (returns `Fail`, mutates nothing) when: the list is empty; an `EmployeeId` appears twice; an id is not an active employee; any percent is outside 0–100; the resulting total over **all active employees** (posted value, else stored value) exceeds 100 — message `"Total share is {sum}%; it cannot exceed 100%."`. Otherwise set `SharePercent`, call `UpdateRangeAsync` once, log `Information` with the count and resulting total. Load with `FindAllAsync(e => e.IsActive)` (it is `AsNoTracking`), mutate `SharePercent`, then one `UpdateRangeAsync(entities)` (it writes the whole row — acceptable for this single-user local app). No concurrency-exception handling: the token is not effective here (see Facts).
- Depends on: Step 1.
- Edge cases / error handling: unchanged values still succeed (no-op); a total of exactly 100 is allowed; never throws for validation problems (only returns `Fail`).
- Tests: valid update persists values; total 100 allowed; total 101 rejected and DB unchanged; unknown/inactive id rejected; duplicate id rejected; out-of-range percent rejected; posting only a subset checks the total against the stored values of the others; `GetActiveEmployeeSharesAsync` excludes inactive and soft-deleted rows and orders correctly; the report reflects an update immediately.
- Verify: CI green; diff shows only the interface, the service, the stub, the three small types and tests.
- Done when: both methods behave as above under test.
- Risk: MEDIUM
- Confidence: HIGH

## [ ] Step 3 — ProfitLoss page handler
- Files: inspect: `src/MobileShop.Web/Pages/Reports/ProfitLoss.cshtml.cs`, `src/MobileShop.Tests/Web/Pages/Reports/ProfitLossTests.cs`, how the filter form posts `From`/`To` (field names and date format in `ProfitLoss.cshtml` lines ~91–168) `Products/Details.cshtml.cs` (the `[TempData] SuccessMessage` pattern) and `Infrastructure/JalaliDateTimeModelBinder.cs`; modify: `ProfitLoss.cshtml.cs`, `ProfitLossTests.cs`; do not touch: the report queries and chart code.
- Symbols: `ProfitLossModel.OnPostSaveSharesAsync()`; new properties `[BindProperty] List<EmployeeShareInput> Shares`, `[TempData] string? SuccessMessage`, `IReadOnlyList<EmployeeShareViewModel> EditableShares`, `string? SharesError`, `string? DistributionError`; private `LoadAsync()` extracted from `OnGetAsync` (same body, no behaviour change).
- Current -> Desired: GET only. Desired: `OnGetAsync` = `ResolveBoundsAsync` + `LoadAsync`. `LoadAsync` loads rows/trend/total as today, `EditableShares` from the service, and distribution rows inside a `try`; an `InvalidOperationException` from `GetDistributionRowsAsync` sets `DistributionError` and leaves `DistributionRows` empty (Edit button must still work).
- Change: `OnPostSaveSharesAsync` → call `UpdateEmployeeSharesAsync(Shares)`. Success: `SuccessMessage = "Shares saved."` and `RedirectToPage(routeValues: new { Mode, Preset, From = From?.ToJalaliDateString(), To = To?.ToJalaliDateString() }, fragment: "distribution")` — the date values are **Jalali strings**, never `DateTime` (a `DateTime` route value would be formatted with the wrong culture and not parse back). Failure: set `SharesError`, run `ResolveBoundsAsync` + `LoadAsync`, then overwrite the displayed editable values with the user's posted values and `return Page()`. Hidden fields in Step 4 carry `Mode/Preset/From/To` so the date range survives.
- Depends on: Step 2.
- Edge cases / error handling: `Shares` null or empty → treated as failure with a generic message; a null `From`/`To` is omitted from the redirect.
- Tests: success redirects to the same page with `Mode`, `Preset`, Jalali `From`/`To` and the `distribution` fragment and sets `SuccessMessage`; failure returns `PageResult`, sets `SharesError`, keeps posted values; `OnGetAsync` populates `EditableShares`; the calculator `InvalidOperationException` sets `DistributionError` and does not throw; existing `ProfitLossTests` still pass (add the new service mock setups to `CreateModel`).
- Verify: CI green.
- Done when: the handler works under test and `OnGetAsync` behaviour is unchanged.
- Risk: MEDIUM
- Confidence: HIGH

## [ ] Step 4 — Modal markup and live preview
- Files: inspect: `src/MobileShop.Web/Pages/Reports/ProfitLoss.cshtml` (Distribution tab, lines ~214–232), `Pages/Products/CreatePhone.cshtml` modal markup, `wwwroot/js/profit-loss-chart.js` and `site.js` (script style, how tabs/hash are handled), the existing CSS tokens used by modals and `.stat`; `MoneyExtensions.ToToman` (output format to mirror); create: `src/MobileShop.Web/wwwroot/js/distribution-shares.js`; modify: `ProfitLoss.cshtml` (+ the shared stylesheet only if a few lines are unavoidable); do not touch: chart script, other pages.
- Symbols: `#editSharesModal`, button `Edit shares` (above the distribution table, always rendered when `EditableShares.Count > 0`), form `asp-page-handler="SaveShares" method="post"` with hidden `Mode/Preset/From/To` (`From`/`To` rendered with `Model.From?.ToJalaliDateString()`, exactly like the filter inputs), one row per employee bound as `Shares[i].EmployeeId` (hidden) / `Shares[i].SharePercent` (`type=number min=0 max=100 step=1 inputmode=numeric`).
- Current -> Desired: read-only table. Desired modal (compact, one column on a phone), per row: name · `% input` · **"was → now" amount in Toman**; then a read-only **Shop** row with the same columns, then a summary line `Total X% · Shop gets Y%` in `role="status" aria-live="polite"`.
- Live behaviour (`distribution-shares.js`, vanilla JS, no new dependency; data attributes carry `data-total-profit` in rials, each row's original percent and original amount):
  - on every `input`: recompute each employee's amount as `floor(profit × percent / 100)` using **BigInt**, shop = `profit − Σ` (loss/zero profit: employees 0, shop = profit); recompute `Shop % = 100 − Σ percent`;
  - rows whose percent changed get a "changed" style and show `was 40% → 45%` plus the amount delta (`+`/`−`, Toman, grouped like `ToToman`); unchanged rows stay quiet; the shop row always shows its before → after;
  - total over 100: the summary turns into an error (`Over by N%`), invalid inputs get `aria-invalid`, and **Save changes is disabled**; empty or non-integer input counts as invalid;
  - a `Reset` text button restores the original values; the primary button is `Save changes`; opening the modal always starts from the server values;
  - if the profit is ≤ 0, show one line: `No profit in this range — amounts are 0, percentages still apply.`
- Server error path: when `SharesError` is set the page renders the modal already open (small inline script after the Bootstrap bundle: `bootstrap.Modal.getOrCreateInstance(...).show()`) with the alert text inside; when `DistributionError` is set the Distribution tab shows an alert instead of the table. Also activate `#distribution-tab` when `location.hash === "#distribution"` (needed after the redirect). Render `<partial name="_ProductCreateSuccess" model="Model.SuccessMessage" />` near the top of the page so the saved message appears after the redirect. When `EditableShares` is empty, hide the button and show a muted line `No active employees.`
- Depends on: Step 3.
- Edge cases / error handling: more than ~8 employees → the modal body scrolls inside its own container; RTL/LTR names use `dir="auto"`; reduced-motion and contrast tokens from Stage 15 are inherited, add no animation.
- Tests: add a page-level test that the rendered Distribution tab contains the `Edit shares` button and one input per active employee (follow the existing rendering-test style in `ProfitLossTests`, or in a web test if one exists); the JS has no test harness, so list the manual checks below in `act.md`.
- Verify: CI green. Manual: open Reports → Distribution; change one percent (other rows and the shop row update live, amounts show before → after); push the total above 100 (error shown, Save disabled); Reset; save a valid change (redirect keeps the date range and the Distribution tab, success message, table updated); also at ~390 px width and in dark mode.
- Done when: the modal behaves as described and CI is green; then run the full validation below and record it.
- Risk: MEDIUM
- Confidence: MEDIUM

## Global Definition of Done
- No `Distribution` config or `DistributionSettings` anywhere (`src` and README); `MobileShop.Api` changed only in its two JSON files and the one stub.
- The distribution report uses `Employee.SharePercent` of active employees by id; the shop share is `100 − sum`; no hardcoded names or percentages remain.
- Active shares can never be saved above 100 %; legacy bad data shows an inline message and stays repairable from the modal.
- No schema change, no migration, no sample-data change; `SampleDataSeedTests` unchanged and green.
- CI green (build + test), with the run number cited by the Reviewer.
- Manual deployment note for the user: before using this on the live database, check the live `Employees.SharePercent` values of active employees; report numbers follow them from now on.
