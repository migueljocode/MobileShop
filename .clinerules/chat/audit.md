# Audit — Job B (Execution Check): Stage S Step 2 (`9fc17df`)

**Verdict: FAIL — Step 2 was not done initially; one correction pass was authorized.**

## Findings

### HIGH — missing `using Microsoft.EntityFrameworkCore.Migrations;`
`IMigrator` was referenced without the required namespace import, causing Action #288 to fail during build.

### HIGH — nullable `PRAGMA foreign_key_check` assertion
The original `Assert.Empty(Scalar<string>(..., "PRAGMA foreign_key_check;"))` could receive null when the pragma returned no rows.

### LOW — CHECK constraint assertions
The original `Assert.True(... == 1)` assertions hid the actual value on failure.

## Correction

Correction commit:
`8cc7a67ba8f89001b041469390dda3d1a1c2db30` — `test: fix legacy money upgrade test compile and assertions`

Only `src/MobileShop.Tests/Dal/EfStructures/LegacyMoneyUpgradeTests.cs` was changed.

## CI Gate

Action **#290** — **Success**  
Run ID: `37101311917`

- Windows PowerShell log utility: passed.
- Ubuntu `test`: passed.
- Build: 0 warnings, 0 errors.
- .NET tests: **320 passed, 0 failed, 0 skipped**.
- Bash comprehensive tests: passed.
- PowerShell tests: passed.

The legacy-upgrade test therefore passed as part of the full .NET suite.

**Gate: Step 2 correction PASSED. Step 2 is closed. Step 3 is authorized by the CI gate.**

Actor report recorded separately in `act.md` commit:
`ef83abd934e5ffb5b637adcda14f350237040898`.

Per the correction instructions, `to-do.md` and `plan.md` were not changed.


# Audit — Job B (Execution Check): Stage S Step 2 correction (`8cc7a67`)

**Verdict: PASS. Step 2 is closed; Step 3 is authorized.**

- **Fix:** exactly the three changes from the previous audit in `LegacyMoneyUpgradeTests.cs`: the `Microsoft.EntityFrameworkCore.Migrations` using; the foreign-key assertion replaced by `SELECT COUNT(*) FROM pragma_foreign_key_check` = 0; both CHECK-constraint assertions as `Assert.Equal(1L, …)`. No other file changed.
- **CI evidence (actor-reported):** `Action: #290 — Success` for `8cc7a67`; Ubuntu 320 passed / 0 failed / 0 skipped with 0 warnings, Windows PowerShell and the Bash/PowerShell script tests passed. The legacy-upgrade test therefore proves rows are preserved and fractional prices are rounded when upgrading from `AddPartNumber`.
- **MEDIUM/LOW:** none.

## Gate
Next: the actor does **Stage S Step 3** (explicit backed-up `--migrate-database` command and read-only Production startup guard; HIGH risk). One step → one commit → report `Action: #<run_number>` → STOP for Job B.


# Audit — Job B (Execution Check): Stage S Step 3 (`3e6bbdf`)

**Evidence**: `act.md` still holds the Step 2 report, so there is **no Step 3 report and no `Action: #<run_number>`** for `3e6bbdf`. I could not read the run myself (GitHub API rate limit). The verdict below comes from reading the code.

**Verdict: FAIL — Step 3 does not compile (two HIGH findings in `DatabaseMigrator.cs`). One correction pass is authorized; Step 4 is NOT authorized.**

## Findings

### HIGH — unescaped quotes inside the interpolated SQL
`DatabaseMigrator.cs:186` contains an unescaped `"notnull"` inside a C# string.

### HIGH — `SchemaColumn` is not defined
`SqlQueryRaw<SchemaColumn>(...)` references an undefined type.

### MEDIUM
An existing empty database file should be treated like a fresh database and migrated, not refused.

### LOW
The logger is ignored; the command catches only `InvalidOperationException`; baseline product version should come from the migration snapshot when available.

## Correction authorized

- Correct the two HIGH compile errors.
- Handle an existing zero-table file as a fresh database.
- Add outcome logging.
- Use snapshot `ProductVersion` with fallback.
- Broaden command exception handling.
- Update the empty-file test.
- No local build/test.
- Correction commit must be:
`fix(migrations): compile DatabaseMigrator and harden the migrate command`
- After CI, update `act.md` separately with:
`docs(act): record Stage S Step 3 result`
- Do not touch `to-do.md`, `plan.md`, or this audit after this audit commit.

## Gate
Step 3 correction → CI run → Job B. Step 4 starts only after Step 3 passes in CI.



# Audit — Job B (Execution Check): Stage S Step 3 second correction (7673a2b)

**Verdict: PASS — Step 3 second correction passed CI. Step 4 remains NOT authorized pending the Job B gate.**

