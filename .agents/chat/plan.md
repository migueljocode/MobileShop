# Plan — Glass UI polish: disabled Add button, list Identifier, Details fits, Edit fits

## Proposed stages
- [ ] Stage 7 — Glass UI polish (disabled Add accent, Identifier cell, Details compatible phones, Edit fits list)

## Reviewer Briefing
- Step 1 (CSS disabled primary): LOW risk but touches shared `controls.css`; wrong selector could change every disabled primary on the site. Must mirror the existing `.share-editor__save:disabled` accent mix so the disabled Add button is light teal/green, not Bootstrap blue.
- Step 2 (Identifier): MEDIUM confidence until the exact malformation is confirmed in markup/CSS. Strong hypothesis: `td.num` + `"Barcode: " + barcode` mixes a label with a hex string under numeric styling. Fix is scoped to glass list projection and/or not applying `.num` when Identifier is not pure numeric.
- Step 3 (Details): HIGH confidence bug — glass branch of `GetDetailsAsync` never sets `CompatibleModels` (or `Notes`), and `Details.cshtml` only renders Compatible models when `Type == "Case"`. Straightforward projection + markup.
- Step 4 (Edit): HIGH risk for scope creep. Create uses chip list + modal (`create-glass-fits.js`); Edit still uses a non-working multi `<select>`. Prefer reuse of create markup/JS patterns on Edit for Glass only; do not redesign Case in this stage unless required for shared partial safety.
- Do not touch `src/MobileShop.Api`, auth, DB init, or glass create/save business rules beyond display.

## [ ] Step 1 — Disabled primary Add button uses accent (light green/teal), not Bootstrap blue
- Files: inspect: `src/MobileShop.Web/wwwroot/css/controls.css`, `src/MobileShop.Web/wwwroot/css/overlays.css` (`.share-editor__save:disabled`); modify: `src/MobileShop.Web/wwwroot/css/controls.css`; do not touch: page markup for the modal button class (`btn btn-primary` stays).
- Symbols: `.btn-primary`, `.btn-success` block already sets `--bs-btn-bg: var(--accent)`; Bootstrap’s default disabled primary stays blue because `--bs-btn-disabled-*` is unset in controls.
- Current -> Desired: disabled `#addCompatiblePhoneBtn` (and any other `.btn-primary:disabled` / `.btn-success:disabled`) renders with light accent fill (same approach as `.share-editor__save:disabled`: `color-mix(in srgb, var(--accent) 45%, white)` for bg/border, dark readable ink), not light blue.
- Change: in the buttons section of `controls.css`, after the enabled `.btn-primary, .btn-success` rules, add disabled overrides for `.btn-primary:disabled`, `.btn-primary.disabled`, `.btn-success:disabled`, `.btn-success.disabled` setting `--bs-btn-disabled-color`, `--bs-btn-disabled-bg`, `--bs-btn-disabled-border-color` (and the non-variable `color`/`background-color`/`border-color` if Bootstrap still wins) to match the share-editor disabled accent treatment. Keep opacity behavior consistent with other disabled controls.
- Depends on: none.
- Edge cases / error handling: dark theme — accent mix must remain readable (share-editor already pins dark ink `#06201C`); do not change outline/secondary disabled looks.
- Tests: none (CSS-only).
- Verify: open Create glass → Add phone modal with manufacturer/model empty so Add stays disabled; visual check that the button is light teal/green, not blue. Optional: same check on any other page with a disabled primary.
- Done when: disabled primary Add matches accent family; enabled primary unchanged.
- Risk: LOW
- Confidence: HIGH

