# Plan — Stage A: Foundation — generic repo + area DataService seams

## Locked decisions (binding on every stage)
L1 Boundaries — pages inject area interfaces only. No EF entity crosses the boundary in either direction:
   requests enter as input bind models or primitives, results leave as ViewModels or primitives. Area
   DataServices may inject any IBaseRepo<T> they need.
L2 One area service per page (see map), local parameter/property name `dataService`. No page orchestrates
   two data services, and no page injects a repo.
L3 DI ordering — add the open-generic IBaseRepo<> registration now; delete each entity repo registration in
   the area stage that kills its last consumer; delete the remainder in Stage H. Never remove a
   registration whose call sites still exist.
L4 PDF + invoice ownership — IPdfGenerator is injected only by TransactionsDataService. No PageModel
   injects IPdfGenerator. InvoiceDataService's behavior moves into the Transactions area service (it has
   no page consumer today, only tests).
L5 Reports owns profit/loss, the earliest-transaction-date bounds query, and the DistributionCalculator call.

L6 Logging — area services inject ILogger<TAreaService> and keep the DataServiceBase pattern:
   LogInformation on success, LogWarning on failure, for add/update/delete.
L7 Api — area stubs live in src/MobileShop.Services/DataServices/Api/, every member throws
   NotImplementedException("Api<Area>DataService is not implemented yet."). Every interface member added
   in any stage lands with its Api body in the same step, or the build breaks. UseApi stays false by
   default. Never touch the src/MobileShop.Api host project.
L8 Project constraints — Apple ID inventory passwords stay plaintext (never hash them); DatabaseInitializer
   policy untouched; no schema/migration changes; no auth/authorization; never edit bin/obj; never enable
   UseApi: true.
L9 Tests — each area stage ports the coverage from the entity-service tests it replaces; mock-only
   "make it compile" test edits are not sufficient evidence.

## Page → service map (all 21 PageModels)
| Page | Injects | Key members |
|---|---|---|
| Pages/Index | IHomeDataService dataService | GetStockAsync, GetRecentTransactionsAsync |
| Products/Index | IProductsDataService dataService | GetInventoryRowsAsync |
| Products/Details | IProductsDataService dataService | GetDetailsAsync (rows include its transactions) |
| Products/SecondHand | IProductsDataService dataService | GetSecondHandRowsAsync |
| Products/CreatePhone | IProductsDataService dataService | GetManufacturers/Models/Colors/Corporations, CreateManufacturer/Model/Color, CreatePhoneAsync |
| Products/CreateAppleId | IProductsDataService dataService | GetManufacturers/Models, CreateAppleIdAsync |
| People/Customers | IPeopleDataService dataService | GetCustomerRowsAsync |
| People/Sellers | IPeopleDataService dataService | GetSellerRowsAsync |
| People/CustomerDetails | IPeopleDataService dataService | GetCustomerDetailsAsync |
| People/SellerDetails | IPeopleDataService dataService | GetSellerDetailsAsync |
| People/CreateCustomer | IPeopleDataService dataService | CreateCustomerAsync |
| People/CreateSeller | IPeopleDataService dataService | CreateSellerAsync |
| Transactions/Index | ITransactionsDataService dataService | GetListAsync, GetTransactionFactorPdfAsync |
| Transactions/Details | ITransactionsDataService dataService | GetDetailsAsync, GetTransactionFactorPdfAsync |
| Transactions/Buy | ITransactionsDataService dataService | GetSellersAsync, GetSelectableProductsAsync, RecordBuyAsync |
| Transactions/Sell | ITransactionsDataService dataService | GetCustomersAsync, GetSelectableProductsAsync, RecordSellAsync |
| Reports/ProfitLoss | IReportsDataService dataService | GetProfitLossRowsAsync, GetProfitLossTotalAsync, GetEarliestTransactionDateAsync, GetDistributionRowsAsync |
| Account/Login | IAccountDataService dataService | ValidateCredentialsAsync |
| Account/Profile | IAccountDataService dataService | GetAdminUsernameAsync, ValidateCredentialsAsync, ChangePasswordAsync |
| Account/Logout, Error, Privacy | none | unchanged |