## Evidence

- Correction commit: `7673a2b35fd839097e82e4577f7899bbacd26cf8`
- Action **#298** — **Success**
- Run ID: `37109046164`
- Ubuntu `test`: passed.
- Windows PowerShell log utility: passed.
- Restore: passed.
- Build: passed.
- .NET tests: passed.
- Bash log utility tests: passed.
- PowerShell log utility tests: passed.

The authorized second correction added the two required file-local usings and parameterised the schema query. No further code correction was required.

## Gate

**Stage S Step 3 second correction PASSED CI. Step 3 is eligible for closure by Job B. Step 4 is NOT authorized by this audit.**

Per the execution workflow, no Step 4 work was started.


# Audit — Job B: Stage S Step 3 second correction

**Verdict: PASS. Step 3 is closed; Step 4 is authorized.**

- Correction `7673a2b35fd839097e82e4577f7899bbacd26cf8` contains exactly the instructed changes.
- Action #298 — Success.
- Restore, build, .NET tests including DatabaseMigratorTests, Bash and PowerShell log tests, PDF artifact upload, and Windows PowerShell job passed.
- No remaining MEDIUM/LOW findings.

## Gate

Stage S Step 4 is authorized: CI Production smoke using `.github/scripts/production-smoke.sh` plus its workflow step. One implementation commit, one CI gate, then STOP for Job B.


# Audit — Job B (Execution Check): Stage S Step 4 (`f8525af`)

**Verdict: PASS. Step 4 is closed; Step 5 is authorized.**

- **CI evidence (actor-reported):** Action **#302 — Success** for `f8525af`; restore, build, .NET tests, Bash and PowerShell log tests, the Production smoke step and both artifact uploads passed, plus the Windows PowerShell job. The run was not independently readable because of GitHub API rate limiting.
- **What the script proves:** a Development-seeded `EnsureCreated` database (no history) is refused by Production startup with `--migrate-database` guidance; `--migrate-database` baselines it with exactly one backup and six history rows; seven routes return 200 in Production; the row-count fingerprint is unchanged after migration and after Production startup; no second backup appears.
- **Scope:** only `.github/scripts/production-smoke.sh` and `.github/workflows/dotnet.yml` (plus `act.md`); no production code or tests changed.

## Step 5 findings carried forward

1. **MEDIUM — local-run data loss:** add a guard refusing to run unless `CI=true` or `MOBILESHOP_SMOKE_ALLOW_DELETE=1` because the script deletes repo-root database/log artifacts.
2. **MEDIUM — possible CI hang:** wrap the foreground Production guard and `--migrate-database` invocations in `timeout 120`, treating exit 124 as failure, and set `timeout-minutes: 10` on the smoke step.
3. **LOW — stronger evidence:** grep the migration log for `Legacy database baselined successfully`; fail if the Production log contains `[ERR]`, `[FTL]`, `fail:`, `crit:`, or `Unhandled exception`.

## Gate

Next: the actor does **Stage S Step 5** (smoke hardening, README documentation, and final validation). One step → one commit → report `Action: #<run_number>` → STOP for Job B.


# Audit — Job B (Execution Check): Stage S Step 5 (`0baf6c9`)

**Verdict: PENDING — final workflow evidence must be recorded in `act.md`.**

- Implementation reviewed as correct: local-run deletion guard, 120-second command timeouts, 10-minute smoke-step timeout, stronger migration/Production log evidence, and the Production database upgrade README section.
- Stage-wide scope reviewed from `0bfaabc..HEAD`: only the planned Stage S files changed; no API, entities, authentication, Development initialization, Razor pages, or services changes.
- Required next action: record the final workflow result for `0baf6c9` in `act.md`, including Action number/run ID/head SHA, every job/step result, the hardened Production smoke result and `production-smoke` artifact, and the requested scope diff-stat output.
- If the run failed, report the exact failing step/output and stop. Do not loosen the Production log pattern.

## LOW — carry to Stage U (cleanup sweep)

- Remove the personal note in `README.md` about being in Stage 3 and asking Claude to verify it.
- Remove the literal `&amp;` in the first sentence of `README.md`.


# Audit — Job B (Execution Check): Stage S Step 5 and Stage S sign-off

**Verdict: PASS. Stage S is complete and signed off (ticked in `to-do.md`).**

