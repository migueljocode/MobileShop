# Plan — Stage 7 — Liquid Glass foundation: design tokens, Bootstrap dark mode, dead-CSS cleanup (no layout change)

## Proposed stages
(New phase: Apple-inspired Liquid Glass UI/UX redesign. These exact lines go into `.agents/TODO.md` only after the Reviewer approves them; then delete this section. Stage 7 is planned below; Stages 8-16 are planned one at a time when they become active, from the task catalogue in R18. The Actor must not start while this section exists.)
- [ ] Stage 7 — Liquid Glass foundation: design tokens, Bootstrap dark mode, dead-CSS cleanup (no layout change)
- [ ] Stage 8 — Core controls: buttons, fields, selects, checks, validation states, label association
- [ ] Stage 9 — Content surfaces: canvas shell, cards, tables, chips, alerts, empty and loading states
- [ ] Stage 10 — Overlays and pickers: glass modals and menus, scrim, picker keyboard support, Jalali date picker
- [ ] Stage 11 — Navigation shell: dock token migration, flyout grouping, focus clearance, action bar, mobile safe areas
- [ ] Stage 12 — Pages A: Login, Profile, Dashboard, People
- [ ] Stage 13 — Pages B: Products list, details, edit and the 11 create forms
- [ ] Stage 14 — Pages C: Transactions, Reports, Error and Privacy
- [ ] Stage 15 — Motion and preference modes: motion tokens, press feedback, reduced motion/transparency/contrast, solid-surfaces switch
- [ ] Stage 16 — QA and handoff: browser harness decision, baselines, accessibility sweep, performance, final audit

## Pipeline note
- Role: Planner. Planning only. No production file was changed, `TODO.md` is untouched, nothing was committed (the owner commits plan.md).
- Source of requirements: the owner's "Comprehensive Website UI/UX Redesign Plan (Apple-inspired Liquid Glass)" mission, which asks for the audit, research, design system and roadmap in this single file.
- CONFLICT TO APPROVE (AGENTS.md: report, never invent): `roles/planner.md` says plan.md covers the active stage only and is overwritten per stage; the mission asks for one authoritative file holding the whole audit, spec and roadmap. Resolution used: the actionable Stage 7 block is first and follows the standard template; everything long-lived sits under `# Redesign Reference` (sections R1-R24) and is marked non-actionable. Rule for later planners: replace the step blocks per stage, keep `# Redesign Reference` and update its status lines. If the owner prefers the strict rule, move the Reference elsewhere and trim this file to Stage 7 (decision D-01).
- Stage 6 history was trimmed per the planner rule; what still constrains new work (navigation invariants, known limits) is kept in R24.
- Evidence tags in the Reference: [S] source read, [G] grep count, [C] computed by script, [CI] GitHub Actions API, [W] web research, [A] assumption, [U] unverified, needs a browser or device.

## Reviewer Briefing
- New phase, ten proposed stages (dependency order 7 -> 16). Please judge the stage list, the scope of Stage 7 and the D-01 conflict above.
- Stage 7 is deliberately small and low risk: three steps plus validation. 7.1 only appends inert tokens; 7.3 removes two dead rule groups. 7.2 (`data-bs-theme="dark"`) is the only step with visible effect (MEDIUM risk): it fixes four static findings (select chevron F-12, modal close X F-13, bright person-picker hover F-44, `<code>` colour) and nothing else is expected to change.
- The scoped template stylesheet (`_Layout.cshtml.css`) is NOT removed in Stage 7 on purpose: by specificity it still overrides app-theme for layout-level elements (footer position and line-height, Privacy link colour, F-45). Removing it changes the footer layout, so it is task SHELL-002 in Stage 11 with a browser check.
- Evidence limits: static source reading, greps, computed contrast and GitHub Actions data only. The sandbox has no .NET SDK, no browser and no NuGet access, so no screenshots or measurements exist; visual claims are tagged [U] and listed in the manual check of Step 7.4.
- No dependency is added in Stage 7. Later optional test-only packages (Playwright, axe) and the font licence question need owner approval (R17, R22).
- Items that need an owner decision before Stage 8+: R22 Q-01 .. Q-10 (stage list, SF Pro Rounded licence, light theme, canvas shell, auto-submit filters, browser targets, test dependencies).

## Plan map (for a reader with only this file)
- Actor: do the Stage 7 steps below, in order. Tokens come from the block between the `BEGIN tokens-v1` and `END tokens-v1` marker lines in R11.A (extract with the exact `sed` in Step 7.1).
- Reviewer: Job A reads this section, the briefing and the steps; Job B checks one step against its commit.
- Everyone: the Reference holds the audit (R4-R7), research (R8), design system (R9-R13), per-route and per-component specs (R14-R15), UX changes (R16), dependencies (R17), task catalogue (R18), verification (R19), decisions/risks/questions (R20-R22), coverage gaps (R23) and Stage 5/6 carry-over (R24).
- Find things fast: `grep -n "^## \[ \] Step" .agents/chat/plan.md` and `grep -n "^## R[0-9]" .agents/chat/plan.md`.

## Hard constraints (Stage 7)
- Follow `AGENTS.md` and `.agents/project.md`. Touch only `src/MobileShop.Web/wwwroot/css/tokens.css`, `src/MobileShop.Web/Pages/Shared/_Layout.cshtml` and `src/MobileShop.Web/wwwroot/css/site.css`. Do not touch `navigation-menu.css/.js` (Stage 5/6 invariants, R24), `app-theme.css`, `components.css`, `_Layout.cshtml.css`, any other view or script.
- No new dependency, no new file, no hex colour literal added, no `!important` added, no behaviour change. One step per pass and per commit.
- Preserve each file's existing encoding and line endings (`_Layout.cshtml` has a UTF-8 BOM and LF).

## [ ] Step 7.1 — Add the design-token layer (additive and inert)
- Task: FOUND-001 · Priority P0 · Effort S · Mandatory.
- Files: modify: `src/MobileShop.Web/wwwroot/css/tokens.css`; do not touch: every other file.
- Symbols: a second `:root` block plus one `@supports not (transition-timing-function: linear(0, 1))` block appended after the existing `:root` block.
- Current -> Desired: `tokens.css` has three `@font-face` rules and one `:root` with the legacy tokens (`--ground`, `--panel`, `--panel-raised`, `--line`, `--line-strong`, `--ink`, `--ink-muted`, `--accent*`, `--warn*`, `--danger*`, `--r-*`, `--fs-*`, `--focus-ring`, `--nav-dock-height`, `--font-body`). Desired: the same file plus the `tokens-v1` block (new names only, all derived from the legacy tokens; nothing consumes them yet).
- Change: from the repo root run exactly
  `{ printf '\n'; sed -n '/^\/\* BEGIN tokens-v1 \*\/$/,/^\/\* END tokens-v1 \*\/$/p' .agents/chat/plan.md; } >> src/MobileShop.Web/wwwroot/css/tokens.css`
  This appends a blank line and the whole block (both marker comment lines included) at the end of the file. Do not edit, reorder or remove any existing line, and do not retype the block by hand.
- Depends on: none.
- Edge cases / error handling: the file must still end with exactly one newline; no existing token name may be redefined (checked below); the block contains no hex literal (colours derive from `var()` and `color-mix()`); the `--ease-spring` fallback for browsers without `linear()` is inside the block; nothing references the new tokens, so every page must render exactly as before.
- Tests: none (no CSS test infrastructure).
- Verify: CI green (cite `Action: #<run>`). `git diff --numstat src/MobileShop.Web/wwwroot/css/tokens.css` shows 0 deleted lines; `grep -c "BEGIN tokens-v1" src/MobileShop.Web/wwwroot/css/tokens.css` = 1 and `grep -c "END tokens-v1" …` = 1; `sed -n '/BEGIN tokens-v1/,/END tokens-v1/p' src/MobileShop.Web/wwwroot/css/tokens.css | grep -cE '#[0-9A-Fa-f]{3,8}\b'` = 0; `grep -oE '^\s+--[a-z0-9-]+:' src/MobileShop.Web/wwwroot/css/tokens.css | sort | uniq -d` prints nothing; the same `sed` range piped to `grep -cE '^\s+--'` = 75; `git diff --stat` = `tokens.css` only.
- Done when: the block is appended verbatim, the checks above hold, CI green.
- Risk: LOW
- Confidence: HIGH

