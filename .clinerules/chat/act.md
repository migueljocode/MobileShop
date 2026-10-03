# Stage S — Step 2 Act Report

## Step
Legacy-upgrade test: verify data preservation and fractional-money rounding when upgrading from `20261001161450_AddPartNumber` to the latest migration.

## Initial Implementation

Commit:
`9fc17df278b6b367d3d7460e499f2dbe20d3da6f` — `test: verify legacy money upgrade`

Initial CI Action #288 failed at build because `IMigrator` was missing its namespace import.

## Correction

Correction commit:
`8cc7a67ba8f89001b041469390dda3d1a1c2db30` — `test: fix legacy money upgrade test compile and assertions`

Only `src/MobileShop.Tests/Dal/EfStructures/LegacyMoneyUpgradeTests.cs` was changed. The correction:
- added `using Microsoft.EntityFrameworkCore.Migrations;`;
- replaced the nullable `PRAGMA foreign_key_check` scalar assertion with a count assertion;
- changed both CHECK-constraint assertions to `Assert.Equal(1L, ...)`.

No production code, migration, snapshot, or other test was changed.

No local `dotnet build` or `dotnet test` was run.

## CI Verification

**Action: #290 — Success**

Run ID: `37101311917`  
Head SHA: `8cc7a67ba8f89001b041469390dda3d1a1c2db30`

Jobs:
- Windows PowerShell log utility: passed.
- Ubuntu `test`: passed.

Ubuntu verification:
- Build: 0 warnings, 0 errors.
- .NET tests: **320 passed, 0 failed, 0 skipped**.
- Bash comprehensive tests: passed.
- PowerShell tests: passed.
- The full .NET test run therefore included the new legacy-upgrade test and passed.

## Status

**Step 2 correction PASSED CI. Step 2 is closed.**

The legacy-upgrade scenario compiled and the complete test suite passed. Step 3 was not started.

## Limitations / Problems

The initial Step 2 implementation failed to compile and required the authorized correction pass. After the correction, CI passed with 320/320 .NET tests passing.

No remaining Step 2 CI problem was observed.


# Stage S — Step 3 Act Report

## Step
Explicit backed-up --migrate-database command and read-only Production startup guard.

## Initial Implementation

Commit:
3e6bbdfde6c4ca8649b2c03391e931fe5d71a783 — feat: add explicit database migration command and production guard

Action: #294 — Failure

The Ubuntu build failed before tests ran. The Windows PowerShell job passed.

Build errors included:
- unescaped "notnull" inside the interpolated SQL string in DatabaseMigrator.cs;
- undefined SchemaColumn.

## Correction

Correction commit:
b3e0254dc380e8a7dc10c5c392a76251d24f06b1 — fix(migrations): compile DatabaseMigrator and harden the migrate command

No local dotnet build or dotnet test was run.

The correction added the requested schema-column materialization type, empty-file handling, outcome logging, snapshot product-version lookup, broader migration-command exception handling, and the empty-file test.

## CI Verification

Action: #296 — Failure
Run ID: 37103464059

- Windows PowerShell log utility: passed.
- Ubuntu test: failed during build; tests were skipped.
- Restore: passed.

Exact Ubuntu build failures:
- DatabaseMigrator.cs(29,20), (41,24), (47,20), (58,20), (72,16): ILogger has no LogInformation extension method available.
- DatabaseMigrator.cs(215,23), (216,26), (220,30): AppDbContext does not expose the GetService extension used by the correction.
- One EF1002 warning was also emitted for the interpolated raw SQL; the build failed because of the eight errors.

## Status

Step 3 correction FAILED CI. Step 3 is not closed. Step 4 is not authorized.

The authorized correction pass is exhausted by the CI failure. No further Step 3 correction was made after Action #296.

## Limitation / Problem

The correction still requires the appropriate logging and EF infrastructure access namespaces/API usage before it can compile. Per the Job B gate, no Step 4 work was started.
