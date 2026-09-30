# Audit — Job B (Execution Check): Stage H Step 3

**Reviewed**: `90d07ad` → `f5570ea` (`e3a6622` deletions, `4b39100` act.md, `f5570ea` plan tick) against `plan.md` Step 3, `actor.md` and the Step 3 prompt.
**Method**: static review only (the sandbox can't restore NuGet packages). The reported "0 errors, 1 warning, 214 passed / 0 failed / 2 skipped" is unverified; re-run `dotnet build` and `dotnet test` locally.

**Verdict: NEEDS ONE CORRECTION PASS — one HIGH finding (the surviving `BaseRepo<T>` lost all of its direct tests). The deletions themselves are clean.**

## Finding

### HIGH — `BaseRepo<T>` now has no direct tests; 26 tests were silently lost
- **What happened:** `BaseRepoTests<TEntity, TRepo>` is an **abstract** generic base (`BaseRepoTests.cs:12`) with 26 `[Fact]`s. They only ever ran through concrete subclasses, and all ten of those (Guarantee, Person, Phone, SecondHand, Seller, AppleId, Customer, Product, Transaction, User) were in the 14 deleted `*RepoTests.cs` files. `grep "BaseRepoTests<"` now finds only the abstract declaration itself. So the file was "kept" (as L7 asked), but **zero of its tests execute**.
- **Why it matters:** `IBaseRepo<T>`/`BaseRepo<T>` is the only surviving repository abstraction, and every area service and page depends on it. Stage H's Global Definition of Done requires it to "remain registered and functional". The 26 tests were its only direct coverage:
  - `Add`/`AddAsync` with persist true and false;
  - `Update` marking the entity unchanged;
  - `Delete` soft-deleting and hiding the row;
  - `SaveChanges`;
  - the predicate `Find` overloads;
  - `SelectFirstAsync` ascending, descending and no-match.
  Area-service tests only exercise some of these paths indirectly.
- **Disclosure:** `act.md` explains the 308-test drop well (54 declared + 26 inherited × 10, reconciled by an actual run) but reports "Problems: none" and doesn't mention that keeping the abstract base leaves it running nothing.
- **Root cause:** `plan.md` L7/Step 3 assumed `BaseRepoTests` stays useful "because area/page tests still use them". That's true for `RepoTestBase`, not for the abstract generic. This was a plan premise gap, not an actor error.
- **Fix (tests only, no production change):** add a concrete derived class in `src/MobileShop.Tests/Dal/BaseClass/`, for example `BaseRepoPersonTests : BaseRepoTests<Person, BaseRepo<Person>>`, with `CreateRepo() => new BaseRepo<Person>(Context)` and a `CreateValidEntity()` copied from the deleted `PersonRepoTests.cs` (recoverable with `git show e3a6622^:src/MobileShop.Tests/Dal/Repos/PersonRepoTests.cs`). One entity restores all 26 generic tests; the old per-entity subclasses added no behaviour beyond that. Expected suite result: **240 passed / 2 skipped**.

## Verified correct
- **Deletions:** exactly 44 files (15 `*Repo.cs`, the whole `Interfaces/` folder of 15, and 14 test files). `Dal/Repos/` now contains only `Base/BaseRepo.cs` and `Base/IBaseRepo.cs`. `RepoTestBase`, `BaseRepoTests`, `TestDataHelpers` and `Initialization/` survive.
- **Consumer proof:** the actor re-ran the word-boundary search and found only DI lines and the repo tests; I found the same independently. There were no surviving consumers.
- **Forced change was minimal:** exactly four `global using MobileShop.Dal.Repos.Interfaces;` lines removed (Dal, Web, Tests, Services), as predicted. `global using MobileShop.Dal.Repos;` was correctly kept because `Repos.Base` still exists.
- **DI:** the 15 specialized registrations are gone, `AddScoped(typeof(IBaseRepo<>), typeof(BaseRepo<>))` stays, and the method's XML summary was updated.
- **Scope:** `src/MobileShop.Api` is untouched, no area service or production logic changed, and `to-do.md` was not touched. The actor ticked only the Step 3 header in `plan.md`.
- **Test-count reconciliation:** 522 → 214 = 308. The actor measured it by stashing and running the baseline: 10 classes × 26 inherited + declared tests (AppleId 30, Customer 28, Product 29, Transaction 31, User 36, 26 each for the five bare subclasses, 6 each for Category/Color/Manufacturer/Model). That is solid evidence.
- **CS9124** reappeared on the full recompile, as predicted. Still backlog item 1.

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
- [x] No Act-mode suggestion in this file; the correction prompt is delivered separately as `act.md`.
