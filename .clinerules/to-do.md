# To-do — UI/UX Enhancements (New Actionable Demands)

## HIGH Priority (Broken Core Features)

- ~~[x] Task 1 — Fix sticky navbar (currently malformed, fades on scroll)~~
Navbar should remain fixed at top with `sticky-top` class and proper `z-index`; content needs `padding-top` to prevent hiding.
   Done: commit `fe8a1a4`. `sticky-top mb-0` moved to `<header>` (direct child of `<body>`, so the sticky range is no longer trapped in a zero-height wrapper); `mb-3` and the redundant inline `z-index` removed from `<nav>`; `body { padding-top: 56px }` deleted. `dotnet build src/MobileShop.slnx --nologo` → 0 warnings, 0 errors. Manual browser checks confirmed by the owner (bar pins, no gap/jump).

- ~~[x] Task 2 — Live profit calculation (% ↔ $) on Create Phone & Create Apple ID~~
When user enters Profit % OR Profit $, the other field should calculate live based on Price:  
`ProfitAmount = Price * ProfitPercent / 100` and vice versa. Blank/zero handled gracefully.
   Already delivered by commit `5ae227c` — checkbox was stale, carried over from the old stage list. `CreatePhone.cshtml:163-192` and `CreateAppleId.cshtml:65-94` contain byte-identical `updateFromPercent`/`updateFromAmount` handlers bound to `data-price`/`data-percent`/`data-amount`; markup hooks at `CreatePhone.cshtml:36,43,53`. Owner confirmed the feature works as expected in the browser. No new work required; the plan for this task was rejected (REQUIRES REPLANNING) because it proposed adding code that already existed.

- ~~[x] Task 3 — Fix Manual Date Range mode (From/To not working)~~
   Done: commits `cf34904` (fix) + `87f3c68` (verification record). Two defects in how the inputs got their `value`: `asp-format="yyyy-MM-dd"` was passed as a *composite* format string, so with no `{0}` placeholder it emitted the literal `value="yyyy-MM-dd"` that browsers reject (hence the `mm/dd/yyyy` placeholder); and `asp-for` let the empty query-string `ModelState.AttemptedValue` override the server-computed defaults. Replaced with a plain `<input type="date" id/name>` whose value is `Model.From/To.ToString("yyyy-MM-dd", InvariantCulture)`. Evidence: served HTML for `?Mode=Manual` **and** a no-query fresh load both render `value="2026-01-05"` / `value="2026-09-28"`; the `From=&To=` round trip keeps both; `grep -c 'value="yyyy-MM-dd"'` → `0`; build 0 warnings / 0 errors; suite 407 passed, 0 failed, 2 pre-existing skips. Note: the picker *displays* `01/05/2026` (browser locale) — the emitted and submitted value is `yyyy-MM-dd`.
- Date pickers show literal `"mm/dd/yyyy"` template instead of actual dates
- Template format should be `"yyyy-MM-dd"` (HTML5 date input standard)
- **From** should default to earliest transaction/product date in DB
- **To** should default to current date
- Values must populate in the input fields on page load

---

## MEDIUM Priority (Consistency & UX)

- [ ] Task 4 — Color dropdown with "Add New" on Create Phone page
Same pattern as Manufacturer/Model: `<select>` from existing Colors, "Add New" button → modal → POST handler → JSON refresh.

- [ ] Task 5 — Guarantee Corporation dropdown with "Add New" on Create Phone page
Same pattern: select from existing corporations (distinct values from Guarantee.Corporation), "Add New" modal.

- [ ] Task 6 — Auto-refresh on combobox selection (remove Apply button)
When any filter combobox changes (Manufacturer, Model, Color, Guarantee Corp, Preset, etc.), page should refresh automatically via HTMX/fetch — no Apply button needed.

---

## LOW Priority (Cosmetic & Enhancements)

- [ ] Task 7 — Show "N/A" instead of "—" for products without Color
In Products list and Details, display `N/A` when `Color` is null/empty.

- [ ] Task 8 — Add "All" option to Preset combobox in Reports page
After "Year" option, add "All" preset that shows entire date range (earliest transaction to today).

---

**Total: 8 new actionable tasks**  
**Ordering**: High → Medium → Low (within each priority, ordered by user impact)
