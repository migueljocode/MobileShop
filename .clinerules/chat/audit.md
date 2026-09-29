# Audit — Job A (Plan Review): feature DataServices + thin generic repo

**Reviewed**: the planner's raw plan for the "feature DataServices + thin generic repo" migration
(pasted in chat — `.clinerules/chat/plan.md` was 0 bytes at review time).
**Stage**: 8 new stages (A–H). The Correction annex at the end holds the approved stage lines and the
approved `plan.md` text for Stage A.
**Verdict**: **APPROVED WITH CORRECTIONS** — 11 HIGH findings, every one a narrow doc-level fix inside
the chosen architecture (area DataServices returning ViewModels + open-generic `IBaseRepo<T>`).
No re-architecture, no second review round: apply the annex verbatim and execute.

## Checks performed (verified against source this session)
- `IBaseRepo<T>` / `BaseRepo<T>` member surface, `abstract`ness, and that no derived repo overrides anything.
- All 15 concrete repos + 16 repo interfaces, and each specialized member's real implementation.
- `ServiceCollectionExtensions.cs`: method visibility, the 15 repo registrations, the 9 entity data
  services, and the `UseApi` branch shape.
- All 21 PageModel constructor signatures, plus which pages inject repos directly.
- Both hosts' `AddMobileShop(...)` call sites, and that `src/MobileShop.Api` has no repo/service references.
- Whether tests resolve through DI (they do not) and which test files construct deleted types.
- `DistributionCalculator`'s real requirement on `Employee.PersonNavigation`.
- Whether `IInvoiceDataService` has any page consumer (it does not).
- Which `ITransactionDataService` members have no page consumer.
- Global-usings fallout from deleting the repo interfaces.

## Findings

### HIGH

**H1 — `BaseRepo<T>` is `abstract`, so the proposed `IBaseRepo<>` → `BaseRepo<>` registration has no
constructible implementation.**
- Location: raw plan Step A.2; `src/MobileShop.Dal/Repos/Base/BaseRepo.cs:4`.
- Problem: Step A.2 registers the open generic while the same migration deletes every concrete repo
  (`PhoneRepo`, `UserRepo`, …). `BaseRepo<T>` is `public abstract class BaseRepo<T>(AppDbContext context)`,
  so the container cannot build it.
- Evidence: `BaseRepo.cs:4`; `grep -rn 'override' src/MobileShop.Dal/Repos/*.cs` → 0 hits; the 15 concrete
  repos are plain `class XRepo(AppDbContext context) : BaseRepo<X>(context), IXRepo` and 10 of them have
  empty bodies — the `abstract` modifier protects nothing.
- Fix: drop `abstract` from `BaseRepo<T>` in Stage A (owner-approved). No other change needed.

**H2 — Step A.2 removes the entity repo registrations while their consumers still exist: the app breaks
at runtime and the stated gate cannot see it.**
- Location: raw plan Step A.2 (`ServiceCollectionExtensions.cs:64-78`) vs the doc's §1 ("after their call
  sites are migrated") and Step H item 2.
- Problem: the 9 entity data services (`:100-119`) still consume `IPhoneRepo`/`IUserRepo`/… and all 21
  pages still inject those services. Removing the registrations makes resolution fail for every page,
  while `dotnet build` + `dotnet test` stay green — no test resolves through DI: `PhoneDataServiceTests`
  does `new PhoneDataService(new PhoneRepo(Context), NullLogger<PhoneDataService>.Instance)`,
  `RepoTestBase` builds its own `UseInMemoryDatabase(Guid…)` context, `IndexModelTests` uses
  `Mock<IPdfGenerator>` plus direct construction, and there is no host-level test in `src/MobileShop.Tests`.
- Also wrong in that step's edge-case note: "Code using `IPhoneRepo` will still resolve via
  `IBaseRepo<Phone>`" — false. An open-generic registration maps `IBaseRepo<T>` only; a request for
  `IPhoneRepo` is unaffected. There are also 15 registrations, not 14.
