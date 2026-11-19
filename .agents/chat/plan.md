# Plan — Local install and safe updates for Linux x64 and macOS arm64 (Stages 1–7)

> Scope note. plan.md normally holds one stage. On the owner's explicit request this file holds all seven stages and TODO.md was recreated to match. Everything else in AGENTS.md still applies; the Actor still performs exactly one step per `act`. Written against `main` at 7440f2b.

## 0. Stand-alone context

### 0.1 Goal

Ship MobileShop to the shop's Linux x64 and macOS arm64 machine so that (a) an update is one command, (b) the SQLite database is never lost, replaced or rewritten by an update, (c) an update costs only what the existing self-contained archive costs (no new runtime, package or dependency), and (d) a failed update rolls itself back. The full analysis (evidence E1–E10, S1, trust report) is in the Claude document "MobileShop: macOS and Linux publishing and update strategy"; the facts this plan relies on are restated below.

### 0.2 Decisions (final, do not reopen)

| ID | Decision |
|---|---|
| D1 | One root folder `ROOT` with `app/`, `data/`, `state/`, `cache/`. Linux `${XDG_DATA_HOME:-$HOME/.local/share}/mobileshop`; macOS `$HOME/Library/Application Support/MobileShop`. |
| D2 | Only `linux-x64` and `osx-arm64`. The installer refuses every other platform with a clear message. No new CI legs. |
| D3 | The site is used only on the machine that runs it: bind `localhost`, default port **5143** (dev profile uses 5043; 5000 and 7000 collide with AirPlay on macOS 12+). |
| D4 | Start at login (systemd user unit, LaunchAgent). Boot-time start stays optional (documented only). |
| D5 | Shop name, address, phone and Instagram live in the database (single-row table, admin page). `Pdf:Shop*` in `appsettings.json` stays only as the first-run seed. |
| D6 | No new runtime, package or dependency for updates: the full self-contained archive stays. Cheaper only through resume, skip-if-same-version, offline `--archive`, and `prune`. |
| D7 | The installer has `prune` to remove old unused versions. Immutable releases and provenance were not requested and are out of scope. |
| D8 | Existing manual installs: `adopt DIR` copies the old `MobileShop.db` (copy only). The owner re-enters shop name, address and Instagram once on the new page; the phone is taken from the existing shop party. |
| Git | One new branch `feat/local-install-and-update`, six commits (Stages 1–6, one per stage), merged to main only after all CI is green. |

### 0.3 Facts verified on the real v0.0.0 linux-x64 build (reasons behind the steps)

- Launched from a folder other than its own, the published binary creates a new empty DB and `logs/` there and does not load `appsettings.json` (content root = working directory). The launcher therefore pins everything (E3, E10).
- Serilog writes the relative `logs/app-.log`; `SolutionPaths.LogDirectory` is unused. `MOBILESHOP_DATA_DIRECTORY` moves only the DB.
- With `ASPNETCORE_ENVIRONMENT=Development`, the published binary deletes the DB at startup (E8).
- Data Protection isolates apps by content-root path: a different app folder logged the user out and made a stale form return HTTP 400 (E7). A fixed application name removes the cause; pinning the content root to a `current` symlink also worked (S1).
- With no URL configured the app listens on `http://localhost:5000` (E4).
- `normal startup` creates a fresh DB when the file is missing and does not detect a DB newer than the binary.
- `--migrate-database` makes a verified online backup `MobileShop.db.<yyyyMMddHHmmss>.bak` only when migrations are pending. The DB runs in WAL mode.
- `Pdf:ShopPhone` is written back into the shop party's DB row at every start and during `--migrate-database`.
- GitHub serves `releases/latest` as a redirect to the tag and `releases/latest/download/NAME` as a redirect to the asset, with range support (resume verified).
- Release assets today: `MobileShop-vX.Y.Z-linux-x64.tar.gz` (61.4 MiB), `MobileShop-vX.Y.Z-osx-arm64.tar.gz` (58.5 MiB), `checksums.txt`; each archive has one top-level folder `MobileShop-vX.Y.Z-RID/` containing `MobileShop.Web`, `appsettings.json`, `wwwroot/`, PDF fonts.

### 0.4 Target design (built by Stages 1–5)

Layout under `ROOT`:

| Path | Content |
|---|---|
| `app/versions/TAG/` | extracted archive folder (one per version) |
| `app/current` | symlink to `versions/TAG` (the only moving pointer) |
| `app/bin/mobileshop` | the installer script copy, used as launcher and CLI |
| `data/` | `MobileShop.db` (+ `-wal`, `-shm`), `MobileShop.db.*.bak`, `logs/` — never touched by the installer except backup restore and `adopt` |
| `state/` | `installed.env`, `install.log`, `lock/`, `app.pid` (no-service mode) |
| `cache/` | `downloads/`, `extract/` (bundle extraction cache, deletable) |

Launcher (`mobileshop run`) environment, set on every start from any caller:

| Variable | Value |
|---|---|
| `ASPNETCORE_ENVIRONMENT`, `DOTNET_ENVIRONMENT` | `Production` (overrides anything inherited) |
| `ASPNETCORE_CONTENTROOT` | `ROOT/app/current` (symlink path, never the resolved path) |
| `MOBILESHOP_DATA_DIRECTORY` | `ROOT/data` |
| `ASPNETCORE_URLS` | `http://localhost:PORT`, `PORT` = `MOBILESHOP_PORT` or 5143 |
| `DOTNET_BUNDLE_EXTRACT_BASE_DIR` | `ROOT/cache/extract` |
| working directory | `ROOT/data`; then `exec ROOT/app/current/MobileShop.Web` |

### 0.5 Git and CI workflow (binding for every step)

