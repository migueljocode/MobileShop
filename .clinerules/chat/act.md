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
