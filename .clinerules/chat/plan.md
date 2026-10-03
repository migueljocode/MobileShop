# Plan — Stage S — Trusted verification workflow (CI evidence for what cannot be checked by reading code)

## Proposed stages
(new phase — they go into `to-do.md` only after the reviewer approves; Stage S is planned below)
- [ ] Stage S — Trusted verification workflow (CI evidence for what cannot be checked by reading code): CI-run checks that the migration chain is discoverable, matches the model snapshot, applies to an empty DB and upgrades a legacy-shaped DB with data preserved; an explicit backed-up `--migrate-database` command plus a read-only startup guard for Production; a CI Production smoke that proves startup is non-destructive. From now on every DB/migration/CI claim in a review cites the workflow run number
- [ ] Stage T — IRR money foundation (needs Stage S): the whole app speaks IRR (Rial) — unit label/indicator on every page, factor PDF and report; all seeded sample data in Rials; integer-only price inputs (no decimals/precision, thousands separators for display only); money widened to `long` (decided: prices can reach 1,000,000,000 Rials or more) across Product.Price, Transaction.FinishedPrice, input/view models, report and factor sums, with an `AlterColumn<long>` migration and overflow-boundary tests; percentages stay decimal
- [ ] Stage U — Cleanup sweep (needs Stage T): re-verify and fix known leftovers — the `Phone`-category filter in `CreatePhoneAsync`'s model lookup (planner default: keep it and restrict the phone model dropdown to Phone-category models so the form is consistent, unless the owner overrules), stale comments naming deleted types, over-indented lines, missing trailing newlines, empty-namespace global usings, dead code and unused usings, README/docs/`.github/copilot-instructions.md` drift against the real structure (Repo rename, Scripts location, CI), zero skipped tests and a 0-warning clean build
- [ ] Stage V — Products list: Available / Sold filter (live auto-apply like the Transactions filters; derive availability from transactions, not a new column unless the plan proves it necessary; keep the part-number and existing filters working together)
- [ ] Stage W — Transactions list sorting: click-to-sort headers on Price, Seller, Customer, Date and Product (asc/desc, keep filters and paging, server-side, sort indicator)
- [ ] Stage X — Persian (Shamsi/Jalali) dates and factor polish: one shared Jalali date formatter (reused later by the UI polish), factor PDF dates in Shamsi, small typography/spacing/alignment polish keeping the Persian/RTL and English tests green
- [ ] Stage Y — Searchable person picker + create-person popup on Record Sell and Record Buy (needs Stage U): the customer (Sell) and seller (Buy) selects become typeable comboboxes that search by name, phone number or national code; "create new" opens a polished modal with the create-person form, saves via fetch/AJAX and selects the new person without leaving the page; one shared component and endpoint used by both pages
- [ ] Stage Z — Searchable product picker + create-product popup on Record Buy (needs Stage Y; also upgrade the Sell product select): product combobox in the same style; a "Create product" button opens a modal that first asks the product type in a combobox, then loads that type's create form inside the modal (phone, Apple ID and glass first); a registry so every later product type plugs in by registering its create form; the new product is selected on save
- [ ] Stage AA — Device product pages (needs Stage Z, V and W; the entities already exist): Tablet, Smart watch and Laptop — services, create/details/list pages and Products-list integration following the phone/glass rules (computed price, guarantee and second-hand notes, availability filter, sorting, part-number where it applies), registered in the create-product popup, with tests
- [ ] Stage AB — Accessory product pages (needs Stage AA): Cable, Charger, Power bank, Portable storage and Case (entities already exist; Case keeps its model fits) following the glass rules — bulk count in one submit, same pricing rules, registered in the create-product popup, with tests
- [ ] Stage AC — UI/UX overhaul planning (needs every page above): ask Claude Sonnet 5.5 at maximum effort, using Anthropic's frontend-design skill, to write a complete `to-do.md` and `plan.md` that takes the current functional UI to an industry-level UI/UX (design system and tokens, Persian RTL and English typography, components including the combobox/modal/table patterns from Stages W–Z, page-by-page redesign, responsive, accessibility, empty/error/loading states), ordered into stages and steps that a reviewer can verify
- [ ] Stage AD — UI/UX overhaul implementation (needs Stage AC): hand the full plan from Stage AC to ChatGPT to implement stage by stage under the same planner → actor → reviewer workflow and CI evidence rules; the stages Claude writes in Stage AC replace this line when they are added

