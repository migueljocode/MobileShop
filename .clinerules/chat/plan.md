# Plan — Stage M — PartNumber

## Reviewer Job B — Step 3

**Status: PASS — Step 3 approved.**

Step 3 adds the optional PartNumber filter to the Products list while preserving existing type filtering.

### ~~Step 1 — Add the PartNumber schema and production-safe migration~~

- [x] Step 1 — Add PartNumber, nullable Phone.PartNumberId, EF configuration/DbSet, and one additive EF migration.
- Step 1 passed Reviewer Job B and remains approved.

### ~~Step 2 — Seed PartNumbers + list/create operations~~

- [x] Step 2 — Add representative development PartNumbers, assign them to existing phone seed rows, and expose list/create operations through the existing Products data-service abstraction.
- Step 2 passed Reviewer Job B and remains approved.

## ~~Step 3 — Add the PartNumber filter to Products~~

- [x] Step 3 — Add the optional PartNumber filter to the Products list; selected PartNumber shows only matching phone inventory, Apple IDs are excluded, invalid/non-positive IDs behave as no selection, and existing type routes remain unchanged.
- Positive PartNumber IDs filter phone inventory by Phone.PartNumberId.
- An active PartNumber filter excludes Apple IDs.
- Null, zero, and negative PartNumber IDs are treated as no filter.
- Existing all, phone, and appleid type semantics remain intact.
- The Products page exposes a GET dropdown with an “All part numbers” option and preserves the selected PartNumber when switching type routes.
- GetPartNumbersAsync now supports an optional model ID; existing model-scoped behavior remains available.
- Focused ProductsDataService tests: 47 passed, 0 failed, 0 skipped.
- Build: 0 warnings, 0 errors.
- Full suite: 267 passed, 0 failed, 0 skipped.
- No API behavior, authentication, PDF, or database-initialization-policy change was introduced.
- Step 3 is approved; proceed to Step 4.

## Step 4

- [ ] Step 4 — Show PartNumber and SIM options on phone details, with safe N/A handling when the phone has no PartNumber; Apple ID details remain unchanged.
- One step → commit → Reviewer Job B before final Stage M sign-off.

## Stage M Definition of Done

- [ ] All four implementation steps have passed Reviewer Job B.
- [ ] Production migration is additive/non-destructive and verified against an existing-phone database.
- [ ] Existing phone/product IDs and relationships remain stable in development seed data.
- [ ] PartNumber list/create operations are covered by focused tests.
- [ ] Products PartNumber filtering is covered by regression tests.
- [ ] Phone details show PartNumber/SIM options and handle null safely.
- [ ] Full build/test passes.
- [ ] No API, authentication, PDF, or database-initialization-policy changes.
- [ ] Reviewer gives final Stage M sign-off before .clinerules/to-do.md is updated.
