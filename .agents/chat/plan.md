# Plan — Stage 17 — Repair product edit loading, binding, and update integrity

## Reviewer Briefing
- **HIGH risk:** `ProductsDataService.UpdateProductAsync` mutates shared fields and clears Guarantee/SecondHand profiles before type-specific handling. Validate identity and all submitted data before mutating; preserve unrelated profile data.
- **HIGH risk:** Details must use one canonical product identifier. Never use an ambiguous `Id == id || ProductId == id` predicate; numeric IDs can overlap.
- **HIGH risk:** Unknown Details type values must not fall through to the Phone query. Resolve only explicit supported type keys and return no result for unknown types.
- **MEDIUM risk:** `EditProductInputModel.CompatibleModels` is currently `List<string>`; change it to `List<int>` of `Model.Id` values and validate the entire selection before changing fit rows.
- **MEDIUM risk:** Power Bank port rows and `PortCount` must be updated as one consistent collection; connector values need one enum representation.
- Scope: DAL-backed Web edit flow and focused tests only. Do not touch `src/MobileShop.Api/*`, API stubs/configuration, or `src/MobileShop.Dal/Initialization/sample-data.json`. Preserve plaintext Apple ID inventory passwords. CI is the build/test gate; do not run local `dotnet build/test` unless requested or CI is unavailable.

## Requirements and constraints
- Fix Apple ID save redirect to a blank Details page.
- Fix Case (`id=13`) and Glass (`id=10`) edit-page null-reference exceptions.
- Populate Cable connector choices and reject missing/invalid connectors server-side.
- Ensure unchanged Charger edits (`id=4`) do not mutate data.
- Restore Power Bank port editing, including connector options and Add/Remove controls.
- Check all other product edit types for shared mapping/update failures.
- Keep changes minimal and follow existing service, repository, Razor Pages, and test patterns.
- The query-string `type` is untrusted. Validate it against the loaded product's actual type and reject mismatches before mutation.

## ~~[x] Step 1 — Fix Case/Glass navigation loading and null-safe mapping~~
- Files: inspect `src/MobileShop.Dal/Repo/IBaseRepo.cs`, `src/MobileShop.Dal/Repo/BaseRepo.cs`, `src/MobileShop.Services/DataServices/Dal/ProductsDataService.cs`, `src/MobileShop.Web/Pages/Products/Edit.cshtml.cs`, and relevant tests; modify only the minimum required files/tests. Do not touch API files or sample data.
- Symbols: `ProductsDataService.EditIncludes`, `GetProductForEditAsync`, `EditModel.OnGetAsync`, `EditModel.MapToEditModel`, `IBaseRepo<T>.FindWithIncludesAsync`, `BaseRepo<T>.FindWithIncludesAsync`.
- Current -> Desired: Case/Glass mapping dereferences `f.ModelNavigation.Name`; missing nested navigation can crash GET. Load the required nested model navigation and map defensively.
- Change:
  1. Inspect the existing expression-based Include abstraction first. Use a supported `ThenInclude`/targeted query strategy; do not add generic repository overloads unless necessary.
  2. Load `CaseProfile.ModelFits.ModelNavigation` and `GlassProfile.ModelFits.ModelNavigation` consistently for no-tracking reads and tracked update reads.
  3. Null-safely project valid model fits and ignore missing navigation rows; preserve an empty selection when no valid fits remain.
  4. Do not present missing/deleted model relationships as valid selections.
- Depends on: none.
- Edge cases: no fits, missing model navigation, absent profile, both Case and Glass.
- Tests: repository/service coverage for nested loading and page/mapping coverage proving both pages render with empty/missing fit data.
- Verify: targeted CI tests; manual URLs `/Products/Edit?id=10&type=Glass` and `/Products/Edit?id=13&type=Case` render without exceptions.
- Done when: Case/Glass GETs do not throw and valid current compatibility selections are available.
- Risk: MEDIUM. Confidence: HIGH for the null-dereference diagnosis; MEDIUM for the smallest include change until the abstraction is inspected.