## Requirements
- Functional
  - The six migrations must be discoverable by EF and apply cleanly, in order, to (a) an empty database and (b) a legacy database built at `20261001161450_AddPartNumber` that holds fractional prices.
  - Production gets one explicit, backed-up way to create or upgrade the schema; normal Production startup only checks the schema and refuses to run on a stale one (never mutates).
  - CI proves all of this and the proof is cited by workflow run number in later reviews.
- Non-functional: no data loss, backup created and verified before any schema change, fail-fast with an actionable message, evidence reproducible in CI.
- Constraints (project-specific-rules.md): API untouched, no auth changes, Apple ID inventory passwords stay plaintext, development initialization stays destructive and unchanged (`EnsureDeleted` + `EnsureCreated` + seed), EF configuration stays centralized, project-wide usings stay in `GlobalUsings.cs`, no `bin`/`obj` changes.

## Assumptions and decisions (labelled)
- **D1 (owner, decided):** prices can reach 1,000,000,000 Rials or more, so `int` is not enough. The widening to `long` is Stage T. Stage S keeps the current `int` model; its fixtures must stay within `int` range, and the overflow-boundary tests belong to Stage T (the earlier draft listed them in Stage S; they move because the fix lives in T).
- **D2 (owner unsure → planner default):** the `Phone`-category filter question is recorded in Stage U above; nothing in Stage S touches it.
- **A1:** `DatabaseInitializer.InitializeForDevelopment` uses `EnsureDeleted()` + `EnsureCreated()`, so migrations are not exercised in Development; only `SampleDataInitializer.DropAndCreateDatabase` uses `Migrate()`. Migration drift is therefore invisible today — Steps 1–2 close that gap. Switching Development to `Migrate()` is OUT OF SCOPE.
- **A2:** Production currently has no provisioning at all (`ConfigureApp` only acts when `IsDevelopment()`); the real shop database may have been created by `EnsureCreated` (no `__EFMigrationsHistory`). Step 3 handles that case explicitly.
- **A3:** GitHub Actions is the build/test gate (`.clinerules/chatbot_skill.md` §8). The actor does not run `dotnet build`/`dotnet test` locally; each step is verified by the pushed commit's workflow run, reported as `Action: #<run_number> — <Success|Failure|Pending>`.

## Reviewer Briefing
- **Step 3 is HIGH risk / MEDIUM confidence:** first time startup behaviour changes (a read-only schema guard in Production) and the first code that copies/modifies a real database file. Three database states matter: fresh, with history, and legacy without history.
- **Step 1 is MEDIUM / MEDIUM:** `20261002060000_UseIntegerRialMoney.cs` has no `[Migration]`/`[DbContext]` attributes and no Designer file, so EF cannot discover it while the snapshot already says `int`. The fix is a hand-built Designer whose `BuildTargetModel` equals the current snapshot (SQLite `AlterColumn` rebuilds tables from the target model). The CI tests are the arbiter, not the reading of the code.
- **Step 4 is MEDIUM / LOW:** shell + process orchestration in CI can only be proven by running it.
- **Invariant change, explicit and approved by this plan:** normal Production startup now checks the schema and refuses to start when it is stale. It still performs no writes; schema changes happen only through `--migrate-database`.
- **Reviewer rule from this stage on:** DB/migration/CI claims in Job B cite the Actions run number. (The owner may add this sentence to `reviewer.md`; the actor does not edit rule files.)

