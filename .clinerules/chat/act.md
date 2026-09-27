# Act Report — Step 6: Reports page Total Profit sign/color + Distribution sort

## Commit
- **Hash**: pending (recorded in follow-up)
- **Message**: feat(web): show signed Total Profit and sort Distribution rows by Share%

## Status
COMPLETE

## Changes
- `ProfitLoss.cshtml` (both renderings, lines 80 & 91): Total profit wrapped in `<span class="@(Model.TotalProfit >= 0 ? "text-success" : "text-danger")">@(Model.TotalProfit > 0 ? "+" : "")@Model.TotalProfit.ToString("N0")</span>` — `+` only when strictly positive (zero renders plain `0`), green when `>= 0`, red when negative.
- `DistributionCalculator.Calculate()`: removed the early `return rows;` in the `totalProfit <= 0` branch and added `if (totalProfit > 0) { ... }` around the profit block so both paths reach a single exit: `return rows.OrderByDescending(r => r.SharePercent).ThenBy(r => r.EmployeeName).ToList();`.
  - Note: simply deleting the early return broke the control flow (loss rows AND profit rows both added → 6 rows, 2 tests failed). Fixed by gating the profit block with `if (totalProfit > 0)`, keeping the single sorted exit the plan requires.
- `DistributionCalculatorTests.cs`: re-indexed all four test methods (five assertion groups) to Anis (50%) → Mikaeeil (40%) → Shop (10%): names, shares, and amounts (Block 2: 50m/40m/11m for `Calculate(101m, …)`; loss case: 0/0/loss). Order-independent blocks (sum checks) untouched.

## Verification
- `dotnet build src/MobileShop.slnx --nologo` → Build succeeded, 0 Warning(s), 0 Error(s)
- `dotnet test src/MobileShop.slnx --nologo --filter "DistributionCalculatorTests"` → Passed: 11, Failed: 0
- `dotnet test src/MobileShop.slnx --nologo` (full suite) → Passed: 405, Failed: 0, Skipped: 2 (pre-existing QuestPDF skips)

## Friction noted
- Plan's instruction "delete the early `return rows;`" alone was incomplete — without gating the profit block the loss branch fell through and appended the profit rows (6 rows instead of 3). The minimal fix (adding `if (totalProfit > 0)`) preserves the plan's single-exit-point requirement; flagged here in case the plan text should be amended.

## Problems
- None beyond the above.

## Status
COMPLETE