## ~~[x] Step 2 — Correct Apple ID Details lookup and post-save navigation
- Files: inspect `ProductsDataService.cs`, `Edit.cshtml.cs`, `Details.cshtml.cs`, product list/details links and their projections; modify only required service/page/tests. Do not touch API files.
- Symbols: `ProductsDataService.GetDetailsAsync`, type normalization/resolution helper, `EditModel.OnPostAsync`, `DetailsModel.OnGetAsync`, Details links/projections.
- Current -> Desired: Apple ID saves redirect to a Details page that does not resolve the intended record. Trace the actual identifier contract before choosing the fix; do not assume whitespace normalization or an ID mismatch without verifying callers.
- Change:
  1. Trace the identifiers emitted by index/details links, edit GET, and edit POST redirect. Establish whether each is the base `Product.Id` or a profile ID.
  2. Use one canonical `/Products/Details` identifier contract, preferably base `Product.Id` consistently. If legacy profile-ID routes must remain, resolve them explicitly by known type/route.
  3. Normalize type strings in one helper (trim whitespace and remove only supported separators), then resolve through an explicit allowlist/map of supported type keys. Preserve canonical display type for redirects.
  4. **Unknown, malformed, or unsupported types must return `null` from `GetDetailsAsync`** so the page returns `NotFound()` or follows an existing explicit error pattern. Never let a catch-all `else` query Phones.
  5. Query Apple ID details through the confirmed relationship and project the associated base product/profile. Preserve current Apple ID password storage/exposure behavior.
  6. Never use `entity.Id == id || entity.ProductId == id`; base-product and profile IDs can overlap and resolve the wrong record.
- Depends on: none.
- Edge cases: `Apple ID`, `appleid`, casing/spacing, missing product, unsupported type, overlapping numeric IDs, direct Details navigation and post-save redirect.
- Tests: canonical ID resolution; type normalization; Apple ID profile selection; missing product; invalid/unsupported type returns no result (never a Phone); deliberate base/profile ID collision proves the wrong record is not returned.
- Verify: edit an Apple ID, save a real change, and confirm the redirect renders that same product with updated values; verify index-to-details links.
- Done when: Apple ID saves reach the correct Details page and ID resolution is unambiguous.
- Risk: HIGH until contracts are traced; MEDIUM after canonical contract/collision test. Confidence: MEDIUM until all callers are inspected.

## [ ] Step 3 — Populate and validate Cable connector fields
- Files: inspect `_ProductEditForm.cshtml`, `Edit.cshtml.cs`, `ProductsDataService.cs`, `EditProductInputModel.cs`, Cable enum and create-form binding; modify only necessary files/tests. Do not touch API files.
- Symbols: `EditModel.OnPostAsync`, `PopulateDropdownsAsync`, `EditProductInputModel.Connector1`, `Connector2`, `ProductsDataService.UpdateProductAsync`.
- Current -> Desired: Cable selects contain only placeholders, and nullable connector values can be submitted. Both selects must show defined values and persisted selections; missing/undefined values must be rejected server-side.
- Change:
  1. Populate both selects using the existing enum-select-list convention for `CableConnector`, with field-level validation messages.
  2. Validate both connectors in the PageModel when the loaded product is Cable.
  3. Validate again in `UpdateProductAsync` before any mutation, including null and `Enum.IsDefined` checks. Never rely on client validation.
  4. Repopulate dropdowns when returning a validation error.
- Depends on: none.
- Edge cases: either connector missing, forged numeric enum, same connector on both ends if domain rules allow, malformed type.
- Tests: service tests for null/undefined values and valid updates; PageModel tests for field errors and dropdown repopulation.
- Verify: edit Cable (`id=15`), confirm stored values are selected and invalid direct POSTs are rejected.
- Done when: valid connector choices round-trip and invalid values never persist.
- Risk: HIGH. Confidence: MEDIUM pending inspection of current form conventions.

