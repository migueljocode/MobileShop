# Plan — Stage 4 — UI/UX accessibility, responsive layout, empty/loading/error states

## Assumptions (labelled, not facts — Reviewer/user may veto)
- A1: UI language/direction stay English/LTR; Persian data handled with existing `dir="auto"` pattern.
- A2: Dark theme only; Bootstrap 5 stays; no new npm/CDN dependency.
- A3: No visual-regression tooling. CI proves build + tests only; layout/visuals verified by the exact grep checks below + the user's manual check at 360 / 768 / 1280 px.
- A4: Verified from code (not assumptions): skip link + `main#main` exist (`_Layout.cshtml:17,19`); global `:focus-visible` exists but uses hard-coded `rgba(143,160,255,.72)` instead of `var(--focus-ring)` (`app-theme.css:556-563`); `scope="col"` exists on Transactions/Index, ProfitLoss (×2), SecondHand but NOT on Products/Index, People/Index, Customers, Sellers, Details; zero `table-responsive` wrappers; `_EmptyState` used only by Products/Index + SecondHand; `.loading-line` CSS exists but no JS references `aria-busy`/`loading-line`; nav Escape + focus-return already handled (`navigation-menu.js:195-203`); no legacy `#7388ff/#aebdff/#258cfb/radial-gradient` left in css/js/Pages.

## Reviewer Briefing
- Highest risk is Step 4.3 (MEDIUM): touches `person-picker.js`, `product-picker.js`, `product-create-modal.js` fetch flows — success-path behavior must stay byte-identical; Job B must diff that only `aria-busy`/loading/alert lines were added.
- Lowest confidence is Step 4.2 (MEDIUM): sticky `.action-bar` + `table-responsive` + small-screen padding cannot be proven by CI; relies on the listed grep counts + user's 360/768/1280 px manual check.
- Step 4.1 is LOW risk but load-bearing for the stage: the only global CSS behavior change (focus ring to token value) — keep selector list identical, change declarations only.
- Step 4.4 absorbs the audit residual (dashboard `Index.cshtml:43` `text-bg-primary`/`text-bg-dark` badges); it is a markup-or-CSS tidy, not a redesign — no binding or label changes.
- Step 4.5 modifies no production files; its evidence (CI run number + manual checklist in `act.md`) is what the Reviewer signs off from.

## Hard constraints for Stage 4 (unless a step says otherwise)
- Follow `AGENTS.md` + `.agents/project.md`. Apple ID inventory passwords stay plaintext and displayed exactly as today; no auth/cookie/claims boilerplate; never touch `src/MobileShop.Api`, DB init, migrations, `bin`/`obj`.
- Preserve every `id`, `name`, `asp-for`, `asp-page`, `asp-route-*`, `asp-items`, `data-*`, `onchange` and form-field name, and every element the JS files query. Never rename a handler, route or bind property.
- No new JS/CSS frameworks or CDN links. No unrelated refactors (mark OUT OF SCOPE). Use the newest C# syntax available in any Razor `@{ }` code added; usings only via `GlobalUsings.cs`.
- Visuals are not CI-testable: every step's Verify = CI green (cite `Action: #N — Success`) + the listed grep checks + a "Manual check (user)" line recorded in `act.md`.

## Prior stages (done, trimmed)
- ## ~~Stage 1 — Product edit save hardening~~ — Done: single shared `EditIncludes` list; unchanged save returns success with new test.
- ## ~~Stage 2 — UI/UX design system foundation (tokens, fonts, surfaces, component classes, overlays)~~ — Done: `tokens.css` + `components.css` linked in `_Layout.cshtml`; palette/type/radius/focus tokens live, old `--app-*` names aliased. (Audit: signed off.)
- ## ~~Stage 3 — UI/UX page redesign (shell, dashboard, products, forms, transactions, people, reports)~~ — Done: `.page-head`/`.stat-grid`/`.data-table`/`.form-section`/`.action-bar`/`.status-chip` applied; `_EmptyState` partial created and used on Products/Index + SecondHand; chart reads CSS tokens. (Audit: signed off, `Action: #614 — Success`.)

