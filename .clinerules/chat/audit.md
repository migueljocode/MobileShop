# Audit — Job B (Execution Check): Stage T Step 2 (`32b5511`)

**Evidence gap:** `act.md` has no Step 2 report, so there is no `Action: #<run_number>` for `32b5511`. I could not read the run myself (GitHub API rate limit). The code review below is mine.

**Verdict: FAIL — one HIGH (the plan's "computed finished price above the limit is rejected" is missing for the glass flow, which can also throw), plus missing tests. One correction pass is authorized; Step 3 is NOT authorized.**

## Findings

### HIGH — `CreateGlassesAsync` never checks the computed finished price and can throw
It validates only `Price` and `ProfitAmount` against `MoneyLimits.MaxRials`, then calls `ComputeFinishedPrice(...)` and stores the result in every product. The glass `ProfitPercent` is `[Range(0, double.MaxValue)]` (percentages above 100 are legitimate for glass), so a price near the limit with a large percent stores a `Product.Price` far above `MaxRials` (up to the `long` range), and a very large percent makes `paid * percent / 100m` or the `(long)Math.Floor(...)` cast throw `OverflowException`, which surfaces as an unhandled 500. Plan Step 2 requires a computed price above the limit to return a failed `ServiceResult` (`ErrorField` `Price`) and write nothing; only the phone and Apple ID flows do it.

### MEDIUM — tests do not cover the rest of the plan's limit scenarios
Added: SQLite round trip of large values, factor total, profit/loss rows, distribution (profit and loss), model validation (`Money_inputs_accept_max_and_reject_above_max`), phone create (exact large price, over-limit rejected), Sell (accepts above `int.MaxValue`, over-limit rejected). Missing: Buy over-limit rejection, Apple ID over-limit input and computed-over-limit rejection, and any glass over-limit/overflow case.

### LOW — copy-paste in `CreatePhoneAsync`
After computing `finishedPrice` there are two identical `if (finishedPrice > MoneyLimits.MaxRials)` returns; the first uses `nameof(CreateGlassInputModel.Price)` (same string, wrong type) and makes the second unreachable. Keep one, using `nameof(CreatePhoneInputModel.Price)`.

## Verified correct
- `MoneyLimits.MaxRials = 10_000_000_000_000L`; all six money inputs (`Price`/`ProfitAmount` of phone, Apple ID and glass; `Price` of Buy and Sell) use `[Range(0, MoneyLimits.MaxRials)]`.
- **The Step 1 carry-over is fixed:** `SellInputModel.Price` is now `long`.
- `RecordBuyAsync`/`RecordSellAsync` reject `Price > MaxRials` ("The price is too large.") before any write and keep the previous message for negative prices; the Buy/Sell pages do not read `ErrorField`, so setting it to `Price` changes no UI.
- Scope: only `MoneyLimits.cs`, the five bind models, the two services and tests; no entity, migration, page, PDF or script change.

## What the actor must do (correction pass — one commit)
Read this section as the instruction; do not wait for a separate prompt.
1. `git pull`.
2. In `src/MobileShop.Services/DataServices/Dal/ProductsDataService.cs`:
   - Replace the private `ComputeFinishedPrice(...)` with a `TryComputeFinishedPrice(long paid, decimal? percent, long? amount, out long finished)` that computes in `decimal`, keeps the existing amount-first rule, flooring and "never negative" behaviour, and returns `false` when the result is above `MoneyLimits.MaxRials` or when the decimal arithmetic throws `OverflowException`. Behaviour for values within the limit must not change.
   - Use it in `CreatePhoneAsync`, `CreateAppleIdAsync` and `CreateGlassesAsync`; on `false` return `new ServiceResult(false, "The price is too large.", nameof(<Input>.Price), null)` **before any model, product or other row is added** (glass: before `models.AddAsync`).
   - In `CreatePhoneAsync` remove the duplicate unreachable check and the wrong `nameof(CreateGlassInputModel.Price)`.
3. Add tests to the existing test files:
   - `TransactionsDataServiceTests`: `RecordBuyAsync` rejects `Price = MoneyLimits.MaxRials + 1` without writing.
   - `ProductsDataServiceTests`: Apple ID over-limit input price is rejected without writing; Apple ID with a price near the limit plus a percent that pushes the computed price above it is rejected without writing; glass computed over-limit (price `MoneyLimits.MaxRials` with a large percent) is rejected with no `Product`, `Glass` or `Model` rows added; glass with an absurd percent (for example `1e30m`) returns a failed `ServiceResult` instead of throwing; glass valid large path (paid 2,000,000,000, 25%, count 3) stores three products at exactly 2,500,000,000.
4. Do not run `dotnet build`/`dotnet test` locally. Commit (`fix(products): reject over-limit computed prices in all create flows`, never amend), push, and read the workflow run for the new commit. If the first push's run for `32b5511` is available, include its result too.
5. Add a "Stage T — Step 2 Act Report" section to `act.md` in a separate `docs(act): record Stage T Step 2 result` commit: both commit hashes, `Action: #<run_number> — <Success|Failure|Pending>` for each, results and any limitation. If a run fails, STOP and report the exact failing test names and messages. Then STOP for Job B. Do not start Step 3 and do not touch `to-do.md`, `plan.md` or `audit.md`.

## Gate
Step 2 correction → CI run → Job B. Step 3 starts only after Step 2 passes in CI.