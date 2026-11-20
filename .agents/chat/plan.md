# Plan — Headphone product type (all stages)

## Reviewer Briefing
- Step 1 (entity + migration) is HIGH-risk: the migration runs on every user's database during update. It must be purely additive (one `CreateTable` + one `CreateIndex` in Up, one `DropTable` in Down). Confidence MEDIUM if hand-written, HIGH if generated with `dotnet ef` (only if the user permits a local run; AGENTS.md says CI is the only build gate).
- Entity, model snapshot, migration and the id list hard-coded in `Migrations_are_discovered_in_order` are ONE step on purpose: any of them alone turns `Snapshot_matches_the_current_model` or `Migrations_are_discovered_in_order` red in CI, and every step commit must pass CI.
- Existing rows must survive: Step 2 adds a data-preservation test modeled on `Cpu_and_gpu_names_are_preserved_when_laptop_catalog_is_migrated`. No seeding, no `UpdateData`.
- Optional Color and Guarantee need NO schema change: they use the existing `Product.ColorId` and the `Guarantee` table (`Product.GuaranteeProfile`). They are NOT in the migration.
- Enums persist as integers: member order of `HeadphoneKind` / `HeadphoneConnection` is permanent once released.
- Interface change: `IProductsDataService.CreateHeadphonesAsync` forces a stub in `ApiProductsDataService` (Step 5). That is the only Api-layer touch and is explicitly in scope; `src/MobileShop.Api` is not touched.
- Edit gating: `IsDeviceType` gates colour + second-hand + guarantee together today. Step 7 splits it so Headphone gets colour + guarantee but NOT second-hand, with identical behaviour for the four device types (existing tests must stay green unchanged).
- Details read path: `ProductDetailsViewModel` has one init property per type-specific field (no generic extras collection) and `Details.cshtml` renders one block per type. Step 6 adds the five view-model properties; Step 10 adds the `Details.cshtml` block. Colour, guarantee and second-hand rows on Details are already generic.
- Assumptions (my labelled interpretations of the approved shape): `Connector` reuses `CableConnector`; `HasMicrophone` is a nullable tri-state; Notes is edit-page only (like other accessories); colour list is the shared `Colors` table with no inline "add colour" button on the headphone create page.
- Dead code found, so NOT touched: `wwwroot/js/product-create-modal.js`, `Pages/Shared/ProductCreateRegistry.cs` and all `_ProductCreate*Form.cshtml` partials have zero references in `.cs` / `.cshtml` / `.js` outside themselves. Headphones therefore get a standalone create page only (like every live accessory). `_ProductCreateSuccess.cshtml` is NOT dead (live partial used by every create page and Details); Step 9 reuses it.
- OUT OF SCOPE (user chose to ignore or not asked): (a) `Products/Index.cshtml.cs GetCategoryName()` returns `"Smart Watch"`, `"Power Bank"`, `"Portable Storage"` while models are stored as `SmartWatch`, `PowerBank`, `PortableStorage` (model filter probably empty on those tabs); (b) in `_ProductEditForm.cshtml` the Guarantee blocks for Phone/Tablet/Smart Watch/Laptop are `style="display:none"` and no script reveals them, so guarantee looks uneditable on those edit forms (~90% sure). The Headphone edit block deliberately does NOT copy that `display:none`.
- `Category` row `"Headphone"` is created lazily by `GetOrCreateCategoryAsync` on the first model; never seeded.

## Requirements & constraints
- One `Headphone` type: Kind = Earbuds / OverEar / Headset.
- Final fields — create form: Seller, Manufacturer, Model, Paid price, Profit % / amount, Count, Kind, Connection (Wired / Wireless / WiredAndWireless), Noise cancelling (bool), Connector (optional), Microphone (optional: unknown/yes/no), Color (optional), Guarantee (optional: corporation, expiry, notes). Edit page adds Notes.
- Existing user databases upgrade through `DatabaseMigrator` (verified backup, then `Migrate()`) with no change to existing tables or rows.
- Must NOT change: existing migrations, `src/MobileShop.Api`, `IsDeviceType` behaviour for devices, Apple ID password handling, `DatabaseInitializer.InitializeForDevelopment` semantics.

