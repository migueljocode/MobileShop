# Plan — UI/UX Enhancements — Step 5 of 7

Copied verbatim from .clinerules/to-do.md. Full stage list: to-do.md → # To-do — UI/UX Enhancements (7 Actionable Demands — Step 7 Already Implemented).

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


## Execution notes
- Work fast: combine independent shell commands (`&&`).
- Keep verification output short — only tail of build/test.
- Do not restate this plan in chat; write report to `act.md` per step.
- Stop after each step; wait for reviewer approval before next.
