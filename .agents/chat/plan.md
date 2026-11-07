# Plan — Make the Zone Mobile shop party the stored seller of every sale and buyer of every purchase

## Reviewer Briefing
- Requirement (user): Anis Sahabi stays only as the person with the 50% profit share (Person 9 / Employee 6 untouched). Every sale's seller and every purchase's customer is the shop "Zone Mobile". The shop party is created by a migration; shop phone is +98 911 500 9109. Supersedes all earlier plans.
- Verified: sales hard-code `ShopSellerId = 1` (`TransactionsDataService.cs` 18, 336); purchases hard-require Person "Anis Sahabi" (`ProductsDataService.ResolvePurchasePartiesAsync` ~1388, 6 callers); `ShopLabels.ZoneMobile` exists; sample data treats Person/Seller/Customer 1 as the shop. `DatabaseMigrator.EnsureCurrent` throws on pending migrations, so new code cannot start on an un-migrated Production DB; a fresh DB runs `Migrate()` (all migrations); Development uses `EnsureCreated` (no migrations) and seeds sample data.
- Design: static `ShopPartyResolver` (precedent `ProductTransactionProjection`) finds the Person whose `FirstName + " " + LastName == ShopLabels.ZoneMobile`, then gets or creates its Seller (Legal) and Customer (NationalId "0099999999"). Purchases and sales both use it. A NEW data migration creates the party on fresh and existing databases; the resolver's "not registered" message remains the safety net (e.g. a legacy DB baselined by `DatabaseMigrator.BaselineHistory` skips data migrations).
- ASSUMPTIONS (user to confirm): A1 "first migration" = a new data migration (the `Initial` migration is already applied on live DBs, so editing it would not reach them). A2 phone stored as `09115009109` (local format used by every other stored number; `Person.PhoneNumber` has no format rule). A3 Person split Zone / Mobile, Seller EntityType Legal, Notes "The shop". A4 old rows are not repointed (Stage 2 labels cover history). A5 Anis's sample Customer 5 is removed.
- MEDIUM risk: Step 2 and 3 (many test call sites, no local compiler: CI is the only gate), Step 5 (hand-written migration applied to the live DB; its Designer is a copy). Step 5 also breaks two existing tests that it must update: `MigrationChainTests.Migrations_are_discovered_in_order` and the People count in `LegacyMoneyUpgradeTests`.
- Out of scope: employee / profit-share rows and fixtures, label changes (Stage 2), docs (Stage 3), `Seller` 1 `entityType` in sample data, repointing old transactions, vendored `wwwroot/lib`, generated `graphify-out`.

## ~~[x] Step 1 — Shared resolver and its tests~~
- Files: inspect: `src/MobileShop.Services/DataServices/Shared/ProductTransactionProjection.cs`, `src/MobileShop.Dal/Repo/IBaseRepo.cs`, `src/MobileShop.Services/DataServices/Dal/ProductsDataService.cs` lines ~1388–1412 (logic to port), `src/MobileShop.Models/Settings/ShopLabels.cs`; create: `src/MobileShop.Services/DataServices/Shared/ShopPartyResolver.cs`, `src/MobileShop.Tests/Services/DataServices/Shared/ShopPartyResolverTests.cs`; do not touch: existing services.
- Symbols: new `public static class ShopPartyResolver` with `Task<ShopParty> ResolveAsync(IBaseRepo<Person> people, IBaseRepo<Seller> sellers, IBaseRepo<Customer> customers)` and `public sealed record ShopParty(int SellerId, int CustomerId, string? Error)`.
- Current -> Desired: no shared shop identity -> one helper.
- Change: find the Person with `people.FindAsync(p => p.FirstName + " " + p.LastName == ShopLabels.ZoneMobile)`. Null -> `new ShopParty(0, 0, $"{ShopLabels.ZoneMobile} (the shop) must be registered before this can be recorded. In People > Customers, add a customer with first name Zone and last name Mobile, then try again.")`. Otherwise `sellers.FindAsync(s => s.PersonId == person.Id)`, else `AddAsync(new Seller { PersonId = person.Id, EntityType = SellerEntityType.Legal })`; `customers.FindAsync(c => c.PersonId == person.Id)`, else `AddAsync(new Customer { PersonId = person.Id, NationalId = "0099999999" })`. An `AddAsync` result `<= 0` -> Error `$"{ShopLabels.ZoneMobile} could not be registered as a seller."` / `"... as a customer."`. Return `new ShopParty(seller.Id, customer.Id, null)`.
- Depends on: none.
- Edge cases / error handling: repeat calls must not create duplicate roles (unique PersonId indexes); no logging or exceptions inside the helper; follow the namespace, file-scoped style and global usings of `ProductTransactionProjection.cs`.
- Tests: class `ShopPartyResolverTests : RepoTestBase` with `[Collection("MobileShop-Dal-Serial")]`, namespace `MobileShop.Tests.Services.DataServices.Shared`; four tests: person missing -> Error contains "Zone Mobile" and "People", no Seller/Customer created; person only -> seller (Legal) and customer created and ids returned; person, seller and customer exist -> same ids and still one row each; seller exists but customer missing -> only the customer is created. Create the Person with `FirstName = "Zone", LastName = "Mobile", PhoneNumber = "09115009109"`.
- Verify: CI green (helper not yet used, no behavior change); `Action: #<run_number> — Success`.
- Done when: helper and its four tests exist and pass.
- Risk: LOW
- Confidence: HIGH

