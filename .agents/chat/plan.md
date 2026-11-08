# Plan — Keep real contact details out of sample data and document the Zone Mobile shop party

## Reviewer Briefing
- Defect found while planning the docs: Stage 1 Step 4 put the real shop phone (`09115009109`) into `sample-data.json` Person 1. `.agents/project.md` (line 15) requires sample data to contain "no real contact details or credentials". The real number must exist only in the Production migration (`SeedZoneMobileShopParty`) and the test that asserts it. Step 1 restores a fake number (`09120000001`, Person 1's previous value, unique in the sample data) and replaces the real number in the resolver test fixture.
- Docs gap: README and `docs/publishing.md` still say Production first start creates only the schema and the admin account, and never mention the Zone Mobile shop party or the upgrade behaviour. There are no People edit pages, so the docs must not promise that the shop's phone can be edited in the UI.
- Verified: README `### Development` item 2, `### Production` paragraph, publish paragraph (~line 156) and `docs/publishing.md` second paragraph are the statements to update; the real phone currently appears in `sample-data.json`, the migration, `MigrationChainTests`, `ShopPartyResolverTests` and (pre-existing, unrelated) `PhoneNumberExtensionsTests`.
- Risk: LOW (data value, docs text, one test fixture value).
- Out of scope: People edit pages, `.agents/project.md`, `PhoneNumberExtensionsTests`, any behavior change.

## ~~[x] Step 1 — Fake shop phone in sample data and the resolver test~~
- Files: modify: `src/MobileShop.Dal/Initialization/sample-data.json`, `src/MobileShop.Tests/Services/DataServices/Shared/ShopPartyResolverTests.cs`; do not touch: the migration, `MigrationChainTests`.
- Symbols: sample Person 1 `phoneNumber`; `ShopPartyResolverTests.AddShopPerson`.
- Current -> Desired: `"phoneNumber": "09115009109"` (Person 1) -> `"09120000001"`; `PhoneNumber = "09115009109"` in `AddShopPerson` -> `"09120000000"`.
- Change: edit those two values only.
- Depends on: none.
- Edge cases / error handling: JSON stays valid; no other sample person uses `09120000001`.
- Tests: none new; existing `SampleDataSeedTests` and `ShopPartyResolverTests` stay green.
- Verify: CI green; `grep -rn "09115009109" src` lists only the migration, `MigrationChainTests` and `PhoneNumberExtensionsTests`.
- Done when: no real shop phone remains in sample data or the resolver test.
- Risk: LOW
- Confidence: HIGH

## ~~[x] Step 2 — Document the Zone Mobile shop party~~
- Files: modify: `README.md`, `docs/publishing.md`; do not touch: everything else.
- Symbols: none (docs).
- Current -> Desired: docs mention only the admin account at first start -> docs also describe the Zone Mobile shop party, its role in transactions and the upgrade behaviour.
- Change: (a) README `### Development` item 2: append "(Person 1, Zone Mobile, is the shop party)". (b) README `### Production`: change "creates the schema and initial admin account" to "creates the schema, the initial admin account and the Zone Mobile shop party", and add one paragraph after that paragraph: the shop party is a person named Zone Mobile with a Legal seller and a customer record, created by the `SeedZoneMobileShopParty` migration; every sale is recorded with it as seller and every purchase with it as customer, and the transaction pages show "Zone Mobile" for those sides, including existing purchases; migrating an existing database adds the party without changing existing rows; if the party is missing (for example a legacy database baselined without running migrations), recording a sale or purchase fails with a message asking for a customer with first name `Zone` and last name `Mobile` to be added under People > Customers. (c) README publish paragraph (~line 156): "first startup creates the schema and initial admin account" -> "first startup creates the schema, the initial admin account and the Zone Mobile shop party". (d) `docs/publishing.md` second paragraph: "creates the current schema and a single initial admin account (`admin` / `Admin@123`)" -> "creates the current schema, a single initial admin account (`admin` / `Admin@123`) and the Zone Mobile shop party (the seller of every sale and the customer of every purchase)".
- Depends on: none.
- Edge cases / error handling: do not print the shop phone number; do not claim the phone can be edited in the UI; keep list numbering and the rest of the text unchanged.
- Tests: none.
- Verify: CI green; diff touches only those two files and only the described sentences.
- Done when: README and publishing guide describe the shop party and the migration behaviour.
- Risk: LOW
- Confidence: HIGH

## ~~[x] Step 3 — Final validation~~
- Files: inspect only.
- Symbols: none.
- Current -> Desired: unvalidated -> validated.
- Change: run `graphify update .` and commit the refreshed `graphify-out/graph.json` and `GRAPH_REPORT.md` (AGENTS.md rule); confirm the final-commit CI run is green; confirm with grep that `sample-data.json` no longer contains `09115009109` and that README and `docs/publishing.md` each mention "Zone Mobile"; record the run number in act.md.
- Depends on: Steps 1–2.
- Edge cases / error handling: report anything unexpected; do not widen scope.
- Tests: none new.
- Verify: `Action: #<run_number> — Success`.
- Done when: the Global Definition of Done is met.
- Risk: LOW
- Confidence: HIGH

## Global Definition of Done
- Steps 1–3 done; only the files named above changed (plus the refreshed graph); CI green (cite run number).
- No real contact details in sample data; README and `docs/publishing.md` describe the Zone Mobile shop party and the migration/upgrade behaviour.
- No behavior, schema, migration, auth or API changes; Conventional Commits; named-file `git add` only.
