# Plan — Stage J — Api layer cleanup

## Reviewer Briefing

- Stage J intentionally lifts the Stage H restriction on the unused API-backed entity-service stubs.
- Repository inspection shows the eight entity-service interfaces are referenced only by their matching API stubs and `ServiceCollectionExtensions.cs`; no Web, Tests, or other production consumers were found.
- `IDataService<T>` is used only by those eight interfaces and `ApiDataServiceBase<T>`; after those are removed, neither has a remaining source consumer.
- The six surviving API registrations are `IHomeDataService`, `IProductsDataService`, `IPeopleDataService`, `ITransactionsDataService`, `IReportsDataService`, and `IAccountDataService`.
- `src/MobileShop.Api` remains strictly out of scope.
- Step 2 is the highest-risk step because it removes a shared-looking abstraction and eight concrete types; repository-wide reference verification is mandatory.

## ~~[x] Step 1 — Remove unused entity-service API registrations~~

- Files: inspect/modify `src/MobileShop.Services/ServiceCollectionExtensions.cs` only.
- Symbols: `AddMobileShopDataServices(bool useApi)`; the eight entity registrations for User, Customer, Seller, Transaction, Product, Invoice, Phone, and AppleId.
- Current -> Desired: the `useApi` branch contains exactly:
  - `IHomeDataService -> ApiHomeDataService`
  - `IProductsDataService -> ApiProductsDataService`
  - `IPeopleDataService -> ApiPeopleDataService`
  - `ITransactionsDataService -> ApiTransactionsDataService`
  - `IReportsDataService -> ApiReportsDataService`
  - `IAccountDataService -> ApiAccountDataService`
- Change: registration cleanup only. Keep the unused types until Step 2.
- Depends on: Stage I PASS.
- Preserve: `UseApi` configuration, default DAL path, all non-API registrations, and all project-specific constraints.
- Do not touch: `src/MobileShop.Api`, database initialization, authentication, PDF configuration, or unrelated code.
- Verify: `dotnet build src/MobileShop.slnx --nologo && dotnet test src/MobileShop.slnx --nologo --no-build`.
- Done when: build/tests pass, exactly six API area registrations remain, and only this file changed.
- Risk: MEDIUM
- Confidence: HIGH

## ~~[x] Step 2 — Remove unused entity API surface~~

- Files to remove:
  - `src/MobileShop.Services/DataServices/Api/ApiUserDataService.cs`
  - `src/MobileShop.Services/DataServices/Api/ApiCustomerDataService.cs`
  - `src/MobileShop.Services/DataServices/Api/ApiSellerDataService.cs`
  - `src/MobileShop.Services/DataServices/Api/ApiTransactionDataService.cs`
  - `src/MobileShop.Services/DataServices/Api/ApiProductDataService.cs`
  - `src/MobileShop.Services/DataServices/Api/ApiInvoiceDataService.cs`
  - `src/MobileShop.Services/DataServices/Api/ApiPhoneDataService.cs`
  - `src/MobileShop.Services/DataServices/Api/ApiAppleIdDataService.cs`
  - `src/MobileShop.Services/DataServices/Interfaces/IUserDataService.cs`
  - `src/MobileShop.Services/DataServices/Interfaces/ICustomerDataService.cs`
  - `src/MobileShop.Services/DataServices/Interfaces/ISellerDataService.cs`
  - `src/MobileShop.Services/DataServices/Interfaces/ITransactionDataService.cs`
  - `src/MobileShop.Services/DataServices/Interfaces/IProductDataService.cs`
  - `src/MobileShop.Services/DataServices/Interfaces/IInvoiceDataService.cs`
  - `src/MobileShop.Services/DataServices/Interfaces/IPhoneDataService.cs`
  - `src/MobileShop.Services/DataServices/Interfaces/IAppleIdDataService.cs`
  - `src/MobileShop.Services/DataServices/Interfaces/Base/IDataService.cs`
  - `src/MobileShop.Services/DataServices/Api/Base/ApiDataServiceBase.cs`
- Symbols: the eight entity API contracts/stubs, `IDataService<T>`, and `ApiDataServiceBase<T>`.
- Current -> Desired: those unused types/files are gone; the six surviving area API services remain.
- Change: remove only the now-unreferenced API entity-service layer.
- Depends on: Step 1 Job B PASS.
- Before removal: search all `src` source/test files for every deleted type and `IDataService<`; no source/test reference may remain.
- Do not create compatibility types or replacement abstractions.
- Do not touch `src/MobileShop.Api` or the six surviving area API services.
- Verify: repository-wide stale-reference search, then `dotnet build src/MobileShop.slnx --nologo && dotnet test src/MobileShop.slnx --nologo --no-build`.
- Done when: all listed files are gone, no stale source/test references remain, the six area API registrations remain, and full validation passes.
- Risk: HIGH
- Confidence: HIGH

## Global Definition of Done

- `useApi` registers exactly the six surviving area services.
- Eight unused entity API interfaces and eight matching API stub classes are gone.
- `IDataService<T>` and `ApiDataServiceBase<T>` are gone because repository inspection confirms no remaining source/test consumers.
- Six surviving area API services remain intact.
- `src/MobileShop.Api` is untouched.
- No database/schema/migration, initialization, authentication, PDF, package, or unrelated behavior changes.
- Final validation passes: `dotnet build src/MobileShop.slnx --nologo && dotnet test src/MobileShop.slnx --nologo --no-build`.
- No stale source/test references to the removed API entity-service types remain.
- Stage J is signed off only after Step 2 Job B PASS.

## Execution notes

- Implement exactly one step, verify it, commit it, and **STOP for Job B** before the next step.
- Report the exact commit SHA, verification, limitations, friction, problems, and status in `.clinerules/chat/act.md`.
- Actor must not edit `.clinerules/to-do.md`; reviewer ticks the stage only after final Job B PASS.
- Reviewer owns `.clinerules/chat/audit.md`.
