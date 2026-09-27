# Plan — UI/UX Enhancements — Step 6 of 7

Copied verbatim from .clinerules/to-do.md. Full stage list: to-do.md → # To-do — UI/UX Enhancements (7 Actionable Demands — Step 7 Already Implemented).

## [ ] Step 6 — Reports page: Total Profit sign/color + Distribution sort by Share%
- **Files**: 
  - modify: `src/MobileShop.Web/Pages/Reports/ProfitLoss.cshtml`
  - modify: `src/MobileShop.Services/Logging/Settings/DistributionSettings.cs` (DistributionCalculator)
  - modify: `src/MobileShop.Tests/Services/Logging/DistributionCalculatorTests.cs`
- **Symbols**: 
  - `ProfitLoss.cshtml` line 80 & 91: `@Model.TotalProfit.ToString("N0")` → add sign and conditional class
  - `DistributionCalculator.Calculate()` — already returns List<DistributionRow>; ensure it's ordered by `SharePercent desc`, then `EmployeeName`
  - `DistributionCalculatorTests` — **all four** test methods currently assert the insertion order (Mikaeeil → Anis → Shop) and must be re-indexed to descending Share%: Anis (50%), Mikaeeil (40%), Shop (10%). The four methods: `Profit_ReturnsExactlyThreeRowsWithFixedSharesAndFloorRounding`, `Profit_SharePercentAlways40_50_10EvenWhenEmployeesHaveDifferentShares`, `ZeroProfit_ReturnsThreeRowsWithZeroAmountsAndLossFlag`, `Loss_ReturnsThreeRowsWithEmployeesZeroShopGetsFullLoss`.
- **Current → Desired**: 
  - Total Profit shows `+1,234` (green) or `-567` (red) with explicit sign
  - Distribution table rows ordered: highest Share% first (Anis 50%, Mikaeeil 40%, Shop 10%)
- **Change**: 
  1. In `ProfitLoss.cshtml` (**both** renderings — lines 80 and 91): sign condition must be `TotalProfit > 0` (not `>= 0`, which renders `+0`), class condition `TotalProfit >= 0`: `<span class="@(Model.TotalProfit >= 0 ? "text-success" : "text-danger")">@(Model.TotalProfit > 0 ? "+" : "")@Model.TotalProfit.ToString("N0")</span>`
  2. In `DistributionCalculator.Calculate()`: there are **two** `return rows;` statements (line ~100 in the `totalProfit <= 0` branch, line ~133 at the end of the profit path). Replace **both** with a single exit point so the sort cannot be missed on either path — delete the early `return rows;` and end the method with `return rows.OrderByDescending(r => r.SharePercent).ThenBy(r => r.EmployeeName).ToList();`.
  3. In `DistributionCalculatorTests.cs`: update **all four test methods** (five assertion groups — Blocks 1 and 2 are both inside the first method) to Anis→Mikaeeil→Shop order with correct amounts:
     - **Block 1 (lines ~46-53)**: `Profit_ReturnsExactlyThreeRowsWithFixedSharesAndFloorRounding` — names. Re-index `rows[0].EmployeeName == "Anis Sahabi"`, `rows[1] == "Mikaeeil Jorjany"`, `rows[2] == "Shop"`; shares 50/40/10.
     - **Block 2 (lines ~57-59)**: same test — amounts for `Calculate(101m, …)`. Re-index `rows[0].CalculatedAmount == 50m` (Anis), `rows[1] == 40m` (Mikaeeil), `rows[2] == 11m` (Shop). **Shop = 11m**, not 10m.
     - **Block 3 (lines ~80-82)**: `Profit_SharePercentAlways40_50_10EvenWhenEmployeesHaveDifferentShares` (100m) — **SharePercent** assertions. Re-index `rows[0].SharePercent == 50` (Anis), `rows[1] == 40` (Mikaeeil), `rows[2] == 10` (Shop). Line 83 (`Sum == 100m`) needs no change.
     - **Block 4 (lines ~95-101)**: zero-profit case — names/shares re-indexed to Anis/Mikaeeil/Shop (50/40/10), amounts all 0m.
     - **Block 5 (lines ~119-125)**: loss case — names/shares re-indexed to Anis/Mikaeeil/Shop (50/40/10), amounts: Anis 0, Mikaeeil 0, Shop = totalProfit (the loss).
     - **The loss case is NOT already correct** — the earlier note claiming so was wrong: `Loss_ReturnsThreeRowsWithEmployeesZeroShopGetsFullLoss` (lines ~119-125) also asserts the insertion order (Mikaeeil, Anis, Shop) and **must** be re-indexed, otherwise the filtered test run fails.
     - Blocks at lines ~62 (sum check, `Assert.Equal(101m, …)`) and ~133 (`Sum == 100m`) are order-independent — **no change needed**.
- **Edge cases**: 
  - TotalProfit = 0 → show `0` with **no `+` sign**; color stays `text-success` (zero is not a loss)
  - Distribution rows with equal Share% → stable order by EmployeeName
- **Tests**: 
  - `DistributionCalculatorTests` must pass with updated assertions
  - Manual visual check
- **Verify**: `dotnet build src/MobileShop.slnx --nologo && dotnet test src/MobileShop.slnx --nologo --filter "DistributionCalculatorTests"`
- **Done when**: Total Profit shows sign/color; Distribution tab lists Anis → Mikaeeil → Shop; all tests pass.
- **Risk**: LOW | **Confidence**: HIGH

## Execution notes
- Work fast: combine independent shell commands (`&&`).
- Keep verification output short — only tail of build/test.
- Do not restate this plan in chat; write report to `act.md` per step.
- Stop after each step; wait for reviewer approval before next.
