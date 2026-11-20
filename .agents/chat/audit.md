# Audit — Job A: Headphone product type plan (all stages)

**APPROVED WITH CORRECTIONS** (corrections already applied in `plan.md`; no further round needed)

Verified against `main` @ 37e4166: step file lists, symbols and line refs for Steps 1-10 (Cable/PortableStorage precedents, migration chain, `ProductsDataService` create/edit/read paths, `TransactionsDataService`, `ProductSpecExtensions`, Web pages), plus the dead-code claim (`ProductCreateRegistry`, `product-create-modal.js`, `_ProductCreate*Form` have no live references; `_ProductCreateSuccess` is live). Step 11 and the Stage validation steps: sanity check only.

## Findings
**HIGH-1 — Step 1 would fail CI.** `MigrationChainTests.Snapshot_matches_the_current_model` (lines 44-60) diffs the snapshot against the model. Adding `Headphone` without the snapshot/migration turns it red, and the plan's Step 1 had no migration.
**HIGH-2 — Step 2 would fail CI.** `Migrations_are_discovered_in_order` (lines 20-42) hard-codes the full migration id list; the new id was only appended in Step 3 ("if it hard-codes a list").
Fix (applied): Steps 1 and 2 merged into one Step 1 (entity, enums, config, DbSet, navigation, snapshot, migration, id appended to the list). Old Step 3 split into Step 2 (data-preservation test) and Step 3 (sample data + ERD) so Steps 5-11 keep their numbers and Stage 1 stays Steps 1-4.
**HIGH-3 — Details read path under-specified.** Step 6 said "extras" for `ProductDetailsViewModel`, but that record has one init property per type-specific field (`StorageKind`, `Speed`, ...) and `Details.cshtml` renders one block per type (lines 37-125). Neither `ProductDetailsViewModel.cs` nor `Details.cshtml` was in any file list, so the Actor would have had to invent the design.
Fix (applied): Step 6 adds `HeadphoneKind`, `HeadphoneConnection`, `NoiseCancelling`, `HeadphoneConnector`, `HasMicrophone` to the view model; Step 10 adds the `Details.cshtml` block.

## Notes (MEDIUM/LOW, not blocking)
- LOW: `Details.cshtml.cs` mapping `"Headphone" => "headphone"` was unnecessary (default branch already lowercases); removed from Step 10.
- LOW: Step 6 Tablet guarantee-label ref corrected to ~286; `GetProductType` is ~1362 (not ~1373); `EditIncludes` named explicitly.
- LOW: the plan's "do not copy `_ProductCreate*Form`" is right; Step 9 reuses the live `_ProductCreateSuccess` partial (now stated).
- MEDIUM: Step 2's raw-SQL inserts target the `AddShopProfile` schema; the Actor should check migrations after `20261006215821` for new NOT NULL columns before copying the precedent column lists.
- MEDIUM: migration id `20261121100000` is dated after today (2026-11-20); it only needs to sort after `20261119153500`, so fine.
- Confirmed correct: additive-only migration shape, no schema for colour/guarantee, `IsDeviceType` split (the `CaptureEditableState` guarantee change is needed so a guarantee-only edit is not reported as "No changes"), single interface implementer outside Dal (`ApiProductsDataService`), spec key is runtime-only (no persisted keys, no exact-string assertions).

Next: Planner adds the approved stages to `TODO.md`, then Actor starts Step 1.
