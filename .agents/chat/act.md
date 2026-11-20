# Act — Stage 1 (Steps 1–4)

- Commit: uncommitted working tree changes (local graph commit `27c4fce` by `check.sh`; waiting for user review before committing/pushing Stage 1)
- Verification:
  - `./src/MobileShop.Scripts/Bash/check.sh` -> `STATUS: PASS` (`restore: PASS`, `build: PASS`, `test: PASS`, `graphify: PASS`, `graph_commit: PASS`)
  - `grep -nE "AlterColumn|DropColumn|InsertData|UpdateData|\.Sql\(" src/MobileShop.Dal/Migrations/20261121100000_AddHeadphones.cs` -> no matches
  - `MigrationChainTests` (5/5 passed, including `Headphone_migration_keeps_existing_rows`, `Snapshot_matches_the_current_model`, `Migrations_are_discovered_in_order`, `Chain_applies_to_an_empty_database`)
  - `SampleDataSeedTests` (7/7 passed)
  - `Action: unavailable` (local workspace only; not pushed)
- Files changed (Stage 1):
  - Created: `src/MobileShop.Models/Enums/HeadphoneKind.cs`, `src/MobileShop.Models/Enums/HeadphoneConnection.cs`, `src/MobileShop.Models/Entities/Headphone.cs`, `src/MobileShop.Models/Entities/Configuration/HeadphoneConfiguration.cs`, `src/MobileShop.Dal/Migrations/20261121100000_AddHeadphones.cs`, `src/MobileShop.Dal/Migrations/20261121100000_AddHeadphones.Designer.cs`
  - Modified: `src/MobileShop.Models/Entities/Product.cs`, `src/MobileShop.Dal/EfStructures/AppDbContext.cs`, `src/MobileShop.Dal/Migrations/AppDbContextModelSnapshot.cs`, `src/MobileShop.Tests/Dal/EfStructures/MigrationChainTests.cs`, `src/MobileShop.Dal/Initialization/SampleDataLoader.cs`, `src/MobileShop.Dal/Initialization/SampleDataInitializer.cs`, `src/MobileShop.Dal/Initialization/sample-data.json`, `docs/erd.mermaid`
- Limitations: None (Stage 1 only; held for user review before Stage 2).
- Friction noted: None.
- Problems: None.
- Status: COMPLETE (Stage 1; awaiting user review)