1. Before Step 1: `git fetch origin`, working tree clean, `git checkout main && git pull --ff-only`, then create `feat/local-install-and-update` from main. If that branch already exists locally or on origin, stop and report; never reuse it silently.
2. One commit per implementation step (Steps 1–6) with the exact message given in the step. `git add` named files only. No `Co-authored-by`. Steps 1–6 produce exactly six commits.
3. First push: `git push -u origin feat/local-install-and-update`, then open a **draft pull request** to `main` titled `feat: local installer and safe updates for Linux x64 and macOS arm64` (`gh pr create --draft`, or the GitHub API with the user's token, or ask the user to open it in the browser; report which). Reason: `.NET CI` runs only on pushes to `main` and on pull requests, never on plain branch pushes.
4. CI evidence for every push: `GET /repos/migueljocode/MobileShop/actions/runs?head_sha=TIP` (token from the user). Every `.NET CI` job must be green (`Build and test`, and the `Log utility` / scripts legs). Record `Action: #<run_number> — Success|Failure|Pending`. Runs cancelled by a newer push do not count.
5. CI failure or Reviewer Job B FAIL: fix, `git add` named files, `git commit --amend --no-edit`, `git push --force-with-lease origin feat/local-install-and-update`. This is allowed only for the tip commit of the current step and only on this branch. Once the next step starts, earlier commits are frozen (the only exception is the content-identical rebase onto a moved main in Step 7). Never plain `--force`, never touch another branch.
6. After a step's CI is green: stop and tell the user to request Job B. The next step starts only after Job B PASS.
7. Release dry run (Steps 5 and 7): dispatch `release.yml` from the branch with `dry_run=true` and `version=0.0.1-dryrun.N` (`POST /repos/migueljocode/MobileShop/actions/workflows/release.yml/dispatches` with `{"ref":"feat/local-install-and-update","inputs":{"version":"0.0.1-dryrun.1","dry_run":"true"}}`). If the token cannot dispatch, ask the user to run it from the Actions tab and record `Action: Pending`. Poll until completed.
8. Never create a tag or a GitHub Release. Never dispatch `release.yml` without `dry_run=true`.
9. Merge only in Step 7: fast-forward only (no squash, no merge commit), so the six commits land unchanged.
10. Bookkeeping per `actor.md`: inside each of Steps 1–6 change that step's heading to `## ~~[x] Step N — title~~` and include `plan.md` in the step's commit (`git add` the named files plus `plan.md`). `act.md` is the Actor's report and is not committed. Step 7 creates no commit on the branch: its heading tick and the `TODO.md` ticks are ordinary docs commits on `main` after the merge, outside the six. Every commit message below ends with the Actor's standard trailer `Implements Step N of plan.md.`

### 0.6 Approved deviations from project.md and AGENTS.md (explicit, limited to this plan)

1. Data Protection configuration (fixed application name) is approved in Step 1. It adds no authentication or authorization code.
2. `DatabaseInitializer.InitializeForDevelopment` gains a refusal guard for published data folders in Step 2. It stays Development-only and destructive otherwise.
3. Stage 4's `Pdf:ShopPhone` to shop-party sync is replaced in Step 3: the shop profile row is the source; config is only the first-run seed.
4. Step 3 may run `dotnet ef migrations add` locally (needed to generate migration and snapshot). No other local `dotnet build/test`.
5. Amend and `--force-with-lease` on the feature branch (rule 5 above), because the owner asked for exactly six commits.
6. New files under `src/MobileShop.Scripts/Bash`, `src/MobileShop.Scripts/tests`, `.github/scripts` and edits to `.github/workflows` are in scope for Steps 4–5.
7. Shell tests of the installer may be run locally (they are not `dotnet build/test`).

### 0.7 Out of scope

Intel Mac, arm64 Linux, musl; any smaller-download mechanism (framework-dependent, deltas, zstd, Velopack, Homebrew, Docker); LAN access or non-loopback binding; boot-time start and LaunchDaemons; immutable releases, provenance or signing; automatic deletion of `.bak` files; configuration overlay files; Data Protection key persistence in the data folder; automatic import of old `appsettings.json` values; changes to `log.sh`, `log.ps1`, `src/MobileShop.Api/**`; forcing a password change for the default admin; in-app update notices; cutting a release (`v0.1.0`) — the owner does that after the merge.

## Reviewer Briefing

- HIGH risk: Step 2 (startup path), Step 3 (migration plus data flow shop-party/PDF), Step 4 (largest script; bash 3.2 and BSD tools), Step 5 (release workflow cannot be proven by normal CI; the dry run is the proof).
- Invariants to check in every diff: the installer never copies, moves or edits `MobileShop.db*` except backup restore (rollback) and the copy in `adopt`; no `rm -rf` with an unchecked variable; every installer path stays under `ROOT`; no new product dependency (only POSIX tools, curl, tar, gzip, `sha256sum` or `shasum`, plus `systemctl`/`launchctl`/`codesign`/`xattr`/`sysctl` where applicable).
- Check that Steps 1–2 keep Development (`dotnet run` from the repo root) behaviour unchanged: `SolutionPaths.IsPublishedLayout` is false there.
- Check the Git rules in 0.5: exactly six commits with the given messages, no extra fix commits, no force without lease, no tags or releases.
- Step 3's deliberate choice: the shop profile values are pushed into the singleton `IOptions<PdfSettings>` instance so `QuestPdfGenerator` and its tests stay untouched. Flag it only if it breaks a test or thread-safety is demonstrably an issue.

## ~~[x] Step 1 — Runtime hardening (commit 1)~~

Commit message (exact):

```
fix(host): make published runs independent of the working directory

- write logs under SolutionPaths.LogDirectory (the data folder) instead of the relative logs/ path
- set a fixed Data Protection application name so sessions survive app-folder changes
- listen on http://localhost:5143 when no URL or port is configured
- add --version and --backup-database to the host commands

Implements Step 1 of plan.md.
```

**Depends on**: none (branch creation per 0.5).

**Files**
- Inspect: `src/MobileShop.Dal/EfStructures/SolutionPaths.cs`; `src/MobileShop.Services/Logging/Configuration/LoggingsConfiguration.cs`; `src/MobileShop.Web/Extensions/WebApplicationBuilderExtensions.cs` (`ConfigureBuilder`, `TryRunDatabaseCommand`, `ConfigureApp`); `src/MobileShop.Web/Program.cs`; `src/MobileShop.Web/Properties/launchSettings.json`; `src/MobileShop.Dal/Initialization/DatabaseMigrator.cs`; `src/MobileShop.Tests/Logging/LoggingsConfigurationTests.cs`; `src/MobileShop.Tests/Dal/Initialization/DatabaseMigratorTests.cs`.
- Modify: `SolutionPaths.cs`, `LoggingsConfiguration.cs`, `WebApplicationBuilderExtensions.cs`, `DatabaseMigrator.cs`, `LoggingsConfigurationTests.cs`, `DatabaseMigratorTests.cs`.
- Create: `src/MobileShop.Web/Extensions/DataProtectionExtensions.cs`, `HostUrlExtensions.cs`, `VersionInfo.cs`; matching tests next to the existing Web tests (mirror the folder layout the other tests use).
- Do not touch: `src/MobileShop.Api/**`, any `appsettings*.json`, `log.sh`, `log.ps1`, smoke scripts, docs.

**Symbols (new, pinned)**: `SolutionPaths.LogDirectory` (value changes), `LoggingsConfiguration.LogFilePath`, `DataProtectionExtensions.AddMobileShopDataProtection` + const `ApplicationName = "MobileShop"`, `HostUrlExtensions.UseDefaultUrlWhenUnset` + const `DefaultUrl = "http://localhost:5143"`, `VersionInfo.Describe(Assembly)`, `DatabaseMigrator.BackupVerified(AppDbContext)`.

**Current → Desired**: logs go to `<cwd>/logs` regardless of the data-folder variable → logs go to `SolutionPaths.Root/logs` (same place as today in dev and in a publish folder, but follows `MOBILESHOP_DATA_DIRECTORY`). Data Protection discriminator = content-root path → fixed name `MobileShop`. Default URL 5000 → 5143 when nothing is configured. No `--version`/`--backup-database` → both exist.

**Change**
1. `SolutionPaths.LogDirectory` becomes `Path.Combine(Root, "logs")`; update its comment.
2. `LoggingsConfiguration` exposes `public static string LogFilePath` (`Path.Combine(SolutionPaths.LogDirectory, "app-.log")`) and uses it as the file sink path. Keep rolling interval, retention and levels unchanged.
3. `AddMobileShopDataProtection` calls `services.AddDataProtection().SetApplicationName(ApplicationName)`; call it from `ConfigureBuilder` before authentication is added. Do not persist keys anywhere else.
4. `UseDefaultUrlWhenUnset(WebApplicationBuilder)`: if none of the configuration keys `urls`, `HTTP_PORTS`, `HTTPS_PORTS` holds a non-blank value and there is no `Kestrel:Endpoints` section, call `builder.WebHost.UseUrls(DefaultUrl)`. Call it first in `ConfigureBuilder`. Development is unaffected because `launchSettings.json` sets `ASPNETCORE_URLS`.
5. `VersionInfo.Describe(Assembly)` returns the assembly's `AssemblyInformationalVersion` (fall back to the assembly version). `--version` prints `MobileShop <value>` and exits 0 without touching the database or the data folder.
6. `DatabaseMigrator.BackupVerified(AppDbContext)` is a public wrapper over the existing `CreateVerifiedBackup` (no duplicated logic) returning the backup path. `--backup-database` prints `Verified backup: <path>` and exits 0; with no DB file it prints `No database found at <path>.` and exits 1.
7. Handle `--version` and `--backup-database` where `--migrate-database` is handled, with the same early-exit and exit-code mechanism; unknown arguments behave as today.

**Edge cases**: `--version` must work with no DB and no data folder; `--backup-database` must work while the app is running (online backup API); default URL logic must not override `--urls`, `ASPNETCORE_URLS` or `ASPNETCORE_HTTP_PORTS`.

**Tests** (new or extended, xUnit, existing patterns): `LogFilePath` lies under `SolutionPaths.LogDirectory` and ends with `app-.log`; `AddMobileShopDataProtection` yields `DataProtectionOptions.ApplicationDiscriminator == "MobileShop"`; `UseDefaultUrlWhenUnset` sets the default when nothing is configured and leaves `urls` and `HTTP_PORTS` untouched; `VersionInfo.Describe` returns a non-empty string; `BackupVerified` on a migrated temp DB returns one `.bak` that passes `PRAGMA integrity_check`.

**Graph**: after code edits run `graphify update .` and stage `graphify-out/graph.json` and `graphify-out/GRAPH_REPORT.md`.

**Verify**: push; PR runs; `Action: #N — Success` for every `.NET CI` job; `git diff --stat main..HEAD` lists only the files above plus the two graph files.

**Done when**: all new tests and the existing suite are green in CI; the commit message matches exactly; Development behaviour is unchanged (no env-variable dependence added).

**Risk**: LOW–MEDIUM (host startup and log path). **Confidence**: HIGH.

## ~~[x] Step 2 — Startup guards for published data folders (commit 2)~~

Commit message (exact):

```
fix(dal): add startup guards for published data folders

- refuse to create a new database at startup when a published data folder has none
- refuse the destructive Development initialization in a published layout
- refuse to start when the database holds migrations this build does not know
- name MobileShop.Web --migrate-database in published-mode messages

Implements Step 2 of plan.md.
```

**Depends on**: Step 1.

**Files**
- Inspect: `SolutionPaths.cs`; `src/MobileShop.Dal/Initialization/DatabaseMigrator.cs` (`Migrate`, `EnsureCurrent`, current-migration message); `src/MobileShop.Dal/Initialization/DatabaseInitializer.cs` (`InitializeForDevelopment`); `WebApplicationBuilderExtensions.cs` (`ConfigureApp`); `DatabaseMigratorTests.cs`.
- Modify: those four source files and `DatabaseMigratorTests.cs`.
- Create: `src/MobileShop.Tests/Dal/Initialization/DatabaseInitializerTests.cs` (check first whether it already exists; extend instead).
- Do not touch: `src/MobileShop.Api/**`, appsettings, smoke scripts, docs.

**Symbols (new, pinned)**: `SolutionPaths.IsPublishedLayout`; `DatabaseMigrator.EnsureCurrent(AppDbContext context, bool publishedLayout = false)`; `DatabaseMigrator.NewestBackup(string databaseFile)`; `DatabaseInitializer.EnsureDevelopmentIsSafe(bool publishedLayout)`.

**Current → Desired**: a missing DB at startup silently becomes a new empty shop; a DB migrated by a newer build is accepted by an older one; Development start wipes whatever DB the data folder holds; the pending-migration message tells published users to use `dotnet run` → each case refuses with a clear message in a published layout.

**Change**
1. `SolutionPaths.IsPublishedLayout` (static bool, computed beside `Root`): true when `MOBILESHOP_DATA_DIRECTORY` is non-blank, or the publish marker `MobileShop.Web.staticwebassets.endpoints.json` exists in the current directory (the marker `FindRoot` already uses). Leave `ResolveRoot` and `FindRoot` behaviour unchanged. In a normal `dotnet run` from the repo root it is false.
2. `EnsureCurrent(context, publishedLayout)`: (a) when `publishedLayout` and the DB file is missing → throw `InvalidOperationException`: `No database found at {file}. If this is a first install run MobileShop.Web --migrate-database. If the data folder moved, point MOBILESHOP_DATA_DIRECTORY at it.` and create nothing; (b) when the history table lists migrations this build does not know (`GetAppliedMigrations().Except(GetMigrations())`) → throw: `This database was migrated by a newer MobileShop (unknown migration: {id}). Install the newer version, or restore the backup taken before the upgrade: {NewestBackup or "none found"}.`; (c) pending-migration message: published → `The database schema is not current. Back up MobileShop.db and run: MobileShop.Web --migrate-database (or: mobileshop update)`, otherwise the existing text unchanged. With `publishedLayout == false` the existing behaviour, including creating a missing DB, is unchanged.
3. `Migrate(context)`: apply the same unknown-applied-migrations refusal when a history table exists, before any backup or migration.
4. `NewestBackup(databaseFile)`: newest `{databaseFile}.*.bak` by file name (timestamp order), or null.
5. `DatabaseInitializer.EnsureDevelopmentIsSafe(bool publishedLayout)` (new public static method): throws `InvalidOperationException`: `Refusing to run the destructive Development initialization against a published data folder ({SolutionPaths.Root}). Remove ASPNETCORE_ENVIRONMENT=Development or use a different data folder.` Call it from `ConfigureApp`, in the Development branch immediately before `DatabaseInitializer.InitializeForDevelopment(app.Services)`, with `SolutionPaths.IsPublishedLayout`. Do not change `InitializeForDevelopment` itself: `SampleDataSeedTests` calls it directly and the static flag could be true in the test process.
6. `ConfigureApp` passes `SolutionPaths.IsPublishedLayout` to `EnsureCurrent`.

**Edge cases**: tests must not mutate process environment variables (the flag is static); pass the flag as a parameter. DBs without a history table (legacy baseline path) skip the unknown-migration check. The refusal exceptions end the process with a non-zero exit code (the installer's health check relies on that).

**Tests**: missing DB + published → throws and the file is not created; missing DB + not published → created (existing test unchanged); extra history row `('99990101000000_FutureMigration','10.0.0')` → `EnsureCurrent` and `Migrate` throw with `newer MobileShop`, no backup is created by `Migrate`, and the message names an existing `.bak` file; both pending-migration message variants; `NewestBackup` picks the newest and returns null when none; `EnsureDevelopmentIsSafe(true)` throws, `(false)` does not.

**Graph**: `graphify update .`; stage the two graph files.

**Verify**: push; `Action: #N — Success` for every job.

**Done when**: new and existing tests are green; no change to behaviour when `publishedLayout` is false.

**Risk**: MEDIUM–HIGH (startup path of every deployment). **Confidence**: MEDIUM–HIGH.

## [ ] Step 3 — Shop profile in the database (commit 3)

Commit message (exact):

```
feat(shop): store the shop profile in the database

- add the ShopProfile entity, migration, data service and an Account/ShopProfile page
- seed the profile once from the existing shop party phone or the Pdf:Shop* settings
- read the shop party phone from the profile and push profile values into PdfSettings
- stop writing Pdf:ShopPhone back into the database at startup

Implements Step 3 of plan.md.
```

**Depends on**: Step 2.

**Files**
- Inspect: `src/MobileShop.Services/PDF/Settings/PdfSettings.cs`; `src/MobileShop.Services/PDF/Configuration/QuestPdfGenerator.cs` (consumers of `ShopName`, `ShopAddress`, `ShopPhone`, `ShopInstagram`); `src/MobileShop.Services/DataServices/Dal/ShopPartyDataService.cs` and `.../Interfaces/IShopPartyDataService.cs`; `src/MobileShop.Services/ServiceCollectionExtensions.cs`; `WebApplicationBuilderExtensions.cs` (`EnsureShopParty` and every call site); `src/MobileShop.Web/Pages/Account/Profile.cshtml` and `ProfileModel.cs`; `src/MobileShop.Web/ViewComponents/NavigationMenuViewComponent.cs`; `src/MobileShop.Models/Entities/Base/BaseEntity.cs`, one entity and its `Configuration/*Configuration.cs`; `src/MobileShop.Dal/EfStructures/AppDbContext.cs`; `IAccountDataService` (error-reporting convention); the phone validation used by the customer/seller create forms; tests `ShopPartyDataServiceTests.cs`, `ProfileModelTests.cs`, `MigrationChainTests.cs`.
- Create: `src/MobileShop.Models/Entities/ShopProfile.cs`; `.../Entities/Configuration/ShopProfileConfiguration.cs`; the generated migration `AddShopProfile` (+ designer) in `src/MobileShop.Dal/Migrations`; `IShopProfileDataService.cs`; `Dal/ShopProfileDataService.cs`; input/view-model types where the Profile types live; `src/MobileShop.Web/Pages/Account/ShopProfile.cshtml` and `ShopProfileModel.cs`; tests `ShopProfileDataServiceTests.cs`, `ShopProfileModelTests.cs`.
- Modify: `AppDbContext.cs` (DbSet), the model snapshot (generated), `ServiceCollectionExtensions.cs`, `ShopPartyDataService.cs`, `IShopPartyDataService.cs` (doc comment: the phone now comes from the shop profile), `ShopPartyDataServiceTests.cs`, `WebApplicationBuilderExtensions.cs`, `NavigationMenuViewComponent.cs`, `PdfSettings.cs` (comment: live values come from the database).
- Do not touch: `src/MobileShop.Api/**` (including Api data-service stubs: register the new service always-local, like `IShopPartyDataService`), `appsettings*.json` (the `Pdf:Shop*` keys stay as first-run seed), `QuestPdfGenerator.cs`, existing migrations.

**Symbols (new, pinned)**: `ShopProfile : BaseEntity` {`Name` required ≤100, `Address` ≤300, `Phone` required ≤30, `Instagram` ≤100}; `AppDbContext.ShopProfiles`; `IShopProfileDataService` {`Task EnsureAsync()`, `Task<ShopProfileView> GetAsync()`, `Task<ServiceResult> SaveAsync(ShopProfileInput input)`}; `ShopProfileView` and `ShopProfileInput` carry Name, Address, Phone, Instagram; `ServiceResult` is the existing result type of `IAccountDataService.ChangeCredentialsAsync` (`Succeeded`, `Message`, `ErrorField`); route `/Account/ShopProfile`.

**Current → Desired**: shop identity is read from `appsettings.json` and the shop party phone is overwritten from it at every start → identity lives in one DB row edited on a page; the party phone follows the row; config only seeds an empty database.

**Change**
1. Entity, configuration, DbSet. Single logical row (no unique index; the service always uses the first row).
2. Generate the migration: `dotnet ef migrations add AddShopProfile --project src/MobileShop.Dal --startup-project src/MobileShop.Web`. If `dotnet ef` is missing run `dotnet tool install --global dotnet-ef` (developer tool only; add no tool manifest). The migration only creates the table; no seeding in the migration.
3. `ShopProfileDataService` (scoped, Dal, always-local registration with a comment like the shop-party one). Dependencies: `IBaseRepo<ShopProfile>`, `IBaseRepo<Person>`, `IShopPartyDataService`, `IOptions<PdfSettings>`.
   - `EnsureAsync`: if no row exists, create it: phone from the existing shop party (the same lookup `ShopPartyDataService` uses: `people.FindAsync` on `FirstName + " " + LastName == ShopLabels.ZoneMobile`), else `PdfSettings.ShopPhone`; name, address, Instagram from `PdfSettings`. Then copy the row's four values into the `IOptions<PdfSettings>.Value` instance (deliberate in-place update; explain in a comment) so `QuestPdfGenerator`, which holds that instance, prints database values.
   - `GetAsync` returns the row as `ShopProfileView`. `SaveAsync` returns `ServiceResult` (build success and failure the way `AccountDataService` does): validate, update the row, refresh `PdfSettings`, then call `IShopPartyDataService.EnsureAsync()` so the shop party phone follows.
4. `ShopPartyDataService.EnsureAsync`: take the phone from the profile row (via the repo) instead of `IOptions<PdfSettings>`; remove the config-to-DB phone write-back; keep creation and the DB-to-DB phone sync. If no profile row exists, log a warning and do nothing (the host always ensures the profile first).
5. Host: extend the existing `EnsureShopParty` helper (its two call sites: the `--migrate-database` command and the Production branch of `ConfigureApp`) to run `IShopProfileDataService.EnsureAsync()` before `IShopPartyDataService.EnsureAsync()`. In the Development branch run only `IShopProfileDataService.EnsureAsync()` after `InitializeForDevelopment` (that branch does not ensure the shop party today; do not add that).
6. Page `ShopProfile` modelled on `Profile`: fields Name, Address, Phone, Instagram, validation attributes matching the entity limits and reusing the repo's phone validation (do not invent a regex), success message. Add an Account-menu entry "Shop profile" (description "Edit the shop details printed on invoices.") beside "Profile", reusing an existing sub-icon class.

**Edge cases**: existing DB with an owner-edited shop phone → the seed takes the DB phone, so nothing the owner typed is lost; fresh DB → seed phone is the appsettings default, so the shop party still exists after first start; saving a new phone updates row, party and `PdfSettings`; Address and Instagram may be empty; use the entity's concurrency token like other entities.

**Tests**: Ensure seeds from the party phone when the party exists, else from settings; Ensure never overwrites an existing row from settings; Ensure fills `PdfSettings`; Save updates row, party phone and `PdfSettings`; Save rejects blank name or phone; updated `ShopPartyDataServiceTests` (phone from the profile row, no write-back, no party without a profile row); `ShopProfileModelTests` (OnGet loads, valid OnPost saves and sets the message, invalid model returns the page); `MigrationChainTests` stay green.

**Graph**: `graphify update .`; stage the two graph files.

**Verify**: push; `Action: #N — Success` for every job; the migration files in the diff are the generated ones (no hand edits except none).

**Done when**: CI green; `git diff --stat` shows only expected files; no remaining read of `Pdf:ShopPhone` outside the seed path.

**Risk**: MEDIUM–HIGH (new table, data flow shop party/PDF). **Confidence**: MEDIUM.

## [ ] Step 4 — Local installer, launcher and user services (commit 4)

Commit message (exact):

```
feat(scripts): add the local installer, launcher and user services

- add install.sh: install, update, rollback, prune, adopt, backup, doctor, uninstall and service control
- keep app versions side by side behind a current symlink and migrate with the new binary before switching
- pin the launcher environment to Production, the data folder and a stable content root
- test the installer with fake archives and add macOS and shellcheck to the scripts CI job

Implements Step 4 of plan.md.
```

**Depends on**: Steps 1–3 (the installer runs the migrated, guarded app and the shop-profile flow).

**Files**
- Create: `src/MobileShop.Scripts/Bash/install.sh`; `src/MobileShop.Scripts/tests/test-install.sh`.
- Modify: `.github/workflows/dotnet.yml` (scripts job only).
- Inspect: `src/MobileShop.Scripts/README.md`, `Bash/log.sh` and `tests/test-bash.sh` (style, BSD/GNU handling), `.github/workflows/dotnet.yml`.
- Do not touch: app code, `release.yml`, docs, `log.sh`, `log.ps1`, `test-bash.sh`, `test-powershell.ps1`.

**Requirements (I1–I20)**
- I1 Form: `#!/usr/bin/env bash`; bash 3.2-compatible (no associative arrays, `mapfile`, `${var,,}`, `declare -g`, `local -n`, `readlink -f`, `timeout`, `flock`, GNU-only `sed -i`/`date -d`/`stat -c`); everything inside functions with `main "$@"` as the last line; `set -eu`; clean under `shellcheck -s bash`.
- I2 Version: `MOBILESHOP_VERSION="__VERSION__"` placeholder (stamped in Step 5); unstamped means `dev`. `version` prints it and the installed tag.
- I3 Overrides (also the test hooks): `MOBILESHOP_ROOT`, `MOBILESHOP_BIN_DIR` (default `$HOME/.local/bin`), `MOBILESHOP_PORT`, `MOBILESHOP_SERVICE` (`auto`|`none`; `--no-service` sets `none`), `MOBILESHOP_REPO` (default `migueljocode/MobileShop`), `MOBILESHOP_BASE_URL` (default `https://github.com`), `MOBILESHOP_UNAME_S`, `MOBILESHOP_UNAME_M`.
- I4 Platform: `uname -s`/`-m`; Linux x86_64 → `linux-x64`; Darwin arm64 → `osx-arm64`; Darwin reporting x86_64 while `sysctl -n hw.optional.arm64` is 1 (Rosetta shell) → `osx-arm64`; anything else exits 2 with `Unsupported platform: this release supports Linux x86_64 and macOS Apple Silicon only.`
- I5 Paths: `ROOT` is `MOBILESHOP_ROOT` if set, else the folder two levels above the script's own real path when the script lives in `ROOT/app/bin` (so the installed copy works when started by systemd, launchd or the PATH symlink), else the D1 default; create `app`, `data`, `state`, `cache` with mode 700 on `data`; quote every path (macOS root contains a space); PATH entry `MOBILESHOP_BIN_DIR/mobileshop` symlink to `ROOT/app/bin/mobileshop`; print a PATH hint if the directory is not on PATH; never edit shell rc files.
- I6 Release resolution: tag from `--version vX.Y.Z`, or from the final URL of `MOBILESHOP_BASE_URL/REPO/releases/latest` (`curl -fsSL -o /dev/null -w '%{url_effective}'`, tag = text after `/tag/`); archive URL `…/releases/download/TAG/MobileShop-TAG-RID.tar.gz`; checksums URL `…/checksums.txt`. Never use the REST API (unauthenticated calls get rate-limited or 403).
- I7 Download and verify: into `ROOT/cache/downloads` with `curl -fL --retry 5 --retry-delay 2 --retry-all-errors -C -` writing a `.part` file then renaming (omit `-C -` for `file://` URLs); verify SHA-256 against the matching `checksums.txt` line using `sha256sum` or `shasum -a 256`; mismatch deletes the file and exits 3. `--archive FILE` skips the download; it verifies against `checksums.txt` next to the file or `--sha256 HEX`, otherwise warns `unverified`.
- I8 Extract: `tar -xzf` into `ROOT/app/versions/.stage-TAG`; require exactly one top-level folder containing an executable `MobileShop.Web`; on macOS run `xattr -dr com.apple.quarantine` (ignore failure) and `codesign --verify` (failure exits 4); rename to `versions/TAG` (replace an existing folder of the same tag).
- I9 `run` (launcher): export the table in plan 0.4, `cd ROOT/data`, `exec ROOT/app/current/MobileShop.Web "$@"`. The environment is set unconditionally (inherited values are overridden).
- I10 Service management: `ROOT/state/app.pid` pid mode when `MOBILESHOP_SERVICE=none` (`nohup … run >> data/logs/stdout.log &`, `stop` kills and waits). Linux `auto`: user unit `${XDG_CONFIG_HOME:-$HOME/.config}/systemd/user/mobileshop.service` (`ExecStart="ROOT/app/bin/mobileshop" run`, `Restart=on-failure`, `RestartSec=3`, `TimeoutStopSec=30`, `WantedBy=default.target`), `systemctl --user daemon-reload` then `enable --now`; if no user bus is available fall back to pid mode with a warning. macOS `auto`: `~/Library/LaunchAgents/com.migueljocode.mobileshop.plist` (absolute paths, `ProgramArguments` launcher + `run`, `RunAtLoad` true, `KeepAlive` with `SuccessfulExit` false, `ExitTimeOut` 30, stdout/stderr under `~/Library/Logs/MobileShop/`), `launchctl bootstrap gui/$(id -u)`, stop with `bootout`, restart with `kickstart -k`. `start|stop|restart|status` act on whichever mode is active.
- I11 Install/update protocol (`install`, also what `update` runs): (1) lock with `mkdir ROOT/state/lock` and remove it on exit; (2) platform and target tag; (3) tag equals `installed.env` TAG and no `--force` → print `Already up to date (TAG).` exit 0; (4) pre-flight: free space ≥ 3× archive plus DB (`df -Pk`), `data` writable, port-in-use probe via bash `/dev/tcp` (only when nothing of ours runs); (5) download and verify; (6) extract and validate; (7) stop the running app and wait up to 20 s until the port is free; (8) run `ROOT/versions/TAG/MobileShop.Web --migrate-database` with the I9 environment but `ASPNETCORE_CONTENTROOT=` the staged folder, record exit code and any new `MobileShop.db.*.bak` (list before and after); (9) `ln -sfn versions/TAG current` (relative target); (10) start and poll `GET /Account/Login` on the port until HTTP 200 for up to 30 s; (11) write `installed.env` (`TAG`, `PREVIOUS`, `RID`, `INSTALLED_AT`, `LAST_BACKUP`), keep the current and previous version folders, clear `cache/extract`.
- I11a Failure handling: step 8 non-zero → leave `current` unchanged, restore a newly created `.bak` over the DB (delete `-wal` and `-shm` first), restart the previous version if it was running, exit 5. Step 10 failure → stop, restore a newly created `.bak`, point `current` back to the previous version, restart it, exit 5. First install with no previous version → remove `current`, keep `data`, exit 5. Never delete `.bak` files automatically.
- I12 `rollback [--yes]`: needs `PREVIOUS` and its folder; if `LAST_BACKUP` is non-empty print that restoring it discards everything entered since the update and require typed confirmation (or `--yes`); stop, restore only when confirmed, point `current` back, start, swap TAG/PREVIOUS, clear `LAST_BACKUP`.
- I13 `prune [--keep N] [--yes]` (D7): default N=2 (current plus newest other); list removable `versions/*` (never `current`, never `.stage-*` of a running install), confirm unless `--yes`, remove, clear `cache/downloads` and `cache/extract`. Never touches `data`.
- I14 `adopt DIR` (D8): refuse (exit 6) if `ROOT/data/MobileShop.db` exists, if `DIR/MobileShop.db` is missing, if `DIR/MobileShop.db-wal` exists and is non-empty (tell the user to stop the old app so it can checkpoint), or if our app is running. Copy to a temp name, `cmp -s` verify, rename into place, mode 600, leave the original untouched, print the next step (`mobileshop update` or `install`; the first `--migrate-database` makes its own verified backup).
- I15 `backup`: runs `ROOT/app/current/MobileShop.Web --backup-database` with the I9 environment.
- I16 `doctor` (exit 1 if any FAIL): platform supported; installed tag; `current` target exists; service/pid state; port listening; free space; `data` writable; DB file present and size; newest backup; PATH hint; Linux: glibc (`getconf GNU_LIBC_VERSION`), libstdc++ and ICU (`ldconfig -p` when available; WARN with the invariant-globalization hint if ICU is missing); macOS: `codesign --verify` of the current executable. `status`: tag, running or not, URL. `logs`: `tail -n 100 -f` on the newest `data/logs/app-*.log`.
- I17 `uninstall [--purge] [--yes]`: stop and disable the service, delete the unit/plist, remove the PATH symlink only if it points to our launcher, remove `app`, `state`, `cache`; keep `data`. `--purge` also removes `data` and `ROOT` after typing `delete` (or `--purge --yes`).
- I18 Exit codes: 0 ok, 1 generic or doctor failure, 2 usage or unsupported platform, 3 download or checksum, 4 extraction or signature, 5 migrate or health failure (rolled back), 6 refused for safety.
- I19 Logging: append timestamped lines to `ROOT/state/install.log`; concise console output; never print secrets.
- I20 Safety: no `rm -rf` on a variable that could be empty (use one guarded helper that refuses paths outside `ROOT`); the installer never writes `MobileShop.db*` except the I11a restore and the I14 copy; python3 is not used by the installer.
- I21 Update self-service: `update` fetches the latest release's `install.sh` (asset URL `…/releases/latest/download/install.sh`), verifies it against `checksums.txt`, then runs it with `install`; the installed `ROOT/app/bin/mobileshop` is a copy of the script that performed the last install.

**Work order inside the step** (one commit at the end): (1) I1–I9 and I18–I20 with tests T1–T3 and T14; (2) I10–I13 and I21 with T4–T9, T13, T15; (3) I14–I17 with T10–T12. Run the shell tests locally after each part.

**Tests (`test-install.sh`, bash 3.2-safe, fake archives)**: build `MobileShop-vX-RID.tar.gz` folders whose `MobileShop.Web` is a small bash script: `--version` prints a marker; `--migrate-database` creates `MobileShop.db` on first run, appends to it and writes a `.bak` when `FAKE_PENDING=1`, exits 1 when `FAKE_FAIL_MIGRATE=1`; `--backup-database` writes a `.bak`; run mode records its environment and working directory to a file and serves `GET /Account/Login` with python3 on the port from `ASPNETCORE_URLS` (python3 is allowed in tests and CI only). Scenarios: T1 platform refusal via the uname hooks; T2 fresh install (layout, launcher, symlink, `installed.env`, healthy); T3 re-run says up to date; T4 update keeps DB bytes when nothing is pending and records the backup when pending; T5 migrate failure → rollback, DB bytes unchanged, old version healthy; T6 health failure → rollback restores the new `.bak`; T7 checksum mismatch exits 3 and changes nothing; T8 `rollback --yes`; T9 `prune --keep 1 --yes` never removes `current` or `data`; T10 `adopt` copies, refuses an existing DB and a non-empty WAL; T11 `uninstall` keeps `data`, `--purge --yes` removes it; T12 `doctor` exit codes; T13 latest-tag discovery and download from a local python redirect server through `MOBILESHOP_BASE_URL`; T14 launcher pins Production even when `ASPNETCORE_ENVIRONMENT=Development` is exported, plus content root, data folder, URLs, extract dir and working directory; T15 a second concurrent install is refused by the lock.

**CI change (`dotnet.yml`, scripts job only)**: add `macos-latest` to the matrix; keep the job name `Log utility (${{ matrix.os }})` unchanged (renaming could break required checks and is not needed); add `if: runner.os != 'macOS'` to the existing PowerShell step (it has no condition today and would otherwise run on the new macOS leg); leave the existing Linux-only `test-bash.sh` step untouched. Add steps for the non-Windows legs: install `shellcheck` if missing (`brew install shellcheck` on macOS), run `shellcheck -s bash src/MobileShop.Scripts/Bash/install.sh src/MobileShop.Scripts/tests/test-install.sh`, run `bash src/MobileShop.Scripts/tests/test-install.sh`.

**Graph**: shell and YAML only — run `graphify update .` only if it reports code changes; stage graph files only when they changed.

**Verify**: push; `Action: #N — Success` for `Build and test` and all three scripts legs (ubuntu, windows, macos).

**Done when**: all fifteen scenarios pass on Ubuntu and macOS in CI; shellcheck is clean; no product dependency was added.

**Risk**: HIGH (size, bash 3.2/BSD differences). **Confidence**: MEDIUM.

## [ ] Step 5 — Release pipeline: installer asset, dry run, packaged-archive scenarios (commit 5)

Commit message (exact):

```
ci(release): ship the installer and test it against the packaged archives

- add a dry_run input that builds, packages and tests without creating a release
- stamp install.sh with the tag and attach it to the release with its checksum
- run installer scenarios against the real packaged archive on linux-x64 and osx-arm64

Implements Step 5 of plan.md.
```

**Depends on**: Step 4.

**Files**
- Inspect: `.github/workflows/release.yml` (inputs, `validate`, `publish`, `release` jobs), `.github/scripts/publish-smoke.sh` (login, HTTP and SQLite helpers to reuse in style), `src/MobileShop.Scripts/Bash/install.sh`.
- Modify: `.github/workflows/release.yml`.
- Create: `.github/scripts/installer-smoke.sh`.
- Do not touch: app code, `dotnet.yml`, docs, other scripts.

**Change**
1. `release.yml`: add input `dry_run` (boolean, default false, description "Build, package and test without creating a GitHub release"). The `release` job gets `if: ${{ !inputs.dry_run }}`. The `validate` job still validates the version and tag availability.
2. In each `publish` leg, after the existing packaged-archive smoke test, add a step "Installer scenarios" running `bash .github/scripts/installer-smoke.sh src/MobileShop.Scripts/Bash/install.sh <packaged archive>` (timeout-minutes 10). No new third-party actions.
3. In the `release` job: check out the repo, produce `dist/install.sh` by replacing `__VERSION__` with the tag (`sed`), compute `checksums.txt` over `MobileShop-*.tar.gz` and `install.sh`, and attach `install.sh` next to the archives in `gh release create`. Keep the existing flags, notes and permissions.
4. `installer-smoke.sh` (bash 3.2-safe, python3 allowed, temp HOME and `MOBILESHOP_ROOT`, random free port, traps for cleanup, no `timeout`/`flock`). Scenarios against the real archive: S1 fresh install with `--archive` and `--no-service`, app answers 200 on `/Account/Login`, log in as the default admin, stop, write the sentinel `PRAGMA user_version = 4242` into `data/MobileShop.db` with python's `sqlite3`, start again; S2 re-run says up to date; S3 repackage the same publish folder as `v9.9.9` and update while a session cookie from before the update exists: the cookie is still accepted afterwards, the sentinel and `PRAGMA integrity_check` are intact, `current` points at `v9.9.9`, both version folders exist; S4 `rollback --yes` restores the previous version and the sentinel is intact; S5 `prune --keep 1 --yes`; S6 start the packaged binary directly with `ASPNETCORE_ENVIRONMENT=Development` and `MOBILESHOP_DATA_DIRECTORY` pointing at a copy of the data folder: it must exit non-zero, its error output must contain `Refusing to run the destructive Development initialization`, and the copy's sentinel must survive (Step 2 guard); S7 start the packaged binary with no URL variables and answer on `http://localhost:5143/Account/Login` (Step 1 default; skip with a notice if the port is busy); S8 `adopt` into a fresh root, then update; S9 `uninstall` keeps `data`.
5. Do not publish anything: the dry run must not create a tag or release.

**Work order inside the step** (one commit at the end): workflow edits first, then `installer-smoke.sh` scenarios S1–S5, then S6–S9; run the script locally against a fake archive built with the Step 4 test helpers before the first push.

**Edge cases**: macOS runner has no `timeout` and bash 3.2; `release.yml` concurrency stays as is; the dry-run version must pass the existing semver validation (use `0.0.1-dryrun.N`).

**Verify**: push; `.NET CI` green; then dispatch the dry run from the branch (plan 0.5 rule 7) and require both `publish` legs including "Installer scenarios" to be green and the `release` job skipped. Record `Action: #N — Success` for both the CI run and the dry-run run. If the dry run fails, amend this commit (it is still the tip) and re-dispatch before Step 6 starts.

**Done when**: dry run green on both platforms; `release.yml` still creates a release when `dry_run` is false (read-only review of the job conditions; do not run it).

**Risk**: HIGH (workflow that cannot be exercised by normal CI). **Confidence**: MEDIUM.

## [ ] Step 6 — Documentation (commit 6)

Commit message (exact):

```
docs: document the local install, update, rollback and uninstall flow

- add an install and update guide to the README
- correct docs/publishing.md: log folder, default port, working directory and release process
- describe the installer and its tests in the scripts README

Implements Step 6 of plan.md.
```

**Depends on**: Step 5.

**Files**: modify `README.md`, `docs/publishing.md`, `src/MobileShop.Scripts/README.md`. Inspect the current text of each first. Do not touch code, workflows or scripts.

**Content requirements**
1. README "Install and update": one-liner `curl -fsSL https://github.com/migueljocode/MobileShop/releases/latest/download/install.sh | bash` (note: available from the first release published after this change); supported platforms (Linux x64, macOS Apple Silicon); what gets created under `ROOT`; open `http://localhost:5143`; the `mobileshop` commands from Step 4 in one table; the first-login reminder to change the default admin password; after the first update that carries Step 3, open Account → Shop profile and re-enter name, address and Instagram once; `adopt` for an existing manual install (copy only, old app stopped); `prune`; backups are made by the app before migrations and never deleted automatically; how to start at boot on Linux (`loginctl enable-linger`, may need admin rights) and that macOS starts at login.
2. Troubleshooting: port 5143 busy (`MOBILESHOP_PORT`), macOS AirPlay Receiver holding 5000/7000 (why 5143), browser-downloaded archive and Gatekeeper (use the one-liner, or `--archive` which clears quarantine), `~/.local/bin` not on PATH, Linux without libicu (`doctor` message), no systemd user session (pid mode), a failed update rolled back (read `ROOT/state/install.log`).
3. `docs/publishing.md`: fix the statements that logs follow the data folder only via the variable (now true), the default port (5143, not 5000), launching from another folder (use `mobileshop`; running `MobileShop.Web` directly from elsewhere still needs `MOBILESHOP_DATA_DIRECTORY` and a pinned content root); document the release process including the `dry_run` input and the installer asset; explain that `Pdf:Shop*` are first-run seeds only. State that in a published layout (data-folder variable set, or run from the publish folder) the first start no longer creates the database: run `MobileShop.Web --migrate-database` first (the installer does this), and that everyone is signed out once after the first update that contains Step 1.
4. `src/MobileShop.Scripts/README.md`: installer section with how to run `test-install.sh` and the CI legs.

**Verify**: push; `Action: #N — Success` (pull-request runs ignore no paths). Docs-only commit: no graph update.

**Done when**: every command and path in the docs matches Steps 1–5; no claim about a feature that is not in the branch.

**Risk**: LOW. **Confidence**: HIGH.

## [ ] Step 7 — Final validation and fast-forward merge (no commit)

**Depends on**: Steps 1–6 (each with CI green and Job B PASS).

1. `git fetch origin`; `git log --oneline origin/main..feat/local-install-and-update` shows exactly six commits with the six messages above, in order.
2. CI evidence for the tip: every `.NET CI` job green; record `Action: #N — Success`. No failing or pending runs for the tip.
3. Dispatch the release dry run on the tip (0.5 rule 7); it must be green with the `release` job skipped.
4. If `origin/main` moved since the branch point, rebase the branch onto it (`git rebase origin/main`), `git push --force-with-lease`, and repeat 2 and 3 on the new tip.
5. Merge: `git checkout main && git pull --ff-only && git merge --ff-only feat/local-install-and-update && git push origin main`. If the push is rejected (branch protection), stop and report instead of changing the method.
6. Check the `.NET CI` run triggered on `main` for the merged tip; record `Action: #N — Success`.
7. Leave the branch in place unless the owner asks to delete it. Do not create a tag or a release; tell the owner that cutting `v0.1.0` (Release workflow, `dry_run=false`) is their next action, after which the README one-liner works.
8. Bookkeeping on `main`, outside the six commits: tick the Step 7 heading in `plan.md`; the Reviewer ticks the stages in `TODO.md`; both are ordinary docs commits.

**Done when**: main contains the six commits unchanged, post-merge CI is green, evidence is recorded. Then Job B, then `sign off`.

**Risk**: MEDIUM (touches main). **Confidence**: HIGH.

## Global Definition of Done

- Six commits on `feat/local-install-and-update`, one per Stages 1–6, with the exact messages, merged fast-forward into `main`; no extra fix commits.
- Every `.NET CI` run for the final tip and for `main` after the merge is green; the release dry run is green on linux-x64 and osx-arm64.
- Verified behaviour (by tests): published binary refuses to create a database in a missing published data folder, refuses a database from a newer build, refuses the Development wipe, keeps sessions across an app-folder change, listens on 5143 by default, writes logs under the data folder; shop identity lives in the database and is never written back from config; the installer updates, rolls back, prunes, adopts and uninstalls without touching `MobileShop.db*` except the two documented cases.
- Docs match the shipped behaviour; no new runtime, package or dependency was added to the product.
- `graphify-out/graph.json` and `GRAPH_REPORT.md` were refreshed in the commits that changed code.
