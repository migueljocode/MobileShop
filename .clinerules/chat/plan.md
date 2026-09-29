# Plan — Stage C: Products — ProductsDataService + the five product pages

Written on the owner's explicit instruction (normally the planner's file, per reviewer.md). Stage B is
signed off; this plan covers Stage C only and reuses the decisions locked in Stage A.

## Locked decisions (carried, binding)
L1 Boundaries — pages inject area interfaces only. No EF entity crosses the boundary: requests enter as
   input bind models or primitives, results leave as ViewModels or primitives. Area DataServices may
   inject any `IBaseRepo<T>` they need.
L2 One area service per page, local name `dataService`. No page injects a repo; no two services.
L3 DI ordering — **this stage removes nothing.** The 15 per-entity repo registrations and the 9 entity
   service registrations stay: `Buy`/`Sell` still call `GetSelectableProductsAsync` on
   `IPhoneDataService`/`IAppleIdDataService` (`Buy.cshtml.cs:35-36`, `Sell.cshtml.cs:35-36`) until Stage E.
L4 PDF/invoice ownership — Transactions area. Unchanged here.
L5 Reports owns profit/loss, bounds, distribution. Unchanged here.
L6 Logging — `ILogger<TAreaService>` with `LogInformation` on success and `LogWarning` on failure for
   mutations. Read-only members need no logging. If the primary-constructor `logger` is otherwise unused
   the compiler emits CS9113 — bind it to a `protected ILogger<ProductsDataService> Logger { get; }` property
   exactly as `DataServiceBase.cs:14` does.
L7 Api — `UseApi` stays false; never touch the `src/MobileShop.Api` host. **Any interface member whose
   signature changes in this stage must have its `ApiProductsDataService` body updated in the same step**,
   or the build breaks.
L8 Project constraints — **Apple ID inventory passwords stay plaintext** (`CreateAppleId.cshtml.cs:61`
   stores `Input.Password.Trim()` as-is; never hash it). Application-user passwords are a different path
   and untouched. DatabaseInitializer policy, schema and migrations untouched; no auth; no bin/obj.
L9 Tests — port the coverage for what you move; the repo pattern is `RepoTestBase` + real services over
   `new BaseRepo<T>(Context)`, **not** Moq. Do not delete a test whose subject still exists.

## Scope
Five pages, one new service: `ProductsDataService` absorbs the catalog and creation logic currently split
across `CreatePhoneModel` (191 lines), `CreateAppleIdModel` (82) and the read paths of `Products/Index`,
`Products/Details` and `Products/SecondHand`. Two `.cshtml` files keep working untouched
(`Index`, `Details`, `SecondHand`, `CreateAppleId`); `CreatePhone.cshtml` keeps its markup and JS exactly
as they are — see Step 1 for why the JSON contract must not move.

## Reviewer Briefing
- **This is the largest stage and it is split into five steps on purpose.** The service alone is ~450 lines
  of ported behaviour, and the two create pages carry validation that currently produces field-specific
  `ModelState` errors. Steps 1-2 build the service in two halves; Steps 3-4 migrate pages; Step 5 consolidates
  coverage and validates. Do not merge steps.
- **HIGH risk — the contract additions in Step 1.** Two of Stage A's approved signatures cannot express what
  these pages actually do, and the plan pins the exact fixes rather than leaving them to you. Getting these
  wrong means either the AJAX modals break or field errors silently vanish. Read Step 1's "Contract
  additions" before writing any code.
- **MEDIUM risk — behaviour drift in the ports.** Every projection, predicate and dedupe rule is quoted with
  its source line. Copy them; do not tidy them, and do not add ordering that the originals lack
  (`ModelRepo.GetByManufacturerAsync` has none).
- **LOW risk** — Steps 3 and 4 are mechanical once the service exists.
- **Two things this stage must NOT do:** remove any DI registration (L3), and change `CreatePhone.cshtml`
  or any other `.cshtml`. If you believe a view must change, stop and report instead.

## [ ] Step 1 — Contract additions + `ProductsDataService` (catalog and creation) + Dal registration
- Files: create: src/MobileShop.Models/ViewModels/Web/DropdownCreateResult.cs,
  src/MobileShop.Services/DataServices/Dal/ProductsDataService.cs; modify:
  src/MobileShop.Models/ViewModels/Web/ServiceResult.cs, src/MobileShop.Services/GlobalUsings.cs,
  src/MobileShop.Services/DataServices/Interfaces/IProductsDataService.cs,
  src/MobileShop.Services/DataServices/Api/ApiProductsDataService.cs,
  src/MobileShop.Services/ServiceCollectionExtensions.cs; create: test file (below);
  do not touch: any page, any `.cshtml`, the `if (useApi)` branch, the 9 entity registrations, the 15 repo
  registrations, `PhoneDataService`, `AppleIdDataService`, `ProductDataService`, any entity.
