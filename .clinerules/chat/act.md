# Act Report — Step 2

## Commit
- **Hash**: 657dbb8
- **Message**: feat(web): manufacturer & model dropdowns with add-new modals on Create Phone page

## Changes
1. **`src/MobileShop.Models/ViewModels/Web/BindModels/CreatePhoneInputModel.cs`**
   - Replaced `string Manufacturer` → `int ManufacturerId` with `[Required]`
   - Replaced `string Model` → `int ModelId` with `[Required]`

2. **`src/MobileShop.Dal/Repos/Interfaces/IModelRepo.cs`**
   - Added `Task<IEnumerable<Model>> GetByManufacturerAsync(int manufacturerId)` to interface

3. **`src/MobileShop.Dal/Repos/ModelRepo.cs`**
   - Implemented `GetByManufacturerAsync` using `FindAllAsync(m => m.ManufacturerId == manufacturerId)`

4. **`src/MobileShop.Web/Pages/Products/CreatePhone.cshtml.cs`**
   - Added `Manufacturers` / `Models` properties (IEnumerable, private set)
   - Added `OnGetAsync()` — loads all manufacturers via `FindAllAsync()`
   - Added `OnGetModelsAsync(int manufacturerId)` — returns JSON of models for cascading dropdown
   - Added `OnPostCreateManufacturerAsync(string name)` — creates/returns existing manufacturer, returns JSON `{ id, name }`
   - Added `OnPostCreateModelAsync(int manufacturerId, string name)` — resolves "Phone" category server-side, creates model, returns JSON `{ id, name }`
   - Updated `OnPostAsync`: validates ManufacturerId/ModelId exist via repo lookup (not client-supplied names), repopulates dropdowns on validation failure
   - Removed old `AddManufacturerAsync`/`AddModelAsync` private helpers (now inline in POST handlers)

5. **`src/MobileShop.Web/Pages/Products/CreatePhone.cshtml`**
   - Replaced text inputs with `<select>` dropdowns using `asp-items="new SelectList(...)"`
   - Added "Add New" buttons with `data-bs-toggle="modal"` targets
   - Added two Bootstrap modals (Add Manufacturer, Add Model) with form inputs
   - Added inline `<script>` for: cascading manufacturer→model fetch, modal POST handlers, dropdown refresh on success

6. **`src/MobileShop.Tests/Web/Pages/Products/CreatePhoneModelTests.cs`** (created)
   - 9 tests covering: dropdown population, cascading, modal POST handlers (create, duplicate, 404), main form submit with valid IDs, validation errors for invalid IDs

## Verification
- **Command**: `dotnet build src/MobileShop.slnx --nologo`
- **Result**: Build succeeded, 0 Warning(s), 0 Error(s)
- **Command**: `dotnet test src/MobileShop.slnx --nologo --filter "CreatePhoneModelTests"`
- **Result**: Passed! Failed: 0, Passed: 9, Skipped: 0, Total: 9
- **Command**: `dotnet test src/MobileShop.slnx --nologo` (full suite)
- **Result**: Passed! Failed: 0, Passed: 393, Skipped: 2, Total: 395 (2 skipped are pre-existing QuestPDF tests)

## Status
- **COMPLETE** — Step 2 implementation done, awaiting reviewer approval
