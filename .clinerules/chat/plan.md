# Plan — Stage H: Cleanup — delete entity services/repos/tests, GlobalUsings, final validation

Stage G is signed off. This file plans **Stage H only**, the final unchecked stage in `.clinerules/to-do.md`.

## Process (binding)

- **One step → commit → Job B review → next step.**
- Only the **reviewer** ticks Stage H in `.clinerules/to-do.md` after the final validation PASS.
- This stage is cleanup only: preserve the area-service architecture and all current behavior/contracts unless explicitly required by the cleanup.

## Locked decisions

L1 **API is untouched.** Do not modify `src/MobileShop.Api`, API stubs, API configuration, or API endpoints.

L2 **Keep API-facing entity-service interfaces/stubs that are still required by the API branch.** In particular, `IUserDataService`, `ICustomerDataService`, `ISellerDataService`, `ITransactionDataService`, `IProductDataService`, `IInvoiceDataService`, `IPhoneDataService`, and `IAppleIdDataService` remain because their API implementations depend on them. Do not delete these contracts merely because the DAL implementations disappear.

L3 **Delete obsolete DAL entity services.** The area services are now the application-facing contracts for Home, Products, People, Transactions, Reports, and Account. The old entity-oriented DAL services are no longer part of the normal DAL path.

L4 **Generic repository is the surviving DAL abstraction.** After this stage, surviving Services area code should depend on `IBaseRepo<T>`, not specialized per-entity repository interfaces.

L5 **No database/schema/migration changes.** Do not modify entities, EF configurations, migrations, sample data, or initialization policy.

L6 **No behavioral cleanup.** Preserve current page behavior, messages, PDF behavior, reporting behavior, password hashing, Apple ID plaintext passwords, and development-only admin initialization.

L7 **Do not delete shared test infrastructure.** `RepoTestBase`, `BaseRepoTests`, and other helpers remain when still referenced by area-service/page tests. Delete only tests whose production target is being removed.

L8 **GlobalUsings are cleaned by evidence, not preference.** Remove only imports proven unused after the deletions; keep project-wide usings that remain required.

## Current repository facts

- Surviving area services: `AccountDataService`, `HomeDataService`, `ProductsDataService`, `PeopleDataService`, `TransactionsDataService`, and `ReportsDataService`.
- `AccountDataService` still uses `IUserRepo`; it must move to `IBaseRepo<User>` before specialized repository deletion.
- `ReportsDataService` still uses `ITransactionRepo` and `IEmployeeRepo`; it must move to generic repository projections before specialized repository deletion.
- The other area services already use `IBaseRepo<T>`.
- The obsolete DAL entity services currently present are:
  - `UserDataService`
  - `CustomerDataService`
  - `SellerDataService`
  - `TransactionDataService`
  - `ProductDataService`
  - `InvoiceDataService`
  - `PhoneDataService`
  - `AppleIdDataService`
  - `EmployeeDataService`
- `IEmployeeDataService` has no API implementation/registration and no surviving application consumer; it is an orphaned entity-service contract and is eligible for deletion with its obsolete implementation.
- `DataServiceBase<TService,TEntity>` is used by the obsolete DAL entity-service implementations and becomes removable once those implementations are gone.
- Specialized repository implementations/interfaces are still registered centrally even though their consumers are being migrated/deleted. Their tests live under `src/MobileShop.Tests/Dal/Repos`.
- The API data-service base `ApiDataServiceBase<T>` is separate and must remain.

## Reviewer Briefing

- **HIGH risk:** deleting specialized repositories and old entity services is a broad structural deletion. The actor must prove there are no remaining production consumers before each deletion.
- **MEDIUM risk:** `ReportsDataService` currently relies on specialized repository behavior for earliest-transaction and active-employee queries; the generic-repository replacement must preserve filtering, ordering, and employee-name semantics.
- **MEDIUM risk:** `AccountDataService` must preserve credential behavior while replacing `IUserRepo` with `IBaseRepo<User>`.
- **MEDIUM risk:** GlobalUsings cleanup can create compile-only failures if an import is removed prematurely; perform it after the code deletions and use the build as the final proof.
- **HIGH confidence:** the existing area services already demonstrate the intended `IBaseRepo<T>` pattern, and current searches show the remaining specialized production consumers are concentrated in Account, Reports, and the obsolete entity services.

## ~~[x] Step 1 — Remove specialized-repo dependencies from surviving area services~~

