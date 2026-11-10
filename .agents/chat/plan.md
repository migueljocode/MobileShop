# Plan — People list: hide role-irrelevant columns by filter

## Proposed stages
- [ ] Stage 10 — People list columns: customers hide Seller type; sellers hide National ID; all unchanged

## Reviewer Briefing
- Pure presentation on `People/Index.cshtml`. Data already omits the other role’s field (`EntityType` null for customers, `NationalId` null for sellers), so the table shows `N/A` noise on filtered views.
- `type=all` must keep **both** National ID and Seller type (user confirmed fine).
- No service, model, or API changes required unless a test asserts fixed column headers.
- Risk is low; only watch colspan/empty-state and sort toolbar (unchanged).

## ~~[x] Step 1 — Conditionally render National ID and Seller type columns~~
- Files: inspect: `src/MobileShop.Web/Pages/People/Index.cshtml`, `Index.cshtml.cs` (`Type`, `PersonRow`); modify: `Index.cshtml` only; do not touch: data service, create pages, Api.
- Symbols: `Model.Type` (`"all"` | `"customers"` | `"sellers"`); `PersonRow.NationalId`, `PersonRow.EntityType`.
- Current -> Desired:
  - `?type=customers` → columns: Role, Name, Phone, **National ID**, Purchased, Details — **no** Seller type.
  - `?type=sellers` → columns: Role, Name, Phone, **Seller type**, Sold, Details — **no** National ID.
  - `?type=all` (or default) → all columns as today: Role, Name, Phone, National ID, Seller type, Purchases / sales, Details.
- Change: In thead and each tbody row, wrap the National ID `<th>`/`<td>` in `@if (Model.Type is "all" or "customers")` and the Seller type `<th>`/`<td>` in `@if (Model.Type is "all" or "sellers")`. Keep `N/A` fallbacks for the `all` view when a field is null.
- Depends on: none.
- Edge cases: empty list still uses `_EmptyState` (no table); Activity column label already switches (Purchased / Sold / Purchases / sales) — leave as is.
- Tests: none required (markup-only). Optional snapshot/page test only if one already pins header text.
- Verify: open the three URLs from the report; column sets match Desired; `all` still shows both role-specific columns.
- Done when: customers view has no Seller type; sellers view has no National ID; all unchanged.
- Risk: LOW
- Confidence: HIGH

## [ ] Step 2 — Final validation
- Files: inspect only.
- Current -> Desired: CI green if a docs-only/markup push still runs checks; otherwise note `Action: unavailable` when paths-ignore applies.
- Depends on: Step 1.
- Verify: manual three-filter check; cite CI run if any.
- Done when: Global Definition of Done met.
- Risk: LOW
- Confidence: HIGH

## Global Definition of Done
- Customers filter: no Seller type column.
- Sellers filter: no National ID column.
- All filter: both columns remain.
- Only `Index.cshtml` changed for this stage; Conventional Commit; Job B after the single step commit.

## Execution notes
One step → one commit → STOP for Job B. Actor must not edit plan/audit/TODO. After Reviewer APPROVES, Planner adds Stage 10 to `.agents/TODO.md` unchecked; Actor starts Step 1 only. Stages 8–9 (model uniqueness / person reuse) remain separate proposed work and are not blocked by this stage.
