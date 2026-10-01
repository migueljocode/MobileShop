# Plan — Stage M — PartNumber

## Reviewer Job B — Step 1

**Status: BLOCKED — implementation needs a migration-safety test correction before approval.**

Step 1's schema design and generated migration are otherwise aligned with the approved plan. The blocker is specifically the required production-safety verification: the submitted test does **not** migrate an existing database containing a Phone.

### Step 1 — Add the PartNumber schema and production-safe migration

- [ ] Step 1 — Add `PartNumber`, nullable `Phone.PartNumberId`, EF configuration/DbSet, and one additive EF migration.
- Keep the implementation limited to Models/DAL/migration files plus focused DAL tests.
- Configure the unique index as **(ModelId, Code)** with the existing soft-delete convention.
- Configure Phone → PartNumber as optional with no destructive cascade behavior.
- Test:
  - PartNumber → Model is many-to-one;
  - Phone → PartNumber is optional;
  - duplicate **Model + Code** is rejected;
  - the same Code under different Models is allowed;
  - **an existing Phone row survives applying the new migration and has NULL PartNumberId**.
- Inspect migration `Up`/`Down` for destructive operations.
- Re-run build, focused DAL tests, and full suite after the correction.
- **Reviewer Job B must pass before Step 2.**

### Required correction

The current migration test deletes the database and runs `Migrate()` before creating the Phone. That proves a fresh schema can hold a null PartNumberId, but it does **not** prove the new migration preserves a pre-existing Phone.

The test must instead:

1. create a database at the schema version immediately before `AddPartNumber`;
2. insert and persist a Phone;
3. apply `AddPartNumber`;
4. reload the Phone;
5. assert the same Phone/IMEI remains and `PartNumberId == null`.

Do not mark Step 1 complete or proceed to Step 2 until this passes.

### Actor workflow note

The Step 1 commit also modified `.clinerules/chat/plan.md` and changed the checkbox into the invalid `[~~x~~]` form. Per project workflow, the Actor must not edit the planning/audit/checklist files. This is recorded as a workflow violation; no rework is required for that violation, but the Reviewer is restoring the plan here.

## Step 2

- [ ] Step 2 — Add representative development PartNumbers, assign them to existing phone seed rows, and expose list/create operations through the existing Products data-service abstraction.

**Do not start Step 2 while Step 1 is blocked.**

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