## ~~[x] Step 1 — Make `UseIntegerRialMoney` discoverable and add the migration-chain tests~~
- Files
  - inspect: `src/MobileShop.Dal/Migrations/20261002060000_UseIntegerRialMoney.cs`, `20261001161450_AddPartNumber.Designer.cs` (format reference), `AppDbContextModelSnapshot.cs`, `src/MobileShop.Tests/Dal/BaseClass/SqliteRepoTestBase.cs`, `src/MobileShop.Tests/Dal/EfStructures/PartNumberEfTests.cs`
  - create: `src/MobileShop.Dal/Migrations/20261002060000_UseIntegerRialMoney.Designer.cs`, `src/MobileShop.Tests/Dal/EfStructures/MigrationChainTests.cs`
  - modify: none (leave the hand-written migration file as it is)
  - do not touch: the snapshot, entities, other migrations, EF configuration, `src/MobileShop.Api`
- Symbols
  - `partial class UseIntegerRialMoney` in namespace `MobileShop.Dal.Migrations` gets `[DbContext(typeof(AppDbContext))]`, `[Migration("20261002060000_UseIntegerRialMoney")]` and `protected override void BuildTargetModel(ModelBuilder modelBuilder)` whose body is the current `AppDbContextModelSnapshot.BuildModel` body (same `ProductVersion` annotation, same relational annotations). Copy mechanically; follow the `AddPartNumber` Designer layout.
  - `MigrationChainTests` (xUnit, real SQLite file in a temp directory, connection string with `Pooling=False`, file deleted in `Dispose`).
- Current → Desired: EF sees five migrations and the money migration never runs; the snapshot and the migrations can drift unseen → EF sees all six in order, the snapshot matches the model, and the chain applies to an empty DB.
- Tests (`MigrationChainTests`)
  1. `Migrations_are_discovered_in_order`: `context.GetService<IMigrationsAssembly>().Migrations.Keys` equals the six migration ids in timestamp order, ending with `20261002060000_UseIntegerRialMoney`.
  2. `Snapshot_matches_the_current_model`: finalize the snapshot model (`IMigrationsAssembly.ModelSnapshot.Model` through `IModelRuntimeInitializer`) and assert `context.GetService<IMigrationsModelDiffer>().HasDifferences(snapshotRelationalModel, context.GetService<IDesignTimeModel>().Model.GetRelationalModel())` is `false`.
  3. `Chain_applies_to_an_empty_database`: `Database.Migrate()`; `GetPendingMigrations()` is empty; `__EFMigrationsHistory` has six rows; `pragma_table_info` reports `INTEGER` for `Products.Price` and `Transactions.FinishedPrice`; the `CK_Products_Price_NonNegative` and `CK_Transactions_FinishedPrice_NonNegative` constraints exist in `sqlite_master`.
- Edge cases / error handling: if test 2 fails for a reason other than the Designer, do NOT regenerate the snapshot or other migrations; report the exact differing operations in `act.md` as a blocker.
- Verify: push the commit and read the workflow run for it (`GET /repos/{owner}/{repo}/actions/runs?head_sha=<sha>`); report `Action: #<run_number>`. Expect the existing suite plus 3 new tests passing.
- Done when: the Designer exists, the three tests pass in CI, and no other file changed.
- Risk: MEDIUM
- Confidence: MEDIUM

## ~~[x] Step 2 — Legacy-upgrade test: data preserved, fractions rounded~~
- Files
  - inspect: `AddPartNumber`/`NormalizeCatalog` migration SQL (NOT NULL columns, FKs, CHECK constraints), `Initialization/sample-data.json` shapes for realistic values
  - create: `src/MobileShop.Tests/Dal/EfStructures/LegacyMoneyUpgradeTests.cs`
  - modify / do not touch: no production code
