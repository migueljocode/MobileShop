# Reviewer Audit — Stage T Step 3

## Verdict
**FAIL — Stage T Step 3 is incomplete.**

- Commit `9d20a9b8bd51ac65a84d337a21dcb925763d1e77` scaled all 17 product prices and 26 transaction finished prices in `sample-data.json` by ×10, and Action #364 is `Success`.
- The required test `Sample_data_money_is_expressed_in_rials` was not implemented in `src/MobileShop.Tests/Dal/Initialization/SampleDataSeedTests.cs`.
- The actor can fix this directly.

## Fix
Add `Sample_data_money_is_expressed_in_rials` to `SampleDataSeedTests.cs` asserting every seeded `Price` and `FinishedPrice` is a multiple of 10, >= 1,000,000, and <= `MoneyLimits.MaxRials`. Update `act.md`, push the correction commit, and verify CI.