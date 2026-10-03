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
