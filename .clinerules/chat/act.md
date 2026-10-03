# Stage S — Step 1 Act Report

## Step
Add the EF migration Designer metadata and the three migration-chain tests for `UseIntegerRialMoney`.

## Initial Implementation
Committed as:
`4635012a73d9ffeb51c7b3f7d2f76d40310430a5` — `test: verify EF migration chain`

The initial CI run #282 failed because the generated Designer was missing EF's `MigrationAttribute`.

## Correction
Committed as:
`9d1e7d0fc6d247b3459bb9a7ce2d987a9ad288b5` — `fix(migrations): register UseIntegerRialMoney with its Migration attribute`

Changed exactly one file:
`src/MobileShop.Dal/Migrations/20261002060000_UseIntegerRialMoney.Designer.cs`

Added:
- `using Microsoft.EntityFrameworkCore.Migrations;`
- `[Migration("20261002060000_UseIntegerRialMoney")]` directly below `[DbContext(typeof(AppDbContext))]`

No tests, snapshot, migration SQL, production code, or other files were changed in the correction.

Per the correction instructions, no local `dotnet build` or `dotnet test` was run.

## CI Verification

**Action: #284 — Success**

Run ID: `37100383021`  
Head SHA: `9d1e7d0fc6d247b3459bb9a7ce2d987a9ad288b5`

Jobs:
- Ubuntu `test`: passed.
- Windows PowerShell log utility: passed.

The full CI run completed successfully. The three Stage S migration-chain tests passed as part of the Ubuntu test job:
- `Migrations_are_discovered_in_order`
- `Snapshot_matches_the_current_model`
- `Chain_applies_to_an_empty_database`

## Limitations / Problems

The original Step 1 implementation failed CI due to the missing migration attribute. That defect was corrected in the authorized one-file correction pass.

The correction CI run passed, so no remaining Step 1 CI problem is recorded.

## Status

**Step 1 correction PASSED CI.**

Job B is complete for this correction pass. Step 2 was not started. No `to-do.md`, `plan.md`, or `audit.md` changes were made.