## [ ] Step 2 — Products list Identifier column for glass is not malformed
- Files: inspect: `src/MobileShop.Web/Pages/Products/Index.cshtml` (Identifier `<td class="num">`), `src/MobileShop.Web/wwwroot/css/surfaces.css` (`.num` / `td.num`), glass projection in `ProductsDataService.GetInventoryAsync` (~lines 113–126); modify: only what the inspection requires (prefer Index markup and/or glass projection string; avoid broad CSS changes).
- Symbols: `ProductListItemViewModel.Identifier`; glass `project` lambda `"Barcode: " + product.Barcode`; `td.num`.
- Current -> Desired: Identifier cells for glass (and other barcode types if the same bug appears) render a clear, correctly spaced label+value (or barcode-only if that matches product convention), without numeric-class distortion, missing space, or broken layout under `?type=glass&sortBy=name&sortDirection=asc`.
- Change: (1) Confirm in browser/devtools whether the malformation is missing space, `.num` LTR/font on mixed text, truncation, or something else. (2) If `.num` is the cause, stop applying `class="num"` when Identifier is not a pure number (e.g. only for IMEI-style pure digits, or drop `.num` for barcode-prefixed values). (3) Ensure the glass projection string is exactly `"Barcode: " + product.Barcode` (space after colon) consistent with other barcode product types. Do not change glass product naming (`GlassModelNaming`).
- Depends on: none (parallel-safe with Step 1).
- Edge cases / error handling: empty barcode; RTL `dir="auto"` still readable; sort by identifier still works on the string.
- Tests: none required unless an existing list test asserts Identifier text — then update expectation only if the string format intentionally changes.
- Verify: `/Products?type=glass&sortBy=name&sortDirection=asc` shows four glass rows with readable Identifier values aligned under the column; no visual glitch.
- Done when: user-visible Identifier for glass is correct and consistent.
- Risk: LOW
- Confidence: MEDIUM (exact malformation not re-checked in a live browser in this plan pass)

## [ ] Step 3 — Glass Details shows compatible phones (and notes)
- Files: inspect: `ProductsDataService.GetDetailsAsync` glass branch (~205–221), Case branch for pattern, `ProductDetailsViewModel.CompatibleModels` / `Notes`, `Details.cshtml` Case-only Compatible block; modify: `src/MobileShop.Services/DataServices/Dal/ProductsDataService.cs`, `src/MobileShop.Web/Pages/Products/Details.cshtml`; do not touch: Api, create path.
- Symbols: `GetDetailsAsync`; `ProductDetailsViewModel`; `GlassProfile.ModelFits`; includes already list `"GlassProfile.ModelFits.ModelNavigation"`.
- Current -> Desired: `/Products/Details?id={glassId}&type=glass` lists each compatible phone model name (same shape as Case). Notes from `GlassProfile.Notes` when present.
- Change: (1) Glass branch of `GetDetailsAsync`: object initializer like Case — `CompatibleModels = product.GlassProfile!.ModelFits.Where(fit => !fit.IsDeleted).Select(fit => fit.ModelNavigation.Name).ToList()`, `Notes = product.GlassProfile.Notes` (or existing normalize helper if Case uses one). Prefer manufacturer+model labels only if Case already does; Case currently uses `ModelNavigation.Name` only — match that unless Create’s selected-fit labels are required for parity. (2) In `Details.cshtml`, extend the Compatible models block from `Type == "Case"` to Case **or** Glass (or duplicate a small Glass block). Keep empty → `N/A`.
- Depends on: none for data; markup depends on projection.
- Edge cases / error handling: soft-deleted fits excluded; no fits → N/A; null GlassProfile should not happen for type=glass but keep null-safe access consistent with Case.
- Tests: if there is an existing details/service test for Case CompatibleModels, add a parallel glass assertion; otherwise no new test file required for this stage.
- Verify: open Details for a glass that was created with multiple compatible phones (e.g. id 18 from the report); list shows those model names; Notes line correct.
- Done when: Details glass page shows compatible phones; Case still works.
- Risk: LOW
- Confidence: HIGH

