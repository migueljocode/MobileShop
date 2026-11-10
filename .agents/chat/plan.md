# Plan — Keep glass Model label in sync when compatible phones change on edit

## Proposed stages
- [ ] Stage 7 — Glass Model label follows compatible-phone edits

## Reviewer Briefing
- Single focused bug: after Edit removes (or adds) a glass compatible phone, fits persist correctly but Identity **Model** stays the old composite name (e.g. still `iPhone 11 / iPhone 14 Glass` when only `iPhone 11` remains).
- Root cause is in `ProductsDataService.UpdateProductAsync` glass branch: it calls `SynchronizeGlassModelFits` only. Create path already rebuilds the catalog name via `GlassModelNaming.Build` and find-or-creates a `Model` row; update never does.
- Do **not** rename `Model.Name` in place: create shares one `Model` across glass units with the same manufacturer + name; in-place rename would corrupt sibling products. Always find-or-create by (ManufacturerId, Glass category, new name) and re-point `product.ModelId` / `ModelNavigation`.
- `input.ModelId` is rejected when it differs from the stored id (“cannot be changed from this form”); the service must change `ModelId` server-side, not via the form field.
- `CaptureEditableState` already includes `product.ModelId` and glass fit ModelIds, so re-pointing ModelId is enough for change detection.
- Out of scope: Case products, Create path (already correct), Api, auth, DB init, UI chrome, Stage 7 UI polish items from the prior plan unless this step necessarily touches the same files.

## ~~[x] Step 1 — On glass update, rebuild catalog Model name from remaining fits~~
- Files: inspect: `src/MobileShop.Services/DataServices/Shared/GlassModelNaming.cs`, create path in `ProductsDataService` (~719–740) that builds `glassModelName` and find-or-creates `Model`, glass `case "Glass":` in `UpdateProductAsync` (~1239–1244), `EditIncludes`, `CaptureEditableState` Glass arm; modify: `src/MobileShop.Services/DataServices/Dal/ProductsDataService.cs`; tests: `src/MobileShop.Tests/Web/Pages/Products/EditModelTests.cs` (or a dedicated service test next to existing update tests); do not touch: `src/MobileShop.Api`, Razor pages unless a binding bug is proven.
- Symbols: `UpdateProductAsync`; `SynchronizeGlassModelFits`; `GlassModelNaming.Build`; `models` (`IBaseRepo<Model>`); `categories` (Glass category); `product.ModelId` / `product.ModelNavigation`.
- Current -> Desired: after a successful glass edit that changes the set of non-deleted compatible phone models, the product’s catalog model name equals `GlassModelNaming.Build` of the remaining phone model names (same rules as create: sorted, distinct, ` / ` separator, ` Glass` suffix, length limits). List/Details/Edit Identity **Model** field show the new label after reload. Fits-only changes with no name change leave behavior unchanged.
- Change: In the `case "Glass":` branch of `UpdateProductAsync`, after notes + `SynchronizeGlassModelFits` (when `selectedCompatibleModels is not null`):
  1. Resolve the effective fit list: prefer `selectedCompatibleModels` when non-null; otherwise the profile’s non-deleted `ModelFits` with `ModelNavigation` (must have at least one phone model — if zero after sync, return the same validation failure create uses for empty compatible sets, or match existing update validation if it already rejects empty).
  2. `var newName = GlassModelNaming.Build(fitModels.Select(m => m.Name).ToList());`
  3. If `product.ModelNavigation.Name` already equals `newName` (ordinal), skip model re-point.
  4. Else: `glassCategory` = category named `"Glass"` (same lookup as create); `manufacturerId` = `product.ModelNavigation.ManufacturerId` (glass brand, e.g. Anker — do not use phone manufacturers).
  5. `FindAsync` Model where ManufacturerId + CategoryId + Name == newName; if missing, `AddAsync` a new `Model` with those fields (`persist: false` if that matches create).
  6. Assign `product.ModelId = glassModel.Id` and `product.ModelNavigation = glassModel` so the tracked graph and `CaptureEditableState` see the change.
  7. Do not delete the previous Model row (may still be referenced by other glass products).
  Keep the existing guard that rejects client-submitted `input.ModelId` changes; only this server path may change ModelId for glass.
- Depends on: none.
- Edge cases / error handling:
  - Remove one of two phones → name shrinks to single `… Glass`.
  - Add a phone → name gains the new segment (sorted).
  - Fits unchanged → no ModelId churn; “No changes were made” still works.
  - Two glass products shared the old Model; only the edited product moves to the new Model; the other keeps the old name until edited.
  - Missing Glass category seed → same error style as create.
  - Empty compatible list after edit → reject with a clear field error on `CompatibleModels` (do not call `GlassModelNaming.Build` with zero names).
- Tests: add to `EditModelTests` (or service tests):
  1. Create/setup a glass product whose Model.Name is built from two phone models (e.g. iPhone 11 + iPhone 14); `UpdateProductAsync` with only one CompatibleModels id → result succeeded; reloaded `product.ModelNavigation.Name` equals `GlassModelNaming.Build` of the remaining name; `GlassProfile` has one non-deleted fit.
  2. Update with the same two ids → ModelId/Name unchanged (or still equal to the two-name build).
  3. Optional: second product still pointing at the old shared Model keeps the old name after the first product is retargeted.
- Verify: after push, CI green (`Action: #<run_number> — Success`). Manual: Edit Anker glass that shows `iPhone 11 / iPhone 14 Glass`, remove iPhone 14, Save → Identity Model (and list/details title) show `iPhone 11 Glass` (or exact `GlassModelNaming` output); chip list matches.
- Done when: tests pass; Model label follows fit set on edit; no in-place rename of shared Model rows.
- Risk: MEDIUM (Model identity shared across units; wrong manufacturer/category would orphan or mis-label)
- Confidence: HIGH

## [ ] Step 2 — Final validation
- Files: inspect only; `graphify update .` if code changed and graph is part of the commit policy for this repo.
- Symbols: none.
- Current -> Desired: bug closed; CI green.
- Change: confirm test + manual checks from Step 1; record Action run number in act.md when Job B runs.
- Depends on: Step 1.
- Edge cases: report any leftover Stage 7 UI items as out of scope for this stage.
- Tests: full solution suite via CI.
- Verify: `Action: #<run_number> — Success`.
- Done when: Global Definition of Done met.
- Risk: LOW
- Confidence: HIGH

## Global Definition of Done
- Editing a glass product’s compatible phones updates the product’s catalog `Model.Name` via `GlassModelNaming.Build` and find-or-create (same as create), so Identity **Model**, list, and details titles match the remaining phones.
- Shared old Model rows are left intact for other products; client cannot change ModelId via the form; empty fits rejected.
- Tests cover remove-one-phone rename; CI green; only the files named above changed (plus graphify outputs if updated).

## Execution notes
One step → one commit → STOP for Job B. Actor must not edit plan/audit/TODO. After Reviewer APPROVES, Planner adds Stage 7 to `.agents/TODO.md` unchecked; Actor starts Step 1 only.
