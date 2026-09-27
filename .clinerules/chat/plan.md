# Plan — UI/UX Enhancements — Step 4 of 7

Copied verbatim from .clinerules/to-do.md. Full stage list: to-do.md → # To-do — UI/UX Enhancements (7 Actionable Demands — Step 7 Already Implemented).

## ~~[x] Step 4 — Purchased/Sold count columns on Customers & Sellers list pages~~
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

## Execution notes
- Work fast: combine independent shell commands (`&&`).
- Keep verification output short — only tail of build/test.
- Do not restate this plan in chat; write report to `act.md` per step.
- Stop after each step; wait for reviewer approval before next.