## [ ] Step 4 — Make updates type-safe and no-op safe
- Files: inspect `ProductsDataService.UpdateProductAsync`, `EditProductInputModel`, edit PageModel/form and profile entities; modify only necessary service/page/model/tests. Do not touch API files or seed data.
- Symbols: `ProductsDataService.UpdateProductAsync`, `EditModel.OnPostAsync`, `EditProductInputModel`, `SecondHandProfile`, `GuaranteeProfile`, pricing fields.
- Current -> Desired: The service loads a tracked product, recomputes price, mutates shared fields and clears SecondHand/Guarantee profiles when form fields are absent before switching on type. A no-op or partial accessory edit can lose unrelated data.
- Change:
  1. Load the base product by ID and establish actual type from persisted entity; reject missing product and forged/mismatched type before any mutation.
  2. Validate all common and type-specific input before changing tracked entities or saving.
  3. Preserve SecondHand/Guarantee profiles when those fields are not editable for the current product or are omitted. Clear/update a profile only on an explicit, valid user intent represented by the form contract.
  4. Do not treat `ProfitPercent = 0`/`ProfitAmount = 0` placeholders as authoritative if the form does not expose them. Preserve current price on unchanged submissions; recalculate only from valid, intentionally submitted pricing inputs.
  5. Normalize submitted/persisted values consistently and compare editable common and type-specific fields before writing. Do not rely on `DbContext.HasChanges()` alone after mapping has already mutated tracked entities.
  6. Return a distinguishable no-change result using the existing `ServiceResult` contract or a minimal explicit status; do not branch on message text.
  7. Client dirty tracking is optional UX only and comes after server-side protection; it must account for text, checkbox, select, multi-select and dynamic port rows.
  8. Keep success/no-change messages and redirects consistent with existing patterns.
- Depends on: shared identity behavior from Step 2; port behavior from Step 5 if dirty tracking is added.
- Edge cases: whitespace, nullable values, enums, equivalent normalized values, unexposed profiles, unchanged price, intentional price change, stale form, JavaScript disabled, invalid type.
- Tests: Charger no-op snapshots product and related profiles; accessory edits preserve unrelated Guarantee/SecondHand data; common/type-specific real changes persist; forged type rejected before mutation.
- Verify: submit Charger (`id=4`) unchanged, including a direct POST bypassing JavaScript, and compare full persisted state; then change one field and verify only intended values change.
- Done when: no-op submissions do not mutate fields/pricing/profiles, and unrelated profile data survives edits.
- Risk: HIGH. Confidence: MEDIUM; inspect pricing contract and form fields before implementation.

## [ ] Step 5 — Repair Power Bank port builder and accessory dropdown binding
- Files: inspect `_ProductEditForm.cshtml`, `Edit.cshtml.cs`, `Edit.cshtml`, `ProductsDataService.cs`, `EditProductInputModel.cs`, `PowerBank`/`PowerBankPort` entities and create-form JavaScript; modify only required files/tests; optional new `wwwroot/js/product-edit-ports.js`. Do not touch API files.
- Symbols: `EditModel.PopulateDropdownsAsync`, `EditProductInputModel.PortTypes`, `EditProductInputModel.CompatibleModels`, `PowerBankProfile.Ports`, `PowerBankPort.PortNumber`, `CableConnector`, `StorageKind`.
- Current -> Desired: Power Bank has no usable port options/Add control; connector string/numeric representations may not match. Portable Storage `StorageKind` and Case/Glass compatibility options are unpopulated.
- Change:
  1. Confirm `PowerBankPort` relationship and persistence semantics. Populate existing ports ordered by `PortNumber`; use one connector representation end-to-end (prefer numeric enum values) with explicit parsing/validation.
  2. Render each row as a select bound to `Product.PortTypes`, select the persisted connector, provide Add/Remove controls, renumber labels, and synchronize `PortCount` with the posted list.
  3. Define zero-port policy explicitly; prefer requiring at least one if consistent with create flow. Never silently accept count/list mismatch.
  4. Validate the complete port list before mutation. Synchronize additions, removals and ordering with persisted `PowerBankProfile.Ports`, assign sequential `PortNumber`, and update `PortCount` consistently; do not leave stale rows.
  5. Populate Portable Storage `StorageKind` from its enum using existing conventions and preserve the selected value.
  6. For Case/Glass, change `EditProductInputModel.CompatibleModels` from `List<string>` to **`List<int>`**, where every submitted value is the eligible phone model's `Model.Id` (not a name or string-encoded ID). Populate options and current selections using those same integer IDs. Before changing any fit rows, validate the complete selection: every ID must be positive, distinct, and belong to an existing eligible phone model. Reject the entire submission if any ID is invalid; do not partially apply valid IDs. Only then update fit rows.
  7. Reload required dropdowns after validation errors. If the compatibility work expands substantially, split it into a separately proposed stage rather than implementing a partial name-based solution.
