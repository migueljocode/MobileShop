# Plan — Stage M — PartNumber

## Reviewer Job B — Step 1

**Status: PASS — Step 1 approved.**

The migration-safety correction now exercises the required production-like path: a Phone is created under the schema immediately before `AddPartNumber`, then the new migration is applied and the same Phone is reloaded with `NULL PartNumberId`.

### Step 1 — Add the PartNumber schema and production-safe migration

- [x] Step 1 — Add `PartNumber`, nullable `Phone.PartNumberId`, EF configuration/DbSet, and one additive EF migration.
- Keep the implementation limited to Models/DAL/migration files plus focused DAL tests.
- Configure the unique index as **(ModelId, Code)** with the existing soft-delete convention.
- Configure Phone → PartNumber as optional with no destructive cascade behavior.
- Verify:
  - PartNumber → Model is many-to-one;
  - Phone → PartNumber is optional;
  - duplicate **Model + Code** is rejected;
  - the same Code under different Models is allowed;
  - an existing Phone survives `AddPartNumber` with ID/IMEI intact and `PartNumberId == null`;
  - migration `Up` contains only additive operations.
- Build: 0 warnings, 0 errors.
- Focused PartNumber tests: 5 passed, 0 failed, 0 skipped.
- Full suite: 256 passed, 0 failed, 0 skipped.
- No API/auth/PDF/database-initialization-policy changes found.
- Step 1 is approved; proceed to Step 2.

## Step 2

- [ ] Step 2 — Add representative development PartNumbers, assign them to existing phone seed rows, and expose list/create operations through the existing Products data-service abstraction.
- One step → commit → Reviewer Job B before Step 3.

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
