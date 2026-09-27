# To-do — UI/UX Enhancements (7 Actionable Demands — Step 7 Already Implemented)

## Reviewer Briefing
- **HIGH-risk steps**: Step 1 (Back button URL mapping for Apple ID), Step 2 (Manufacturer/Model dropdowns with add-new modals) — requires new repo queries, AJAX/modal wiring, cascading dropdown logic, explicit modal POST handlers, and server-side category resolution; Step 3 (profit estimation dual-field) — requires JS calculation binding on two pages with nullable validation; Step 5 (sorting on People pages) — requires query changes, URL state, header UI, new web test files; Step 6 (Distribution sort) — requires updating 4 existing test blocks that currently expect insertion order.
- **MEDIUM-confidence steps**: Step 4 (count columns) — touches list VMs, service projections (both sync + async via repo Select, no DbContext in services), requires subquery counts to avoid client eval, and explicit decision on distinct vs raw count + soft-delete handling.
- **LOW-risk steps**: Steps 8 — pure Razor/HTML/CSS changes.
- **NOTE**: Step 7 was already implemented in the repository (manual mode defaults to earliest transaction date / today). Not an actionable step.

---

## ~~[x] Step 1 — Add "Back to Products" button on Product Details page~~
   Verified: `dotnet build src/MobileShop.slnx --nologo` → Build succeeded, 0 Warning(s), 0 Error(s) (re-run by the reviewer). Change: 1 line in `Details.cshtml` inside the `@if (Model.Product is not null)` guard — `<a asp-page="/Products/Index" asp-route-type="@(Model.Product.Type == "Apple ID" ? "appleid" : "phone")" class="btn btn-outline-secondary mb-3">Back to Products</a>`; no other source file touched. Committed as `feat(web): add "Back to Products" button on Product Details page`.

---

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

---

## [ ] Step 3 — Profit estimation (percent ↔ dollars) on Create Phone & Create Apple ID
- **Files**: 
  - modify: `src/MobileShop.Models/ViewModels/Web/BindModels/CreatePhoneInputModel.cs`, `src/MobileShop.Models/ViewModels/Web/BindModels/CreateAppleIdInputModel.cs`
  - modify: `src/MobileShop.Web/Pages/Products/CreatePhone.cshtml`, `src/MobileShop.Web/Pages/Products/CreateAppleId.cshtml`
  - do not touch: services, repos
- **Symbols**: 
  - `CreatePhoneInputModel` / `CreateAppleIdInputModel` — add `ProfitPercent?` (nullable decimal, `[Range(0, 100)]`) and `ProfitAmount?` (nullable decimal, `[Range(0, double.MaxValue)]`) properties
  - Both pages — add JS to sync: `ProfitAmount = Price * ProfitPercent / 100` and vice versa
- **Current → Desired**: Only `Price` field → add two linked fields near Price; user enters either % or $, the other updates live.
- **Change**: 
  1. Add nullable `ProfitPercent` and `ProfitAmount` with `[Range]` attributes to both input models (nullable so blank is valid).
  2. In both `.cshtml`: render both fields in an `input-group` with `%` and currency symbols; add `data-price`, `data-percent`, `data-amount` for JS binding.
  3. Add inline `<script>` that listens `input` on either field, reads `Price`, computes the other, updates it. Handle blank/zero gracefully.
  4. On submit, both values are posted; server ignores them (not persisted).
- **Edge cases**: 
  - Price = 0 or blank → percent/amount show blank
  - User clears one field → other clears
  - Validation: fields are optional; no server-side cross-check required
- **Tests**: None new (UI-only). Verify manually.
- **Verify**: `dotnet build src/MobileShop.slnx --nologo`
- **Done when**: Both Create pages show linked Profit % / $ fields that auto-calculate; blank fields allowed.
- **Risk**: MEDIUM | **Confidence**: MEDIUM

---