---

## ~~[x] Step 4.1 — Focus ring, table semantics, icon-only labels~~
- Files: inspect: `src/MobileShop.Web/wwwroot/css/app-theme.css` (lines ~556-570), `Pages/Products/Index.cshtml` (thead ~119-127), `Pages/People/Index.cshtml` (~64-65), `Pages/People/Customers.cshtml` (12-17), `Pages/People/Sellers.cshtml` (12-17), `Pages/Products/Details.cshtml` (~140), `Pages/Products/CreateGlass.cshtml` (115,123,131), `wwwroot/js/navigation-menu.js` (132-203); modify: `app-theme.css` (focus rules only), the 5 table files above + `Details.cshtml`; do not touch: any selector list, dock geometry, `.cshtml.cs`, picker JS (Escape already handled — verify only).
- Symbols: CSS `:focus-visible` rule; `<th scope="col">` / `scope="row"`; `aria-label` on icon-only `.btn-close`.
- Current -> Desired: `:focus-visible` uses hard-coded `outline: 3px solid rgba(143,160,255,.72)`; 5 tables use bare `<th>` and bare empty actions `<th></th>`; `Details.cshtml` `<table class="table">` has no scope; 3 `btn-close` in `CreateGlass.cshtml` lack `aria-label="Close"`. Desired: every keyboard focus shows `box-shadow: var(--focus-ring)`; every data table has `scope="col"` (row headers `scope="row"` if any), empty actions header reads `<th scope="col"><span class="visually-hidden">Actions</span></th>`; every icon-only button has an accessible name.
- Change: (1) In `app-theme.css`, keep the existing `:focus-visible` selector list exactly, replace declarations with `outline: none; box-shadow: var(--focus-ring);` and keep the `:focus` fallback rule using `var(--focus-ring)` (delete the hard-coded `rgba` line). (2) Add `scope="col"` to each `<th>` in Products/Index, People/Index, Customers, Sellers, Details; convert each empty actions `<th></th>` to the visually-hidden-Actions form. (3) Add `aria-label="Close"` to the 3 `btn-close` buttons in `CreateGlass.cshtml` (attribute only). (4) Verify (read-only) that `navigation-menu.js` Escape-closes the flyout and refocuses the trigger; if present, change nothing there.
- Depends on: none.
- Edge cases / error handling: do not change specificity or selector order; do not add `tabindex`; `visually-hidden` is Bootstrap's class — do not redefine it.
- Tests: none added (markup/CSS only; Web tests are PageModel-only — none should change; if one fails, stop and report instead of editing it).
- Verify: CI green, cite run. `grep -rn "143, 160, 255\|143,160,255" src/MobileShop.Web/wwwroot/css` → no match. `grep -rn "<th>\|<th></th>" src/MobileShop.Web/Pages` → no match. `grep -rn 'btn-close" data-bs-dismiss' src/MobileShop.Web/Pages | grep -v 'aria-label'` → no match.
- Done when: focus ring is token-driven, all data-table headers scoped, all icon-only closes labelled, CI green, counts recorded in `act.md`.
- Risk: LOW
- Confidence: MEDIUM (ring visibility needs human check)