- Symbols: `ServiceResult`; new `DropdownCreateResult`; `IProductsDataService.CreateManufacturerAsync` /
  `CreateModelAsync` / `CreateColorAsync`; `ApiProductsDataService` (same three);
  `ProductsDataService` with ctor `(IBaseRepo<Phone> phones, IBaseRepo<AppleId> appleIds,
  IBaseRepo<Product> products, IBaseRepo<Manufacturer> manufacturers, IBaseRepo<Model> models,
  IBaseRepo<Category> categories, IBaseRepo<Color> colors, IBaseRepo<Guarantee> guarantees,
  ILogger<ProductsDataService> logger)` — **no `SecondHand` repo** (the second-hand profile is a
  `Product` navigation, not a separate write) and **no `Transaction` repo yet** (that arrives in Step 2).
- Current -> Desired: today `CreatePhoneModel` and `CreateAppleIdModel` do this work inline with direct repo
  access. Desired: the same behaviour behind `IProductsDataService`, with pages holding no repo.
- Change — **Contract additions (do these first, they are small and mechanical):**
  1. `ServiceResult` gains two **optional, defaulted** members so the page can put the error on the right
     field and redirect to the new row:
     `public sealed record ServiceResult(bool Succeeded, string? Message, string? ErrorField = null, int? EntityId = null);`
     Positional defaults mean every existing call site still compiles — no interface or Api change needed.
  2. New `public sealed record DropdownCreateResult(bool Succeeded, DropdownOptionViewModel? Option, string? Error, int StatusCode);`
     `StatusCode` is the HTTP status the page must return so the existing JS keeps behaving identically.
  3. In `IProductsDataService`, change exactly three return types —
     `Task<DropdownOptionViewModel> CreateManufacturerAsync(string name)`,
     `CreateModelAsync(int manufacturerId, string name)`, `CreateColorAsync(string name)` — to
     `Task<DropdownCreateResult>`, and update those three bodies in `ApiProductsDataService` to the new type
     (still `throw new NotImplementedException("ApiProductsDataService is not implemented yet.")`).
     Nothing else in either file changes.
  4. Add `global using MobileShop.Models.ViewModels.Web.BindModels;` to
     `src/MobileShop.Services/GlobalUsings.cs` (mirroring `MobileShop.Web/GlobalUsings.cs:13`) and shorten
     the four fully-qualified references in `IProductsDataService` and `ApiProductsDataService`. Leave the
     eight FQNs in the People/Transactions interfaces and stubs for Stages D and E.