- Depends on: Step 1 for current Case/Glass fits; Step 4 for safe update semantics.
- Edge cases: no/one/multiple ports, add/remove/reorder, duplicate connector types if allowed, invalid enum, count/list mismatch, removing all ports, invalid/nonexistent/ineligible/duplicate model IDs, missing enum/navigation data.
- Tests: Power Bank add/remove/reorder and validation; selected connector values and posted collection binding; StorageKind options; Case/Glass integer `Model.Id` binding; invalid/nonexistent/ineligible/duplicate compatible IDs; all-or-nothing validation.
- Verify: `/Products/Edit?id=14&type=Power%20Bank` shows persisted ports and connectors; Add/Remove then save/reopen preserves exact configuration. Portable Storage, Case and Glass options round-trip.
- Done when: controls are populated, valid selections persist, and port count/rows stay consistent.
- Risk: HIGH. Confidence: MEDIUM until persistence semantics and current form binding are inspected.

## [ ] Step 6 — Cross-type regression coverage and CI validation
- Files: inspect `src/MobileShop.Tests/**/*`, fixtures, `.github/workflows/dotnet.yml`; modify/create only necessary tests. Do not touch API files, sample data or unrelated workflows.
- Symbols: existing `ProductsDataServiceTests`, Products Edit/Details PageModel tests, repository tests and fixtures.
- Current -> Desired: all six reported symptoms and shared update behavior across other product types are covered by tests asserting response behavior and persisted state.
- Change:
  1. Add focused tests for each step using existing test conventions; verify test paths/classes before creating new ones.
  2. Add a regression matrix for Phone, Apple ID, Tablet, Smart Watch, Laptop, Cable, Charger, Power Bank, Portable Storage, Case and Glass. Test GET mapping for each; test no-op and real updates where applicable.
  3. Cover type/ID mismatches, unknown Details type, missing records, null navigations, invalid enums, malformed collections, ID collisions and profile preservation.
  4. Use CI as the validation gate. Do not run local build/test unless requested or CI is unavailable.
- Depends on: Steps 1–5.
- Edge cases: fixtures must create required profiles/related models and must not depend on accidental numeric ID alignment.
- Tests: full existing suite plus focused service, repository and PageModel tests.
- Verify: report relevant GitHub Actions run numbers and statuses; if no run is observable, report Pending/unavailable, never assume success.
- Done when: CI build/tests pass and no reported defect or known cross-type regression remains.
- Risk: MEDIUM. Confidence: HIGH for coverage goals; MEDIUM until CI evidence is observed.

## Global Definition of Done
- Apple ID edit redirects to the correct Details page for the same base product; identifier resolution is unambiguous.
- Case (`id=13`) and Glass (`id=10`) edit pages render without null-reference exceptions and show valid compatibility selections.
- Cable (`id=15`) shows persisted connectors and rejects missing/invalid values server-side.
- Charger (`id=4`) no-op submissions preserve product fields, price, Guarantee, SecondHand and related data.
- Power Bank (`id=14`) port builder works; port additions/removals persist and `PortCount` matches the collection.
- Portable Storage `StorageKind` and Case/Glass compatible-model options are populated and round-trip with validated `Model.Id` values.
- Other product types have regression coverage for shared mapping/update behavior.
- No unrelated refactors, API changes, seed-data changes or new dependencies.
- CI build/test results are reported with visible run numbers; no unsupported claim of success.
- Active-stage plan. Actor implements one approved step per commit; Reviewer Job B verifies each completed step.

## Handoff
- Next role: Reviewer (Job B) to verify the Actor's completed Step 1 against the implementation and CI evidence.