## ~~[x] Step 2 — Purchases use the shop party; test fixtures~~
- Files: modify: `src/MobileShop.Services/DataServices/Dal/ProductsDataService.cs`, `src/MobileShop.Tests/Dal/BaseClass/TestDataHelpers.cs`, `src/MobileShop.Tests/Services/DataServices/Dal/ProductsDataServiceTests.cs`, and every other caller of `SeedAnisCustomer` (grep): `src/MobileShop.Tests/Web/Pages/Products/CreateAccessoryModelTests.cs`, `EditModelTests.cs`, `CreateAppleIdModelTests.cs`, `CreateGlassModelTests.cs`, `CreatePhoneModelTests.cs`, `CreateDeviceModelTests.cs`; do not touch: `TransactionsDataService.cs`.
- Symbols: `ProductsDataService.ResolvePurchasePartiesAsync(int)`; `TestDataHelpers.SeedShopSentinels`; `TestDataHelpers.SeedAnisCustomer` (delete).
- Current -> Desired: purchase customer = Person "Anis Sahabi" -> purchase customer = the Zone Mobile customer from `ShopPartyResolver`.
- Change: keep the "Selected seller not found." check; replace the Anis block and its two messages with `var shop = await ShopPartyResolver.ResolveAsync(people, sellers, customers);` and, when `shop.Error` is not null, `return new PurchasePartyResolution(0, 0, new ServiceResult(false, shop.Error, null, null));`; otherwise `return new PurchasePartyResolution(sellerId, shop.CustomerId, null);`. In `SeedShopSentinels` change the sentinel Person from "Shop"/"Owner" to "Zone"/"Mobile" (ids unchanged). Delete `SeedAnisCustomer`; in each caller remove the call (the shop party now comes from `SeedShopSentinels`; if a caller does not already call it, call it instead; never seed Id = 1 twice).
- Depends on: Step 1.
- Edge cases / error handling: `PeopleDataServiceTests` line ~374 builds its own "Shop Owner" fixture: leave it. `SampleDataInitializer.EnsureAnisCustomer` is untouched here (Step 4).
- Tests: in `ProductsDataServiceTests` the 6 Anis lookups (~1447, 1456, 1561, 1732, 2239, 2302) now expect the Zone Mobile customer's id (`Context.Customers.Single(c => c.PersonNavigation.FirstName == "Zone" && c.PersonNavigation.LastName == "Mobile").Id`); rename `CreatePhoneAsync_registers_Anis_as_customer_when_missing` to `CreatePhoneAsync_registers_ZoneMobile_customer_when_missing` (remove the Customer rows, keep the Person); add `CreatePhoneAsync_fails_with_actionable_message_when_ZoneMobile_person_missing` (remove the Zone Mobile Customer, Seller and Person rows) asserting `Succeeded` is false, `Message` contains "Zone Mobile" and "People", and `Context.Transactions` and `Context.Phones` are empty.
- Verify: CI green; `grep -rniw "anis\|sahabi" src/MobileShop.Services` returns nothing.
- Done when: no Anis reference remains in `ProductsDataService.cs` or `TestDataHelpers.cs` and CI is green.
- Risk: MEDIUM
- Confidence: MEDIUM