- Fix: add the open generic and **keep** all 15 registrations; delete each per-entity registration in the
  area stage that kills its last consumer; delete the remainder in Stage H. Add a Production-environment
  served-page pass as stage-end evidence (H10).

**H3 — Step A.5 registers six Dal area services that do not exist yet: compile error in Stage A, not a
runtime one.**
- Location: raw plan Step A.5, whose own edge-case note claims this is a runtime resolution problem.
- Problem: `HomeDataService`, `ProductsDataService`, `PeopleDataService`, `TransactionsDataService`,
  `ReportsDataService`, `AccountDataService` are created in Stages B–G. A reference to a non-existent type
  does not compile.
- Fix: A.5 registers only the six Api area stubs inside `if (useApi)`; each area stage adds its own Dal
  line to the `else` branch; Stage H removes the 9 entity registrations from both branches.

**H4 — "Specialized repo behavior becomes expressions on `IBaseRepo<T>`" is false for three of the six
specialized members, and the IMEI claim is wrong.**
- Location: raw plan Steps A.2/E.1/F.1; `UserRepo.cs:16-52`, `TransactionRepo.cs:15-20`,
  `EmployeeRepo.cs:15-25`, `ProductRepo.cs:13-16`.
- `IUserRepo.ChangePassword(...)` ×4 is read-modify-write (find → set `PasswordHash` → `Update`), and the
  repo does **not** hash — `UserDataService`/`IPasswordHasher` hashes upstream. Not an expression.
- `ITransactionRepo.GetEarliestTransactionDateAsync()` (`TransactionRepo.cs:15-20`) is
  `Where(!IsDeleted).OrderBy(Date).Select((DateTime?)Date).FirstOrDefaultAsync()`. `IBaseRepo<T>` has no
  ordered top-1 projection; `Select(predicate, selector)` takes `FirstOrDefault()` in key order. A
  "pure expression" port changes semantics or forces a full-column pull.
- `IEmployeeRepo.FindAllActiveAsync()` (`EmployeeRepo.cs:15-25`) applies `.Include(e => e.PersonNavigation)`
  plus `OrderBy(FirstName).ThenBy(LastName)`. `IBaseRepo<T>` exposes no `Include` and no ordering, so a
  naive port silently loses both the nav load and the sort.
- `IProductRepo.IsInStockAsync` (`ProductRepo.cs:13-16`) queries `SelectMany(p => p.Transactions)` — it
  means "no Sell transaction exists for this product". A client-side port returns wrong results.
- "IMEI exists" is not a repo member: `ImeiExists`/`ImeiExistsAsync` live on `IPhoneDataService`
  (`Interfaces/IPhoneDataService.cs:36,39`; `PhoneDataService.cs:204,208`).
- Fix: owner-approved single addition to `IBaseRepo<T>`/`BaseRepo<T>`
  (`SelectFirstAsync<TResult, TKey>(predicate, orderBy, selector)`) in Stage A Step 1, plus explicit
  per-member replacement statements in the area stages (annex §C).

**H5 — Entity types leak through the proposed area interfaces, in both directions.**
- Location: raw plan Step A.3 interface lists.
- Problem (in): `AddAsync(Phone)`, `AddAsync(AppleId)`, `FindByEmailAsync → AppleId?`,
  `GetManufacturersAsync → Manufacturer`, `GetModelsByManufacturerAsync → Model`,
  `GetColorsAsync → Color`, `GetDistributionAsync(…, IReadOnlyList<Employee>, …)`, `FindByUsernameAsync → User?`.
  Today `CreatePhoneModel` even exposes `IEnumerable<Manufacturer>/Model/Color` as page properties
  (`CreatePhone.cshtml.cs:11-14`), which the pages bind to.