---
# STAGE 1 — Data model and additive migration (ships a dormant schema; a release after this stage is safe)

## ~~[x] Step 1 — Entity, enums, configuration, DbSet, Product navigation, migration `AddHeadphones`~~
- Files: inspect: `src/MobileShop.Models/Entities/Cable.cs`, `Entities/Configuration/CableConfiguration.cs`, `Enums/CableConnector.cs`, `src/MobileShop.Dal/Migrations/20260920163656_NormalizeCatalog.cs` (Cables `CreateTable` + `IX_Cables_ProductId`), `20261119153500_AddShopProfile*.cs`; modify: `src/MobileShop.Models/Entities/Product.cs`, `src/MobileShop.Dal/EfStructures/AppDbContext.cs`, `src/MobileShop.Dal/Migrations/AppDbContextModelSnapshot.cs`, `src/MobileShop.Tests/Dal/EfStructures/MigrationChainTests.cs`; create: `src/MobileShop.Models/Enums/HeadphoneKind.cs`, `src/MobileShop.Models/Enums/HeadphoneConnection.cs`, `src/MobileShop.Models/Entities/Headphone.cs`, `src/MobileShop.Models/Entities/Configuration/HeadphoneConfiguration.cs`, `src/MobileShop.Dal/Migrations/20261121100000_AddHeadphones.cs` and `.Designer.cs`; do not touch: Api, Web, Services, all other migration files.
- Symbols: `enum HeadphoneKind { Earbuds, OverEar, Headset }`; `enum HeadphoneConnection { Wired, Wireless, WiredAndWireless }`; `Headphone : BaseEntity` with `[Table("Headphones")]`, `ProductId`, `ProductNavigation`, `Kind`, `Connection`, `bool NoiseCancelling`, `CableConnector? Connector`, `bool? HasMicrophone`, `[StringLength(500, ErrorMessage = "{0} cannot exceed {1} characters.")] string? Notes`; `Product.HeadphoneProfile` (`Headphone?`, after `PortableStorageProfile`); `AppDbContext.Headphones` (after `PortableStorages`); `partial class AddHeadphones : Migration` (`Up`, `Down`); id `20261121100000_AddHeadphones` (must sort after `20261119153500`).
- Current -> Desired: no headphone type, snapshot without `Headphone` -> entity mapped one-to-one with `Product`, snapshot with `Headphone` + `Product.HeadphoneProfile` navigation, and the matching migration.
- Change: (a) Entity: copy the `Cable` / `CableConfiguration` pattern exactly (`HasOne(...).WithOne(p => p.HeadphoneProfile).HasForeignKey<Headphone>(h => h.ProductId).OnDelete(DeleteBehavior.NoAction)`). Enums in `MobileShop.Models.Enums` with one-line `<summary>`; no `HasConversion`. Rely on `GlobalUsings.cs`; no redundant usings. (b) Migration: preferred, only if the user permitted a local run in this request: `dotnet ef migrations add AddHeadphones --project src/MobileShop.Dal --startup-project src/MobileShop.Web`, then confirm only the migration, designer and snapshot files changed. Otherwise hand-write: `Up` = `CreateTable("Headphones")` with `Id` (INTEGER, `Sqlite:Autoincrement`), `ProductId` INTEGER not null, `Kind` INTEGER not null, `Connection` INTEGER not null, `NoiseCancelling` INTEGER not null, `Connector` INTEGER null, `HasMicrophone` INTEGER null, `Notes` TEXT maxLength 500 null, plus the `BaseEntity` columns exactly as in the Cables table (`TimeStamp` BLOB rowVersion nullable, `IsDeleted` INTEGER not null); `PK_Headphones`; `FK_Headphones_Products_ProductId` -> `Products.Id` (no cascade); `CreateIndex("IX_Headphones_ProductId", unique: true)`. `Down` = `DropTable("Headphones")`. Designer + snapshot: add the `Headphone` entity block and `b.Navigation("HeadphoneProfile")` in the `Product` block, mirroring `Cable`. (c) In `MigrationChainTests.Migrations_are_discovered_in_order`, which hard-codes the full list, append `"20261121100000_AddHeadphones"` after `"20261119153500_AddShopProfile"`.
- Depends on: none.
- Edge cases / error handling: soft delete is applied by `ApplySoftDeleteForEntities()` for every `BaseEntity`; add nothing. Up/Down contain NO `AlterColumn`, `DropColumn`, `Sql`, `InsertData`, `UpdateData`, `DropTable` (in Up) or any operation on an existing table.
- Tests: existing `Snapshot_matches_the_current_model`, `Chain_applies_to_an_empty_database`, `Migrations_are_discovered_in_order` (list updated in this step). All must be green on this single commit.
- Verify: CI green; `grep -nE "AlterColumn|DropColumn|InsertData|UpdateData|\.Sql\(" src/MobileShop.Dal/Migrations/20261121100000_AddHeadphones.cs` prints nothing; `git diff --name-only` lists only the 4 modified + 6 created files above.
- Done when: model compiles, `Product.HeadphoneProfile` and `AppDbContext.Headphones` exist, and the migration chain + snapshot tests pass in CI.
- Risk: HIGH
- Confidence: MEDIUM (hand-written) / HIGH (generated)