## [ ] Step 4 — Purchased/Sold count columns on Customers & Sellers list pages
- **Files**: 
  - modify: `src/MobileShop.Models/ViewModels/Web/CustomerListItemViewModel.cs`, `src/MobileShop.Models/ViewModels/Web/SellerListItemViewModel.cs`
  - modify: `src/MobileShop.Web/Pages/People/Customers.cshtml.cs`, `src/MobileShop.Web/Pages/People/Customers.cshtml`, `src/MobileShop.Web/Pages/People/Sellers.cshtml.cs`, `src/MobileShop.Web/Pages/People/Sellers.cshtml`
  - modify: `src/MobileShop.Services/DataServices/Dal/CustomerDataService.cs`, `src/MobileShop.Services/DataServices/Dal/SellerDataService.cs`
  - modify: `src/MobileShop.Services/DataServices/Interfaces/ICustomerDataService.cs`, `src/MobileShop.Services/DataServices/Interfaces/ISellerDataService.cs`
- **Symbols**: 
  - `CustomerListItemViewModel` — add `int PurchasedCount { get; init; }` as last parameter in positional record
  - `SellerListItemViewModel` — add `int SoldCount { get; init; }` as last parameter in positional record
  - `ICustomerDataService.GetListRowsAsync()` — project `PurchasedCount` via repo `SelectAll(c => new CustomerListItemViewModel(..., c.Transactions.Count(t => t.Direction == TransactionDirection.Sell && !t.IsDeleted)))` — no DbContext in service layer
  - `ISellerDataService.GetListRowsAsync()` — project `SoldCount` via repo `SelectAll(s => new SellerListItemViewModel(..., s.Transactions.Count(t => t.Direction == TransactionDirection.Buy && !t.IsDeleted)))`
  - `ICustomerDataService.GetListRows()` — same projection for sync method
  - `ISellerDataService.GetListRows()` — same projection for sync method
- **Current → Desired**: Tables show only Name/Phone/NationalId/Type → add count column with sortable header.
- **Change**: 
  1. Extend both VM records with count property appended last (no default value; all 4 call sites updated).
  2. Update **both sync and async** service projections using repo `SelectAll` with navigation property count + `!t.IsDeleted` filter (soft-deleted transactions excluded).
  3. **Count definition**: raw transaction count (not distinct products) — a customer buying same product twice shows 2. "Counts match detail pages" criterion dropped.
  4. Update `.cshtml` tables: add `<th>Purchased</th>` / `<th>Sold</th>` and `<td>@customer.PurchasedCount</td>`.
- **Edge cases**: 
  - Customers/Sellers with zero transactions → show 0
  - Soft-deleted transactions excluded from count
  - Performance: use `AsNoTracking` and server-side count projection; avoid client-side evaluation
- **Tests**: 
  - Add new tests in `CustomerDataServiceTests` / `SellerDataServiceTests` to assert count column values (raw count, excludes soft-deleted)
- **Verify**: `dotnet build src/MobileShop.slnx --nologo && dotnet test src/MobileShop.slnx --nologo --filter "CustomerDataServiceTests|SellerDataServiceTests"`
- **Done when**: Both list pages display correct raw transaction counts; soft-deleted transactions excluded.
- **Risk**: LOW | **Confidence**: HIGH

---

## [ ] Step 5 — Sorting on Customers & Sellers pages (Name, Phone, Count)
- **Files**: 
  - modify: `src/MobileShop.Web/Pages/People/Customers.cshtml.cs`, `src/MobileShop.Web/Pages/People/Customers.cshtml`, `src/MobileShop.Web/Pages/People/Sellers.cshtml.cs`, `src/MobileShop.Web/Pages/People/Sellers.cshtml`
  - modify: `src/MobileShop.Services/DataServices/Interfaces/ICustomerDataService.cs`, `src/MobileShop.Services/DataServices/Interfaces/ISellerDataService.cs`
  - modify: `src/MobileShop.Services/DataServices/Dal/CustomerDataService.cs`, `src/MobileShop.Services/DataServices/Dal/SellerDataService.cs`
  - create: `src/MobileShop.Tests/Web/Pages/People/CustomersModelTests.cs`, `src/MobileShop.Tests/Web/Pages/People/SellersModelTests.cs` (mirror `IndexModelTests.cs` pattern)
  - do not touch: `ApiCustomerDataService.cs`, `ApiSellerDataService.cs` (they inherit default interface implementation; no edit needed)
