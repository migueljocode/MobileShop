# Plan — Stage O — Transactions Buy/Sell form rework

## Assumptions
- **A1** The label reads "Finished price"; the bound property stays `Price`.
- **A2** Product change shows catalog price and prefills finished price; the user may overwrite it. The server remains the source of truth.
- **A3** Date inputs are `type="date"` and default to today on GET.
- **A4** Shared picker options/script; pages keep tag-helper controls for validation and round-trip binding.
- **A5** No typeahead library.
- **A6** Buy keeps Seller; Sell has no Seller field (the shop is the seller).

## Reviewer Briefing
- Step 3 is complete. Its Job B evidence confirmed the selected-product round-trip fix on both pages.
- Step 4 is validation-only and is the remaining gate before Stage O sign-off.
- Do not touch `src/MobileShop.Api`, services/entities/schema/PDF/auth unless validation discovers a regression requiring replanning.

## ~~[x] Step 1 — Suggested price on selectable products~~
- **Done** — Job B PASS (`8660d69`). Added `SuggestedPrice` and selectable-product projections.

## ~~[x] Step 2 — Shared product-picker partial + Buy page~~
- **Done** — Job B PASS after correction (`bee5ba9`, `546440f`, `b5df8fe`). Shared picker/script, Buy tag-helper controls, Finished price label, date default.
- **Carry-over resolved in Step 3:** posted product re-selection.

## ~~[x] Step 3 — Sell page + selected-product round-trip fix~~
- **Done** — Job B PASS (`a7e9419d868b7160f82c69f7cb5129ded402146b`).
- Delivered: Sell uses the shared picker/script; both pages pass `Input.ProductId` to the options partial; the partial explicitly re-selects the posted product; Sell defaults date to today and labels Price as Finished price; focused test added.
- Evidence: 294/294 tests passed, build had 0 warnings/errors, rendered Buy/Sell POST checks preserved selected product and posted price, and API diff was empty.
- Actor scope: only the seven approved files changed; `plan.md`, `audit.md`, and `to-do.md` were untouched.

## [ ] Step 4 — Final Stage O validation
- **Files:** none expected; validation only.
- **Verify**
  - `dotnet clean src/MobileShop.slnx --nologo && dotnet build src/MobileShop.slnx --nologo --no-incremental && dotnet test src/MobileShop.slnx --nologo --no-build` — require 0 warnings/errors and all tests passing.
  - Manually check the prefilled Finished price under the app's configured culture, including a comma-decimal culture if configured; report whether binding/rendering is correct.
  - Confirm Buy and Sell show Product, Suggested price, Finished price, Date (today by default), and Back; Buy has Seller, Sell has no Seller.
  - Record concise evidence in `act.md` without changing implementation files.
- **Done when:** all checks pass. Then Reviewer may sign off Stage O and tick `to-do.md`.
- **Risk:** LOW
- **Confidence:** HIGH

## Global Definition of Done
- Buy and Sell use the shared picker (options partial + script) with page-owned tag-helper controls.
- Validation messages render; posted product and price survive failed posts on both pages.
- Finished price, suggested price, date default, and Back behavior are present on both pages.
- Clean build and full test suite pass; no API/auth/PDF/schema changes.

## Execution notes
- One implementation step → one commit → Job B → next step.
- Completed steps may be compacted to a single outcome/evidence line; do not retain their old implementation instructions once they have passed Job B.
- Step 4 is validation only. Do not tick `to-do.md` until final validation passes.