- Symbols: `IMigrator.Migrate("20261001161450_AddPartNumber")` (target = the last schema before the money migration), then `Database.Migrate()` to latest; rows inserted with raw SQL (`ExecuteSqlRaw`) because the old schema has `decimal(18,2)` columns the current model cannot write.
- Current → Desired: the rounding statements and the `AlterColumn` rebuild have never run against data → a test proves they preserve every row and round fractions.
- Change (test only): build the temp file DB at the pre-money migration; insert the smallest valid graph (Manufacturer, Category, Model, Product(s), the persons needed for a Transaction, Transaction(s)) with `Products.Price` = `800000.75` and `1500000.5`, `Transactions.FinishedPrice` = `799999.49` and `1200000.5`; migrate to latest; assert counts unchanged, `Products.Price` = 800001 / 1500001, `FinishedPrice` = 799999 / 1200001 (SQLite `ROUND` rounds half away from zero), column types `INTEGER`, `PRAGMA foreign_key_check` returns no rows, `PRAGMA integrity_check` is `ok`, both CHECK constraints still exist.
- Edge cases: keep fixtures within `int` range (D1); satisfy FK and CHECK constraints in the raw inserts; do not use `SampleDataInitializer` (it targets the latest schema).
- Verify: CI run number as in Step 1; expect the new test(s) passing.
- Done when: the legacy-upgrade test passes in CI without production code changes. If it fails, report the exact SQLite error and the failing statement in `act.md`; do not patch the migration without a plan change.
- Risk: MEDIUM
- Confidence: MEDIUM

## ~~[x] Step 3 — Explicit backed-up migration command and read-only Production startup guard~~
- Files
  - inspect: `src/MobileShop.Web/Program.cs`, `Extensions/WebApplicationBuilderExtensions.cs` (`ConfigureApp`), `src/MobileShop.Dal/Initialization/DatabaseInitializer.cs`, `EfStructures/SolutionPaths.cs`, `AppDbContext`
  - create: `src/MobileShop.Dal/Initialization/DatabaseMigrator.cs`, `src/MobileShop.Tests/Dal/Initialization/DatabaseMigratorTests.cs`
  - modify: `src/MobileShop.Web/Program.cs`, `src/MobileShop.Web/Extensions/WebApplicationBuilderExtensions.cs`
  - do not touch: `DatabaseInitializer.InitializeForDevelopment`, `SampleDataInitializer`, the Development branch of `ConfigureApp`, `src/MobileShop.Api`, authentication
- Symbols
  - `DatabaseMigrator.Migrate(AppDbContext context, string databaseFile, ILogger logger)` returns a result (`Created`, `UpToDate`, `Upgraded` with backup path, `Baselined` with backup path) and throws `InvalidOperationException` with an actionable message when it refuses.
  - `DatabaseMigrator.EnsureCurrent(AppDbContext context)` is read-only: throws `InvalidOperationException("The database schema is not current. Back up MobileShop.db and run: dotnet run --project src/MobileShop.Web -- --migrate-database")` when the file is missing, there is no migration history, or migrations are pending; returns normally otherwise.
  - `WebApplicationBuilderExtensions.TryRunDatabaseCommand(this WebApplication app, string[] args)` returns `true` after handling `--migrate-database` (prints the result, sets exit code 0, or 1 on refusal).
