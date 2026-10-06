# Plan — Stage Z — Searchable product picker + create-product modal

## Requirements
- Functional (from `to-do.md`)
  - **Record Buy and Record Sell**: product control becomes a **typeable combobox** in the **same style as the person picker** (search by name / identifier / type / color / part number as available on `ProductListItemViewModel`).
  - **Suggested price** behavior from today’s `product-picker.js` must still apply after selection.
  - **"Create product"** on **Buy** (and Sell if practical without breaking sell inventory rules) opens a **modal**: first pick **product type**, then load that type’s create form inside the modal.
  - First types: **Phone**, **Apple ID**, **Glass**.
  - **Registry** so later stages (AA/AB) plug in by registering a type → form loader + create handler.
  - On success: select the new product on the transaction form without leaving the page.
- Non-functional: CI gate; no schema/migration; no Api host/auth.
- Constraints: reuse `IProductsDataService.CreatePhoneAsync` / `CreateAppleIdAsync` / `CreateGlassesAsync` and existing input models; keep PageModels thin.

## Decisions (labelled)
- **D1 — Search ownership:** `ITransactionsDataService.SearchSelectableProductsAsync(TransactionDirection direction, string? q, int take = 25)` so direction rules stay with the same path as `GetSelectableProductsAsync` (Buy vs Sell eligibility). Implementation may call into Products helpers internally if needed, but the **public** search for the transaction pages is on Transactions.
- **D2 — Match fields (case-insensitive contains on projected strings):** product **Name**, **Type**, **Identifier**, **Color**, **PartNumberLabel** (skip empty / "N/A"). Empty query → first N selectable products for that direction (same spirit as person search).
- **D3 — UI:** no new combobox library. Evolve `product-picker.js` (or add `product-picker-search.js` if cleaner) to match person-picker: text search, results list, hidden `Input.ProductId`, debounced GET handler. Keep `data-suggested-price` / finished-price sync.
- **D4 — Create modal scope (honest):** full Create Phone page has nested manufacturer/model/part-number modals. For Stage Z v1:
  - Modal embeds **streamlined forms** (same bind models) with dropdowns populated from existing service methods (`GetManufacturersAsync`, `GetModelsAsync`, …).
  - Nested "Add manufacturer/model/color/part-number" **optional** if cheap (reuse existing Products page handlers/JS); otherwise **require selecting existing catalog rows** and document that full nested create remains on Products pages.
  - Glass may create a **batch** (`CreateGlassesAsync`); return the **first created product id** (or the id the service already returns) for selection — if service returns only count, extend `ServiceResult.EntityId` consistently.
- **D5 — Registry:** a small Web-side map (static or DI) of `type key → { form partial or GET handler, POST create handler name }` for `phone` | `appleid` | `glass`. Stages AA/AB only add a registry entry + partial/handler.
- **D6 — Sell create product:** allowed only if creating inventory that can later be sold is coherent; **default: Create product button on Buy only**; Sell gets searchable picker only. (Owner can overrule in review.)
- **A1:** CI gate; Action #; no local `dotnet`.
- **A2:** Api stubs for any new interface members = NIE only.

## Reviewer Briefing
- **Step 1 LOW/HIGH:** search API + tests against selectable rules.
- **Step 2 MEDIUM/MEDIUM:** searchable product UI; must not break suggested price.
- **Step 3 HIGH/MEDIUM:** modal + registry + three create paths; largest risk is form complexity and EntityId for glass batch.
- Person picker is the template for UX contracts (handlers, antiforgery, JSON shape).

## ~~[x] Step 1 — Search selectable products (service)~~

- Files: `ITransactionsDataService`, Dal `TransactionsDataService`, Api stub if present; tests for direction filter + query match + take/empty.
- Do not touch Web UI yet.
- Done when: search works; CI green; Job B PASS.
- Risk: LOW · Confidence: HIGH

## ~~[x] Step 2 — Searchable product picker UI (Buy + Sell)~~

- Files: `product-picker.js` (or companion), Buy/Sell cshtml, `OnGetSearchProductsAsync` handlers on both pages, optional partial mirroring `_PersonPicker` structure for products.
- Keep suggested-price display and price autofill.
- Done when: both pages can search/select products; CI green; Job B PASS.
- Risk: MEDIUM · Confidence: MEDIUM

## ~~[x] Step 3 — Create-product modal + registry (Phone, Apple ID, Glass)~~

- Files: shared modal shell + registry; GET form fragment handlers; POST create handlers calling `IProductsDataService`; slim partials or reused field markup for the three types.
- Ensure antiforgery + independent validation (same lesson as Stage Y: no `Input` ModelState pollution; prefer `Validator.TryValidateObject`).
- Tests: at least one successful create-via-handler returns selectable product id for phone (and glass EntityId policy explicit).
- Done when: create-in-modal works for the three types; new product selectable; CI green; Job B PASS.
- Risk: HIGH · Confidence: MEDIUM

## [ ] Step 4 — Final Stage Z validation

- Full CI green; scope check; tick Stage Z in `to-do.md`.
- Risk: LOW · Confidence: HIGH

## Global Definition of Done
- Buy/Sell product fields are searchable comboboxes with suggested price intact.
- Buy can create Phone / Apple ID / Glass via modal + registry; new product selected in-page.
- No schema change; CI green.

## Execution notes
One step → one commit → green Action → STOP for Job B. Do not edit plan/audit/todo. Do not run local `dotnet`.

**Awaiting Job A** before Act on Step 1.
