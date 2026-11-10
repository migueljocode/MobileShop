# Plan — Catalog model uniqueness + person reuse on Create seller/customer

## Proposed stages
- [ ] Stage 8 — Prohibit duplicate catalog models (same manufacturer + category + name)
- [ ] Stage 9 — Create seller/customer: pick existing person or enter new (combo + quick-fill)

## Reviewer Briefing
- **Stage 8:** `CreateModelAsync` already finds exact `ManufacturerId + Name + CategoryId` and returns the existing option as success; the unique index is only `(ManufacturerId, Name)` among live rows (case-sensitive, no CategoryId). Users can still land two “exact” iPhone 16 rows via case variants (`iPhone 16` vs `iphone 16`), whitespace variants that normalize differently, or UI that appends a second option after a reuse response. Goal: case-insensitive uniqueness under manufacturer **and** category, clear message when reuse happens, UI selects existing without a second list entry.
- **Stage 9:** `CreateSellerAsync` / `CreateCustomerAsync` always `new Person { ... }` even when the same human already exists (e.g. as the other role). EF already has `Person` 1–1 with `Seller` and with `Customer`. User wants a combo of current persons (name + last name) that quick-fills the form fields, while still allowing a blank form for a brand-new person. Reuse `IPeopleDataService` search patterns and the existing person-picker UX where practical; do not invent a second person identity model.
- Do not touch Api, auth, DB init policy, or glass Model-label work except if Stage 8 shares `CreateModelAsync`.
- Stage 8 first (smaller, service-heavy); Stage 9 second (UI + service).

---

## [ ] Step 1 — Case-insensitive model uniqueness in `CreateModelAsync` (+ tests)
- Files: inspect: `ProductsDataService.CreateModelAsync`, `ModelConfiguration` unique index, create-phone/glass JS that posts CreateModel; modify: `src/MobileShop.Services/DataServices/Dal/ProductsDataService.cs`; tests: `src/MobileShop.Tests/Services/DataServices/Dal/ProductsDataServiceTests.cs`; optional later UI in Step 2.
- Symbols: `CreateModelAsync(int manufacturerId, string name, string categoryName = "Phone")`; `DropdownCreateResult`.
- Current -> Desired: creating a model whose trimmed name equals an existing live model for the same manufacturer **and** category under ordinal-ignore-case returns the existing option (Succeeded) and does **not** insert a second row. Distinct names (different spelling beyond case) still create. Empty name still 400.
- Change:
  1. After resolving manufacturer + category and `trimmed = name.Trim()`, find existing with case-insensitive name match scoped to `ManufacturerId` + `CategoryId` (e.g. load candidates for that manufacturer+category and compare with `StringComparer.OrdinalIgnoreCase`, or EF `EF.Functions.Collate` / normalized form — prefer the approach already used elsewhere in the codebase for name reuse).
  2. If found → return `DropdownCreateResult(true, existing option, null, 200)` (same as today for exact match).
  3. If not found → insert as today. Rely on the unique index as a safety net; if `AddAsync` fails due to unique violation, re-fetch and return existing or a clear 400 — do not leave a partial failure opaque.
  4. Do **not** change the unique index shape in this step unless inspection proves SQLite allows a filtered unique on `(ManufacturerId, CategoryId, Name)` without a migration fight; prefer service-level case-insensitive guard first. Document in step notes if a follow-up migration is needed.
- Depends on: none.
- Edge cases: leading/trailing spaces; soft-deleted model with same name (index allows a new live row — keep current soft-delete semantics); same name different category (e.g. accessory) still allowed when CategoryId differs; same name different manufacturer allowed.
- Tests: (1) create `iPhone 16` under Apple/Phone, second call `iphone 16` → same Id, single row in DB; (2) different manufacturer can still create `iPhone 16`; (3) existing exact-match test still passes.
- Verify: CI green; manual Create Phone → add model `iPhone 16` twice under Apple → one catalog model.
- Done when: case-insensitive reuse works and is tested; no duplicate live rows for same manufacturer+category+name ignoring case.
- Risk: MEDIUM (query style / EF provider case rules on SQLite)
- Confidence: HIGH

## [ ] Step 2 — Create-model UI treats reuse as select-existing (no duplicate dropdown entries)
- Files: inspect: `CreatePhone.cshtml` CreateModel fetch handler, `create-glass-fits.js` `saveNewPhoneModel`, other product create pages that post CreateModel; modify: only the JS/handlers that append options without checking id.
- Symbols: `OnPostCreateModelAsync` JSON `{ id, name }`; client code that pushes options into `<select>`.
- Current -> Desired: when the server returns an existing model id already present in the select, the UI selects that option and does not add a second identical entry. Optional: show a short non-blocking note “Already in list” if easy; not required.
- Change: after successful CreateModel response, `if (select has option[value=id]) select it; else append option`. Apply to Create Phone and Create Glass phone-model creator at minimum; other create pages if they share the same bug pattern (scan and fix the same one-liner pattern only).
- Depends on: Step 1 preferred (so case variants collapse server-side).
- Edge cases: concurrent two tabs; empty response error path unchanged.
- Tests: none required if pure client; optional page test only if one already stubs CreateModel.
- Verify: UI cannot show two identical iPhone 16 options under the same manufacturer after double-add.
- Done when: double-add selects one option; list has one entry.
- Risk: LOW
- Confidence: HIGH

