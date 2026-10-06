# Plan — Stage AB — Accessory product pages (Cable, Charger, Power bank, Portable storage, Case)

## Requirements
- **Functional (from `to-do.md`)**:
  - Implement full support for **Cable**, **Charger**, **Power bank**, **Portable storage**, and **Case** across the product stack using the existing database entities (`Cable`, `Charger`, `PowerBank`, `PortableStorage`, `Case`, `CaseModelFit`, `Product`).
  - **Glass Rules**: Bulk count in one submit (`Count >= 1`), integer IRR pricing bounded by `MoneyLimits.MaxRials`, optional profit percentage or profit amount, computed finished price, and unique barcode generation per unit.
  - **Case Model Fits**: Phone cases retain their explicit many-to-many model fits join (`CaseModelFit`) to link compatible phone/device models.
  - **Services**: Create batch methods, details retrieval, inventory rows, and selectable transaction queries for all 5 accessory types.
  - **Create Pages**: Dedicated Razor Pages for each accessory (`CreateCable`, `CreateCharger`, `CreatePowerBank`, `CreatePortableStorage`, `CreateCase`).
  - **List & Details Integration**: Integrated into `/Products/Index` (type filter buttons and create buttons) and `/Products/Details` (accessory-specific specifications and transaction history).
  - **Buy Modal Registry**: Registered in the create-product popup (`ProductCreateRegistry`), allowing inline bulk creation on `/Transactions/Buy`.
- **Non-functional**:
  - CI gate on GitHub Actions; no schema/migration changes; no Api host/auth changes; `sample-data.json` counts must remain undisturbed.

## Decisions (labelled)
- **D1 — Category Names & Model Structure:**
  - `Cable` uses `"Cable"`, `Charger` uses `"Charger"`, `PowerBank` uses `"PowerBank"`, `Case` uses `"Case"`, and `PortableStorage` uses `"PortableStorage"`.
  - For accessories with compatible models (`Case`), the model name format follows Glass: `<CompatibleModel> Case`, associating `CaseModelFit` entries.
- **D2 — Bulk Count Creation:**
  - All 5 input models include `[Range(1, int.MaxValue)] int Count = 1`. The service creates `Count` individual `Product` rows with unique barcodes, returning the first created entity ID in `ServiceResult.EntityId`.
- **D3 — Pricing Calculation:**
  - Identical to Glass/Phone: `TryComputeFinishedPrice(input.Price, input.ProfitPercent, input.ProfitAmount, out var finishedPrice)` with `MoneyLimits.MaxRials`.
- **D4 — ProductDetailsViewModel Extension:**
  - Add optional `init` properties for accessory attributes so existing call sites remain untouched:
    - `Cable`: `CableConnector? Connector1`, `CableConnector? Connector2`, `decimal? CableLength`
    - `Charger`: `int? Wattage`, `bool? Pd`, `int? PortCount`
    - `PowerBank`: `int? CapacityMah`, `int? MaxWattage`
    - `PortableStorage`: `StorageKind? StorageKind`, `string? StorageCapacityLabel`, `int? Speed`
    - `Case`: `IReadOnlyList<string> CompatibleModels`
- **D5 — Selectable for Transactions:**
  - `TransactionsDataService.GetSelectableProductsAsync` queries unsold accessory products via `products.SelectAllAsync(p => p.<Profile> != null && ...)` and concatenates them for Buy/Sell search.
- **D6 — Registry Keys:**
  - `"cable"`, `"charger"`, `"powerbank"`, `"portablestorage"`, `"case"` registered in `ProductCreateRegistry` with matching modal form partials.
- **A1:** CI gate; GitHub Actions run number is required for every step verification.
- **A2:** Api service stubs implement new interface members with `throw new NotImplementedException();`.

## Reviewer Briefing
- **Step 1 LOW/HIGH:** Bind models, `ProductDetailsViewModel` extension, service contracts, DAL implementations, and DAL unit tests.
- **Step 2 MEDIUM/HIGH:** Razor create pages (`CreateCable`, `CreateCharger`, `CreatePowerBank`, `CreatePortableStorage`, `CreateCase`), Details page enhancements, and Products Index filter buttons.
- **Step 3 MEDIUM/MEDIUM:** Modal registry entries (`ProductCreateRegistry`), partial form views, client-side JS registry, and `Buy.cshtml.cs` POST handlers.
- **Step 4 LOW/HIGH:** Final Stage AB verification: full test suite, 0 warnings, clean diff stat check.

