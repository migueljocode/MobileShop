# Plan — Stage D: People — PeopleDataService + customer/seller pages

Stage C is signed off. Incorporates Job A audit **H1** (details members required in Step 1) and locks details ViewModels with a defaulted `Products` list.

## Locked decisions (carried + Stage D)
L1 Boundaries — pages inject area interfaces only. No EF entity crosses the page boundary.
L2 One area service per page, local name `dataService`. No page injects a repo or a second data service.
L3 DI — **this stage removes nothing.** Keep per-entity repo registrations and entity data services. Buy/Sell still use party options / selectable products on entity services until Stage E.
L4–L5 PDF / Reports — unchanged.
L6 Logging — `ILogger<PeopleDataService>`; `LogInformation` / `LogWarning` on create success/failure only.
L7 Api — `ApiPeopleDataService` already exists (NIE). Signature changes must update the Api stub in the same step. Never touch `src/MobileShop.Api`. `UseApi` stays false.
L8 No auth, no schema/migrations, no DatabaseInitializer policy change, no bin/obj.
L9 Tests — `RepoTestBase` + real `PeopleDataService` over `new BaseRepo<T>(Context)`. Port coverage; do not delete tests for types that still exist.
L10 **Details shape (locked)** — extend both records with a last defaulted member:
   `IReadOnlyList<ProductListItemViewModel> Products = []`.
   `GetCustomerDetailsAsync` / `GetSellerDetailsAsync` always return header fields; Step 1 ships `Products = []`; Step 2 fills product rows. No alternate “separate list method” API.

## Scope
Six pages → one `PeopleDataService` (Dal):
`Customers`, `Sellers`, `CustomerDetails`, `SellerDetails`, `CreateCustomer`, `CreateSeller`.
No `.cshtml` edits if page models keep `Customer`/`Seller`/`Products` property names.

## Current page inventory (verified)
| Page | Injects today | Behavior to preserve |
|------|---------------|----------------------|
| Customers | `ICustomerDataService` | `GetListRowsAsync(sortBy, ascending)` — Name/Phone/Count; invalid sort → Name |
| Sellers | `ISellerDataService` | same; Count = Buy-direction txn count |
| CustomerDetails | `ICustomerDataService` + `IProductDataService` | details + purchased rows (Sell → product ids ∩ inventory) |
| SellerDetails | `ISellerDataService` + `IProductDataService` | details + supplied-to-shop (Buy only) |
| CreateCustomer | `ICustomerDataService` | Customer+Person; `AddAsync`; redirect `/People/Customers` |
| CreateSeller | `ISellerDataService` | Seller+Person; `AddAsync`; redirect `/People/Sellers` |

Entity leak today: `PurchasedProductsAsync` / `SoldToShopAsync` return `IEnumerable<Product>`. Area path returns **`ProductListItemViewModel`** only (`Name` for the list views).

## Reviewer Briefing
- **H1 resolved:** Step 1 implements **all six** `IPeopleDataService` members; details are header-only with empty `Products`.
- **MEDIUM — Step 2 product rows:** Sell vs Buy id sets + `ProductDataService` inventory projection filtered by ids.
- **MEDIUM — sort switches:** port verbatim from entity services.
- **Do not** remove entity Customer/Seller DI (L3).

## [x] Step 1 — ViewModel defaults + full `PeopleDataService` skeleton (lists, creates, header-only details) + Dal registration
- Files:
  - modify: `src/MobileShop.Models/ViewModels/Web/CustomerDetailsViewModel.cs`, `SellerDetailsViewModel.cs`
  - create: `src/MobileShop.Services/DataServices/Dal/PeopleDataService.cs`, `src/MobileShop.Tests/Services/DataServices/Dal/PeopleDataServiceTests.cs`
  - modify: `src/MobileShop.Services/ServiceCollectionExtensions.cs` (one Dal line only)
  - modify if needed: entity `CustomerDataService` / `SellerDataService` `GetDetails` constructions (default `Products` keeps them compiling)
  - optionally: shorten BindModels FQNs on `IPeopleDataService` / `ApiPeopleDataService`
  - do not touch: pages, `.cshtml`, repo registrations, `if (useApi)` branch
- Symbols: `CustomerDetailsViewModel` / `SellerDetailsViewModel` gain `Products`; `PeopleDataService` implements **every** `IPeopleDataService` member.
- Ctor Step 1: `(IBaseRepo<Customer> customers, IBaseRepo<Seller> sellers, ILogger<PeopleDataService> logger)` — **no** `IBaseRepo<Product>` yet (added in Step 2).
- Change:
  1. Append `IReadOnlyList<ProductListItemViewModel> Products = []` as the last positional parameter on both details records.
  2. `GetCustomerRowsAsync` / `GetSellerRowsAsync` — port projections + sort switches from `CustomerDataService` / `SellerDataService` `GetListRowsAsync` (Sell count / Buy count; `!t.IsDeleted`).
  3. `CreateCustomerAsync` / `CreateSellerAsync` — port Create page construction; success iff `AddAsync > 0`; failure messages as today; log on success/failure.
  4. `GetCustomerDetailsAsync` / `GetSellerDetailsAsync` — port entity header projections only; **do not** load product rows; rely on default empty `Products` (or pass `[]` explicitly). **No** `NotImplementedException`.
  5. Register `services.AddScoped<IPeopleDataService, PeopleDataService>();` in the Dal (`else`) branch only.