- **Evidence (actor-recorded, `act.md`):** `Action: #306 — Success` for `0baf6c9` (run `37117860688`): restore, build, all .NET tests, Bash and PowerShell log tests, the hardened **Run Production smoke** step, both artifact uploads and the Windows PowerShell job all passed; the `production-smoke` artifact is present (22,175 bytes). The hardened script ran green with the local-delete guard, the 120-second timeouts, the baseline-log assertion and the Production error-log scan.
- **Stage-wide scope (checked independently):** `0bfaabc..HEAD` touches only the 10 planned files; nothing under `src/MobileShop.Api`, entities, authentication, Development initialization, Razor pages or services.
- **Stage S outcome:** the six migrations are discoverable and match the snapshot, the chain applies to an empty database, a legacy-shaped database upgrades with rows preserved and fractions rounded, `--migrate-database` creates/upgrades/baselines with a verified backup, normal Production startup is a read-only schema guard, and CI proves non-destructive startup (run #306). From now on DB/migration/CI claims in a review cite the run number.
- **MEDIUM/LOW:** none open. The README personal note and the literal `&amp;` are already recorded for Stage U.

## Gate
Next: the **planner** plans **Stage T — IRR money foundation** (`to-do.md` is ticked for Stage S). Decided inputs: money widens to `long`; Stage S's migration, legacy-upgrade and smoke checks must stay green; the overflow-boundary tests live in Stage T.


# Audit — Job B: Stage T plan review and Step 1 authorization

**Verdict: PASS. The Stage T plan is coherent and Step 1 is authorized.**

## Review

- The owner decision to widen money to `long` is consistent with the identified overflow risks: the current factor total is an `int`, and the distribution calculation performs percentage multiplication in `int` arithmetic.
- The planned Step 1 scope correctly covers the two money-bearing entities, the listed view/bind models, reporting/distribution/PDF paths, snapshot, migration chain, and the CI history-count assertions.
- The planned SQLite migration is appropriately a no-op schema migration: SQLite represents both CLR `int` and `long` money columns as 64-bit INTEGER, while the migration records the CLR model change for EF's snapshot/chain.
- The plan preserves the existing EF configuration/check constraints, API host, authentication, Development initialization policy, and prior `UseIntegerRialMoney` migration.
- The Designer requirements learned from Stage S are explicit: migration using, `[DbContext(typeof(AppDbContext))]`, `[Migration("20261004090000_WidenMoneyToLong")]`, and a `BuildTargetModel` matching the updated snapshot.
- The Stage T boundary strategy is appropriate: Step 1 establishes the wider type end-to-end; Step 2 then proves persistence, factor totals, reports, distribution and create-flow calculations across `int.MaxValue`.
- The IRR-unit, seed-data and integer-input work is correctly separated into later steps, reducing Step 1 behavioral scope.
- The plan explicitly requires CI as the gate and forbids local `dotnet build`/`dotnet test`, matching the established workflow.

## Step 1 gate

**Authorized:** Stage T Step 1 — Widen money to `long` end to end (no behaviour change).

Execution constraints:
- One implementation commit, then push and read the resulting GitHub Actions run.
- Do not run local `dotnet build` or `dotnet test`.
- Do not touch `to-do.md`, `plan.md`, or `audit.md` during implementation/reporting.
- Do not touch API, authentication, `DatabaseInitializer`, EF configuration, `UseIntegerRialMoney`, or unrelated migrations.
- Preserve the exact planned migration id and Designer requirements.
- Record the no-`int`-money search and CI evidence in `act.md` after the gate as instructed.
- Stop after the Step 1 CI gate; Step 2 is not authorized by this audit.

**Carry-forward note:** D4 (existing Production rows are assumed to already be Rials; no automatic ×10 conversion) remains a planner assumption to be revisited only if the owner overrules it.

# Audit — Job B: Stage T Step 1 correction authorization (`1da44be`)

**Verdict: FAIL — Step 1 implementation does not compile; one narrow correction pass is authorized. Step 2 remains NOT authorized.**

## Finding

### HIGH — Persian invoice presentation money type remains `int`

Action **#313** / run **37122821381** failed during Ubuntu Build with:

`src/MobileShop.Services/PDF/Configuration/QuestPdfGenerator.cs(475,29): error CS0266: Cannot implicitly convert type 'long' to 'int'`

The failing path is `ResolvePersian(InvoiceViewModel model)`: `InvoiceViewModel.FinishedPrice` was widened to `long`, while `PersianInvoicePresentation.FinishedPrice` remains `int`. The corresponding `InvoicePresentation.FinishedPrice` is already `long`.

## Correction authorized

Make exactly this mechanical correction:

- Change `PersianInvoicePresentation.FinishedPrice` from `int` to `long` in `src/MobileShop.Services/PDF/Configuration/QuestPdfGenerator.cs`.
- No other production, test, migration, snapshot, workflow, script, plan, audit, or to-do changes.
- Do not run local `dotnet build` or `dotnet test`.
- Use one implementation commit with message:
  `fix: widen Persian invoice presentation price to long`
- Push the correction and use its GitHub Actions run as the Step 1 correction gate.
- After the CI gate, update `act.md` separately with the correction result.
- Step 2 must not start regardless of the correction result.

## Gate

**Authorized:** one Stage T Step 1 correction pass only.
**Next:** correction commit → CI → actor report in `act.md` → STOP for Job B.


# Audit — Job B: Stage T Step 1 migration-chain correction authorization

**Verdict: FAIL — Step 1 remains open; one narrowly scoped migration metadata correction is authorized. Step 2 remains NOT authorized.**

## Finding

### HIGH — the existing UseIntegerRialMoney migration Designer was renamed/replaced

Action **#316** / run **37123756678** failed in the Ubuntu test job after the build passed.

The migration chain discovered six migrations instead of the required seven, and the legacy-money upgrade test failed because 20261002060000_UseIntegerRialMoney was no longer discovered/applied. The Stage T plan explicitly requires preserving UseIntegerRialMoney and all prior migrations.

The implementation incorrectly moved the existing 20261002060000_UseIntegerRialMoney.Designer.cs metadata onto 20261004090000_WidenMoneyToLong.Designer.cs. This removed the Designer registration for the existing migration.

## Correction authorized

1. Restore src/MobileShop.Dal/Migrations/20261002060000_UseIntegerRialMoney.Designer.cs as the Designer for the existing 20261002060000_UseIntegerRialMoney migration.
   - Preserve its original migration identity: [Migration("20261002060000_UseIntegerRialMoney")]
   - Preserve its existing BuildTargetModel for the pre-Step-1 model.
   - Preserve [DbContext(typeof(AppDbContext))] and the required migrations namespace.
2. Keep src/MobileShop.Dal/Migrations/20261002060000_UseIntegerRialMoney.cs unchanged.
3. Keep src/MobileShop.Dal/Migrations/20261004090000_WidenMoneyToLong.cs unchanged.
4. Create/fix src/MobileShop.Dal/Migrations/20261004090000_WidenMoneyToLong.Designer.cs so it is the Designer for only 20261004090000_WidenMoneyToLong, with [Migration("20261004090000_WidenMoneyToLong")], and its BuildTargetModel matching the current snapshot with the widened money properties.
5. Do not change migration IDs, migration operations, snapshot, EF configuration, tests, workflow, scripts, API/authentication, DatabaseInitializer, plan, to-do, or any unrelated migration.
6. Both Designer files must exist side-by-side; no rename of the existing UseIntegerRialMoney migration is permitted.
7. Do not run local dotnet build or dotnet test.

## Commit and gate

- Use one implementation commit with exact message:
  fix(migrations): restore migration chain Designer metadata
- Push the correction and use its GitHub Actions run as the Step 1 correction gate.
- After the CI gate, update act.md separately with the correction result.
- Step 2 must not start regardless of the correction result.

## Gate

**Authorized:** one Stage T Step 1 migration-chain correction pass only.
**Next:** correction commit → CI → actor report in act.md → STOP for Job B.


# Audit — Job B: Stage T Step 1 migrator-test correction authorization

**Verdict: FAIL — Step 1 remains open; one narrowly scoped test-expectation correction is authorized. Step 2 remains NOT authorized.**

## Finding

Action **#319** / run **37125640782** reached the test gate with a clean build (**0 warnings, 0 errors**) and **328 passed / 2 failed / 0 skipped**.

The migration-chain correction successfully restored the required seven-migration chain. The only remaining failures are two existing `DatabaseMigratorTests` whose expected migration-history count is still 6. With the restored `UseIntegerRialMoney` Designer, the actual chain is correctly 7.

Failed tests:
1. `DatabaseMigratorTests.Legacy_migration_database_is_backed_up_before_upgrade`: expected 6, actual 7.
2. `DatabaseMigratorTests.Ensure_created_database_is_baselined_after_verified_backup`: expected 6, actual 7.

## Correction authorized

The actor may make exactly this narrow correction:

1. In `src/MobileShop.Tests/Dal/Initialization/DatabaseMigratorTests.cs`, update the two stale migration-history assertions above from **6** to **7**.
2. Do not alter production code, migration files, Designer files, snapshot, EF configuration, workflow, scripts, API/authentication, DatabaseInitializer, plan, to-do, or unrelated tests.
3. Do not change test logic beyond the two expected migration-count literals required by Action #319.
4. Do not run local `dotnet build` or `dotnet test`.
5. Use one implementation commit with exact message:
   `test: update migrator history expectations for seven migrations`
6. Push the correction and use its GitHub Actions run as the Step 1 correction gate.
7. After the CI gate, update `act.md` separately with the result.
8. Step 2 must not start regardless of the correction result.

## Gate

**Authorized:** one Stage T Step 1 DatabaseMigrator test-expectation correction pass only.
**Next:** correction commit → CI → actor report in `act.md` → STOP for Job B.
