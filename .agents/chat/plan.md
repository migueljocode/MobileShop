# Plan — Store the shop phone in appsettings.json and ensure the Zone Mobile shop party at startup

## Reviewer Briefing
- Requirement (user): use the shop's actual phone number `+98 911 500 9109`, stored in `appsettings.json`.
- Verified: `appsettings.json` (Web and Api) already has `Pdf:ShopPhone` (empty), bound to `PdfSettings.ShopPhone` and printed in the PDF header. A migration is static SQL and cannot read configuration, so the hard-coded `09115009109` in `SeedZoneMobileShopParty` cannot come from appsettings. The Zone Mobile party must instead be ensured by a startup step. `Program`/`ConfigureApp` already runs database steps for Production (`EnsureCurrent` + `EnsureAdminUser`) and `TryRunDatabaseCommand` runs `--migrate-database`; the Api project does no database startup; `IBaseRepo` has `AddAsync`/`UpdateAsync`.
- Design: new scoped `IShopPartyDataService.EnsureAsync()` reads `PdfSettings.ShopPhone`, creates the Person Zone Mobile with that phone when missing (and when a phone is configured), keeps the phone equal to the configured value when the person exists, then reuses `ShopPartyResolver.ResolveAsync` to get-or-create its Legal seller and customer. It is called after the database is ready in Production startup and in `--migrate-database`, both idempotent. Development is unchanged (sample data provides Person 1).
- ASSUMPTIONS (user to confirm): A1 reuse `Pdf:ShopPhone` as the single source (also printed on factor PDFs) rather than a new setting; set in both `src/MobileShop.Web/appsettings.json` and `src/MobileShop.Api/appsettings.json`. A2 the number is stored exactly as written, `+98 911 500 9109` (16 chars, within the 20-char limit). A3 the already-pushed migration `SeedZoneMobileShopParty` becomes a no-op (kept, not deleted, so migration history stays valid wherever it may already have run). A4 a configured phone overwrites the existing shop person's phone. A5 `PhoneNumberExtensions.ToRtlDisplayPhone` was written for local `0911 500 9109`; how the `+98` prefix renders in the PDF header is not verified here and needs a visual check.
- Risk: Step 2 (startup wiring, untestable by unit tests) and Step 4 (edits an applied-migration class and its tests) are MEDIUM; the rest LOW.
- Out of scope: People edit pages, changing `ToRtlDisplayPhone`, Api startup, Development startup, repointing rows.

## ~~[x] Step 1 — Shop party data service and its tests~~
- Files: inspect: `src/MobileShop.Services/DataServices/Interfaces/IAccountDataService.cs`, `src/MobileShop.Services/DataServices/Dal/AccountDataService.cs` (logger/ctor style), `src/MobileShop.Services/DataServices/Shared/ShopPartyResolver.cs`, `src/MobileShop.Services/PDF/Settings/PdfSettings.cs`, `src/MobileShop.Services/ServiceCollectionExtensions.cs`; create: `src/MobileShop.Services/DataServices/Interfaces/IShopPartyDataService.cs`, `src/MobileShop.Services/DataServices/Dal/ShopPartyDataService.cs`, `src/MobileShop.Tests/Services/DataServices/Dal/ShopPartyDataServiceTests.cs`; modify: `src/MobileShop.Services/ServiceCollectionExtensions.cs`; do not touch: `ShopPartyResolver.cs`.
- Symbols: new `IShopPartyDataService.EnsureAsync()` returning `Task`; new `ShopPartyDataService(IBaseRepo<Person> people, IBaseRepo<Seller> sellers, IBaseRepo<Customer> customers, IOptions<PdfSettings> options, ILogger<ShopPartyDataService> logger)`.
- Current -> Desired: nothing ensures the shop party outside the migration -> one idempotent service driven by `Pdf:ShopPhone`.
- Change: `phone = options.Value.ShopPhone?.Trim()`. Find the person with `people.FindAsync(p => p.FirstName + " " + p.LastName == ShopLabels.ZoneMobile)`. Missing: if `phone` is blank, log a warning that the shop phone is not configured and return; otherwise add `Person { FirstName = "Zone", LastName = "Mobile", PhoneNumber = phone, Notes = "The shop" }` and, if `AddAsync` returns `<= 0`, log a warning and return. Present: if `phone` is not blank and differs from `PhoneNumber`, set it and `UpdateAsync`. Finally `var shop = await ShopPartyResolver.ResolveAsync(people, sellers, customers);` and log a warning when `shop.Error` is not null. Register `services.AddScoped<IShopPartyDataService, ShopPartyDataService>();` in the common section of `ServiceCollectionExtensions` (next to the `IBaseRepo<>` registration, outside the `UseApi` branches).
- Depends on: none.
- Edge cases / error handling: never throw for missing configuration; follow the logging and primary-constructor style of `AccountDataService`.
- Tests: `ShopPartyDataServiceTests : RepoTestBase`, `[Collection("MobileShop-Dal-Serial")]`, options via `Options.Create(new PdfSettings { ShopPhone = ... })`, `NullLogger<ShopPartyDataService>.Instance`: (1) person missing + phone configured -> one Person Zone/Mobile with that phone, one Legal seller, one customer; (2) person missing + blank phone -> nothing created; (3) person exists with another phone -> phone updated to the configured one, still one seller and one customer; (4) person exists + blank phone -> phone unchanged; (5) calling twice leaves exactly one person, seller and customer.
- Verify: CI green; `Action: #<run_number> — Success`.
- Done when: the service, its registration and the five tests exist and pass.
- Risk: LOW
- Confidence: HIGH