## [ ] Step 7.2 — Bootstrap dark mode flag and colour-scheme meta
- Task: FOUND-002 · Priority P0 (fixes F-12, F-13, F-44) · Effort S · Mandatory.
- Files: modify: `src/MobileShop.Web/Pages/Shared/_Layout.cshtml`; do not touch: everything else.
- Symbols: the `<html>` start tag and the `<meta name="viewport" …>` line in `_Layout.cshtml`.
- Current -> Desired: `<html lang="en">`, no `data-bs-theme`, no colour-scheme meta. Bootstrap therefore applies its LIGHT defaults to anything the app CSS does not override: the select chevron stroke is `#343a40` (1.62:1 on the page ground, F-12), `.btn-close` is a black glyph (29 uses, F-13), `.list-group-item-action:hover/focus` in the person picker uses a bright `#f8f9fa` background (F-44, source analysis), `code` is `#d63384`. Desired: `<html lang="en" data-bs-theme="dark">` plus `<meta name="color-scheme" content="dark" />`.
- Change: in `_Layout.cshtml` (a) replace `<html lang="en">` with `<html lang="en" data-bs-theme="dark">`; (b) add a new line `    <meta name="color-scheme" content="dark" />` directly after the `<meta name="viewport" … />` line (same 4-space indent). No other line changes.
- Depends on: none (independent of 7.1).
- Edge cases / error handling: the app already overrides the Bootstrap variables it cares about at `:root` in `app-theme.css`, which loads after Bootstrap with equal specificity, so those overrides keep winning (`--bs-body-bg`, `--bs-body-color`, `--bs-secondary-color`, `--bs-tertiary-color`, `--bs-border-color`, `--bs-link-color`, `--bs-link-hover-color`). Variables it does not override switch to Bootstrap's dark values; the only visible ones (R4 F-12, F-13, F-44) are intended. Keep the UTF-8 BOM (check below). Do not add `data-bs-theme` anywhere else.
- Tests: none.
- Verify: CI green. `grep -c 'data-bs-theme="dark"' src/MobileShop.Web/Pages/Shared/_Layout.cshtml` = 1; `grep -c 'name="color-scheme"' …` = 1; `head -c3 src/MobileShop.Web/Pages/Shared/_Layout.cshtml | od -An -tx1` prints ` ef bb bf` (BOM kept); `git diff --numstat` for the file = `2 1` (two lines added, one replaced); `git diff --stat` = `_Layout.cshtml` only.
- Done when: both edits are in, BOM preserved, CI green.
- Risk: MEDIUM (a global attribute: Bootstrap's dark variables apply to every page; the CI cannot see CSS, so the visual change list in Step 7.4 is checked by the owner)
- Confidence: MEDIUM

## [~] Step 7.3 — Remove dead rules from site.css (OUT OF SCOPE)
- Task: FOUND-003 · Priority P2 · Effort S · Optional hygiene (zero visual effect).
- Status: OUT OF SCOPE per AGENTS.md minimal-change rule and Reviewer Job A MEDIUM note — unrelated cleanup deferred; site.css untouched in Stage 7.
- Files: none modified (`site.css` left intact; `app-theme.css` not touched).
- Symbols: the first rule `.btn:focus, .btn:active:focus, .btn-link.nav-link:focus, .form-control:focus, .form-check-input:focus { … }` and the last two rules `.form-floating > …::placeholder` and `.form-floating > …:focus::placeholder`.
- Current -> Desired: the focus-ring rule is duplicated verbatim in `app-theme.css` (same specificity, loaded later, so the site.css copy never decides anything); `.form-floating` has zero uses in the markup (grep below). Desired: both removed; everything else in `site.css` (the two `@font-face`, `:root`, `html` rules, `body` rule) stays.
- Change: with an exact-text edit tool remove (1) the block `.btn:focus, .btn:active:focus, .btn-link.nav-link:focus, .form-control:focus, .form-check-input:focus {` / `  box-shadow: var(--focus-ring);` / `}` and the blank line after it (so `html {` follows the media query directly); (2) the final two `.form-floating …` rules and the blank lines before them, leaving the `body { margin-bottom: 60px; }` rule as the last rule, then one newline at end of file.
- Depends on: none.
- Edge cases / error handling: first confirm `grep -rn "form-floating" src --include=*.cshtml --include=*.js` finds nothing; if it finds a use, stop and report. Do not touch the `html { position: relative; min-height: 100%; }` or `body { margin-bottom: 60px; }` rules.
- Tests: none.
- Verify: CI green. `grep -c "form-floating" src/MobileShop.Web/wwwroot/css/site.css` = 0; `grep -c "focus-ring" …site.css` = 0; `grep -c "btn:active:focus" src/MobileShop.Web/wwwroot/css/app-theme.css` = 1 (the surviving copy); `git diff --numstat` for site.css shows only deleted lines (added = 0); `git diff --stat` = `site.css` only.
- Done when: the three rules are gone, nothing else changed, CI green.
- Risk: LOW
- Confidence: HIGH

## [ ] Step 7.4 — Stage 7 validation
- Task: FOUND-004 · Mandatory.
- Files: none modified.
- Change: confirm CI green on the last code commit and record `Action: #<run>` per code step in act.md; record the greps of Steps 7.1-7.3; `git diff --stat` over the stage is exactly `tokens.css`, `_Layout.cshtml`, `site.css` (+ `.agents/` state files); `git diff --stat` shows NO change in `navigation-menu.css` / `navigation-menu.js`. No browser measurements are required (no harness).
- Manual check (owner, hard refresh first): (1) a page with a select (Products filters, People filters): the chevron is light and clearly visible. (2) Products -> Create phone -> any "Add New": the modal's close X is light and visible. (3) Record sell -> Customer search: hovering or focusing a result row gives a dark highlight, not a bright one. (4) Products -> Details of an Apple ID: the password `code` text is lighter pink than before. (5) Dashboard, Products list, Login, a create form and the dock look otherwise identical to before. (6) Repeat 1-3 in Safari and Firefox. Anything else that changed is a defect to report.
- Done when: evidence recorded.
- Risk: LOW
- Confidence: HIGH

## Global Definition of Done
- `tokens.css` contains the `tokens-v1` block verbatim and no existing line changed; `_Layout.cshtml` has `data-bs-theme="dark"` and the colour-scheme meta with the BOM intact; `site.css` has the dead rules removed.
- Only those three files changed (plus `.agents/` state); `navigation-menu.css/.js` untouched; no new dependency, file or hex literal.
- CI green on the final code commit with run numbers in act.md; the owner's manual check (Step 7.4) done in Chromium plus Safari and Firefox; no unresolved CRITICAL/HIGH finding.


---

# Redesign Reference
Long-lived, non-actionable. Steps cite these sections by ID. Status of planning work: DISC and RES tasks done (R18); everything else open. Nothing in this Reference has been implemented.

## R1 Executive summary
- Assessment: a coherent dark UI with a real token file, a small shared component layer and one well-engineered glass surface (the Stage 5/6 floating dock). Gaps: partial token coverage (surface steps only ~1.1:1, radius and colour literals, two table idioms, three card idioms), Bootstrap light defaults leaking into the dark theme (select chevron, modal close X, one bright hover), 111 unassociated form labels, custom comboboxes and date picker without full keyboard support, opaque overlays, a flat canvas that gives glass nothing to blur, and a footer/link still styled by .NET template CSS.
- Direction: "quiet content, glass chrome". Opaque, low-noise content surfaces; Regular glass only on the navigation layer (dock, flyout) and overlay sheets (modal, menu, picker, date picker); one static ambient background so glass has something to sample; concentric radii; elevation by shadow plus edge highlight; token-driven motion; every effect has a solid fallback and an in-app solid-surfaces switch because Safari and Firefox lack `prefers-reduced-transparency`.
- Stack decision: stay on .NET 10 Razor Pages + Bootstrap 5.3.3 + vanilla CSS/JS. No runtime dependency is added. The React/shadcn/Radix/Motion candidates do not fit the stack (R17).
- Scale: 33 routable pages (32 render UI), 19 shared partials, 1 view component, 5 stylesheets (+1 scoped template stylesheet), 8 scripts, 29 Bootstrap modals; 10 stages, 68 catalogued tasks (R18); 45 audit findings (R4).
- First milestone: Stage 7 (tokens, dark-mode flag, dead-CSS cleanup), then Stage 8 (controls and label association).

## R2 Scope, assumptions and constraints
- In scope: all web UI under `src/MobileShop.Web` (Razor Pages, partials, view component, CSS, JS). Out of scope: `MobileShop.Api`, API stubs, the QuestPDF factor/invoice rendering, authentication logic, data and money handling, Apple ID plaintext password handling (project.md non-negotiables).
- Constraints from the repo: CI is the only build/test gate; minimal change; project-wide usings; no new dependency without need; no generated output edits.
- Assumptions [A]: UI language stays English LTR (`lang="en"`; Persian only as data via `dir="auto"`); browser targets are the last two major versions of Chrome, Edge, Safari (macOS and iOS) and Firefox (README documents none); the app is used on a desktop at the shop and sometimes on a phone or tablet; dark is the primary theme.
- Brand to preserve: ground `#0E1318`, panel `#151C23`, raised `#1C252E`, teal accent `#36C5B0`, warn `#E5A94B`, danger `#F0707A`, SF Pro Rounded + Vazirmatn type stack, the floating dock. No new hue is introduced; success and info reuse the accent by design (primary and success buttons are already the same teal).
- Non-goals: copying Apple assets or presenting the site as an Apple product; native-exact rendering (lensing, system materials, device-motion lighting are not reproducible in a browser; R10 names the closest web approximation).

## R3 Verified inventory
### Stack and assets [S][G]
| Item | Facts |
|---|---|
| Framework | .NET 10 Razor Pages (`MobileShop.Web`, 180 tracked files), layered solution `src/MobileShop.slnx`; static assets via `MapStaticAssets()`; `asp-append-version` on app CSS/JS |
| Front-end libs | Bootstrap 5.3.3 (css 232,804 B min, bundle js 80,722 B), jQuery (87,533 B), jquery-validation + unobtrusive, Chart.js 4.5.1 (208,522 B, ProfitLoss only) |
| App CSS (43,458 B total) | `tokens.css` 51 lines, `site.css` 53, `app-theme.css` 1049, `components.css` 256, `navigation-menu.css` 545, scoped `_Layout.cshtml.css` 48 (template leftovers). Load order: bootstrap, tokens, site, app-theme, components, navigation-menu, then `MobileShop.Web.styles.css` (last, wins ties) |
| App JS (74,843 B total) | `site.js` 114, `navigation-menu.js` 273, `jalali-datepicker.js` 421, `person-picker.js` 162, `product-picker.js` 314, `product-create-modal.js` 197, `profit-loss-chart.js` 191, `create-product-pricing.js` 98 |
| Fonts | SF Pro Rounded 400/700 only (526,788 + 485,268 B), Vazirmatn 400/500/700 (~269 KB each); `font-display: swap`, no preload |
| Icons | inline SVG (nav sprite of 17 symbols; login/password-eye SVG duplicated 4 times); no icon library, no `<img>` |
| Theme | dark only (`color-scheme: dark`), no `prefers-color-scheme`, no theme switch; Bootstrap `data-bs-theme` unset |
| Tests | 45 tracked files; 16 under `Web/Pages/*` test page models only; no browser, markup or JS tests |
| CI | `.github/workflows/dotnet.yml` build+test on ubuntu-24.04 (.NET 10) plus a log-utility matrix; ignores `**.md`, `.agents/**`, `.kilo/**`, `docs/**`. Last code run #618 success on `597fba4` [CI] |

### Routable pages (33; 31 by plain grep plus Error and Privacy, whose first line has a BOM) [S][G]
Account: Login, Logout (confirm form), Profile · Dashboard: Index · People: Index, Customers, Sellers, CustomerDetails, SellerDetails, CreateCustomer, CreateSeller · Products: Index, SecondHand, Details, Edit, CreateAppleId/Cable/Case/Charger/Glass/Laptop/Phone/PortableStorage/PowerBank/SmartWatch/Tablet (11), PersonPicker (handler only, no markup) · Transactions: Index, Sell, Details · Reports: ProfitLoss · Error, Privacy.

### Shared UI units [S]
`_Layout` (head, skip link, container/main, footer, dock invocation, scripts) · `_EmptyState` (title + hint) · `_PersonPicker` (customer/seller search combobox with a modal) · `_ProductPickerOptions`, `_ProductSellerPicker` · `_ProductCreate{AppleId,Cable,Case,Charger,Glass,Laptop,Phone,PortableStorage,PowerBank,SmartWatch,Tablet}Form` (11 partials, loaded into modals by `product-create-modal.js`) + `_ProductCreateSuccess` · `_ProductEditForm` (447 lines) · `_ValidationScriptsPartial` · view component `NavigationMenu` (6 tabs, 6 flyout panels, 22 links; Products flyout has 13).

### Element usage counts [G]
Buttons: outline-secondary 59, primary 50, secondary 29, success 5, outline-primary 5, link 2, outline-warning 1, dark 1, danger 1 · `btn-close` 29 (none `btn-close-white`) · modals (`modal fade`) 29, triggers `data-bs-toggle="modal"` 33 · alerts 43 (39 danger, 3 success, 1 info) · `input-group` 50 · `form-check` 48 (12 checkbox, 5 radio inputs) · textarea 23 · tables: `.data-table` 8, Bootstrap `.table` 1, `.table-responsive` 9 · stat tiles 4 (dashboard) · `.action-bar` 18 · `.form-section` many · inline `style=` 16 · views with inline `<script>` 12 · none: dropdown, accordion, toast, offcanvas, tooltip, popover, spinner, progress, switch, range, file upload, date input, pagination, `aria-busy` in markup.

## R4 UI/UX audit findings (45)
Severity: HIGH blocks users or fails an AA criterion; MED degrades quality or risks AA; LOW polish; INFO keep or note. Evidence tags: see Pipeline note. "Fix" names the catalogue task (R18) or UX item (R16).

| ID | Sev | Finding | Evidence | Fix |
|---|---|---|---|---|
| F-01 | MED | Surface steps are almost invisible: ground to panel is 1.09:1 and panel to raised 1.11:1; separation relies on 1px borders (`--line` 1.42:1 on panel) and `box-shadow: none` on cards, stats and sections | [C][S] tokens.css, app-theme.css | FOUND-001, COMP-007 |
| F-02 | MED | Three card idioms with different fills: `.card` (panel), `.stat` and `.form-section` (raised) inside bordered `main` (panel): card-on-card, weak hierarchy | [S] app-theme.css, components.css | COMP-007, SHELL-001 |
| F-03 | MED | Backdrop blur on opaque surfaces does nothing: `.account-login-card` (blur 28px) and `.alert` (blur 16px) have opaque backgrounds over a flat page, yet still cost compositing | [S] app-theme.css | ROUTE-001, COMP-010 |
| F-04 | MED | Overlays (modal, popover, date picker) are opaque `--panel-raised` with one shared shadow; scrim is plain `rgb(0 0 0 / .6)`: no layered depth | [S] | COMP-012, COMP-014 |
| F-05 | LOW | No ambient layer: the canvas is one flat colour, so even the dock's glass samples only page content | [S] | SHELL-001 |
| F-06 | MED | SF Pro Rounded ships only 400 and 700, but CSS asks 500 (`.btn`, `.status-chip`, `.badge`, table headers), 600 (labels) and 650 (date picker); by CSS font matching these collapse to 400/700, so emphasis is inconsistent | [S] plus font-matching rule; [U] | COMP-006 |
| F-07 | LOW | All-caps with tracking on table headers, login and nav eyebrows, date-picker weekdays (5 uppercase rules, 7 letter-spacing declarations); Apple moved section headers to title-style capitalization | [G][W] | COMP-006, COMP-008 |
| F-08 | MED | Root font-size is 14px below 768px, so rem sizes shrink: `.form-control` min-height 2.65rem is about 37px and `.btn` about 36px on phones (above the 24px AA minimum, below the 44px comfort target) | [S] derived from CSS; [U] | COMP-001, COMP-002, SHELL-006 |
| F-09 | LOW | Radii are mixed: tokens `--r-control` .5rem, `--r-panel` .75rem, `--r-overlay` 1rem plus literals (.8, .9, .95, .65, .55, .5rem), 999px and the dock at 1.9rem; nesting is not concentric | [S] | FOUND-001, COMP-007 |
| F-10 | INFO | Positive: colour tokens pass AA. ink 15.0:1, muted 6.0 to 7.2:1, accent text 7.2 to 8.0:1, danger 5.4 to 6.0:1, warn 8.3:1, status chips 5.0 to 6.1:1, `--line-strong` control borders 3.8 to 4.1:1 | [C] | keep values |
| F-11 | LOW | Raw `rgba()` literals bypass tokens and use a blue palette that is not the brand (disabled and readonly fills, fieldset tint, finished-price field) | [S] app-theme.css lines 559-620 | COMP-002 |
| F-12 | HIGH | Select chevron uses Bootstrap's light stroke `#343a40`, 1.62:1 on the page ground (fails 1.4.11, 3:1); `data-bs-theme` is unset and the app never overrides `--bs-form-select-bg-img` | [C][S]; [U] | FOUND-002 (Step 7.2) |
| F-13 | HIGH | `.btn-close` (29 uses) is Bootstrap's black glyph at 50% opacity on `--panel-raised`, no `btn-close-white` and no dark theme: the close X is almost invisible | [G][S]; [U] | FOUND-002 (Step 7.2) |
| F-14 | MED | No visual invalid state: no CSS for `.input-validation-error` or `.is-invalid`; errors show only as `.text-danger` text | [G] | COMP-002, COMP-005 |
| F-15 | HIGH | 111 `<label class="form-label">` with neither `for` nor `asp-for` in 19 files (all 11 Create pages, Edit, 11 modal form partials): fields lack a programmatic name (1.3.1, 3.3.2, 4.1.2). Exact-string grep, a few may caption non-inputs | [G] | COMP-004 |
| F-16 | MED | No `aria-describedby`, `aria-invalid` or `aria-live` anywhere; error text is not linked to its field | [G] | COMP-005 |
| F-17 | MED | 24 of 29 `modal fade` lines have no `aria-labelledby` (line-level grep) | [G] | COMP-012 |
| F-18 | MED | Transactions row-select checkboxes have no accessible name | [S] Transactions/Index.cshtml:132 | ROUTE-009 |
| F-19 | HIGH | `person-picker.js` and `product-picker.js` expose `role=combobox/listbox/option` and `aria-expanded` but register no key handling (the only `keydown` listeners are in site.js, navigation-menu.js, jalali-datepicker.js) | [G] | COMP-013 |
| F-20 | MED | Date picker: `role=dialog`, Escape closes; no arrow, PageUp/PageDown grid navigation found; focus trap and return unverified | [G][S]; [U] | COMP-014 |
| F-21 | MED | Focus Not Obscured (2.4.11, new in WCAG 2.2): fixed dock (z 1040) and sticky `.action-bar` (18 uses) with no `scroll-padding-*` in any stylesheet | [G][W] | SHELL-004 |
| F-22 | MED | Filters auto-submit on change: `onchange="this.form.submit()"` x11 (Products, People) plus script submit on Transactions; WCAG 3.2.2 risk, and arrow-keying a select can submit repeatedly in some browsers | [G]; [A] browser behaviour | UX-002 |
| F-23 | LOW | Customers and Sellers sortable headers are plain links: no `aria-sort`, no direction cue (Transactions has both) | [S] | ROUTE-004 |
| F-24 | LOW | Customers and Sellers render an empty table with no data; `_EmptyState` unused there | [G] | COMP-011, ROUTE-004 |
| F-25 | LOW | `<html lang="en">` while cells hold Persian text with only `dir="auto"` (no `lang`) | [S] | ROUTE-004, ROUTE-006 |
| F-26 | LOW | `viewport-fit=cover` missing while CSS uses `env(safe-area-inset-bottom)` (insets stay 0 on iPhone without it); no `theme-color` | [S][W]; [U] | SHELL-006 |
| F-27 | MED | Products flyout lists 13 links (11 "Add a ..."), and Products tabs also carry per-type Create buttons: two creation paths, one long flat list | [S] | SHELL-007, UX-001 |
| F-28 | LOW | Password-eye SVG duplicated 4 times (Login 1, Profile 3) | [S] | ROUTE-001, ROUTE-002 |
| F-29 | MED | Create forms: 11 pages (CreatePhone 514 lines) plus 11 modal partials with near-identical markup; `_ProductEditForm` is 447 lines | [S] | ROUTE-008, COMP-004 |
| F-30 | LOW | Inline `style=` x16 and inline `<script>` blocks in 12 views | [G] | ROUTE-* |
| F-31 | LOW | Two table idioms: `.data-table` x8 and Bootstrap `.table` x1 (Products/Details) | [G] | COMP-008 |
| F-32 | MED | 6 to 7 column tables scroll horizontally on phones (`.table-responsive`) | [S] | COMP-008, UX-005 |
| F-33 | LOW | Error page always prints "Development Mode" guidance to end users | [S] Error.cshtml | ROUTE-012 |
| F-34 | LOW | Privacy page is the template placeholder sentence | [S] Privacy.cshtml | ROUTE-012 |
| F-35 | LOW | Dashboard heading order: `h2` then `h5.card-title` (levels skipped) | [S] Index.cshtml | ROUTE-003 |
| F-36 | LOW | Fonts total about 1.8 MB (SF 1.0 MB, Vazirmatn 0.8 MB), `swap`, no preload | [S] | QA-004, R17 |
| F-37 | LOW | `jalali-datepicker.js` and `person-picker.js` load on every page; the date picker is also included again by Edit and Sell (harmless, guarded by `dataset.jalaliWired`) | [S] | QA-006 |
| F-38 | LOW | 6 render-blocking stylesheets plus Bootstrap 233 KB min; no bundling step | [S] | QA-004 |
| F-39 | INFO | No CSP header; inline scripts work today; any future CSP needs nonces | [S] | note |
| F-40 | MED | SF Pro Rounded (an Apple typeface) is self-hosted; whether Apple's font licence allows web self-hosting was not verified | [S]; [U] | RES-005, Q-02 |
| F-41 | LOW | `.btn:hover { transform: translateY(-1px) }` lift; the target model is press feedback (scale or brightness), no lift | [S] | MOTION-002 |
| F-42 | INFO | Positive: global reduced-motion rule (0.01 ms), skip link, `--focus-ring`, `aria-sort` on Transactions, labelled sections, `role=alert/status`, `aria-current` and `inert` in the dock | [S] | keep |
| F-43 | INFO | Positive: chart colours are read from CSS tokens (`profit-loss-chart.js`), so token changes reach the chart | [S] | keep |
| F-44 | MED | Person-picker results (`.list-group-item-action`) are outside `.product-picker-results`, so hover and focus use Bootstrap's light `#f8f9fa` in a dark UI (bright flash) | [S]; [U] | FOUND-002 (Step 7.2) |
| F-45 | MED | Scoped template CSS wins by specificity for layout-level elements: the footer gets `position:absolute; bottom:0; width:100%; white-space:nowrap; line-height:60px` (beats app-theme `.footer`), the Privacy link is `#0077cc` (4.01:1 on ground, 3.69:1 on panel) with no hover change | [S] specificity plus [C]; [U] computed style | SHELL-002 |

## R5 Accessibility audit (target WCAG 2.2 AA)
| SC | Status | Evidence | Task |
|---|---|---|---|
| 1.1.1 Non-text content | Pass | no `<img>`; decorative SVG is `aria-hidden` | none |
| 1.3.1 Info and relationships | Fail | F-15, F-18; heading skip F-35 | COMP-004, ROUTE-003 |
| 1.4.1 Use of colour | Pass | chips carry text; links underlined | none |
| 1.4.3 Contrast (minimum) | Pass with exception | tokens pass (F-10); footer link F-45 | SHELL-002 |
| 1.4.11 Non-text contrast | Fail | chevron F-12, close X F-13; control borders and focus ring pass | FOUND-002 |
| 1.4.12 Text spacing | Unknown | not tested | QA-002 |
| 1.4.13 Content on hover or focus | Partial | dock flyout opens on hover, Escape closes it; persistence unverified | QA-002 |
| 2.1.1 Keyboard | Fail (partial) | pickers F-19, date-picker grid F-20 | COMP-013, COMP-014 |
| 2.4.1 Bypass blocks | Pass | skip link to `#main` | none |
| 2.4.3 Focus order | Unknown | natural DOM order; modals Bootstrap-managed | QA-002 |
| 2.4.7 Focus visible | Pass | `--focus-ring` on buttons, inputs, links; dock ring | QA-002 |
| 2.4.11 Focus not obscured (min) | At risk | F-21 | SHELL-004 |
| 2.5.8 Target size (min) | Pass (estimate) | 24px minimum met; coarse-pointer comfort F-08 | COMP-001, COMP-002 |
| 3.2.2 On input | At risk | F-22 | UX-002 |
| 3.3.1 Error identification | Partial | text errors and `role=alert` summary; no field state F-14 | COMP-005 |
| 3.3.2 Labels or instructions | Fail | F-15 | COMP-004 |
| 3.3.7 Redundant entry | N/A | no multi-step flow | none |
| 3.3.8 Accessible authentication | Pass | `autocomplete` on login fields, password toggle, no paste block found | none |
| 4.1.2 Name, role, value | Fail | F-15, F-18, F-19 | COMP-004, COMP-013 |
| 4.1.3 Status messages | Partial | `role=status` x5, `role=alert` x40; no live region for filter or picker results | COMP-005, UX-003 |

## R6 Responsive and browser audit
| Topic | Finding | Evidence |
|---|---|---|
| Breakpoints in use | `max-width` 575.98 / 767.98 / 1199.98 and `min-width` 768 (root font 14px below, 16px from 768); Bootstrap grid classes (`col-sm/md/lg`) in views; `(hover: none), (pointer: coarse)` handled in the dock | [S][G] site.css, app-theme.css 808-827, components.css 146-250, navigation-menu.css 43-88, 472-531 |
| Container | `max-width: 1320px`, centered; on wide and ultrawide screens both sides are flat canvas colour | [S] app-theme.css:35 |
| Viewport units | `body` min-height 100vh and `main` min-height `calc(100vh - 10rem)`; only the dock flyout uses `dvh`; `100vh` can overshoot under iOS dynamic toolbars | [S]; [U] |
| Safe areas | `env(safe-area-inset-bottom)` is used (action bar, dock at <=575px) but `viewport-fit=cover` is absent, so insets are 0 on iPhone; no `theme-color` | [S][W]; [U] |
| Touch targets | controls about 36 to 37px tall at the 14px root (F-08); dock tabs sized by Stage 5/6 | [S] |
| Keyboard overlap | sticky `.action-bar` plus the dock reduce the visible area when the on-screen keyboard is open; no `visualViewport` handling | [S]; [U] |
| Landscape phones | dock plus action bar (offset 5.5rem) leave little height on short viewports | [S]; [U] |
| Tables | `.table-responsive` horizontal scroll for 6 to 7 columns (F-32) | [S] |
| Dense desktop | forms use `col-md-6/col-lg-7 mx-auto`; tables fill the 1320px container; keep desktop density, do not stretch a mobile layout | [S] |
| Browser targets | not documented in README (Q-06); assumed last two majors of Chrome, Edge, Safari (macOS and iOS), Firefox | [A] |

Feature support (for the effects planned in R10 and R12):
| Feature | Chrome/Edge | Safari | Firefox | Decision |
|---|---|---|---|---|
| `backdrop-filter` blur and saturate | yes | yes (prefixed form already in the code) | yes | baseline glass; solid fallback via `@supports not` [A] general knowledge, not re-checked this pass |
| `prefers-reduced-transparency` | 118/119+ | not shipped | not shipped (flag) | not enough alone: add an in-app solid-surfaces switch (D-09) [W] Chrome blog, caniuse feature page |
| `prefers-contrast: more` | yes | yes | yes | map to solid surfaces and stronger borders [A] |
| `color-mix()`, `linear()` easing | yes | yes | yes | already used by Stage 5/6 with fallbacks [A] |
| Cross-document view transitions (`@view-transition`) | 126+ | 18.2+ | no | optional navigation polish only (SHELL-009) [W] Chrome docs |
| Same-document view transitions | 111+ | 18+ | 144 | not needed (multi-page app) [W] |
| `backdrop-filter: url(#svg)` refraction | yes | declaration parses but renders nothing, no reliable detection | no | rejected as baseline (D-08) [W] community write-ups |
| `backdrop-filter` side effects | creates a containing block for `position: fixed` descendants and a stacking context; stacked blurs are costly | same | same | keep modals and toasts outside glass parents; max 2 blur layers [W] |

Fallback strategy (all required): (1) `@supports not (backdrop-filter: blur(1px)) and not (-webkit-backdrop-filter: blur(1px))` gives solid surfaces; (2) `prefers-reduced-transparency: reduce` gives solid; (3) `prefers-contrast: more` gives solid plus `--ink-muted` borders; (4) `html[data-surfaces="solid"]` from the in-app switch (Stage 15) gives solid on every browser; (5) the page must stay fully legible and coherent with all four.

## R7 Performance and technical constraints
- Payload today [G]: app CSS 43 KB (+ Bootstrap 233 KB min), app JS 75 KB (+ jQuery 88 KB, Bootstrap bundle 81 KB, Chart.js 209 KB on ProfitLoss only), fonts about 1.8 MB (`swap`, no preload).
- Proposed budgets (measure at QA-004 against a baseline taken before Stage 8): LCP <= 2.5 s, INP <= 200 ms, CLS <= 0.1 on a throttled mid-tier mobile profile [A thresholds]; at most 2 concurrent `backdrop-filter` layers in the viewport (dock plus one overlay) and blur radius <= 24px; never animate `backdrop-filter`, `filter` or shadow blur; animate only `transform` and `opacity` (plus short colour fades); app CSS <= 60 KB uncompressed and <= 8 files after migration; no new runtime JS library; each new script <= 8 KB; no layout shift from late-loading fonts (keep `swap`, preload only the two faces used above the fold if SF is kept).
- Technical constraints [S]: Bootstrap is plain CSS (no Sass), so tokens live in plain custom properties and Bootstrap is overridden, not recompiled; no bundler (new CSS files are extra requests, acceptable on a local app, each linked with `asp-append-version`); CI cannot see CSS, so every CSS stage needs the owner's manual check list; Stage 5/6 dock invariants must keep passing (R24); modals created inside a glass parent would break (containing block), so overlays stay direct children of `body`/Bootstrap's modal root; label association inside the 11 modal partials needs unique ids per instance (id prefix parameter).
- Debt that can affect the redesign: `app-theme.css` mixes tokens mapping, base, login, buttons, forms, tables, overlays and the date picker in 1049 lines; 13 `!important` (11 app-theme, 2 dock); 10 `rgb()/rgba()` literals (9 app-theme, 1 dock); 15 literal durations in app-theme.css; scoped template CSS (F-45); duplicated form markup (F-29). Hex literals outside `tokens.css`: 0 (keep it that way).
- Migration pattern (D-05): no big-bang split. Each stage creates a new family file (`controls.css`, `surfaces.css`, `overlays.css`, `shell.css`, `motion.css`), moves the family's rules out of `app-theme.css` in the same step, and keeps the cascade order explicit; `app-theme.css` is deleted in Stage 16 when empty.

## R8 References
Official Apple guidance is authoritative for principles; community items are inspiration only. "Read" states how much I actually read.
| Reference (URL) | Type, platform | Pattern | Why it works | Translation to MobileShop | Limits and read depth |
|---|---|---|---|---|---|
| WWDC25 219 Meet Liquid Glass https://developer.apple.com/videos/play/wwdc2025/219/ | Official, all Apple platforms | lensing; materialize and morph; adaptive tint and light/dark flip by element size; Regular vs Clear variants, never mixed, Clear only over media with dimming; glass on the navigation layer only; no glass on glass; tint only for primary actions; scroll-edge effect; accessibility modes (reduced transparency frostier, increased contrast black/white with border, reduced motion) | controls stay distinct from content and legible over anything | glass only for dock, flyout, overlay sheets; Regular only (no media in the app, Clear reserved); accent tint only on primary buttons and selected tab; soft scroll-edge fade under the action bar; four accessibility fallbacks (R6) | lensing and adaptive sampling need native compositing; web approximates with blur, saturate, tint. Read: full transcript |
| Adopting Liquid Glass https://developer.apple.com/documentation/technologyoverviews/adopting-liquid-glass | Official, Apple platforms | remove custom bar and sheet backgrounds; avoid overuse; concentric shapes; larger row heights and padding in lists, tables, forms; title-style section headers; sheets inset, more opaque when expanded; controls morph into menus; accessibility labels on icons; group glass for performance; test with accessibility settings | restraint plus consistent geometry | R10 material rules, R11 radii (concentric formula), larger control and row sizes, no all-caps headers, solid when a modal is large, one blur layer shared by a group | SwiftUI/UIKit APIs not applicable. Read: full page (Markdown version) |
| HIG Materials https://developer.apple.com/design/human-interface-guidelines/materials | Official, Apple platforms | two kinds of material: Liquid Glass for controls and navigation, standard materials (blur, vibrancy) for the content layer; do not put Liquid Glass in the content layer | clear layer model | content layer = opaque tokens (a subtle blur only if ever needed); glass only above it | The page needs JavaScript; the direct fetch returned no text, so this row rests on a third-party text mirror (glama.ai, a copy of the HIG). Re-read the official page in a browser before Stage 9. Read: mirror only |
| WWDC25 102 Platforms State of the Union https://developer.apple.com/videos/play/wwdc2025/102/ | Official | hierarchy, harmony, consistency; concentricity; larger, bolder, left-aligned type; softer shapes, taller controls; controls recede on scroll; content in glass monochrome by default | a single design vocabulary across surfaces | H1 larger and bolder, taller controls, concentric radii, dock already shrinks when idle (Stage 6) | native components only. Read: transcript (token-capped, design sections read) |
| WCAG 2.2 What's New https://w3.org/WAI/standards-guidelines/wcag/new-in-22/ | Standard | new AA criteria: 2.4.11 Focus Not Obscured, 2.5.8 Target Size (Minimum), 3.3.8 Accessible Authentication, also 2.5.7, 3.2.6, 3.3.7 | measurable acceptance | R5 mapping; SHELL-004 targets 2.4.11 for the dock and action bar | Read: search-result level, page not opened in full |
| prefers-reduced-transparency https://developer.chrome.com/blog/css-prefers-reduced-transparency and https://caniuse.com/wf-prefers-reduced-transparency | Standards and browser vendor | media query for solid surfaces | respects OS setting | used by the dock already; extended to all materials | Chrome/Edge only; Safari and Firefox not shipped, hence the in-app switch. Read: search-result level |
| View Transitions https://developer.chrome.com/docs/web-platform/view-transitions and .../cross-document | Browser vendor | `@view-transition { navigation: auto }` for multi-page navigation | smooth page change with tiny CSS | optional SHELL-009; dock gets a `view-transition-name` so it stays still | cross-document: Chrome 126+, Safari 18.2+, no Firefox. Read: search-result level |
| Liquid Glass on the web: 6 ways https://dev.to/devyatov/liquid-glass-on-the-web-6-ways-to-build-it-with-css-and-svg-3m07 | Community, web | blur-saturate-tint baseline; SVG displacement refraction via `backdrop-filter: url()`; pitfalls (containing block, stacked cost, Safari renders nothing and detection lies) | honest comparison of approximations | baseline only; refraction rejected (D-08) | Read: search-result level |
| rizroze/liquid-glass https://github.com/rizroze/liquid-glass; Z1Code/glass-refraction https://github.com/Z1Code/glass-refraction; deepika-builds/liquid-glass wiki https://github.com/deepika-builds/liquid-glass/wiki | Community, web | displacement-map refraction, specular edges | convincing lensing in Chromium | only the idea of a 1px edge highlight plus glass border is adopted (`--edge-highlight`, `--border-glass`) | Chromium-only effect, cost; repositories not inspected beyond search results |
| Dribbble collections https://dribbble.com/ryanontheroad/collections/7635955-Glass-liquid, https://dribbble.com/MaxMonastyrev/collections/7537867-Glass, https://dribbble.com/iamefe/collections/7563096-Liquid-glass | Community concepts | floating tab bars, segmented controls, glass sidebars, toggles | visual inspiration | segmented control for Products tabs and report ranges (R15) | concept art, no performance or contrast constraints; collection pages only, no individual shot analysed |
| Create with Swift, Exploring a new visual language: Liquid Glass https://www.createwithswift.com/exploring-a-new-visual-language-liquid-glass/ | Community explainer | summary of the visual language | quick orientation | none beyond Apple sources | search-result level |
| Deque axe-core NuGet https://www.github.com/dequelabs/axe-core-nuget; Deque.AxeCore.Playwright 4.9.0 https://nuget.org/packages/Deque.AxeCore.Playwright/4.9.0 | Tooling | automated a11y checks inside Playwright for .NET | repeatable WCAG scan | optional QA-002 | adds a test-only package (R17). Read: package pages in search results |

Not reached: Awwwards (the search returned no usable pages). Nothing here claims native Apple rendering can be reproduced in a browser.

## R9 Design principles
1. Quiet content, glass chrome: glass is the navigation and overlay layer only (dock, flyout, modal sheets, menus, pickers, date picker, login card). Dense content (tables, forms, cards) stays opaque and low-noise.
2. One glass layer per stack and never glass on glass; things sitting on glass use fills, not another blur. At most two `backdrop-filter` layers in view (dock plus one overlay); while a modal is open the dock and action bar go solid.
3. Regular glass only. Clear glass is reserved: the app has no media-rich content, so Apple's three conditions for Clear are not met (WWDC25 219).
4. Tint is semantic: accent fill only for primary actions and the selected state; everything else is neutral.
5. Depth comes from hierarchy: opaque surface steps, hairlines, edge highlights and four elevation levels, not from blur everywhere.
6. Concentric geometry: nested radii follow `inner = outer - inset`; one radius per role, not per component.
7. Legibility first: text over any material meets 4.5:1 (UI 3:1) on the worst-case backdrop (R10); materials get a tint floor and a brightness filter instead of trusting the backdrop.
8. Motion answers an action: press, present, dismiss, select. Springs only where Stage 6 already uses them (dock) and for sheet entrance; everything else is short and linear-ish; reduced motion removes scale and spring.
9. Preserve the brand and the behaviour: dark ground, teal accent, rounded type stack, the dock; no workflow changes for looks (UX items are separate, R16).
10. Progressive enhancement: every effect has a solid fallback (R6) and an in-app switch; Safari and Firefox get the same legibility as Chromium.

## R10 Material taxonomy
### R10.A Materials and usage rules
| Material | Use for | Never for | Tokens | Edge, shadow, highlight | Contrast rule | Fallback |
|---|---|---|---|---|---|---|
| M0 Canvas | page background; plus one static ambient layer (spec below) | text containers | `--surface-canvas` | none | n/a | plain canvas under `prefers-contrast: more` |
| M1 Content surface | `main` groups, tables, form sections | blur, transparency | `--surface-1` | 1px `--hairline`, no shadow | ink 15:1, muted 6:1 | none needed |
| M2 Raised surface | cards, stat tiles, empty-state well, selected rows | blur, transparency | `--surface-2` | `--hairline`, `--edge-highlight`, `--shadow-1` | as M1 | none needed |
| M3 Inset surface | fields, readonly values, code | shadows | `--surface-inset` | 1px `--border-control` (3.8:1) | placeholder >= 4.5:1 | none needed |
| G1 Regular glass | dock, flyout, selected high-priority floating controls | content, tables, forms | `--glass-regular-bg` 78% tint, `--glass-regular-filter` blur 20, saturate 1.5, brightness .6 | `--border-glass`, `--shadow-3`, `--edge-highlight` | muted text 4.13:1 over a pure white strip, 4.89:1 over a pure accent strip (computed); Stage 6 measured 4.3:1 in Chromium and documents it as a known limit | solid `--glass-solid-bg` |
| G2 Subtle glass | sticky action bar, tooltips if added | modals, dense text blocks | `--glass-subtle-bg` 88% tint, `--glass-subtle-filter` blur 12, saturate 1.3, brightness .8 | `--hairline`, `--shadow-2` | muted 4.54:1 white strip, 5.12:1 accent strip (computed) | solid |
| G3 Overlay glass | modal sheets (small), popovers, floating picker lists, date picker, login card, toasts if added | large or form-heavy modals | `--glass-overlay-bg` 92% tint, `--glass-overlay-filter` blur 24, saturate 1.4, brightness .85 | `--border-glass`, `--shadow-4` (modal) or `--shadow-3`, `--edge-highlight` | muted 4.95:1 white strip, 5.38:1 accent strip (computed) | solid |
| G3-solid | modals that are large or form-heavy (`modal-lg/xl`, `modal-dialog-scrollable`, more than 6 fields) | | `--glass-solid-bg` | same border and shadow as G3 | ink 15:1 | n/a (already solid); mirrors Apple's "sheets turn more opaque when expanded" |
| G4 Clear glass | reserved, not used | everything today | none defined | | | revisit only if media (product photos) is ever added, with owner approval |
| Scrim | behind modals | | `--scrim-bg`, `--scrim-filter` blur 6 (one layer) | | dims content to unreadable-on-purpose | solid `rgb(0 0 0 / .72)`, no blur |
Hover on glass controls: mix 6% `--ink` into the fill. Pressed: scale .98 (not under reduced motion) plus 6% darker fill. Selected nav item: `--accent-soft` fill plus accent ring (already in Stage 5). Disabled: `--text-disabled` (3.0:1 on ground, exempt from 1.4.3) and 60% surface.
Ambient layer (SHELL-001): one `body::before`, `position: fixed; inset: 0; z-index: -1; pointer-events: none`, two radial gradients (accent at <= 14% alpha top-left, `--ink-muted` at <= 8% top-right), no animation, no blur, removed under `prefers-contrast: more`. It exists so glass has luminance variation to sample. No WebGL, canvas, shader or pointer-tracking effect is planned (D-08).
Budget rule: while `body.modal-open`, `.floating-navigation` and `.action-bar` switch to solid (they are under the scrim anyway), keeping the count at scrim plus sheet.

### R10.B Light and dark theme rules
- Dark is the shipped theme and the brand. Tokens are semantic (surface, text, border, glass) so a light theme is an override set, not a rewrite; components must use semantic tokens only and never name a theme.
- Light theme is OPTIONAL (Q-03). If approved: `[data-theme="light"]` overrides, white-tint glass with inverted brightness, `data-bs-theme="light"`, and its own contrast measurements. Until approved there are no light tokens and no `prefers-color-scheme` rules.
- `color-scheme: dark` stays on `:root`, plus the meta added in Step 7.2.

## R11 Design tokens
### R11.A Canonical token block (copied into `tokens.css` by Step 7.1)
```css
/* BEGIN tokens-v1 */
/* Liquid Glass design tokens, v1. Additive: the legacy tokens above are unchanged and still used by the pages. */
:root {
  /* Opaque content surfaces. */
  --surface-canvas: var(--ground);
  --surface-1: var(--panel);
  --surface-2: var(--panel-raised);
  --surface-3: color-mix(in srgb, var(--panel-raised) 90%, var(--ink));
  --surface-inset: var(--ground);

  /* Text. */
  --text-1: var(--ink);
  --text-2: var(--ink-muted);
  --text-disabled: color-mix(in srgb, var(--ink-muted) 55%, var(--ground));

  /* Lines and edges. */
  --hairline: var(--line);
  --border-control: var(--line-strong);
  --border-glass: color-mix(in srgb, var(--ink) 16%, transparent);
  --edge-highlight: inset 0 1px 0 color-mix(in srgb, var(--ink) 10%, transparent);

  /* Accent family. */
  --accent-soft: color-mix(in srgb, var(--accent) 16%, transparent);
  --accent-strong: color-mix(in srgb, var(--accent) 88%, white);
  --danger-soft: color-mix(in srgb, var(--danger) 16%, transparent);
  --warn-soft: color-mix(in srgb, var(--warn) 16%, transparent);
  --success-soft: var(--accent-soft);
  --info-soft: var(--accent-soft);

  /* Materials (usage rules: plan R10). */
  --glass-regular-bg: color-mix(in srgb, var(--panel-raised) 78%, transparent);
  --glass-regular-filter: blur(20px) saturate(1.5) brightness(0.6);
  --glass-subtle-bg: color-mix(in srgb, var(--panel-raised) 88%, transparent);
  --glass-subtle-filter: blur(12px) saturate(1.3) brightness(0.8);
  --glass-overlay-bg: color-mix(in srgb, var(--panel-raised) 92%, transparent);
  --glass-overlay-filter: blur(24px) saturate(1.4) brightness(0.85);
  --glass-solid-bg: var(--panel-raised);
  --scrim-bg: rgb(0 0 0 / 0.55);
  --scrim-filter: blur(6px);

  /* Elevation. */
  --shadow-1: 0 1px 2px rgb(0 0 0 / 0.3), 0 1px 1px rgb(0 0 0 / 0.2);
  --shadow-2: 0 0.5rem 1.25rem rgb(0 0 0 / 0.35);
  --shadow-3: 0 1rem 2rem rgb(0 0 0 / 0.45);
  --shadow-4: 0 1.5rem 3rem rgb(0 0 0 / 0.55);

  /* Corner radii. Concentric rule: inner radius = outer radius - inset, never below --radius-xs. */
  --radius-xs: 0.375rem;
  --radius-sm: 0.5rem;
  --radius-md: 0.75rem;
  --radius-lg: 1rem;
  --radius-xl: 1.5rem;
  --radius-2xl: 2rem;
  --radius-pill: 999px;

  /* Spacing, 4px grid at a 16px root. */
  --space-1: 0.25rem;
  --space-2: 0.5rem;
  --space-3: 0.75rem;
  --space-4: 1rem;
  --space-5: 1.25rem;
  --space-6: 1.5rem;
  --space-8: 2rem;
  --space-10: 2.5rem;
  --space-12: 3rem;

  /* Component sizes. */
  --size-control-sm: 2.25rem;
  --size-control: 2.75rem;
  --size-control-lg: 3.25rem;
  --size-hit-min: 44px;
  --size-icon-sm: 1rem;
  --size-icon: 1.25rem;
  --size-icon-lg: 1.5rem;
  --size-measure: 70ch;

  /* Typography. Only 400 and 700 exist for the Latin face. */
  --fs-xs: 0.75rem;
  --lh-tight: 1.2;
  --lh-snug: 1.35;
  --lh-body: 1.5;
  --fw-regular: 400;
  --fw-strong: 700;
  --tracking-tight: -0.02em;

  /* Motion. */
  --dur-instant: 90ms;
  --dur-fast: 150ms;
  --dur-base: 220ms;
  --dur-slow: 360ms;
  --dur-sheet: 460ms;
  --ease-standard: cubic-bezier(0.4, 0, 0.2, 1);
  --ease-out: cubic-bezier(0.32, 0.72, 0, 1);
  --ease-press: cubic-bezier(0.22, 1, 0.36, 1);
  --ease-spring: linear(0, 0.044, 0.149, 0.284, 0.428, 0.565, 0.687, 0.789, 0.871, 0.933, 0.977, 1.007, 1.026, 1.035, 1.038, 1.037, 1.033, 1.028, 1.023, 1.018, 1.013, 1.009, 1.005, 1.003, 1.001, 1);

  /* Stacking order (Bootstrap defaults: dropdown 1000, sticky 1020, fixed 1030). */
  --z-dock: 1040;
  --z-scrim: 1050;
  --z-modal: 1060;
  --z-popover: 1070;
  --z-toast: 1090;
}

@supports not (transition-timing-function: linear(0, 1)) {
  :root {
    --ease-spring: cubic-bezier(0.3, 1.25, 0.5, 1);
  }
}
/* END tokens-v1 */
```
Computed notes [C]: `--surface-3` is `#313942` (1.33:1 over raised, 1.47:1 over panel; ink 10.2:1; muted 4.53:1, so keep muted small text off persistent surface-3). `--text-disabled` is `#57626D`. Legacy tokens stay as primitives; they are deprecated for new code from Stage 8 (`--r-control/--r-panel/--r-overlay` map to `--radius-sm/md/lg` only in migration steps).

### R11.B Colour roles
| Role | Token | Value | Notes |
|---|---|---|---|
| Canvas, panel, raised | `--surface-canvas`, `--surface-1`, `--surface-2` | `#0E1318`, `#151C23`, `#1C252E` | brand, unchanged; steps are 1.09:1 and 1.11:1 so depth comes from edges and shadow (F-01) |
| Hover/selected fill | `--surface-3` | `#313942` | states only |
| Text | `--text-1`, `--text-2`, `--text-disabled` | `#ECF0F4`, `#93A3B3`, `#57626D` | 15.0:1, 6.0 to 7.2:1, 3.0:1 (disabled exempt) |
| Borders | `--hairline`, `--border-control`, `--border-glass` | `#2B3743`, `#66788A`, ink at 16% | hairline is decorative only (1.42:1); controls use 3.8 to 4.1:1 |
| Primary action | `--accent`, `--accent-ink`, `--accent-strong`, `--accent-soft` | `#36C5B0`, `#06201C`, `#4ECCB9`, accent 16% | accent text 7.2 to 8.0:1; success and info reuse accent |
| Warning | `--warn`, `--warn-soft` | `#E5A94B` | 8.3:1 text |
| Danger | `--danger`, `--danger-soft` | `#F0707A` | 5.4 to 6.0:1 text |
| Focus | `--focus-ring` (legacy) | 2px ground gap plus 2px accent | accent on ground about 8:1; 3px under `prefers-contrast: more` |
| Text over glass | `--text-1` only; `--text-2` allowed on G1 to G3 because the tint floors above keep it >= 4.5:1 on white and accent strips (G1 excepted: 4.13:1 on pure white, documented Stage 6 limit) | | measure at QA-003 |
| Text over image | n/a (no images) | | |

### R11.C Typography scale
Latin face SF Pro Rounded (400, 700 only) then Vazirmatn then the system stack (`--font-body`, unchanged). Weight policy: 700 for headings, stat values, table headers, chips, form-section titles, selected tab label; 400 for everything else including buttons (this equals today's rendering, where 500 collapses to 400). If the face is replaced (Q-02) reconsider 500/600. Vazirmatn 500 becomes unused: its `@font-face` can be removed (about 269 KB less) once no `font-weight: 500` remains.
| Role | Size at 16px root | Weight | Line height | Tracking | Use |
|---|---|---|---|---|---|
| H1 `--fs-h1` | `clamp(1.75rem, 1.5rem + 0.8vw, 2rem)` (target; today 1.75rem) | 700 | `--lh-tight` | `--tracking-tight` | page title |
| H2 `--fs-h2` | 1.375rem | 700 | 1.25 | -0.01em | section heading |
| Section title `--fs-lg` | 1.125rem | 700 | `--lh-snug` | 0 | `form-section__title`, card titles; title-case, no uppercase |
| Body `--fs-body` | 1rem | 400 | `--lh-body` | 0 | text, values |
| Label and small `--fs-sm` | 0.875rem | 400 label, 700 table header | `--lh-snug` | 0 | labels in `--text-2`, helper text, table cells |
| Caption `--fs-xs` | 0.75rem | 700 chips, 400 captions | `--lh-snug` | 0 | chips, badges, footnotes |
Rules: root is 14px below 768px and 16px from 768 (kept); maximum prose line `--size-measure` (70ch); single-column forms at most 40rem; headings left-aligned; numerals use `.num` (tabular); Persian runs follow the stack automatically, give Persian-heavy blocks `line-height: 1.7`.

### R11.D Spacing, radius, elevation, borders, sizing, icons, breakpoints, motion
- Spacing: `--space-1..12` (4 to 48px). Defaults: icon gap 1, stack tight 2, field row gap 3, card padding 5, section gap 6, page section 8, hero/empty-state 10 to 12. Table cell padding .75rem 1rem, row min-height 3rem (Apple asks for roomier lists, tables and forms). Grid: Bootstrap 12 columns with `g-3` gutters kept.
- Radius roles: xs chips inside tables, row hover; sm date-picker day cells, small buttons, menu items; md buttons, fields, alerts, segmented items; lg cards, stat tiles, form sections, empty-state well, floating picker lists; xl main panel (if kept), modal sheet, login card, date-picker panel, flyout panel; 2xl dock; pill chips, badges, tabs, segmented track, switch. Concentric examples: modal xl (24) with 8px inset holds cards at lg (16); card lg (16) with 4px inset holds pill chips; dock 2xl (32) with 6px inset holds tab items at calc(2rem - 6px).
- Elevation: 0 canvas and table rows; 1 `--shadow-1` plus edge highlight on M2 cards; 2 `--shadow-2` on menus, floating lists, action bar, tooltips; 3 `--shadow-3` on dock, flyout, toasts; 4 `--shadow-4` on modal sheets. On a dark canvas shadows read weakly, so the edge highlight and `--border-glass` carry separation.
- Borders: hairline 1px for separators (never the only state cue); control border 1px `--border-control`; glass border 1px `--border-glass` plus inset top highlight; error border `--danger` with `aria-invalid`; selected uses accent ring or fill plus `aria-selected/current/pressed`.
- Sizing: button md `--size-control` (44px at 16px root, 38.5px at the 14px root, so `@media (pointer: coarse) { min-height: var(--size-hit-min) }`), sm `--size-control-sm`, lg `--size-control-lg`; icon-only buttons square with an accessible name; fields as button md, textarea min 6rem; checkbox and radio box 1.25rem inside a 44px coarse row; segmented control `--size-control-sm`; chips 1.5rem; card padding `--space-5`; modal radius xl, max-height `calc(100dvh - 2rem)`; dock sizes stay as in Stage 5/6.
- Icons: inline SVG, 24x24 viewBox, 1.75 stroke, round caps and joins, `currentColor`, sizes 16/20/24 (`--size-icon-*`), decorative icons `aria-hidden="true" focusable="false"`, icon-only controls get `aria-label`. One `_Icons` sprite partial in the layout (always rendered, so the login page can use it) replaces the 4 duplicated password-eye SVGs. No icon font and no SF Symbols (Apple asset); Lucide (ISC) path data may be copied per icon without a dependency (optional).
- Breakpoints (media queries need literals): xs below 576 (root 14px, single column, dock compact, tables scroll or become cards), sm 576-767 (root 14px), md 768-991 (root 16px, two-column forms), lg 992-1199 (tablet landscape, small laptop), xl 1200-1399 (desktop), xxl 1400+ (container capped at 1320px, ambient visible on both sides). Existing queries (575.98, 767.98, 1199.98) already match.
- Motion tokens: instant 90 ms (link colour, checkbox tick); fast 150 ms with `--ease-standard` (hover, focus, row hover); base 220 ms with `--ease-press` or `--ease-out` (press release, popover present with scale .98 to 1 and opacity, segmented thumb); slow 360 ms with `--ease-out` (scrim fade, collapse, toast); sheet 460 ms with `--ease-spring` (modal entrance: translateY 12px and scale .98 to 1). The dock keeps its own Stage 5/6 curves. Animate only `transform`, `opacity` and short colour fades (plus the existing flyout `max-height`); never `backdrop-filter`, `filter`, shadow blur, width, height, margin, top or left. Reduced motion: durations 0.01 ms, spring becomes standard, no scale presses, spinners become static text.

## R12 Interaction states and accessibility alternatives
### R12.A State definitions (apply to every interactive component)
- Hover (only under `@media (hover: hover) and (pointer: fine)`): mix 6% `--ink` into the fill; primary fill goes to `--accent-strong`; no lift or translate.
- Focus-visible: `--focus-ring` on every interactive element, never `outline: none` without a ring; where a parent clips overflow use an inset ring; ring colour stays accent over glass.
- Pressed: `transform: scale(0.98)` over `--dur-instant` plus a 6% darker fill; removed under reduced motion (fill change only).
- Selected: `--accent-soft` fill, accent text or ring and the matching ARIA (`aria-selected`, `aria-current`, `aria-pressed`, `aria-checked`); never colour alone.
- Disabled: `--text-disabled`, 60% surface, `cursor: not-allowed`, no hover or press; links get `aria-disabled="true"` and `tabindex="-1"`; submit buttons are disabled only while submitting.
- Loading: `aria-busy="true"` on the control or region; a button keeps its width and shows a 1rem ring spinner plus visually hidden "Working..."; reduced motion shows static text; picker lists keep the existing `.loading-line` skeleton.
- Error: field border `--danger`, `aria-invalid="true"`, message linked by `aria-describedby`, icon plus text; on failed submit the summary (`role="alert"`) takes focus.
- Success: `role="status"` message with accent icon plus text, not auto-dismissed in under 8 seconds.
- Empty: `_EmptyState` with title, hint and one primary action slot. Offline: not applicable (server-rendered, no PWA).
### R12.B Accessibility alternatives
| Mode | Trigger | Changes |
|---|---|---|
| Reduced motion | `prefers-reduced-motion: reduce` | durations 0.01 ms, spring to standard, no scale press, static spinners |
| Reduced transparency | `prefers-reduced-transparency: reduce` or `html[data-surfaces="solid"]` | G1 to G3 become `--glass-solid-bg`, no blur, border `--border-control`, scrim solid `rgb(0 0 0 / 0.72)` without blur |
| Increased contrast | `prefers-contrast: more` | solid surfaces, borders `--ink-muted`, `--text-2` becomes `--text-1`, focus ring 3px, ambient layer off |
| Forced colors | `forced-colors: active` | system colours (`CanvasText`, `Highlight`), no shadows or blur, borders kept, chart gets a data-table alternative (R15) |
| No `backdrop-filter` | `@supports not` | solid surfaces |
| Coarse pointer | `pointer: coarse` | 44px minimum targets, no hover-only affordance |
| Zoom and reflow | 200% text, 320px width | no horizontal scroll except data tables (1.4.10) |

## R13 Composition, naming and reuse
- Composition: (1) a glass element never contains another glass element; (2) overlays are direct children of `body` or Bootstrap's modal root, never inside a glass parent (containing-block gotcha); (3) content surfaces never use `backdrop-filter`; (4) at most three nested rounded levels (panel, card, control) with concentric radii; (5) elevation only from `--shadow-*`; (6) colour only from role tokens (no hex anywhere; `rgb()` only inside `tokens.css`); (7) z-index only from `--z-*`; (8) state classes `is-*`, hooks `data-*`, ARIA mirrors state; (9) no `outline: none` without a ring; (10) user data that can be Persian sits in `dir="auto"` containers, with `lang="fa"` where known.
- Naming: keep the existing BEM-like blocks (`page-head__title`, `form-section__title`, `navigation-*`, `account-login-*`); new blocks `glass` (+ `glass--regular|subtle|overlay|solid`), `surface` (+ `surface--1|2|inset`), `field` (`field__label`, `field__control`, `field__help`, `field__error`), `segmented` (`segmented__item`), `chip`, `scroll-fade`, `toast` (only if UX-004 is approved). Tokens follow `--{role}-{variant}`.
- Reuse: Razor partials, not TagHelpers (matches the repo): `_FormField` (label, control, help, error with generated ids and `aria-describedby`), `_PasswordField`, `_PageHead`, `_StatTile`, `_Icons` (sprite), `_EmptyState` (extended), `_Modal` shell (guarantees `aria-labelledby`). Introduce each in the stage that first needs it, then migrate callers per page in Stages 12 to 14.
- Migration: strangler per R7 (D-05). Legacy class names stay as aliases until Stage 16 (for example `btn-outline-secondary`, `btn-success`, `btn-dark` map to the new variants in CSS), so Razor edits stay minimal. Legacy tokens stay as primitives; `--r-control/--r-panel/--r-overlay` are retired by Stage 10 and deleted at Stage 16.
- Review gate for every CSS step: no hex literal, no new `!important`, no literal duration or easing outside `tokens.css` and `motion.css`, no `backdrop-filter` outside the allowed classes (`.glass*`, dock, scrim), no new z-index number, Stage 5/6 dock invariants still pass (R24).

## R14 Route-by-route redesign specification
Status of every row: nothing implemented. "Verify" is what the owner or Reviewer checks; browser items are manual until a harness exists (R19). Priority P0 blocks accessibility or foundation, P1 core, P2 polish.
| Route (files) | Existing, verified | Issues | Redesign | Pri | Verify | Task |
|---|---|---|---|---|---|---|
| Layout and footer (`_Layout.cshtml`, `site.css`, scoped css) | skip link, `container > main[role=main]`, footer, dock only when signed in, 8 scripts at end | F-45, F-26, F-21, F-37 | tokenized footer in normal flow, remove scoped template css, `viewport-fit=cover`, `theme-color`, `scroll-padding-bottom`, `_Icons` sprite | P1 | footer static, link accent, no scoped bundle request | SHELL-002, SHELL-004, SHELL-006 |
| Account/Login | centered card, eyebrow, username and password fields, show-password toggle, submit with arrow | F-03, F-07, F-28 | G3 card over ambient canvas, one blur layer, title-case eyebrow, `_PasswordField`, button lg | P1 | contrast, focus order, toggle name | ROUTE-001 |
| Account/Logout | confirm form with Cancel link | plain | button variants only | P2 | Enter and Esc paths | ROUTE-012 |
| Account/Profile | three password fields with eye toggles, success and error alerts | F-28, F-16 | `_PasswordField` x3, field system, inline validation linking | P1 | each toggle named, errors linked | ROUTE-002 |
| Dashboard (Index) | 4 stat tiles, recent transactions as cards, empty state | F-02, F-35 | one `surface` family, heading order h2 then h3, tiles with edge highlight, list instead of card-per-row | P1 | heading outline, empty state | ROUTE-003 |
| People/Index | availability radios with auto-submit, Customers and Sellers links | F-22 | segmented control, explicit apply or feedback (UX-002) | P1 | keyboard use of radios | ROUTE-004 |
| People/Customers, Sellers | search form, sortable data table, no empty state | F-23, F-24, F-25, F-32 | table system, `aria-sort` plus indicator, `_EmptyState`, `lang` hints, phone card transform optional | P1 | sort announced, empty shown | ROUTE-004 |
| People/CustomerDetails, SellerDetails | detail sections and transaction lists | F-02 | surface family, table system | P2 | visual | ROUTE-005 |
| People/CreateCustomer, CreateSeller | short forms | F-15 | `_FormField`, validation pattern | P1 | label for/id pairs | ROUTE-005 |
| Products/Index | type tabs, availability filter, per-type Create button, product table | F-22, F-27, F-31, F-32 | segmented tabs, one creation entry (UX-001), table system, chips | P1 | tab keyboard model, filter feedback | ROUTE-006 |
| Products/SecondHand | list with table | F-31 | table system | P2 | visual | ROUTE-006 |
| Products/Details | sections plus Bootstrap `.table` of transactions, Apple ID password in `code` | F-31 | table system, password reveal pattern unchanged | P2 | code colour, table idiom | ROUTE-007 |
| Products/Edit (`_ProductEditForm`, 447 lines) | long form with modals | F-15, F-29 | `_FormField`, section hierarchy, action bar G2 | P1 | all labels associated | ROUTE-007 |
| Products/Create x11 (`Create*` and `_ProductCreate*Form`) | 11 near-identical forms plus 11 modal partials, pricing script | F-15, F-29, F-30 | shared field partial, same section order, modal partials get `idPrefix`, inline scripts left unless touched | P1 | grep unlabeled = 0, modals open and submit | ROUTE-008 |
| Products/PersonPicker | handler only | none | none | none | none | none |
| Transactions/Index | filter form with auto-submit, table with row checkboxes and sort headers, bulk actions | F-18, F-22, F-32 | named checkboxes, filter feedback, table system, selection bar | P1 | checkbox names, aria-sort kept | ROUTE-009 |
| Transactions/Sell | two-column form, pickers, date picker, sticky action bar | F-19, F-20 | pickers with keyboard model, date picker grid, action bar G2 | P1 | pick by keyboard only | ROUTE-010 |
| Transactions/Details | summary sections, factor download | F-02 | surface family | P2 | visual | ROUTE-010 |
| Reports/ProfitLoss | filters, KPI tiles, Chart.js trend, tables, inline script | F-22 | segmented range control, token-fed chart colours kept, text alternative table, `aria-describedby` summary | P1 | chart summary read, forced colors | ROUTE-011 |
| Error, Privacy | template pages with dev-mode text and placeholder sentence | F-33, F-34 | production-safe copy, 70ch measure, `<code>` request id kept | P2 | copy review | ROUTE-012 |
| Dock (`NavigationMenu`) | Stage 5/6 behaviour, 6 tabs, flyouts, 22 links | F-27, F-21 | token migration only, grouped Products flyout, clearance | P1 | R24 invariants | SHELL-003, SHELL-007 |

## R15 Component-family specifications
Retain, refactor, consolidate, replace or introduce is stated per row. All families use R11 tokens and R12 states.
| Family | Appearance | Interaction and states | Accessibility | Responsive | Performance and reuse | Existing component decision |
|---|---|---|---|---|---|---|
| Navigation: dock, flyout | G1 regular glass, accent-soft selected tab | Stage 5/6 behaviour unchanged; solid while a modal is open | `nav` label, `aria-current`, `inert` closed flyout (keep); clearance for focus (2.4.11) | compact on touch, safe-area padding | one blur layer; tokens only | RETAIN behaviour, REFACTOR css to tokens (SHELL-003) |
| Navigation: breadcrumbs, sidebar, tab bar, command palette | none exist | none planned | n/a | n/a | n/a | not introduced (no need; dock is the nav) |
| Navigation: page tabs (Products types) | pill segmented control, accent-soft thumb | arrow keys move, Home and End | `role=tablist` or radio group semantics, selected state not colour only | scrolls horizontally if too wide | CSS only | REPLACE `nav-tabs` (COMP-009, ROUTE-006) |
| Buttons | primary accent fill, secondary raised neutral, tertiary text, destructive danger; radius md, lg for page action; weights per R11.C | hover mix, press scale .98, spinner when busy | min 44px coarse, ring, names on icon-only | stack full width at xs in action bars | one rule set | CONSOLIDATE 9 variants to 4 via aliases (COMP-001) |
| Links and inline actions | accent, underlined in text | hover colour; visited unchanged | contrast 7:1+ | n/a | n/a | RETAIN, fix footer link (SHELL-002) |
| Text fields, textarea, search, password | inset surface, border-control, radius md, label above in `--text-2` | focus ring, error border plus icon | label for/id, `aria-describedby`, `aria-invalid`, placeholder 4.5:1 | one column at xs, two from md | `_FormField`, `_PasswordField` | REFACTOR (COMP-002, COMP-004, COMP-005) |
| Select, combobox pickers | same field look, chevron from token (light) | pickers: ArrowUp/Down, Home/End, Enter, Escape, `aria-activedescendant` | combobox pattern with live result count | full-width list at xs | listbox is G3 floating list | REFACTOR (FOUND-002, COMP-013) |
| Checkbox, radio, switch | 1.25rem box, accent checked; switch not used | label click, ring | row min 44px coarse, group `fieldset` and `legend` | n/a | CSS | REFACTOR (COMP-003) |
| Segmented control, tabs, accordion | segmented replaces `nav-pills`/radio filters | roving focus | `aria-selected/pressed` | wraps or scrolls | `segmented` block | INTRODUCE segmented (COMP-009); no accordion |
| Date picker (Jalali) | G3 panel, radius xl, day cells sm | arrow, PageUp/PageDown, Home/End, Escape returns focus | dialog label, live month title, focus trap | bottom-anchored full width at xs | existing script, token-driven | REFACTOR (COMP-014) |
| Sliders, file upload, tooltips, popovers | not used | not introduced | n/a | n/a | n/a | not introduced; icon-only controls use `aria-label` and `title` |
| Cards, nested cards | M2 raised, radius lg, edge highlight | static | landmark or heading per card | grid reflows | `surface` family | CONSOLIDATE `.card`, `.stat`, `.form-section` (COMP-007) |
| Tables, data grids | M1, header 700 `--text-2`, row min 3rem, hairline separators, no zebra, hover surface-3 | sort links with `aria-sort` and arrow | `scope=col`, caption or `aria-label` | phone: horizontal scroll or card transform (UX-005) | one `.data-table` idiom | CONSOLIDATE (COMP-008) |
| Lists, list items | opaque rows, hairline | hover and focus surface-3 | semantic lists | n/a | n/a | REFACTOR with pickers |
| Badges, chips, tags | pill, tinted soft fill plus text (status never colour alone), `--fs-xs` 700 | static | text carries status | n/a | CSS | REFACTOR (COMP-009) |
| Avatars, images, media, previews | none exist | n/a | n/a | n/a | n/a | not introduced |
| Charts, metrics, progress | Chart.js with token colours; KPI tiles | tooltips by Chart.js | data table alternative and summary | canvas resizes | keep one chart lib | RETAIN, add alternative (ROUTE-011) |
| Dividers, section headers | hairline, title-case headers `--fs-lg` | n/a | heading levels in order | n/a | n/a | REFACTOR |
| Pagination, infinite scroll | not present | n/a | n/a | n/a | n/a | not introduced |
| Dialogs and confirmations (Bootstrap modal) | G3 sheet radius xl, G3-solid when large; scrim blur 6 | present spring 460 ms, dismiss 220 ms, focus returns to trigger | `aria-labelledby` on all 29, `aria-modal`, Esc closes | max-height `100dvh - 2rem`, scroll body | `_Modal` shell | REFACTOR (COMP-012) |
| Drawers, sheets | none exist | n/a | n/a | n/a | n/a | not introduced |
| Toasts, notifications | none exist; alerts inline | optional toast for success after redirect (UX-004) | `role=status`, 8 s minimum | bottom above dock | G3 | optional INTRODUCE |
| Alerts and banners | tinted soft fill plus border, icon plus text, no blur | dismissible only when useful | `role=alert` for errors, `role=status` for info and success | full width | opaque | REFACTOR (COMP-010) |
| Loading, empty, error, success, offline | `_EmptyState` with action; button spinner; picker skeleton | see R12.A | `aria-busy`, live regions | n/a | n/a | EXTEND `_EmptyState` (COMP-011) |
| Forms (layout, sections, validation, submit) | `form-section` as `surface`, single column to 40rem, sticky action bar G2 | inline plus summary validation, focus to summary on failure | required marking in text, not colour; error summary links to fields | one column at xs, two from md | `_FormField` | REFACTOR (COMP-004/005, ROUTE-008) |

## R16 Functional UX recommendations (tracked and tested separately from visual work)
Type tags in R18: V visual, A accessibility or markup behaviour, U UX or functional, T tooling or test, D discovery or documentation. Nothing below changes behaviour without owner approval; the three flows that matter most are signing in, recording a sale and creating a product.
| ID | Change | Justification | Expected benefit | Needs approval |
|---|---|---|---|---|
| UX-001 | Group the 11 "Add a ..." links in the Products flyout under one "Create" entry (type chooser or collapsed group); keep the per-type Create buttons on the Products tabs | F-27: 13 flat links and two creation paths | flyout scans in seconds, one mental model | yes (Q-09) |
| UX-002 | Filters: keep auto-apply only with a visible "Updating" state and a live result count, or switch to an explicit Apply button; no-JS path keeps working | F-22, WCAG 3.2.2; arrow-keying a select can submit repeatedly | keyboard users can choose values, fewer reloads | yes (Q-05) |
| UX-003 | Polite live region for result counts and picker results ("12 results") | 4.1.3 partial | screen-reader feedback | no (A11y) |
| UX-004 | Optional toast for the success message after a redirect, inline fallback kept | inline alert pushes content down | calmer feedback | yes, optional |
| UX-005 | Below 576px render the three main tables as stacked cards (needs `data-label` on cells) | F-32 | no horizontal scroll for primary tasks | yes, optional |
| UX-006 | In-app "Solid surfaces" preference (Profile page) | `prefers-reduced-transparency` missing in Safari and Firefox | accessibility parity | yes (D-09) |
| UX-007 | Error page: production-safe wording, keep the Request ID | F-33 | clearer failure | no (copy) |
Information architecture is already small (six dock tabs, one-page sell flow) and is kept; no screen is removed or merged.

## R17 Dependency evaluation
Rule: native CSS first. No library is added to make a glass card. Fields per candidate: purpose, why existing is insufficient, alternatives, compatibility, size and performance, accessibility and maintenance, verdict, migration risk, necessity.
| Candidate | Purpose and why existing is insufficient | Alternatives | Compatibility, size, performance | A11y and maintenance | Verdict and risk | Necessity |
|---|---|---|---|---|---|---|
| Any new runtime library | none needed; CSS custom properties, `linear()` easing and a 273-line dock script already do what the redesign needs | n/a | n/a | n/a | ADD NOTHING; risk none | n/a |
| React, shadcn/ui, Radix UI, React Aria | accessible components; the app is server-rendered Razor Pages with Bootstrap JS, no SPA toolchain | Bootstrap modal, hand-written ARIA combobox (about 150 lines each for the two pickers) | needs Node build and a client framework; hundreds of KB; not compatible with MPA pages | strong a11y, but framework replacement | AVOID; migration risk HIGH | not applicable |
| Motion for React, GSAP | spring and layout animation; CSS `linear()` springs and transitions already do this (dock proves it) | CSS, Web Animations API | 25 to 70 KB each, React variant impossible here | extra reduced-motion handling | AVOID; risk MED | optional never |
| Lucide or Phosphor icons | consistent icon set; today 17 hand-made inline symbols | copy individual SVG paths into the `_Icons` sprite; no icon font | zero runtime cost when copied; ISC (Lucide) and MIT (Phosphor) licences, confirm at adoption | decorative icons aria-hidden | OPTIONAL, copy-paste only; risk LOW | optional |
| Bootstrap 5.3.3 | grid, modal JS, validation classes, dark mode attribute | none worth the churn | already loaded (233 KB css, 81 KB js) | modal focus trap is built in | RETAIN; no version bump in this redesign; risk LOW | mandatory (existing) |
| jQuery and jquery-validation(-unobtrusive) | ASP.NET unobtrusive validation depends on them; `$.validator.setDefaults` is the hook for `aria-invalid` | native Constraint Validation | already loaded | maintained by the platform template | RETAIN; risk LOW | mandatory (existing) |
| Chart.js 4.5.1 | profit/loss trend chart | none | already loaded on one page | canvas needs a table alternative | RETAIN; risk LOW | mandatory (existing) |
| Microsoft.Playwright (.NET), test-only | screenshots, visual comparison, keyboard flows in Chromium, WebKit and Firefox; today there are no UI tests and the sandbox cannot run the app | manual screenshots, Selenium, Cypress (needs Node) | dev-only NuGet; CI would need browser install (extra minutes) and the app started against SQLite | not shipped to users; actively maintained | RECOMMENDED for Stage 16; risk LOW to MED (CI time, flaky pixels); needs approval Q-07 | optional |
| Deque.AxeCore.Playwright 4.9.0, test-only | automated WCAG scan per route | Accessibility Insights (manual), axe browser extension | dev-only; pairs with Playwright | open source engine, confirm licence at adoption | RECOMMENDED with Playwright; risk LOW | optional |
| Stylelint, PostCSS, Sass, Tailwind | linting and authoring | grep gates in R13 | adds a Node toolchain to a .NET-only repo; Tailwind would fight Bootstrap; Sass would need Bootstrap source compilation | n/a | AVOID; risk MED | not needed |
| Open rounded font (for example Nunito, OFL) | replacement if SF Pro Rounded cannot be self-hosted (Q-02) | `ui-rounded` plus system stack | font files comparable in size; adds 500 and 600 weights possibly | licence clarity | CONDITIONAL on Q-02; risk MED (metrics shift) | optional |
| Lighthouse CI or WebPageTest | repeatable performance baseline | Playwright tracing, manual Lighthouse | Node or hosted service | n/a | OPTIONAL; risk LOW | optional |
| View-transition polyfills | cross-browser page transitions | none (native only) | script weight | n/a | AVOID | not needed |

## R18 Roadmap and task catalogue
Universal definition of done for every task: acceptance met, CI green on the code commit (run number recorded), Reviewer Job B PASS, no new hex literal, no new `!important`, no literal duration or easing outside the token files, Stage 5/6 dock invariants unchanged (R24), manual check list answered by the owner for visual tasks. "M/O": mandatory or optional. Effort: S, M, L, XL (relative). Stages 8 to 16 are expanded into Actor-ready steps only when active.

### R18.A Phases and dependencies
| Stage | Mission phase | Depends on | Delivers |
|---|---|---|---|
| (done in this pass) | 0 Discovery, 1 Research and architecture | none | R1 to R17, R19 to R24 |
| 7 | 2 Foundation | none | tokens, dark-mode flag, dead-CSS cleanup |
| 8 | 3 Shared components (controls) | 7 | buttons, fields, checks, validation, label association, type tokens |
| 9 | 3 Shared components (surfaces) | 7, 8 | surfaces, tables, chips, segmented control, alerts, empty states, canvas shell |
| 10 | 3 Shared components (overlays) | 8, 9 | modals, pickers, date picker, scrim |
| 11 | 4 Application shell | 8, 9, 10 | dock tokens, footer, focus clearance, action bar, mobile shell |
| 12, 13, 14 | 5 Page-by-page | 8 to 11 | all routes in three batches |
| 15 | 6 Motion and polish | 10 to 14 | motion tokens, press feedback, preference modes, solid-surfaces switch |
| 16 | 7 Quality assurance | all | harness decision, baselines, a11y sweep, performance, final audit |
COMP-004 (label association) has no visual dependency and may move earlier or run in parallel if the owner wants the accessibility fix first.

### R18.B Task catalogue
Columns: Stage, Type (R16 tags), Problem and change, Files, Depends on, Acceptance and verify, Risk, Priority, Effort, M/O.
| ID | Stg | T | Problem and change | Files | Depends | Acceptance and verify | Risk | Pri | Eff | M/O |
|---|---|---|---|---|---|---|---|---|---|---|
| DISC-001 | pre | D | Map repo, rules, pipeline | none | none | rules read, structure mapped; DONE (R3) | LOW | P0 | M | M |
| DISC-002 | pre | D | Route and component inventory | none | none | 33 routes, 19 partials, 1 view component, 5 css plus 1 scoped, 8 js listed; DONE (R3, R14) | LOW | P0 | M | M |
| DISC-003 | pre | D | Static UI/UX and accessibility audit | none | DISC-002 | 45 findings with evidence tags; DONE (R4, R5); browser confirmation moves to QA-001 | LOW | P0 | L | M |
| DISC-004 | pre | D | CI baseline evidence | none | none | last code commit `597fba4` CI #618 success [CI]; DONE | LOW | P0 | S | M |
| DISC-005 | pre | T | Browser baseline screenshots of every route, state and viewport | harness outside the repo or Playwright script | Q-07 | stored outside the repo or as CI artifact; OPEN, blocked here (no .NET, browser or NuGet); folded into QA-001 | LOW | P0 | M | M |
| RES-001 | pre | D | Apple official guidance | none | none | WWDC25 219, 102 and Adopting Liquid Glass read, HIG Materials via mirror; DONE (R8) | LOW | P0 | M | M |
| RES-002 | pre | D | Browser capability research | none | none | support facts and pitfalls recorded; DONE (R6) | LOW | P0 | M | M |
| RES-003 | pre | D | Community and gallery references | none | none | recorded with limits; DONE (R8) | LOW | P2 | S | O |
| RES-004 | pre | D | WCAG 2.2 criteria mapping | none | none | R5 table; DONE | LOW | P0 | S | M |
| RES-005 | pre | D | SF Pro Rounded licence check by the owner | none | none | decision recorded as Q-02; OPEN | MED | P1 | S | M |
| FOUND-001 | 7 | V | Token layer v1 (Step 7.1) | `tokens.css` | none | block appended verbatim, 75 new lines, no collision, no hex; CI green | LOW | P0 | S | M |
| FOUND-002 | 7 | V | Bootstrap dark-mode flag and colour-scheme meta (Step 7.2): fixes F-12, F-13, F-44 | `_Layout.cshtml` | none | attribute and meta present, BOM kept; owner check list 1 to 6 | MED | P0 | S | M |
| FOUND-003 | 7 | V | Remove dead rules from `site.css` (Step 7.3) | `site.css` | none | zero `form-floating`, zero focus-ring; CI green | LOW | P2 | S | O |
| FOUND-004 | 7 | D | Stage 7 validation (Step 7.4) | none | FOUND-001..003 | greps and `git diff --stat` recorded | LOW | P0 | S | M |
| COMP-001 | 8 | V | 9 button variants to 4 via class aliases, radius md, press scale, no lift, coarse 44px | new `controls.css`, `app-theme.css`, `_Layout.cshtml` link | FOUND-001 | variants per R15; no Razor class edits; `translateY(-1px)` count 0; owner check on 5 pages | MED | P1 | M | M |
| COMP-002 | 8 | V | Field system on tokens: inset surface, border-control, chevron from token, invalid state, no rgba literals, placeholder 4.5:1 | `controls.css`, `app-theme.css` | COMP-001 | `.input-validation-error` and `.is-invalid` styled; rgba literals in field rules 0; owner check | MED | P1 | M | M |
| COMP-003 | 8 | V | Checkbox and radio on tokens | `controls.css` | COMP-002 | 44px coarse rows; ring visible | LOW | P1 | S | M |
| COMP-004a | 8 | A | Label association, Create pages AppleId, Cable, Case, Charger, Glass, Laptop | 6 views | none | zero `<label class="form-label">` without for/asp-for in these files; ids unique; page-model tests green | MED | P0 | M | M |
| COMP-004b | 8 | A | Label association, Create pages Phone, PortableStorage, PowerBank, SmartWatch, Tablet | 5 views | none | same grep in these files | MED | P0 | M | M |
| COMP-004c | 8 | A | Label association, Edit and `_ProductEditForm` | 2 views | none | same grep | MED | P0 | M | M |
| COMP-004d | 8 | A | Label association, 11 modal partials with an `idPrefix` parameter | 11 partials, callers | none | repo-wide grep = 0 (documented captions excepted); modals still submit | MED | P0 | L | M |
| COMP-005 | 8 | A | Validation pattern: `aria-invalid`, `aria-describedby`, summary focus, live region | `site.js`, `_FormField.cshtml` | COMP-002 | invalid fields flagged and linked in browser | MED | P1 | M | M |
| COMP-006 | 8 | V | Type tokens applied: only 400/700, no uppercase, H1 scale, drop Vazirmatn 500 face | `tokens.css`, `app-theme.css`, `navigation-menu.css` (eyebrow text-transform only) | COMP-001 | no weight other than 400/700, no `text-transform: uppercase`, fonts about 269 KB lighter | LOW | P2 | S | M |
| COMP-007 | 9 | V | Surface family replaces `.card`, `.stat`, `.form-section`, `.empty-state`, `.page-head` styling | new `surfaces.css`, `app-theme.css` | COMP-001 | one radius, fill, shadow per level; owner visual check | MED | P1 | M | M |
| COMP-008 | 9 | V | One table idiom, sort pattern, header 700, row 3rem, hover surface-3 | `surfaces.css`, `Products/Details.cshtml` | COMP-007 | Bootstrap `.table` uses 0; `aria-sort` kept | MED | P1 | M | M |
| COMP-009 | 9 | V | Chips on soft fills with text; segmented control replaces `nav-tabs` and radio filters | `surfaces.css`, `controls.css`, Products/People views | COMP-001 | chips >= 4.5:1; arrow keys move selection | MED | P1 | M | M |
| COMP-010 | 9 | V | Alerts on soft fills with icon, no blur, roles by severity | `surfaces.css` | COMP-007 | `.alert` has no `backdrop-filter` | LOW | P1 | S | M |
| COMP-011 | 9 | U | `_EmptyState` with icon and action; used in Customers and Sellers; busy button spinner | `_EmptyState.cshtml`, People lists, `controls.css` | COMP-007 | empty lists show the state | LOW | P2 | S | M |
| SHELL-001 | 9 | V | Canvas shell: transparent main at 768px and up, ambient layer, no card-on-card | new `shell.css`, `app-theme.css` | COMP-007, Q-04 | ambient visible, contrast unaffected | MED | P1 | M | O (approval) |
| COMP-012 | 10 | A | Modal sheets: G3 and G3-solid, scrim, `aria-labelledby` on all 29, focus return, `_Modal` shell | new `overlays.css`, modal pages | FOUND-002, COMP-007 | 29 of 29 labelled; blur layers only scrim plus sheet | MED | P1 | L | M |
| COMP-013 | 10 | A | Pickers keyboard model: arrows, Home, End, Enter, Escape, `aria-activedescendant`, live count | `person-picker.js`, `product-picker.js`, partials | COMP-012 | picker usable with keyboard only | MED | P0 | L | M |
| COMP-014 | 10 | A | Jalali date picker: G3 panel, grid keys, focus trap and return, live title | `jalali-datepicker.js`, `overlays.css` | COMP-012 | arrows, PageUp/PageDown, Home/End, Escape | MED | P1 | L | M |
| COMP-015 | 10 | V | Floating lists and menus on G3, hover and focus tokens | `overlays.css` | COMP-013 | no bright hover anywhere | LOW | P2 | S | M |
| SHELL-002 | 11 | V | Delete scoped template css and its link; tokenized footer in normal flow (F-45) | `_Layout.cshtml`, `_Layout.cshtml.css`, `shell.css` | SHELL-001 or COMP-007 | computed style: footer static, link accent with hover; no `MobileShop.Web.styles.css` request | MED | P1 | S | M |
| SHELL-003 | 11 | V | Dock tokens migration, behaviour unchanged | `navigation-menu.css` | FOUND-001 | R24 invariants pass, no hex | MED | P1 | M | M |
| SHELL-004 | 11 | A | Focus clearance: `scroll-padding-bottom` for dock and action bar, `scroll-margin` on fields (2.4.11) | `shell.css` | SHELL-003 | tabbing to the last field is never obscured at 320x568 and 1280x720 | LOW | P0 | S | M |
| SHELL-005 | 11 | V | Action bar G2 plus scroll-edge fade, solid under modal | `shell.css` | COMP-001 | contrast per R10, solid fallback | MED | P1 | M | M |
| SHELL-006 | 11 | A | Mobile shell: `viewport-fit=cover`, `theme-color`, `dvh`, coarse 44px, safe areas | `_Layout.cshtml`, `shell.css` | SHELL-003 | insets effective on iPhone; no 100vh overshoot | MED | P1 | M | M |
| SHELL-007 | 11 | U | Group Products flyout creation links (UX-001) | `Default.cshtml`, nav css and js | Q-09 | flyout shows at most 5 rows | MED | P2 | M | O (approval) |
| SHELL-008 | 11 | A | Landmark and skip-link polish | `_Layout.cshtml` | none | axe landmark rules clean | LOW | P2 | S | O |
| SHELL-009 | 11 | V | Cross-document view transitions, dock `view-transition-name` | `shell.css`, `motion.css` | SHELL-003, MOTION-001 | no flash; Firefox unaffected | MED | P3 | M | O |
| ROUTE-001 | 12 | V | Login: G3 card over ambient canvas, one blur, `_PasswordField` | `Login.cshtml`, partial, css | COMP-002, COMP-012 | contrast, toggle named | LOW | P1 | M | M |
| ROUTE-002 | 12 | V | Profile: `_PasswordField` x3, field system | `Profile.cshtml` | ROUTE-001 | errors linked | LOW | P1 | S | M |
| ROUTE-003 | 12 | V | Dashboard: tiles, heading order, list instead of cards | `Index.cshtml` | COMP-007 | heading outline h1, h2, h3 | LOW | P1 | S | M |
| ROUTE-004 | 12 | A | People lists: segmented filter, `aria-sort` plus indicator, empty state, `lang` hints | `People/Index`, `Customers`, `Sellers` | COMP-008, COMP-009, COMP-011 | sort announced, empty shown | MED | P1 | M | M |
| ROUTE-005 | 12 | V | People details and create forms | 4 views | COMP-004 | label pairs, surfaces | LOW | P2 | M | M |
| ROUTE-006 | 13 | V | Products list and SecondHand: segmented tabs, chips, table | 2 views | COMP-009 | tab keyboard model | MED | P1 | M | M |
| ROUTE-007 | 13 | V | Products Details and Edit | 3 files | COMP-004c | one table idiom, labels | MED | P1 | L | M |
| ROUTE-008a | 13 | V | Create forms group 1 on `_FormField` | 6 views | COMP-004a | same section order, no behaviour change | MED | P1 | L | M |
| ROUTE-008b | 13 | V | Create forms group 2 on `_FormField` | 5 views | COMP-004b | as above | MED | P1 | L | M |
| ROUTE-008c | 13 | V | Modal create partials on `_FormField` | 11 partials | COMP-004d | modals open and submit | MED | P1 | L | M |
| ROUTE-009 | 14 | A | Transactions list: named checkboxes (F-18), filter feedback, table | `Transactions/Index.cshtml` | COMP-008 | every checkbox has a name | MED | P0 | M | M |
| ROUTE-010 | 14 | V | Sell and Transactions Details on new pickers, date picker, action bar | 2 views | COMP-013, COMP-014 | keyboard-only sale possible | MED | P1 | L | M |
| ROUTE-011 | 14 | V | ProfitLoss: segmented range, chart text alternative | `ProfitLoss.cshtml` | COMP-009 | summary readable, forced colors | MED | P1 | M | M |
| ROUTE-012 | 14 | V | Error, Privacy, Logout copy and measure | 3 views | none | no dev text for users | LOW | P2 | S | M |
| MOTION-001 | 15 | V | Replace literal durations and easings with tokens | all css | FOUND-001 | no literal ms or cubic-bezier outside token, motion and dock files | LOW | P1 | M | M |
| MOTION-002 | 15 | V | Press feedback, remove hover lift (F-41) | `motion.css` | COMP-001 | `translateY(-1px)` count 0 | LOW | P1 | S | M |
| MOTION-003 | 15 | V | Sheet and popover present and dismiss | `overlays.css`, `motion.css` | COMP-012 | 460 ms spring in, 220 ms out; instant under reduced motion | MED | P1 | M | M |
| MOTION-004 | 15 | A | Preference modes block: motion, transparency, contrast, forced colors | `motion.css`, `tokens.css` | MOTION-003 | each row of R12.B verified | MED | P0 | M | M |
| MOTION-005 | 15 | U | Solid-surfaces switch plus pre-paint attribute (UX-006) | Profile, `site.js`, Layout | MOTION-004 | works in Safari and Firefox, persisted in guarded storage | MED | P1 | M | O (approval) |
| MOTION-006 | 15 | U | Busy state and double-submit guard | `site.js` | COMP-011 | one POST per click | MED | P2 | S | O |
| QA-001 | 16 | T | Browser harness decision and baseline screenshots (33 routes, 3 viewports, key states) | outside repo or test project | Q-07 | baselines stored; computed-style checks for F-12, F-13, F-44, F-45 answered | LOW | P0 | L | M |
| QA-002 | 16 | T | Accessibility sweep: axe, keyboard-only flows, spot screen-reader check | all routes | QA-001 | 0 critical and 0 serious findings; sale and product creation by keyboard | MED | P0 | L | M |
| QA-003 | 16 | T | Contrast measurement of glass over white and accent strips | script, outside repo | MOTION-004 | G2 and G3 muted >= 4.5:1, G1 documented | LOW | P1 | S | M |
| QA-004 | 16 | T | Performance measurement against the pre-redesign baseline | none | QA-001 | R7 budgets met or exceptions recorded | LOW | P1 | M | M |
| QA-005 | 16 | T | Safari, iOS and Firefox manual matrix by the owner | none | all | all stage checklists pass | LOW | P0 | M | M |
| QA-006 | 16 | V | Final consistency audit and cleanup: grep gates, delete `app-theme.css` and retired tokens, drop redundant script includes | css, views | all | R13 gates pass; css <= 60 KB and <= 8 files | MED | P1 | M | M |
| QA-007 | 16 | D | Handoff: owner trims this Reference into a README section | `README.md` (owner) | QA-006 | README updated by the owner | LOW | P2 | S | O |

## R19 Baseline and verification strategy
- Build, run, test (from README and project.md): `dotnet build src/MobileShop.slnx`, `dotnet run --project src/MobileShop.Web` (http://localhost:5043, SQLite, development seeding, documented default admin account in README). CI (`.github/workflows/dotnet.yml`) is the only gate; agents do not run build or test locally unless asked.
- Known baseline [CI]: last code commit `597fba4` (Stage 6) run #618 success; #617 and #616 success; HEAD `6f2b88d` is README-only and CI ignores it. No pre-existing failure observed. No UI, markup or JS tests exist.
- Not captured in this pass (blocked: the sandbox has no .NET SDK, no browser and no NuGet access): screenshots, interaction states, performance numbers. Recommended before Stage 8, by the owner: capture the baseline set below (DISC-005, then folded into QA-001).
- Baseline set: all 33 routes (PersonPicker excluded, no UI) at 360x740, 768x1024, 1440x900 in dark Chromium; key states: login error, Customers and Sellers with no rows, validation errors on Sell and one Create page, an open modal, an open picker list, the date picker, the dock expanded with a flyout, ProfitLoss with data, Error page. Store outside the repo (or as a CI artifact) as `route-viewport-state.png`.
- Functional regression list (owner, every visual stage): sign in and out; dock navigation to all tabs; create a phone and one accessory by page and by modal from Sell; record a sale with both pickers and the date picker; Transactions filter, sort, select; People filter and sort; Profile page opens (do not change the real password); ProfitLoss range change; invoice or factor download still works.
- Automated option (needs Q-07): a Playwright for .NET test project with screenshots, an axe scan per route and keyboard flows, first as a non-blocking CI job; mask dates and ids; start with a pixel-ratio threshold of 0.2%.
- Performance baseline before Stage 8, after Stage 11 and after Stage 16: Lighthouse mobile on Login, Dashboard, Products/Index, Transactions/Sell (LCP, CLS, INP or TBT, transferred bytes, request count); paint-flashing and layer-border view to count backdrop layers; one mid-tier Android scroll and animation check.
- Browser and OS assumptions: Chrome and Edge latest on Windows and macOS, Safari latest on macOS and iOS, Firefox latest, Chrome on a mid-tier Android phone.
- Failure handling: record pre-existing failures separately in act.md ("Pre-existing") so they are never counted as redesign regressions.
- Definition of done for the whole redesign: every route row in R14 closed; every R15 family implemented with all R12 states; R13 gates pass in the final grep audit; responsive checks done at the baseline viewports; axe 0 critical and 0 serious, keyboard-only completes sign-in, sale and product creation; regression list passes; build, tests and CI green; performance within R7 budgets or recorded exceptions; no known high-priority inconsistency left without a written reason; fallbacks verified (no `backdrop-filter`, reduced transparency, increased contrast, forced colors, reduced motion); this file's status lines updated after each milestone.

## R20 Decision log
| ID | Decision | Rationale | Status |
|---|---|---|---|
| D-01 | This file holds the Reference plus the active stage's steps | owner mission asks for one authoritative file; conflicts with `planner.md` "active stage only" | needs approval (Q-01) |
| D-02 | Stay on Razor Pages, Bootstrap 5.3.3, vanilla CSS and JS | framework change is out of scope and unjustified (R17) | decided |
| D-03 | Dark-first, brand palette preserved, light theme optional | brand identity; Q-03 | decided, light open |
| D-04 | Glass only on the navigation and overlay layer; Regular only; Clear reserved | Apple guidance (WWDC25 219, HIG); no media in the app | proposed |
| D-05 | Strangler CSS migration: new family files, `app-theme.css` shrinks and is deleted at Stage 16; no big-bang split | an exact-move split would mix families and a reorder cannot be verified mechanically | proposed |
| D-06 | `data-bs-theme="dark"` as the base for unthemed Bootstrap defaults | fixes F-12, F-13, F-44 in one line instead of re-overriding each variable | proposed (Step 7.2) |
| D-07 | Keep SF Pro Rounded until the licence question is answered | F-40, Q-02 | open |
| D-08 | Rejected as baseline: SVG-displacement refraction, pointer-tracking lighting, WebGL, canvas, shaders | Chromium-only or costly, little value for a dense admin UI | proposed |
| D-09 | In-app solid-surfaces switch | `prefers-reduced-transparency` is Chromium-only | proposed (MOTION-005) |
| D-10 | Cross-document view transitions are optional and late | Chromium and Safari only; must not disturb dock invariants | proposed (SHELL-009) |
| D-11 | Accessible forms via Razor partials (`_FormField`, `_PasswordField`) and `asp-for` or `for`/`id` labels | matches repo style, no TagHelper learning curve | proposed |
| D-12 | New semantic token prefixes beside legacy primitives; `--r-*` retired by Stage 10 | additive foundation, no mass rename | proposed |
| D-13 | Scoped template CSS removal deferred to Stage 11 | it changes footer layout (F-45) and needs a browser check | proposed |
| D-14 | CI stays the only gate; browser harness and test packages optional | project rules; Q-07 | decided, packages open |
| D-15 | UI stays English LTR; no RTL mirror | Q-08 | assumption |

## R21 Risks and trade-offs
| ID | Risk | Mitigation |
|---|---|---|
| RK-01 | Safari and Firefox parity (no reduced-transparency media query) | D-09 switch; every effect has a solid fallback; WebKit and Gecko checks in QA-005 |
| RK-02 | Backdrop blur cost on low-end phones | max 2 layers, no animated filters, QA-004 on a mid-tier phone |
| RK-03 | CI cannot see CSS, so visual regressions slip through | owner manual check list per stage, baseline capture before Stage 8, optional Playwright |
| RK-04 | Overriding plain-CSS Bootstrap relies on cascade order | new files link after Bootstrap, no `!important`, R13 gate |
| RK-05 | Dock regressions during token migration | R24 invariants as acceptance in SHELL-003; Stage 6 was measured in Chromium only |
| RK-06 | `backdrop-filter` makes a containing block for fixed children | overlays stay outside glass parents (R13 rule 2) |
| RK-07 | Label association touches 111 labels in 19 files; id collisions inside modals | `idPrefix`, grep gates, page-model tests, small sub-tasks 004a to 004d |
| RK-08 | Replacing the font shifts metrics | decide Q-02 before Stage 8; recheck line heights |
| RK-09 | UX changes smuggled in with visuals | separate `UX-*` items, approvals, separate commits |
| RK-10 | Persian text and the new type scale | test with real Persian data; line-height 1.7 rule |
| RK-11 | Long plan burdens the Reviewer | plan map and grep helpers; trim the Reference at QA-007 |
Trade-offs: readable opaque content over a more "glassy" look; Bootstrap kept over a full custom system (less churn); no runtime library over faster authoring; ten small stages over fewer large ones so the app works after every commit.

## R22 Open questions and assumptions (owner decisions)
| ID | Question | Default if unanswered |
|---|---|---|
| Q-01 | Approve the ten stages and D-01 (plan structure)? | Reviewer decides; Stage 7 only |
| Q-02 | SF Pro Rounded: keep, replace with an open rounded font, or `ui-rounded` plus system stack? | keep for now |
| Q-03 | Add a light theme or stay dark-only? | dark-only |
| Q-04 | Approve the canvas shell (transparent `main` from 768px)? | keep the panel; SHELL-001 skipped |
| Q-05 | Filters: auto-apply with feedback or an explicit Apply button? | auto-apply with feedback |
| Q-06 | Browser targets: last two majors of Chrome, Edge, Safari (macOS and iOS), Firefox? | as stated |
| Q-07 | Approve test-only Playwright and axe packages plus a non-blocking CI job? | no packages; owner-run baselines |
| Q-08 | Confirm UI stays English LTR (Persian as data only)? | yes |
| Q-09 | Group the Products flyout creation links (UX-001)? | no change |
| Q-10 | Confirm PDF invoice and factor styling is out of scope? | yes |
Assumptions: A-01 dark is the primary theme; A-02 desktop first, phone and tablet secondary; A-03 local shop deployment, modest performance budgets; A-04 no CSP in use; A-05 the documented development admin account is used only locally.

## R23 Coverage: what was and was not inspected
- Read in full: `tokens.css`, `site.css`, `components.css`, `app-theme.css` (all 1049 lines), `_Layout.cshtml` and its scoped css, the NavigationMenu markup and view component, `site.js`; pages Index, Login, People/Index, Customers, Transactions/Index, Sell, Details, Products/Index, SecondHand, Error, Privacy, CreateAppleId plus its partial, Profile (most), `navigation-menu.css` lines 1 to 130, the whole Stage 6 plan.
- Read partially or by grep only: `navigation-menu.css` (about 415 remaining lines), `navigation-menu.js`, `jalali-datepicker.js`, `person-picker.js`, `product-picker.js`, `product-create-modal.js`, `profit-loss-chart.js`, `create-product-pricing.js`; CreatePhone and the other create pages (structure greps), `_ProductEditForm`, Products/Details and Edit (head only), CustomerDetails and SellerDetails (head), Sellers (counts), `_PersonPicker` (class greps), ProfitLoss (markup head and filters); `.cshtml.cs` handlers, test contents and `docs/*` were not read.
- Not inspected: `MobileShop.Api`, QuestPDF output, EF and database code, scripts.
- Not done: no app run, no browser, no screenshots, no interaction states, no performance numbers, no keyboard or screen-reader test; computed styles for F-12, F-13, F-44, F-45 are unconfirmed; HIG Materials was read through a mirror; WCAG page not opened in full; community repositories and Dribbble shots at search-result level; Awwwards not reached; Apple font licence not researched.
- Not changed: no production code, `TODO.md`, `act.md`, `audit.md`; nothing committed.
- Pre-existing failures: none observed (CI #616 to #618 green).

## R24 Stage 5 and 6 carry-over (still binding)
- Behaviour: the dock is compact when idle (fine pointers) or while the page scrolls and springs to full size on hover, focus or an open flyout; touch: not compact at rest, compact while scrolling; reduced motion: never compact; keyboard Tab expands immediately; `is-initializing` keeps the first paint compact for two frames; clicks inside a 190 ms gate after expansion are dropped on purpose (do not lower it, the spring needs 175 to 197 ms to reach 99%).
- Glass (G1 values): 78% tint, blur 20, saturate 1.5, brightness .6, tuned by measurement in Chromium: muted label 5.0:1 over realistic content, 4.8:1 over a full-width teal strip, `--ink` at least 9.6:1. Known limits (documented, not defects): after a full page load with a stationary pointer the dock starts compact until the first pointer move; over an all-white strip the muted label is 4.3:1 (the dark theme has none).
- Stage 5 invariants that must still hold: spring tokens (`--nav-spring`, `--nav-ease-out`, `--nav-ease-in-out`) unchanged, real-height flyout, focus ring on all 6 tabs, per-property delays `0s, 0s, 0.082s, 0.082s`, no keyframes in the dock css, no hex literals, `grep -c "::before" navigation-menu.css` = 2, one `.is-compact`, one `.is-initializing` and one `.is-settling` rule set. Only Chromium was measured; Safari, iOS and Firefox were manual checks.
- SHELL-003 must prove all of the above still hold after the token migration.

## Final recommendation for the first implementation milestone
Approve the stage list (Q-01), then run Stage 7 as three small commits (Steps 7.1, 7.2, 7.3) plus validation. Before Stage 8 the owner should capture the baseline screenshots (DISC-005) and answer Q-02 (font licence), Q-03 and Q-04. Stage 8 should start with the label association sub-tasks (COMP-004a to 004d): they are the highest-severity accessibility finding, independent of the visual work, and small enough to review one file group at a time.