## ~~[x] Step 4.2 — Responsive tables and small-screen layout~~
- Files: inspect: `src/MobileShop.Web/wwwroot/css/components.css`, `app-theme.css` (main padding), `navigation-menu.css` (dock sizing, read-only); modify: `components.css` (+ `app-theme.css` main-padding rule only), the 8 `<table class="data-table">` pages (`Products/Index`, `Products/SecondHand`, `Products/Details` only if its table becomes `data-table` — otherwise wrap as-is, `People/Index`, `People/Customers`, `People/Sellers`, `Transactions/Index`, `Reports/ProfitLoss` ×2 tables); do not touch: dock geometry values, `_Layout.cshtml`, any `asp-route-*`/`onchange`/`data-*`.
- Symbols: `.table-responsive` (Bootstrap), `.toolbar`, `.page-head__actions`, `.action-bar`, `.has-floating-navigation`.
- Current -> Desired: zero `table-responsive` wrappers; small screens keep desktop padding/radius; `.action-bar` is static. Desired: tables scroll horizontally inside their container on narrow screens; `<576px` main panel padding `1rem`, radius `0`, no side border; `.toolbar` children stack full-width; `.page-head__actions` full width; `.action-bar` sticky above the nav dock without covering content.
- Change: (1) Wrap each `<table class="data-table">` (and the Details `<table class="table">`) in `<div class="table-responsive">` — wrapper only, no table-attribute changes. (2) In CSS add: `@media (max-width: 575.98px)` → main panel padding `1rem`, `border-radius: 0`, side borders removed; `.toolbar > * { flex: 1 1 100%; }`; `.page-head__actions { width: 100%; }`. (3) `.action-bar { position: sticky; bottom: 0; background: var(--panel); padding-bottom: calc(1rem + env(safe-area-inset-bottom)); z-index: 10; }` with `@media (max-width: 767.98px)` override `bottom: var(--nav-dock-height, 5rem)` when the dock is present; add `.has-floating-navigation { padding-bottom: calc(var(--nav-dock-height, 5rem) + 1rem); }` and define `--nav-dock-height` in `tokens.css` from the real dock height read in `navigation-menu.css` (token definition only — do not restyle the dock).
- Depends on: Step 4.1 (table headers must be scoped before wrapping).
- Edge cases / error handling: sticky bar must not cover the dock or form buttons; horizontal scroll must apply to the wrapper, never cause whole-page sideways scroll; keep `env(safe-area-inset-bottom)` for phones with gesture bars.
- Tests: none added (same PageModel-only constraint as 4.1).
- Verify: CI green, cite run. `grep -rln "<table" src/MobileShop.Web/Pages --include="*.cshtml" | wc -l` ≤ `grep -rln "table-responsive" src/MobileShop.Web/Pages --include="*.cshtml" | wc -l` (every page containing a table contains a wrapper). Manual check (user, recorded in `act.md`): 360 / 768 / 1280 px on Products, Sell, one Edit — no sideways page scroll, actions reachable above the dock.
- Done when: all tables wrapped, small-screen rules in place, manual widths checked, CI green.
- Risk: LOW
- Confidence: MEDIUM (real-device widths need human check)

