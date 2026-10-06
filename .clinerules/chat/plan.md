# Plan — Stage AA — Device product pages (Tablet, Smart watch, Laptop)

## Requirements
- **Functional (from `to-do.md`)**:
  - Implement full support for **Tablet**, **Smart watch**, and **Laptop** across the product stack using the existing database entities (`Tablet`, `SmartWatch`, `Laptop`, `Product`, `DeviceSpec`, `SecondHand`, `Guarantee`).
  - **Services**: Create, details, inventory list, second-hand list, and selectable transactions list for all three device types.
  - **Pricing & Rules**: Follow phone/glass rules — integer IRR prices bounded by `MoneyLimits.MaxRials`, optional profit percentage or profit amount, computed finished price, optional guarantee, optional second-hand notes, and barcode generation.
  - **Create Pages**: Dedicated Razor Pages for each type (`/Products/CreateTablet`, `/Products/CreateSmartWatch`, `/Products/CreateLaptop`).
  - **List & Details Integration**: Integrated into `/Products/Index` (type filter buttons: Tablets, Smart Watches, Laptops; availability filter: All, Available, Sold) and `/Products/Details` (specifications, guarantee, second-hand status, and transaction history).
  - **Buy Modal Registry**: Registered in the Stage Z create-product popup (`ProductCreateRegistry`), allowing inline creation on `/Transactions/Buy` for all three device types.
- **Non-functional**:
  - CI gate on GitHub Actions; no schema/migration changes; no Api host/auth changes.
- **Constraints**:
  - No database migration or entity modifications; `sample-data.json` counts must remain stable (0 seeded tablets/watches/laptops asserted in `SampleDataSeedTests`).

## Decisions (labelled)
- **D1 — Category Names & Model Lookup:** Category names match the entity/schema naming: `"Tablet"`, `"SmartWatch"`, and `"Laptop"`. Models for each device type are linked to their corresponding category.
- **D2 — Identifiers for Inventory & Picker:** Each device has a unique barcode generated on `Product.Barcode` (`Guid.NewGuid().ToString("N")[..12]`). Display identifier is `"Barcode: " + product.Barcode`.
- **D3 — Part Numbers:** The `PartNumber` table is linked exclusively to `Phone` entities in the schema; Tablets, Smart Watches, and Laptops report `PartNumberLabel = "N/A"`.
- **D4 — Device Specifications & View Model Contract:** `ProductDetailsViewModel` is extended with optional `init` properties (`Cpu`, `Gpu`, `DisplaySize`, `Notes`), keeping existing positional constructor calls source-compatible. `Laptop` populates CPU, GPU, DisplaySize, and Notes; `Tablet` and `SmartWatch` populate Notes.
- **D5 — Selectable for Transactions:** `TransactionsDataService.GetSelectableProductsAsync` and `SearchSelectableProductsAsync` include unsold Tablets, Smart Watches, and Laptops, making them selectable for Buy and Sell operations.
- **D6 — Registry Keys:** Modal registry keys are `"tablet"`, `"smartwatch"`, and `"laptop"`, rendering streamlined forms in the `/Transactions/Buy` modal.
- **A1:** CI gate; GitHub Actions run number is required for every step verification.
- **A2:** Api service stubs (`ApiProductsDataService`) implement new interface members by throwing `NotImplementedException`.

## Reviewer Briefing
- **Step 1 LOW/HIGH:** Bind models, `ProductDetailsViewModel` extension, service contracts, DAL implementations, and DAL unit tests (completed).
- **Step 2 MEDIUM/HIGH:** Razor create pages (`CreateTablet`, `CreateSmartWatch`, `CreateLaptop`), Details page enhancements, and Products Index filter buttons.
- **Step 3 MEDIUM/MEDIUM:** Modal registry entries (`ProductCreateRegistry`), partial form views, and `Buy.cshtml.cs` POST handlers.
- **Step 4 LOW/HIGH:** Final Stage AA verification: full test suite, 0 warnings, clean diff stat check.

## ~~[x] Step 1 — Input models, service contracts, DAL implementations & tests~~
- Files: input models created, `ProductDetailsViewModel` extended, `IProductsDataService` + `ProductsDataService` + `TransactionsDataService` updated, unit tests added.
- Completed: CI Action #520 — Success.
- Risk: LOW · Confidence: HIGH