- Fix (owner decision H5): no entity crosses the boundary. Requests enter as input bind models or
  primitives, results leave as ViewModels or primitives. Concretely: a new
  `DropdownOptionViewModel(int Id, string Name)` for the four dropdowns (keeping `Id`/`Name` means the
  existing markup at `CreatePhone.cshtml:15,26,61,104` needs no edit), a new
  `ServiceResult(bool Succeeded, string? Message)` for the create/POST paths, `GetAdminUsernameAsync()`
  for `ProfileModel:28-32`, and `GetDistributionRowsAsync(decimal totalProfit)` instead of taking an
  employee list.

**H6 — Five pages would break in Stage H because no stage migrates them.**
- Location: raw plan Step D.2 (customers/sellers lists only) and Step G.2 (profile only).
- Evidence: `Account/Login.cshtml.cs:3,27` (injects `IUserDataService`, calls `ValidateCredentialsAsync`);
  `People/CreateCustomer.cshtml.cs:3`; `People/CreateSeller.cshtml.cs:3`; `People/CustomerDetails.cshtml.cs:5`
  (also injects `IProductDataService`); `People/SellerDetails.cshtml.cs:5`.
- Also missing: `IAccountDataService` omits `EnsureAdminUser()`, the only non-page consumer
  (`WebApplicationBuilderExtensions.cs:31`, dev-only block).
- Fix: the annex's page→service map covers all 21 PageModels; Stage D migrates all six people pages and
  Stage G migrates Login + Profile, with `EnsureAdminUser()` on `IAccountDataService`.

**H7 — Test scope is wrong in both directions: Step H.1 claims "Tests: None", Step H.4 deletes test
infrastructure the surviving code needs, and no area step ports coverage.**
- Evidence: 8 files under `src/MobileShop.Tests/Services/DataServices/Dal/` construct the very types
  H.1 deletes (e.g. `new PhoneDataService(new PhoneRepo(Context), NullLogger<…>.Instance)`) → H.1 breaks
  the build; H.4 proposes deleting `Dal/Repos/*RepoTests.cs` **and** `Dal/BaseClass/BaseRepoTests.cs`,
  but `BaseRepoTests` covers the *surviving* generic base and `RepoTestBase` + `TestDataHelpers` are shared
  infrastructure (`IndexModelTests : RepoTestBase`).
- Fix: delete only the **14** `*RepoTests` in Stage H; keep `BaseRepoTests` (extend it with
  `SelectFirstAsync` coverage — Stage A Step 1), `RepoTestBase`, `TestDataHelpers`; H.1 deletes/replaces
  the 8 entity data-service test files; every area stage ports the assertions for the members it absorbs
  (mock-only edits are not evidence).

**H8 — Stage H misses the namespace death: four GlobalUsings files will fail to compile.**
- Evidence: deleting the 16 repo interfaces removes the namespace `MobileShop.Dal.Repos.Interfaces`, and
  deleting the 15 concrete repos empties `MobileShop.Dal.Repos`. `global using` lines referencing them then
  raise CS0246 in `src/MobileShop.Dal/GlobalUsings.cs:8`, `src/MobileShop.Services/GlobalUsings.cs:19,20,21`,
  `src/MobileShop.Tests/GlobalUsings.cs:9,10,11`, `src/MobileShop.Web/GlobalUsings.cs:7`.
- Not affected: `src/MobileShop.Api` (3 files, no repo/service references) — so the "never modify
  `src/MobileShop.Api`" rule holds throughout.

**H9 — Reports: the naive `IBaseRepo<Employee>` port makes the distribution throw at runtime.**
- Evidence: `DistributionCalculator.Calculate` (`src/MobileShop.Services/Logging/Settings/DistributionSettings.cs:31+`)
  requires matched employees with `PersonNavigation != null` and throws `InvalidOperationException`
  otherwise; `IBaseRepo<T>.FindAllAsync` is `AsNoTracking()` with no `Include`, and
  `Employee.PersonNavigation` (`Employee.cs:9`) is a navigation that today only loads because
  `EmployeeRepo.FindAllActiveAsync` Includes it. InMemory tests hand-build graphs, so tests would pass
  while the served page throws.
