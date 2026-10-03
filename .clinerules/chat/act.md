# Stage S — Step 1 Act Report

## Step
Add the EF migration Designer metadata and the three migration-chain tests for `UseIntegerRialMoney`.

## Implementation
Committed as:
`4635012a73d9ffeb51c7b3f7d2f76d40310430a5` — `test: verify EF migration chain`

Files added:
- `src/MobileShop.Dal/Migrations/20261002060000_UseIntegerRialMoney.Designer.cs`
- `src/MobileShop.Tests/Dal/EfStructures/MigrationChainTests.cs`

The test suite covers:
1. Migration discovery/order.
2. Snapshot/current-model equivalence.
3. Applying the complete migration chain to an empty SQLite database, including migration count, integer money columns, and non-negative constraints.

## CI Verification
GitHub Actions run #282 (`37099923811`) was triggered for commit `4635012a73d9ffeb51c7b3f7d2f76d40310430a5`.

Result: **FAIL**

- Restore: passed.
- Build: passed, 0 warnings/errors.
- Windows PowerShell job: passed.
- Ubuntu test job: 317 passed, 2 failed.
- Failed tests:
  - `Migrations_are_discovered_in_order`
  - `Chain_applies_to_an_empty_database`

## Failure Diagnosis
EF discovered only 5 migrations instead of the expected 6. The new Designer file was created by transforming the snapshot, but it is missing the required migration attribute:

`[Migration("20261002060000_UseIntegerRialMoney")]`

Because EF does not register the migration without that attribute:
- migration discovery returns only the existing 5 migrations;
- applying the chain records only 5 migrations in `__EFMigrationsHistory`.

The snapshot-equivalence test did not report a failure before the test run stopped.

## Status
**Step 1 FAILED CI.**

Per the Stage S execution rule, no corrective implementation or Step 2 work was performed after the failed verification.