## [ ] Step 2 — Dedicated Create pages, Details rendering & Products list integration
- **Files**:
  - `create`:
    - `src/MobileShop.Web/Pages/Products/CreateTablet.cshtml` & `.cshtml.cs`
    - `src/MobileShop.Web/Pages/Products/CreateSmartWatch.cshtml` & `.cshtml.cs`
    - `src/MobileShop.Web/Pages/Products/CreateLaptop.cshtml` & `.cshtml.cs`
    - `src/MobileShop.Tests/Web/Pages/Products/CreateDeviceModelTests.cs`
  - `modify`:
    - `src/MobileShop.Web/Pages/Products/Index.cshtml`
    - `src/MobileShop.Web/Pages/Products/Details.cshtml`
- **Symbols**:
  - `CreateTabletModel`, `CreateSmartWatchModel`, `CreateLaptopModel`
  - `IndexModel`, `DetailsModel`
- **Change**:
  - Implement the three create pages with cascading manufacturer/model selects, pricing calculator JS, second-hand toggle, and guarantee details.
  - Update `Index.cshtml` to add type filter buttons ("Tablets", "Smart Watches", "Laptops") and create buttons ("Create tablet", "Create smart watch", "Create laptop").
  - Update `Details.cshtml` to display laptop specs (CPU, GPU, Screen size) and device notes.
  - Add Razor Page unit tests covering GET/POST workflows for all three device create pages.
- **Done when**: Pages render and post correctly, unit tests pass, CI is green, Job B PASS.
- **Risk**: MEDIUM · **Confidence**: HIGH

## [ ] Step 3 — Buy modal integration & Product Create Registry
- **Files**:
  - `create`:
    - `src/MobileShop.Web/Pages/Shared/_ProductCreateTabletForm.cshtml`
    - `src/MobileShop.Web/Pages/Shared/_ProductCreateSmartWatchForm.cshtml`
    - `src/MobileShop.Web/Pages/Shared/_ProductCreateLaptopForm.cshtml`
  - `modify`:
    - `src/MobileShop.Web/Pages/Shared/ProductCreateRegistry.cs`
    - `src/MobileShop.Web/Pages/Transactions/Buy.cshtml.cs`
    - `src/MobileShop.Web/wwwroot/js/product-create-modal.js`
    - `src/MobileShop.Tests/Web/Pages/Transactions/BuyModelTests.cs`
- **Symbols**:
  - `ProductCreateRegistry`
  - `BuyModel.OnGetCreateProductFormAsync`, `OnPostCreateTabletAsync`, `OnPostCreateSmartWatchAsync`, `OnPostCreateLaptopAsync`
- **Change**:
  - Register `"tablet"`, `"smartwatch"`, and `"laptop"` in `ProductCreateRegistry`.
  - Add modal partial forms for Tablet, Smart Watch, and Laptop.
  - Implement form loaders and POST create handlers in `Buy.cshtml.cs`, returning `{ productId, type, label, suggestedPrice }`.
  - Add client-side modal registry entries in `product-create-modal.js`.
  - Add unit tests in `BuyModelTests.cs` verifying modal creation for each device type.
- **Done when**: Devices can be created from the Buy modal and auto-selected, CI is green, Job B PASS.
- **Risk**: MEDIUM · **Confidence**: MEDIUM

## [ ] Step 4 — Final Stage AA validation & sign-off
- **Files**:
  - `modify`:
    - `.clinerules/chat/act.md`
    - `.clinerules/chat/audit.md`
    - `.clinerules/chat/plan.md`
    - `.clinerules/to-do.md` (Reviewer only)
- **Change**:
  - Full CI test suite run: zero warnings, zero failed tests, production smoke passes.
  - Scope verification: confirm no schema, migration, entity, or authentication modifications.
  - Reviewer signs off Stage AA in `.clinerules/to-do.md`.
- **Done when**: Full CI run is green, Global Definition of Done is satisfied, Stage AA is checked off.
- **Risk**: LOW · **Confidence**: HIGH

## Global Definition of Done
- Tablets, Smart Watches, and Laptops can be created, viewed in details, and listed on `/Products/Index` with working availability filters.
- Tablets, Smart Watches, and Laptops can be created directly from the `/Transactions/Buy` modal and are immediately selectable on transaction forms.
- Suggested price and integer Rial constraints are enforced.
- Zero schema or migration changes; 100% CI pass rate with 0 build warnings.

## Execution notes
One step → one commit → green Action → STOP for Job B. Do not edit `to-do.md`, `plan.md`, or `audit.md` during execution. Do not run local `dotnet`.