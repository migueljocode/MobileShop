# Audit — Job B (Execution Check): Stage H Step 3 correction pass (`3b04b29`)

**Reviewed**: `f5570ea` → `1cc2772` (`3b04b29` test, `1cc2772` act.md, plus my previous audit and the correction prompt committed) against the Step 3 Job B HIGH finding.
**Method**: static review only (the sandbox can't restore NuGet packages). The reported "0 errors, 1 warning, 240 passed / 0 failed / 2 skipped" is unverified; re-run `dotnet build` and `dotnet test` locally.

**Verdict: PASS — no CRITICAL or HIGH findings. Stage H Step 3 and its correction are closed. Stage H Step 4 is the next authorized step.**

## Verified correct
- **HIGH resolved:** the new `BaseRepoPersonTests : BaseRepoTests<Person, BaseRepo<Person>>` (`src/MobileShop.Tests/Dal/BaseClass/BaseRepoPersonTests.cs`) is a concrete subclass, so the abstract base's 26 tests run again against the surviving `BaseRepo<T>`: add with persist on/off, update, soft-delete, save, predicate find and `SelectFirstAsync` ascending/descending/no-match.
- **Faithful recovery:** `CreateValidEntity()` is identical to the deleted `PersonRepoTests` (`Test` / `Person` / `09120000000`). `CreateRepo() => new(Context)` returns the concrete `BaseRepo<Person>`, which satisfies the `IBaseRepo<Person>` constraint.
- **Count:** 214 + 26 = 240 passed / 2 skipped, exactly as predicted. The actor also confirmed 26 tests execute under the new class.
- **Scope:** one new test file plus `act.md`. `BaseRepoTests.cs`, every production project and the Api are untouched, and `to-do.md` is untouched. The commit message follows the repo style.
- **Disclosure:** the actor owned the earlier "Problems: none" omission in the report. Good practice.

## Findings (LOW)
- The new file has no trailing newline. The class also uses a verbose XML summary for a 10-line file. Both are cosmetic.
- CS9124 still shows on full recompile, as expected. It remains backlog item 1.

## Guidance for Step 4 (from this review, not a finding)
- **Use a clean build** for Step 4's final validation (`dotnet build --no-incremental` or `dotnet clean` first), because incremental builds can hide CS9124 and report a false 0 warnings.
- **Global usings likely to be obsolete after Steps 2-3:** `global using MobileShop.Dal.Repos;` in the Dal, Tests and Services projects, since the namespace now holds no types (only the child `Repos.Base`). The actor must prove each removal by a build, as L8 requires.
- **Backlog items are not Step 4's scope.** Its plan files are the four `GlobalUsings.cs` files and validation only; the backlog needs its own pass after Stage H closes.

## Cleanup backlog (for the later Act prompt)
1. **Delete `protected IBaseRepo<Transaction> Transactions { get; } = transactions;`** at `ProductsDataService.cs:26` (the cause of CS9124).
2. **Stale comments naming deleted types:** `SampleDataSeedTests.cs:127`, `ProfileModelTests.cs:12` (should be `AccountDataService`) and `PeopleDataService.cs:114`.
3. **`ProductDetailsViewModel.Transactions = null!`:** replace with a body property `{ get; init; } = []`.
4. **Missing ordering test** for `GetInventoryRowsAsync`.
5. **Minor lost test:** the empty-inventory case for `ProductsDataService`.
6. **Owner decision, still open:** the `Phone`-category filter in `CreatePhoneAsync` (`ProductsDataService.cs:266`).

## Reviewer checklist
- [x] Job B; one verdict; only `audit.md` written.
- [x] `to-do.md` untouched (Stage H isn't finished).
- [x] No Act-mode suggestion.