- **Symbols**: 
  - Add `sortBy` (string: "Name", "Phone", "Count") and `ascending` (bool = true default) parameters to `GetListRowsAsync` and page `OnGetAsync`
  - PageModel: bind `[FromQuery] string sortBy`, `bool ascending = true`; default `Name`, `true` (so omitted `ascending` param = ascending)
  - Razor: make column headers `<a>` with toggled sort dir; preserve other query params via `asp-route-*`
- **Current → Desired**: Static tables → clickable column headers that toggle asc/desc; URL reflects sort state.
- **Change**: 
  1. Extend service interface: `Task<IReadOnlyList<CustomerListItemViewModel>> GetListRowsAsync(string sortBy, bool ascending = true)`
  2. Implement ordering in `CustomerDataService`/`SellerDataService` using `OrderBy`/`OrderByDescending` on projected VM properties.
  3. `ICustomerDataService.cs:15` / `ISellerDataService.cs:15` default bodies keep throwing `NotImplementedException` with the new parameter list; the Api stub classes need no edit.
  4. PageModel `OnGetAsync` accepts sort params with `ascending = true` default, passes to service, stores current sort for View.
  5. Razor: `<th><a asp-route-sortBy="Name" asp-route-ascending="@(Model.SortBy=="Name" && Model.Ascending ? "false" : "true")">Name</a></th>` etc. (lowercase "true"/"false" for clean URLs).
- **Edge cases**: 
  - Invalid sortBy → default to Name
  - Omitted `ascending` query param binds to `false` in ASP.NET; handler default `ascending = true` ensures ascending is the effective default.
  - Preserve sort across pagination (none yet) and filter (none yet)
- **Tests**: 
  - Create `CustomersModelTests.cs` / `SellersModelTests.cs` for sort param binding and header links
  - Service tests for ordering
- **Verify**: `dotnet build src/MobileShop.slnx --nologo && dotnet test src/MobileShop.slnx --nologo --filter "CustomerDataServiceTests|SellerDataServiceTests|CustomersModelTests|SellersModelTests"`
- **Done when**: Clicking Name/Phone/Count headers sorts table; URL updates; direction toggles; API stubs compile; new web tests pass.
- **Risk**: MEDIUM | **Confidence**: MEDIUM

---

## [ ] Step 6 — Reports page: Total Profit sign/color + Distribution sort by Share%
- **Files**: 
  - modify: `src/MobileShop.Web/Pages/Reports/ProfitLoss.cshtml`
  - modify: `src/MobileShop.Services/Logging/Settings/DistributionSettings.cs` (DistributionCalculator)
  - modify: `src/MobileShop.Tests/Services/Logging/DistributionCalculatorTests.cs`
- **Symbols**: 
  - `ProfitLoss.cshtml` line 80 & 91: `@Model.TotalProfit.ToString("N0")` → add sign and conditional class
  - `DistributionCalculator.Calculate()` — already returns List<DistributionRow>; ensure it's ordered by `SharePercent desc`, then `EmployeeName`
  - `DistributionCalculatorTests` — four test blocks currently expect insertion order (Mikaeeil→Anis→Shop). Must re-index to descending Share%: Anis (50%), Mikaeeil (40%), Shop (10%).
- **Current → Desired**: 
  - Total Profit shows `+1,234` (green) or `-567` (red) with explicit sign
  - Distribution table rows ordered: highest Share% first (Anis 50%, Mikaeeil 40%, Shop 10%)
