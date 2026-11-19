# Audit — Job A: plan review (local install and safe updates, Stages 1–7)

Scope: `.agents/chat/plan.md` as handed over on 2026-11-16, against `main` at 7440f2b, including the proposed stages. Checked in the repo: every file and symbol the steps name; the `ConfigureApp` / `TryRunDatabaseCommand` flow and the `EnsureShopParty` call sites; `DatabaseMigrator.EnsureCurrent` and `Migrate`; the `InitializeForDevelopment` callers; `IBaseRepo<>` registration; the `IAccountDataService` / `ProfileModel` conventions; `ShopPartyDataService`; `.github/workflows/dotnet.yml` (triggers, scripts job conditions) and `release.yml` (inputs, jobs, step names, version validation); `publish-smoke.sh` (it migrates before it starts the app) and `production-smoke.sh` (the migration count is computed, not hard-coded); the Actor, Planner and Reviewer rules. The facts the plan states about the published build were reproduced on the real v0.0.0 linux-x64 binary during planning.

## Findings

### HIGH

1. **plan.md Steps 1–6, "Commit message (exact)" vs actor.md "Commit the step".** The Actor commits with `-m "<type>(<scope>): <description>" -m "Implements Step N of plan.md."` and edits the step heading in `plan.md`; the plan prescribed messages without that trailer. Evidence: actor.md, "Commit the step". Risk: the Actor stalls on a plan/rules conflict or drops the trailer. Fix: every message keeps the plan's subject and bullets and ends with `Implements Step N of plan.md.`; rule 10 in plan section 0.5 records the bookkeeping.
2. **plan.md Step 7 vs the six-commit rule.** The Actor protocol ticks a step heading with a commit; Step 7 has no commit, so a tick before the merge would add a seventh commit to the branch. Evidence: actor.md step-tick rule, plan 0.5 (exactly six commits). Fix: Steps 1–6 tick inside their own commits (`plan.md` is part of each); Step 7's tick and the `TODO.md` ticks are ordinary docs commits on `main` after the merge.

### MEDIUM

- Step 2: the Development guard sat inside `InitializeForDevelopment`, which `SampleDataSeedTests.cs` calls directly while the static flag may be true in the test process; call the guard from `ConfigureApp` instead.
- Step 3: the service contract was under-specified for an Actor that must not design public APIs; pin `ShopProfileView`, `ShopProfileInput`, `Task<ServiceResult> SaveAsync` and the shop-party lookup (`ShopLabels.ZoneMobile`).
- Step 3: the Development branch does not ensure the shop party today; run only the profile ensure there. The `IShopPartyDataService` doc comment still says `Pdf:ShopPhone` and must change.
- Step 4 CI: the PowerShell step has no `if` and would run on the new macOS leg; add `if: runner.os != 'macOS'`. Do not rename the job (required checks).
- Step 4: `ROOT` of the installed copy must come from the script's own path, or a service started with a custom `MOBILESHOP_ROOT` breaks.
- Steps 4 and 5 are large: add an internal work order so a smaller model can proceed incrementally with local shell tests.
- Step 6: document the behaviour change (a published layout no longer creates a DB at first start) and the one-time sign-out after Step 1.
- Template: steps lacked `Depends on`.

### LOW

- Step 5 S6: assert the refusal message, not only the exit code.
- 0.5 rule 5 vs Step 7: the content-identical rebase is the only allowed rewrite of frozen commits; say so.
- `--backup-database` and `mobileshop backup` were not requested by the owner; small and reusing `CreateVerifiedBackup`, so kept.
- Until a release contains `install.sh`, `update` and the README one-liner cannot work; the plan already leaves cutting that release to the owner (Step 7).

## Missing Implementation Details

None left open; each item above is covered by a correction.

## Corrections applied by the Planner before commit

HIGH 1–2 and every MEDIUM and LOW item were applied to plan.md in the same session. They clarify or tighten and change no scope, decision or architecture. The `## Proposed stages` section was removed and its seven lines are `TODO.md`. For an independent second look at the corrected plan, request `review` again.

## Proposed stages

Simple, in dependency order (app changes, installer, pipeline, docs, merge); nothing missing against D1–D8, the six-commit rule and the merge rule.

## Status

APPROVED WITH CORRECTIONS