- Fix: `ReportsDataService.GetDistributionRowsAsync(decimal totalProfit)` loads active employees and
  attaches `PersonNavigation` (join via `IBaseRepo<Person>`) before calling `DistributionCalculator`
  unchanged.

**H10 — Every step's verification is build+test only, which cannot prove DI wiring or rendered pages.**
- This repo's own history already proved that (the Task 3 stage needed served-HTML evidence).
- Fix: each stage's final step runs the standard chain **and** a Production-environment served-page pass
  (`ASPNETCORE_ENVIRONMENT=Production`; Development would run `DatabaseInitializer.InitializeForDevelopment`
  and recreate `MobileShop.db`). Stage A's version is in annex §B Step 6.

**H11 (plan hygiene) — Step A.1 creates an empty directory.**
- Git cannot stage or commit an empty directory, so the step has nothing to commit and its "Done when:
  Directory exists" is unverifiable in the pipeline's commit flow.
- Fix: drop A.1; create `DataServices/Shared/` in the first step that actually adds a shared helper.

### MEDIUM (notes — optional for the planner/actor, not blockers)
- Logging convention: `DataServiceBase<TService,TEntity>` is where `LogInformation`/`LogWarning` on
  add/update/delete lives; the area services' dependency list omits `ILogger<T>`. Keep the pattern (annex L6).
- Dead types after migration: `IDataService<T>`, `DataServiceBase<,>`, `ApiDataServiceBase<T>` become
  unreferenced; Stage H should delete them explicitly.
- Api stubs: the new area interfaces are not entity-typed, so `ApiDataServiceBase<T>` cannot serve them.
  The existing convention is a per-member `throw new NotImplementedException("Api<Name> is not implemented yet.")`
  (see `ApiTransactionDataService.cs`). Any interface member added in a later area stage must land with its
  Api body in the same step or the build breaks.
- Api host: `AddMobileShop(...)` is called from `WebApplicationBuilderExtensions.cs:16` and
  `ApiApplicationBuilderExtensions.cs:15`; the Api host carries no `UseApi` key, so it keeps selecting Dal.
  Area Api stubs belong in `src/MobileShop.Services/DataServices/Api/`, never in the `src/MobileShop.Api` project.
- Keep `UserRepo.FindByUsername`'s `u.Username.ToLower() == username.ToLower()` verbatim; swapping in
  `StringComparison.OrdinalIgnoreCase` does not translate server-side on SQLite.
- `IInvoiceDataService` currently has **no** page consumer (DI registration + tests only), so folding it
  into the Transactions area (owner decision H6) carries no behavior risk — but its
  `InvoiceDataServiceTests` coverage must move with it.
- `GetProductsBoughtByShop*`, `GetProductsSoldByShop*`, `GetRecent`/`GetRecentAsync` and the sync
  `GetByProduct*` have no page consumer either; Stage E must list them as explicitly dropped, with their tests.

### LOW
- `AddMobileShopRepository` is `private static` (`ServiceCollectionExtensions.cs:62`, call site `:27`), so
  the rename touches no public surface.
- 10 of the 15 derived repos have empty bodies
  (`class ColorRepo(AppDbContext context) : BaseRepo<Color>(context), IColorRepo { }`), confirming the
  "no empty or forwarding derived repos" decision.
- The 14 `*RepoTests` largely duplicate CRUD assertions `BaseRepoTests` already covers.

## Missing Implementation Details
- Per-step `Files: inspect/modify/create/do-not-touch`, exact symbols, current→desired, Risk + Confidence,
  and an exact `Verify:` command (annex §B supplies these for Stage A).
- The exact `IBaseRepo<T>` addition (`SelectFirstAsync`) rather than a "make it work" instruction.
- The DI-resolution evidence command per stage-end step (H10).
- Stage E's member-by-member mapping of the existing `ITransactionDataService` (123 lines), marking each
  member migrated or dropped, since several are unconsumed.
- Per-area test-porting lists (which of the 8 entity-service test files and 8 page test files change).

