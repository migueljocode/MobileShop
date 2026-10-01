# Act — Stage M Step 4 — PartNumber and SIM options on phone details

## Commit
- This commit — feat(products): show part number and SIM options on phone details
  - Implements Step 4 of plan.md.

## What changed
- `ProductDetailsViewModel.cs` — three init-only string properties with `"N/A"` defaults: `PartNumberLabel`, `DualSimLabel`, `EsimLabel`. Defaults keep the phone-only fields neutral for Apple IDs and for phones without a part number.
- `ProductsDataService.GetDetailsAsync` — the **phone** projection now sets the three labels from the existing `Phone.PartNumberNavigation`: `N/A` when the navigation is null, otherwise the `Code` and `Yes`/`No` for `SupportsDualSim`/`SupportsEsim`. The **Apple ID** branch is unchanged (it keeps the defaults).
- `Products/Details.cshtml` — the Part number / Dual SIM / eSIM rows render only when `Type == "Phone"`, so Apple ID details are byte-for-byte unchanged.
- `ProductsDataServiceTests.cs` — 5 new `GetDetailsAsync` tests (plus two small local seeding helpers) covering the required cases.

## Verification
- `dotnet build src/MobileShop.slnx --nologo` → **0 warnings, 0 errors**
- Focused `dotnet test ... --filter "FullyQualifiedName~GetDetailsAsync"` → **10 passed, 0 failed, 0 skipped**
- Full suite `dotnet test src/MobileShop.slnx --nologo --no-build` → **272 passed, 0 failed, 0 skipped**
- Null-part-number case explicitly asserted: phone loads with `PartNumberId == null` and shows `N/A` for all three.
- Apple ID details explicitly asserted unchanged (Type/Identifier unchanged; the phone-only fields keep `N/A`).
- No API, authentication, PDF, or database-initialization-policy changes: `git status` shows only the four files above.

## Required test coverage (map)
1. PartNumber code displayed → `GetDetailsAsync_phone_with_part_number_shows_code_and_capabilities`
2. `SupportsDualSim` displayed → same test (`"Yes"`) and the both-true test
3. `SupportsEsim` displayed → same test (`"No"`) and the both-true test
4. `PartNumberId == null` loads → `GetDetailsAsync_phone_without_part_number_loads_and_shows_na`
5. Null shows `N/A` → same test (all three labels)
6. Existing PartNumber with false capabilities ≠ missing → `GetDetailsAsync_phone_with_false_capabilities_is_not_treated_as_missing_part_number`
7. Apple ID details unchanged → `GetDetailsAsync_apple_id_details_remain_unchanged_without_part_number_fields`

## Files changed
- `src/MobileShop.Models/ViewModels/Web/ProductDetailsViewModel.cs`
- `src/MobileShop.Services/DataServices/Dal/ProductsDataService.cs`
- `src/MobileShop.Tests/Services/DataServices/Dal/ProductsDataServiceTests.cs`
- `src/MobileShop.Web/Pages/Products/Details.cshtml`

## Limitations
None. Step 4 is display-only; assigning a part number during phone creation was deliberately not touched (that is Stage N, as plan.md states). I did **not** edit plan.md/audit.md/to-do.md, per plan.md §92 — the Reviewer owns those.

## Friction noted
- Build is now ~31s and the full suite ~35s, both beyond the 30s tool window; run in the background and polled.

## Problems
None — verification green. Reviewer Job B determines PASS/FAIL.

## Status
COMPLETE — awaiting Reviewer Job B for Step 4.

---

# Act — Stage M Step 3 — PartNumber filter on the Products list

## Commit
- Pending (this commit) — feat(products): add optional part-number filter to the products list
  - Implements Step 3 of plan.md.

## What changed
- `IProductsDataService.GetInventoryRowsAsync(string? type = null, int? partNumberId = null)` — new optional part-number filter. `GetPartNumbersAsync(int? modelId = null)` — `modelId` made optional so the filter dropdown can list every part number; called with a model id it still scopes to that model (Step 2 behaviour preserved).
- `ProductsDataService.GetInventoryRowsAsync` — a positive `partNumberId` applies `phone.PartNumberId == partNumberId` and excludes the Apple ID block entirely; zero/negative/null applies no filter (the predicate is only used when `hasPartNumberFilter`, so a stray value can never blank the phone list). Shared projection extracted to a single `Expression` to keep one `ProductListItemViewModel` shape.
- `ProductsDataService.GetPartNumbersAsync` — returns all part numbers when `modelId` is null, else the model's part numbers.
- `ApiProductsDataService` — both signatures synced; bodies remain `NotImplementedException` stubs (unchanged policy).
- `Products/Index.cshtml.cs` — `OnGetAsync(string? type, int? partNumberId)`; non-positive ids normalised to null; builds `PartNumberOptions` (`SelectListItem`, leading "All part numbers") via `GetPartNumbersAsync()`.
- `Products/Index.cshtml` — part-number `<select>` + Filter button in a GET form; the existing All/Phones/Apple IDs type links now carry `asp-route-partNumberId` so the type route and part-number filter compose.
- Tests — 5 new `ProductsDataServiceTests`: filter returns only matching phones; filter excludes Apple IDs; zero/negative behave as no filter; the `appleid` type route still excludes phones under a filter; `GetPartNumbersAsync()` returns every part number.