## [ ] Step 3 — Sales use the shop party
- Files: modify: `src/MobileShop.Services/DataServices/Dal/TransactionsDataService.cs`, `src/MobileShop.Tests/Services/DataServices/Dal/TransactionsDataServiceTests.cs`, `src/MobileShop.Tests/Services/DataServices/Dal/ReportsDataServiceTests.cs`, `src/MobileShop.Tests/Web/Pages/Transactions/DetailsModelTests.cs`; do not touch: other services.
- Symbols: `TransactionsDataService` primary constructor; `TransactionsDataService.RecordSellAsync` (~lines 306–345); constant `ShopSellerId` and its TODO comment (delete).
- Current -> Desired: `SellerId = ShopSellerId` (always 1) -> `SellerId` from `ShopPartyResolver`.
- Change: add `IBaseRepo<Person> people` to the primary constructor right after `customers`; in `RecordSellAsync`, resolve the shop party before building the Transaction; when `Error` is not null, log a warning like the method's other failures and return `new ServiceResult(false, shop.Error)`; set `SellerId = shop.SellerId`. Update the three test constructor calls with `new BaseRepo<Person>(Context)` in the same position.
- Depends on: Steps 1 and 2 (the sentinel Person is Zone Mobile).
- Edge cases / error handling: every test that calls `RecordSellAsync` must seed the shop party via `SeedShopSentinels`: confirm the seeded tests cover all 10 calls in `TransactionsDataServiceTests` and fix the 1 call in `ReportsDataServiceTests`. Tests that mock `ITransactionsDataService` are unaffected.
- Tests: strengthen `RecordSellAsync_persists_with_shop_seller_and_rejects_duplicate_sell` to assert the persisted `SellerId` equals the Zone Mobile seller's id; add `RecordSellAsync_fails_with_actionable_message_when_ZoneMobile_person_missing` (no sentinel seeded) asserting a failed result containing "Zone Mobile" and no Transaction row.
- Verify: CI green; `grep -rn "ShopSellerId" src` returns nothing.
- Done when: sales no longer use a fixed seller id and CI is green.
- Risk: MEDIUM
- Confidence: MEDIUM

## [ ] Step 4 — Sample data and seeder: Anis only as profit-share person
- Files: modify: `src/MobileShop.Dal/Initialization/sample-data.json`, `src/MobileShop.Dal/Initialization/SampleDataInitializer.cs`, `src/MobileShop.Tests/Dal/Initialization/SampleDataSeedTests.cs`; do not touch: all other records and IDs, Person 9 and Employee 6.
- Symbols: `SampleDataInitializer.EnsureAnisCustomer` (delete) and its call in `SeedIfEmpty`.
- Current -> Desired: Person 1 "Mohammad Karimi" (notes "Shop owner") -> Person 1 "Zone Mobile" (phone `09115009109`, notes "The shop"); Customer 5 (Anis, 0099999999) removed.
- Change: edit those JSON values and delete customer 5; delete the method and its call, leaving `SeedIfEmpty` as `if (!context.People.Any()) { SeedData(context); }`. Seller 1 and Customer 1 keep their ids so sample sells (seller 1) and buys (customer 1) now belong to Zone Mobile.
- Depends on: Steps 2 and 3.
- Edge cases / error handling: JSON stays valid; no transaction references customer 5 (verified: sample buys use customer 1, sells use customers 2–4).
- Tests: in `SampleDataSeedTests` the Customers count 5 -> 4; replace the Anis-customer assertion (~line 55) with an assertion that Customer 1's person is "Zone Mobile"; leave the employee assertions (~84, 91) unchanged; check the label expectation near line 127.
- Verify: CI green; word-bounded grep for `Anis|Sahabi` in `src/MobileShop.Dal` leaves only Person 9 and Employee 6 in `sample-data.json`.
- Done when: sample data has Zone Mobile as the shop party and Anis only as Person 9 + Employee 6.
- Risk: LOW
- Confidence: HIGH