## ~~[x] Step 2 — Data-preservation test~~
- Files: inspect: `src/MobileShop.Tests/Dal/EfStructures/MigrationChainTests.cs` (`Cpu_and_gpu_names_are_preserved_when_laptop_catalog_is_migrated`), migrations after `20261006215821_AddPowerBankPortsAndPhoneSimSupport` for new NOT NULL columns; modify: `MigrationChainTests.cs`; do not touch: Web, Services, Api, Dal production code.
- Symbols: test `Headphone_migration_keeps_existing_rows`.
- Current -> Desired: no proof existing data survives -> test proves it.
- Change: migrate a temp SQLite DB to `20261119153500_AddShopProfile`, insert a minimal Manufacturer, Category, Model, Product and Cable via raw SQL (copy column lists from the precedent test; Cable columns: `Id, ProductId, Connector1, Connector2, Length, Notes, IsDeleted`), migrate to latest, assert those rows and values unchanged, `Headphones` exists with 0 rows, and `__EFMigrationsHistory` contains `20261121100000_AddHeadphones`.
- Depends on: Step 1.
- Edge cases / error handling: raw-SQL inserts must satisfy every NOT NULL column (`IsDeleted` 0) as of the `AddShopProfile` schema; use the temp-file SQLite pattern and dispose like neighbouring tests.
- Tests: the new test.
- Verify: CI green; the new test is listed in the CI log.
- Done when: the preservation test passes in CI and no existing test changed.
- Risk: MEDIUM
- Confidence: MEDIUM

## ~~[x] Step 3 — Sample-data wiring and ERD~~
- Files: inspect: `src/MobileShop.Tests/Dal/Initialization/SampleDataSeedTests.cs` (locate with `git ls-files | grep SampleDataSeedTests`); modify: `src/MobileShop.Dal/Initialization/SampleDataLoader.cs`, `SampleDataInitializer.cs`, `sample-data.json`, `docs/erd.mermaid`; do not touch: Web, Services, Api, tests.
- Symbols: `SampleDataSet.Headphones`; `SampleDataLoader.Load()`; clear/insert lists in `SampleDataInitializer`.
- Current -> Desired: dev seed ignores the new table -> dev seed knows it.
- Change: `SampleDataSet`: `public List<Headphone> Headphones { get; init; } = [];` after `PortableStorages`; `Load()`: `Headphones = Deserialize<Headphone>(root, "headphones")`; `SampleDataInitializer`: `ClearSet<Headphone>(context);` after `ClearSet<PortableStorage>` and `ProcessInsert(context, context.Headphones, data.Headphones);` after the `PortableStorages` line; JSON: `"headphones": []` beside `"portableStorages": []`. No sample rows (portable storages precedent), so seed-test counts stay valid. ERD: `PRODUCT ||--o| HEADPHONE : "profile"` next to the CABLE relation and a `HEADPHONE { ... }` block mirroring `CABLE`.
- Depends on: Step 1.
- Edge cases / error handling: keep relationships and IDs in `sample-data.json` stable.
- Tests: existing seed tests unchanged.
- Verify: CI green; `Development_startup_recreates_and_seeds_database_with_expected_counts` passes unchanged.
- Done when: seed test is untouched and green.
- Risk: LOW
- Confidence: HIGH

