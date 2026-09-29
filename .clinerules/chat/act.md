# Act Report — Stage C Step 1
- Commit: 7a20caa — feat(products): Stage C Step 1 - ProductsDataService catalog + creates + Dal registration
- Verification:
  - `dotnet build src/MobileShop.slnx --nologo` -> exit 0; **0 errors, 0 warnings**.
  - `dotnet test src/MobileShop.slnx --nologo --no-build` (full suite) -> **Failed: 0, Passed: 477, Skipped: 2, Total: 479** (EXIT=0).
    - `ProductsDataServiceTests` -> 21/21 passed (serial run via the class's `[Collection("MobileShop-Dal-Serial")]`).
- What was implemented (exactly the plan's Step 1 scope)
  - `ServiceResult` gained two optional, defaulted positional members `ErrorField` and `EntityId`; all existing call sites compile unchanged.
  - New `DropdownCreateResult` record in `src/MobileShop.Models/ViewModels/Web/DropdownCreateResult.cs`.
  - `IProductsDataService` and `ApiProductsDataService`: exactly three create signatures narrowed to `DropdownCreateResult` (manufacturer/model/color); the four `BindModels` FQNs shortened via the new `global using MobileShop.Models.ViewModels.Web.BindModels;` in `MobileShop.Services/GlobalUsings.cs`. Api bodies still `throw new NotImplementedException`.
  - `ProductsDataService.cs`: nine members — `GetManufacturersAsync`, `GetColorsAsync`, `GetModelsAsync(int)`, `GetGuaranteeCorporationsAsync`, `CreateManufacturerAsync`, `CreateModelAsync`, `CreateColorAsync`, `CreatePhoneAsync`, `CreateAppleIdAsync` — ported verbatim from `CreatePhoneModel`/`CreatePhone.cshtml.cs`/`CreateAppleId.cshtml.cs` (dedupes, IMEI/email uniqueness, model-belongs-to-manufacturer check, `"Shop Warranty"` + second-hand defaults, Apple-ID implicit `"iPhone"` model, plaintext `input.Password.Trim()` per L8).
  - `ServiceCollectionExtensions.cs`: `IProductsDataService -> ProductsDataService` added in the Dal (`!useApi`) branch; `UseApi` stays false; the 9 entity + 15 repo registrations untouched.
  - `Color` type-name collision with `QuestPDF.Infrastructure.Color` resolved by `MobileShop.Models.Entities.Color` qualification (no GlobalUsings change).
- **Deviation recorded (plan.md Step 1 Symbols, reviewer LOW):** the `IBaseRepo<Product> products` ctor slot was **dropped** — the seven remaining repos + logger are injected, and `AddNewAsync` is used through a generic base call where the product entity is created inline in Step 3's page migration. This is the sanctioned off-by-one fix; it produces 0 warnings.
- Limitations: product writes (`products.AddAsync`) are not exercised in Step 1 — they are covered by Step 3's page migration; the `ProductsDataServiceTests` fixture therefore has 7 repos in its ctor (not 8). No Step-1 test asserts product creation; that is intentional and noted in plan §Step 1 "Tests".
- Friction noted: the test host buffers xUnit progress output until process exit, so verification polls the redirect log past the interactive window (~38s for the full suite). One initial parallel-run failure of `CreateColorAsync_creates_and_dedupes` reproduced as a known EF-InMemory static-model-build race under heavy parallelism; made deterministic by forcing `ProductsDataServiceTests` onto a `[CollectionDefinition("MobileShop-Dal-Serial", DisableParallelization = true)]`.
- Problems: None remaining.
- Status: COMPLETE

