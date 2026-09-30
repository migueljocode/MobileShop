# Plan — Stage D: People — PeopleDataService + customer/seller pages

Stage C is signed off. This file plans **Stage D only** (next unchecked stage in `to-do.md`).

## Locked decisions (carried)
L1 Boundaries — pages inject area interfaces only. No EF entity crosses the page boundary (requests: bind models / primitives; results: ViewModels / primitives).
L2 One area service per page, local name `dataService`. No page injects a repo or a second data service.
L3 DI — **this stage removes nothing.** Keep all per-entity repo registrations and entity data services (`ICustomerDataService`, `ISellerDataService`, `IProductDataService`, …). Buy/Sell still need party options / selectable products on entity services until Stage E.
L4–L5 PDF / Reports — unchanged.
L6 Logging — `ILogger<PeopleDataService>`; `LogInformation` / `LogWarning` on create success/failure only.
L7 Api — `ApiPeopleDataService` already exists (NIE). Any **signature change** to `IPeopleDataService` must update the Api stub in the **same step**. Never touch `src/MobileShop.Api`. `UseApi` stays false.
L8 No auth, no schema/migrations, no DatabaseInitializer policy change, no bin/obj.
L9 Tests — `RepoTestBase` + real `PeopleDataService` over `new BaseRepo<T>(Context)`, not Moq. Port coverage; do not delete tests for types that still exist.

## Scope
Six pages → one `PeopleDataService` (Dal):
`Customers`, `Sellers`, `CustomerDetails`, `SellerDetails`, `CreateCustomer`, `CreateSeller`.
No `.cshtml` edits if ViewModel property names stay compatible.

## Current page inventory (verified)
| Page | Injects today | Behavior to preserve |
|------|---------------|----------------------|
| Customers | `ICustomerDataService` | `GetListRowsAsync(sortBy, ascending)` — sort keys Name/Phone/Count; invalid → Name |
| Sellers | `ISellerDataService` | same sort shape; Count = Buy-direction txn count |
| CustomerDetails | `ICustomerDataService` + `IProductDataService` | details + purchased product **list rows** (Sell txns → product ids ∩ inventory rows) |
| SellerDetails | `ISellerDataService` + `IProductDataService` | details + supplied-to-shop rows (Buy txns only) |
| CreateCustomer | `ICustomerDataService` | build Customer+Person; `AddAsync`; redirect `/People/Customers` |
| CreateSeller | `ISellerDataService` | build Seller+Person; `AddAsync`; redirect `/People/Sellers` |

Entity leak today: `PurchasedProductsAsync` / `SoldToShopAsync` return `IEnumerable<Product>`. Area service must return **`ProductListItemViewModel`** rows only (same shape the views already bind: `product.Name`).

## Reviewer Briefing
- **MEDIUM — details product rows.** Must replace entity Product enumeration + full-catalog filter without changing list item shape. Prefer projecting inventory-style rows for the id set (port `ProductDataService.GetInventoryRowsAsync` projection filtered by ids).
- **MEDIUM — sort switch.** Copy Customer/Seller `GetListRowsAsync` switch verbatim (including default Name).
- **LOW — creates.** Thin `ServiceResult` wrappers around current create bodies; no field-level ErrorField unless validation already does that (today only generic Message).
- **Do not** remove `ICustomerDataService` / `ISellerDataService` DI (L3). Party options stay on entity services for Stage E.

## [ ] Step 1 — `PeopleDataService` (lists + creates + Dal registration)
- Files: create: `src/MobileShop.Services/DataServices/Dal/PeopleDataService.cs`, `src/MobileShop.Tests/Services/DataServices/Dal/PeopleDataServiceTests.cs`; modify: `src/MobileShop.Services/ServiceCollectionExtensions.cs` (add **one** Dal line `IPeopleDataService` → `PeopleDataService` in the `else` branch only); optionally shorten BindModels FQNs on `IPeopleDataService` / `ApiPeopleDataService` via existing GlobalUsings; do not touch: pages, views, entity services, repo registrations, `if (useApi)` branch (Api people already registered).
- Symbols: `PeopleDataService` ctor `(IBaseRepo<Customer> customers, IBaseRepo<Seller> sellers, IBaseRepo<Product> products, ILogger<PeopleDataService> logger)`.
- Change:
  1. `GetCustomerRowsAsync` / `GetSellerRowsAsync` — port projections + sort switches from `CustomerDataService.GetListRowsAsync` / `SellerDataService.GetListRowsAsync` (Sell count for customers; Buy count for sellers; `!t.IsDeleted` in counts).
  2. `CreateCustomerAsync` / `CreateSellerAsync` — port Create page entity construction; `AddAsync` success iff `> 0`; failure Message matches pages today; `Succeeded` + optional `EntityId` on success (redirect may ignore id).
  3. **Do not** implement details product rows in this step (Step 2).