## ~~[x] Step 4 — Stage 1 validation~~
- Files: inspect only.
- Symbols: n/a.
- Current -> Desired: evidence recorded for the Reviewer.
- Change: none; collect `Action: #<run_number> — Success` for the head commit, `git diff --name-only <base>..HEAD` (must equal Steps 1-3 files) and the Step 1 grep result.
- Depends on: Steps 1-3.
- Edge cases / error handling: CI pending/unavailable -> report `Pending` / `Action: unavailable`, never claim success.
- Tests: full CI suite.
- Verify: CI green; no file outside the plan changed.
- Done when: evidence recorded in `act.md`.
- Risk: LOW
- Confidence: HIGH

---
# STAGE 2 — Services (no UI yet; nothing can create a headphone until Stage 3)

## ~~[x] Step 5 — Create path: input model, interface, service, API stub~~
- Files: inspect: `BindModels/CreatePortableStorageInputModel.cs`, `ProductsDataService.cs` (`CreatePortableStoragesAsync` ~979, `CreateAccessoryBatchAsync` ~1438, phone guarantee block ~687-695, `GetColorsAsync`), `ApiProductsDataService.cs`; create: `src/MobileShop.Models/ViewModels/Web/BindModels/CreateHeadphoneInputModel.cs`; modify: `src/MobileShop.Services/DataServices/Interfaces/IProductsDataService.cs`, `src/MobileShop.Services/DataServices/Dal/ProductsDataService.cs`, `src/MobileShop.Services/DataServices/Api/ApiProductsDataService.cs`, `src/MobileShop.Tests/Services/DataServices/Dal/ProductsDataServiceTests.cs`; do not touch: `CreateAccessoryBatchAsync` signature, `src/MobileShop.Api`.
- Symbols: `CreateHeadphoneInputModel` (`SellerId` [Range 1..], `ManufacturerId`, `ModelId` [Required], `HeadphoneKind Kind`, `HeadphoneConnection Connection`, `bool NoiseCancelling`, `CableConnector? Connector`, `bool? HasMicrophone`, `int? ColorId`, `bool HasGuarantee`, `[StringLength(100)] string? GuaranteeCorporation`, `[DataType(DataType.Date)] DateTime? GuaranteeExpiry`, `[StringLength(500)] string? GuaranteeNotes`, `Price`, `ProfitPercent`, `ProfitAmount`, `Count = 1`, same attributes as `CreatePortableStorageInputModel`); `Task<ServiceResult> CreateHeadphonesAsync(CreateHeadphoneInputModel input)`.
- Current -> Desired: no way to create a headphone -> batch create with optional colour and guarantee.
- Change: in `CreateHeadphonesAsync`: reject undefined `Kind` / `Connection` / non-null `Connector` (`Enum.IsDefined`, error field = property name); if `ColorId is > 0` and `colors.FindAsync` is null -> `"Selected color not found."` (`nameof(CreateHeadphoneInputModel.ColorId)`); if `GuaranteeExpiry` year outside 2000-2100 -> same message as the edit path; then `return await CreateAccessoryBatchAsync(..., "Headphone", product => { product.HeadphoneProfile = new Headphone { Kind, Connection, NoiseCancelling, Connector, HasMicrophone }; product.ColorId = input.ColorId is > 0 ? input.ColorId : null; if (input.HasGuarantee) product.GuaranteeProfile = new Guarantee { StartDate = DateTime.Today, ExpirationDate = input.GuaranteeExpiry ?? DateTime.Today.AddYears(1), Corporation = string.IsNullOrWhiteSpace(input.GuaranteeCorporation) ? "Shop Warranty" : input.GuaranteeCorporation.Trim(), Notes = NormalizeNote(input.GuaranteeNotes) }; })`. The lambda runs once per unit, so each unit gets its own `Guarantee` instance (one-to-one). Interface: add the method after `CreatePortableStoragesAsync`. Api stub: copy the existing `NotImplementedException` style.
- Depends on: Stage 1 (Steps 1-3).
- Edge cases / error handling: `Count < 1` handled by the batch method; no cross-validation between Connection and Connector (wireless earbuds charge over Lightning/USB-C, so any connector is valid); `HasGuarantee == false` ignores any guarantee fields submitted.
- Tests: in `ProductsDataServiceTests.cs`, mirroring the portable-storage tests (seed `new Category { Name = "Headphone" }` where those tests seed categories by name): creates N units with correct profile fields and one purchase transaction each; with colour + guarantee persists `ColorId` and a `Guarantee` per unit; without them persists none; rejects unknown colour; rejects undefined kind; rejects unknown model/manufacturer.
- Verify: CI green; new tests listed in the CI log.
- Done when: all new tests pass and no existing test changed.
- Risk: MEDIUM
- Confidence: HIGH

