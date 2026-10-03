# Audit — Job B (Execution Check): Stage S Step 1

**Reviewed**: `4635012` (`test: verify EF migration chain`) and `c97c15d` (act.md) against Stage S `plan.md` Step 1.  
**Evidence**: the actor's report cites Actions run **#282** for `4635012`: Restore, Build (0 warnings) and Windows PowerShell passed; Ubuntu tests **317 passed, 2 failed** (`Migrations_are_discovered_in_order`, `Chain_applies_to_an_empty_database`). I could not read the run myself (GitHub API rate limit from the sandbox), so the CI numbers were the actor's report at audit time. The code review below is mine.

**Verdict: FAIL — Step 1 is not done (HIGH: the step's own acceptance tests fail in CI). One correction pass is authorized; Step 2 is NOT authorized.**

## Finding

### HIGH — the new Designer is missing `[Migration("20261002060000_UseIntegerRialMoney")]` (and its `using`)
`src/MobileShop.Dal/Migrations/20261002060000_UseIntegerRialMoney.Designer.cs` carries `[DbContext(typeof(AppDbContext))]` but no `[Migration(...)]` attribute and no `using Microsoft.EntityFrameworkCore.Migrations;`. The reference `20261001161450_AddPartNumber.Designer.cs` has both. EF registers a migration through `MigrationAttribute`, so it still sees five migrations: test 1 fails (6 expected) and test 3 fails (history has 5 rows). The actor's diagnosis is correct, and it was right to stop instead of improvising.

**Fix (two lines in one file):** add the using and the attribute under `[DbContext(typeof(AppDbContext))]`, mirroring the AddPartNumber Designer. Nothing else changes.

## What is verified correct
- **The failing tests are doing their job:** this is the exact defect Stage S was created to catch (a migration EF cannot discover while the snapshot already says `int`).
- **Designer body:** `BuildTargetModel` is the current snapshot body (`ProductVersion` 10.0.12, same relational annotations) and the class is `partial class UseIntegerRialMoney`, matching the hand-written `public partial class UseIntegerRialMoney : Migration`.
- **Tests:** `MigrationChainTests` follows the plan: temp-file SQLite with `Pooling=False`, `Dispose` cleans `-wal`/`-shm`, the discovery order lists all six ids, the snapshot test finalizes the snapshot via `IModelRuntimeInitializer` and asserts `HasDifferences` is false, and the empty-DB test checks six history rows, `INTEGER` money columns and both CHECK constraints.
- **Scope:** only the Designer, the new test file, `act.md` and the 12-line `to-do.md`. No production code, snapshot, entity or Api change.
- **Not yet proven:** at audit time, the snapshot test did not report a result in the actor's account, and the CHECK-constraint assertions rely on the exact constraint names; the correction CI run was required to prove both.

## What the actor must do (correction pass — one file, one commit)
1. `git pull`.
2. In `src/MobileShop.Dal/Migrations/20261002060000_UseIntegerRialMoney.Designer.cs`:
   - add `using Microsoft.EntityFrameworkCore.Migrations;` among the usings, as in the reference Designer;
   - add `[Migration("20261002060000_UseIntegerRialMoney")]` directly under `[DbContext(typeof(AppDbContext))]`;
   - optionally add `/// <inheritdoc />` above `BuildTargetModel`, as in the reference Designer.
   Change nothing else: not the tests, the snapshot, the migration SQL or any other file.
3. Do not run `dotnet build`/`dotnet test` locally. Commit (`fix(migrations): register UseIntegerRialMoney with its Migration attribute`, never amend), push, and read the workflow run for the new commit.
4. Expect every test passing, including the three `MigrationChainTests`. If the run still fails, STOP and report the exact failing test names and messages; if `Snapshot_matches_the_current_model` fails, report the differing operations it names. Do not edit the snapshot, the migration SQL or the tests to make them pass.
5. Update `act.md` with the commit hash, `Action: #<run_number> — <Success|Failure|Pending>`, passing/failing tests, limitations and problems, in a separate `docs(act): record Stage S Step 1 correction result` commit. Then STOP for Job B. Do not start Step 2, and do not touch `to-do.md`, `plan.md` or `audit.md`.

## Gate
Step 1 correction → CI run → Job B. Step 2 starts only after Step 1 passes in CI.

## Reviewer checklist
- [x] Job B; one verdict; only `audit.md` written; `to-do.md` untouched.

This audit is committed as requested with:
`git commit -m "docs(audit): Job B FAIL for Stage S Step 1 with correction instructions"`