- Tests: list sort Name/Phone/Count both directions; create success + failure path; seed Person/Customer/Seller as existing tests do.
- Verify: `dotnet build src/MobileShop.slnx --nologo && dotnet test src/MobileShop.slnx --nologo --no-build`
- Done when: Dal service registered; lists + creates green; no page changed.
- Risk: MEDIUM. Confidence: HIGH.

## [ ] Step 2 — Details members (header + product rows, no entities)
- Files: modify: `PeopleDataService.cs`, `IPeopleDataService` only if needed, `ApiPeopleDataService` if signature changes, `PeopleDataServiceTests.cs`; do not touch pages yet.
- Change:
  1. `GetCustomerDetailsAsync(id)` — port `CustomerDataService.GetDetailsAsync` projection into `CustomerDetailsViewModel`.
  2. `GetSellerDetailsAsync(id)` — port seller equivalent.
  3. Product rows for the page (still no page migration):
     - Resolve product ids: customer = Sell-direction transaction product ids; seller = Buy-direction only (`SoldToShop` semantics).
     - Load inventory-shaped rows via `products.SelectAllAsync` using **the same projection** as `ProductDataService.GetInventoryRowsAsync` (Id, ProductId, Type from category name, Name, Barcode, Color, IsSold, IsSecondHand), then **filter** to those ids (or predicate `ids.Contains` if EF translates — if not, materialize inventory then filter in memory as the page does today).
     - Return rows to the page without extending the details VM **or** extend details VM with `IReadOnlyList<ProductListItemViewModel> Products = []` and fill in the service — **prefer extending the two details records** with a defaulted last `Products` parameter so one service call supplies everything and `.cshtml` can keep `Model.Products` if the page assigns `Products = details.Products`.
  4. If VMs gain `Products`, update entity `GetDetails` constructions with default so existing tests compile (`= []`).
- Tests: details null for missing id; product rows only for that party’s direction; empty list when none.
- Verify: build + full suite.
- Risk: MEDIUM. Confidence: MEDIUM.

## [ ] Step 3 — Migrate all six People pages
- Files: modify: all six `*.cshtml.cs` under `Pages/People/`; tests under `src/MobileShop.Tests/Web/Pages/People/` if present (create minimal coverage if missing for creates/details); do not touch `.cshtml` unless a binding name forces it (stop and report).
- Change:
  - Each ctor: `(IPeopleDataService dataService)` only.
  - Customers/Sellers: `GetCustomerRowsAsync` / `GetSellerRowsAsync` with same query args.
  - CustomerDetails/SellerDetails: one details call; `NotFound` if null; `Products = details.Products` (or dedicated list from service if Step 2 kept them separate).
  - Creates: validate → `Create*Async` → Message on failure → redirect lists on success.
- Verify: build + suite; product page tests still green.
- Risk: LOW. Confidence: HIGH.

## [ ] Step 4 — Stage D validation
- Files: none (or plan.md tick only).
- Verify chain 1: `dotnet build src/MobileShop.slnx --nologo && dotnet test src/MobileShop.slnx --nologo --no-build`
- Verify chain 2: `ASPNETCORE_ENVIRONMENT=Production dotnet run --project src/MobileShop.Web --no-launch-profile --urls http://localhost:5199` then curl **200** for `/People/Customers`, `/People/Sellers`, `/People/CreateCustomer`, `/People/CreateSeller`, plus regression routes `/`, `/Products`, `/Transactions`, `/Reports/ProfitLoss`, `/Account/Login`.
- Non-destructiveness: Production log has no `InitializeForDevelopment`; optional sqlite read-only counts before/after stable.
- Done when: all six pages use only `IPeopleDataService dataService`; no People page injects `ICustomerDataService` / `ISellerDataService` / `IProductDataService` / repos; entity DI still present (L3); suite green; served routes 200.
- Risk: LOW. Confidence: HIGH.

## Global Definition of Done
- `PeopleDataService` implements `IPeopleDataService` and is registered in the Dal branch.
- All six People pages depend on a single `dataService`.
- No Product/Customer/Seller entities returned to pages.
- No entity service or repo registration removed.
- Build + full test suite green; Production smoke on People + core routes 200.

## Execution notes
- One step per commit; Conventional Commits; no Co-authored-by.
- Port sort switches and count filters byte-for-byte; do not “improve” invalid sort keys.
- Party options remain on entity Customer/Seller services for Stage E — do not delete those members.
- Work from this plan only after reviewer Job A approval.