## ~~[x] Step 6 — Read paths: list, details, type detection, spec key, invoice extras, sell rows~~
- Files: inspect: `ProductsDataService.cs` (lines ~33-56 `EditIncludes`, ~68 + ~141 list, ~221 + ~308 details, ~1362 `GetProductType`), `TransactionsDataService.cs` (~226-232 sell rows, ~503-516 includes, ~562 extras), `ProductSpecExtensions.cs`, `InvoiceProfileExtensions.cs`, `ProductDetailsViewModel.cs`; modify: `ProductsDataService.cs`, `TransactionsDataService.cs`, `src/MobileShop.Models/Extensions/ProductSpecExtensions.cs`, `src/MobileShop.Models/Extensions/InvoiceProfileExtensions.cs`, `src/MobileShop.Models/ViewModels/Web/ProductDetailsViewModel.cs`, `ProductsDataServiceTests.cs`, matching Transactions/extension tests that already cover `PortableStorage`; do not touch: edit path (Step 7), `Details.cshtml` (Step 10).
- Symbols: include `"HeadphoneProfile"`; type key `"headphone"`; type label `"Headphone"`; `HeadphoneSegments(Headphone?)`; `GetInvoiceExtras(this Headphone)`; new init properties on `ProductDetailsViewModel`: `HeadphoneKind? HeadphoneKind`, `HeadphoneConnection? HeadphoneConnection`, `bool? NoiseCancelling`, `CableConnector? HeadphoneConnector`, `bool? HasMicrophone`.
- Current -> Desired: headphones invisible to every read path -> listed, detailed, grouped and invoiced.
- Change: (1) `EditIncludes` gets `"HeadphoneProfile"` (it is the include list used by `GetProductForEditAsync` and `UpdateProductAsync`). (2) List: add `"headphone"` to both allow-lists (`normalizedType is not (...)` ~68 and ~221) and add a `showAll || normalizedType == "headphone"` row block mirroring portable storage (~141), passing the colour name where `ProductListItemViewModel` has `Color`. (3) Details: add `"headphone"` branch (~308) building `ProductDetailsViewModel("Headphone", ...)` with `product.ColorNavigation?.Name` for `Color` and the same `GuaranteeLabel` expression the Tablet branch uses (~286), and set the five new `ProductDetailsViewModel` properties plus `Notes` from `product.HeadphoneProfile!`. The view model has no generic extras collection: each type-specific field is its own init property (precedent: `StorageKind`, `Speed`). (4) Type detection (`GetProductType` ~1362): `if (product.HeadphoneProfile is not null) return "Headphone";` after the portable-storage line. (5) `ToSpecKey`: `segments.AddRange(HeadphoneSegments(product.HeadphoneProfile));` after the portable-storage line; `HeadphoneSegments` returns 5 empty strings when null, else `Kind`, `Connection`, `NoiseCancelling`, `Connector?.ToString() ?? ""`, `HasMicrophone?.ToString() ?? ""` (colour is already in the key via `ColorId`, and `ToSpecLabel` already prints the colour). (6) `GetInvoiceExtras(this Headphone)`: labelled pairs, skipping unset `Connector` / `HasMicrophone`; guarantee lines already come from the generic `product.GuaranteeProfile?.GetInvoiceExtras()` in `BuildInvoice`. (7) Transactions: add `headphoneRows` beside `portableStorageRows` (~232) including the colour where the row model supports it; add `HeadphoneProfile` to `IncludeProductGraph` (~514-516); add the `GetProductExtras` case (~562).
- Depends on: Step 5.
- Edge cases / error handling: two headphones differing only in colour must produce different spec keys; unset `Connector` / `HasMicrophone` must not throw; a product with `HeadphoneProfile` must never also appear under another type.
- Tests: list returns headphones for tab `headphone` and `all`; details show colour and guarantee label (and `N/A` colour when unset) and expose the five headphone properties; spec-key differs by colour / kind; invoice extras content; sell rows include headphones and exclude already-sold units.
- Verify: CI green.
- Done when: new read-path tests pass; no existing assertion changed.
- Risk: MEDIUM
- Confidence: MEDIUM

