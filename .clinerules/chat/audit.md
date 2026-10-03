# Reviewer Audit — Stage T Step 2

## Evidence reviewed

- Current Stage T Step 2 implementation at correction commit `1312570d8f55b2aa773f24450cbb5e0f2403a29a`.
- Current reviewer documentation on `main`, including the previous Action #350 follow-up.
- `ReportsDataService.cs` and `ReportsDataServiceTests.cs`.
- Recorded CI Action **#350**, run ID `37150091734`: build passed; .NET tests reported **345 passed, 1 failed, 0 skipped, 346 total**.

## Verdict

**FAIL — Stage T Step 2 is not closed. Step 3 is NOT authorized.**

The only remaining recorded test failure is the large-Rial profit-percentage assertion. The production money totals themselves are represented correctly as `long`; the failure is in the test's exact decimal expectation.

## Finding

### HIGH — remaining percentage assertion does not match the production calculation

`ProfitLossRowViewModel.ProfitPercent` is calculated as:

`(Profit / (decimal)Bought) * 100m`

For the test values:

- Bought = 3,000,000,000
- Sold = 5,000,000,000
- Profit = 2,000,000,000
- Mathematical percentage = 200 / 3 percent

The current test asserts `Assert.Equal(200m / 3m, row.ProfitPercent)`, while Action #350 reports the service result as:

`66.666666666666666666666666666666666666666666666670`

versus the expected value:

`66.666666666666666666666666666666666666666666666667`

The discrepancy is at the final decimal places and does not indicate loss of precision in the large-Rial integer totals. The test should verify the intended percentage semantics without requiring an incompatible exact decimal representation.

## Verified positives

- Action #350 reached the complete .NET test suite after a clean build.
- The earlier glass computed-price rejection/count failures are resolved.
- The earlier money-boundary fixture failures are resolved.
- The large-Rial Bought, Sold, Profit, and total-profit assertions are passing according to Action #350.
- The production implementation uses decimal arithmetic for computed product prices and enforces `MoneyLimits.MaxRials` before writes.
- No Step 3 work should begin while this Step 2 gate remains failed.

## Required disposition

**Stage T Step 2 remains FAILED.**

The remaining correction is narrowly scoped to the percentage test expectation/assertion. No production-code defect is established by the current CI evidence.

**Step 3 is NOT authorized.**

A new explicit actor authorization is required before another correction pass.