## ~~[x] Step 2 — Run it at startup and in `--migrate-database`~~
- Files: inspect: `src/MobileShop.Web/Extensions/WebApplicationBuilderExtensions.cs`; modify: `src/MobileShop.Web/Extensions/WebApplicationBuilderExtensions.cs`; do not touch: the Api project, Development branch.
- Symbols: `WebApplicationBuilderExtensions.TryRunDatabaseCommand`, `WebApplicationBuilderExtensions.ConfigureApp` (Production `else` branch); new private static helper `EnsureShopParty(IServiceProvider services)`.
- Current -> Desired: only the admin user is ensured after the database is ready -> the shop party is ensured too.
- Change: add `private static void EnsureShopParty(IServiceProvider services) => services.GetRequiredService<IShopPartyDataService>().EnsureAsync().GetAwaiter().GetResult();`. In `TryRunDatabaseCommand`, call it with `scope.ServiceProvider` right after the `Created` admin step and before the console message (for every status). In `ConfigureApp`'s Production branch, call it after the `EnsureCurrent` admin step (every start). Do not call it in the Development branch.
- Depends on: Step 1.
- Edge cases / error handling: `EnsureCurrent` still throws first on an outdated schema, so the service never runs against an old schema.
- Tests: none (startup code is not unit-tested in this repo; behavior is covered by Step 1).
- Verify: CI green (build).
- Done when: both call sites exist and the Development branch is untouched.
- Risk: MEDIUM
- Confidence: MEDIUM

## ~~[x] Step 3 — Put the shop phone in appsettings.json~~
- Files: modify: `src/MobileShop.Web/appsettings.json`, `src/MobileShop.Api/appsettings.json`; do not touch: `appsettings.Development.json`.
- Symbols: `Pdf:ShopPhone`.
- Current -> Desired: `"ShopPhone": ""` -> `"ShopPhone": "+98 911 500 9109"`.
- Change: edit that one value in both files.
- Depends on: none (Step 2 consumes it).
- Edge cases / error handling: JSON stays valid; no test reads these files (checked: none reference `appsettings`).
- Tests: none.
- Verify: CI green.
- Done when: both files carry the number.
- Risk: LOW
- Confidence: HIGH

