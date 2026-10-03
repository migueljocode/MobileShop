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


# Stage S — Step 3 Second Correction Act Report

## Correction

Commit:
7673a2b35fd839097e82e4577f7899bbacd26cf8 — fix(migrations): add missing usings and parameterise the schema query

No local dotnet build or dotnet test was run, per the audit instruction.

The correction was limited to src/MobileShop.Dal/Initialization/DatabaseMigrator.cs:
- added file-local Microsoft.EntityFrameworkCore.Infrastructure and Microsoft.Extensions.Logging usings;
- replaced the interpolated pragma_table_info query with the requested parameterised SqlQueryRaw form;
- made no other code changes.

## CI Verification

Action: #298 — Success
Run ID: 37109046164
Head SHA: 7673a2b35fd839097e82e4577f7899bbacd26cf8

Jobs:
- Windows PowerShell log utility: passed.
- Ubuntu test: passed.
- Restore: passed.
- Build: passed with no reported build failure/warning.
- .NET tests: passed.
- Bash log utility tests: passed.
- PowerShell log utility tests: passed.
- Factor PDF inspection artifact upload: passed.

## Status

The Stage S Step 3 second correction passed its CI gate. Step 4 remains unauthorized pending Job B review.


# Stage S — Step 4 Act Report

## Step
CI Production smoke (non-destructive proof).

## Implementation

Commit:
f8525af2b629d53611542c6d453b2a095fb6ea89 — ci: add production database smoke test

Changed only:
- .github/scripts/production-smoke.sh
- .github/workflows/dotnet.yml

No production code or tests were changed.

## CI Verification

**Action: #302 — Success**
Run ID: 37116102870
Head SHA: f8525af2b629d53611542c6d453b2a095fb6ea89

Ubuntu test job:
- Restore: passed.
- Build: passed.
- .NET tests: passed.
- Bash log utility tests: passed.
- PowerShell log utility tests: passed.
- Factor PDF inspection artifact upload: passed.
- Production smoke: passed.
- Production smoke artifact upload: passed.

Windows PowerShell log utility job: passed.

The smoke exercised the Development-created legacy-style database, verified Production startup refusal with --migrate-database guidance, ran the explicit migration command, verified six migration-history rows and a single backup, then verified the requested Production routes returned 200 while the recorded data fingerprint remained unchanged.

Artifacts:
- production-smoke: created successfully.
- factor-pdf-inspection: created successfully.

## Status

**Step 4 passed its CI gate. Job B is now stopped here. Step 5 was not started.**


# Stage S — Step 5 Act Report

- Implementation commit: 0baf6c9f55a29b407c8171ba7278aaddf42bb365 (ci: harden production smoke and document database upgrades).
- Action: **#306 — Success**
- Run ID: 37117860688
- Head SHA: 0baf6c9f55a29b407c8171ba7278aaddf42bb365
- Workflow conclusion: Success.

## Jobs and steps

- **test — Success:** checkout, .NET setup, Restore, Build, Test, Bash log utility test, PowerShell log utility test, factor PDF artifact upload, Run Production smoke — Success, Upload Production smoke artifacts — Success, cleanup, and job completion all passed.
- **PowerShell log utility (Windows) — Success:** checkout, PowerShell log utility test, cleanup, and job completion all passed.
- **Hardened Run Production smoke — Success:** the smoke step completed successfully with the new local-delete guard, 120-second command timeouts, migration baseline-log assertion, and Production error-level log scan in place.
- **production-smoke artifact — uploaded successfully:** artifact present, 22,175 bytes, not expired. The separate factor-pdf-inspection artifact was also present and not expired.

## Stage-wide scope check

Requested comparison: git diff --stat 0bfaabc..HEAD -- . ':!.clinerules'.

The equivalent GitHub commit comparison for 0bfaabcb5d8448024bdcab4d889d29b2371fb7c8..0baf6c9f55a29b407c8171ba7278aaddf42bb365 reports:

