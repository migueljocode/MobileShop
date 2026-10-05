# Plan — Stage Y — Searchable person picker + create-person modal

## Requirements
- Functional (from `to-do.md`)
  - **Record Sell**: customer control is a **typeable combobox** (search by **name**, **phone**, **national code**).
  - **Record Buy**: seller control is a **typeable combobox** (search by **name**, **phone**; sellers have no NationalId in the create model — do not invent a column).
  - **"Create new"** opens a **modal** with the create-person fields (customer on Sell, seller on Buy), saves via **fetch/AJAX**, selects the new party **without leaving the page**.
  - **One shared component + shared endpoint pattern** used by both pages (same JS/partial contract; role-specific URLs/fields).
- Non-functional: CI gate; no schema/migration; no Api host/auth; Apple ID plaintext policy untouched.
- Constraints: keep area services; PageModels stay thin; reuse existing `CreateCustomerInputModel` / `CreateSellerInputModel` and `IPeopleDataService.Create*Async` (already returns `EntityId`).

## Decisions (labelled)
- **D1:** Search lives on **`IPeopleDataService`** (not Transactions): e.g. `SearchCustomersAsync(string? q, int take = 25)` and `SearchSellersAsync(string? q, int take = 25)` → `IReadOnlyList<PartyOptionViewModel>` (or a thin search DTO with `Id`, `Label`, optional secondary text). Empty/`null` query → first N by name (or empty list — **prefer first N by name** so the control is usable without typing).
- **D2:** Match fields (case-insensitive contains):
  - Customer: `FirstName`, `LastName`, full name, `PhoneNumber`, `NationalId`.
  - Seller: `FirstName`, `LastName`, full name, `PhoneNumber` only.
- **D3:** UI: **no new NuGet/CDN combobox library**. Follow the existing lightweight pattern (`product-picker.js` + data attributes). Shared `person-picker.js` + partial `_PersonPicker.cshtml` (or equivalent) with:
  - text filter input
  - results list / filtered `<select>`
  - hidden or real bound `Input.CustomerId` / `Input.SellerId`
  - "Create new" button opening Bootstrap modal (site already uses Bootstrap).
- **D4:** AJAX create endpoints as **Razor Page handlers** on Buy/Sell (or one shared People page handler pair) returning JSON shaped like existing **`DropdownCreateResult`** / **`DropdownOptionViewModel`** when practical (`Succeeded`, option `Id`+`Name`, `Error`, `StatusCode`). Handler calls `IPeopleDataService.CreateCustomerAsync` / `CreateSellerAsync` and maps `EntityId` + display label.
- **D5:** After successful create: close modal, add/select the new option in the picker, set the bound id. Validation errors stay in the modal (JSON error message).
- **D6:** Initial page load may still preload a short party list for progressive enhancement; **search is the primary** selection path. Do not require loading every party into the HTML.
- **A1:** CI gate; report Action #; no local `dotnet`.
- **A2:** Api stubs: if `IPeopleDataService` gains methods, update `ApiPeopleDataService` with NIE only.

## Reviewer Briefing
- **Step 1 LOW/HIGH:** service search + tests; easy assertions on match fields.
- **Step 2 MEDIUM/MEDIUM:** handlers + JSON contract; anti-forgery for POST handlers.
- **Step 3 MEDIUM/MEDIUM:** shared partial/JS + wire Buy/Sell; modal accessibility (focus trap optional if expensive — at least `aria-label` and Escape to close if Bootstrap default).
- Seller has **no** NationalId today — search must not claim otherwise.
- Existing full-page Create Customer/Seller routes stay; modal is an alternate entry for transaction flow.

## ~~[x] Step 1 — People search on the service~~

- Files
  - Modify: `IPeopleDataService`, `PeopleDataService`, `ApiPeopleDataService` (NIE).
  - Tests: `PeopleDataServiceTests` — match by name fragment, phone, national id (customer); seller phone/name; take limit; empty query behavior per D1.
  - Do not touch: Web UI, schema.

- Done when: search methods work; tests green; Job B PASS.

- Risk: LOW
- Confidence: HIGH

## [ ] Step 2 — AJAX create handlers (JSON)

- Files
  - Modify: `Buy.cshtml.cs` / `Sell.cshtml.cs` (or a single shared page under `Pages/People` if cleaner — **prefer handlers on Buy and Sell** to keep antiforgery with the form page).
  - `OnPostCreateSellerAsync` (Buy) / `OnPostCreateCustomerAsync` (Sell) accepting the existing bind models, calling People service, returning JSON (`DropdownCreateResult` or equivalent).
  - Ensure antiforgery token is available to fetch (standard Razor token in the modal form).
  - Tests: page-model or service-level coverage for success `EntityId` mapping; minimal handler test if the suite already tests page handlers that way.
  - Do not touch: person-picker UI chrome yet (can be temporary plain endpoint smoke via tests only).

- Done when: create-via-handler returns selectable id; CI green; Job B PASS.

- Risk: MEDIUM
- Confidence: MEDIUM

## [ ] Step 3 — Shared person-picker UI + wire Buy/Sell

- Files
  - Create: `wwwroot/js/person-picker.js`, Shared partial for picker + modal markup (parameterized: role customer|seller, search URL, create handler name).
  - Modify: `Buy.cshtml`, `Sell.cshtml` (and page models if needed for search handler `OnGetSearchSellersAsync` / `OnGetSearchCustomersAsync` returning JSON options).
  - Layout: register script once (same pattern as product-picker).
  - Optional: reduce reliance on full `GetCustomersAsync`/`GetSellersAsync` payload on GET if search covers it — keep a fallback list if simpler for v1.
  - Do not touch: product picker behavior, schema, other areas.

- Done when: both pages search + create-in-modal + select without navigation; CI green; Job B PASS.

- Risk: MEDIUM
- Confidence: MEDIUM

## [ ] Step 4 — Final Stage Y validation

- Full CI green; scope check; reviewer ticks Stage Y in `to-do.md`.

- Risk: LOW
- Confidence: HIGH

## Global Definition of Done
- Buy/Sell use searchable party pickers; create-new modal works via AJAX and selects the new party.
- Shared JS/partial contract; service search covers required fields; no schema change; CI green.

## Execution notes
One step → one commit → green Action → merge → STOP for Job B. Do not edit plan/audit/todo. Do not run local `dotnet`.

**Awaiting Job A** before Act on Step 1.