## ~~[x] Step 4 — Make the migration a no-op and remove its data tests~~
- Files: modify: `src/MobileShop.Dal/Migrations/20261107120000_SeedZoneMobileShopParty.cs`, `src/MobileShop.Tests/Dal/EfStructures/MigrationChainTests.cs`, `src/MobileShop.Tests/Dal/EfStructures/LegacyMoneyUpgradeTests.cs`; do not touch: the Designer, the snapshot, the migration id.
- Symbols: `SeedZoneMobileShopParty.Up/Down`; `MigrationChainTests.Chain_creates_the_Zone_Mobile_shop_party_on_an_empty_database`, `Zone_Mobile_migration_does_not_duplicate_an_existing_shop_person`, helper `Count`; the People expectation in `LegacyMoneyUpgradeTests` (~line 68).
- Current -> Desired: the migration inserts the party with a hard-coded phone -> the migration changes nothing; the number lives only in appsettings.json.
- Change: replace both method bodies with empty bodies and a class XML summary: "Intentionally empty. The Zone Mobile shop party is ensured at startup from `Pdf:ShopPhone` (see `ShopPartyDataService`); a migration cannot read configuration. Kept so migration history stays valid." Delete the two migration data tests and the `Count` helper from `MigrationChainTests` (keep the migration list entry and every other test). In `LegacyMoneyUpgradeTests` revert the expectation to `Assert.Equal(beforeCounts["People"], ...)` and remove the added comment line.
- Depends on: Steps 1–3.
- Edge cases / error handling: the Designer and `[Migration]` id stay, so `Migrations_are_discovered_in_order` and `Snapshot_matches_the_current_model` are unaffected.
- Tests: the remaining migration tests; no new ones.
- Verify: CI green including `MigrationChainTests` and `LegacyMoneyUpgradeTests`; `grep -rn "09115009109" src` returns only `PhoneNumberExtensionsTests`.
- Done when: the migration is a no-op and no source file outside that test contains the old number.
- Risk: MEDIUM
- Confidence: HIGH

## ~~[x] Step 5 — Update the docs~~
- Files: modify: `README.md`, `docs/publishing.md`; do not touch: everything else.
- Symbols: none (docs).
- Current -> Desired: docs say the `SeedZoneMobileShopParty` migration creates the shop party and "migrating an existing database adds the party" -> docs say startup and `--migrate-database` ensure the party using `Pdf:ShopPhone` from appsettings.json.
- Change: in README `### Production` replace the sentence naming the migration and the sentence about migrating an existing database with: the Zone Mobile shop party (a person with a Legal seller and a customer record) is ensured at every Production startup and by `--migrate-database`, using the shop phone from `Pdf:ShopPhone` in `appsettings.json` (which is also printed on factor PDFs); if `Pdf:ShopPhone` is empty and the party does not exist, recording a sale or purchase fails with a message asking for a customer with first name `Zone` and last name `Mobile` under People > Customers. Keep the other README and `docs/publishing.md` Zone Mobile sentences, adjusting "created" to "ensured" where needed. Do not print the phone number in the docs.
- Depends on: Steps 1–4.
- Edge cases / error handling: keep list numbering and surrounding text; mention that Development uses sample data instead.
- Tests: none.
- Verify: diff touches only those two files; docs-only pushes start no CI run (`paths-ignore`), report `Action: unavailable`.
- Done when: README and publishing guide match the new behavior.
- Risk: LOW
- Confidence: HIGH

## [ ] Step 6 — Final validation
- Files: inspect only.
- Symbols: none.
- Current -> Desired: unvalidated -> validated.
- Change: run `graphify update .` and commit the refreshed `graphify-out/graph.json` and `GRAPH_REPORT.md`; confirm the final-commit CI run is green; confirm `grep -rn "+98 911 500 9109" src` lists only the two appsettings files; record the run number in act.md.
- Depends on: Steps 1–5.
- Edge cases / error handling: report anything unexpected; do not widen scope.
- Tests: none new.
- Verify: `Action: #<run_number> — Success`.
- Done when: the Global Definition of Done is met.
- Risk: LOW
- Confidence: HIGH

## Global Definition of Done
- Steps 1–6 done; only the files named above changed (plus the refreshed graph); CI green (cite run number).
- The shop phone `+98 911 500 9109` is stored only in the two `appsettings.json` files; Production startup and `--migrate-database` ensure the Zone Mobile party from it; the old migration is a no-op.
- No schema, auth, API or Development-startup changes; Conventional Commits; named-file `git add` only.
