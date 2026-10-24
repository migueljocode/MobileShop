# Plan — Stage 22 — Edit-shares modal keeps the saved baseline after a rejected save

## Requirements
- Folds the one open MEDIUM finding from the Stage 21 Job B review: after a **rejected** save the modal must keep comparing against the **stored** shares, not the rejected ones.
- No other behaviour change. Must not change: the service, calculator, DB schema, entities, sample data, Api, the success path, the live-preview rules, or the JS file unless Step 1 proves it necessary.
- All other stages in `TODO.md` are already ticked, so this is the only open stage.

## Facts found in the code (verified)
- `ProfitLossModel.OnPostSaveSharesAsync` on failure runs `ResolveBoundsAsync()` + `LoadAsync()` and then `ShowPostedShares()`, which **overwrites `EditableShares`** with the posted percents (`share with { SharePercent = percent }`).
- `ProfitLoss.cshtml` renders both `data-original-percent="@share.SharePercent"` and the input `value="@share.SharePercent"` from `EditableShares`, and `shopPercent = 100 − EditableShares.Sum(SharePercent)`. After a rejected save all three therefore hold the rejected values.
- Effect: the modal opens with "was" amounts, the Reset button and the Shop row's baseline built from the **rejected** numbers; the Shop baseline can be negative (e.g. posted 60 + 50 → `−10%`). `distribution-shares.js` reads the baseline only from `data-original-percent` and calls `recalc()` once on load, so it needs no change once the markup is right.
- Reachability is low (the JS disables Save above 100 %): a stale/deactivated employee, or a bypassed form, is what reaches the server error path.
- The only test of this path is `OnPostSaveSharesAsync_failure_rerenders_page_with_error_and_posted_values` in `src/MobileShop.Tests/Web/Pages/Reports/ProfitLossTests.cs`; it asserts the posted value on `EditableShares`. The test project has no Razor-rendering infrastructure (no `WebApplicationFactory`/view-engine tests).

## Reviewer Briefing
- One LOW-risk step touching one page model, one view and one test. A quick sanity check is enough; the only thing worth confirming is that `EditableShares` is never overwritten and the input values come from `PostedPercents`.

## ~~[x] Step 1 — Separate stored baseline from posted input values~~
- Files: inspect: `src/MobileShop.Web/Pages/Reports/ProfitLoss.cshtml.cs`, `src/MobileShop.Web/Pages/Reports/ProfitLoss.cshtml` (the modal, ~lines 255–300), `src/MobileShop.Web/wwwroot/js/distribution-shares.js` (read only); modify: `ProfitLoss.cshtml.cs`, `ProfitLoss.cshtml`, `src/MobileShop.Tests/Web/Pages/Reports/ProfitLossTests.cs`; do not touch: the service, calculator, JS (unless the manual check below fails), other pages.
- Symbols: `ProfitLossModel.EditableShares` (stays the **stored** values, never overwritten); new `IReadOnlyDictionary<int, int> PostedPercents { get; private set; }` (employee id → posted percent), empty by default; `ProfitLossModel.ShowPostedShares()` (replace by filling `PostedPercents`).
- Current -> Desired: the failure path replaces the stored percents with the posted ones. Desired: it only records the posted values; the view uses stored values for the baseline and posted values for the inputs.
- Change:
  1. Replace `ShowPostedShares()` with code that sets `PostedPercents` from `Shares` (first value wins for a duplicated id, as today's `DistinctBy`), leaving `EditableShares` untouched.
  2. In the modal, keep `data-original-percent` and `shopPercent` on the stored `EditableShares`, and render each input's `value` as the posted percent when `Model.PostedPercents` has the employee, otherwise the stored percent.
  3. Do not clamp the baseline when stored data is already over 100 % (the tab already shows `DistributionError`); no JS change expected: its `recalc()` on load shows the posted values as changes against the stored baseline with the "Over by N%" summary and Save disabled.
- Depends on: none.
- Edge cases / error handling: a posted id that is no longer active has no row, so its value is ignored; an employee that was not posted shows its stored value; the success path and the GET path leave `PostedPercents` empty.
- Tests: update `OnPostSaveSharesAsync_failure_rerenders_page_with_error_and_posted_values` so `EditableShares` still holds the stored percent and `PostedPercents[1]` holds the posted one; add a case where `Shares` repeats an id (first value kept) and one where a posted id is not in `EditableShares` (no exception, id kept only in `PostedPercents`); assert `PostedPercents` is empty after `OnGetAsync` and after a successful save.
- Verify: CI green. Manual (user, since no render test exists): open Distribution → Edit shares; in DevTools remove `disabled` from Save, type percents totalling over 100 and submit; the modal reopens with the error, the "was" amounts and Shop baseline equal to the saved numbers, the typed values shown as changes with `Over by N%`, and Reset restores the saved numbers.
- Done when: after a rejected save the baseline comes from stored data and CI is green.
- Risk: LOW
- Confidence: HIGH

## Global Definition of Done
- `EditableShares` always holds stored shares; typed values live only in `PostedPercents` and the input `value`s.
- The existing live-preview rules, the success path, and all Stage 21 tests still pass; no service, schema, Api or JS change unless the manual check required one.
- CI green (build + test), with the run number cited by the Reviewer; the user confirms the manual check.
