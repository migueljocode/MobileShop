# Plan — Stage B: Home — HomeDataService + dashboard

Written on the owner's explicit instruction (normally the planner's file, per reviewer.md). Stage A is
signed off (`20f6c37`); this plan covers Stage B only, and reuses the decisions locked in Stage A.

## Locked decisions (carried, binding)
L1 Boundaries — pages inject area interfaces only. No EF entity crosses the boundary: requests enter as
   input bind models or primitives, results leave as ViewModels or primitives. Area DataServices may
   inject any `IBaseRepo<T>` they need.
L2 One area service per page, local name `dataService`. No page orchestrates two data services; no page
   injects a repo.
L3 DI ordering — **this stage removes nothing.** The 15 per-entity repo registrations and the 9 entity
   service registrations are still consumed by other pages; Stage H removes them. Do not "tidy" them here.
L4 PDF/invoice ownership — Transactions area. Not touched in Stage B.
L5 Reports owns profit/loss, bounds and distribution. Not touched in Stage B.
L6 Logging — `ILogger<TAreaService>`, LogInformation/LogWarning on mutations, per DataServiceBase.
L7 Api — untouched in Stage B. `UseApi` stays false; never touch the `src/MobileShop.Api` host.
L8 Project constraints — Apple ID inventory passwords stay plaintext; DatabaseInitializer policy
   untouched; no schema changes; no auth; never edit bin/obj; never enable `UseApi: true`.
L9 Tests — port the coverage for the members you move, and strengthen it where it is only an inequality.

## Scope
One page: `Web/Pages/Index.cshtml.cs` (dashboard). One new service: `HomeDataService`. `Index.cshtml` is
NOT edited: it renders `Model.Stock.PhonesInStock/PhonesSecondHand/PhonesSecondHandAvailable/
AppleIdsInStock` and `Model.RecentTransactions`, and both page properties keep their current types
(`DashboardStockSummary`, `IReadOnlyList<TransactionCardViewModel>`).

## Reviewer Briefing
- LOW risk / HIGH confidence throughout: Stage B is a mechanical port of four counts and one projection.
- The whole risk is *behaviour drift* in the four stock numbers and in the recent-cards projection. The
  predicates and the projection are quoted below so the actor copies them rather than rewrites them.
- Existing coverage for these members is almost nil — `PhoneDataServiceTests:74-75` and
  `AppleIdDataServiceTests:76-77` assert only `SecondHandQuantity() >= 1` and
  `AvailableSecondHandQuantity() >= 0`, and nothing anywhere tests `GetRecentCards`. Step 3 must replace
  that with real assertions, otherwise this stage ships unverified numbers.

## ~~[x] Step 1 — Implement HomeDataService (Dal) and register it~~
- Files: create: src/MobileShop.Services/DataServices/Dal/HomeDataService.cs; modify:
  src/MobileShop.Services/ServiceCollectionExtensions.cs (one line in the `else` branch only);
  do not touch: any interface, any Api stub, any repo, any page, any .cshtml, the `if (useApi)` branch,
  the 15 repo registrations, the 9 entity service registrations.
- Symbols: `HomeDataService : IHomeDataService` with ctor `(IBaseRepo<Phone> phones, IBaseRepo<AppleId>
  appleIds, IBaseRepo<Transaction> transactions, ILogger<HomeDataService> logger)`;
  `HomeDataService.GetStockAsync()`, `HomeDataService.GetRecentTransactionsAsync(int count = 20)`;
  `ServiceCollectionExtensions.AddMobileShopDataServices`.
- Current -> Desired: the four stock numbers currently come from `PhoneDataService.QuantityAsync`,
  `PhoneDataService.SecondHandQuantityAsync`, `PhoneDataService.AvailableSecondHandQuantityAsync` and
  `AppleIdDataService.QuantityAsync`, assembled in that order at `Index.cshtml.cs:11-17`; the cards come
  from `TransactionDataService.GetRecentCards` (`:54-70`). Desired: the identical numbers computed inside
  HomeDataService from `IBaseRepo<T>` alone.
