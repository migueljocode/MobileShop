# Plan — UI/UX Enhancements — Step 2 of 7

Copied verbatim from .clinerules/to-do.md. Full stage list: to-do.md → # To-do — UI/UX Enhancements (7 Actionable Demands — Step 7 Already Implemented).

## [ ] Step 2 — Manufacturer & Model dropdowns with "Add New" on Create Phone page
- **Files**: 
  - inspect: `src/MobileShop.Dal/Repos/Interfaces/IManufacturerRepo.cs`, `IModelRepo.cs`, `BaseRepo.cs`
  - modify: `src/MobileShop.Web/Pages/Products/CreatePhone.cshtml.cs`, `src/MobileShop.Web/Pages/Products/CreatePhone.cshtml`, `src/MobileShop.Models/ViewModels/Web/BindModels/CreatePhoneInputModel.cs`
  - create: `src/MobileShop.Tests/Web/Pages/Products/CreatePhoneModelTests.cs` (mirrors `CreateAppleIdModelTests.cs`)
  - do not touch: migrations
- **Symbols**: 
  - `IManufacturerRepo` / `ManufacturerRepo` — reuse `FindAllAsync()` for all manufacturers
  - `IModelRepo` / `ModelRepo` — add `GetByManufacturerAsync(int manufacturerId)` returning `IEnumerable<Model>`
  - `ICategoryRepo` / `CategoryRepo` — inject to resolve "Phone" category by name server-side
  - `CreatePhoneModel` — inject `IManufacturerRepo`, `IModelRepo`, `ICategoryRepo`; load manufacturers on GET; load models via AJAX on manufacturer change
  - `CreatePhoneInputModel` — change `Manufacturer` from `string` to `int ManufacturerId`; change `Model` from `string` to `int ModelId`; add `[Required]` validation
- **Current → Desired**: Text inputs → `<select>` dropdowns; "Add New" buttons open modals with POST handlers that create entities and return JSON for dropdown refresh.
- **Change**: 
  1. Update `CreatePhoneInputModel`: `ManufacturerId` (required), `ModelId` (required), remove `Manufacturer`/`Model` strings.
  2. In `CreatePhoneModel.OnGetAsync`: load all manufacturers into `Manufacturers` property (IEnumerable<Manufacturer>).
  3. Add `OnGetModelsAsync(int manufacturerId)` handler returning `JsonResult` of models for that manufacturer (for cascading dropdown).
  4. Add "Add Manufacturer" modal with form posting to `OnPostCreateManufacturerAsync(string name)` — creates Manufacturer via repo, returns JSON { id, name }.
  5. Add "Add Model" modal with form posting to `OnPostCreateModelAsync(int manufacturerId, string name)` — resolves "Phone" category via `ICategoryRepo.FindAsync(c => c.Name == "Phone")`, creates Model via repo, returns JSON { id, name }. Category ID is NOT client-supplied.
  6. In `OnPostAsync` (main form): look up manufacturer/model by ID via repo; if not found, add ModelState error.
  7. On validation failure in `OnPostAsync`, repopulate `Manufacturers` and (if manufacturer selected) `Models` for the view.
- **Edge cases**: 
  - Manufacturer/Model not found → validation error on main form
  - Cascading: when manufacturer changes, reset model dropdown via JS fetch to `OnGetModelsAsync`
  - "Add New" modals must re-fetch dropdowns on success (fetch + DOM update)
  - Category for new Model: resolved server-side by name "Phone" (seeded)
- **Tests**: 
  - Create `CreatePhoneModelTests.cs` for dropdown population, cascading, and modal POST handlers
  - Verify `OnPostAsync` with valid IDs creates phone correctly
- **Verify**: `dotnet build src/MobileShop.slnx --nologo && dotnet test src/MobileShop.slnx --nologo --filter "CreatePhoneModelTests"`
- **Done when**: Create Phone page shows dropdowns; changing manufacturer updates models; "Add New" modals create entities and refresh dropdowns; form submits with IDs.
- **Risk**: HIGH | **Confidence**: MEDIUM

## Execution notes
- Work fast: combine independent shell commands (`&&`).
- Keep verification output short — only tail of build/test.
- Do not restate this plan in chat; write report to `act.md` per step.
- Stop after each step; wait for reviewer approval before next.