## Reviewer Briefing
- Stage A is deliberately behaviour-free: one BaseRepo member, one DI line, six interfaces, six Api stubs,
  one registration block. Nothing user-visible changes, so Step 6 is a pure regression gate on the DI work.
- Step 2 is HIGH risk: every page resolves through that method. It is also HIGH confidence because the step
  changes nothing else, and Step 6 proves it end to end.
- Runtime-vs-build divergence is this repo's main hazard: the suite never resolves through DI and never
  renders a page. Steps 2 and 5 are exactly the kind of change that compiles green while breaking the app.
- Deferred risk, later stages: Stage C carries ~700 lines of behaviour; Stage E replaces a 596-line service;
  Stage F must keep Employee.PersonNavigation loaded or the distribution throws at runtime.
- Deliberate deviation from the raw plan: entity repo registrations survive Stage A (L3).

## ~~[x] Step 1 — Dal: drop `abstract` from BaseRepo<T> and add SelectFirstAsync~~
- Files: inspect: src/MobileShop.Dal/Repos/Base/IBaseRepo.cs, Repos/Base/BaseRepo.cs,
  src/MobileShop.Tests/Dal/BaseClass/BaseRepoTests.cs; modify: those three; create: none;
  do not touch: any derived repo, any service, DI, Web.
- Symbols: IBaseRepo<T>.SelectFirstAsync (new); BaseRepo<T> (remove `abstract`);
  BaseRepo<T>.SelectFirstAsync (new virtual); BaseRepoTests (extend).
- Current -> Desired: IBaseRepo<T> has no ordered top-1 projection (TransactionRepo.GetEarliestTransactionDateAsync
  needs one); BaseRepo<T> is abstract although nothing overrides it and its 15 derivations are passthroughs.
  Desired: one extra async member and a concrete base class.
- Change:
  1. IBaseRepo<T>: add
     Task<TResult?> SelectFirstAsync<TResult, TKey>(Expression<Func<T, bool>> predicate,
         Expression<Func<T, TKey>> orderBy, Expression<Func<T, TResult>> selector);
  2. BaseRepo<T>: implement it as
     Table.AsNoTracking().Where(predicate).OrderBy(orderBy).Select(selector).FirstOrDefaultAsync()
     (virtual, async).
  3. BaseRepo<T> declaration: remove the `abstract` keyword only. No other change; the 15 derived repos keep
     compiling untouched.
  4. BaseRepoTests: seed >=2 rows and assert SelectFirstAsync returns the extreme row for both directions,
     and null for a non-matching predicate.
- Depends on: none.
- Edge cases / error handling: the query stays server-side (no ToList before OrderBy); AsNoTracking matches
  SelectAllAsync's read-only shape; null when nothing matches.
- Tests: extend src/MobileShop.Tests/Dal/BaseClass/BaseRepoTests.cs (existing file).
- Verify: dotnet build src/MobileShop.slnx --nologo && dotnet test src/MobileShop.slnx --nologo --no-build
  --filter "FullyQualifiedName~UserRepoTests"
- Done when: build green, BaseRepoTests green, no other file touched.
- Risk: LOW. Confidence: HIGH.

## ~~[x] Step 2 — DI: rename to AddMobileShopRepository and register the open generic~~
- Files: inspect: src/MobileShop.Services/ServiceCollectionExtensions.cs; modify: that file only;
  do not touch: WebApplicationBuilderExtensions.cs, ApiApplicationBuilderExtensions.cs, any repo class,
  any test.
- Symbols: ServiceCollectionExtensions.AddMobileShopRepository (renamed from AddMobileShopRepositories,
  currently :62), its call site in AddMobileShop (:27), IBaseRepo<> / BaseRepo<>.