## ~~[x] Step 7 — Edit path (service): input model, validation, apply, audit snapshot, colour/guarantee gating~~
- Files: inspect: `EditProductInputModel.cs` (~101 portable-storage block, existing `ColorId` / `Guarantee*` properties), `ProductsDataService.cs` (`UpdateProductAsync` ~1050: ~1077 `deviceFieldsEditable` + colour check, ~1143 per-type validation, ~1176-1210 apply, ~1273 type switch, ~1382 `IsDeviceType`, ~1385-1424 `CaptureEditableState`); modify: `src/MobileShop.Models/ViewModels/Web/BindModels/EditProductInputModel.cs`, `ProductsDataService.cs`, tests that cover the portable-storage edit path; do not touch: second-hand behaviour, `IsDeviceType` membership.
- Symbols: `private static bool SupportsColorAndGuarantee(string type) => IsDeviceType(type) || type == "Headphone";`; new input properties `HeadphoneKind? HeadphoneKind`, `HeadphoneConnection? HeadphoneConnection`, `bool? HeadphoneNoiseCancelling`, `CableConnector? HeadphoneConnector`, `bool? HeadphoneHasMicrophone`.
- Current -> Desired: headphone cannot be edited -> its own fields, colour and guarantee are editable; second-hand is not offered.
- Change: (1) add `SupportsColorAndGuarantee` and use it for the colour existence check (~1078), the colour assignment (~1176) and the guarantee upsert block (~1199-1207); keep `deviceFieldsEditable` (= `IsDeviceType`) for the second-hand block only. Behaviour for Phone / Tablet / Smart Watch / Laptop must stay byte-for-byte equivalent. (2) `CaptureEditableState`: guarantee snapshot uses `SupportsColorAndGuarantee` (without this a guarantee-only edit would report "No changes were made."); second-hand snapshot stays `IsDeviceType`; add a `"Headphone"` case capturing Kind, Connection, NoiseCancelling, Connector, HasMicrophone, Notes. (3) Validation (~1143 pattern): for `"Headphone"`, a supplied `HeadphoneKind` / `HeadphoneConnection` / `HeadphoneConnector` must be a defined enum value. (4) Apply switch: `case "Headphone"`: `Kind = input.HeadphoneKind ?? existing`; `Connection = input.HeadphoneConnection ?? existing`; `NoiseCancelling = input.HeadphoneNoiseCancelling ?? existing`; optional `Connector = input.HeadphoneConnector` and `HasMicrophone = input.HeadphoneHasMicrophone` assigned directly so they can be cleared back to unset; `Notes = NormalizeNote(input.Notes)`.
- Depends on: Steps 5-6.
- Edge cases / error handling: existing semantics kept — guarantee fields only upsert when corporation or expiry is submitted (no remove control); colour cannot be cleared back to none (same as devices); only the Razor form posts these fields, so direct assignment of the two optional fields is safe.
- Tests: add Headphone to the existing edit theories/tests that enumerate types (portable-storage precedent in `EditModelTests.cs` ~219/244/481 and the service tests); new tests: colour change persists, guarantee upsert persists, second-hand flag is ignored for headphones, optional connector/microphone can be cleared, undefined enum rejected; plus a regression assertion that Phone/Tablet/Smart Watch/Laptop edits still persist colour, second-hand and guarantee.
- Verify: CI green with all pre-existing edit tests unchanged.
- Done when: new and existing edit tests pass.
- Risk: HIGH (shared edit method used by all types)
- Confidence: MEDIUM