## ~~[x] Step 4.3 — Empty, loading, and error states~~
- Files: inspect: `Pages/People/Index.cshtml:59`, `Pages/Transactions/Index.cshtml:65`, `Pages/Index.cshtml:31`, `Pages/Reports/ProfitLoss.cshtml` (empty branches), `Pages/Shared/_EmptyState.cshtml`, `wwwroot/js/person-picker.js` (~52-121), `product-picker.js` (~109-219), `product-create-modal.js` (~41-167), pages rendering `_ProductCreateSuccess.cshtml` (role check only); modify: the plain-`<p>` empty branches + the 3 JS files; do not touch: request URLs, payloads, selection behavior, `_ProductCreateSuccess.cshtml` (already `role="status"`), any `asp-*`/`data-*`/`id`.
- Symbols: `_EmptyState` partial `@model (string Title, string? Hint)`; `aria-busy="true"`; `.loading-line`; `.alert.alert-danger` with `role="alert"`.
- Current -> Desired: People/Index ("No people found."), Transactions/Index ("No transactions found."), dashboard ("No transactions yet.") and any ProfitLoss empty branch render plain `<p class="text-muted">`; picker/modal fetches fail silently or console-only with no busy/loading/error UI. Desired: every empty list uses `_EmptyState` with page-specific copy; every async fetch shows busy + loading + failure states without changing success behavior.
- Change: (1) Replace each plain empty `<p>` with `<partial name="_EmptyState" model='("<what is empty>", "<next action>")' />` — specific copy per page (e.g. `("No customers yet", "Create the first customer to start selling.")`, `("No transactions found", "Record the first sale from Transactions / Sell.")`, ProfitLoss keeps its computed numbers, only its empty branch changes). (2) In each of the 3 JS files: before `fetch`, set `aria-busy="true"` on the results container and insert one `<div class="loading-line">Loading…</div>`; on failure replace results content with `<div class="alert alert-danger" role="alert">Couldn't load results. Try again.</div>`; on completion (success or failure) clear the loading line and remove `aria-busy`. Nothing else in the fetch path changes. (3) Inline alerts: only alerts lacking a role gain one (`role="status"` success, `role="alert"` error; attribute only).
- Depends on: none (parallelizable with 4.1/4.2; do not combine into one commit).
- Edge cases / error handling: first read each script's current error path and preserve successful-path behavior byte for byte; loading node must be removed even when the request throws; never leave `aria-busy="true"` stuck after completion.
- Tests: none added (JS has no harness; C# suite must stay green).
- Verify: CI green, cite run. `grep -rn "No people found\|No transactions found\|No transactions yet" src/MobileShop.Web/Pages` → no match outside `_EmptyState.cshtml`. `grep -c "aria-busy" src/MobileShop.Web/wwwroot/js/person-picker.js src/MobileShop.Web/wwwroot/js/product-picker.js src/MobileShop.Web/wwwroot/js/product-create-modal.js` → ≥1 in each file. `grep -rn 'class="alert' src/MobileShop.Web/Pages --include="*.cshtml" | grep -v 'role='` → no match.
- Done when: all empty lists use the partial with specific copy, all three scripts show busy/loading/error and clean up, CI green.
- Risk: MEDIUM (fetch-path edits in untested JS; mitigated by diff-only-added-lines review)
- Confidence: MEDIUM

## ~~[x] Step 4.4 — Motion, colour, and dashboard-badge audit (carries audit residual)~~
- Files: inspect + modify: `src/MobileShop.Web/wwwroot/css/app-theme.css`, `components.css`, `navigation-menu.css`, `Pages/Index.cshtml:43`; do not touch: bindings, labels, order, `.cshtml.cs`, JS logic, tokens' hex values.
- Symbols: `@media (prefers-reduced-motion: reduce)` blocks; `.text-bg-primary` / `.text-bg-dark` badges on `Index.cshtml:43`; `.status-chip` variants.
- Current -> Desired: residual from audit — dashboard transaction badges still `text-bg-primary`/`text-bg-dark` (Step 3.1 asked for none left on that page); possible transitions/animations outside reduced-motion coverage; possible hex literals outside `tokens.css`. Desired: every animation respects reduced motion; no legacy hard-coded colours; dashboard badges match the design system; any remaining hex outside `tokens.css` is justified with a contrast ratio.
- Change: (1) Extend (don't duplicate) the existing `@media (prefers-reduced-motion: reduce)` block(s) so every `transition`/`animation` in the three CSS files is disabled under it. (2) Fix `Index.cshtml:43`: replace `text-bg-primary`/`text-bg-dark` with the design-system equivalent already used for availability — `<span class="status-chip status-chip--@(transaction.Direction == TransactionDirection.Buy ? "available" : "sold")">` keeping the same `transaction.Direction` condition and inner text; if the badge carries meaning beyond buy/sell, use `--used` only with Reviewer agreement (default: available/sold as above). (3) Replace any remaining hard-coded colour with the matching token; list any hex that must stay outside `tokens.css` in `act.md` with its contrast ratio (≥ 4.5:1 text, ≥ 3:1 control borders).
- Depends on: Steps 4.1–4.3 (audit runs last over their output).
- Edge cases / error handling: keep badge text and condition identical; do not invent new token values — reuse `--accent`/`--ink-muted`/`--warn` mappings from Stage 2.
- Tests: none added.
- Verify: CI green, cite run. `grep -rn "#7388ff\|#aebdff\|#258cfb\|rgba(115, 136, 255\|radial-gradient\|text-bg-" src/MobileShop.Web/wwwroot/css src/MobileShop.Web/wwwroot/js src/MobileShop.Web/Pages` → no match. `grep -rn "#[0-9A-Fa-f]\\{3,6\\}\\b" src/MobileShop.Web/wwwroot/css --include="*.css" | grep -v "tokens.css"` → empty, or every hit listed in `act.md` with a ratio.
- Done when: reduced-motion covers all motion, no legacy colours/badges, exceptions (if any) justified with ratios, CI green.
- Risk: LOW
- Confidence: HIGH

## ~~[x] Step 4.5 — Final validation (whole roadmap)~~
- Files: none modified (evidence recorded in `act.md` only: `Action: #N — Success` + the checklist below). Do not touch production code in this step.
- Symbols: none (process step).
- Current -> Desired: Stage 4 steps individually green but no whole-roadmap sign-off. Desired: full CI green on the final commit with run number recorded, plus the manual checklist filled in `act.md` for the Reviewer's sign-off (Reviewer does not re-run anything).
- Change: confirm the last commit's full-suite CI run is green; record `Action: #<run_number> — Success` in `act.md`; fill the checklist: (a) skip link moves focus to `#main`; (b) focus ring visible on nav, filters, table links, forms, modals; (c) keyboard-only path Dashboard → Products → Details → Edit → Save completes; (d) 360 / 768 / 1280 px checked on every page group (Dashboard, Products, Details/Edit, Create, Transactions/Sell, People, ProfitLoss, Login, Error/Privacy); (e) one empty, one loading, one error state each seen; (f) `prefers-reduced-motion` emulation shows no animation. Also refresh `act.md` past its stale Stage-1-only state: one line per Stage 4 step with its commit SHA + CI run number (this also clears audit residual #2; hook-count tables from Stages 2–3 stay unrecorded per audit residual #3 — do not retrofit them).
- Depends on: Steps 4.1–4.4 all PASS (Job B each).
- Edge cases / error handling: if CI is unobservable, report `Action: unavailable` + Pending and wait — never claim success from pending/interrupted runs.
- Tests: full suite via CI (no local `dotnet build`/`test` unless CI unavailable).
- Verify: `GET /repos/{owner}/{repo}/actions/runs?head_sha={sha}` shows the final commit's run with `conclusion: success`; quote its human-visible `run_number` (never run ID/SHA). `git diff --stat` for this step shows `act.md` only.
- Done when: evidence + checklist in `act.md`, CI green quoted, no unresolved CRITICAL/HIGH findings.
- Risk: LOW
- Confidence: HIGH

## Global Definition of Done
- Focus: every interactive element shows `var(--focus-ring)` on keyboard focus; all data tables have `scope="col"`; all icon-only buttons named; nav Escape/focus-return verified.
- Responsive: every table in a `.table-responsive` wrapper; small-screen padding/radius/stack/sticky-bar rules in place; 360/768/1280 px manually checked with no sideways page scroll and actions clear of the dock.
- States: every empty list uses `_EmptyState` with specific next-action copy; all three picker/modal scripts set/clear `aria-busy` + `.loading-line` and show `role="alert"` on failure; all inline alerts carry the correct role.
- Audit residuals cleared: dashboard `text-bg-*` badges replaced (4.4); `act.md` refreshed with per-step SHA + run numbers (4.5); no legacy hard-coded colours; reduced-motion covers all motion.
- Constraints kept: every `id`/`name`/`asp-*`/`data-*`/`onchange` hook preserved; no change to Api, auth, migrations, DB init, Apple ID plaintext handling, `IBaseRepo`/`BaseRepo`; no new frameworks/CDN; no unrelated refactors.
- CI green on the final commit with `Action: #N — Success` recorded in `act.md`; no unresolved CRITICAL/HIGH findings.