.github/scripts/production-smoke.sh | 168 +++++++++++++++++++
.github/workflows/dotnet.yml | 18 +++
README.md | 25 +++-
src/MobileShop.Dal/Initialization/DatabaseMigrator.cs | 259 ++++++++++++++++++++
src/MobileShop.Dal/Migrations/20261002060000_UseIntegerRialMoney.Designer.cs | 1525 +++++++++++++++++++++
src/MobileShop.Tests/Dal/EfStructures/LegacyMoneyUpgradeTests.cs | 135 +++++++++
src/MobileShop.Tests/Dal/EfStructures/MigrationChainTests.cs | 121 ++++++++
src/MobileShop.Tests/Dal/Initialization/DatabaseMigratorTests.cs | 246 ++++++++++++++
src/MobileShop.Web/Extensions/WebApplicationBuilderExtensions.cs | 45 +++++
src/MobileShop.Web/Program.cs | 10 +-

No files under src/MobileShop.Api, entities, authentication, Development initialization, Razor pages, or services appear in this comparison.

## Limitations / problems

- The workflow result was not independently available to the reviewer earlier because of GitHub API rate limiting; it is now verified from the workflow run API.
- No code correction was required. The Stage U cleanup note remains as requested: remove the personal Stage 3 note and literal &amp; from README.md; those changes were not made in Step 5.


# Stage T — Step 1 Act Report

## Step
Widen money to long end to end, add the no-op WidenMoneyToLong migration, update the EF snapshot/migration chain to seven migrations, and keep the Stage S smoke history assertion at seven.

## Implementation

Commit:
1da44be2894728278fe007c255c7f29bd8a4cc92 — feat: widen money to long

The implementation changed the planned money-bearing entities, web/PDF view models, bind models, reporting/distribution signatures, PDF DTO, snapshot, migration Designer/new migration, migration-chain/migrator assertions, report page model, and Production smoke migration-history assertion. No API host, authentication, DatabaseInitializer, EF configuration, or prior migration was changed.

No local dotnet build or dotnet test was run.

## CI Verification

**Action: #313 — Failure**
Run ID: 37122821381
Head SHA: 1da44be2894728278fe007c255c7f29bd8a4cc92

Jobs:
- Windows PowerShell log utility: Success.
- Ubuntu test: Failure at Build.

Exact build failure:
src/MobileShop.Services/PDF/Configuration/QuestPdfGenerator.cs(475,29): CS0266 — Cannot implicitly convert type 'long' to 'int'.

Build reported 0 warnings, 1 error.

## Status

**Step 1 is NOT closed.** The CI gate failed, so Step 2 is not authorized.


# Stage T — Step 1 Correction Act Report

## Correction

Correction commit:
81927e98340e1765f53f6ab9b47a6989a5ca80bd — fix: widen Persian invoice presentation price to long

The authorized correction changed only src/MobileShop.Services/PDF/Configuration/QuestPdfGenerator.cs, widening PersianInvoicePresentation.FinishedPrice from int to long. No other production/test/migration/snapshot/workflow/script/plan/audit/to-do file was changed. No local dotnet build or dotnet test was run.

## CI Verification

**Action: #316 — Failure**
Run ID: 37123756678
Head SHA: 81927e98340e1765f53f6ab9b47a6989a5ca80bd

Jobs:
- PowerShell log utility (Windows): Success.
- Ubuntu test: Failure at Test.
- Build: Success — 0 warnings, 0 errors.
- .NET tests: Failure — 5 failed, 325 passed, 0 skipped, 330 total.

