# Plan — Stage 23 — Edit-shares modal: aligned, compact amount layout

## Requirements (user-reported)
- The `Edit profit shares` modal looks scrambled: the amount column holds several prices per row (was, now, change) that wrap unpredictably and do not align.
- Make the amounts line up cleanly on a phone-width modal, in light and dark mode, without losing any live-preview information.
- Must not change: any behaviour, the page model, service, calculator, form field names (`Shares[i].EmployeeId`, `Shares[i].SharePercent`, hidden `Mode/Preset/From/To`), the handler, the live-preview maths, or the success/failure flow.

## Facts found in the code (verified)
- The modal (`src/MobileShop.Web/Pages/Reports/ProfitLoss.cshtml`, ~lines 250–300) uses a 3-column `<table class="data-table">` (name · input · amount). `.data-table td` has `var(--space-3) var(--space-4)` padding, so in a ~350 px modal the columns are cramped.
- The amount cell holds `<span data-was> <span data-now>` on one line plus a `data-change` line; `distribution-shares.js` fills them with `formatAmount()` (digits grouped by spaces **plus the suffix ` Toman`**) and a note like `was 40% → 45% · +1 234 Toman`. Long numbers with the repeated suffix wrap mid-line and are not right-aligned to one edge.
- The number input is a full-width `.form-control`, so it competes with the amount column for width.
- There is no CSS at all for the editor: `is-changed` is toggled by the JS but never styled (`grep` finds no `share-`/`is-changed` rule).
- JS hooks (all in `distribution-shares.js`): `[data-share-row]`, `[data-shop-row]`, `[data-share-input]`, `[data-was]`, `[data-now]`, `[data-change]`, `[data-shop-share]`, `[data-summary]`, `[data-reset]`, `[data-save]`; `renderRow()` writes `data-was/now/change` and toggles `is-changed`; `recalc()` and the maths are correct and stay as is.
- Amounts are displayed with the project's existing label `Toman` (`ToToman()` only groups digits and appends the word; it does not convert units). Keep that label; do not change units.
- CSS lives in `src/MobileShop.Web/wwwroot/css/` in `/* BEGIN name-vN */ … /* END name-vN */` blocks; modal rules are in `overlays.css` (`overlays-modal-v1`). Tokens: `--space-1..8`, `--fs-xs/sm`, `--text-1/2`, `--hairline`, `--border-control`, `--accent-soft`, `--accent-strong`, `--danger`, `--danger-soft`, `--radius-*`. Bootstrap's `visually-hidden` is available. The test project has no Razor-render or JS tests.

## Reviewer Briefing
- One step, MEDIUM risk / MEDIUM confidence only because the result is visual and cannot be unit-tested; the logic does not change. The thing to check in Job B is that the JS maths, input names and form fields are untouched and that every hook listed above still exists in the markup.