- Change:
  1. `GetStockAsync()` returns
     `new DashboardStockSummary(await phones.CountAsync(), await phones.CountAsync(p => p.ProductNavigation.SecondHandProfile != null), await phones.CountAsync(p => p.ProductNavigation.SecondHandProfile != null && !p.ProductNavigation.Transactions.Any(t => t.Direction == TransactionDirection.Sell)), await appleIds.CountAsync())`
     — the predicates are copied verbatim from `PhoneDataService.cs:276-287` and `AppleIdDataService.cs:251-252`;
     the argument ORDER must stay exactly as `Index.cshtml.cs:11-17` has it, because
     `DashboardStockSummary` is positional.
  2. `GetRecentTransactionsAsync(int count = 20)` = `await transactions.SelectAllAsync(<selector>)` with
     the selector copied verbatim from `TransactionDataService.cs:56-67` (date, direction, finished price,
     `ManufacturerNavigation.Name + " " + ModelNavigation.Name`, and the "From: "/"To: " seller/customer
     label with the `PersonNavigation == null ? "Shop"` fallback), then `.OrderByDescending(card => card.Date).Take(count).ToList()`
     — ordering and taking stay in memory after the projection, exactly as today.
  3. Add `services.AddScoped<IHomeDataService, HomeDataService>();` to the `else` branch of
     `AddMobileShopDataServices`, after the 9 entity lines and before `return services;`
     (`ServiceCollectionExtensions.cs:121-130`). Nothing is removed (L3).
- Depends on: Stage A (complete).
- Edge cases / error handling: keep the `PersonNavigation == null` → `"Shop"` fallback — seller/customer
  navigation can be null for shop-sentinel rows; keep the `count` default of 20 because the page passes
  no argument; use the async repo methods throughout (project rule: async where an async path exists).
- Tests: create src/MobileShop.Tests/Services/DataServices/Dal/HomeDataServiceTests.cs extending
  `RepoTestBase`, constructing the service with `new BaseRepo<Phone>(Context)`, `new BaseRepo<AppleId>(Context)`,
  `new BaseRepo<Transaction>(Context)` — `BaseRepo<T>` has been instantiable since Stage A Step 1, so this
  test file needs no rewrite in Stage H. Delete the two superseded assertion sites
  (`PhoneDataServiceTests:74-75`, `AppleIdDataServiceTests:76-77`) only if Step 3's coverage fully replaces them.
- Verify: dotnet build src/MobileShop.slnx --nologo && dotnet test src/MobileShop.slnx --nologo --no-build
- Done when: service compiles, is registered in the Dal branch, the three stock/predicates are copied
  verbatim, and no other file is touched.
- Risk: LOW. Confidence: HIGH.

## ~~[x] Step 2 — Migrate IndexModel to IHomeDataService dataService~~
- Files: modify: src/MobileShop.Web/Pages/Index.cshtml.cs; do not touch: Index.cshtml, any service, any repo.
- Symbols: `IndexModel` primary constructor; `IndexModel.OnGetAsync`.
- Current -> Desired: ctor takes `(IPhoneDataService, IAppleIdDataService, ITransactionDataService)` and
  assembles the stock summary inline. Desired: ctor takes `(IHomeDataService dataService)` and the handler
  is two awaits.
- Change: replace the three parameters with `IHomeDataService dataService`; `OnGetAsync` becomes
  `Stock = await dataService.GetStockAsync(); RecentTransactions = await dataService.GetRecentTransactionsAsync();`
  Property types stay `DashboardStockSummary` and `IReadOnlyList<TransactionCardViewModel>`; the page
  keeps no repo or entity service reference. Remove the now-unused parameters' storage if you added any —
  note this PageModel used the primary constructor directly, so there is no private field to delete.
- Depends on: Step 1.
- Edge cases / error handling: none — behaviour is identical by construction; the view keeps working
  because the property types are unchanged (no `.cshtml` edit).
- Tests: none. The page is a pure pass-through and Step 3's served-page pass exercises it end to end.
- Verify: dotnet build src/MobileShop.slnx --nologo && dotnet test src/MobileShop.slnx --nologo --no-build
- Done when: build green, `Index.cshtml` untouched, and no other page references a repo.
- Risk: LOW. Confidence: HIGH.

## ~~[x] Step 3 — Strengthen coverage and run Stage B validation~~
- Files: modify: src/MobileShop.Tests/Services/DataServices/Dal/HomeDataServiceTests.cs (created in Step 1);
  create: none; delete: nothing.
- Symbols: `HomeDataServiceTests`; the two superseded inequality assertions in `PhoneDataServiceTests:74-75`
  and `AppleIdDataServiceTests:76-77`.
- Current -> Desired: the dashboard's numbers and cards have no real assertions today. Desired: the four
  stock numbers and the card projection are pinned by tests.
- Change:
  1. `GetStockAsync` — assert exact counts for: phones; phones with a `SecondHandProfile`; second-hand
     phones still available; second-hand phones excluded once a Sell transaction exists; Apple IDs in
     stock. Use exact equality, not `>=`.
  2. `GetRecentTransactionsAsync` — assert newest-first ordering, that `count` truncates, and that the
     manufacturer+model label and the "From: "/"To: " seller/customer label (including the "Shop" fallback)
     render as today.
  3. Replace or remove the two inequality assertions in `PhoneDataServiceTests:74-75` and
     `AppleIdDataServiceTests:76-77` so no weaker duplicate of the new coverage is left behind.