Failed tests:
1. DatabaseMigratorTests.Empty_database_file_is_created_and_migrated_without_backup: expected 7 migrations, actual 6.
2. DatabaseMigratorTests.Fresh_database_is_created_and_migrated_without_backup: expected 7 migrations, actual 6.
3. LegacyMoneyUpgradeTests.Legacy_money_upgrade_preserves_rows_and_rounds_fractions: expected 1500001, actual 1500000.
4. MigrationChainTests.Migrations_are_discovered_in_order: expected the seven-migration chain including 20261002060000_UseIntegerRialMoney, but the actual discovered chain omitted that migration and contained only six migrations including 20261004090000_WidenMoneyToLong.
5. MigrationChainTests.Chain_applies_to_an_empty_database: expected 7 migrations, actual 6.

The CI evidence showed the correction fixed compilation, but the Step 1 implementation did not preserve the required seven-migration chain.

## Status

**Step 1 is NOT closed.** The authorized correction pass failed its CI gate. Step 2 remains not authorized.


# Stage T — Step 1 Migration-Chain Correction Act Report

## Correction

Correction commit:
31bfade35817faef0c6cf5ff96fd78e6099f68e1 — fix(migrations): restore migration chain Designer metadata

The correction restored the original 20261002060000_UseIntegerRialMoney.Designer.cs, preserving its original migration identity and pre-Step-1 target model. The existing UseIntegerRialMoney.cs, WidenMoneyToLong.cs, and current widened WidenMoneyToLong.Designer.cs were not changed. No local dotnet build or dotnet test was run.

## CI Verification

**Action: #319 — Failure**
Run ID: 37125640782
Head SHA: 31bfade35817faef0c6cf5ff96fd78e6099f68e1

Jobs:
- PowerShell log utility (Windows): Success.
- Ubuntu test: Failure at Test.
- Build: Success — 0 warnings, 0 errors.
- .NET tests: Failure — 2 failed, 328 passed, 0 skipped, 330 total.

Failed tests:
1. DatabaseMigratorTests.Legacy_migration_database_is_backed_up_before_upgrade: expected 6 migrations, actual 7.
2. DatabaseMigratorTests.Ensure_created_database_is_baselined_after_verified_backup: expected 6 migrations, actual 7.

The CI result confirmed the migration-chain Designer correction restored the required seventh migration; the remaining failures were stale test expectations.

## Status

**Step 1 is NOT closed.** The authorized migration-chain correction pass reached CI but failed because two migrator tests still expected 6 migrations. Step 2 remains NOT authorized.


# Stage T — Step 1 Final Correction Act Report

## Correction

Implementation commit:
9fb1d374bac734f0e5f6f9c3814ef84c2cc15f44 — test: fix remaining migrator history expectation

The authorized correction updated the remaining stale migration-history expectation in DatabaseMigratorTests.cs from 6 to 7. No local dotnet build or dotnet test was run.

## CI Verification

**Action: #325 — Success**
Run ID: 37128953318
Head SHA: 9fb1d374bac734f0e5f6f9c3814ef84c2cc15f44

Jobs:
- **PowerShell log utility (Windows): Success.**
- **Ubuntu test: Success.**
- **Restore: Success.**
- **Build: Success — 0 warnings, 0 errors.**
- **.NET tests: Success — 330 passed, 0 failed, 0 skipped, 330 total.**
- **Bash log utility test: Success.**
- **PowerShell log utility test: Success.**
- **Factor PDF inspection artifact upload: Success.**
- **Production smoke: Success.**
- **Production smoke artifact upload: Success.**

Production smoke evidence included:
- Production startup initially refused the legacy database with the required `--migrate-database` guidance.
- The explicit migration command successfully baselined the legacy database and created a verified backup.
- The final production smoke check passed.

## Action #326

Action #326 (run ID 37129575983) was triggered by the documentation commit that records this act report. It completed successfully. It is **not** a new Stage T Step 1 implementation/correction CI gate; Action #325 is the relevant verification of commit `9fb1d374bac734f0e5f6f9c3814ef84c2cc15f44`.

## Status

**Stage T Step 1 PASSED its CI gate and is closed. Step 2 is authorized.**

The remaining migrator history expectation was corrected, and the complete CI pipeline passed with 330/330 .NET tests passing and the production smoke succeeding.
