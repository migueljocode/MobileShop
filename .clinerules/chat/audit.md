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