- Tests: sort Name/Phone/Count both directions; create success/failure; details returns non-null header and empty `Products` for a seeded customer/seller; null for missing id.
- Verify: `dotnet build src/MobileShop.slnx --nologo && dotnet test src/MobileShop.slnx --nologo --no-build`
- Done when: interface fully implemented; Dal registered; no page changed; suite green.
- Risk: MEDIUM. Confidence: HIGH.

## [x] Step 2 — Fill details `Products` (inventory-shaped rows, no entities)
- Files: modify: `PeopleDataService.cs` (add `IBaseRepo<Product> products` to ctor), `PeopleDataServiceTests.cs`; do not touch interface signatures (already return details VMs); do not touch pages yet.
- Change:
  1. Inject `IBaseRepo<Product> products`.
  2. After resolving the header (or in the same method), resolve product ids:
     - Customer: Sell-direction transaction product ids (purchased from shop).
     - Seller: Buy-direction only (`SoldToShop` semantics).
  3. Build inventory-shaped rows with the **same projection** as `ProductDataService.GetInventoryRowsAsync` (Id, ProductId, Type from category name, Name, Barcode, Color, IsSold, IsSecondHand). Filter to those ids (in-memory filter after `SelectAllAsync` is fine if `Contains` is awkward on the provider).
  4. Return `details with { Products = rows }` (or construct with the list). Distinct product ids as today’s `.Distinct()` behavior.
- Tests: product rows only for the correct direction; empty when none; still null details for unknown id.
- Verify: build + full suite.
- Risk: MEDIUM. Confidence: MEDIUM.

## [ ] Step 3 — Migrate all six People pages + rewire page tests
- Files: modify: all six `Pages/People/*.cshtml.cs`; `src/MobileShop.Tests/Web/Pages/People/CustomersModelTests.cs`, `SellersModelTests.cs`; add minimal tests for creates/details if missing; do not touch `.cshtml` unless forced (stop and report).
- Change:
  - Each ctor: `(IPeopleDataService dataService)` only.
  - Customers/Sellers: `GetCustomerRowsAsync` / `GetSellerRowsAsync` with existing query args.
  - CustomerDetails/SellerDetails: `var details = await dataService.Get*DetailsAsync(id)`; `NotFound` if null; assign header property from details; `Products = details.Products`.
  - Creates: validate → `Create*Async` → `Message` on failure → redirect list on success.
  - Page tests: construct `PeopleDataService` with `BaseRepo<T>` (include `Product` repo after Step 2).
- Verify: build + full suite.
- Risk: LOW. Confidence: HIGH.

## [ ] Step 4 — Stage D validation
- Files: none (or tick plan.md only).
- Verify chain 1: `dotnet build src/MobileShop.slnx --nologo && dotnet test src/MobileShop.slnx --nologo --no-build`
- Verify chain 2: `ASPNETCORE_ENVIRONMENT=Production dotnet run --project src/MobileShop.Web --no-launch-profile --urls http://localhost:5199` then curl **200** for `/People/Customers`, `/People/Sellers`, `/People/CreateCustomer`, `/People/CreateSeller`, and if seed ids exist `/People/CustomerDetails?id=1`, `/People/SellerDetails?id=1`, plus regression `/`, `/Products`, `/Transactions`, `/Reports/ProfitLoss`, `/Account/Login`.
- Non-destructiveness: Production log has no `InitializeForDevelopment`; optional read-only row counts stable before/after.
- Done when: all six pages use only `IPeopleDataService dataService`; no People page injects entity people/product services or repos; entity DI still registered (L3); suite green; smoke 200.
- Risk: LOW. Confidence: HIGH.

## Global Definition of Done
- `PeopleDataService` implements full `IPeopleDataService` and is registered in the Dal branch.
- Details VMs carry `Products`; details methods never return Product entities.
- All six People pages depend on a single `dataService`.
- No entity service or repo registration removed.
- Build + full suite green; Production smoke green.

## Execution notes
- One step per commit; Conventional Commits; no Co-authored-by.
- Port sort switches byte-for-byte; do not “improve” invalid sort keys.
- Party options stay on entity Customer/Seller services for Stage E.
- Audit H1 is already applied in this plan — actor may start Step 1 without another review round.