- **Files:** inspect/modify `src/MobileShop.Services/DataServices/Dal/AccountDataService.cs`, `src/MobileShop.Services/DataServices/Dal/ReportsDataService.cs`, `src/MobileShop.Tests/Services/DataServices/Dal/AccountDataServiceTests.cs`, `src/MobileShop.Tests/Services/DataServices/Dal/ReportsDataServiceTests.cs`; do not touch `src/MobileShop.Api`.
- **Symbols:** `AccountDataService`, `EnsureAdminUser()`, `GetAdminUsernameAsync()`, `ValidateCredentialsAsync()`, `ChangePasswordAsync()`, `ReportsDataService.GetEarliestTransactionDateAsync()`, `GetDistributionRowsAsync()`.
- **Current → Desired:** `IUserRepo` → `IBaseRepo<User>`; `ITransactionRepo` → `IBaseRepo<Transaction>`; `IEmployeeRepo` → `IBaseRepo<Employee>`.
- **Change:** replace Account username/password lookups and updates with generic repository predicates/projections and normal entity updates. Preserve admin constants, Argon2 hashing/verification, logging, null behavior, and exception behavior. Replace Reports' specialized earliest-date query with the generic ordered projection capability. Replace the active-employee query with a generic projection that preserves active filtering and supplies employee id plus person first/last-name data required by the existing distribution calculation. Do not alter report rules or distribution percentages/messages.
- **Depends on:** none.
- **Edge cases:** missing admin still throws the same `InvalidOperationException`; credential failures remain false; earliest date remains null for no non-deleted transactions; distribution still rejects missing/duplicate/inactive required employees.
- **Tests:** retarget Account/Reports tests from specialized repos to `BaseRepo<T>`; preserve assertions; add focused regression only if the generic projection exposes a behavior gap.
- **Verify:** targeted Account/Reports tests, then `dotnet build src/MobileShop.slnx --nologo && dotnet test src/MobileShop.slnx --nologo --no-build`.
- **Done when:** no surviving production file outside obsolete entity-service implementations references `IUserRepo`, `ITransactionRepo`, or `IEmployeeRepo`.
- **Risk:** MEDIUM
- **Confidence:** MEDIUM

## ~~[x] Step 2 — Delete obsolete DAL entity services and their tests~~

- **Files:** delete `UserDataService.cs`, `CustomerDataService.cs`, `SellerDataService.cs`, `TransactionDataService.cs`, `ProductDataService.cs`, `InvoiceDataService.cs`, `PhoneDataService.cs`, `AppleIdDataService.cs`, `EmployeeDataService.cs` under `src/MobileShop.Services/DataServices/Dal/`; delete `src/MobileShop.Services/DataServices/Dal/Base/DataServiceBase.cs` after confirming no consumer remains.
- Delete the matching obsolete DAL service tests under `src/MobileShop.Tests/Services/DataServices/Dal/`: `UserDataServiceTests.cs`, `CustomerDataServiceTests.cs`, `SellerDataServiceTests.cs`, `TransactionDataServiceTests.cs`, `ProductDataServiceTests.cs`, `InvoiceDataServiceTests.cs`, `PhoneDataServiceTests.cs`, `AppleIdDataServiceTests.cs`, and `EmployeeDataServiceTests.cs` if present.
- Delete `src/MobileShop.Services/DataServices/Interfaces/IEmployeeDataService.cs`; it has no API implementation/registration or surviving application consumer.
- Modify `src/MobileShop.Services/ServiceCollectionExtensions.cs` to remove only the corresponding **DAL** registrations.
- **Do not remove API-branch registrations or API stub files.**
- **Symbols:** `AddMobileShopDataServices(bool useApi)` and the obsolete entity-service registrations.
- **Depends on:** Step 1.
- **Edge cases:** keep `IDataService<T>` and `ApiDataServiceBase<T>` for the API path; do not delete area services or their tests; do not delete API-required entity-service interfaces.
- **Verify:** search for every deleted service type and `DataServiceBase<`, then `dotnet build src/MobileShop.slnx --nologo && dotnet test src/MobileShop.slnx --nologo --no-build`.
- **Done when:** no obsolete DAL entity-service implementation or `DataServiceBase` remains, and normal DI still resolves all surviving area services.
- **Risk:** HIGH
- **Confidence:** HIGH

## ~~[x] Step 3 — Delete specialized repositories, interfaces, and obsolete repository tests~~

