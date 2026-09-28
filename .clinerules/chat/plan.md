# Plan — Task 3: Fix Manual Date Range mode (From/To not working)

## Reviewer Briefing
- **Root Cause & Core Fix**: Date pickers rendered the browser's empty date placeholder `mm/dd/yyyy` because `From` and `To` were only set on the server inside the `else` (Manual mode) branch. On initial page load (`Mode == Automatic`), `From` and `To` were `null`, rendering `<input type="date">` without `value` attributes.
- **Pre-populating on initial load**: Querying `earliest` transaction date and setting `From ??= earliest ?? today` and `To ??= today` on initial load (while strictly keeping `EffectiveFrom`/`EffectiveTo` governed by `Preset` in Automatic mode) guarantees the pickers carry valid default `value` attributes on initial page load.
- **Client-side instant toggle (Removing JS reload hack)**: Commit `1555e24` introduced `document.querySelector('form').requestSubmit()` on the Manual radio change event to trigger a server round-trip. Because the pickers will now be pre-populated in the DOM on initial load, this reload hack is completely unnecessary and causes unwanted page reloads. Reverting it to a pure client-side `updateVisibility()` makes switching instant and clean.
- **Test coverage**: `SetupProfitLossMocks` in `ProfitLossTests.cs` must configure a default return for `GetEarliestTransactionDateAsync()` so automatic preset tests don't receive `null`. The test asserting `Assert.Null(model.From)` on automatic load will be updated to verify pre-population.
- **HTML5 Date Format**: Native `<input type="date">` stores and submits values in ISO format (`yyyy-MM-dd`), which is already configured via `asp-format="yyyy-MM-dd"`. When populated, the browser renders the date according to locale rather than displaying the empty placeholder `mm/dd/yyyy`.

---

## ~~[x] Step 1 — Pre-populate From/To in page model and update tests~~
- **Files**:
  - modify: `src/MobileShop.Web/Pages/Reports/ProfitLoss.cshtml.cs`
  - modify: `src/MobileShop.Tests/Web/Pages/Reports/ProfitLossTests.cs`
  - do not touch: DAL, repositories, services
- **Symbols**: `ProfitLossModel.ResolveBoundsAsync()`, `SetupProfitLossMocks`, `ProfitLossTests`
- **Current -> Desired**:
  - Current: `From` and `To` remain `null` on Automatic mode and are only populated in Manual mode.
  - Desired: In `ResolveBoundsAsync()`, fetch `earliest = await transactionDataService.GetEarliestTransactionDateAsync();` upfront.
    - If `Mode == DateRangeMode.Automatic`: calculate `(EffectiveFrom, EffectiveTo)` from `Preset` as before. Then pre-populate `From ??= earliest ?? today;` and `To ??= today;` so the date inputs render with default values on page load without altering `EffectiveFrom`/`EffectiveTo`.
    - If `Mode == DateRangeMode.Manual`: `EffectiveFrom = From ?? (earliest ?? today); EffectiveTo = To ?? today; From = EffectiveFrom; To = EffectiveTo;` and set `EmptyDatabaseNote` if `earliest == null`.
  - In `ProfitLossTests.cs`:
    - Add `serviceMock.Setup(s => s.GetEarliestTransactionDateAsync()).ReturnsAsync(new DateTime(2024, 1, 1));` to `SetupProfitLossMocks()`.
    - Update `Manual_toggle_populates_From_with_earliest_and_To_with_today` (line 208) to assert that on the initial Automatic load, `model.From` equals `earliestTx` and `model.To` equals `DateTime.Today` instead of `Assert.Null`.
    - Add a test verifying that on Automatic load with preset `Month`, `model.From` and `model.To` are pre-populated while `model.EffectiveFrom` remains the 1st of the month.
- **Verify**: `dotnet test src/MobileShop.Tests/MobileShop.Tests.csproj --filter "ProfitLossTests"`
- **Done when**: All `ProfitLossTests` pass, verifying both preset effective filtering and picker pre-population.
- **Risk**: LOW
- **Confidence**: HIGH

---

## [ ] Step 2 — Remove JS form reload hack and restore instant client-side toggle
- **Files**:
  - modify: `src/MobileShop.Web/Pages/Reports/ProfitLoss.cshtml`
  - do not touch: backend
- **Symbols**: `<script>` block in `ProfitLoss.cshtml`
- **Current -> Desired**:
  - Current: `manualRadio.addEventListener('change', () => { updateVisibility(); if (manualRadio.checked) document.querySelector('form').requestSubmit(); });`
  - Desired: `manualRadio.addEventListener('change', updateVisibility);`
  - Switching between Automatic and Manual becomes an instant, flicker-free client-side toggle that merely reveals or hides controls. The form submits only when the user clicks the "Apply" button.
- **Verify**: `dotnet build src/MobileShop.slnx --nologo && dotnet test src/MobileShop.slnx --nologo --no-build`
- **Done when**: Solution builds cleanly with 0 warnings/errors, all tests pass, and manual radio switch no longer submits the form.
- **Risk**: LOW
- **Confidence**: HIGH

---

## Global Definition of Done
- `From` and `To` date inputs render with valid `value="yyyy-MM-dd"` on page load (no `mm/dd/yyyy` empty placeholder).
- Automatic mode continues to filter by the selected Preset without interference from `From`/`To`.
- Manual mode filters by `From` and `To`.
- Toggling between Automatic and Manual mode is instant on the client side with no page reload.
- Full build and test suite pass (`dotnet build src/MobileShop.slnx --nologo && dotnet test src/MobileShop.slnx --nologo --no-build`).
- No extraneous files or unrelated projects modified.

## Execution notes for the Actor
- Work fast and compact.
- Run tests directly and check results.
- Implement exactly one step at a time, verify, commit, and report to `act.md`.
