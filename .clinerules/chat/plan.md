# Plan — Stage W — Transactions list sorting

## Requirements
- Functional (from `to-do.md`)
  - Click-to-sort **column headers** on **Date**, **Product**, **Price**, **Seller**, **Customer** (asc/desc).
  - **Server-side** sort; keep existing **direction** and **take** filters; preserve selection checkboxes / factor download query string where practical.
  - **Sort indicator** on the active column (e.g. ↑ / ↓ or `aria-sort`).
- Non-functional: no schema/migration; CI is the gate (no local `dotnet`); Api **stubs** only if interface signatures change (same pattern as Stage V).
- Constraints: no Api **host**/auth/entities/init policy changes; Apple ID passwords untouched.

## Decisions (labelled)
- **D1:** Today `GetListAsync(direction, take, ascending)` only orders by **Date**. Extend with a `sortBy` string (or equivalent) so the service can order by any of the five columns after the projected rows are built (same in-memory pattern as the current date OrderBy).
- **D2:** Canonical `sortBy` values (lowercase): `date` (default), `product`, `price`, `seller`, `customer`. Unknown → treat as `date`.
- **D3:** `order` stays `asc` | `desc` (default **`desc`** for date, matching today's "Newest first"). For non-date columns, default first click = **asc** is optional; **simpler rule: default order remains `desc` for every column**, and clicking the active header toggles asc↔desc.
- **D4:** UI: replace the filter **Order** dropdown with **sortable `<th>` links** that GET with `direction`, `take`, `sort`, `order` (and keep `selectedIds` on factor links). Direction/Count filters still auto-submit as today.
- **D5:** `GenerateListFactorPdfAsync` continues to use the **same** list ordering as the visible page (pass through the new sort args via `GetListAsync`), so the PDF matches what the user sees.
- **D6:** Sort keys on the view model: `Date`, `ProductLabel`, `FinishedPrice`, `SellerLabel`, `CustomerLabel` (string ordinal ignore-case for labels; numeric for price; date for Date).
- **A1:** CI gate — actor reports `Action: #<n> — Success|Failure|Pending`; wait green before merge.
- **A2:** Api **project** host stays untouched; only `ApiTransactionsDataService` method signature if the interface changes (NIE body).

## Reviewer Briefing
- **Step 1 LOW/HIGH:** pure service contract + tests; easy to verify with ordered assertions.
- **Step 2 MEDIUM/MEDIUM:** Razor header links, toggle logic, factor query carry-over; watch for broken Download Factor routes missing `sort`/`order`.
- Direction column is **not** in the to-do sort list — leave it as a plain header.
- Existing test `GetListAsync_filters_by_direction_and_orders_by_date` must be updated for the new signature without losing direction/take coverage.

## ~~[x] Step 1 — Server-side multi-column sort in TransactionsDataService~~

- Files
  - Modify: `ITransactionsDataService.GetListAsync` — add parameter e.g. `string? sortBy = null` (keep `ascending` or rename to clarity; prefer **keep `bool ascending`** + add `sortBy`).
  - Modify: `TransactionsDataService.GetListAsync` — after `SelectListRowsAsync`, OrderBy the chosen column then `Take`.
  - Modify: `GenerateListFactorPdfAsync` call path so it forwards the same `sortBy`/`ascending` into `GetListAsync` (extend its signature if the page will pass them).
  - Modify: `ApiTransactionsDataService` stub signature only.
  - Tests: `TransactionsDataServiceTests` — date asc/desc still pass; add cases for price, product, seller, customer (seed enough rows with distinct labels/prices).
  - Do not touch: Web pages (Step 2), schema, sample-data, auth, Api host.

- Current → Desired
  - Current: order only by `Date`.
  - Desired: `sortBy` selects column; `ascending` selects direction; default `sortBy=date`, `ascending=false`.

- Edge cases: null/empty/unknown `sortBy` → date; `take` clamp unchanged (1–500).

- Verify: CI green on the step PR; record Action #.

- Done when: service sorts all five columns; tests green; Job B PASS.

- Risk: LOW
- Confidence: HIGH

## [ ] Step 2 — Transactions Index: clickable headers + indicators + query carry-over

- Files
  - `Pages/Transactions/Index.cshtml.cs` — accept `sort` (or `sortBy`) + `order`; expose `Sort` / `Order` for the view; `LoadAsync` and `OnGetDownloadFactorAsync` pass them to the service.
  - `Index.cshtml` — remove Order **dropdown** (D4); Date/Product/Price/Seller/Customer headers become links (or buttons in a GET form) that set `sort` + toggled `order` while keeping `direction` and `take`; show sort indicator on the active column; factor/download links include `sort` and `order`.
  - Optional: small page-model tests if the repo already has Transactions page tests; otherwise service tests + CI smoke if any Transactions route is already smoke-checked.
  - Do not touch: Buy/Sell create forms, Details, PDF generator layout (Stage X).

- Current → Desired
  - Headers inert → clickable server sort with indicator; filters preserved.

- Done when: UI matches to-do; Job B PASS.

- Risk: MEDIUM
- Confidence: MEDIUM

## [ ] Step 3 — Final Stage W validation

- Full CI green (build 0 warnings, suite, smoke as applicable).
- Scope check: no Api host / auth / entities / migrations.
- Reviewer ticks Stage W in `to-do.md` only after PASS.

- Risk: LOW
- Confidence: HIGH

## Global Definition of Done
- Transactions list sorts by Date, Product, Price, Seller, Customer (asc/desc) from headers with a visible indicator.
- Direction and take filters still work; factor download uses the same sort.
- Server-side only; no schema; CI green; Stage W checked.

## Execution notes
One step → one commit → wait for green Action → merge → STOP for Job B. Do not edit plan/audit/todo. Do not run local `dotnet`. Report `Action: #<n> — …` per step.

**Awaiting Job A** before Act on Step 1.