## [ ] Step 5 — Data migration that creates the Zone Mobile shop party
- Files: inspect: `src/MobileShop.Dal/Migrations/20261104100000_AddAppleIdSecurityAnswers.cs` and `.Designer.cs` (shape), `20261008023227_AddCpuGpuLookups.cs` (`migrationBuilder.Sql` raw-string style); create: `src/MobileShop.Dal/Migrations/20261107120000_SeedZoneMobileShopParty.cs`, `src/MobileShop.Dal/Migrations/20261107120000_SeedZoneMobileShopParty.Designer.cs`; modify: `src/MobileShop.Tests/Dal/EfStructures/MigrationChainTests.cs`, `src/MobileShop.Tests/Dal/EfStructures/LegacyMoneyUpgradeTests.cs`; do not touch: `AppDbContextModelSnapshot.cs`, every other migration.
- Symbols: new `SeedZoneMobileShopParty : Migration` (`Up`, `Down`); `MigrationChainTests.Migrations_are_discovered_in_order`; `LegacyMoneyUpgradeTests` People count assertion (~line 68).
- Current -> Desired: no shop party in new or upgraded databases -> the migration creates Person + Seller + Customer for Zone Mobile.
- Change: Designer = copy of the latest Designer with `20261104100000_AddAppleIdSecurityAnswers` replaced by `20261107120000_SeedZoneMobileShopParty` and `partial class AddAppleIdSecurityAnswers` by `partial class SeedZoneMobileShopParty` (model unchanged, so `BuildTargetModel` stays identical to the snapshot; keep file encoding). `Up` runs three `migrationBuilder.Sql` statements, each guarded so it is idempotent: (1) `INSERT INTO "People" ("FirstName","LastName","PhoneNumber","Notes","IsDeleted") SELECT 'Zone','Mobile','09115009109','The shop',0 WHERE NOT EXISTS (SELECT 1 FROM "People" WHERE "FirstName"='Zone' AND "LastName"='Mobile' AND "IsDeleted"=0);` (2) `INSERT INTO "Sellers" ("PersonId","EntityType","IsDeleted") SELECT p."Id",1,0 FROM "People" p WHERE p."FirstName"='Zone' AND p."LastName"='Mobile' AND p."IsDeleted"=0 AND NOT EXISTS (SELECT 1 FROM "Sellers" s WHERE s."PersonId"=p."Id");` (3) the same shape into `"Customers" ("PersonId","NationalId","IsDeleted")` with `'0099999999'`. `Down` deletes the Zone Mobile Customer, Seller and Person rows only when no `"Transactions"` row references the customer or seller (`NOT EXISTS`), then the person only if neither role row remains.
- Depends on: none (independent of Steps 1–4).
- Edge cases / error handling: `EntityType` 1 = `SellerEntityType.Legal`; `PersonId` is unique on Sellers and Customers, hence the `NOT EXISTS`; `TimeStamp` is nullable, so it is omitted; a legacy DB baselined by `DatabaseMigrator.BaselineHistory` skips this migration, and the Step 1 message covers it.
- Tests: `Migrations_are_discovered_in_order` gains `"20261107120000_SeedZoneMobileShopParty"` as the last item; add `Chain_creates_the_Zone_Mobile_shop_party_on_an_empty_database` (after `Migrate()`: one People row Zone/Mobile with phone `09115009109`; one Seller for it with `EntityType` 1; one Customer for it with NationalId `0099999999`); add `Zone_Mobile_migration_does_not_duplicate_an_existing_shop_person` (migrate to `20261104100000_AddAppleIdSecurityAnswers`, insert a Zone/Mobile person via `ExecuteSqlRaw`, `Migrate()`, assert still one such Person and exactly one Seller and one Customer pointing at it); in `LegacyMoneyUpgradeTests` change the after-migration People expectation to `beforeCounts["People"] + 1` with a one-line comment that the migration adds the Zone Mobile person.
- Verify: CI green including `Snapshot_matches_the_current_model`, `Chain_applies_to_an_empty_database` and the two new tests; `Action: #<run_number> — Success`.
- Done when: the migration, its Designer and the three test changes are in and CI is green.
- Risk: MEDIUM
- Confidence: MEDIUM

## [ ] Step 6 — Final validation
- Files: inspect only.
- Symbols: none.
- Current -> Desired: unvalidated -> validated.
- Change: run `graphify update .`; confirm the final-commit CI run is green; run `grep -rniw "anis\|sahabi" src --include=*.cs --include=*.json --exclude-dir=lib` and confirm only the employee / profit-share fixtures remain (`sample-data.json` Person 9 and Employee 6, `SampleDataSeedTests`, `ReportsDataServiceTests`, `DistributionCalculatorTests`, `ProfitLossTests`); record the run number in act.md.
- Depends on: Steps 1–5.
- Edge cases / error handling: report any leftover outside those files; do not widen scope.
- Tests: none new.
- Verify: `Action: #<run_number> — Success`.
- Done when: the Global Definition of Done is met.
- Risk: LOW
- Confidence: HIGH

## Global Definition of Done
- Steps 1–6 done; only the files named above changed; CI green (cite run number).
- Sales and purchases both use `ShopPartyResolver`; no `ShopSellerId`; no Anis lookup in production code; Anis remains only as Person 9 + Employee 6.
- The migration creates the Zone Mobile party (phone `09115009109`) on fresh and upgraded databases; no model or snapshot change.
- Conventional Commits; named-file `git add` only; no other behavior, auth, API or Apple ID changes.