## Approval Status
**APPROVED WITH CORRECTIONS** — the 11 HIGH findings are all incorporated into the Correction annex below;
no re-review round is required once the annex is applied as written. The 8-stage split (A–H) is approved,
including the deliberate deviation that entity repo registrations survive Stage A.

## Reviewer checklist
- [x] Job identified first — A (plan review); no Job B work mixed in.
- [x] Verdict written into `.clinerules/chat/audit.md`.
- [x] Output length tracks problems found (11 HIGH warranted full write-ups).
- [x] Exactly one verdict.
- [x] Role taken from `reviewer.md`, not the mode toggle.
- [x] No stage added or ticked in `to-do.md`; `plan.md` not written.

---

# Correction annex (for the planner — not part of the verdict)

How this annex is used, given `planner.md:32` (to-do.md = **stages only**, one line each) and
`planner.md` (plan.md = **the current stage only**):

1. **§A → `.clinerules/to-do.md`**: paste the eight one-line stage entries exactly as given. Nothing else
   goes into to-do.md. The reviewer ticks a stage at sign-off; the planner never ticks.
2. **§B → `.clinerules/chat/plan.md`**: paste the whole block verbatim when Stage A becomes current. It is
   self-contained (files, symbols, changes, tests, verify commands, risks). The actor works from plan.md
   plus to-do.md only.
3. **§C** holds the binding decisions for Stages B–H; each becomes its own §B-style plan.md when its stage
   becomes current. Do not paste §C into plan.md as a whole.

All of §A–§C already folds in the 11 HIGH findings and the owner's decisions (drop `abstract`; keep entity
registrations until their consumers die; add `SelectFirstAsync`; no `IPdfGenerator` in PageModels;
no entities across the page boundary; invoice folded into Transactions; unused tests removed).

## §A — to-do.md entries (stages only, paste verbatim)
- [ ] Stage A — Foundation: generic repo registration, area interfaces, Api stubs, DI seams
- [ ] Stage B — Home: HomeDataService + dashboard
- [ ] Stage C — Products: ProductsDataService + product pages
- [ ] Stage D — People: PeopleDataService + customer/seller pages
- [ ] Stage E — Transactions: TransactionsDataService + transaction/invoice/PDF
- [ ] Stage F — Reports: ReportsDataService + profit/loss
- [ ] Stage G — Account: AccountDataService + login/profile
- [ ] Stage H — Cleanup: delete entity services/repos/tests, GlobalUsings, final validation

Note: this plan does not restore the old Task 6/7/8 backlog that commit `1450979` removed from to-do.md
(auto-refresh on combobox change, "N/A" for missing colour, "All" preset). It remains in git history.

## §B — plan.md for Stage A (paste verbatim)

```markdown
# Plan — Stage A: Foundation — generic repo + area DataService seams

## Proposed stages
(approved by the reviewer; the planner copies these into to-do.md as unchecked boxes)
- [ ] Stage A — Foundation: generic repo registration, area interfaces, Api stubs, DI seams
- [ ] Stage B — Home: HomeDataService + dashboard
- [ ] Stage C — Products: ProductsDataService + product pages
- [ ] Stage D — People: PeopleDataService + customer/seller pages
- [ ] Stage E — Transactions: TransactionsDataService + transaction/invoice/PDF
- [ ] Stage F — Reports: ReportsDataService + profit/loss
- [ ] Stage G — Account: AccountDataService + login/profile
- [ ] Stage H — Cleanup: delete entity services/repos/tests, GlobalUsings, final validation

One stage per plan.md: this file fully specifies Stage A. Stages B–H get their own plan.md when they become
current; the Locked Decisions and the page→service map below are binding on every stage.

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

## [ ] Step 1 — Dal: drop `abstract` from BaseRepo<T> and add SelectFirstAsync
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
  --filter "FullyQualifiedName~BaseRepoTests"
- Done when: build green, BaseRepoTests green, no other file touched.
- Risk: LOW. Confidence: HIGH.

## [ ] Step 2 — DI: rename to AddMobileShopRepository and register the open generic
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
```

