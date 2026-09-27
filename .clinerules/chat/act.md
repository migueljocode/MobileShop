# Act Report — Step 4

## Commit
- **Hash**: 80c2e5d
- **Message**: feat(web): add Purchased/Sold count columns on Customers & Sellers list pages

## Status
COMPLETE

## Changes
- `CustomerListItemViewModel` / `SellerListItemViewModel`: appended `int PurchasedCount` / `int SoldCount` as last positional record parameter.
- `CustomerDataService.GetListRows()` / `GetListRowsAsync()`: projection includes `customer.Transactions.Count(t => t.Direction == TransactionDirection.Sell && !t.IsDeleted)`.
- `SellerDataService.GetListRows()` / `GetListRowsAsync()`: projection includes `seller.Transactions.Count(t => t.Direction == TransactionDirection.Buy && !t.IsDeleted)`.
- `Customers.cshtml` / `Sellers.cshtml`: added `<th>Purchased</th>` / `<th>Sold</th>` header and `<td>@...PurchasedCount</td>` / `<td>@...SoldCount</td>` cell.
- `ICustomerDataService` / `ISellerDataService`: no signature change (projection is internal to implementation).
- New tests in `CustomerDataServiceTests` and `SellerDataServiceTests`: count excludes soft-deleted transactions; zero for no transactions.

## Verification
- `dotnet build src/MobileShop.slnx --nologo` → Build succeeded, 0 Warning(s), 0 Error(s)
- `dotnet test src/MobileShop.slnx --nologo --filter "CustomerDataServiceTests|SellerDataServiceTests"` → Passed: 6, Failed: 0
- Full suite: `dotnet test src/MobileShop.slnx --nologo` → Passed: 397, Failed: 0, Skipped: 2 (pre-existing QuestPDF skips)

## Notes
- Soft-delete also enforced by global EF query filter (`ApplySoftDeleteForEntities`); explicit `!t.IsDeleted` kept per plan.
- Count is raw transaction count (not distinct products), per plan decision.
- Only files listed in plan.md Step 4 were modified.