- Current -> Desired: 15 AddScoped<IXRepo, XRepo>() lines (:64-78) and no generic binding. Desired: those
  15 lines kept as-is plus one services.AddScoped(typeof(IBaseRepo<>), typeof(BaseRepo<>));
- Change: rename the method and its call site; append the open-generic registration at the end of the
  method; leave every existing registration in place; update the XML doc comment to state that per-entity
  registrations are removed per area as their consumers migrate, with the remainder removed in Stage H.
- DO NOT "tidy" the 15 lines: the 9 entity data services (:100-119) and all 21 pages still consume them.
  Removing them compiles and the whole test suite still passes (no test resolves through DI) while every
  page fails at runtime. Step 6 is what catches it.
- Depends on: Step 1.
- Edge cases / error handling: the if (useApi) branch stays byte-identical (entity Api stubs are untouched
  in Stage A); IBaseRepo<Phone> and IPhoneRepo are distinct service types, so both registrations coexist
  harmlessly.
- Tests: none added by this step.
- Verify: dotnet build src/MobileShop.slnx --nologo && dotnet test src/MobileShop.slnx --nologo --no-build
  (runtime proof is Step 6).
- Done when: build + suite green and Step 6 green.
- Risk: HIGH. Confidence: HIGH.

## [ ] Step 3 — Create the six area interfaces and the two shared ViewModels
- Files: create: src/MobileShop.Services/DataServices/Interfaces/{IHomeDataService, IProductsDataService,
  IPeopleDataService, ITransactionsDataService, IReportsDataService, IAccountDataService}.cs and
  src/MobileShop.Models/ViewModels/Web/{DropdownOptionViewModel, ServiceResult}.cs;
  do not touch: existing interfaces, DI, any Dal/Api class, any page, any .cshtml.
- Symbols: the six interfaces; DropdownOptionViewModel(int Id, string Name);
  ServiceResult(bool Succeeded, string? Message).