## [ ] Step 4 — Edit glass uses functioning compatible-phones list (not old multi-select)
- Files: inspect: `CreateGlass.cshtml` + `wwwroot/js/create-glass-fits.js` + glass-fit CSS in `controls.css`, `Edit.cshtml` / `Edit.cshtml.cs`, `_ProductEditForm.cshtml` Glass (and Case) blocks, `ProductsDataService` update path for `CompatibleModels` / `SynchronizeGlassModelFits`; modify: `_ProductEditForm.cshtml` (Glass section), `Edit.cshtml` Scripts if needed, `Edit.cshtml.cs` handlers as required, optionally extract or reuse JS; do not touch: Api, create save rules, Case section unless a shared partial forces a minimal safe tweak.
- Symbols: `Product.CompatibleModels` on edit model; `CompatibleModelOptions`; `SynchronizeGlassModelFits`; create’s `glassFitList` / `Input.CompatibleModelIds` pattern.
- Current -> Desired: Edit glass page shows the same style of compatible-phones list as Create (list of selected fits + Add phone modal with manufacturer/model + optional inline create), posts the selected model IDs, and save persists via existing update + `SynchronizeGlassModelFits`. Old `<select multiple>` for Glass is removed.
- Change: Replace the Glass block in `_ProductEditForm.cshtml` with markup parallel to CreateGlass’s fit list (list + template + empty state + open-modal button). Wire scripts: either include `create-glass-fits.js` with edit-friendly element IDs, or a thin `edit-glass-fits.js` that reuses the same DOM contracts and posts `Product.CompatibleModels` (or the name the edit handler already binds). Ensure `Edit.cshtml.cs` exposes manufacturers, model-by-manufacturer handler (mirror CreateGlass `OnGetModelsAsync` / create manufacturer/model posts if edit must add new phones), and that OnPost maps the list into `CompatibleModels` for the existing update path. Pre-populate selected fits from `product.GlassProfile.ModelFits` (already partially loaded in EditModel). Keep Notes textarea.
- Depends on: Step 3 optional (shared understanding of fit labels); not blocked by 1–2.
- Edge cases / error handling: glass with zero fits; remove last fit (validation: at least one compatible model if create enforces that — match create/update service rules); cancel modal; antiforgery on create-manufacturer/model posts from edit; do not break Case multi-select in this step.
- Tests: manual save round-trip; extend service test only if update CompatibleModels lacks coverage.
- Verify: Edit a glass → see current fits as a list (not a multi-select) → add/remove a phone → Save changes → Details (after Step 3) shows the new set; `dotnet` CI green.
- Done when: Edit glass fits UI works end-to-end; old multi-select gone for Glass.
- Risk: HIGH (UI + handlers + binding)
- Confidence: MEDIUM

## [ ] Step 5 — Final validation
- Files: inspect only (+ `graphify update .` if code changed).
- Symbols: none.
- Current -> Desired: all four user issues closed; CI green.
- Change: run full build/test per project rules when user requests local gate or after push; `graphify update .` and commit graph artifacts if the actor’s last code commit did not; record Action run number.
- Depends on: Steps 1–4.
- Edge cases: none beyond reporting leftovers.
- Tests: solution test suite.
- Verify: `Action: #<run_number> — Success`; manual checks for the four bullets in the user report.
- Done when: Global Definition of Done met.
- Risk: LOW
- Confidence: HIGH

## Global Definition of Done
- Disabled primary Add on Create glass modal is light accent (green/teal), not light blue.
- Products glass list Identifier column is readable and correctly formatted.
- Glass Details shows compatible phone model names (and notes when set).
- Glass Edit uses a working fit list + modal (Create-style), not the old multi-select; save persists fits.
- Only files named in steps changed (plus graphify outputs if updated); no Api/auth/DB-init changes; Conventional Commits; CI green with cited run number.

## Execution notes
One step → one commit → STOP for Job B. Actor must not edit plan/audit/TODO. After Reviewer APPROVES this plan (and proposed Stage 7), Planner copies Stage 7 into `.agents/TODO.md` unchecked; Actor starts Step 1 only.