## ~~[x] Step 8 — Stage 2 validation~~
- Files: inspect only.
- Symbols: n/a.
- Current -> Desired: evidence recorded.
- Change: none; collect CI run number for the head commit and `git diff --name-only` against the Stage 2 file lists (Steps 5-7).
- Depends on: Steps 5-7.
- Edge cases / error handling: as Step 4.
- Tests: full CI suite.
- Verify: CI green; no file outside the plan changed; `src/MobileShop.Api` untouched.
- Done when: evidence recorded in `act.md`.
- Risk: LOW
- Confidence: HIGH

---
# STAGE 3 — Web UI

## ~~[x] Step 9 — Create page, tab, navigation~~
- Files: inspect: `Pages/Products/CreatePortableStorage.cshtml(.cs)`, `CreateSmartWatch.cshtml(.cs)` (guarantee markup + Jalali date + scripts), `CreatePhone.cshtml` (colour select), `Pages/Products/Index.cshtml` (~17 tab tuples), `ViewComponents/NavigationMenuViewComponent.cs` (~29), `Tests/Web/Pages/Products/CreateAccessoryModelTests.cs`; create: `src/MobileShop.Web/Pages/Products/CreateHeadphone.cshtml` and `.cshtml.cs`; modify: `Pages/Products/Index.cshtml`, `NavigationMenuViewComponent.cs`, `CreateAccessoryModelTests.cs`; do not touch: `product-create-modal.js`, `ProductCreateRegistry.cs`, `_ProductCreate*Form.cshtml` (dead code).
- Symbols: `CreateHeadphoneModel(IProductsDataService dataService) : PageModel` with `Input`, `Manufacturers`, `Models`, `Colors`, `Corporations`, `OnGetAsync`, `OnGetModelsAsync` (category `"Headphone"`), `OnPostCreateManufacturerAsync`, `OnPostCreateModelAsync` (category `"Headphone"`), `OnPostAsync`; tab tuple `("headphone", "Headphones", "/Products/CreateHeadphone", "Create headphone")`; nav link `Link("Add headphone", "Register headphones, earbuds or headsets in stock.", "/Products/CreateHeadphone", "nav-subicon-plus")`.
- Current -> Desired: no create UI -> page reachable from the Products tabs and the nav menu.
- Change: copy `CreatePortableStorage` for structure, seller picker, manufacturer/model modals, pricing and Count (including its `<partial name="/Pages/Shared/_ProductCreateSuccess.cshtml" ...>` line); replace the capacity section with: Kind select, Connection select, Noise cancelling checkbox, Connector select (`-- None --` option + `CableConnector` values), Microphone select (Unknown / Yes / No -> `bool?`), Colour select (`-- No color --`, `Model.Colors`, no add-colour button), and the Guarantee section copied from `CreateSmartWatch` (toggle `HasGuarantee`, corporation select with `Shop Warranty` default option, Jalali expiry input, notes) including its toggle script and whatever Jalali handling `CreateSmartWatch` relies on. Placement: tab tuple after `portablestorage`, nav link after "Add portable storage". Success redirects with `SuccessMessage = "Headphones created successfully."` like the neighbours.
- Depends on: Stage 2.
- Edge cases / error handling: model errors re-populate dropdowns (`PopulateAsync` incl. `Colors` and `Corporations`); service `ErrorField` mapped with `ModelState.AddModelError`; Count default 1.
- Tests: in `CreateAccessoryModelTests.cs`, mirroring `PortableStorage_*`: `Headphone_Create_Works` (units + colour + guarantee persisted), `Headphone_AddModel_CreatesHeadphoneCategoryModel` (asserts `Categories.Single(c => c.Name == "Headphone")`), invalid model state re-populates dropdowns.
- Verify: CI green.
- Done when: page tests pass; tab and nav entries present.
- Risk: MEDIUM
- Confidence: MEDIUM