<!-- END plan.md -->

## §C — Stage sketches B–H (binding decisions; each becomes its own plan.md)

### Stage B — Home
- Goal: `HomeDataService` (Dal) implementing `IHomeDataService`; inject `IBaseRepo<Phone>`, `IBaseRepo<AppleId>`,
  `IBaseRepo<Transaction>`, `ILogger<HomeDataService>`.
- Port `GetStockAsync` from `PhoneDataService.QuantityAsync`/`SecondHandQuantityAsync`/`AvailableSecondHandQuantityAsync`
  + `AppleIdDataService.QuantityAsync` (`Index.cshtml.cs:11-17` is the exact current shape) and
  `GetRecentTransactionsAsync` from `TransactionDataService.GetRecentCardsAsync`.
- Files: create `DataServices/Dal/HomeDataService.cs`; modify `ServiceCollectionExtensions.cs` (add the Dal line
  to the else branch), `Web/Pages/Index.cshtml.cs` (ctor becomes `(IHomeDataService dataService)`); port the
  assertions those members had in `PhoneDataServiceTests`/`AppleIdDataServiceTests`/`TransactionDataServiceTests`.
- Verify: build + full suite + the Production served-page pass for `/`.
- Risk: MEDIUM. Confidence: HIGH.

### Stage C — Products (split into three steps)
- Goal: `ProductsDataService` (Dal) + all five product pages.
- C1 (service, list/details/dropdowns): inject `IBaseRepo<Phone>`, `<AppleId>`, `<Product>`, `<Manufacturer>`,
  `<Model>`, `<Category>`, `<Color>`, `<Guarantee>`, `<SecondHand>`, `<Transaction>`, `ILogger<>`.
  `GetInventoryRowsAsync` = phone + AppleId inventory rows; `GetSecondHandRowsAsync` = the same concat+order
  `SecondHand.cshtml.cs:12-14` does today; `GetDetailsAsync(id, type)` routes to phone/AppleId details and
  includes its transaction rows (absorbs `GetProductTransactionsAsync`, `Products/Details.cshtml.cs:23`);
  `GetGuaranteeCorporationsAsync` = `FindAllAsync(g => g.Corporation != null)` then in-memory `Distinct()`
  (there is no async distinct-projection primitive — do not reach for `SelectAllAsync`+`Distinct` on the
  `IQueryable`); any "in stock" check = `!AnyAsync(t => t.ProductId == id && t.Direction == Sell)`
  (preserve `ProductRepo.cs:13-16` semantics).
- C2 (create paths): move the CreatePhone/CreateAppleId POST logic into the service — catalog dropdown loads,
  create-or-get manufacturer/model/category, IMEI duplicate check, Apple-ID email duplicate check, the
  "Shop Warranty" guarantee default, and the modal handlers (`CreateManufacturerAsync`, `CreateModelAsync`,
  `CreateColorAsync`). **Apple ID passwords stay plaintext — never hash them.**
- C3 (remaining pages): migrate `Products/Index`, `Products/Details`, `Products/SecondHand` to a single
  `IProductsDataService dataService`. Drop the direct repo injections from both create pages
  (`CreatePhone.cshtml.cs:4-9`, `CreateAppleId.cshtml.cs:4-7`) and the entity page properties
  (`CreatePhone.cshtml.cs:11-14`) in favour of `DropdownOptionViewModel`, so no .cshtml edit is needed.
- Tests: `CreatePhoneModelTests`, `CreateAppleIdModelTests` + the phone/AppleId/Product service tests get
  ported to the area service; `InvoiceDataServiceTests` is not touched here.
- Verify each step: build + full suite; C3 also the Production served-page pass for the five product routes.
- Risk: HIGH. Confidence: MEDIUM.

### Stage D — People
- Goal: `PeopleDataService` (Dal) implementing `IPeopleDataService`; inject `IBaseRepo<Customer>`, `<Seller>`,
  `<Person>`, `<Product>`, `ILogger<>`.
