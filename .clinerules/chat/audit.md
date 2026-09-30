# Audit — Job B (Execution Check): Stage H Step 2

**Reviewed**: `386e0e7` → `be467fc` (`a31cb6e` refactor + `be467fc` act.md) against `plan.md` Step 2 and `actor.md`.
**Method**: static review only (the sandbox can't restore NuGet packages). The reported "0 errors, 1 warning, 519 passed / 0 failed / 2 skipped" is unverified; re-run `dotnet build` and `dotnet test` locally.

**Verdict: PASS — no CRITICAL or HIGH findings.** Three MEDIUM/LOW items are non-blocking, but I'd close the coverage gap before Step 3.

## Verified correct
- **Scope:** deleted exactly the plan's set: nine DAL entity services, `Dal/Base/DataServiceBase.cs`, `IEmployeeDataService.cs` and eight test files (`EmployeeDataServiceTests` never existed). Only `ServiceCollectionExtensions.cs`, `Services/GlobalUsings.cs`, one doc comment in `TestDataHelpers.cs` and the `plan.md` header were modified.
- **API untouched:** the `if (useApi)` branch keeps all 8 entity-service registrations and the Api stubs; `IDataService<T>`, `ApiDataServiceBase<T>` and the eight entity-service interfaces survive (L1/L2).
- **No dangling references:** a word-boundary search of `*.cs`, `*.cshtml`, `*.md` and `*.json` for all nine deleted names, `IEmployeeDataService` and `DataServiceBase<` finds no code references. The remaining consumers of the eight kept interfaces are the Api DI lines only.
- **Forced GlobalUsings change is justified:** removing `global using …Dal.Base;` is the minimum needed for the deletion to compile (CS0234). No other GlobalUsings touched; Step 4 still owns the rest.
- **Count reconciliation:** 550 → 519 is 31, matching the deleted tests (27 `[Fact]` + 2 `[Theory]` × 2 `[InlineData]`); I counted the same method names.
- **DI:** the six area-service registrations are kept and only the nine DAL entity registrations were removed.

## Findings (non-blocking)

### MEDIUM — Two behaviours lost their only test
I compared the deleted test names with the surviving area-service tests. Most behaviours are re-covered (Account, Reports, Transactions record/PDF paths, Products, People details). Two that the surviving services still implement are not:
1. **Soft-deleted exclusion in the People list counts.** `PeopleDataService.cs:22` and `:43` filter `!t.IsDeleted` on purchased/sold counts. The deleted `Customer…` and `Seller…GetListRows_counts_…_excludes_soft_deleted_async` tests were the only coverage, and `PeopleDataServiceTests` has no `IsDeleted` assertion. (The zero-count case is still covered.)
2. **`RecordBuyAsync` negative-price rejection** (`TransactionsDataService.cs:148`). The deleted `RecordBuyAsync_rejects_negative_price` was the only test; survivors cover `RecordSellAsync_rejects_negative_price` only.

**Fix (tests only, no source change):** add three tests to the existing survivor files: customer and seller count ignores a soft-deleted transaction, and `RecordBuyAsync` rejects a negative price. Do this as a small correction commit before Step 3, or record it in `act.md` if you deliberately accept the loss.

### MEDIUM — Build reports 1 warning, and it isn't identified
Stage H Step 1 ended at 0 warnings and this step's report says "0 errors, 1 warning" without naming it. I found no dangling `cref` to a deleted type, so I can't pin it down statically. The actor should run `dotnet build --nologo -v n | grep -i warning` and either fix it or state that it's pre-existing and why, since Step 4's final validation expects a clean build.

### LOW
- **Stale prose comments** naming deleted types (no compile impact):
  - `SampleDataSeedTests.cs:127` mentions `TransactionDataService`;
  - `ProfileModelTests.cs:12` says "calls `UserDataService.ChangePasswordAsync`" (it's `AccountDataService` now);
  - `PeopleDataService.cs:114` mentions `ProductDataService`.
  Fix them in Step 4's cleanup.
- **Minor lost coverage:** the deleted `GetInventoryRows_returns_empty_list_when_no_products` has no `ProductsDataService` counterpart.

## Carried from earlier audits (not actioned, none blocking)
- `ProductDetailsViewModel.Transactions = null!`
- the unused `protected IBaseRepo<Transaction> Transactions` property in `ProductsDataService`
- no ordering test for `GetInventoryRowsAsync`
- the `Phone`-category filter in `CreatePhoneAsync`'s model lookup (still an owner decision)

## Reviewer checklist
- [x] Job B; one verdict; only `audit.md` written.
- [x] `to-do.md` untouched (Stage H isn't finished).
- [x] No Act-mode suggestion.
