# Act — Stage M (reopened) Step 2 — Create Phone PartNumber selector + Add New

## Commit
- This commit — feat(products): add Model-scoped PartNumber selector and Add New to Create Phone
  - Implements Step 2 of the reopened Stage M plan.md.

## What changed
- `CreatePhoneInputModel.cs` — added nullable `PartNumberId` (omitted → null; additive, no schema change).
- `ProductsDataService.CreatePhoneAsync` — when `PartNumberId` is supplied it must exist and belong to the selected model, else `ServiceResult` fails on the `PartNumberId` field; the selected part number is persisted on the `Phone` (null when omitted).
- `CreatePhone.cshtml.cs` — new `PartNumbers` list, `OnGetPartNumbersAsync(modelId)` (model-scoped, empty for `modelId <= 0`), `OnPostCreatePartNumberAsync(modelId, code, supportsDualSim, supportsEsim)` (400 without a model; delegates to `CreatePartNumberAsync` so duplicate Model+Code reuses the existing row). `PopulateDropdownsAsync` loads model-scoped options for the posted model.
- `CreatePhone.cshtml` — Part number combobox next to Color with an **Add New** button; new `#addPartNumberModal` (code + Dual SIM + eSIM checkboxes); JS reloads part numbers on model change, and on Add New posts `handler=CreatePartNumber` then selects the returned option without a full reload.
- Tests — 3 service tests (`ProductsDataServiceTests`) + 5 page tests (`CreatePhoneModelTests`).

## Verification
- `dotnet build src/MobileShop.slnx --nologo` → **0 warnings, 0 errors**
- Focused `CreatePhoneModelTests|ProductsDataServiceTests` → **86 passed, 0 failed, 0 skipped**
- Full suite `dotnet test src/MobileShop.slnx --nologo --no-build` → **284 passed, 0 failed, 0 skipped**
- `git status` shows only the six files above — no API project/auth/PDF/migration edits; no schema change.

## Requirement map
1. Nullable `PartNumberId` on input → done.
2. Combobox + Add New (existing modal/AJAX pattern) → done.
3. Options Model-scoped via `GetPartNumbersAsync(modelId)` → done (`PopulateDropdownsAsync` + `OnGetPartNumbersAsync`).
4. Model change resets/reloads PartNumber options → `modelSelect` `change` listener.
5. Add New disabled without Model (client) / rejected server-side → JS bails when no model; handler returns 400 for `modelId <= 0`.
6. Duplicate Model+Code reuses existing → via existing `CreatePartNumberAsync`.
7. Persist selected PartNumberId; omit → null → done in service.
8. Reject PartNumber belonging to another model → `partNumber.ModelId != model.Id` fails.
9. No unrelated field behavior changes → only `PartNumberId` added.

## Limitations
None. The Stage N "shared client script for both pages" was not introduced — this step follows the existing per-page modal/AJAX pattern, as plan.md specifies the existing pattern.

## Friction noted
- Build ~31s / full suite ~37s exceed the 30s tool window; run in the background and polled.
- A first run of the new page test assumed camelCase JSON for the `PartNumbers` GET; the handler returns PascalCase (`o.Id`/`o.Name`) so I corrected the assertion to `Name`. POST handlers intentionally return lowercase (`id`/`name`) and their assertions were already correct.

## Problems
None — verification green. Reviewer Job B determines PASS/FAIL.

## Status
COMPLETE — awaiting Reviewer Job B for Step 2.

---

# Act — Stage M (reopened) Step 1 — Correct Products PartNumber filtering and inventory display

## Commit
- This commit — feat(products): phone-only inventory-derived part-number filter with list column
  - Implements Step 1 of the reopened Stage M plan.md.

## What changed
- `ProductListItemViewModel.cs` — added `PartNumberLabel` as an **init-only** property (default `"N/A"`), deliberately **not** a positional parameter, so every existing construction stays source-compatible (audit HIGH #1 satisfied without breaking callers).
- `ProductsDataService.GetInventoryRowsAsync` + `GetSecondHandRowsAsync` — phone projections set `PartNumberLabel` from `phone.PartNumberNavigation` (code or `"N/A"`). Apple ID projections keep the `"N/A"` default.
- `TransactionsDataService.GetSelectableProductsAsync` — the phone projection sets the same label; Apple IDs unchanged.
- `PeopleDataService` — untouched: that projection is over `Product` (no phone part number), so it correctly keeps the `"N/A"` default.
- `IProductsDataService` / `ProductsDataService` — new `GetInventoryPartNumbersAsync()` returning distinct part numbers **attached to live phone inventory** (unused catalog part numbers excluded, soft-delete filters inherited from the repos). Model-scoped `GetPartNumbersAsync(modelId)` is untouched and no longer used by the Index (audit MEDIUM).
- `ApiProductsDataService` — new member stubbed as `NotImplementedException`, matching the file's contract.
- `Products/Index.cshtml.cs` — builds options only when `Type == "phone"`; a stale positive id not in the option set is normalised to null; null/0/negative remain no selection.
- `Products/Index.cshtml` — filter form renders **only** for phones (audit HIGH #4); **Filter button removed**, GET form auto-submits via `onchange="this.form.submit()"` (audit HIGH #3); **All** and **Apple IDs** links no longer carry `partNumberId` (audit HIGH #2), Phones preserves a valid selection; new **Part number** column bound to `PartNumberLabel`.
- `ProductsDataServiceTests.cs` — 4 new tests.

## Verification
- `dotnet build src/MobileShop.slnx --nologo` → **0 warnings, 0 errors**
- Focused `dotnet test ... --filter "FullyQualifiedName~ProductsDataServiceTests"` → **56 passed, 0 failed, 0 skipped**
- Full suite `dotnet test src/MobileShop.slnx --nologo --no-build` → **276 passed, 0 failed, 0 skipped**
- `git status` shows only the eight files above — no API project/auth/PDF/migration edits; the Api *services* member is a stub consistent with its existing contract, not an API-project change.

## Plan requirement map
- #1 phone-only control → `Index.cshtml` `@if (Model.ShowPartNumberFilter)`
- #2 no Filter button → removed; auto-submit on change
- #3/#4 auto-apply, empty option clears → `onchange` submit; leading "All part numbers" value `""`
- #5 type links → All/Apple IDs drop `partNumberId`; Phones keeps it
- #6 inventory-derived options, filter omitted → `GetInventoryPartNumbersAsync()` (not filter-scoped)
- #7 stale id → normalised to null in `IndexModel`
- #8 null/0/negative → no selection
- #9 ViewModel field + all projections → `PartNumberLabel` set on every phone site; Apple IDs `"N/A"`
- #10 Part number column → added to the table

## Limitations
None. Create Phone PartNumber + Add New is Step 2 and was deliberately not touched. `PeopleDataService`'s generic `Product` projection was correctly left at the default because it has no phone part number to show (not a missed call site).

## Friction noted
- Build ~31s and full suite ~36s exceed the 30s tool window; both run in the background and polled.

## Problems
None — verification green. Reviewer Job B determines PASS/FAIL.

## Status
COMPLETE — awaiting Reviewer Job B for reopened Stage M Step 1.

---

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