- Migrate **all six** people pages: Customers, Sellers, CustomerDetails, SellerDetails, CreateCustomer,
  CreateSeller. Port the "Name"/"Phone"/"Count" sort with invalid→"Name" fallback verbatim from
  `CustomerDataService.GetListRows`/`SellerDataService.GetListRows`, and the purchased/sold product rows the
  detail pages show today.
- Files: create `DataServices/Dal/PeopleDataService.cs`; modify `ServiceCollectionExtensions.cs` (Dal line),
  the six page models; port `CustomerDataServiceTests`, `SellerDataServiceTests`, `CustomersModelTests`,
  `SellersModelTests` (+ new coverage for the four previously untested pages).
- Verify: build + full suite + the Production served-page pass for `/People/Customers`, `/People/Sellers`.
- Risk: MEDIUM. Confidence: HIGH.

### Stage E — Transactions
- Goal: `TransactionsDataService` (Dal) implementing `ITransactionsDataService`; inject `IBaseRepo<Transaction>`,
  `<Product>`, `<Seller>`, `<Customer>`, `<Phone>`, `<AppleId>`, `IPdfGenerator`, `ILogger<>`.
- Required first: a member-by-member mapping of the existing `ITransactionDataService` (123 lines) into the new
  interface, marking each member *migrated* or *dropped*. No page consumes `GetByProduct*`, `GetRecent`/
  `GetRecentAsync`, `GetProductsBoughtByShop*`, `GetProductsSoldByShop*` — drop them and their tests explicitly.
- Absorb `InvoiceDataService` (`GetInvoiceAsync`, `GenerateInvoicePdfAsync`) and both factor-PDF paths
  (`Transactions/Index.cshtml.cs:40`, `Transactions/Details.cshtml.cs:27`) so **no PageModel injects
  `IPdfGenerator`** (L4). Use the new `SelectFirstAsync` for the earliest-transaction-date query (L5 puts the
  public method on Reports — keep one implementation and one owner).
- Migrate all four transaction pages to a single `ITransactionsDataService dataService`, including Buy/Sell
  party options and selectable products (so those pages need no second service).
- Keep the `ShopSellerId`/`ShopCustomerId` sentinel behavior. Port `TransactionDataServiceTests` and
  `InvoiceDataServiceTests`, plus `IndexModelTests`, `DetailsModelTests`.
- Verify: build + full suite + the Production served-page pass for `/Transactions` and the buy/sell pages.
- Risk: HIGH. Confidence: MEDIUM.

### Stage F — Reports
- Goal: `ReportsDataService` (Dal) implementing `IReportsDataService`; inject `IBaseRepo<Transaction>`,
  `<Employee>`, `<Person>`, `IOptions<DistributionSettings>`, `ILogger<>`.
- Move the profit/loss queries (`GetProfitLossRowsAsync`, `GetProfitLossTotalAsync` — port from
  `TransactionDataService`) and `GetEarliestTransactionDateAsync` (new `SelectFirstAsync`) into this service,
  so `Reports/ProfitLoss.cshtml.cs` injects exactly one service.
- `GetDistributionRowsAsync(decimal totalProfit)`: load active employees and **attach `PersonNavigation`**
  (join via `IBaseRepo<Person>`), then call `DistributionCalculator.Calculate(totalProfit, employees,
  settings.Value)` unchanged. Without the attachment the calculator throws `InvalidOperationException`
  (its name matching requires `PersonNavigation != null`) while build and tests stay green — this is H9.
- Preserve the automatic/manual date-range bounds logic exactly (the Task 3 fix lives in this file).
- Files: create `DataServices/Dal/ReportsDataService.cs`; modify `ServiceCollectionExtensions.cs`,
  `Reports/ProfitLoss.cshtml.cs`; port `ProfitLossTests` and the distribution coverage.
- Verify: build + full suite + the Production served-page pass for `/Reports/ProfitLoss`.
- Risk: HIGH. Confidence: MEDIUM.

