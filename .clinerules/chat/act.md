# Act Report — Stage D Step 3
- Verification:
  - Chain 1: `dotnet build src/MobileShop.slnx --nologo` -> exit 0; **0 errors, 0 warnings**.
  - Chain 1: `dotnet test src/MobileShop.slnx --nologo --no-build` -> **Failed: 0, Passed: 510, Skipped: 2, Total: 512** (EXIT=0).
  - DoD spot-checks: all six People page models inject `(IPeopleDataService dataService)` only (grep shows no entity service/repo references); `git diff --stat` on `Pages/People/*.cshtml` is empty (no view edits); entity `ICustomerDataService`/`ISellerDataService`/`IProductDataService` DI still registered in both branches (L3).
- What was implemented (plan Step 3 scope only):
  - `CustomersModel` / `SellersModel`: ctor → `(IPeopleDataService dataService)`; call `GetCustomerRowsAsync` / `GetSellerRowsAsync` with existing query args; `SortBy`/`Ascending` assignments unchanged.
  - `CustomerDetailsModel` / `SellerDetailsModel`: dropped the second service (`IProductDataService`); `var details = await dataService.Get*DetailsAsync(id)`; `NotFound` if null; `Customer`/`Seller = details`; `Products = details.Products`.
  - `CreateCustomerModel` / `CreateSellerModel`: dropped entity construction from the page; validate → `Create*Async` → `Message` on failure (service message, fallback text preserved) → redirect list on success.
  - Page tests: `CustomersModelTests` / `SellersModelTests` rewritten to construct real `PeopleDataService` over `BaseRepo<Customer>/Seller/Product` + `NullLogger` (no Moq); added `DetailsAndCreateModelTests.cs` with minimal known-id/unknown-id tests for both details pages and valid/invalid-ModelState tests for both create pages.
  - No `.cshtml` edits (property names `Customer`/`Seller`/`Products`/`Message` preserved).
- Problems: `PageResult` needed `using Microsoft.AspNetCore.Mvc.RazorPages;` in the new test file (fixed before final green).
- Status: COMPLETE

# Act Report — Stage D Step 2
- Verification:
  - Chain 1: `dotnet build src/MobileShop.slnx --nologo` -> exit 0; **0 errors**; only pre-existing `CS9124` in `ProductsDataService` (reproduced at baseline).
  - Chain 1: `dotnet test src/MobileShop.slnx --nologo --no-build` -> **Failed: 0, Passed: 502, Skipped: 2, Total: 504** (EXIT=0). Filtered `PeopleDataServiceTests`: 12/12 pass.
