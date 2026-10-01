# Audit — Stage M — PartNumber

## Reviewer Job B — Step 1

**Status: BLOCKED**

### What passed

The submitted Step 1 implementation matches the approved data model in the important areas:

- `PartNumber` belongs to one `Model`; `Model` has many PartNumbers.
- `Phone.PartNumberId` is nullable.
- Multiple phones can share one PartNumber.
- Uniqueness is scoped to **Model + Code** and respects the soft-delete filter.
- Phone → PartNumber uses `NoAction`; no destructive cascade was introduced.
- `AppDbContext` exposes `PartNumbers`.
- The generated `AddPartNumber` migration is additive in `Up`: nullable Phone column, new table, indexes, and FKs; no drop/delete/alter operation appears in `Up`.
- Build reported 0 warnings / 0 errors.
- Focused PartNumber tests reported 5 passed / 0 failed / 0 skipped.
- Full suite reported 256 passed / 0 failed / 0 skipped.

### Blocking finding

The required migration-safety test is not testing the required scenario.

The test named `Migration_preserves_existing_phone_with_null_part_number` first executes:

```
Context.Database.EnsureDeleted();
Context.Database.Migrate();
```

and only **after the migration** creates the Phone.

Therefore it does not establish that a Phone created under the previous schema survives the `AddPartNumber` migration. It only establishes that the post-migration schema permits a Phone with a null PartNumberId.

This fails the explicit Stage M Job B requirement to verify the generated migration against a database containing an existing Phone.

### Required actor change

Replace the migration test with a genuine pre-migration scenario:

- bring SQLite to the previous migration/schema;
- create and save an existing Phone;
- apply `AddPartNumber`;
- reload the same Phone;
- assert its ID and IMEI are unchanged and `PartNumberId` is null.

Then rerun the focused DAL tests and full suite.

### Workflow violation

The Step 1 actor commit also edited `.clinerules/chat/plan.md` and changed the checkbox to `[~~x~~]`. This violates the standing workflow rule that the Actor must not edit `plan.md`, `audit.md`, or `.clinerules/to-do.md` to mark progress.

The Reviewer has restored the plan. No implementation rework is requested for this violation.

### Gate

**Step 2 is not approved. Step 1 remains open until the migration-safety test is corrected and Job B passes.**

No `.clinerules/to-do.md` change is made.