- Depends on: Steps 1-2.
- Edge cases / error handling: keep the `? null` seller/customer case covered; if a new test needs the
  sentinel shop rows, reuse the pattern already in `PhoneDataServiceTests`.
- Verify (chain 1): dotnet build src/MobileShop.slnx --nologo && dotnet test src/MobileShop.slnx --nologo --no-build
- Verify (chain 2, guard): `--no-launch-profile` is MANDATORY — `src/MobileShop.Web/Properties/launchSettings.json`
  sets `ASPNETCORE_ENVIRONMENT=Development` for every profile, and without this flag `dotnet run` applies it
  and runs the destructive `DatabaseInitializer.InitializeForDevelopment` (EnsureDeleted/EnsureCreated/seed).
  Confirm the host log says `Hosting environment: Production` BEFORE any route check; if it says
  Development, stop.
- Verify (chain 2): ASPNETCORE_ENVIRONMENT=Production dotnet run --project src/MobileShop.Web
  --no-launch-profile --urls http://localhost:5199 (keep the session attached and read until it listens),
  then in a second call:
  for p in / /Products /Products/SecondHand /People/Customers /People/Sellers /Transactions
  /Reports/ProfitLoss /Account/Login; do printf '%s %s\n' "$(curl -s -o /dev/null -w '%{http_code}'
  http://localhost:5199$p)" "$p"; done
  Fallback if the flag misbehaves: (cd src/MobileShop.Web/bin/Debug/net10.0 && ASPNETCORE_ENVIRONMENT=Production
  dotnet MobileShop.Web.dll --urls http://localhost:5199)
- Done when: chain 1 exit 0; guard line seen; all eight routes 200; and `stat -c '%s %y' MobileShop.db`
  is identical before and after (NOT `git status` — `.gitignore` excludes `*.db`). Current baseline is
  `4096 2026-09-29 17:46:58`; the `-wal`/`-shm` sidecars may move, the main file must not.
- Risk: MEDIUM. Confidence: HIGH.

## Global Definition of Done (Stage B)
- `IndexModel` injects exactly one dependency, `IHomeDataService dataService`; no page injects a repo.
- `HomeDataService` is registered in the Dal branch of `AddMobileShopDataServices`; `UseApi` still false;
  the Api stub for Home is untouched.
- No repo registration and no entity service registration was removed (L3 — Stage H does that).
- The four stock numbers and the recent-transaction cards are behaviourally identical to pre-migration
  (same predicates, same argument order, same projection, same `count` default).
- `Index.cshtml` unchanged; `Model.Stock.*` and `Model.RecentTransactions` still render.
- `HomeDataServiceTests` pins the four counts (exact equality) and the card ordering/limit/label, and no
  weaker inequality-only duplicate is left in the old service tests.
- `dotnet build src/MobileShop.slnx --nologo` and `dotnet test src/MobileShop.slnx --nologo --no-build`
  succeed; the Production served-page pass returns 200 on all eight routes; `MobileShop.db` unchanged.

## Carried notes — NOT Stage B work
- Do not remove the 15 per-entity repo registrations or the 9 entity service registrations: they are still
  consumed by Products/People/Transactions/Reports pages. Removing them compiles, the suite stays green
  (no test resolves through DI) and every page breaks at runtime.
- `IEmployeeDataService` is missing from the `if (useApi)` branch and no `ApiEmployeeDataService` exists
  (pre-existing since `1450979`, inert while `UseApi` is false) — belongs in the Stage H plan.
- The 12 fully-qualified `MobileShop.Models.ViewModels.Web.BindModels.*` references in the area interfaces
  and Api stubs get one `global using` line in `src/MobileShop.Services/GlobalUsings.cs` in Stage C, where
  `ProductsDataService` implements those members. Not in Stage B (Home takes no input models).
- The development database currently holds sample data with the seed still largely in the `MobileShop.db-wal`.

## Execution notes
- Chain dependent commands with `&&`; group independent read-only checks in one `{ ...; }` call; keep
  verification output and the act.md report short.
- One step per commit. Tick the step header exactly as `## ~~[x] Step N — <title>~~` in this plan, and
  commit the touched files plus this plan. Never stage unrelated pre-existing changes; never use `git add -A`.
- Long full-suite runs exceed the interactive window: use an attached "proceed while running" session and
  read it to completion, and only then commit.
- Do not restate this plan back in chat; write the report to `.clinerules/chat/act.md`.