## ~~[x] Step 10 — Edit page, details block, index mapping~~
- Files: inspect: `Pages/Products/Edit.cshtml.cs` (`PopulateDropdownsAsync` ~128-150, `MapToEditModel` ~190-240, type + notes ~250-276), `Pages/Shared/_ProductEditForm.cshtml` (Smart Watch block ~132-184, Portable Storage block ~379-387), `Pages/Products/Details.cshtml` (Portable Storage block ~112-120), `Pages/Products/Index.cshtml.cs` (~147-160); modify: `Edit.cshtml.cs`, `_ProductEditForm.cshtml`, `Details.cshtml`, `Index.cshtml.cs`, `Tests/Web/Pages/Products/EditModelTests.cs`; do not touch: other type blocks, `Details.cshtml.cs` (its `indexType` switches already lowercase `Headphone` to `headphone` in the default branch).
- Symbols: `"Headphone"` type string; `GetCategoryName` entry `"headphone" => "Headphone"`.
- Current -> Desired: a headphone opens no edit form and its Details page shows no headphone fields -> full edit with colour and guarantee, and a Details block.
- Change: `Edit.cshtml.cs`: add `"Headphone"` to the `Colors` and `Corporations` loading conditions; map `HeadphoneKind`, `HeadphoneConnection`, `HeadphoneNoiseCancelling`, `HeadphoneConnector`, `HeadphoneHasMicrophone` from `product.HeadphoneProfile?` into the `EditProductInputModel` that `MapToEditModel` builds; type detection `if (product.HeadphoneProfile != null) return "Headphone";` (after portable storage); `GetProfileNotes`: `"Headphone" => product.HeadphoneProfile?.Notes`. `_ProductEditForm.cshtml`: new `@if (Model.Product.Type == "Headphone")` block with sections "Headphone details" (Kind, Connection via enum selects like the `StorageKind` select; Noise cancelling; Connector select `-- None --` + `Model.CableConnectors`; Microphone tri-state select; Colour select with `Model.Colors`, no add button), "Guarantee" (corporation, expiry with the same `data-jalali-date` value expression as the Smart Watch block, notes — rendered VISIBLE, i.e. without `style="display:none"`), and Notes. No Second-hand section. `Details.cshtml`: new `@if (Model.Product.Type == "Headphone")` block after the Portable Storage block showing Kind, Connection, Noise cancelling (Yes/No), Connector (`N/A` when unset) and Microphone (Yes/No/`N/A`) from the Step 6 view-model properties; colour, second-hand and guarantee rows are already generic. `Index.cshtml.cs`: `"headphone" => "Headphone"` in `GetCategoryName`.
- Depends on: Steps 6, 7, 9.
- Edge cases / error handling: posting without the optional selects clears Connector / Microphone (intended); an existing headphone with no guarantee shows empty guarantee fields and stays without one until corporation or expiry is submitted.
- Tests: in `EditModelTests.cs` add Headphone to the type-enumerating helpers (precedent ~219/244/481) plus tests: `OnGet` maps all headphone fields and loads colours and corporations; `OnPost` persists changes. If a Details page-model test exists for other types, add Headphone to it.
- Verify: CI green; existing edit-page tests unchanged.
- Done when: new edit tests pass and the Details block renders for a headphone.
- Risk: MEDIUM
- Confidence: MEDIUM

## ~~[x] Step 11 — Final validation and docs~~
- Files: inspect: `README.md`, `docs/*` (grep for lists of product types); modify only where such a list exists (add Headphone); do not create new docs.
- Symbols: n/a.
- Current -> Desired: all stages proven end to end.
- Change: docs touch-up if applicable; collect evidence: CI run number for the head commit, `git diff --name-only <base>..HEAD` equal to the union of all step file lists, the Step 1 grep result, and the confirmation that `installer-smoke` / `DatabaseMigratorTests` (legacy DB backed up before upgrade) still pass.
- Depends on: Steps 1-10.
- Edge cases / error handling: CI pending/unavailable -> `Pending` / `Action: unavailable`; never claim success.
- Tests: full CI suite.
- Verify: CI green; manual smoke checklist for the user: create 2 headphones with colour + guarantee, see them in the `Headphones` tab, open details, edit colour and guarantee, sell one and check the invoice lines, then run an update over a DB that already has data and confirm nothing existing changed.
- Done when: evidence recorded in `act.md`; Global Definition of Done met.
- Risk: LOW
- Confidence: HIGH

## Global Definition of Done
- Headphone type works end to end: create (with optional colour and guarantee), list tab, details, edit, invoice extras, sell picker.
- The only schema change is one new table (`Headphones`) via an additive migration; existing rows proven preserved by a test.
- No change in behaviour for existing product types; no edit to existing migrations or `src/MobileShop.Api`.
- CI green with a cited run number; no unresolved issues.
