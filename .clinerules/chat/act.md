# Act — Stage M Step 1 — PartNumber schema and migration

## Commit (follow-up fix)
- `62a7b75` — test(dal): fix PartNumber migration-safety test to use pre-migration schema
  - Resolves the Reviewer Job B BLOCKED finding from the audit.

## What changed (this commit)
Only `src/MobileShop.Tests/Dal/EfStructures/PartNumberEfTests.cs` — the migration-safety test.

### Migration test correction
The original test ran `EnsureDeleted()` + `Migrate()` before creating the Phone, which only proved the final schema can hold a null `PartNumberId` — not that an existing Phone survives the migration.

The corrected test:
1. Creates a database at the pre-`AddPartNumber` schema by applying all prior migrations via `IMigrator.Migrate(targetMigrationId)` for each of `Initial`, `AddPhoneColor`, `NormalizeCatalog`, `AddEmployeeEntity`.
2. Inserts a Phone via raw SQL under the pre-migration schema (no `PartNumberId` column).
3. Confirms `GetPendingMigrations()` returns exactly `AddPartNumber`.
4. Applies `Migrate()`.
5. Reloads the Phone and asserts `Id` and `IMEI1` are unchanged, `PartNumberId == null`, and `PartNumberNavigation == null`.
6. Cleans up the temp SQLite file.

## Verification
- `dotnet build src/MobileShop.slnx --nologo` → 0 warnings, 0 errors
- `dotnet test ... --filter "FullyQualifiedName~PartNumberEfTests"` → 5 passed, 0 failed, 0 skipped
- Full suite (`--no-build`) → 256 passed, 0 failed, 0 skipped

## Limitations
None.

## Friction noted
- Full test suite (~37s) exceeds 30s tool timeout; ran in background and polled.

## Problems
None — migration-safety test corrected and passing.

## Status
Fix committed. Awaiting Reviewer Job B re-check.
