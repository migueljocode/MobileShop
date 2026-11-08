# Plan — Show "Zone Mobile" as the buyer label on every purchase, including existing rows

## Reviewer Briefing
- Requirement (user): every purchase's customer is the shop, "Zone Mobile". Stage 1 stores new purchases against the Zone Mobile customer, but existing rows (and rows in databases that predate it) still point at the old customer ("Anis Sahabi" in the live shop, sample customer 1 in sample data), so the UI still shows that person's name as the buyer.
- Design: mirror the override that already exists for sales: the seller of a Sell row is always `ShopLabels.ZoneMobile` regardless of the stored record. Do the same for the customer of a Buy row at the three duplicated label expressions. No data changes, no migration, no repointing.
- Verified: label sites are `ProductTransactionProjection.Selector` (customer label), `TransactionsDataService` list selector (~lines 70–82) and details selector (~lines 102–114); `GetAppleIdFactorPdfAsync` is Sell-only; factor rows use the seller label for purchases; three pages render `CustomerLabel` directly (`Products/Details.cshtml`, `Transactions/Index.cshtml`, `Transactions/Details.cshtml`). The only existing assertion that conflicts is `ProductsDataServiceTests` ~line 873 (the Buy row expects "Sara Customer").
- Risk: LOW. The expression keeps the same ternary shape already translated by EF for the seller label.
- Out of scope: consolidating the three duplicated label expressions into one (separate refactor), repointing stored rows, `HomeDataService` and Apple ID labels (they show counterparties), docs (Stage 3).

## ~~[x] Step 1 — Label the buyer of every purchase as Zone Mobile~~
- Files: modify: `src/MobileShop.Services/DataServices/Shared/ProductTransactionProjection.cs`, `src/MobileShop.Services/DataServices/Dal/TransactionsDataService.cs`, `src/MobileShop.Tests/Services/DataServices/Dal/ProductsDataServiceTests.cs`, `src/MobileShop.Tests/Services/DataServices/Dal/TransactionsDataServiceTests.cs`; do not touch: view models, pages, other services.
- Symbols: `ProductTransactionProjection.Selector`; the selector lambdas in `TransactionsDataService.GetListAsync` (~70–82) and `TransactionsDataService.GetDetailsAsync` (~102–114); not `GetAppleIdFactorPdfAsync`.
- Current -> Desired: customer label = stored customer's person name (or "Shop") for every direction -> customer label = `ShopLabels.ZoneMobile` when `Direction == TransactionDirection.Buy`, otherwise unchanged.
- Change: at each of the three sites replace the customer-label argument with `transaction.Direction == TransactionDirection.Buy ? ShopLabels.ZoneMobile : <the existing customer-label expression unchanged>`. Leave the seller label, `CustomerId`, `HasAppleIdFactor` and everything else as is.
- Depends on: none.
- Edge cases / error handling: keep the expression a plain ternary over `transaction.Direction` (same shape as the seller label) so EF can translate it; keep the `"Shop"` fallback for sales.
- Tests: in `ProductsDataServiceTests` (~873) the Buy row (`Transactions[1]`) expects `ShopLabels.ZoneMobile` for `CustomerLabel`; the Sell row (`Transactions[0]`) still expects "Sara Customer". In `TransactionsDataServiceTests` add `GetDetailsAsync_labels_the_buyer_of_a_purchase_as_ZoneMobile` (Buy transaction: `SellerLabel` "Ali Zed", `CustomerLabel` `ShopLabels.ZoneMobile`) and `GetListAsync_labels_the_buyer_of_a_purchase_as_ZoneMobile` (`GetListAsync("all", 0, false)` returns one row with `SellerLabel` "Ali Zed" and `CustomerLabel` `ShopLabels.ZoneMobile`); build both like the neighbouring tests with `AddSeller("Ali", "Zed")` and `AddCustomer("Sara", "Ahmadi")`.
- Verify: CI green; `Action: #<run_number> — Success`.
- Done when: all three sites use the Buy override and the tests above pass.
- Risk: LOW
- Confidence: HIGH

## [ ] Step 2 — Final validation
- Files: inspect only.
- Symbols: none.
- Current -> Desired: unvalidated -> validated.
- Change: run `graphify update .` and commit the refreshed `graphify-out/graph.json` and `GRAPH_REPORT.md` (AGENTS.md rule); confirm the final-commit CI run is green; confirm with grep that exactly three customer-label expressions contain `TransactionDirection.Buy ? ShopLabels.ZoneMobile`; record the run number in act.md.
- Depends on: Step 1.
- Edge cases / error handling: report anything unexpected; do not widen scope.
- Tests: none new.
- Verify: `Action: #<run_number> — Success`.
- Done when: the Global Definition of Done is met.
- Risk: LOW
- Confidence: HIGH

## Global Definition of Done
- Steps 1–2 done; only the files named above changed (plus the refreshed graph); CI green (cite run number).
- Every purchase row shows "Zone Mobile" as its buyer in the transactions list, transaction details and product details; sales labels unchanged.
- No schema, data, migration, auth or API changes; Conventional Commits; named-file `git add` only.