- Current -> Desired: no area interfaces exist; pages consume 9 entity services and 7 repo types directly.
- Change: declare the members below, async-only (every one has an async equivalent today), XML-doc'd like the
  existing interfaces. No entity type may appear in any signature, in or out.
  * IHomeDataService:
      Task<DashboardStockSummary> GetStockAsync();
      Task<IReadOnlyList<TransactionCardViewModel>> GetRecentTransactionsAsync(int count = 20);
  * IProductsDataService:
      Task<IReadOnlyList<ProductListItemViewModel>> GetInventoryRowsAsync();
      Task<IReadOnlyList<ProductListItemViewModel>> GetSecondHandRowsAsync();
      Task<ProductDetailsViewModel?> GetDetailsAsync(int id, string type);   // includes its transaction rows
      Task<IReadOnlyList<DropdownOptionViewModel>> GetManufacturersAsync();
      Task<IReadOnlyList<DropdownOptionViewModel>> GetModelsAsync(int manufacturerId);
      Task<IReadOnlyList<DropdownOptionViewModel>> GetColorsAsync();
      Task<IReadOnlyList<string>> GetGuaranteeCorporationsAsync();
      Task<DropdownOptionViewModel> CreateManufacturerAsync(string name);
      Task<DropdownOptionViewModel> CreateModelAsync(int manufacturerId, string name);
      Task<DropdownOptionViewModel> CreateColorAsync(string name);
      Task<ServiceResult> CreatePhoneAsync(CreatePhoneInputModel input);
      Task<ServiceResult> CreateAppleIdAsync(CreateAppleIdInputModel input);
  * IPeopleDataService:
      Task<IReadOnlyList<CustomerListItemViewModel>> GetCustomerRowsAsync(string sortBy, bool ascending);
      Task<IReadOnlyList<SellerListItemViewModel>> GetSellerRowsAsync(string sortBy, bool ascending);
      Task<CustomerDetailsViewModel?> GetCustomerDetailsAsync(int id);   // includes product rows
      Task<SellerDetailsViewModel?> GetSellerDetailsAsync(int id);       // includes product rows
      Task<ServiceResult> CreateCustomerAsync(CreateCustomerInputModel input);
      Task<ServiceResult> CreateSellerAsync(CreateSellerInputModel input);

  * ITransactionsDataService:
      Task<IReadOnlyList<TransactionListItemViewModel>> GetListAsync(string? direction, int take, bool ascending);
      Task<TransactionDetailsViewModel?> GetDetailsAsync(int id);
      Task<byte[]> GetTransactionFactorPdfAsync(int transactionId);
      Task<IReadOnlyList<PartyOptionViewModel>> GetSellersAsync();
      Task<IReadOnlyList<PartyOptionViewModel>> GetCustomersAsync();
      Task<IReadOnlyList<ProductListItemViewModel>> GetSelectableProductsAsync(TransactionDirection direction);
      Task<ServiceResult> RecordBuyAsync(BuyInputModel input);
      Task<ServiceResult> RecordSellAsync(SellInputModel input);
      Task<InvoiceViewModel?> GetInvoiceAsync(int transactionId);        // absorbed from InvoiceDataService
      Task<byte[]> GenerateInvoicePdfAsync(int transactionId);           // absorbed from InvoiceDataService
  * IReportsDataService:
      Task<IReadOnlyList<ProfitLossRowViewModel>> GetProfitLossRowsAsync(DateTime? from, DateTime? to);
      Task<decimal> GetProfitLossTotalAsync(DateTime? from, DateTime? to);
      Task<DateTime?> GetEarliestTransactionDateAsync();
      Task<IReadOnlyList<DistributionRow>> GetDistributionRowsAsync(decimal totalProfit);
  * IAccountDataService:
      void EnsureAdminUser();
      Task<string?> GetAdminUsernameAsync();
      Task<bool> ValidateCredentialsAsync(string username, string plainPassword);
      Task<bool> ChangePasswordAsync(string username, string plainNewPassword);
- Depends on: none to compile; read Step 2's file first so names line up.
- Edge cases / error handling: DropdownOptionViewModel must expose Id and Name so the existing
  `<select asp-items="@(new SelectList(Model.Manufacturers, "Id", "Name"))">` markup in
  CreatePhone.cshtml:15,26,61,104 keeps working with no .cshtml edits; Corporations stays a string list
  because its markup is `new SelectList(Model.Corporations)`. GetDistributionRowsAsync takes a decimal, not
  an employee list, because the service loads its own employees (L5; see §C Stage F for the
  PersonNavigation requirement). No sync twins: an async path exists for every member.
- Tests: none (no behaviour).
- Verify: dotnet build src/MobileShop.slnx --nologo
- Done when: six interfaces + two ViewModels compile; no existing file modified.
- Risk: LOW. Confidence: HIGH.

## [ ] Step 4 — Create the six Api area stubs
- Files: create: src/MobileShop.Services/DataServices/Api/{ApiHomeDataService, ApiProductsDataService,
  ApiPeopleDataService, ApiTransactionsDataService, ApiReportsDataService, ApiAccountDataService}.cs;
  do not touch: ApiDataServiceBase<T>, the 8 existing entity Api stubs, src/MobileShop.Api.
- Symbols: the six Api area classes.
- Current -> Desired: only entity-oriented Api stubs exist. Desired: six area stubs that compile the
  UseApi: true path.
- Change: each class implements its area interface; every member body is
  throw new NotImplementedException("Api<Area>DataService is not implemented yet.");
  No HTTP code and no base class (ApiDataServiceBase<T> is entity-typed and cannot serve these interfaces).
  Implement every member; the compiler lists any omission.
- Depends on: Step 3.
- Edge cases / error handling: none; these types are never resolved while UseApi is false.
- Tests: none.
- Verify: dotnet build src/MobileShop.slnx --nologo
- Done when: six stubs compile.
- Risk: LOW. Confidence: HIGH.

