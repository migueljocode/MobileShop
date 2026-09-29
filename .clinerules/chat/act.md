# Act Report — Stage C Step 1
- Commit: 7a20caa — feat(products): Stage C Step 1 - ProductsDataService catalog + creates + Dal registration
- Verification:
  - `dotnet build src/MobileShop.slnx --nologo` -> exit 0; **0 errors, 0 warnings**.
  - `dotnet test src/MobileShop.slnx --nologo --no-build` (full suite) -> **Failed: 0, Passed: 477, Skipped: 2, Total: 479** (EXIT=0).
    - `ProductsDataServiceTests` -> 21/21 passed (serial run via the class's `[Collection("MobileShop-Dal-Serial")]`).
- What was implemented (plan Step 1 scope; the actor also ported the three read members at once, so
  Step 2 now only *extends* them rather than adding them — see plan.md §Step 2 rescope):
  - Contract: `ServiceResult` gained two optional, defaulted positional members `ErrorField` and `EntityId`; new
    `DropdownCreateResult` record. `IProductsDataService`/`ApiProductsDataService`: three create signatures
    narrowed to `DropdownCreateResult`; four `BindModels` FQNs shortened via the new `global using` in
    `MobileShop.Services/GlobalUsings.cs`. Api bodies still `throw new NotImplementedException`.
  - `ProductsDataService.cs`: 12 members — the 9 creates/dropdowns (`GetManufacturersAsync`,
    `GetColorsAsync`, `GetModelsAsync(int)`, `GetGuaranteeCorporationsAsync`, `CreateManufacturerAsync`,
    `CreateModelAsync`, `CreateColorAsync`, `CreatePhoneAsync`, `CreateAppleIdAsync`) plus the three read members
    `GetInventoryRowsAsync`, `GetSecondHandRowsAsync`, `GetDetailsAsync(int id, string type)` — ported verbatim
    from `PhoneDataService`/`AppleIdDataService` (dedupes, IMEI/email uniqueness, model-owns-manufacturer check,
    `"Shop Warranty"` + second-hand defaults, Apple-ID implicit `"iPhone"` model, plaintext
    `input.Password.Trim()` per L8).
  - `ServiceCollectionExtensions.cs`: `IProductsDataService -> ProductsDataService` in the Dal (`!useApi`)
    branch; `UseApi` stays false; the 9 entity + 15 repo registrations untouched.
  - `Color` collision with `QuestPDF.Infrastructure.Color` resolved by `MobileShop.Models.Entities.Color`
    qualification (no GlobalUsings change).
- **Deviation recorded (plan.md Step 1 Symbols, reviewer LOW):** the `IBaseRepo<Product> products` ctor slot was **dropped** — the seven remaining repos + logger are injected, and `AddNewAsync` is used through a generic base call where the product entity is created inline in Step 3's page migration. This is the sanctioned off-by-one fix; it produces 0 warnings.
- Limitations: product writes (`products.AddAsync`) are not exercised in Step 1 — they are covered by Step 3's page migration; the `ProductsDataServiceTests` fixture therefore has 7 repos in its ctor (not 8). No Step-1 test asserts product creation; that is intentional and noted in plan §Step 1 "Tests".
- Friction noted: the test host buffers xUnit progress output until process exit, so verification polls the redirect log past the interactive window (~38s for the full suite). One initial parallel-run failure of `CreateColorAsync_creates_and_dedupes` reproduced as a known EF-InMemory static-model-build race under heavy parallelism; made deterministic by forcing `ProductsDataServiceTests` onto a `[CollectionDefinition("MobileShop-Dal-Serial", DisableParallelization = true)]`.
- Problems: None remaining.
- Status: COMPLETE
- **Post-commit correction (this turn):** the three read members initially shipped with a deviant
  `Profile != null` nav predicate on the inventory/details lookups and a merged `.OrderBy(row => row.ProductId)`
  on the inventory/second-hand concatenations. Per the review, `ProductsDataService.cs` was realigned to the
  **original** `PhoneDataService`/`AppleIdDataService` semantics: inventory uses the predicate-less
  `SelectAllAsync(<projection>)` + per-block `.OrderBy(row => row.ProductId)` with the two blocks concatenated
  **without** a further OrderBy; second-hand keeps the `SecondHandProfile != null` predicate + per-block
  OrderBy + unordered concat; `GetDetailsAsync` uses `SelectAsync(id, <projection>)` (filters by `Id == id`
  only, no `Profile != null`). The `ProductsDataServiceTests` already pinned the original semantics and
  required no changes (no test asserted the merged OrderBy or Profile-filtered exclusion) \u2014 confirmed by
  `dotnet build` (0 warnings) + `dotnet test` full suite (477 passed, 0 failed, 2 skipped). plan.md Step 2
  was rescoped: the three reads already exist from Step 1, so Step 2 only *extends* them (`type` param,
  `Transactions` wiring, shared projection); the ctor sentence is corrected to "seven repos, this step adds
  the eighth".