### Stage G — Account
- Goal: `AccountDataService` (Dal) implementing `IAccountDataService`; inject `IBaseRepo<User>`,
  `IPasswordHasher`, `ILogger<>`.
- Port from `UserDataService`: `EnsureAdminUser` (keep it in the same dev-only startup block at
  `WebApplicationBuilderExtensions.cs:26-31` and keep throwing when the seeded admin is missing),
  `FindByUsername`'s `u.Username.ToLower() == username.ToLower()` verbatim (do not "improve" it to
  `StringComparison.OrdinalIgnoreCase` — it will not translate on SQLite), Argon2 validate/change, and
  `GetAdminUsernameAsync` so `ProfileModel:28-32` gets a string instead of the `User` entity.
- Migrate **Login and Profile** (`Login.cshtml.cs:3,27` is the page the raw plan missed).
- Files: create `DataServices/Dal/AccountDataService.cs`; modify `ServiceCollectionExtensions.cs`,
  `Web/Extensions/WebApplicationBuilderExtensions.cs` (the startup call switches to `IAccountDataService`),
  `Account/Login.cshtml.cs`, `Account/ProfileModel.cs`; port `UserDataServiceTests`, `ProfileModelTests`.
- **Application-user passwords stay hashed (Argon2); Apple ID inventory passwords stay plaintext.**
- Verify: build + full suite + the Production served-page pass for `/Account/Login`, and a login round trip
  with the default dev credentials.
- Risk: MEDIUM. Confidence: HIGH.

### Stage H — Cleanup
- H1: delete the 9 entity Dal services (`UserDataService` … `EmployeeDataService`), plus
  `Dal/Base/DataServiceBase.cs` once unreferenced, **and** the 8 entity data-service test files under
  `src/MobileShop.Tests/Services/DataServices/Dal/` (they construct the deleted types — H1 is not "Tests: None").
- H2: delete the 9 entity interfaces (`DataServices/Interfaces/*`) + `Interfaces/Base/IDataService.cs`.
- H3: delete the 9 entity Api stubs + `Api/Base/ApiDataServiceBase.cs`.
- H4: delete the 15 repo classes and 16 repo interfaces, keep `Repos/Base/{IBaseRepo,BaseRepo}.cs`, and delete
  only the **14** `src/MobileShop.Tests/Dal/Repos/*RepoTests.cs` — keep `BaseRepoTests`, `RepoTestBase`,
  `TestDataHelpers` (shared by page and area-service tests). Then clean the four GlobalUsings files that
  reference the dying namespaces: `Dal/GlobalUsings.cs:8`, `Services/GlobalUsings.cs:19,20,21`,
  `Tests/GlobalUsings.cs:9,10,11`, `Web/GlobalUsings.cs:7` (H8).
- H5: full validation — build + suite + the Production served-page pass across all six areas, plus
  `grep -rn "IPhoneRepo\|IPhoneDataService\|ICustomerDataService\|ITransactionDataService" src` returning
  nothing outside `plan.md`/`audit.md`.
- Migration end-state DoD: every page depends on exactly one area service injected as `dataService` and
  receives ViewModels; no `IPdfGenerator` in any PageModel; no derived repos or entity DataServices/Api stubs;
  `AddMobileShopRepository` registers only `IBaseRepo<>` → `BaseRepo<>`; area Dal/Api pairs resolve per
  `UseApi` (default false); build, suite and served-page pass all green.
- Risk: HIGH. Confidence: MEDIUM.

---

## Note on the reviewer's write scope
Written by the reviewer into `audit.md` only. `.clinerules/to-do.md` and `.clinerules/chat/plan.md` are
deliberately **not** written: `reviewer.md:13` allows this role to write `audit.md` and to tick a stage in
`to-do.md` at Job B sign-off, and `planner.md:32` keeps `to-do.md` to one-line stages. The planner applies
§A to `to-do.md` and §B to `plan.md`.
