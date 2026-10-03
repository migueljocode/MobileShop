# Audit — Job B (Execution Check): Stage T Step 2 correction (`a6b6026`)

**Evidence (actor-recorded, `act.md`):** `32b5511` failed Action **#342** (13 × CS0103 `MoneyLimits` in the Services project); the correction `a6b6026` failed Action **#344**: the Services project now builds (the `MobileShop.Models` global using was added there), but the **Tests project** has 27 build errors, so no test ran. I could not read the runs myself (GitHub API rate limit); the causes below are confirmed in the code.

**Verdict: FAIL — Step 2 is still not building (HIGH). A second, test-only correction pass is authorized because several causes lie in how the first audit scoped the tests; if it fails again the actor stops and the owner decides. Step 3 is NOT authorized.**

## Findings

### HIGH — the Tests project does not import `MobileShop.Models`
`MoneyLimits` lives in namespace `MobileShop.Models`, and `MobileShop.Tests/GlobalUsings.cs` only imports `MobileShop.Models.Entities…`, `.Enums`, `.Extensions` and `.ViewModels.Web`. `MoneyLimits` is used in `MoneyBoundaryTests.cs`, `ProductsDataServiceTests.cs` and `TransactionsDataServiceTests.cs`: all the CS0103 errors.

### HIGH — `ProductsDataServiceTests.cs` has no `BindModels` import
The new tests at lines ~1563, 1582 and 1592 use `CreatePhoneInputModel` unqualified (CS0246/CS0103), while the rest of the file uses fully-qualified names; the Tests global usings do not include `MobileShop.Models.ViewModels.Web.BindModels`.

### HIGH — `MoneyBoundaryTests.cs:82` (CS0826)
`var overLimit = new[] { new CreatePhoneInputModel…, new CreateAppleIdInputModel…, … }` mixes different types, so C# cannot infer the array type; the first array in the same test correctly uses `new object[]`.

### HIGH (would fail the next run once it compiles) — wrong model count in two new glass tests
`CreateGlassesAsync_rejects_computed_price_above_money_limit_without_writing_any_rows` and `…_rejects_decimal_overflow_in_computed_price_instead_of_throwing` assert `Assert.Equal(3, Context.Models.Count())`. `SeedCatalog` creates exactly **2** models (`iPhone 16`, `Apple ID`), and the service now rejects **before** `models.AddAsync` (the `TryComputeFinishedPrice` check is line 11 of the method, `models.AddAsync` is line 50), so nothing is added and the correct count is **2**.

## What I verified is correct (no change needed)
- `TryComputeFinishedPrice(long paid, decimal? percent, long? amount, out long finished)`: computes in `decimal`, keeps the amount-first/percent/floor/never-negative behaviour, returns `false` above `MoneyLimits.MaxRials` and catches `OverflowException`. It runs before any row is added in the phone, Apple ID and glass flows; the duplicate/wrong `nameof` check in `CreatePhoneAsync` is gone.
- `global using MobileShop.Models;` was added to `MobileShop.Services/GlobalUsings.cs` and fixes the Services build.
- The new tests' intent matches the previous audit (Buy over-limit, Apple ID input and computed over-limit, glass computed over-limit, glass `decimal.MaxValue` percent returning a failed result, glass valid large path with three products at 2,500,000,000).

## What the actor must do (second correction pass — tests only, one commit)
Read this section as the instruction; do not wait for a separate prompt.
1. `git pull`.
2. In `src/MobileShop.Tests/GlobalUsings.cs` add `global using MobileShop.Models;`.
3. In `src/MobileShop.Tests/Services/DataServices/Dal/ProductsDataServiceTests.cs` add `using MobileShop.Models.ViewModels.Web.BindModels;` above the `namespace` line (keep the existing fully-qualified references as they are).
4. In `src/MobileShop.Tests/Dal/EfStructures/MoneyBoundaryTests.cs` change `var overLimit = new[]` to `var overLimit = new object[]`.
5. In the two glass tests listed above change `Assert.Equal(3, Context.Models.Count());` to `Assert.Equal(2, Context.Models.Count());` (the seeded models only; the rejected glass model is never added).
6. Change nothing else: no production code, no other test.
7. Do not run `dotnet build`/`dotnet test` locally. Commit (`test: fix Stage T Step 2 test imports and glass model count`, never amend), push, and read the workflow run for the new commit.
8. Expect a clean build (0 warnings) and every test passing. If the run fails, STOP and report the exact errors or failing test names and messages; do not weaken assertions or touch production code.
9. Update the "Stage T — Step 2 Act Report" section in `act.md` with this commit's hash and `Action: #<run_number> — <Success|Failure|Pending>` in a separate `docs(act): record Stage T Step 2 second correction result` commit. Then STOP for Job B. Do not start Step 3 and do not touch `to-do.md`, `plan.md` or `audit.md`.

## Gate
Step 2 second correction → CI run → Job B. Step 3 starts only after Step 2 passes in CI.