- **Change**: 
  1. In `ProfitLoss.cshtml`: wrap TotalProfit in `<span class="@(Model.TotalProfit >= 0 ? "text-success" : "text-danger")">@(Model.TotalProfit >= 0 ? "+" : "")@Model.TotalProfit.ToString("N0")</span>`
  2. In `DistributionCalculator.Calculate()`: after building `rows`, add `.OrderByDescending(r => r.SharePercent).ThenBy(r => r.EmployeeName).ToList()` before return.
  3. In `DistributionCalculatorTests.cs`: update **four** test blocks to Anis→Mikaeeil→Shop order with correct amounts:
     - **Block 1 (lines ~46-53)**: `Profit_ReturnsExactlyThreeRowsWithFixedSharesAndFloorRounding` — names. Re-index `rows[0].EmployeeName == "Anis Sahabi"`, `rows[1] == "Mikaeeil Jorjany"`, `rows[2] == "Shop"`; shares 50/40/10.
     - **Block 2 (lines ~57-59)**: same test — amounts for `Calculate(101m, …)`. Re-index `rows[0].CalculatedAmount == 50m` (Anis), `rows[1] == 40m` (Mikaeeil), `rows[2] == 11m` (Shop). **Shop = 11m**, not 10m.
     - **Block 3 (lines ~80-82)**: `Profit_SharePercentAlways40_50_10EvenWhenEmployeesHaveDifferentShares` (100m) — **SharePercent** assertions. Re-index `rows[0].SharePercent == 50` (Anis), `rows[1] == 40` (Mikaeeil), `rows[2] == 10` (Shop). Line 83 (`Sum == 100m`) needs no change.
     - **Block 4 (lines ~95-101)**: zero-profit case — names/shares re-indexed to Anis/Mikaeeil/Shop (50/40/10), amounts all 0m.
     - **Block 5 (lines ~119-125)**: loss case — names/shares re-indexed to Anis/Mikaeeil/Shop (50/40/10), amounts: Anis 0, Mikaeeil 0, Shop = totalProfit (the loss).
     - Blocks at lines ~62 (sum check) and ~128-130 (loss case already correct order) — **no change needed**.
- **Edge cases**: 
  - TotalProfit = 0 → show `0` (no sign, default color)
  - Distribution rows with equal Share% → stable order by EmployeeName
- **Tests**: 
  - `DistributionCalculatorTests` must pass with updated assertions
  - Manual visual check
- **Verify**: `dotnet build src/MobileShop.slnx --nologo && dotnet test src/MobileShop.slnx --nologo --filter "DistributionCalculatorTests"`
- **Done when**: Total Profit shows sign/color; Distribution tab lists Anis → Mikaeeil → Shop; all tests pass.
- **Risk**: LOW | **Confidence**: HIGH

---

## ~~[x] Step 7 — Already Implemented (Manual Date Range Defaults)~~
- Manual date range defaults already work: `ResolveBoundsAsync` calls `GetEarliestTransactionDateAsync()`, sets `From = EffectiveFrom; To = EffectiveTo;`. Tests pass. No changes needed.

---

## [ ] Step 8 — Sticky/fixed navbar on scroll
- **Files**: 
  - modify: `src/MobileShop.Web/Pages/Shared/_Layout.cshtml`
  - modify: `src/MobileShop.Web/wwwroot/css/site.css`
- **Symbols**: `<nav class="navbar ...">` — add `sticky-top` Bootstrap class; ensure `z-index` > content
- **Current → Desired**: Navbar scrolls away → stays fixed at top with shadow.
- **Change**: 
  1. In `_Layout.cshtml` line 14: change `<nav class="navbar navbar-expand-sm navbar-toggleable-sm navbar-light bg-white border-bottom box-shadow mb-3">` to `<nav class="navbar navbar-expand-sm navbar-toggleable-sm navbar-light bg-white border-bottom box-shadow mb-3 sticky-top" style="z-index: 1030;">`
  2. In `site.css`: append `padding-top: 56px;` to the existing `body { margin-bottom: 60px; }` block (do not add a second `body` selector).
- **Edge cases**: 
  - Mobile collapse: sticky works with Bootstrap 5 `sticky-top`
  - Footer overlap: not an issue
- **Tests**: None (visual). Manual verify.
- **Verify**: `dotnet build src/MobileShop.slnx --nologo`
- **Done when**: Navbar remains visible at top when scrolling any page; content not hidden.
- **Risk**: LOW | **Confidence**: HIGH

---

## Global Definition of Done
- All 7 actionable demands implemented per acceptance criteria above (Step 7 skipped)
- `dotnet build src/MobileShop.slnx --nologo` → 0 errors, 0 warnings
- `dotnet test src/MobileShop.slnx --nologo` → all tests pass (existing + new)
- No modifications to `MobileShop.Api` endpoints; API stubs use default interface implementation and need no edit
- No migrations, no database initialization policy changes
- Apple ID plaintext passwords unchanged; user passwords remain Argon2-hashed
- Conventional commit per step (after Actor execution)

## Execution Notes (for Actor)
- Work fast: combine independent shell commands (`&&`).
- Keep verification output short — only tail of build/test.
- Do not restate this plan in chat; write report to `act.md` per step.
- Stop after each step; wait for reviewer approval before next.