- Change — **`ProductsDataService` members (port, do not redesign):**
  5. `GetManufacturersAsync()` / `GetColorsAsync()` = `FindAllAsync()` mapped to
     `new DropdownOptionViewModel(x.Id, x.Name)`. No ordering is applied today — do not add any.
  6. `GetModelsAsync(int manufacturerId)` = `models.FindAllAsync(m => m.ManufacturerId == manufacturerId)`
     (`ModelRepo.cs:6-7` verbatim, no ordering) mapped to `DropdownOptionViewModel`.
  7. `GetGuaranteeCorporationsAsync()` = port `CreatePhone.cshtml.cs:186-189` verbatim:
     `(await guarantees.FindAllAsync()).Select(g => g.Corporation).Distinct(StringComparer.OrdinalIgnoreCase).OrderBy(c => c, StringComparer.Ordinal)`.
     Both comparers matter; keep them.
  8. `CreateManufacturerAsync` — port `CreatePhone.cshtml.cs:29-42`: blank/whitespace -> failure with
     `Error = "Name is required."`, `StatusCode = 400`; trim; dedupe on exact
     `m.Name == trimmed` -> return the **existing** option; else `AddAsync` and return the new option.
  9. `CreateModelAsync` — port `:44-65`: blank -> 400; manufacturer lookup by id, null -> failure
     `"Manufacturer not found."` with `StatusCode = 404`; category lookup `c.Name == "Phone"` and **keep the
     `throw new InvalidOperationException("The 'Phone' category is missing from the catalog seed data.")`**
     (deliberate: missing seed data is reported, never papered over); dedupe on
     `ManufacturerId == manufacturerId && Name == trimmed && CategoryId == category.Id` -> existing option;
     else create with those three fields.
  10. `CreateColorAsync` — port `:147-160`: blank -> 400; trim; dedupe `c.Name == trimmed` -> existing; else create.
  11. `CreatePhoneAsync(CreatePhoneInputModel input)` -> `ServiceResult`, porting `OnPostAsync` (`:67-145`)
      **in this exact order**, each failure returning `ErrorField` = the key the page will pass to
      `ModelState.AddModelError`:
      a. `imei1 = input.IMEI1.Trim()`, duplicate check `phones.AnyAsync(p => p.IMEI1 == imei1)` -> `ErrorField = nameof(CreatePhoneInputModel.IMEI1)`, message `"A phone with this IMEI already exists."`;
      b. manufacturer by id -> `nameof(…ManufacturerId)`, `"Selected manufacturer not found."`;
      c. model by id **and** `model.ManufacturerId == manufacturer.Id` -> `nameof(…ModelId)`, `"Selected model not found for this manufacturer."`;
      d. optional color: `input.ColorId` null -> skip; otherwise find, and a set-but-missing id -> `nameof(…ColorId)`, `"Selected color not found."`;
      e. build `Product` with `ModelId`, `ColorId = color?.Id`, `Barcode = Guid.NewGuid().ToString("N")[..12]`,
         `Price = input.Price`, `SecondHandProfile` when `IsSecondHand` (`TestPeriodDays = input.TestPeriodDays ?? 30`,
         `UsedDurationDays = 0`), `GuaranteeProfile` when `HasGuarantee` (`StartDate = DateTime.Today`,
         `ExpirationDate = input.GuaranteeExpiry ?? DateTime.Today.AddYears(1)`, `Corporation` =
         blank ? `"Shop Warranty"` : trimmed);
      f. build `Phone` with `IMEI1 = imei1`, `IMEI2` null when blank else trimmed, `OwnershipTransferred = false`,
         `ProductNavigation = product`;
      g. `phones.AddAsync(phone)`; **failure means `await phones.AddAsync(phone) <= 0`** — the same test
         `DataServiceBase.cs:87` applies (`Repo.AddAsync(entity) > 0`) with the default `persist: true`, so do
         not invent a different notion of "not saved". On failure:
         `Message = "The phone could not be saved. Check the details and try again."` and `Succeeded = false`
         with a null `ErrorField`; on success `Succeeded = true` and
         `EntityId = phone.Id` (the page redirects with it). `LogInformation` on save, `LogWarning` on failure.
  12. `CreateAppleIdAsync(CreateAppleIdInputModel input)` -> `ServiceResult`, porting `:17-81` in order:
      a. `email = input.Email.Trim()`, duplicate check using the **case-insensitive expression from
         `AppleIdRepo.cs:8-11`** — `x => x.Email.ToLower() == email.ToLower()` — not `x.Email == email`.
         On duplicate: `ErrorField = nameof(CreateAppleIdInputModel.Email)`, `"This Apple ID email already exists."`;
      b. find-or-create `Manufacturer { Name = "Apple" }` (the three constants at `:9-12` move into the service);
      c. category `c.Name == "AppleId"`, **keep** `throw new InvalidOperationException("The 'AppleId' category is missing from the catalog seed data.")`;
      d. find-or-create the single shared implicit `Model` named `"iPhone"` inside that category
         (port the private `AddModelAsync` helper, `:76-81`);
      e. build `Product { ModelId, Barcode = Guid.NewGuid().ToString("N")[..12], Price = input.Price }` — no color, no profiles;
      f. build `AppleId { Email = email, Password = input.Password.Trim(), Notes = blank ? null : trimmed, ProductNavigation = product }`
         — **the password is stored plaintext by design (L8); never hash it, never trim it differently**;
      g. `appleIds.AddAsync(appleId)`; **failure means `await appleIds.AddAsync(appleId) <= 0`**, the same
         `DataServiceBase.cs:87` test as for the phone. On failure
         `Message = "The Apple ID could not be saved."`; on success
         `Succeeded = true`, `EntityId = appleId.Id`.
