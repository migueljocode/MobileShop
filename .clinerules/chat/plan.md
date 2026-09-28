# Plan — Fix Manual Date Range Mode (Task 3 of 8)

## Reviewer Briefing
- **MEDIUM-risk**: JavaScript toggle fix + minor Razor adjustment; no schema changes; repo/service layers already tested and correct
- **HIGH-confidence**: Root cause confirmed — write-back logic exists but only runs in Manual branch; page loads in Automatic mode by default; toggle doesn't submit form; JS placement bug would cause infinite reload

---

## ~~[x] Step 1 — Fix: populate From/To with earliest-date defaults on Manual toggle (Razor + test)~~
- **Files**: 
  - inspect: `src/MobileShop.Web/Pages/Reports/ProfitLoss.cshtml.cs`
  - modify: `src/MobileShop.Web/Pages/Reports/ProfitLoss.cshtml`
  - modify: `src/MobileShop.Tests/Web/Pages/Reports/ProfitLossTests.cs`
  - do not touch: services, repos
- **Symbols**: 
  - `ProfitLossModel.ResolveBoundsAsync()`, `From`, `To`, `EffectiveFrom`, `EffectiveTo`, `EmptyDatabaseNote`
- **Current → Desired**: 
  - **Current**: (a) Write-back only runs in Manual branch → Automatic load has empty From/To. (b) Toggle only toggles visibility, no submit → Manual click shows empty pickers.
  - **Desired**: Load in Automatic mode → `From`/`To` empty. Click **Manual** radio → form submits (GET) with `Mode=Manual` → server computes `EffectiveFrom = earliest transaction date` (or today if none) and `EffectiveTo = today` → write-back populates `From`/`To` → page renders with populated pickers.
- **Change**: 
  1. **In `ProfitLoss.cshtml`**: Add `asp-format="yyyy-MM-dd"` to both date inputs (lines 30-31).
- **Edge cases**: 
  - Empty DB: `earliest == null` → `EffectiveFrom = today` (correct per `.cshtml.cs:46`)
  - User's explicit dates: form submit preserves `From`/`To` query params → `.cshtml.cs:46` preserves them via `From ?? (earliest ?? today)`
- **Tests**: 
  - Add test: `Manual_toggle_populates_From_with_earliest_and_To_with_today` to `ProfitLossTests.cs`
  - Run existing `ProfitLossTests.cs` suite
- **Verify**: `dotnet test src/MobileShop.slnx --nologo --filter "ProfitLossTests"`
- **Done when**: All tests pass; manual check: load page → click Manual → From = earliest transaction date, To = today; no Apply click needed
- **Risk**: LOW | **Confidence**: HIGH

---

## ~~[x] Step 2 — Add form auto-submit on Manual radio toggle (with guard to prevent infinite reload)~~
- **Files**: 
  - modify: `src/MobileShop.Web/Pages/Reports/ProfitLoss.cshtml`
  - do not touch: backend
- **Symbols**: `mode-manual` radio, form (`method="get"`), `updateVisibility()` function
- **Current → Desired**: 
  - **Current**: `updateVisibility()` only toggles `d-none` class → no form submit → defaults never populate on first Manual click
  - **Desired**: Clicking **Manual** radio submits the form (GET) with `Mode=Manual` → server runs Manual branch → defaults populate → page renders with populated dates
- **Change**: 
  1. In `ProfitLoss.cshtml`, add `const form = document.querySelector('form');` at top of `DOMContentLoaded` handler.
  2. Replace the `change` listener registration (lines 63-64) with a Manual-specific handler that calls `updateVisibility()` then submits when Manual is checked:
     ```js
     manualRadio.addEventListener('change', () => {
         updateVisibility();
         if (manualRadio.checked) document.querySelector('form').requestSubmit();
     });
     autoRadio.addEventListener('change', updateVisibility);
     ```
  3. Keep `updateVisibility()` as visibility-only (no submit logic).
  4. Keep `d-none` toggle for UX (show/hide while submit processes).
- **Edge cases**: 
  - Switching back to Automatic: no submit needed (Automatic is default); just hide controls
  - User's explicit dates: form submit preserves `From`/`To` query params → server uses them → write-back preserves them
  - Empty DB: submit triggers Manual branch → `earliest == null` → `From` = today, `EmptyDatabaseNote` shown
  - Apply button: becomes redundant but harmless; Task 6 will remove it
- **Tests**: Manual browser check — click Manual → page reloads → From/To populated; toggle back/forth without infinite reload
- **Verify**: `dotnet build src/MobileShop.slnx --nologo` + manual browser check
- **Done when**: Clicking Manual radio loads page with populated From/To; no infinite reload; explicit dates survive round-trip
- **Risk**: MEDIUM | **Confidence**: HIGH

---

## Global Definition of Done
- Load Reports page in Automatic mode → switch to Manual → From/To immediately populated with earliest transaction date / today
- Dates display in `yyyy-MM-dd` format in pickers
- No infinite reload on mode toggle
- `dotnet build src/MobileShop.slnx --nologo` → 0 errors, 0 warnings
- `dotnet test src/MobileShop.slnx --nologo --filter "ProfitLossTests"` → all pass
- No modifications to services, repos, enums, or other pages

## Execution Notes (for Actor)
- **Razor change only**: Add `asp-format="yyyy-MM-dd"` to both `<input asp-for="From" type="date">` (line 30) and `<input asp-for="To" type="date">` (line 31) in `ProfitLoss.cshtml`. No C# code changes needed — write-back already correct in Manual branch.
- **JS change**: In `ProfitLoss.cshtml`, add `const form = document.querySelector('form');` in `DOMContentLoaded`, then register Manual-specific `change` handler that calls `updateVisibility()` then `form.requestSubmit()` when `manualRadio.checked`. Register `autoRadio.change` → `updateVisibility` only.
- Run: `dotnet test src/MobileShop.slnx --nologo --filter "ProfitLossTests"` then manual browser verification
- Add test to `ProfitLossTests.cs`: `Manual_toggle_populates_From_with_earliest_and_To_with_today`
