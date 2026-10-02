# Plan — Stage O — Transactions Buy/Sell form rework

## Assumptions
- **A1** The label reads "Finished price"; the bound property stays `Price`.
- **A2** Product change shows catalog price and prefills finished price; the user may overwrite it. The server remains the source of truth.
- **A3** Date inputs are `type="date"` and default to today on GET.
- **A4** Shared picker options/script; pages keep tag-helper controls for validation and round-trip binding.
- **A5** No typeahead library.
- **A6** Buy keeps Seller; Sell has no Seller field (the shop is the seller).

## Reviewer Briefing
- Steps 1–3 are complete.
- Step 4 was validation-only and is now complete after reviewing Actor evidence and GitHub CI.
- Stage O is ready for final sign-off.

## ~~[x] Step 1 — Suggested price on selectable products~~
- **Done** — Job B PASS (`8660d69`). Added `SuggestedPrice` and selectable-product projections.

## ~~[x] Step 2 — Shared product-picker partial + Buy page~~
- **Done** — Job B PASS after correction (`bee5ba9`, `546440f`, `b5df8fe`). Shared picker/script, Buy tag-helper controls, Finished price label, date default.

## ~~[x] Step 3 — Sell page + selected-product round-trip fix~~
- **Done** — Job B PASS (`a7e9419d868b7160f82c69f7cb5129ded402146b`).
- Delivered: Sell uses the shared picker/script; both pages pass `Input.ProductId`; the partial explicitly re-selects the posted product; Sell defaults date to today and labels Price as Finished price; focused test added.
- Evidence: 294/294 tests passed, build had 0 warnings/errors, rendered Buy/Sell POST checks preserved selected product and posted price, and API diff was empty.

## ~~[x] Step 4 — Final Stage O validation~~
- **Done — Reviewer PASS.**
- Actor evidence: clean build/test = 0 warnings, 0 errors, 294/294 tests passed; culture/decimal binding check passed; final Buy and Sell UI checks passed; failed-post product/price preservation and validation messages passed; regression/scope checks passed.
- GitHub Actions run #7 (`36944340305`) for Actor commit `bcfb190c5c3f976e3cb369e6778d69bea0dbffa0`: **completed/success** on GitHub-hosted CI.
- No API, service, entity/schema, PDF, auth, or unrelated implementation changes found.
- **Stage O sign-off: PASS.**

## Global Definition of Done
- Buy and Sell use the shared picker (options partial + script) with page-owned tag-helper controls.
- Validation messages render; posted product and price survive failed posts on both pages.
- Finished price, suggested price, date default, and Back behavior are present on both pages.
- Clean build and full test suite pass; no API/auth/PDF/schema changes.

## Execution notes
- One implementation step → one commit → Job B → next step.
- Completed steps may be compacted to a single outcome/evidence line; do not retain their old implementation instructions once they have passed Job B.
- Step 4 was validation-only and is now signed off by Reviewer.