- Current → Desired: Production has no way to create/upgrade a schema and silently runs on whatever file exists → one explicit command does it safely, and normal Production startup refuses a stale schema.
- Change
  1. `Program.cs`: build the app, `if (app.TryRunDatabaseCommand(args)) return;` before `ConfigureApp`, then `ConfigureApp().Run()`; keep it minimal.
  2. `ConfigureApp`: in the non-Development branch run `DatabaseMigrator.EnsureCurrent` on a scoped `AppDbContext` before the pipeline is mapped. Development behaviour is byte-for-byte unchanged.
  3. `DatabaseMigrator.Migrate` states:
     - **No file:** create it with `Database.Migrate()` (no backup needed) → `Created`. This is allowed only through the explicit command, so a wrong path at normal startup never silently creates an empty shop.
     - **Has `__EFMigrationsHistory`:** if there are no pending migrations → `UpToDate`, no backup. Otherwise back up first, then `Migrate()` → `Upgraded`.
     - **Tables but no history (legacy `EnsureCreated` database):** build a reference schema by calling `EnsureCreated` on a temporary file from the current model and compare a sorted fingerprint of both (`table|column|declared type|notnull|pk` from `pragma_table_info`, plus index names). If identical → back up, create the history table with `IHistoryRepository.GetCreateScript()` and insert every migration id (`GetInsertScript`) → `Baselined`. If different → refuse with the first 20 differing lines and no writes.
  4. Backup: `<databaseFile>.<yyyyMMddHHmmss>.bak` next to the database, created with SQLite's online backup (`SqliteConnection.BackupDatabase`), verified by opening it and checking `PRAGMA integrity_check = ok`; abort the operation if the backup fails or the target name exists.
- Edge cases: WAL/shm files (use the backup API, not a file copy); database in use; command run twice (second run → `UpToDate`, no new backup); Development environment (command still works against the same file; `EnsureCurrent` is never called in Development).
- Tests (`DatabaseMigratorTests`, temp-file SQLite): fresh → `Created`, six history rows, no backup; current → `UpToDate`, no backup file; migrated only to `AddPartNumber` → `Upgraded`, backup exists and still has the old schema, pending list empty afterwards; `EnsureCreated` database → `Baselined`, history has six rows, backup exists, row data identical; `EnsureCreated` database plus an extra column → throws, database unchanged, no history table created; `EnsureCurrent` throws for missing file / no history / pending and passes for a current database.
- Verify: CI run number as before; expect all new tests passing and the rest of the suite unchanged.
- Done when: the command and the guard exist, Development is unchanged, and the tests pass in CI.
- Risk: HIGH
- Confidence: MEDIUM

## ~~[x] Step 4 — CI Production smoke (non-destructive proof)~~
- Files
  - inspect: `.github/workflows/dotnet.yml`, `src/MobileShop.Scripts/tests/` (style reference), Razor page routes
  - create: `.github/scripts/production-smoke.sh`
  - modify: `.github/workflows/dotnet.yml` (new step or job after build/test; keep the existing steps and the PDF artifact upload)
  - do not touch: production code, tests
- Behaviour of the script (`set -euo pipefail`, a `trap` that kills the app and removes nothing outside the workspace):
  1. Start the Web app detached with `ASPNETCORE_ENVIRONMENT=Development`, `ASPNETCORE_URLS=http://127.0.0.1:5099`, `dotnet run --no-build --no-launch-profile --project src/MobileShop.Web`; wait up to 60 s for `/` = 200; stop it. This leaves a seeded `EnsureCreated` database (no history) at the repo root `MobileShop.db` — the legacy case.
  2. Record a fingerprint with `sqlite3` (row counts of People, Products, Phones, AppleIds, Transactions, Categories plus `PRAGMA integrity_check`).
  3. Start with `ASPNETCORE_ENVIRONMENT=Production`: expect a non-zero exit and a message containing `--migrate-database` (the guard refuses a no-history database).
  4. Run `dotnet run --no-build --no-launch-profile --project src/MobileShop.Web -- --migrate-database` with Production: expect exit 0, exactly one `MobileShop.db.*.bak`, six rows in `__EFMigrationsHistory`, fingerprint unchanged.
  5. Start in Production again and request `/`, `/Products`, `/Products/SecondHand`, `/Transactions`, `/People/Customers`, `/People/Sellers`, `/Reports/ProfitLoss`: all 200; stop; fingerprint unchanged; still exactly one `.bak` (normal startup writes nothing).
  6. Upload the fingerprints, the `.bak` listing and `MobileShop.Log/` as artifact `production-smoke`.
