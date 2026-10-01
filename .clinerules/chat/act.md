# Act — Stage M Step 1 — PartNumber schema and migration

## Verification

- `dotnet build src/MobileShop.slnx --nologo` → Build succeeded. 0 warnings, 0 errors
- `dotnet test ... --filter "FullyQualifiedName~PartNumberEfTests"` → 5 passed, 0 failed, 0 skipped
- Full test suite (`--no-build`) → 256 passed, 0 failed, 0 skipped

## What changed

### New files
- `src/MobileShop.Models/Entities/PartNumber.cs` — PartNumber entity (ModelId FK, Code, SupportsDualSim, SupportsEsim; inherits BaseEntity)
- `src/MobileShop.Models/Entities/Configuration/PartNumberConfiguration.cs` — unique index on (ModelId, Code) with soft-delete filter; Model→PartNumber one-to-many; Phone→PartNumber optional with NoAction delete
- `src/MobileShop.Dal/Migrations/20261001161450_AddPartNumber.Designer.cs` + `AddPartNumber.cs` — additive migration (add column, create table, indexes, FK); no Drop/Delete/Alter in `Up`
- `src/MobileShop.Tests/Dal/BaseClass/SqliteRepoTestBase.cs` — SQLite-backed test base
- `src/MobileShop.Tests/Dal/EfStructures/PartNumberEfTests.cs` — 5 focused tests

### Modified files
- `src/MobileShop.Models/Entities/Phone.cs` — added nullable `PartNumberId` + `PartNumberNavigation`
- `src/MobileShop.Models/Entities/Model.cs` — added `PartNumbers` collection
- `src/MobileShop.Models/Entities/Configuration/PhoneConfiguration.cs` — added optional Phone → PartNumber relationship
- `src/MobileShop.Dal/EfStructures/AppDbContext.cs` — added `PartNumbers` DbSet
- `src/MobileShop.Dal/Migrations/AppDbContextModelSnapshot.cs` — auto-updated

## Tests added (5)
- `PartNumber_requires_model` — PartNumber → Model many-to-one
- `Phone_can_exist_without_part_number` — Phone → PartNumber optional (NULL FK)
- `Multiple_phones_can_share_same_part_number` — One-to-many from PartNumber to Phones
- `Duplicate_model_plus_code_rejected_but_same_code_different_model_allowed` — Unique constraint scoped to Model+Code
- `Migration_preserves_existing_phone_with_null_part_number` — Migration additive; phone survives with NULL PartNumberId

## Commit
`2121f17` — feat(models,dal): add PartNumber entity, nullable Phone.PartNumberId, additive AddPartNumber migration, and EF tests

## Limitations
None.

## Friction noted
- `editor` tool mangled tab/space indentation in some edits, requiring sed-based fixes.
- Full test suite (38s) exceeds 30s tool timeout; ran in background and polled.

## Problems
None

## Status
COMPLETE — awaiting Reviewer Job B.
