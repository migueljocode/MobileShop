# Plan — Stage M — PartNumber

## Reviewer Briefing

- Prior Stage M sign-off is void for UX; stage stays open until this plan’s final Job B PASS.
- Step 1 is HIGH for list projection: `ProductListItemViewModel` must gain a PartNumber display field and **every** constructor site must be updated.
- Step 1 UI: phone-only selector, no Filter button, auto-apply on change, inventory-derived options (not full catalog).
- Step 2 is Create Phone Model-scoped PartNumber + Add New (owner “add new if missing”); not on Products Index. Stage N must not re-implement the same control later.
- One step → one commit → Job B. Actor never edits plan.md / audit.md / to-do.md.

## ~~[x] Step 1 — Correct Products PartNumber filtering and inventory display~~

- Files
  - Inspect/modify:
    - `src/MobileShop.Web/Pages/Products/Index.cshtml`
    - `src/MobileShop.Web/Pages/Products/Index.cshtml.cs`
    - `src/MobileShop.Services/DataServices/Dal/ProductsDataService.cs`
    - `src/MobileShop.Models/ViewModels/Web/ProductListItemViewModel.cs`
    - Existing `IProductsDataService` (locate; do not invent a second contract)
    - Every other `new ProductListItemViewModel(...)` call site if the record shape changes (grep required)
  - Tests: `ProductsDataServiceTests`; page tests only if the repo already has them
  - Do not touch: API project, auth, PDF, migrations/schema, Create Phone (Step 2)

- Symbols
  - `IndexModel.OnGetAsync`
  - `GetInventoryRowsAsync`
  - Inventory-derived PartNumber options (new method if needed; do **not** misuse Model-scoped `GetPartNumbersAsync(modelId)` for Index)
  - `ProductListItemViewModel`
  - `Index.cshtml` type links + filter form + table

- Current → Desired
  - Current: selector always visible; Filter button; options from global `GetPartNumbersAsync()`; no list column; type links all pass `partNumberId`.
  - Desired:
    1. Render the PartNumber control **only** when `Type == "phone"`. For `all` / `appleid`, do **not** render the form.
    2. Remove the Filter button entirely.
    3. Auto-apply: GET form; on select change submit the form (e.g. `onchange` → `this.form.submit()`). No second click.
    4. Empty / “All part numbers” clears the PartNumber restriction.
    5. Type links: **All** and **Apple IDs** must **not** pass `partNumberId`. **Phones** may preserve a valid selection.
    6. Options = distinct PartNumbers attached to **current phone inventory**, built with the PartNumber filter **omitted** so the dropdown does not collapse to the selected id only. Leading “All part numbers”. Exclude unused catalog PartNumbers and soft-deleted phones/part numbers per existing query filters.
    7. Stale positive `partNumberId` not in that option set → treat as no selection.
    8. null / 0 / negative query → no selection.
    9. Add `PartNumberLabel` (or equivalent `string`) to `ProductListItemViewModel`. Update **all** constructions: phones project code or `"N/A"`; Apple IDs always `"N/A"`.
    10. Table: new **Part number** column bound to that field.
    11. Existing Details, ordering, sold/second-hand badges, and other columns unchanged.

- Change (concrete)
  - Extend list ViewModel + inventory projections.
  - Service method for inventory-available PartNumber dropdown options (or extend contract cleanly).
  - Index page: conditional form, auto-submit, type-link query cleanup, column.

- Tests
  - Options only from phones in inventory; unused PartNumber excluded.
  - Valid PartNumber filters phones; Apple IDs not returned under phone+PartNumber filter path as designed.
  - null/0/negative = no filter.
  - Projection exposes code vs N/A.
  - Compile-safe after ViewModel shape change (all call sites).

- Verify: focused ProductsDataService (+ page if any) tests; full suite deferred to Step 3 unless compile fails.

- Done when: phone-only auto filter; no Filter button; inventory options; Part number column; Job B PASS.

- Risk: MEDIUM
- Confidence: HIGH

## ~~[x] Step 2 — Create Phone PartNumber selector + Add New~~

- Files: `CreatePhone.cshtml`, `CreatePhone.cshtml.cs`, `CreatePhoneInputModel.cs`, `ProductsDataService` / `IProductsDataService`, existing Create Phone / service tests.
- Do not: new migration, Create Apple ID PartNumber, parallel PartNumber admin page, new frontend framework.

- Current → Desired
  1. Nullable `PartNumberId` on input model.
  2. Combobox + Add New (existing modal/AJAX pattern).
  3. Options **Model-scoped** via existing `GetPartNumbersAsync(modelId)`.
  4. Manufacturer/Model change resets/reloads PartNumber options.
  5. Add New disabled/rejects without Model; collects code, Dual SIM, eSIM; calls `CreatePartNumberAsync`; selects returned option without full reload.
  6. Duplicate Model+Code reuses existing service behavior.
  7. CreatePhone persists selected PartNumberId; omit → null (valid).
  8. Reject PartNumber belonging to another Model.
  9. No unrelated field behavior changes.

- Tests: omit/persist/reject foreign Model; Model-scoped list; Add New create/select; existing create/reuse intact.

- Verify: focused Create Phone / ProductsDataService tests.

- Done when: select or Add New works; persist/null OK; Job B PASS.

- Risk: HIGH
- Confidence: MEDIUM

## ~~[x] Step 3 — Final Stage M validation (Reviewer sign-off)~~

- Actor: no production changes expected; report SHAs + validation only if asked. Reviewer runs/records full build+test and UX checklist, writes final PASS, then ticks `to-do.md`.
- Checklist: phone-only selector; no Filter button; auto-apply; inventory options; column; Create Phone combobox+Add New; details still OK; no API/auth/PDF/migration scope creep.
- Risk: HIGH | Confidence: HIGH

## Global Definition of Done

- Phone-only, auto-applied PartNumber filter; no Filter button.
- Options from current phone inventory; list column present.
- Create Phone: Model-scoped combobox + Add New; persist or null.
- Details/SIM display unchanged; no destructive schema work in these corrective steps.
- Focused + full build/test green; every step Job B PASS; only then Stage M checked in `to-do.md`.

## Execution notes

Implement **only** the authorized step. One implementation commit, then **STOP** for Job B. Do not edit plan/audit/todo. No silent scope expansion.