- **Files:** after a production-consumer search, delete specialized repository implementations/interfaces under `src/MobileShop.Dal/Repos/` and `src/MobileShop.Dal/Repos/Interfaces/` that have no remaining production consumer; delete their tests under `src/MobileShop.Tests/Dal/Repos/`; remove their registrations from `AddMobileShopRepository()`.
- **Expected removable set, subject to the final production-consumer search:** `UserRepo`, `CustomerRepo`, `SellerRepo`, `ProductRepo`, `TransactionRepo`, `AppleIdRepo`, `PhoneRepo`, `SecondHandRepo`, `GuaranteeRepo`, `ManufacturerRepo`, `ModelRepo`, `CategoryRepo`, `ColorRepo`, `EmployeeRepo`, and `PersonRepo`.
- **Preserve:** `src/MobileShop.Dal/Repos/Base/BaseRepo.cs`, `IBaseRepo.cs`, and any repository proven to have a surviving production consumer.
- **Symbols:** `AddMobileShopRepository()` and every specialized `I*Repo/*Repo` registration proven obsolete.
- **Change:** leave `IBaseRepo<T> → BaseRepo<T>` as the surviving generic repository registration; remove only obsolete specialized registrations.
- **Depends on:** Steps 1–2.
- **Edge cases:** do not remove `IBaseRepo<T>`; do not remove repository code still referenced by a surviving area service or initialization path; treat production source under `src/MobileShop.Services`, `src/MobileShop.Web`, and `src/MobileShop.Dal` as the deletion gate.
- **Tests:** delete only tests targeting removed specialized repositories; keep `BaseRepoTests` and `RepoTestBase` because area/page tests still use them.
- **Verify:** repository-wide production search for all deleted repository names, targeted tests as applicable, then full suite.
- **Done when:** no surviving production code references a deleted specialized repository and DI contains only surviving repository contracts.
- **Risk:** HIGH
- **Confidence:** MEDIUM

## ~~[x] Step 4 — Clean GlobalUsings and perform final Stage H validation~~

- **Files:** inspect/modify `src/MobileShop.Services/GlobalUsings.cs`, `src/MobileShop.Dal/GlobalUsings.cs`, `src/MobileShop.Web/GlobalUsings.cs`, and `src/MobileShop.Tests/GlobalUsings.cs`; do not change API files or unrelated feature files.
- **Change:** after Steps 1–3, remove only imports proven unused, especially obsolete specialized-repository namespaces or obsolete DAL service-base namespaces. Keep imports still required by area services, API stubs, models, and `IBaseRepo<T>`.
- **Depends on:** Steps 1–3.
- **Verify — final required chain:**
  - `dotnet build src/MobileShop.slnx --nologo && dotnet test src/MobileShop.slnx --nologo --no-build`
  - Production smoke: start the Web host in Production and verify **200** for `/`, `/Products`, `/People/Customers`, `/Transactions`, `/Reports/ProfitLoss`, `/Account/Login`, and `/Account/Profile`.
  - Confirm production logs contain no development initialization/wipe/seed activity and no `ERR`/`FTL`/`Exception`.
  - Confirm the production database remains non-destructive using the same fingerprint approach as Stage G.
  - Optional Development smoke: verify seed + `AccountDataService.EnsureAdminUser()` still succeeds, then restore the development database.
  - Repository-wide search: no deleted DAL entity-service classes, deleted specialized repository types/interfaces, obsolete registrations, `IEmployeeDataService`, or `DataServiceBase<` references.
- **Done when:** build succeeds, full suite passes, production smoke passes, production initialization remains non-destructive, the development-only admin path remains valid, and the repository contains the intended generic-repository + area-service architecture.
- **Risk:** MEDIUM
- **Confidence:** HIGH

## Global Definition of Done

- Area services remain the application-facing data services.
- No surviving area service depends on a deleted specialized repository.
- Obsolete DAL entity-service implementations and their obsolete tests are deleted.
- Specialized repositories/interfaces and obsolete repository tests are deleted when no production consumer remains.
- `IBaseRepo<T>` / `BaseRepo<T>` remain registered and functional.
- API service stubs and API-facing contracts remain untouched.
- `IDataService<T>` and `ApiDataServiceBase<T>` remain intact for the API path.
- `IEmployeeDataService` is removed because it has no surviving consumer.
- GlobalUsings contain only imports still required by each project.
- No database/schema/initialization/authentication behavior changes.
- Full build and test suite pass.
- Production smoke and non-destructiveness checks pass.
- Development EnsureAdmin smoke passes without leaving the development database modified.

## Execution notes

- One step per commit; Conventional Commits; no Co-authored-by.
- Stop after each step for Job B review.
- Delete only after a production-consumer search proves the target is dead.
- Keep implementation compact; no unrelated refactors.
- Do not modify `.clinerules/to-do.md`; only the reviewer ticks Stage H after final PASS.
- OUT OF SCOPE: production authentication, API implementation, schema/migrations, database initialization policy, Apple ID password handling, unrelated refactors, dependency upgrades, and UI redesign.