- What was implemented (plan Step 2 scope only):
  - `PeopleDataService` ctor gained `IBaseRepo<Product> products`; interface signatures untouched; pages untouched.
  - `GetCustomerDetailsAsync`: resolves Sell-direction transaction product ids for that customer (distinct), then projects inventory rows via a shared `GetInventoryRowsForAsync` helper using the byte-for-byte `ProductDataService.GetInventoryRowsAsync` projection (EntityId/ProductId both `product.Id`, Type from category name, Identifier=`Barcode`, IsSold via Sell-direction `Any`, IsSecondHand via `SecondHandProfile != null`), in-memory filter after `SelectAllAsync`, ordered by `ProductId`. Returns `details with { Products = rows }`; null header → null.
  - `GetSellerDetailsAsync`: same, but Buy-direction only (`SoldToShop` semantics).
  - Tests: customer details returns only the purchased Sell-direction product (excludes Buy rows and other customers' purchases) with full row shape assertions; seller details returns only Buy-direction supply (excludes other sellers and Sell-only rows); empty list when no matching transactions; still null for unknown id (already covered).
- Problems: None.
- Status: COMPLETE

# Act Report — Stage D Step 1
- Verification:
  - Chain 1: `dotnet build src/MobileShop.slnx --nologo` -> exit 0; **0 errors, 0 warnings**.
  - Chain 1: `dotnet test src/MobileShop.slnx --nologo --no-build` -> **Failed: 0, Passed: 498, Skipped: 2, Total: 500** (EXIT=0). Filtered `PeopleDataServiceTests`: 8/8 pass.
  - Pre-existing note: an incremental build can surface `CS9124` (ProductsDataService `transactions` captured twice) — reproduced on a stashed baseline build at HEAD, unrelated to this step; a clean incremental build after this step is 0/0.
- What was implemented (plan Step 1 scope only):
  - `CustomerDetailsViewModel` / `SellerDetailsViewModel` gained a last defaulted member `IReadOnlyList<ProductListItemViewModel> Products = null!`. EF expression trees cannot contain collection expressions, so the record default stays `null!` and `PeopleDataService` returns `details with { Products = [] }` post-projection — the observable contract (L10: details always carry a non-null empty `Products` in Step 1) holds.
  - Created `PeopleDataService` with ctor `(IBaseRepo<Customer> customers, IBaseRepo<Seller> sellers, ILogger<PeopleDataService> logger)` implementing all six `IPeopleDataService` members: list rows with sort switches ported byte-for-byte from entity services (Sell count for customers, Buy count for sellers, invalid sort → Name); create paths ported from the Create pages (`AddAsync > 0` success, failure messages preserved, `LogInformation`/`LogWarning` only on create); header-only details.
  - Registered `services.AddScoped<IPeopleDataService, PeopleDataService>();` in the Dal branch only; `if (useApi)` untouched; no entity DI removed (L3).
  - Created `PeopleDataServiceTests` (8 tests): sort Name/Phone/Count both directions + invalid-sort fallback for both lists, details header + empty `Products` for known id, null for unknown id, create persistence for customer/seller. Seeding uses explicit `Person`-first + FK pattern per repo-test conventions.
  - No page, `.cshtml`, repo registration, or Api stub changes (interface signatures unchanged → Api stub untouched, L7).
- Problems: two details tests initially failed because seeding via `PersonNavigation` and `Products = null!` default both violated test/provider expectations; fixed by FK-pattern seeding and `with { Products = [] }`.
- Status: COMPLETE

# Act Report — Stage C Step 5
- Commit: cff36bf — docs(products): complete Stage C coverage consolidation and validation
- Verification:
  - Chain 1: `dotnet build src/MobileShop.slnx --nologo` -> exit 0; **0 errors, 0 warnings** (one transient `1 Warning(s)` line on an earlier incremental build did not reproduce — final clean build is 0/0).
  - Chain 1: `dotnet test src/MobileShop.slnx --nologo --no-build` -> **Failed: 0, Passed: 490, Skipped: 2, Total: 492** (EXIT=0).
  - Chain 2 (guard): log shows `Hosting environment: Production` + `Now listening on: http://localhost:5199` (`--no-launch-profile`, `ASPNETCORE_ENVIRONMENT=Production`); log has **0** matches for `EnsureDeleted|EnsureCreated|seed`.
  - Chain 2 (routes): all ten routes **200**: `/`, `/Products`, `/Products/SecondHand`, `/Products/CreatePhone`, `/Products/CreateAppleId`, `/People/Customers`, `/People/Sellers`, `/Transactions`, `/Reports/ProfitLoss`, `/Account/Login`.
  - Chain 2 (dropdowns): `/Products/CreatePhone` body contains all four ids: `manufacturerSelect`, `modelSelect`, `colorSelect`, `corporationSelect`.
  - Non-destructiveness: row-count fingerprint `Products|Phones|Transactions` = `17|7|26` before AND after the pass (read-only `mode=ro`).
- What was implemented (plan Step 5 scope only):
  - No assertion removed: the `Quantity() >= 0`-shaped example does not exist in `PhoneDataServiceTests`/`AppleIdDataServiceTests` (grep found nothing), and every remaining member (`IsSold`, `IsSecondHand`, `GetOwner`, `GetGuarantee`, `GetSecondHandInfo`, `GetAvailableSecondHandRows*`, `Quantity*`, `GetSelectableProductsAsync`) still has a live consumer per plan L3/carried notes — `Buy`/`Sell` pages still resolve `GetSelectableProductsAsync`. Per plan edge-case rule ("if you cannot show that a dropped assertion is reproduced, keep it"), all `[Fact]`s and assertions stay. No file created, none deleted.
  - DoD spot-checks: all five product pages inject only `IProductsDataService dataService`; no page references a repo (`grep Repo` on Products pages = no match); no `.cshtml` diff in the last 4 commits.
- Limitations: None.
- Friction noted: None.
- Problems: None.
- Status: COMPLETE