## [ ] Step 3 — Service: create seller/customer from existing PersonId or new person fields
- Files: inspect: `PeopleDataService.CreateSellerAsync` / `CreateCustomerAsync`, `CreateSellerInputModel` / `CreateCustomerInputModel`, `Person` / `Seller` / `Customer` configurations (1–1); modify: those service methods + input models; tests under `src/MobileShop.Tests` for People; do not touch Api implementation beyond matching interface if required by compile.
- Symbols: `IPeopleDataService.CreateSellerAsync`, `CreateCustomerAsync`; `Seller.PersonId`, `Customer.PersonId`.
- Current -> Desired: submit may include optional `PersonId`. If set and person exists: attach a new Seller/Customer to that person **without** creating another Person; if that person already has that role, return a clear validation error. If `PersonId` is null/0: keep today’s behavior (create Person from FirstName/LastName/Phone/Notes and role-specific fields). When linking existing person, still allow EntityType (seller) / NationalId (customer) from the form; person field edits on link path should either be ignored or applied to the existing Person only if product rules already update person elsewhere — **default: do not overwrite person fields on link** unless a small, explicit update is needed for empty phone; prefer no overwrite in v1.
- Change:
  1. Add optional `int? PersonId` to both input models.
  2. `CreateSellerAsync`: if `PersonId` is > 0, load person; 404 if missing; if `person.SellerProfile` (or sellers.Find by PersonId) exists → fail “This person is already a seller.”; else `new Seller { PersonId = …, EntityType = input.EntityType }` without new Person.
  3. `CreateCustomerAsync`: same with Customer + require NationalId from input (customer-specific); reject if already a customer.
  4. If no PersonId → existing create path.
- Depends on: none (can ship before UI).
- Edge cases: person is seller but not customer → creating customer for same PersonId must succeed; Zone Mobile shop person; soft-deleted seller profile (follow existing soft-delete patterns).
- Tests: create seller with new person; create customer linking same person; second seller for same person fails; invalid PersonId fails; null PersonId still creates person.
- Verify: CI tests green.
- Done when: service supports link-or-create for both roles with tests.
- Risk: MEDIUM (1–1 relationship and NationalId on customer)
- Confidence: HIGH

## [ ] Step 4 — Create seller/customer UI: person combo + quick-fill
- Files: inspect: `CreateSeller.cshtml`, `CreateCustomer.cshtml`, `_PersonPicker.cshtml` / `person-picker.js` (search API), `IPeopleDataService` list/search helpers; modify: both create pages + code-behind; optional shared partial for “person fields + existing person combo”; do not redesign People Index.
- Symbols: create page models; any `OnGetPersons` / search handler reused from person picker.
- Current -> Desired: at the top of Create seller and Create customer, a combo/select of existing persons labeled by first + last name (and phone if needed for disambiguation). Choosing one fills FirstName, LastName, PhoneNumber, Notes (and NationalId on customer if stored on person or left for user). User can clear selection and type a new person. Submit sends `PersonId` when an existing person is selected, otherwise 0/null with typed fields.
- Change:
  1. Page GET loads person options (id + display name) via data service — reuse existing person search/list if one exists; otherwise a thin `GetPersonOptionsAsync` returning id + `"First Last"`.
  2. Markup: `<select>` or combobox “Use existing person” with empty option “— New person —”, then the same name/contact fields as today.
  3. Small script: on change, set field values from `data-*` attributes on the option (or fetch-by-id if payload is large); set hidden `Input.PersonId`.
  4. Prefer minimal new JS; do not require the full transaction person-picker modal unless it already supports fill-without-submit.
- Depends on: Step 3.
- Edge cases: long person lists (prefer combobox/search if > ~50; otherwise select is fine for v1); RTL names `dir=auto`; validation messages when link fails because already seller/customer.
- Tests: page-level optional; rely on service tests from Step 3 for core rules.
- Verify: manual — create seller for person A; open Create customer, pick person A, fields fill, save → one Person, both roles. Create seller again for A → error.
- Done when: both create pages support pick-existing quick-fill and new-person entry.
- Risk: MEDIUM (UX + binding)
- Confidence: MEDIUM

## [ ] Step 5 — Final validation
- Files: inspect only; `graphify update .` if required after code commits.
- Current -> Desired: both user problems addressed; CI green.
- Change: full CI; manual checks for duplicate model and person reuse paths.
- Depends on: Steps 1–4 (Stage 8 = 1–2, Stage 9 = 3–4).
- Verify: `Action: #<run_number> — Success`.
- Done when: Global Definition of Done met.
- Risk: LOW
- Confidence: HIGH

## Global Definition of Done
- Live catalog models are unique per manufacturer + category + name ignoring case; double-add in UI does not create a second row or duplicate dropdown option.
- Create seller and Create customer can pick an existing person (combo + quick-fill) or enter a new one; linking reuses `Person` and refuses a second seller/customer profile for the same person.
- Tests cover model case-insensitive reuse and person link-or-create; no Api/auth/DB-init policy changes; CI green.

## Execution notes
One step → one commit → STOP for Job B. Actor must not edit plan/audit/TODO. After Reviewer APPROVES proposed stages, Planner appends Stage 8 and Stage 9 unchecked to `.agents/TODO.md` and leaves Stage 7 glass work as already tracked separately. Actor starts Step 1 only.
