# Stage S — Step 2 Act Report

## Step
Legacy-upgrade test: verify data preservation and fractional-money rounding when upgrading from `20261001161450_AddPartNumber` to the latest migration.

## Implementation
Committed as:
`9fc17df278b6b367d3d7460e499f2dbe20d3da6f` — `test: verify legacy money upgrade`

Created:
`src/MobileShop.Tests/Dal/EfStructures/LegacyMoneyUpgradeTests.cs`

The test is intended to:
- create a SQLite database through `AddPartNumber`;
- insert two products and two transactions with fractional legacy prices using raw SQL;
- migrate to the latest schema;
- verify row counts, rounded values, INTEGER column types, foreign-key integrity, SQLite integrity, and both non-negative money constraints.

No production code or migration SQL was changed.

## CI Verification

**Action: #288 — Failure**

Run ID: `37100680734`  
Head SHA: `9fc17df278b6b367d3d7460e499f2dbe20d3da6f`

Jobs:
- Windows PowerShell log utility: passed.
- Ubuntu `test`: failed during build.

The failure occurred before the test suite executed:

`src/MobileShop.Tests/Dal/EfStructures/LegacyMoneyUpgradeTests.cs(21,66): error CS0246: The type or namespace name 'IMigrator' could not be found (are you missing a using directive or an assembly reference?)`

Build result:
- 0 warnings
- 1 error

## Status

**Step 2 FAILED CI.**

Per the Stage S execution rule, STOP after the failed verification. No corrective implementation was made and Step 3 was not started.

## Limitations / Problems

The failure is a test-file compilation issue: `IMigrator` is referenced without the namespace import used by the existing migration tests. The CI run therefore did not execute the new legacy-upgrade test, so data preservation and rounding remain unverified.

The plan requires the failing statement/test outcome to be recorded and does not authorize changing migration SQL or tests to make a failing migration scenario pass without the appropriate correction pass.