## Verification
- `dotnet build src/MobileShop.slnx --nologo` → 0 warnings, 0 errors
- Focused `dotnet test ... --filter "FullyQualifiedName~ProductsDataServiceTests"` → 47 passed, 0 failed, 0 skipped
- Full suite `dotnet test src/MobileShop.slnx --nologo --no-build` → **267 passed, 0 failed, 0 skipped**

## Limitations
None. The filter is applied at the data-service layer and surfaced on the list page; phone details display is Step 4 and was deliberately left untouched. `GetPartNumbersAsync` gained an optional parameter (additive; the Step 2 `GetPartNumbersAsync(modelId)` call sites and behaviour are unchanged).

## Friction noted
- Full suite (~36s) and even a single-project build can exceed the 30s tool window; both were run in the background and polled. The first non-positive-id test run caught a genuine implementation bug (predicate applied unconditionally), which is why the DAL now branches on `hasPartNumberFilter`.

## Problems
None — verification green.

## Status
COMPLETE — awaiting Reviewer Job B for Step 3.

---

# Act — Stage M Step 2 — Seed PartNumbers + list/create operations

## Commit
- `aa42599` — feat(products): seed part numbers and add list/create data-service operations
  - Implements Step 2 of plan.md.

## What changed
Resumed the in-progress Step 2 work and made it build + pass.

- `src/MobileShop.Dal/Initialization/sample-data.json` — added 4 `partNumbers` (ids 1–4) scoped to phone models 1, 3, 6, 16, each carrying `supportsDualSim`/`supportsEsim`; assigned `partNumberId` to the matching existing phone rows 1, 3, 4, 7 (phones 2, 5, 6 intentionally left null). Products/phones ids and all other relationships are unchanged.
- `SampleDataLoader.cs` — added `PartNumbers` to `SampleDataSet` and deserialized `"partNumbers"`.
- `SampleDataInitializer.cs` — seed `PartNumbers` after `Products`; `ClearData` FK-order fixed (**Phone now cleared before PartNumber**).
- `IProductsDataService.cs` / `ProductsDataService.cs` — new `IBaseRepo<PartNumber>` dependency (repo-count comment 8 → 9); `GetPartNumbersAsync(modelId)` returns a `DropdownOptionViewModel` per model; `CreatePartNumberAsync(modelId, code, supportsDualSim, supportsEsim)` validates/trims/de-dupes per model and returns `DropdownCreateResult` (400 blank code, 404 unknown model, 200 existing/new).
- `ApiProductsDataService.cs` — both new members stubbed as `NotImplementedException`, matching every other member of that class.
- Tests: 5 new `PartNumber` data-service tests in `ProductsDataServiceTests.cs`; `SampleDataSeedTests` expects 4 PartNumbers; the two page-model tests pass the new repo argument.

### Fixes made while resuming (all inside Step 2's scope)
1. **Compile errors (4)** — the WIP had not been built:
   - `CreatePhoneModelTests` / `CreateAppleIdModelTests` were missing the new `IBaseRepo<PartNumber>` argument.
   - `ProductsDataServiceTests` used `supportsDualSim = true` (assignment) instead of named-argument syntax `supportsDualSim: true`.
2. **Seeding FK failure** — `SampleDataLoader.Load()` had lost its `Models = Deserialize<Model>(root, "models")` line during the WIP edit, so models loaded as 0 and the `Products` insert failed its `ModelId` FK. Restored the line.
3. **ClearData FK failure** — `PartNumbers` were deleted before `Phones`, so the `Phones.PartNumberId` FK blocked the delete. Swapped to clear `Phone` first.

## Verification
- `dotnet build src/MobileShop.slnx --nologo` → 0 warnings, 0 errors
- Full suite `dotnet test src/MobileShop.slnx --nologo --no-build` → **262 passed, 0 failed, 0 skipped**

## Limitations
None. `GetPartNumbersAsync`/`CreatePartNumberAsync` are exposed through the data-service abstraction only; the Products-list filter and phone-details display are later Stage M steps and were deliberately not touched.

## Friction noted
- Full suite (~36–39s) exceeds the 30s tool window; ran it in background and polled. Isolating the seed FK failure needed a throwaway trace test because the seeder reports only "FOREIGN KEY constraint failed" without naming the entity — worth labelling `ProcessInsert` failures per entity in a future cleanup.

## Problems
None — all three resumption defects found and fixed, verification green.

## Status
COMPLETE — awaiting Reviewer Job B for Step 2.
