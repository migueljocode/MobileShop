# Audit — Job B (Execution Check): Stage H Step 2 correction pass (`7f2672e`)

**Reviewed**: `be467fc` → `d3a7a46` (`7f2672e` tests, `6af867c` act.md, `d3a7a46` my previous audit committed) against the Step 2 Job B findings and `actor.md`.
**Method**: static review only (the sandbox can't restore NuGet packages). The reported "522 passed / 0 failed / 2 skipped, 1 warning" is unverified; re-run `dotnet build` and `dotnet test` locally.

**Verdict: PASS — no CRITICAL or HIGH findings.** Step 2 is closed. One MEDIUM test defect is non-blocking; it goes into the cleanup backlog below.

## Verified correct
- **Scope:** tests-only. The commit touches `PeopleDataServiceTests.cs`, `TransactionsDataServiceTests.cs` and `act.md`; no service, Web, Dal or Api file changed. `audit.md` (reviewer's file) was left alone.
- **Count:** 519 → 522 is exactly the 3 added tests.
- **`RecordBuyAsync_rejects_negative_price`:** correct. The expected message matches `TransactionsDataService.cs:151` verbatim and it asserts no transaction row is written. It closes the earlier gap.
- **Customer soft-delete test:** meaningful, since the live and soft-deleted Sells are both seeded on the tested customer and the count is asserted as 1.
- **Warning identified:** CS9124 at `ProductsDataService.cs(26,62)`. Reporting it rather than fixing it is within scope.
- **Mutation testing:** the actor mutation-tested the customer predicate and honestly reported that the tests still pass. `ModelBuilderExtensions` applies a global `HasQueryFilter(!IsDeleted)` to every `BaseEntity`, and I confirmed the filter and that `IgnoreQueryFilters` is used only in the sample-data wipe. So the explicit `!t.IsDeleted` in `PeopleDataService` is currently redundant, and these tests verify behaviour, not the predicate. That's acceptable.

## Findings

### MEDIUM — `GetSellerRowsAsync_sold_count_ignores_soft_deleted_transactions` is vacuous
Both the live and the soft-deleted Buy are seeded on `shopSeller.Id`, never on the tested `seller` (Sara Karimi), who has no transactions at all. The assertion `SoldCount == 0` is therefore true regardless of soft-delete handling. The actor's mutation test only removed the customer predicate (line 22), so this slipped through.
**Fix (tests only):** seed the live Buy and the soft-deleted Buy with `seller.Id`, and assert `SoldCount == 1`.

### LOW
- **Clutter:** the customer test has an unneeded `shopCustomer` setup with `Assert.NotNull(shopCustomer)` as a "sanity" check, and the seller test creates a customer only to satisfy a foreign key. Drop the pointless assertion.
- **`act.md` accuracy:** it says the CS9124 warning was already recorded at Stage H Step 1, but Step 1's own report said 0 warnings. Cause and fix are the same either way (below).

## Cleanup backlog (for the later Act prompt; nothing here blocks Step 3)
1. **Delete `protected IBaseRepo<Transaction> Transactions { get; } = transactions;`** at `ProductsDataService.cs:26`. The unused duplicate is exactly what triggers CS9124 (the parameter is captured and also initializes a property), and deleting it should restore a 0-warning build. I asked for this removal in earlier audits.
2. **Fix the vacuous seller test** (MEDIUM above) and drop the pointless `shopCustomer` assertion.
3. **Stale comments naming deleted types:** `SampleDataSeedTests.cs:127`, `ProfileModelTests.cs:12` (should be `AccountDataService`) and `PeopleDataService.cs:114`.
4. **`ProductDetailsViewModel.Transactions = null!`:** replace with a body property `{ get; init; } = []`.
5. **Missing ordering test** for `GetInventoryRowsAsync` (phones first, then Apple IDs, each by `ProductId`).
6. **Minor lost test:** empty-inventory case for `ProductsDataService`.
7. **Owner decision, still open:** the `Phone`-category filter in `CreatePhoneAsync`'s model lookup (`ProductsDataService.cs:266`) versus the original page behaviour.

## Reviewer checklist
- [x] Job B; one verdict; only `audit.md` written.
- [x] `to-do.md` untouched (Stage H isn't finished).
- [x] No Act-mode suggestion.
