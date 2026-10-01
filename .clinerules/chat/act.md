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
