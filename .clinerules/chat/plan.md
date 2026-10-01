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

## Step 4 — Show PartNumber and SIM options on phone details

- [ ] Step 4 — Update the existing phone-details flow so a phone's PartNumber information is displayed when a PartNumber exists, while phones without one render safely as N/A.

### Actor instructions

- Work only in the existing phone-details page/model/data-service flow. Do not create a new page, endpoint, or parallel details mechanism.
- Extend the existing phone-details data projection only as needed to expose PartNumber code, SupportsDualSim, and SupportsEsim.
- Reuse the Stage M EF PartNumber navigation. Do not add a duplicate relationship or separate lookup abstraction.
- A null Phone.PartNumberId must be fully supported. Loading a phone with no PartNumber must never throw.
- When PartNumber exists, show its code, Dual SIM capability, and eSIM capability.
- When PartNumber is null, show N/A for the PartNumber and SIM-option information. Do not display null as false.
- Preserve the distinction between an existing PartNumber with a false capability and no PartNumber: real false values remain false/no according to the existing UI convention; null remains N/A.
- Keep all existing phone-detail fields, labels, navigation, authorization, and behavior unchanged unless a minimal internal model change is required.
- Apple ID details must remain unchanged. Do not add PartNumber/SIM fields to Apple ID details.
- Do not add PartNumber selection to Create Phone. Assignment for newly created phones belongs to Stage N.
- Do not change Product/Phone creation or editing, Transactions, People, API behavior, authentication, PDF behavior, or development database initialization.
- Follow the existing UI convention for boolean values instead of inventing a new global convention.

### Required tests

- Add/update focused phone-details tests covering:
  1. Phone with PartNumber shows the PartNumber code.
  2. The same phone shows the correct SupportsDualSim value.
  3. The same phone shows the correct SupportsEsim value.
  4. Phone with PartNumberId null loads successfully and shows N/A for PartNumber/SIM information.
  5. Existing PartNumber with false capabilities is not treated as null; actual false values are preserved.
  6. Apple ID details remain unchanged and do not gain PartNumber/SIM fields.
- Explicitly test the null relationship. A populated PartNumber test alone is insufficient.
- Reuse existing fixtures/helpers/test style; do not introduce broad new test infrastructure.

### Workflow and scope rules

- Do not edit .clinerules/chat/plan.md, .clinerules/chat/audit.md, or .clinerules/to-do.md to mark progress. The Reviewer owns those files.
- Do not combine another roadmap step with Step 4.
- Make exactly one implementation commit for Step 4, then stop and wait for Reviewer Job B.
- Do not mark Step 4 complete yourself.
- Preserve existing contracts and architecture; if an ambiguity appears, report it rather than making unrelated design changes.

### Handoff validation

Before handing the commit to Reviewer Job B:
- Run focused phone-details tests and report exact passed/failed/skipped counts.
- Run the full test suite and report exact passed/failed/skipped counts.
- Run the full build and report exact warning/error counts.
- Confirm Apple ID details remain unchanged.
- Confirm no API/authentication/PDF/database-initialization-policy changes were made.
- Report the single implementation commit SHA and changed files.
- Do not call the step approved; Reviewer Job B decides PASS/FAIL.

## Stage M Definition of Done

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
