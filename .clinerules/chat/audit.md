# Audit — Job B (Execution Check + Stage Sign-off): Stage A — Foundation

**Verdict**: **PASS** — every step implemented exactly its spec, and Stage A's Global Definition of Done
is met on the recorded evidence. Stage A signed off in `20f6c37` (`docs(todo): complete Stage A`).
**Supersedes**: the Job A plan-review verdict and its correction annex (commit `33674c7`).

## Step-by-step

| Step | Commit | Verdict | Evidence |
|---|---|---|---|
| 1 — `SelectFirstAsync` + drop `abstract` | `c18eb44` | PASS | Diff matches Change 1-4. Original `--filter ~BaseRepoTests` selected 0 tests (abstract base); actor corrected to `~UserRepoTests` -> 36 discovered, the 3 new facts named. |
| 2 — `AddMobileShopRepository` + open generic | `88ee937` | PASS | All 15 per-entity registrations kept, `IBaseRepo<>` -> `BaseRepo<>` appended, XML doc updated. Census 15 + 1. Suite 451/0/2 (453 discovered, enumerated independently). |
| 3 — six area interfaces + two ViewModels | `ff59798` | PASS | 38 members, zero entity types in any signature, no `using` directives, build 0/0. |
| 4 — six Api area stubs | `b23f719` | PASS | 38 throws with the exact `"Api<ClassName> is not implemented yet."` message; no base class (correct — area interfaces are not entity-typed); no HTTP code. |
| 5 — register the Api area pairs | `0166e2e` | PASS | Exactly 6 lines inside `if (useApi)`; the `else` branch byte-identical; no Dal area lines (those types do not exist until Stages B-G). |
| 6 — Stage A validation | none (validation only) | PASS | Build+suite green; `Hosting environment: Production` guard confirmed; 8/8 routes 200; `MobileShop.db` stat identical before and after. |

## Global Definition of Done — met
- `IBaseRepo<T>`/`BaseRepo<T>` expose `SelectFirstAsync`; `BaseRepo<T>` is instantiable — `c18eb44`.
- `AddMobileShopRepository` registers the open generic and still registers the 15 entity repos; every
  pre-existing page resolves and renders — `88ee937` + the 8 served routes.
- Six area interfaces and six Api stubs exist; no interface signature mentions an entity type — `ff59798`, `b23f719`.
- Area Dal/Api pairs register behind `UseApi`, default false — `0166e2e`.
- `dotnet build` + `dotnet test` succeed and the Production served-page pass returns 200 on all eight routes.
- No user-visible behaviour change; `MobileShop.db` untouched by the validation pass.

## Two defects in this plan — both authored by the reviewer, both found and fixed mid-stage
1. **Step 6's chain-2 command was unusable.** `src/MobileShop.Web/Properties/launchSettings.json` sets
   `ASPNETCORE_ENVIRONMENT=Development` for every profile, and `dotnet run` applies it, overriding the
   inline Production assignment. The actor ran the command, the host logged `Hosting environment:
   Development`, and `DatabaseInitializer.InitializeForDevelopment` (EnsureDeleted/EnsureCreated/seed)
   executed. Reported as a blocker and correctly stopped. Fixed by adding `--no-launch-profile`, making
   the Production guard a precondition, and adding the dll-run fallback.
2. **Step 6's non-destructiveness criterion was vacuous.** It relied on `git status --short MobileShop.db`,
   but `.gitignore` excludes `*.db`, `*.db-shm`, `*.db-wal` — git can never see that file, so a destructive
   run would have passed. Replaced with `stat -c '%s %y'` before/after. (`-shm` legitimately changes on any
   connection; `-wal` and the main file did not.)
   Lesson for the next stage's validation step: this pattern is now carried in the Stage B plan verbatim.

## Database disclosure
The pre-correction run reset the development database: `MobileShop.db` went from 299,008 bytes to 4,096,
with the seed data in the 638 KB `-wal` (SQLite replays it on next open). The design already discards this
database on every Development startup, so it was transient by construction — but any hand-entered rows
from before that run are gone. The validation pass itself was verified non-destructive by stat.

## Carried forward
- `MobileShop.Tests` still constructs the 15 derived repos; Stage H deletes them and the 14 `*RepoTests`,
  keeping `BaseRepoTests`, `RepoTestBase` and `TestDataHelpers`.
- `IEmployeeDataService` is not registered in the `if (useApi)` branch and no `ApiEmployeeDataService` stub
  exists (pre-existing since `1450979`; inert while `UseApi` is false). Belongs in the Stage H plan.
- 12 fully-qualified `MobileShop.Models.ViewModels.Web.BindModels.*` references in the area interfaces and
  Api stubs; tidy with one `global using` line when Stage C next touches the Services usings.
- `audit.md` for Steps 2-6 was kept in chat only while sessions were read-only; this file is now the record.

## Reviewer checklist
- [x] Job identified first — B (execution check), with the stage's last step triggering sign-off.
- [x] Verdict written into `.clinerules/chat/audit.md`, overwriting the prior verdict.
- [x] Only the step(s) under review were checked, not later stages.
- [x] Evidence assessed on record (act.md, commits, stat) — nothing heavy re-run.
- [x] Stage ticked only at the last step with the DoD evidenced; only `to-do.md` was committed for it.
- [x] No push/amend/reset performed as part of the sign-off (a later explicit user instruction requested a
      push of the whole branch).