## [ ] Step 5 — Register the Api area stubs behind UseApi
- Files: inspect/modify: src/MobileShop.Services/ServiceCollectionExtensions.cs (AddMobileShopDataServices,
  :94-121); do not touch: the else branch, appsettings, the Api host.
- Symbols: AddMobileShopDataServices.
- Current -> Desired: the if (useApi) branch registers 8 entity Api services; the else branch registers
  9 entity Dal services. Desired: the if (useApi) branch additionally registers the six area pairs
  I<Area>DataService -> Api<Area>DataService.
- Change: add exactly six AddScoped lines inside the existing if (useApi) block. Do NOT add the Dal lines
  yet: HomeDataService..AccountDataService do not exist until Stages B–G, so referencing them here would not
  compile. Each area stage adds its own Dal line to the else branch; Stage H removes the 9 entity
  registrations from both branches.
- Depends on: Step 4.
- Edge cases / error handling: with UseApi: false nothing resolves the area interfaces yet, which is correct
  — no page injects them until Stage B. UseApi stays false; the Api host (ApiApplicationBuilderExtensions.cs:15)
  passes no key and keeps falling back to Dal.
- Tests: none added.
- Verify: dotnet build src/MobileShop.slnx --nologo && dotnet test src/MobileShop.slnx --nologo --no-build
- Done when: build + suite green; the UseApi: false path unchanged (Step 6 proves it).
- Risk: MEDIUM. Confidence: HIGH.

## [ ] Step 6 — Stage A validation
- Files: none.
- Symbols: N/A.
- Current -> Desired: Stage A's changes are proven not to have broken DI or any page.
- Change: run the standard chain, then serve the app and exercise every page. Use the Production environment
  deliberately: Development runs DatabaseInitializer.InitializeForDevelopment, which deletes and recreates
  MobileShop.db.
- Verify (chain 1): dotnet build src/MobileShop.slnx --nologo && dotnet test src/MobileShop.slnx --nologo --no-build
- Verify (chain 2): ASPNETCORE_ENVIRONMENT=Production dotnet run --project src/MobileShop.Web
  --urls http://localhost:5199 (keep this session attached and read it until it reports it is listening),
  then in a second call:
  for p in / /Products /Products/SecondHand /People/Customers /People/Sellers /Transactions
  /Reports/ProfitLoss /Account/Login; do printf '%s %s\n' "$(curl -s -o /dev/null -w '%{http_code}'
  http://localhost:5199$p)" "$p"; done
- Done when: exit code 0 for build+test, every route returns 200, and `git status --short MobileShop.db` shows
  no change (the served pass is non-destructive).
- Risk: MEDIUM. Confidence: HIGH.

## Global Definition of Done (Stage A)
- IBaseRepo<T>/BaseRepo<T> expose SelectFirstAsync; BaseRepo<T> is instantiable.
- AddMobileShopRepository registers IBaseRepo<> -> BaseRepo<> and still registers the 15 entity repos; every
  pre-existing page still resolves and renders.
- Six area interfaces and six Api area stubs exist; no interface signature mentions an entity type.
- AddMobileShopDataServices registers the Api area pairs behind UseApi (default false).
- dotnet build src/MobileShop.slnx --nologo and dotnet test src/MobileShop.slnx --nologo --no-build succeed,
  and the Production served-page pass returns 200 for all eight routes.
- No user-visible behaviour change; MobileShop.db untouched.

## Execution notes
- Work fast and compact: chain dependent commands with && and group independent read-only checks in one
  { ...; } call; keep verification output and the act.md report short.
- One step per commit, using the step header exactly as written. Never stage unrelated pre-existing changes
  (the uncommitted .clinerules/*.md edits are not yours to stage).
- Do not restate this plan back in chat.

<!-- END plan.md -->