## ~~[x] Step 1 — Row cards with right-aligned amount lines~~
- Files: inspect: `src/MobileShop.Web/Pages/Reports/ProfitLoss.cshtml` (modal block), `src/MobileShop.Web/wwwroot/js/distribution-shares.js`, `src/MobileShop.Web/wwwroot/css/overlays.css`, `tokens.css`; modify: `ProfitLoss.cshtml` (modal markup only), `distribution-shares.js` (rendering helpers only), `overlays.css` (one new block); do not touch: page model, services, tests, other pages, other CSS blocks.
- Symbols: new CSS block `overlays-share-editor-v1`; classes `.share-editor`, `.share-row`, `.share-row__head`, `.share-row__name`, `.share-row__note`, `.share-row__percent`, `.share-row__amounts`, `.share-row__delta`; JS `renderRow()` and `formatAmount()`.
- Current -> Desired: a 3-column table with a wrapping amount cell → a stack of compact row cards (no `<table>` in the modal), each card with a header line and an aligned amounts list.
- Change:
  1. **Markup** — replace the modal's `<table>` by `<div class="share-editor">` that starts with one muted line `Amounts are in Toman.` Per employee emit `<div class="share-row" data-share-row data-original-percent="@share.SharePercent">` containing:
     - `.share-row__head`: left, `<span class="share-row__name" dir="auto">@share.Name</span>` and under it `<span class="share-row__note small" data-change></span>`; right, a fixed-width input wrapper holding the **unchanged** hidden `Shares[i].EmployeeId` and number input (all existing attributes, `aria-label`, `data-share-input`, the posted-value logic from Stage 22) followed by a `%` sign (`aria-hidden`).
     - `<dl class="share-row__amounts">` with four lines, each a `<dt>`/`<dd>` pair: `Amount` (`dd data-amount`), `Was` (`dd data-was`), `Now` (`dd data-now`), `Change` (`dd class="share-row__delta" data-delta`).
     The Shop row is the same card with `data-shop-row data-original-percent="@shopPercent"`, name `Shop` and, instead of an input, `<span class="share-row__percent num" data-shop-share>@shopPercent%</span>`. Keep `<p role="status" aria-live="polite" data-summary>` and the no-profit line below the list.
  2. **CSS** (new block, tokens only, no animation):
     - `.share-row`: grid, `gap: var(--space-2)`, `padding-block: var(--space-3)`, bottom border `1px solid var(--hairline)`; last row has no border.
     - `.share-row__head`: flex, `align-items: flex-start`, `justify-content: space-between`, `gap: var(--space-3)`; `.share-row__name`: `min-width: 0; overflow-wrap: anywhere; font-weight: var(--fw-strong)`; `.share-row__note`: `color: var(--text-2)`, `display: block`; the input wrapper `flex: none; width: 6.5rem`, inline-flex with the `%`, input `text-align: end`; `.share-row__percent` `flex: none`, end-aligned, same height as the input.
     - `.share-row__amounts`: `display: grid; grid-template-columns: auto 1fr; column-gap: var(--space-4); row-gap: var(--space-1); margin: 0`; `dt` `color: var(--text-2); font-size: var(--fs-sm)`; `dd` `margin: 0; text-align: end; white-space: nowrap; font-variant-numeric: tabular-nums` so every number ends on the same right edge.
     - Visibility: `.share-row:not(.is-changed)` hides the `Was`/`Now`/`Change` pairs; `.share-row.is-changed` hides the `Amount` pair. Wrap each `dt`/`dd` pair in `<div class="share-row__line share-row__line--plain">` (the `Amount` pair) or `<div class="share-row__line share-row__line--changed">` (the `Was`, `Now` and `Change` pairs); `.share-row__line { display: contents }` keeps the pairs on the shared two-column grid, and CSS alone hides the inactive ones (`.share-row:not(.is-changed) .share-row__line--changed, .share-row.is-changed .share-row__line--plain { display: none }`). No JS visibility toggling.
     - `.share-row.is-changed`: `background: var(--accent-soft); border-radius: var(--radius-sm); padding-inline: var(--space-3)` (plus a start-side accent border); `.share-row__delta.is-up` `color: var(--accent-strong)`, `.is-down` `color: var(--danger)`. The `+`/`−` sign stays in the text, so colour is never the only cue. Check contrast in light and dark like Stages 15–16.
     - `.share-editor` body scrolls only through `.modal-dialog-scrollable` (already used); no fixed heights.
  3. **JS** (rendering only): `formatAmount()` returns the grouped digits **without** the ` Toman` suffix (the unit is stated once at the top); `renderRow()` fills `data-amount` (was amount), `data-was`, `data-now` (`—` when the input is invalid), `data-delta` (`formatDelta`, with `is-up`/`is-down` set from the sign and cleared at zero; empty text and no class when the new amount is `null`) and `data-change` with `was 40% → 45%` only (the amount delta now lives in `data-delta`); it still toggles `is-changed` exactly as today. `recalc()`, `floorShare`, `parsePercent`, reset, submit guard and open-on-load stay byte-for-byte the same.
- Depends on: none.
- Edge cases / error handling: names of any length or direction wrap inside their column (`dir="auto"`); amounts up to the largest realistic `long` stay on one line at ~360 px; negative shop amounts keep their `-` sign; a loss period shows 0 for employees; with no active employees the modal is never rendered (button hidden); the posted-value / stored-baseline behaviour from Stage 22 is unchanged.
- Tests: none can cover rendering (no Razor-render or JS harness); the existing `ProfitLossTests` must stay green untouched.
- Verify: CI green. Manual (user, final layout): open Reports → Distribution → Edit shares at ~390 px width, light and dark: amounts of all rows end on one right edge; unchanged rows show a single `Amount` line; changing a percent turns that row into the `Was / Now / Change` layout with the `was 40% → 45%` note and a signed change; the Shop row follows live; over 100 % shows the error and disables Save; Reset and a rejected save (Stage 22 manual check) still behave.
- Done when: no amount wraps or misaligns in the modal and CI is green.
- Risk: MEDIUM
- Confidence: MEDIUM

## Global Definition of Done
- Only `ProfitLoss.cshtml` (modal block), `distribution-shares.js` (rendering helpers) and one new CSS block changed; form field names, hooks used by `recalc()`, maths, handler and Stage 21–22 behaviour are untouched.
- CI green (build + test), with the run number cited by the Reviewer; the user confirms the rendered layout on the final build.
