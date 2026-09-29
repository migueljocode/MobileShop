# Audit — Job B (Execution Check + Stage Sign-off): Stage B — Home

**Verdict**: **PASS** — three steps, three commits, and Stage B's Definition of Done is met on the
recorded evidence. Signed off in `fa2422b` (`docs(todo): complete Stage B`).
**Supersedes**: the Stage A sign-off verdict (preserved in commit `eb8f827`).

## Step-by-step

| Step | Commit | Verdict | Evidence |
|---|---|---|---|
| 1 — `HomeDataService` + Dal registration | `cb09010` | PASS | Ctor as prescribed (`IBaseRepo<Phone>`, `<AppleId>`, `<Transaction>`, `ILogger<>`); the four stock predicates and the whole `TransactionCardViewModel` projection copied verbatim, including the `PersonNavigation == null ? "Shop"` fallback on both party branches, with `OrderByDescending(card => card.Date).Take(count)` in memory after the projection. Exactly one DI line, in the `else` branch, nothing removed. Tests construct the service via `new BaseRepo<T>(Context)`. Friction disclosed: CS9113 resolved with the `DataServiceBase.cs:14` `Logger` property pattern. |
| 2 — `IndexModel` -> `IHomeDataService dataService` | `6810754` | PASS | One area interface injected as `dataService`, `OnGetAsync` reduced to the two prescribed awaits, both property types and initializers unchanged, `Index.cshtml` untouched, no registration removed. |
| 3 — coverage strengthened + validation | `707b69e` | PASS | `card.ProductLabel` now asserted by exact equality against `ManufacturerNavigation.Name + " " + ModelNavigation.Name`; new fact pins the `count = 20` default and newest-first ordering at both ends; the three `>= 0/1` assertions removed from each of `PhoneDataServiceTests` and `AppleIdDataServiceTests` with no `[Fact]` lost (457 -> 458 discovered). Validation: 456 passed / 0 failed / 2 skipped of 458, `Hosting environment: Production` guard confirmed before any route check, 8/8 routes 200. |

## Definition of Done — met
- `IndexModel` injects exactly one dependency, `IHomeDataService dataService`; no page injects a repo — `6810754`.
- `HomeDataService` registered in the Dal branch of `AddMobileShopDataServices`; `UseApi` still false; the Api stub untouched — `cb09010`.
- No repo or entity-service registration removed (L3 — Stage H does that) — none of the three diffs removes one.
- The four stock numbers and the card projection are behaviourally identical: predicates, argument order, projection and `count` default all verified against the originals, then pinned by exact-equality tests.
- `Index.cshtml` unchanged throughout the stage.
- `HomeDataServiceTests` pins the counts, ordering, limit and `ProductLabel`; no weaker inequality-only duplicate left behind.
- build + suite green, Production served-page pass 200 on all eight routes, database intact.

## Two corrections recorded
1. **`act.md` stated the wrong mechanism.** The Step 3 report attributed the 4,096 -> 299,008 byte growth to
   "EF Core schema materialization on first Production read of an empty placeholder file". Wrong: the schema
   was created and seeded at 2026-09-29 17:46 and had been sitting in a 638,632-byte `MobileShop.db-wal`
   because no connection had closed cleanly since. The Production pass *read* that data, and SQLite
   checkpointed the WAL into the main file on clean shutdown, removing `-wal`/`-shm`. Confirmed by read-only
   query: Products 17, Phones 7, AppleIds 3, Transactions 26, Sellers 3, Customers 4, Manufacturers 6,
   Colors 6, Guarantees 4, SecondHands 2, Users 1. The conclusion (no Development wipe) stood; the stated
   mechanism did not. `act.md` now carries the corrected account with provenance.
2. **The plan's non-destructiveness criterion was unsound, and it was mine.** Step 3 required
   `stat -c '%s %y' MobileShop.db` to be identical before and after the pass. Against a WAL-backed database
   that is unsound: any clean Production pass that reads data checkpoints the WAL and legitimately changes
   size and mtime. It passed in Stage A only by timing luck. **Superseded for Stage C onward:** the guard
   line must say `Production`, the log must show no `InitializeForDevelopment` activity, and a data-level
   check may be added via
   `sqlite3 "file:MobileShop.db?mode=ro" "select (select count(*) from Products), (select count(*) from Phones), (select count(*) from Transactions);"`
   before and after. File size alone proves nothing — a wipe-and-reseed also lands near 299 KB.

## Development database state
Holds the sample data seeded at 2026-09-29 17:46 by the Development run that overrode `--no-launch-profile`,
now checkpointed into the main file (2026-09-30 00:04). Hand-entered rows from before that run are gone —
the design already discards this database on every Development startup. `EnsureAdminUser` is dev-only, so
the admin account's hash state is whatever that run left behind; worth knowing before any Production-mode
login check in a later stage.

## Carried forward
- **Stage C** (Products — three steps, ~700 lines of behaviour, five pages): Apple ID inventory passwords stay
  plaintext; add the single `global using MobileShop.Models.ViewModels.Web.BindModels;` line to
  `src/MobileShop.Services/GlobalUsings.cs` to retire the 12 fully-qualified references; use the superseded-proof
  criterion above in Stage C's validation step.
- `IEmployeeDataService` is still missing from the `if (useApi)` branch and no `ApiEmployeeDataService` stub
  exists (pre-existing since `1450979`, inert while `UseApi` is false) — belongs in the Stage H plan.
- The 15 derived repos and 14 `*RepoTests` still exist; Stage H removes them, keeping `BaseRepoTests`,
  `RepoTestBase` and `TestDataHelpers`.

## Reviewer checklist
- [x] Job identified first — B, and the stage's last step triggered sign-off.
- [x] Verdict written into `.clinerules/chat/audit.md`, overwriting the prior verdict (preserved in `eb8f827`).
- [x] DoD checked against recorded evidence only; nothing heavy re-run.
- [x] Stage ticked only with the DoD evidenced; the sign-off commit (`fa2422b`) contains only `to-do.md`.
- [x] A factual error in an actor report was corrected with provenance rather than silently rewritten.
- [x] No push, amend or reset performed.
