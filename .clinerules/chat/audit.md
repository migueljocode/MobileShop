# Audit — Job B (Execution Check + Stage Sign-off): Stage H Step 4

**Verdict**: **PASS** — Stage H Definition of Done met. Stage H signed off in `to-do.md`.

## Step 4 verified (against `act.md` + `5cd80f2`)
- Obsolete `global using MobileShop.Dal.Repos;` removed from Services + Tests only; `Repos.Base` kept.
- Clean build: 0 errors, **1 warning** CS9124 (backlog — intentionally not fixed in Step 4).
- Suite **240 passed**, 2 skipped.
- Production 7/7 routes 200; fingerprint stable; no wipe/seed in Production.
- Dev EnsureAdmin path still works; DB restored after optional Dev smoke.
- Dead specialized-repo / entity-service **code** references: none (comment-only leftovers listed below).

## Stage H DoD
- Area services only on Dal DI path; API stubs retained.
- Specialized repos deleted; `IBaseRepo<T>` remains.
- GlobalUsings evidence-based; full validation green.

---

# Act prompt — Post–Stage H cleanup (single commit preferred)

**Scope:** polish only. No architecture changes, no API project edits, no schema/migrations, no auth restore.

Claude’s backlog is accepted with these corrections:
- The **vacuous seller soft-delete test is already fixed** (`GetSellerRowsAsync_sold_count_ignores_soft_deleted_transactions` asserts count `1` after excluding a soft-deleted buy). **Do not rework unless you prove it is still vacuous.**
- CS9124 is **not** a dead ctor parameter: methods use primary-ctor `transactions`. Only the **unused property** is dead.

## Required items

1. **CS9124 — delete the dead property only**  
   File: `src/MobileShop.Services/DataServices/Dal/ProductsDataService.cs`  
   Remove: `protected IBaseRepo<Transaction> Transactions { get; } = transactions;`  
   Keep: ctor parameter `IBaseRepo<Transaction> transactions` and every use of `transactions` in methods.  
   Done when: clean `dotnet build` reports **0 warnings** (or only unrelated pre-existing ones, none CS9124).

2. **Stale comments naming deleted types** (comment-only; no behavior change)  
   - `src/MobileShop.Tests/Web/Pages/Account/ProfileModelTests.cs` (~line 12): `UserDataService.ChangePasswordAsync` → `AccountDataService` / `IAccountDataService`.  
   - `src/MobileShop.Services/DataServices/Dal/PeopleDataService.cs` (~line 114): `ProductDataService.GetInventoryRowsAsync` → current area service (`ProductsDataService` or “inventory projection”).  
   - `src/MobileShop.Tests/Dal/Initialization/SampleDataSeedTests.cs` (~lines naming `TransactionDataService`): reword to shop-sentinel rule / `TransactionsDataService` area behavior without claiming the deleted type still exists.

3. **`ProductDetailsViewModel.Transactions = null!`**  
   File: `src/MobileShop.Models/ViewModels/Web/ProductDetailsViewModel.cs`  
   Prefer a safe default: e.g. make `Transactions` a normal init property defaulting to `[]`, **or** keep the record but default to `Array.Empty<ProductTransactionViewModel>()` / `[]` instead of `null!`.  
   Fix any call sites that relied on `null!` so they still compile and tests pass.

4. **Add ordering test for `GetInventoryRowsAsync`**  
   File: `ProductsDataServiceTests` (or equivalent).  
   Assert deterministic order (document the rule you enforce — e.g. by `ProductId` ascending, or phones block then Apple IDs, matching current implementation).  
   Must fail if order is shuffled.

5. **Add empty-inventory test for `ProductsDataService`**  
   No phones/Apple IDs in DB → `GetInventoryRowsAsync()` returns empty list (not null). Cover default and filtered type args if cheap.

## Out of scope (do not expand)
- Owner decision on `Phone`-category filter in `CreatePhoneAsync` (leave as-is unless already decided elsewhere).
- Re-opening Stage H deletions, API implementations, UI redesign.
- Vacuous seller test rewrite if already non-vacuous (verify only).

## Acceptance
- [ ] CS9124 gone; `transactions` parameter still used where needed
- [ ] Three stale comments updated
- [ ] `ProductDetailsViewModel.Transactions` has no `null!`
- [ ] New ordering + empty-inventory tests green
- [ ] `dotnet build src/MobileShop.slnx --nologo` (prefer clean/no-incremental) + full `dotnet test` green
- [ ] One conventional commit (or small focused commits); update `act.md` with hash

**Reviewer:** after Act completes, Job B on this cleanup only; do not reopen Stage H unless a regression appears.
