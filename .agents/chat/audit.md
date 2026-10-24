## Reviewer Audit — Stage 21, Job A (plan review, round 2)

**Status: APPROVED.** Proposed stage (Stage 21) is simple, in dependency order and complete; the four steps cover every requirement (config removal incl. Api JSON, DB-driven calculator, ≤100 validation, Reports modal with live preview).

Round 1 (APPROVED WITH CORRECTIONS) found four HIGH issues, all folded into `plan.md` and re-checked against the repo:
- `MoneyLimits.MaxRials` is `long.MaxValue`: `profit × percent` can overflow → Step 1 now mandates the overflow-safe floor formula (+ a `long.MaxValue` test).
- Redirect/hidden fields passed `DateTime`; filters are Jalali strings parsed by the global `JalaliDateTimeModelBinder` → Steps 3–4 now use `ToJalaliDateString()` strings.
- Success message needs `[TempData] SuccessMessage` + `_ProductCreateSuccess` partial (pattern in `Products/Details`) → specified.
- Wrong repo claim: no service catches `DbUpdateConcurrencyException` and `BaseEntity.TimeStamp` is not configured for SQLite → concurrency handling removed from Step 2.

MEDIUM/LOW (notes, no further round): calculator also rejects percents outside 0–100; five (not three/four) existing service tests to rewrite; empty-employee state shows a muted line; renaming `DistributionSettings.cs` → `DistributionCalculator.cs` is the only rename (the settings class is deleted); JS amount formatting must mirror `ToGroupedDigits` (Step 4 says to inspect it).

Next: Actor, Step 1 (after the Planner commit of `plan.md`, `audit.md` and the Stage 21 line in `TODO.md`).