## [ ] Step 1 — Input models, view model extensions, service contracts & DAL implementations
- **Files**:
  - `create`:
    - `src/MobileShop.Models/ViewModels/Web/BindModels/CreateCableInputModel.cs`
    - `src/MobileShop.Models/ViewModels/Web/BindModels/CreateChargerInputModel.cs`
    - `src/MobileShop.Models/ViewModels/Web/BindModels/CreatePowerBankInputModel.cs`
    - `src/MobileShop.Models/ViewModels/Web/BindModels/CreatePortableStorageInputModel.cs`
    - `src/MobileShop.Models/ViewModels/Web/BindModels/CreateCaseInputModel.cs`
  - `modify`:
    - `src/MobileShop.Models/ViewModels/Web/ProductDetailsViewModel.cs`
    - `src/MobileShop.Services/DataServices/Interfaces/IProductsDataService.cs`
    - `src/MobileShop.Services/DataServices/Dal/ProductsDataService.cs`
    - `src/MobileShop.Services/DataServices/Dal/TransactionsDataService.cs`
    - `src/MobileShop.Services/DataServices/Api/ApiProductsDataService.cs`
    - `src/MobileShop.Tests/Services/DataServices/Dal/ProductsDataServiceTests.cs`
    - `src/MobileShop.Tests/Services/DataServices/Dal/TransactionsDataServiceTests.cs`
- **Symbols**:
  - `CreateCableInputModel`, `CreateChargerInputModel`, `CreatePowerBankInputModel`, `CreatePortableStorageInputModel`, `CreateCaseInputModel`
  - `ProductDetailsViewModel` (`Connector1`, `Connector2`, `CableLength`, `Wattage`, `Pd`, `PortCount`, `CapacityMah`, `MaxWattage`, `StorageKind`, `StorageCapacityLabel`, `Speed`, `CompatibleModels`)
  - `IProductsDataService.CreateCablesAsync`, `CreateChargersAsync`, `CreatePowerBanksAsync`, `CreatePortableStoragesAsync`, `CreateCasesAsync`
  - `ProductsDataService.GetInventoryRowsAsync`, `GetDetailsAsync`
  - `TransactionsDataService.GetSelectableProductsAsync`
- **Change**:
  - Create input models supporting bulk `Count >= 1`, price, profit calculation, manufacturer/model, and specific entity fields.
  - Extend `ProductDetailsViewModel` with optional `init` properties for accessory attributes.
  - Implement bulk creation methods in `ProductsDataService` using `products.AddRangeAsync(...)` and query via `IBaseRepo<Product> products` without modifying constructor signatures.
  - Extend `GetInventoryRowsAsync` and `GetDetailsAsync` to support the 5 accessory types.
  - Extend `TransactionsDataService.GetSelectableProductsAsync` to include unsold accessory products.
  - Add unit tests verifying creation, inventory listing, details projection, and selectable product queries.
- **Done when**: Build succeeds with 0 warnings, unit tests pass, CI is green, Job B PASS.
- **Risk**: LOW · **Confidence**: HIGH

## [ ] Step 2 — Dedicated Create pages, Details rendering & Products list integration
- **Files**:
  - `create`:
    - `src/MobileShop.Web/Pages/Products/CreateCable.cshtml` & `.cshtml.cs`
    - `src/MobileShop.Web/Pages/Products/CreateCharger.cshtml` & `.cshtml.cs`
    - `src/MobileShop.Web/Pages/Products/CreatePowerBank.cshtml` & `.cshtml.cs`
    - `src/MobileShop.Web/Pages/Products/CreatePortableStorage.cshtml` & `.cshtml.cs`
    - `src/MobileShop.Web/Pages/Products/CreateCase.cshtml` & `.cshtml.cs`
    - `src/MobileShop.Tests/Web/Pages/Products/CreateAccessoryModelTests.cs`
  - `modify`:
    - `src/MobileShop.Web/Pages/Products/Index.cshtml`
    - `src/MobileShop.Web/Pages/Products/Details.cshtml`