- Depends on: Stage B (complete).
- Edge cases / error handling: the two `InvalidOperationException` throws are load-bearing — a missing seed
  category must surface, not be auto-created (the original comment says so: "the catalog categories come
  from the seed data - never created here"). Never introduce `StringComparison.OrdinalIgnoreCase` in a query:
  SQLite needs the `ToLower()` form. Keep `Guid.NewGuid().ToString("N")[..12]` verbatim. The client-side
  percent/amount profit JS is untouched and no price math belongs in the service.
- Tests: create `src/MobileShop.Tests/Services/DataServices/Dal/ProductsDataServiceTests.cs` on `RepoTestBase`,
  building the service over `new BaseRepo<T>(Context)`. Seed a `Category { Name = "Phone" }` and
  `{ Name = "AppleId" }` plus manufacturers, as the existing page tests do. Cover: manufacturer/model/color
  dropdowns return `Id`/`Name`; `GetGuaranteeCorporationsAsync` de-dupes case-insensitively and orders
  ordinally; blank names -> failure with 400; unknown manufacturer -> 404; duplicate catalog name returns
  the existing option; the missing-category `InvalidOperationException` for both creates; `CreatePhoneAsync`
  success (barcode length, second-hand defaults, guarantee defaulting to `"Shop Warranty"`), duplicate IMEI,
  unknown manufacturer, model belonging to another manufacturer, unknown color, and `EntityId` on success;
  `CreateAppleIdAsync` success (implicit model created once, **plaintext password persisted verbatim**),
  case-insensitive email duplicate, and the missing-category throw.
- Verify: dotnet build src/MobileShop.slnx --nologo && dotnet test src/MobileShop.slnx --nologo --no-build
- Done when: the three return types and both records exist, the nine members compile with the quoted
  behaviour, the Dal line is added, the four Products FQNs are shortened, and no page or view changed.
- Risk: HIGH. Confidence: HIGH.

## [ ] Step 2 — `ProductsDataService` read members
- Files: create: src/MobileShop.Services/DataServices/Shared/ProductTransactionProjection.cs; modify:
  src/MobileShop.Services/DataServices/Dal/ProductsDataService.cs (add `IBaseRepo<Transaction> transactions`
  to the ctor, the three public members, and the private projection wrapper),
  **src/MobileShop.Services/DataServices/Interfaces/IProductsDataService.cs** and
  **src/MobileShop.Services/DataServices/Api/ApiProductsDataService.cs** (the `GetInventoryRowsAsync`
  signature change in change 2, and the matching Api body per L7),
  src/MobileShop.Tests/Services/DataServices/Dal/ProductsDataServiceTests.cs (**its constructor changes in
  this step** — Step 1 builds the service with nine repos and this step adds the tenth, so the Step 1 test
  file will not compile until it is updated here); do not touch: pages, views, DI,
  entity services, `TransactionDataService` (it keeps its own copy until Stage E).
- Symbols: `IProductsDataService.GetInventoryRowsAsync` (+ its `ApiProductsDataService` body);
  `GetSecondHandRowsAsync`, `GetDetailsAsync`; private
  `GetProductTransactionsAsync`; new shared `ProductTransactionProjection`; new
  `ProductDetailsViewModel.Transactions`.
- Current -> Desired: the list rows come from `PhoneDataService.GetInventoryRowsAsync` (`:25`),
  `AppleIdDataService.GetInventoryRowsAsync` (`:25`) and the second-hand rows from their `:90` counterparts;
  details from `PhoneDataService.GetDetailsAsync` (`:178-200`) and `AppleIdDataService.GetDetailsAsync` (`:165`);
  the transaction rows from `TransactionDataService.GetProductTransactionsAsync` (`:267-282`). Desired: all
  three members on the area service, with the transaction rows riding along on the details object.
- Change:
  1. Add `IReadOnlyList<ProductTransactionViewModel> Transactions` as the **last, defaulted** positional
     member of `ProductDetailsViewModel` (`… bool IsSecondHand, IReadOnlyList<ProductTransactionViewModel> Transactions = []`).
     A default keeps every existing construction site compiling, including in the two entity services and in
     `PhoneDataServiceTests` / `AppleIdDataServiceTests` — those must keep passing while they still exist.
  2. `GetInventoryRowsAsync` **gains an optional `string? type = null` parameter — do this in this step, not
     later**, because the contract and the service change together here:
     `Task<IReadOnlyList<ProductListItemViewModel>> GetInventoryRowsAsync(string? type = null)` in
     `IProductsDataService` (defaulted, so no other implementation breaks) and the matching
     `ApiProductsDataService` body, which keeps throwing `NotImplementedException` (L7).
     Semantics — `type` is the page's already-normalised lowercase value: `null` or `"all"` returns both
     blocks, `"phone"` phones only, `"appleid"` Apple IDs only, and any unrecognised value returns both
     blocks (defensive; the page only ever passes those three). Build each requested block with
     `phones.SelectAllAsync(<phone projection>)` / `appleIds.SelectAllAsync(<apple-id projection>)`, both
     copied **verbatim** from `PhoneDataService.cs:25`
     and `AppleIdDataService.cs:25` (they build `ProductListItemViewModel`, 8 members). "Verbatim" includes
     three easy-to-normalise details: each projection ends with `.OrderBy(row => row.ProductId)`, the `Type`
     literals are `"Phone"` and `"Apple ID"` (with the space), and the `Identifier` is `"IMEI: " + IMEI1` for
     phones but the raw `appleId.Email` for Apple IDs. The `"all"` result is therefore the phone block ordered
     by ProductId followed by the Apple-ID block ordered by ProductId — the same `rows.AddRange(...)` order as
     `Products/Index.cshtml.cs:16-19`. Do not merge or re-sort the two blocks, and do not filter inside a
     projection.
  3. `GetSecondHandRowsAsync()` = the same two `:90` projections concatenated. **The page keeps the
     `OrderBy(product => product.Name)`** — do not move it into the service and do not add it to the inventory path.
  4. `GetDetailsAsync(int id, string type)` = the phone branch is
     `phones.SelectAsync(id, <projection>)` ported from `PhoneDataService.cs:178-200` (including the
     `"IMEI: "` / `"Not sold"` / `"None"` label logic and the nested sell-transaction owner lookup), the
     apple-id branch is `appleIds.SelectAsync(id, <projection>)` from `AppleIdDataService.cs:165`, and
     `type` is matched case-insensitively for `"appleid"` exactly as `Details.cshtml.cs:13` does.
     **Do not `await` inside the projection** — an expression tree cannot contain `await`, so the rows cannot
     be fetched inside the selector. Use three phases: (1) run the projection unchanged, without
     `Transactions`; (2) if it returned `null`, return `null` immediately; (3) otherwise
     `var rows = await GetProductTransactionsAsync(result.ProductId); return result with { Transactions = rows };`
     `ProductDetailsViewModel` is a positional record, so `with` is the copy mechanism.
  5. **Shared projection (created in this stage, for Stage E's benefit).** Create
     `src/MobileShop.Services/DataServices/Shared/ProductTransactionProjection.cs` with one static
     `Expression<Func<Transaction, ProductTransactionViewModel>> Selector`, body copied verbatim from
     `TransactionDataService.cs:271-280` (the `PersonNavigation == null ? "Shop"` fallbacks included). The body
     must stay a pure expression — property access, the source's ternary null-check
     (`PersonNavigation == null ? "Shop" : …`) and string concatenation only — so it
     converts to an expression tree. Then a private `GetProductTransactionsAsync(int productId)`:
     `(await transactions.SelectAllAsync(t => t.ProductId == productId, ProductTransactionProjection.Selector)).OrderByDescending(item => item.Date).ToList()`.
     Why here: Stage E's `TransactionsDataService` needs the identical projection, and the architecture
     decision approved for this migration places shared pure projections in `DataServices/Shared/` — no
     repos, no `DbContext`. The consumer adds
     `using MobileShop.Services.DataServices.Shared;` at the top of the service file (a needed using, not a
     redundant one; do not put it in GlobalUsings for two consumers). Once nothing calls it,
     `TransactionDataService.GetProductTransactionsAsync` goes in Stage E.
- Depends on: Step 1.
- Edge cases / error handling: `type` is user-supplied query input — match `"appleid"` case-insensitively and
  treat everything else as phone, as today. A missing id returns `null` and the page answers 404. The
  details projections navigate `ProductNavigation`, so an id belonging to another entity type must still
  behave as today (the projections are per-type lookups, not cross-type).
- Tests: extend `ProductsDataServiceTests` with: inventory rows contain both entity types with correct
  `Type`/`Identifier`; second-hand rows contain only second-hand items; `GetDetailsAsync` for each type
  returns the right labels (`"Not sold"`, `"None"`, the IMEI variants) and carries the transaction rows in
  descending date order with `"Shop"` fallbacks; an unknown id returns `null`; an uppercase `"AppleId"`
  still routes to the Apple-ID branch. And for the new `type` parameter: `null` and `"all"` return both
  blocks, `"phone"` phones only, `"appleid"` Apple IDs only, each block still ordered by ProductId, and an
  unrecognised value returns both blocks.
- Verify: dotnet build src/MobileShop.slnx --nologo && dotnet test src/MobileShop.slnx --nologo --no-build
- Done when: the three members behave identically to the entity services they replace,
  `GetInventoryRowsAsync` filters by `type`, the existing
  `ProductDetailsViewModel` construction sites still compile, the shared projection exists, and the only
  files touched are the ones this step lists.
- Risk: MEDIUM. Confidence: HIGH.

## [ ] Step 3 — Migrate `CreatePhone` and `CreateAppleId`
- Files: modify: src/MobileShop.Web/Pages/Products/CreatePhone.cshtml.cs,
  src/MobileShop.Web/Pages/Products/CreateAppleId.cshtml.cs, and their test files
  src/MobileShop.Tests/Web/Pages/Products/CreatePhoneModelTests.cs (359 lines) and
  CreateAppleIdModelTests.cs (139 lines); do not touch: **any `.cshtml`**, any service, any repo.
- Symbols: `CreatePhoneModel` (ctor, `Manufacturers`/`Models`/`Colors`/`Corporations`, `OnGetAsync`,
  `OnGetModelsAsync`, `OnPostCreateManufacturerAsync`, `OnPostCreateModelAsync`, `OnPostCreateColorAsync`,
  `OnPostCreateCorporationAsync`, `OnPostAsync`, `PopulateDropdownsAsync`);
  `CreateAppleIdModel` (ctor, `OnPostAsync`).
- Current -> Desired: six dependencies (one entity service + five repos) and all the logic inline. Desired:
  `(IProductsDataService dataService)` only, with the page holding ViewModels.
- Change — `CreatePhone.cshtml.cs`:
  1. Ctor becomes `(IProductsDataService dataService)`; the five repo parameters and the
     `IPhoneDataService` parameter are deleted.
  2. Page properties become `IReadOnlyList<DropdownOptionViewModel> Manufacturers`, same for `Models` and
     `Colors`, and `IReadOnlyList<string> Corporations` (unchanged type). **Because `DropdownOptionViewModel`
     exposes `Id` and `Name`, the four `<select asp-items="@(new SelectList(Model.X, "Id", "Name"))">` in
     `CreatePhone.cshtml:15,26,61,104` keep working with no view edit.**
  3. `PopulateDropdownsAsync()` = `GetManufacturersAsync()`, `GetModelsAsync(Input.ManufacturerId)` when
     `> 0` else `[]`, `GetColorsAsync()`, `GetGuaranteeCorporationsAsync()`.
  4. `OnGetModelsAsync` keeps returning `new JsonResult(options.Select(o => new { o.Id, o.Name }))` — the JS
     at `CreatePhone.cshtml:252-260` reads `m.id` / `m.name`, so the anonymous property names must stay
     exactly `Id` and `Name`.
  5. The three modal handlers keep their URL-visible names and their HTTP semantics:
     `OnPostCreateManufacturerAsync(string name)`, `OnPostCreateModelAsync(int manufacturerId, string name)`,
     `OnPostCreateColorAsync(string name)`. Each calls its service member and returns
     `new JsonResult(new { id = result.Option.Id, name = result.Option.Name })` on success, or
     `new JsonResult(new { error = result.Error }) { StatusCode = result.StatusCode }` on failure. The
     service now owns the blank-name and manufacturer-not-found checks, so the page's own `IsNullOrWhiteSpace`
     guards may go — but the returned status codes must stay 400/404 as the JS's `response.ok` checks expect.
  6. `OnPostCreateCorporationAsync` stays **exactly as it is** (`:162-177`): it is a page-local,
     in-memory list operation with no persistence — the value is persisted only on the next phone POST. It
     keeps reading `Corporations` and returning `{ name }`.
  7. `OnPostAsync` = `if (!ModelState.IsValid) { await PopulateDropdownsAsync(); return Page(); }` then
     `var result = await dataService.CreatePhoneAsync(Input);` then
     `if (!result.Succeeded) { if (result.ErrorField is not null) ModelState.AddModelError(result.ErrorField, result.Message); else Message = result.Message; await PopulateDropdownsAsync(); return Page(); }`
     and on success `return RedirectToPage("/Products/Details", new { id = result.EntityId, type = "phone" });`.
     `Message` and both page property types stay as they are.
- Change — `CreateAppleId.cshtml.cs`: ctor becomes `(IProductsDataService dataService)`; `OnPostAsync`
  becomes the same four steps (validate -> `CreateAppleIdAsync(Input)` -> field error or `Message` ->
  `RedirectToPage("/Products/Details", new { id = result.EntityId, type = "appleid" })`). The three constants
  (`:9-12`) and the `AddModelAsync` helper move to the service in Step 1 and leave this file.
- Depends on: Steps 1-2.
- Edge cases / error handling: the page must not re-derive validation the service now owns, and must not
  swallow a service failure — every non-success path ends in a `ModelState` error or the `Message` alert,
  both of which `CreatePhone.cshtml:124-126` already renders. `AddModelError` keys must be the
  `nameof(Input.X)` strings the service returns so the existing validation spans light up.
- Tests: rewrite the **constructors** in both page test files to pass a real `ProductsDataService` over
  `new BaseRepo<T>(Context)` (not Moq) and **keep every existing assertion** — they already cover the
  cascading dropdowns, the three modal handlers, the JSON payloads and the POST paths. Add only what the move
  makes newly observable: a `ModelState` error for a duplicate IMEI and one for an unknown manufacturer
  (assert the `AddModelError` key), and that a successful POST redirects with the new id.
- Verify: dotnet build src/MobileShop.slnx --nologo && dotnet test src/MobileShop.slnx --nologo --no-build
- Done when: both pages inject only `dataService`, no repo is referenced, no `.cshtml` changed, and the
  page tests still cover the handlers they covered before.
- Risk: HIGH. Confidence: HIGH.

## [ ] Step 4 — Migrate `Products/Index`, `Products/Details`, `Products/SecondHand`
- Files: modify: src/MobileShop.Web/Pages/Products/Index.cshtml.cs, Details.cshtml.cs, SecondHand.cshtml.cs;
  do not touch: any `.cshtml`, any service, any repo, any test.
- Symbols: `IndexModel` (ctor, `OnGetAsync(string? type)`), `DetailsModel` (ctor, `OnGetAsync(int id, string? type)`),
  `SecondHandModel` (ctor, `OnGetAsync`).
- Current -> Desired: three pages, two entity services each (`Details` also has `ITransactionDataService`).
  Desired: one area service each, no second service, no view change.
- Change:
  1. `IndexModel` ctor -> `(IProductsDataService dataService)`. `OnGetAsync` keeps the `Type` normalisation
     (`string.IsNullOrWhiteSpace(type) ? "all" : type.ToLowerInvariant()`) and then calls
     `Products = await dataService.GetInventoryRowsAsync(Type);` — one call for every case. The optional
     `type` parameter and its filter semantics were added to the interface and the service in **Step 2**;
     this step only passes the already-normalised value. No contract change, no service edit, no test edit
     in this step.
  2. `DetailsModel` ctor -> `(IProductsDataService dataService)`. `OnGetAsync` becomes
     `Product = await dataService.GetDetailsAsync(id, type ?? string.Empty); if (Product is null) return NotFound();`
     `Transactions = Product.Transactions;` — the `ITransactionDataService` parameter disappears because the
     rows now ride on the details object (Step 1's `ServiceResult` additions and Step 2's
     `ProductDetailsViewModel.Transactions`). Both
     property types stay identical, so `Details.cshtml` is untouched.
  3. `SecondHandModel` ctor -> `(IProductsDataService dataService)`; `OnGetAsync` =
     `(await dataService.GetSecondHandRowsAsync()).OrderBy(product => product.Name).ToList()`.
- Depends on: Step 2 (and the Step 1 contracts).
- Edge cases / error handling: `Details` must keep answering `NotFound()` for a missing product, and the
  case-insensitive `"appleid"` routing must survive. `Index` must keep ignoring an unknown `type` by falling
  back to `"all"`.
- Tests: none in this step — the pages are thin pass-throughs and `ProductsDataServiceTests` already covers
  the behaviour behind them. The Stage C served-page pass in Step 5 is the end-to-end evidence.
- Verify: dotnet build src/MobileShop.slnx --nologo && dotnet test src/MobileShop.slnx --nologo --no-build
- Done when: all five product pages inject exactly one area service, no page references a repo, no view
  changed, and `git status` shows no `.cshtml` diff.
- Risk: LOW. Confidence: HIGH.

## [ ] Step 5 — Coverage consolidation and Stage C validation
- Files: modify: src/MobileShop.Tests/Services/DataServices/Dal/PhoneDataServiceTests.cs and
  AppleIdDataServiceTests.cs (only to remove assertions that are now duplicated and strictly weaker);
  create: none; delete: none.
- Symbols: the coverage behind `IPhoneDataService` / `IAppleIdDataService`; Stage C's DoD.
- Current -> Desired: after this stage the Products pages no longer use `IPhoneDataService` or
  `IAppleIdDataService`, but `Buy`/`Sell` still do. Desired: no test asserts the same behaviour twice through
  a service that no page uses, and no coverage is simply lost.
- Change:
  1. For each member whose only consumer was a Products page, confirm the equivalent assertion now exists in
     `ProductsDataServiceTests`; if it does, drop the older weaker duplicate (e.g. an
     `Assert.True(_service.Quantity() >= 0)`-shaped line). **Keep every test whose subject still has a
     consumer** — `GetSelectableProductsAsync` and everything `TransactionDataService` uses must stay
     untouched until Stage H, and this step must not delete a `[Fact]`, only redundant assertion lines.
  2. Run the full validation below.
- Depends on: Steps 1-4.
- Edge cases / error handling: if you cannot show that a dropped assertion is reproduced in
  `ProductsDataServiceTests`, keep it. Coverage loss is a finding, not a cleanup.
- Verify (chain 1): dotnet build src/MobileShop.slnx --nologo && dotnet test src/MobileShop.slnx --nologo --no-build
- Verify (chain 2, guard): `--no-launch-profile` is MANDATORY — every profile in
  `src/MobileShop.Web/Properties/launchSettings.json` sets `ASPNETCORE_ENVIRONMENT=Development`, and without
  it `dotnet run` runs the destructive `DatabaseInitializer.InitializeForDevelopment` (EnsureDeleted/EnsureCreated/seed).
  Confirm the log says `Hosting environment: Production` BEFORE any route check; if it says Development, stop.
- Verify (chain 2): ASPNETCORE_ENVIRONMENT=Production dotnet run --project src/MobileShop.Web
  --no-launch-profile --urls http://localhost:5199 (attached; read until it listens), then:
  for p in / /Products /Products/SecondHand /Products/CreatePhone /Products/CreateAppleId /People/Customers
  /People/Sellers /Transactions /Reports/ProfitLoss /Account/Login; do printf '%s %s\n' "$(curl -s -o
  /dev/null -w '%{http_code}' http://localhost:5199$p)" "$p"; done
  The two create pages must return 200 **and** render their four `<select>` elements — check the response
  body for `manufacturerSelect`, `modelSelect`, `colorSelect` and `corporationSelect`, because a green status
  code alone would not prove the dropdowns still bind.
  Fallback: (cd src/MobileShop.Web/bin/Debug/net10.0 && ASPNETCORE_ENVIRONMENT=Production dotnet MobileShop.Web.dll --urls http://localhost:5199)
- Verify (non-destructiveness — **do not reuse the old `stat` check**): it is unsound for a WAL-backed
  database, because any clean Production pass checkpoints the WAL and legitimately changes size and mtime.
  Instead record, before and after the pass:
  (a) the guard line and the absence of `EnsureDeleted`/`EnsureCreated`/seed activity in the log, and
  (b) a row-count fingerprint: `sqlite3 "file:MobileShop.db?mode=ro" "select (select count(*) from Products), (select count(*) from Phones), (select count(*) from Transactions);"`
  Both must match. (`sqlite3` is installed; `mode=ro` guarantees no writes.)
- Done when: chain 1 exit 0; guard line seen; all ten routes 200 with the four select ids present on
  `/Products/CreatePhone`; the log shows no initializer activity; the row-count fingerprint is unchanged.
- Risk: MEDIUM. Confidence: HIGH.

## Global Definition of Done (Stage C)
- All five product pages inject exactly one dependency, `IProductsDataService dataService`; **no page in the
  solution injects a repo**; no page uses two data services.
- No page or service returns an EF entity: every `IProductsDataService` signature uses ViewModels,
  `DropdownOptionViewModel`, `ServiceResult`, `DropdownCreateResult` or primitives.
- `ProductsDataService` is registered in the Dal branch of `AddMobileShopDataServices`; `UseApi` still false;
  `ApiProductsDataService` still `NotImplementedException` throughout.
- No DI registration was removed — `Buy`/`Sell` still resolve `GetSelectableProductsAsync` (L3).
- The three contract additions exist exactly as specified: `ServiceResult.ErrorField`/`.EntityId` (defaulted),
  `DropdownCreateResult`, `ProductDetailsViewModel.Transactions` (defaulted), plus three narrowed return
  types (the modal creates) and one added optional parameter (`GetInventoryRowsAsync`'s `type`).
- Behaviour is identical: IMEI/email duplicate rules, the model-belongs-to-manufacturer check, the
  `"Shop Warranty"` and second-hand defaults, the case-insensitive corporation de-dupe with ordinal ordering,
  the case-insensitive Apple-ID email lookup via `ToLower()`, the `Phone`/`AppleId` seed-category throws, the
  details labels, the second-hand `OrderBy(Name)`, and the `{id,name}` / `{name}` JSON payloads.
- **No `.cshtml` file changed** — the four `<select>`s and all five modals still bind and still post to the
  same handler names.
- Apple ID inventory passwords are still stored plaintext; application-user hashing is untouched.
- `dotnet build src/MobileShop.slnx --nologo` and `dotnet test src/MobileShop.slnx --nologo --no-build`
  succeed; the Production served-page pass is 200 on all ten routes; the row-count fingerprint and the log
  both show no initializer activity.

## Carried notes — NOT Stage C work
- `IEmployeeDataService` is missing from the `if (useApi)` branch and no `ApiEmployeeDataService` exists
  (pre-existing since `1450979`, inert while `UseApi` is false) — Stage H.
- `IsSold`, `IsSecondHand`, `GetOwner`, `GetGuarantee`, `GetSecondHandInfo`, `GetAvailableSecondHandRows*` and
  the `Quantity*` members of the phone/Apple-ID entity services lose their page consumers in this stage, but
  **the services and their tests stay** — Stage H deletes them. Do not "tidy" them here.
- The product-transaction projection now lives in `DataServices/Shared/ProductTransactionProjection.cs`;
  Stage E must consume it rather than re-porting it, and
  `TransactionDataService.GetProductTransactionsAsync` loses its last page caller in this stage.
- The eight remaining fully-qualified `BindModels` references (People/Transactions interfaces and their Api
  stubs) are shortened in Stages D and E; the `global using` line added in Step 1 already covers them.
- `RepoTestBase`, `TestDataHelpers` and `BaseRepoTests` stay for Stage H; the 14 `*RepoTests` and the 15
  derived repos are Stage H's deletion list.

## Execution notes
- Chain dependent commands with `&&`; group independent read-only checks in one `{ ...; }` call; keep
  verification output and the act.md report short.
- One step per commit. Tick the step header exactly as `## ~~[x] Step N — <title>~~` and commit the touched
  files plus this plan. Never use `git add -A`; never stage unrelated pre-existing changes.
- The full suite exceeds the interactive window (~44s): use an attached "proceed while running" session or a
  log redirect you read to completion, and only then judge the result and commit.
- Report honestly: if a step cannot be completed as written, or if the repository contradicts this plan, stop
  and report rather than improvising an architecture decision.