- Edge cases: port in use, app crashing on start (print the log tail and fail), `sqlite3` availability (preinstalled on `ubuntu-24.04`; otherwise install it in the step), running only on `ubuntu-24.04`.
- Verify: CI run number; the step is green and the artifact is attached.
- Done when: the smoke step passes in CI and its artifact shows unchanged fingerprints.
- Risk: MEDIUM
- Confidence: LOW

## [ ] Step 5 — Smoke hardening, docs and final Stage S validation
- Files: modify `.github/scripts/production-smoke.sh`, `.github/workflows/dotnet.yml`, `README.md`; no other file.
- **Smoke hardening (carried from the Step 4 Job B; do all three in this step):**
  1. *Local-run guard (MEDIUM):* the script deletes the repo-root `MobileShop.db`, its `-wal`/`-shm`, `MobileShop.db.*.bak` and `MobileShop.Log`, which on a developer machine is the real database. At the top, refuse to run (print what it would delete, exit 1) unless `CI=true` or `MOBILESHOP_SMOKE_ALLOW_DELETE=1`. GitHub Actions sets `CI=true`, so CI behaviour does not change.
  2. *Hang protection (MEDIUM):* the foreground Production guard invocation and the `--migrate-database` invocation run `dotnet run` with no time limit, so a regression where the guard stops refusing would hang the job until the platform timeout. Wrap both in `timeout 120` (treat exit 124 as a failure with a clear message) and add `timeout-minutes: 10` to the "Run Production smoke" step.
  3. *Stronger evidence (LOW):* after the migrate step, `grep -F "Legacy database baselined successfully"` in `$MIGRATE_LOG`; after the Production run, fail if `$PROD_LOG` contains an error-level line (`[ERR]`, `[FTL]`, `fail:`, `crit:` or `Unhandled exception`). If the log format makes the pattern unreliable, say so in `act.md` instead of guessing.
- README: modify `README.md` with a new "Database upgrades (Production)" section: always back up, run `--migrate-database`, what each outcome means (Created, UpToDate, Upgraded, Baselined), what a refusal means for a legacy database, where backups are written, and that the CI smoke script deletes the repo-root database unless `CI=true`.
- Verify (record all evidence in `act.md`): on the final commit the workflow run is green: build, full test suite (including the new migration, legacy-upgrade and migrator tests), Bash and PowerShell log utility tests, and the hardened production smoke step with its artifact; `git diff --stat <stage-start>..HEAD` shows no change under `src/MobileShop.Api`, entities, authentication, Development initialization or any unrelated file.
- Done when: README section exists, the smoke hardening is in place and passing in CI, the final run is `Success` and its number is recorded, and the reviewer signs Stage S off (only the reviewer ticks `to-do.md`).
- Risk: LOW
- Confidence: MEDIUM

## Global Definition of Done
- All six migrations are discoverable, match the snapshot, apply to an empty database and upgrade a legacy database with data preserved (CI tests).
- `--migrate-database` creates/upgrades/baselines safely with a verified backup; normal Production startup is read-only and refuses a stale schema; Development is unchanged.
- The CI Production smoke proves non-destructive startup on a seeded legacy-style database.
- README documents the Production procedure; the final workflow run number is recorded; no API, auth, entity or unrelated changes.

## Carry-over to later stages
- **Stage T:** widen money to `long` (D1) with an `AlterColumn<long>` migration; Stage S's chain, legacy-upgrade and smoke checks must stay green, and the overflow-boundary tests land there.
- **Stage U:** the `Phone`-category filter default is recorded in its Proposed-stages line.

## Execution notes
Work compact: chain dependent commands with `&&`, group read-only checks in one `{ ...; }` call, keep reports short, do not restate this plan in chat. One step → one commit → STOP for Job B. Do not run `dotnet build`/`dotnet test` locally; push and report `Action: #<run_number> — <Success|Failure|Pending>` per step. Never touch `to-do.md`, `plan.md` or `audit.md`; never amend; never push beyond what the workflow requires.
