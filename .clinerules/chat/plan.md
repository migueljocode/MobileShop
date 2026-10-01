# Plan — Stage M — PartNumber

## Reviewer Job B — Step 2

**Status: PASS — Step 2 approved.**

Step 2 is complete: representative PartNumbers are seeded and linked to existing phone inventory, and the existing Products data-service abstraction exposes list/create operations.

### ~~Step 1 — Add the PartNumber schema and production-safe migration~~

- [x] Step 1 — Add `PartNumber`, nullable `Phone.PartNumberId`, EF configuration/DbSet, and one additive EF migration.
- Step 1 passed Reviewer Job B and remains approved.

## ~~Step 2 — Seed PartNumbers + list/create operations~~

- [x] Step 2 — Add representative development PartNumbers, assign them to existing phone seed rows, and expose list/create operations through the existing Products data-service abstraction.
- Seeded four PartNumbers with stable IDs 1–4 for phone models 1, 3, 6, and 16.
- Assigned PartNumbers to existing phone rows 1, 3, 4, and 7; phones 2, 5, and 6 remain intentionally unassigned.
- Existing Product/Phone IDs and other seed relationships remain unchanged.
- Added PartNumber loading/seeding and FK-safe destructive reseed ordering.
- Added `GetPartNumbersAsync(modelId)` and `CreatePartNumberAsync(...)` to `IProductsDataService` and the DAL implementation.
- Preserved the API stub contract with matching `NotImplementedException` members.
- Create operation trims codes, rejects blank codes, rejects unknown models, and reuses an existing Model + Code.
- Focused Step 2 coverage: 5 new ProductsDataService tests plus seed-count coverage.
- Build: 0 warnings, 0 errors.
- Full suite: 262 passed, 0 failed, 0 skipped.
- No API behavior, authentication, PDF, or database-initialization-policy change was introduced.
- Step 2 is approved; proceed to Step 3.

## ~~Step 3~~

- ~~Step 3 — Add the optional PartNumber filter to the Products list; selected PartNumber shows only matching phone inventory, Apple IDs are excluded, invalid/non-positive IDs behave as no selection, and existing type routes remain unchanged.~~
- One step → commit → Reviewer Job B before Step 4.

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
- [ ] Reviewer gives final Stage M sign-off before `.clinerules/to-do.md` is updated.