- **Symbols**:
  - `CreateCableModel`, `CreateChargerModel`, `CreatePowerBankModel`, `CreatePortableStorageModel`, `CreateCaseModel`
  - `IndexModel`, `DetailsModel`
- **Change**:
  - Implement Razor create pages for each accessory with model selects, count input, pricing calculator, and attribute controls (connector types, wattage, capacity, storage kind, case model fits).
  - Update `Index.cshtml` to add type filter buttons ("Cables", "Chargers", "Power Banks", "Portable Storages", "Cases") and matching create buttons.
  - Update `Details.cshtml` to render specifications for all accessory types.
  - Add unit tests covering GET/POST workflows for all 5 accessory create pages.
- **Done when**: Pages render and post correctly, unit tests pass, CI is green, Job B PASS.
- **Risk**: MEDIUM · **Confidence**: HIGH

## [ ] Step 3 — Buy modal integration & Product Create Registry
- **Files**:
  - `create`:
    - `src/MobileShop.Web/Pages/Shared/_ProductCreateCableForm.cshtml`
    - `src/MobileShop.Web/Pages/Shared/_ProductCreateChargerForm.cshtml`
    - `src/MobileShop.Web/Pages/Shared/_ProductCreatePowerBankForm.cshtml`
    - `src/MobileShop.Web/Pages/Shared/_ProductCreatePortableStorageForm.cshtml`
    - `src/MobileShop.Web/Pages/Shared/_ProductCreateCaseForm.cshtml`
  - `modify`:
    - `src/MobileShop.Web/Pages/Shared/ProductCreateRegistry.cs`
    - `src/MobileShop.Web/Pages/Transactions/Buy.cshtml.cs`
    - `src/MobileShop.Web/wwwroot/js/product-create-modal.js`
    - `src/MobileShop.Tests/Web/Pages/Transactions/BuyModelTests.cs`
- **Symbols**:
  - `ProductCreateRegistry`
  - `BuyModel.OnGetCreateProductFormAsync`, `OnPostCreateCableAsync`, `OnPostCreateChargerAsync`, `OnPostCreatePowerBankAsync`, `OnPostCreatePortableStorageAsync`, `OnPostCreateCaseAsync`
- **Change**:
  - Register `"cable"`, `"charger"`, `"powerbank"`, `"portablestorage"`, `"case"` in `ProductCreateRegistry`.
  - Add modal partial forms for each accessory type.
  - Implement form loaders and POST create handlers in `Buy.cshtml.cs`, returning `{ productId, type, label, suggestedPrice }`.
  - Add client-side modal registry entries in `product-create-modal.js`.
  - Add unit tests in `BuyModelTests.cs` verifying modal creation for each accessory type.
- **Done when**: Accessories can be bulk created from the Buy modal and auto-selected, CI is green, Job B PASS.
- **Risk**: MEDIUM · **Confidence**: MEDIUM

## [ ] Step 4 — Final Stage AB validation & sign-off
- **Files**:
  - `modify`:
    - `.clinerules/chat/act.md`
    - `.clinerules/chat/audit.md`
    - `.clinerules/chat/plan.md`
    - `.clinerules/to-do.md` (Reviewer only)
- **Change**:
  - Full CI test suite run: zero warnings, zero failed tests, production smoke passes.
  - Scope verification: confirm no schema, migration, entity, or authentication modifications.
  - Reviewer signs off Stage AB in `.clinerules/to-do.md`.
- **Done when**: Full CI run is green, Global Definition of Done is satisfied, Stage AB is checked off.
- **Risk**: LOW · **Confidence**: HIGH

## Global Definition of Done
- Cables, Chargers, Power banks, Portable storages, and Cases can be created in bulk, viewed in details, and listed on `/Products/Index` with working availability filters.
- All 5 accessory types can be created directly from the `/Transactions/Buy` modal and are immediately selectable on transaction forms.
- Case model fits, suggested prices, and integer Rial constraints are enforced.
- Zero schema or migration changes; 100% CI pass rate with 0 build warnings.

## Execution notes
One step → one commit → green Action → STOP for Job B. Do not edit `to-do.md`, `plan.md`, or `audit.md` during execution. Do not run local `dotnet`.