# Act Report — Step 5: Sorting on Customers & Sellers pages

## Commit
- **Hash**: 3de5feb
- **Message**: feat(web): add sortable Name/Phone/Count column headers on Customers & Sellers pages

## Status
COMPLETE

## Changes
- `ICustomerDataService` / `ISellerDataService`: `GetListRows` / `GetListRowsAsync` now take `string sortBy = "Name", bool ascending = true`. Async default bodies rethrow `NotImplementedException` with the new signature.
- `CustomerDataService` / `SellerDataService`: materialize rows then order via tuple switch on (`Phone`|`Count`|default Name) × direction; invalid `sortBy` falls back to Name.
- `CustomersModel` / `SellersModel`: `OnGetAsync(string sortBy = "Name", bool ascending = true)` stores `SortBy`/`Ascending` for the view and passes them through to the service.
- `Customers.cshtml` / `Sellers.cshtml`: Name/Phone/count (Purchased/Sold) headers are now `<a asp-route-sortBy=... asp-route-ascending=...>` toggles that flip direction when the active column is clicked.
- `ApiCustomerDataService` / `ApiSellerDataService`: sync `GetListRows` signature updated to match the interface (plan said "no edit needed", but they explicitly override the *sync* member and the build failed with CS0535 — a one-line signature fix was required; behavior still throws `NotImplementedException`).
- New tests: `CustomersModelTests`, `SellersModelTests` (page-model sort param handling, Moq-based) and ordering tests appended to `CustomerDataServiceTests` / `SellerDataServiceTests` (Name/Phone/Count × asc/desc, invalid fallback, async parity).

## Verification
- `dotnet build src/MobileShop.slnx --nologo` → Build succeeded, 0 Warning(s), 0 Error(s)
- `dotnet test src/MobileShop.slnx --nologo --filter "CustomerDataServiceTests|SellerDataServiceTests|CustomersModelTests|SellersModelTests"` → Passed: 14, Failed: 0
- `dotnet test src/MobileShop.slnx --nologo` (full suite) → Passed: 405, Failed: 0, Skipped: 2 (pre-existing QuestPDF skips)

## Friction noted
- Plan's "Api stub classes need no edit" was wrong for the sync `GetListRows` — stubs override it explicitly, so CS0535 forced a signature update. Minimal fix applied.

## Problems
- None beyond the above.

## Status
COMPLETE
