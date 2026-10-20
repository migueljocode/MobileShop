# Plan — Stages 7–16 — Liquid Glass UI/UX redesign (all stages)

## Pipeline note
- Role: Planner. Planning only: this pass changed no production file. Stages 7-16 are committed, unchecked, in `.agents/TODO.md` (commit `6e913d1`). This revision writes every stage's steps at the owner's request ("write plan with all 7-16 stages at once"), which departs from `planner.md` ("active stage only, overwrite"); the owner's request settles D-01. Reviewer Job A approved Stage 7 and the stage list (`audit.md`: APPROVED). Stage 8 has been reviewed (Job A: APPROVED WITH CORRECTIONS, both HIGH findings folded in), completed and verified; Stage 9 Job A ran and returned APPROVED WITH CORRECTIONS with 2 HIGH findings, both folded in here; Stage 10 Job A ran and returned APPROVED WITH CORRECTIONS and its 3 HIGH findings are folded in here (per-page modal id uniqueness in 10.2, the reduce-transparency owner check moved out of 10.7 to Step 15.1, and the `wireComboboxKeys` definition site pinned to a top-level global in site.js); Stage 11 Job A ran and returned APPROVED WITH CORRECTIONS with 4 HIGH findings, all folded in here (the phone safe-area rule of the dock in 11.4, the bin/obj greps of 11.1 and 11.2, and the Vazirmatn font-asset repair added as Step 15.2); Stages 12-16 and the corrections below have NOT been reviewed yet: Reviewer Job A is needed again.
- Reviewer notes folded in: baseline-capture precondition on Step 7.2 and re-rated risk; CRLF-safe, portable commands in Step 7.1; `--success` and `--info` tokens; Step 7.4 template fields; Stage 8 ordered accessibility-first inside the fixed stage title (D-16); security-sensitive list (R25); end-state CSS file list and budgets (R7); font preload (Step 11.4); only one new partial (D-11); wording fixes (WCAG levels of 3.2.6 and 3.3.7, Chrome 119, button height, plaintext Apple ID `code`).
- Corrections found while planning Stages 8-16 (please re-verify): (a) the 11 `_ProductCreate*Form` partials, `product-create-modal.js` and `ProductCreateRegistry.cs` are dead code (`grep -rn "product-create-modal" src --include=*.cshtml` finds no include; `grep -rn "ProductCreateRegistry" src` finds only its own file; no markup defines `create-product-modal`), so the live unassociated-label count is 12 (the one read-only "Finished price (Toman)" label on each of the 11 Create pages and on Edit), not 111 (F-15 is LOW, R26); the earlier Reviewer note M1 rested on the old claim. (b) Products type "tabs" are plain link buttons; `.nav-tabs` exists only on ProfitLoss. (c) Picker result lists are in-flow, so they stay opaque instead of glass. (d) F-19 is stronger than first written: `person-picker.js` hides its list on blur and has no key handler, so keyboard users cannot select (confirmed by reading the file), while the date picker has a typed path (F-20 LOW). (e) R24 now quotes the real dock invariants. (f) Page `Scripts` sections render before `site.js` (Step 10.4 accounts for it).
- Evidence tags in the Reference: [S] source read, [G] grep count, [C] computed by script, [CI] GitHub Actions API, [W] web research, [A] assumption, [U] unverified, needs a browser or device. The committed knowledge graph (`graphify-out`, built at `6f2b88d`) was used to orient on JS functions and `.cshtml` files; it does not cover CSS.

## Reviewer Briefing
- Review scope: Stages 8-16 (48 steps in total, Stage 7 already approved with notes folded) plus the corrections listed in the Pipeline note. Check the stage list order and the Stage 8 internal order (D-16).
- MEDIUM-risk steps (CSS the CI cannot see, or shared JS): 8.3, 8.4, 9.1, 9.2, 9.3, 9.6, 10.1, 10.3, 10.5, 10.6, 11.1, 11.2, 11.3, 11.4, 12.2, 15.1, 15.2, 15.4, 16.1. Every CSS step carries the same mechanical checks (extraction, brace balance, anchor greps) and an owner visual check at the end of its stage; all other steps are LOW.
- All new CSS comes from the spec blocks in R27: each parses without errors, has balanced braces, uses only defined custom properties and no hex literals; old-versus-new selector coverage was compared and the dropped selectors are listed there. Please spot-check two blocks against the rules they replace (suggest `controls-fields-v1` and `overlays-jalali-v1`).
- Evidence limits (unchanged): static reading, greps, computed contrast and GitHub Actions data; no .NET SDK, browser or NuGet in the sandbox, so every visual effect is tagged for the owner's manual check.
- Gated or deferred: Step 9.6 (owner decision Q-04, default skipped); flyout grouping, view transitions, toasts, phone card tables and the double-submit guard have no step. Dead-code removal (R26) is a separate owner decision (Q-12).
- Open owner decisions are in R22; none blocks Stage 7, Q-02 and Q-03 should be answered before Stage 8.

## Plan map (for a reader with only this file)
- Actor: do the next unchecked `## [ ] Step` in order; each step is self-contained. CSS comes from the blocks in R27 (extract with the command in "Hard constraints"); tokens come from the block in R11.A (Step 7.1). You do not need to read the Reference to execute a step; read only the step you are on (`sed -n` on its heading range) and the sections it names.
- Reviewer: Job A reads this section, the briefing and the steps; Job B checks one step against its commit.
- Reference: audit (R4-R7), research (R8), design system (R9-R13), step map (R14), component specs (R15), UX changes (R16), dependencies (R17), phases and step index (R18), verification (R19), decisions, risks, questions (R20-R22), coverage gaps (R23), dock carry-over (R24), security-sensitive UI (R25), dead code (R26), CSS spec blocks (R27).
- Find things fast: `grep -n "^## \[ \] Step" .agents/chat/plan.md`, `grep -n "^## R[0-9]" .agents/chat/plan.md`, `grep -n "BEGIN .*-v1" .agents/chat/plan.md`.

## Hard constraints (all stages)
- Follow `AGENTS.md` and `.agents/project.md`; minimal change; no new dependency; no new file except those a step names; no behaviour change except where a step says so; one step per pass and per commit. Preserve each file's existing encoding and line endings (`_Layout.cshtml` has a UTF-8 BOM and LF).
- Standard CSS step checks (used by every CSS step):
  - EXTRACT a spec block: `sed -n '/^\/\* BEGIN NAME \*\//,/^\/\* END NAME \*\//p' .agents/chat/plan.md | tr -d '\r'` (`> file` to create, `>> file` after a leading `printf '\n'` to append). Never retype a block by hand.
  - BRACE BALANCE for every CSS file a step touched: `awk '{o+=gsub(/\{/,"{");c+=gsub(/\}/,"}")} END{print o"/"c}' FILE` must print two equal numbers.
  - ANCHOR DELETION: find old rules by their selector text (`grep -n`), never by remembered line numbers (earlier steps shift them); delete from the first anchor rule through the closing brace of the last anchor rule, plus one adjacent blank line; `git diff --stat` must show only deletions for that file.
  - LINK POSITION: a new stylesheet link goes on its own line immediately before the `navigation-menu.css` link, with the same attributes (`asp-append-version="true"`) as its neighbours; a link that replaces a stylesheet keeps the replaced file's position (Step 9.1's `base.css` takes the place of `site.css`, after `tokens.css` and before `app-theme.css`).
- No new hex literal in any CSS outside `tokens.css`, no new `!important` (end state at most 12), no literal duration or easing outside `tokens.css` and the dock (the `0.01ms` of the reduced-motion rule in `base.css` is the one exception). `backdrop-filter` only in `overlays.css`, `shell.css`, `login.css`, `motion.css`, the dock and `tokens.css`.
- The dock (`navigation-menu.css`, `navigation-menu.js`) is touched only by Steps 11.3 and 11.4 and must keep the invariants of R24.
- The dead create-product subsystem (R26) is never edited.
- Owner steps (16.2) and gated steps (9.6) are marked; the Actor never invents the owner's answer.

# Stage 7 — Liquid Glass foundation: design tokens, Bootstrap dark mode, dead-CSS cleanup (no layout change)
Inert tokens first, then the one-line Bootstrap dark flag, then dead-rule removal. Only `tokens.css`, `_Layout.cshtml` and `site.css` change.

## [ ] Step 7.1 — Add the design-token layer (additive and inert)
- Task: FOUND-001 · Priority P0 · Effort S · Mandatory.
- Files: modify: `src/MobileShop.Web/wwwroot/css/tokens.css`; do not touch: every other file.
- Symbols: a second `:root` block plus one `@supports not (transition-timing-function: linear(0, 1))` block appended after the existing `:root` block.
- Current -> Desired: `tokens.css` has three `@font-face` rules and one `:root` with the legacy tokens (`--ground`, `--panel`, `--panel-raised`, `--line`, `--line-strong`, `--ink`, `--ink-muted`, `--accent*`, `--warn*`, `--danger*`, `--r-*`, `--fs-*`, `--focus-ring`, `--nav-dock-height`, `--font-body`). Desired: the same file plus the `tokens-v1` block (new names only, all derived from the legacy tokens; nothing consumes them yet).
- Change: from the repo root run exactly
  `{ printf '\n'; sed -n '/^\/\* BEGIN tokens-v1 \*\//,/^\/\* END tokens-v1 \*\//p' .agents/chat/plan.md | tr -d '\r'; } >> src/MobileShop.Web/wwwroot/css/tokens.css`
  This appends a blank line and the whole block (both marker comment lines included) at the end of the file. Do not edit, reorder or remove any existing line, and do not retype the block by hand.
- Depends on: none.
- Edge cases / error handling: the file must still end with exactly one newline; no existing token name may be redefined (checked below); the block contains no hex literal (colours derive from `var()` and `color-mix()`); the extraction strips any CR so a CRLF checkout cannot leak into the file; the `--ease-spring` fallback for browsers without `linear()` is inside the block; nothing references the new tokens, so every page must render exactly as before.
- Tests: none (no CSS test infrastructure).
- Verify: CI green (cite `Action: #<run>`). `git diff --numstat src/MobileShop.Web/wwwroot/css/tokens.css` shows 0 deleted lines; `grep -c "BEGIN tokens-v1" src/MobileShop.Web/wwwroot/css/tokens.css` = 1 and `grep -c "END tokens-v1" …` = 1; `sed -n '/BEGIN tokens-v1/,/END tokens-v1/p' src/MobileShop.Web/wwwroot/css/tokens.css | grep -cE '#[0-9A-Fa-f]{3,8}\b'` = 0; `grep -E '^[[:space:]]{2}--' src/MobileShop.Web/wwwroot/css/tokens.css | sed 's/:.*//' | sort | uniq -d` prints nothing (legacy plus new names; the four-space-indented `@supports` fallback is excluded on purpose); the same `sed` range piped to `grep -cE '^[[:space:]]+--'` = 77; `git diff --stat` = `tokens.css` only.
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
- Precondition (owner): the owner has captured baseline screenshots of Dashboard, Products list, Login, one Create page and the dock expanded (R19). If the run instructions do not confirm it, stop and ask the owner; this is the first step with a visible effect.
- Edge cases / error handling: the app already overrides the Bootstrap variables it cares about at `:root` in `app-theme.css`, which loads after Bootstrap with equal specificity, so those overrides keep winning (`--bs-body-bg`, `--bs-body-color`, `--bs-secondary-color`, `--bs-tertiary-color`, `--bs-border-color`, `--bs-link-color`, `--bs-link-hover-color`). Variables it does not override switch to Bootstrap's dark values. The full visible set was enumerated against Bootstrap 5.3.3 and the app CSS: F-12, F-13, F-44 and the `<code>` colour (Products/Details Apple ID password, Error page request id). `.table`, `.nav-tabs`, `.alert*`, `.input-group-text`, `.modal-content`, disabled fields and `.jalali-picker-feedback` are overridden by the app and do not change. Keep the UTF-8 BOM (check below). Do not add `data-bs-theme` anywhere else.
- Tests: none.
- Verify: CI green. `grep -c 'data-bs-theme="dark"' src/MobileShop.Web/Pages/Shared/_Layout.cshtml` = 1; `grep -c 'name="color-scheme"' …` = 1; `head -c3 src/MobileShop.Web/Pages/Shared/_Layout.cshtml | od -An -tx1` prints ` ef bb bf` (BOM kept); `git diff --numstat` for the file = `2 1` (two lines added, one replaced); `git diff --stat` = `_Layout.cshtml` only.
- Done when: both edits are in, BOM preserved, CI green.
- Risk: LOW (a global attribute, but the complete effect set is enumerated and each effect is an intended fix; the CI cannot see CSS, so the list in Step 7.4 is checked by the owner)
- Confidence: HIGH

## [ ] Step 7.3 — Remove dead rules from site.css
- Task: FOUND-003 · Priority P2 · Effort S · Optional hygiene (zero visual effect; it is the "dead-CSS cleanup" of the stage title and the Reviewer may drop it).
- Files: modify: `src/MobileShop.Web/wwwroot/css/site.css`; do not touch: `app-theme.css`.
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
- Symbols: none.
- Current -> Desired: n/a (validation only).
- Change: confirm CI green on the last code commit and record `Action: #<run>` per code step in act.md; record the greps of Steps 7.1-7.3; `git diff --stat` over the stage is exactly `tokens.css`, `_Layout.cshtml`, `site.css` (+ `.agents/` state files); `git diff --stat` shows NO change in `navigation-menu.css` / `navigation-menu.js`. No browser measurements are required (no harness).
- Manual check (owner, hard refresh first): (1) a page with a select (Products filters, People filters): the chevron is light and clearly visible. (2) Products -> Create phone -> any "Add New": the modal's close X is light and visible. (3) Record sell -> Customer search: hovering or focusing a result row gives a dark highlight, not a bright one. (4) Products -> Details of an Apple ID: the password `code` text is lighter pink than before (the Error page request id `code` changes the same way). (5) Dashboard, Products list, Login, a create form and the dock look otherwise identical to before. (6) Repeat 1-3 in Safari and Firefox. Anything else that changed is a defect to report.
- Depends on: Steps 7.1, 7.2, 7.3.
- Edge cases / error handling: if any check fails, stop and report; do not fix inside this step.
- Tests: none.
- Verify: the commands above, recorded in act.md.
- Done when: evidence recorded and the owner's manual check answered.
- Risk: LOW
- Confidence: HIGH

# Stage 8 — Core controls: buttons, fields, selects, checks, validation states, label association
Order rule: accessibility markup first (8.1, 8.2), then the two CSS families (8.3, 8.4). `controls.css` is created in 8.3 and appended to in 8.4. Dead variants (`btn-info`, `btn-warning`, `btn-outline-success/danger/dark/info`, `form-control-plaintext`) are dropped, not ported: no view uses them. CSS steps use the standard checks in "Hard constraints" (extraction command, brace balance, anchor deletion).

## ~~[x] Step 8.1 — Name the live unlabeled controls (12 labels, 1 checkbox)~~
- Task: COMP-004 (re-scoped to live code) · F-15, F-18 · Priority P1 · Effort S · Mandatory · Type A.
- Files: modify: `src/MobileShop.Web/Pages/Products/Create{AppleId,Cable,Case,Charger,Glass,Laptop,Phone,PortableStorage,PowerBank,SmartWatch,Tablet}.cshtml` (all 11 Create pages), `src/MobileShop.Web/Pages/Products/Edit.cshtml`, and `src/MobileShop.Web/Pages/Transactions/Index.cshtml`; do not touch the dead `_ProductCreate*Form` partials (R26).
- Symbols: in each of the 12 Products files exactly one pair: the label `Finished price (Toman)` and the read-only input with `data-finished-price` that follows it. Label forms: `<label class="form-label">` in AppleId, Cable, Glass, Laptop, Phone, SmartWatch, Tablet and Edit; a bare `<label>` in Case, Charger, PortableStorage and PowerBank. Input forms: `<input type="text" class="form-control" data-finished-price readonly aria-readonly="true" tabindex="-1" />` on the line after the label in Glass and Edit, and wrapped over two lines in AppleId (lines 45-47) and Phone (lines 72-74) — the Actor picks the insertion point for `id="finished-price"` and keeps the existing wrapping; `<input class="form-control" data-finished-price readonly />` on the same long line as the label in the other eight pages. In Transactions/Index.cshtml the row checkbox `<input class="form-check-input" type="checkbox" name="selectedIds" value="@transaction.Id" …>` (about lines 132-133).
- Current -> Desired: the label has no `for` and the input no `id`, so the read-only price has no accessible name; the row checkbox has a `name` (`selectedIds`) but no accessible name. Desired: the label gets `for="finished-price"` and the input `id="finished-price"`; the checkbox gets `aria-label="Select transaction: @transaction.ProductLabel"`.
- Change: (1) in each of the 12 Products files add `for="finished-price"` to the label (keep its class, if any) and `id="finished-price"` to the `data-finished-price` input, keeping `readonly`, `aria-readonly`, `tabindex`, `data-finished-price` and every other attribute. (2) In Transactions/Index.cshtml add the `aria-label` to the checkbox. Nothing else.
- Depends on: none (independent of Stage 7).
- Edge cases / error handling: each page contains exactly one `data-finished-price` input (verified for all 12), so the id is unique per page; `create-product-pricing.js` finds the field by the `data-finished-price` attribute (its comments say so), not by text or id; do not reflow the long single-line markup of the four accessory pages, edit it in place; if a file turns out to hold two such inputs, stop and report.
- Tests: none (markup only).
- Verify: CI green. `grep -rnE '<label( class="form-label")?>Finished price' src/MobileShop.Web/Pages --include=*.cshtml` prints nothing; `grep -rc 'for="finished-price"' src/MobileShop.Web/Pages/Products --include=*.cshtml` shows 1 in exactly 12 files and 0 elsewhere; `grep -rc 'id="finished-price"' src/MobileShop.Web/Pages/Products --include=*.cshtml` the same; `grep -c 'aria-label="Select transaction' src/MobileShop.Web/Pages/Transactions/Index.cshtml` = 1; `git diff --stat` lists only those 13 files.
- Done when: the greps hold and CI is green.
- Risk: LOW
- Confidence: HIGH

## ~~[x] Step 8.2 — Link validation state and messages for assistive tech (`site.js`)~~
- Task: COMP-005 · F-14, F-16 · Priority P1 · Effort M · Mandatory · Type A.
- Files: modify: `src/MobileShop.Web/wwwroot/js/site.js` only.
- Symbols: a new function `wireValidationAria` called once on `DOMContentLoaded` in the existing ready block (site.js already holds `parseMoneyInput`, `formatMoneyInput`, the validator normalizer, the password toggle and the Enter-to-submit handler).
- Current -> Desired: invalid fields get the class `input-validation-error` from jQuery Validate or ASP.NET but no `aria-invalid`; message spans (`<span data-valmsg-for="Input.X" class="text-danger field-validation-valid">`) have no id, are not linked to their field and are not announced. Desired: (a) every `[data-valmsg-for]` gets `aria-live="polite"` and, if it has no id, `id="val-<name lower-cased, every non-alphanumeric character replaced by "-">"`; the control whose `name` equals the `data-valmsg-for` value (first match, found with `[name="…"]` and `CSS.escape`) gets that id appended to `aria-describedby` (keep existing tokens); (b) `aria-invalid="true"` mirrors the class `input-validation-error` on `input, select, textarea`, once at load (server-rendered errors) and then live through a `MutationObserver` on each `form` (`subtree: true`, `attributes: true`, `attributeFilter: ['class']`) that only writes `aria-invalid`, never the class.
- Change: implement the function in site.js (about 35 lines, no new global, no new file, no jQuery dependency for this function). Behaviour of validation, messages and submit stays unchanged.
- Depends on: none.
- Edge cases / error handling: radio and checkbox groups use the first control with that name; forms without validation spans do nothing; the observer must not touch elements outside `form`; when no `MutationObserver` exists (very old browser) skip silently; `aria-invalid` is removed (not set to "false") when the class is gone.
- Tests: none (no JS test infrastructure).
- Verify: `node --check src/MobileShop.Web/wwwroot/js/site.js` passes; `grep -c "aria-invalid" …site.js` >= 2; `grep -c "aria-describedby" …site.js` >= 1; `grep -c "MutationObserver" …site.js` = 1; `git diff --stat` = `site.js` only; CI green. Owner check (after 8.4 adds the red border): submit Login empty, the username field shows `aria-invalid="true"` in the inspector and a screen reader reads the message.
- Done when: the checks hold and CI is green.
- Risk: LOW
- Confidence: MEDIUM

## ~~[x] Step 8.3 — Buttons: create `controls.css`~~
- Task: COMP-001 · F-08, F-41 · Priority P1 · Effort M · Mandatory · Type V.
- Files: create: `src/MobileShop.Web/wwwroot/css/controls.css`; modify: `src/MobileShop.Web/wwwroot/css/app-theme.css`, `src/MobileShop.Web/Pages/Shared/_Layout.cshtml`.
- Symbols: block `controls-buttons-v1` (R27); in app-theme.css the button variant group is app-theme.css 299-423 (`.btn {` through `.btn-outline-primary:hover, .btn-outline-primary:active { … }`), and the `.btn:focus*` rules live in the 523-635 focus/form group that Step 8.4 deletes; the layout `<link>` for `components.css`.
- Current -> Desired: nine variants spelled out one by one with a hover lift (`translateY(-1px)`), gray edges on primary and danger at rest, and duplicate dead variants. Desired: four looks (primary, secondary, outline, danger) through Bootstrap variables; radius `--radius-md`; press feedback `scale(0.98)`; outline hover gets a faint fill; coarse-pointer minimum height 44px; legacy class names stay as aliases.
- Change: (1) create the file from the block: `sed -n '/^\/\* BEGIN controls-buttons-v1 \*\//,/^\/\* END controls-buttons-v1 \*\//p' .agents/chat/plan.md | tr -d '\r' > src/MobileShop.Web/wwwroot/css/controls.css`. (2) Delete the button group from app-theme.css (locate by the two anchors, not by line number) plus one adjacent blank line. (3) In `_Layout.cshtml` add `<link rel="stylesheet" href="~/css/controls.css" asp-append-version="true" />` on a new line immediately before the `navigation-menu.css` link (same indentation and attributes as its neighbours).
- Depends on: Stage 7 (tokens-v1 provides every `var()` the block uses).
- Edge cases / error handling: before deleting, confirm that no view, script or other stylesheet USES a dropped variant: `grep -rnE "btn-(warning|info|outline-(success|danger|dark|info))" src/MobileShop.Web --include=*.cshtml --include=*.js | grep -v "/lib/"` must print nothing (app-theme.css defines the dropped variants itself and is deleted in this step, so it is deliberately not searched); `.btn-link`, `.btn-close` and `.btn-check` are untouched; the login submit and password toggle keep their own rules in app-theme.css (they still win where they set properties directly). Intended visible changes (owner check): rounder buttons, no gray edge around primary and danger, no lift on hover, slight shrink while pressed, faint fill on outline hover.
- Tests: none.
- Verify: CI green. Brace balance equal for `controls.css` and `app-theme.css`; `grep -c "^\.btn" src/MobileShop.Web/wwwroot/css/app-theme.css` = 4 and `grep -c "^\.btn-primary" src/MobileShop.Web/wwwroot/css/app-theme.css` = 0 (the four remaining `.btn` rules are the focus group `.btn:focus-visible,`, `.btn:focus,`, `.btn:active:focus,`, `.btn-link.nav-link:focus,`, which Step 8.4 removes); `grep -c "translateY(-1px)" src/MobileShop.Web/wwwroot/css/controls.css` = 0 and `grep -c "^\.btn:hover" src/MobileShop.Web/wwwroot/css/app-theme.css` = 0 (the date-picker rules still in app-theme.css keep their own lift until Step 10.5); `grep -c "controls.css" src/MobileShop.Web/Pages/Shared/_Layout.cshtml` = 1; `git diff --stat`: `controls.css` new, `app-theme.css` deletions only, `_Layout.cshtml` +1.
- Done when: the checks hold, CI green, owner confirms buttons on Dashboard, Products list and Login.
- Risk: MEDIUM (CSS the CI cannot see; every page has buttons)
- Confidence: MEDIUM

## ~~[x] Step 8.4 — Fields, checks and focus: append to `controls.css`~~
- Task: COMP-002, COMP-003 · F-11, F-14, F-08 · Priority P1 · Effort M · Mandatory · Type V.
- Files: modify: `src/MobileShop.Web/wwwroot/css/controls.css`, `src/MobileShop.Web/wwwroot/css/app-theme.css`.
- Symbols: block `controls-fields-v1` (R27); in app-theme.css the focus and form group (about lines 523-635 before 8.3, shifted afterwards): first rule `.btn:focus-visible,` (the focus-visible group), last rule `.input-group-text { … }`. It contains: the focus-visible and focus rules, `label`/`.form-label`, `.form-control, .form-select, .form-control-plaintext`, `[data-finished-price]`, placeholder, disabled/readonly, `:focus`, `.form-select option`, `.form-check-input` and `:checked`, `fieldset`, `legend`, `.input-group-text`.
- Current -> Desired: fields are 2.65rem tall with `--r-control` radius, labels bold (600 resolves to 700), blue-tinted `rgba()` fills for disabled and readonly, no red state for invalid fields. Desired: height `--size-control` (44px at the 16px root, 44px minimum on coarse pointers), radius `--radius-md`, labels regular and muted, brand-derived tints, red border on `.input-validation-error` and `.is-invalid`, finished-price field darker but token-derived.
- Change: (1) append the block: `{ printf '\n'; sed -n '/^\/\* BEGIN controls-fields-v1 \*\//,/^\/\* END controls-fields-v1 \*\//p' .agents/chat/plan.md | tr -d '\r'; } >> src/MobileShop.Web/wwwroot/css/controls.css`. (2) Delete the anchored group from app-theme.css plus one adjacent blank line. Do not touch the nav-tabs and profit-chart rules that sit between the button group and this group.
- Depends on: Step 8.3.
- Edge cases / error handling: `.form-control-plaintext` is dropped (`grep -rn "form-control-plaintext" src/MobileShop.Web --include=*.cshtml --include=*.js | grep -v "/lib/"` must print nothing); the `--app-ink` and `--app-edge` aliases stay defined (app-theme.css `:root`) because other rules still use them; the chevron fix comes from Step 7.2, not from this block; `label { … }` is a global element rule on purpose (it also restyles `.form-check-label`, now regular and muted).
- Tests: none.
- Verify: CI green. Brace balance equal for both files; `grep -c "^\.btn" src/MobileShop.Web/wwwroot/css/app-theme.css` = 0 (the focus group is gone after 8.4); `grep -c "^\.form-control" src/MobileShop.Web/wwwroot/css/app-theme.css` = 0; `grep -c "^fieldset" …app-theme.css` = 0; `grep -c "input-validation-error" …controls.css` >= 2; `grep -cE "rgba\(" …controls.css` = 0; `git diff --stat`: `controls.css` additions, `app-theme.css` deletions only.
- Done when: the checks hold, CI green, owner confirms Login, Sell, a Create page and People filters.
- Risk: MEDIUM
- Confidence: MEDIUM

## ~~[x] Step 8.5 — Stage 8 validation~~
- Task: QA (stage gate) · Mandatory.
- Files: none modified.
- Symbols: none.
- Current -> Desired: n/a (validation only).
- Change: confirm CI green on the last code commit and record `Action: #<run>` per code step; record the greps of 8.1-8.4; `git diff --stat` over the stage = the thirteen files of 8.1, `site.js`, `controls.css`, `app-theme.css`, `_Layout.cshtml`.
- Manual check (owner, hard refresh): buttons on 5 pages (hover, press, focus with Tab, disabled); a field in each state (focus, disabled, read-only finished price, invalid after submitting empty Login); selects and checkboxes; People filters; on a phone (coarse pointer) every button and field is at least 44px tall; in a narrow desktop window they are about 38px (`site.css:23-31` sets `html { font-size: 14px }` below 768px, so `--size-control` 2.75rem is 38.5px there; only the `@media (pointer: coarse)` rule reaches 44px); repeat the key ones in Safari and Firefox. Anything else that changed is a defect.
- Depends on: Steps 8.1-8.4.
- Edge cases / error handling: if any check fails, stop and report; do not fix inside this step.
- Tests: none.
- Verify: the commands above, recorded in act.md.
- Done when: evidence recorded and the owner's manual check answered.
- Risk: LOW
- Confidence: HIGH

# Stage 9 — Content surfaces: canvas shell, cards, tables, chips, alerts, empty and loading states
Order rule: base first (9.1), then `surfaces.css` grows by family (9.2 cards, 9.3 tables, 9.4 chips and alerts, 9.5 tabs and segmented control); 9.6 is gated on the owner. Every step removes the replaced rules in the same commit. Products type "tabs" are plain link buttons (`btn btn-primary` or `btn btn-outline-primary` with `aria-current`) and stay that way; `.nav-tabs` exists only on ProfitLoss.

## ~~[x] Step 9.1 — `base.css`: merge `site.css` and the base rules, set the type scale~~
- Task: base consolidation · F-09 · Priority P1 · Effort M · Mandatory · Type V.
- Files: create: `src/MobileShop.Web/wwwroot/css/base.css`; delete: `src/MobileShop.Web/wwwroot/css/site.css`; modify: `src/MobileShop.Web/wwwroot/css/app-theme.css`, `src/MobileShop.Web/wwwroot/css/tokens.css`, `src/MobileShop.Web/Pages/Shared/_Layout.cshtml`.
- Symbols: block `base-v1` (R27); in app-theme.css four groups: (a) from the first line `:root {` through the rule `.border, .border-top, .border-bottom { … }` (about lines 1-97); (b) the rule `.footer { … }`; (c) the two `@media (max-width: 767.98px)` and `@media (max-width: 575.98px)` blocks that style `.container > main[role="main"]` (the first also holds a `.table` rule that dies with the table migration in 9.3); (d) the `@media (prefers-reduced-motion: reduce)` block that contains `scroll-behavior: auto`. In tokens.css the legacy line `--fs-h1: 1.75rem;`. In the layout the `site.css` link.
- Current -> Desired: base rules are split between `site.css` and the top of `app-theme.css`; the main panel is 0.75rem round; H1 is 1.75rem; `.lead`, `.text-body`, `.text-dark`, `.border-bottom` and the `.table` mobile padding rule are dead. Desired: one `base.css` linked at the position `site.css` had (after `tokens.css`, before `app-theme.css`); main panel `--radius-xl` (1.5rem), `--radius-lg` below 768px, square below 576px; `--fs-h1` becomes `clamp(1.75rem, 1.5rem + 0.8vw, 2rem)` with tight tracking and `--lh-tight`.
- Change: (1) create the file: `sed -n '/^\/\* BEGIN base-v1 \*\//,/^\/\* END base-v1 \*\//p' .agents/chat/plan.md | tr -d '\r' > src/MobileShop.Web/wwwroot/css/base.css`. (2) Delete the four anchored groups (a)-(d) from app-theme.css, each with one adjacent blank line. (3) In tokens.css change `--fs-h1: 1.75rem;` to `--fs-h1: clamp(1.75rem, 1.5rem + 0.8vw, 2rem);`. (4) `git rm src/MobileShop.Web/wwwroot/css/site.css` and in `_Layout.cshtml` replace the `site.css` link by `<link rel="stylesheet" href="~/css/base.css" asp-append-version="true" />` in the same position.
- Depends on: Stage 7 (tokens; `site.css` must already be the Step 7.3 version).
- Edge cases / error handling: confirm the dropped classes are unused: `grep -rnE 'class="[^"]*\b(lead|text-body|text-dark|border-bottom)\b' src/MobileShop.Web --include=*.cshtml` must print nothing; the root alias block `--app-*` stays (it moves to `base.css` together with the group (a) `:root` overrides, which are the only rules that read it; the other reader, `.nav-tabs .nav-link` in app-theme.css, is deleted in Step 9.5; Step 16.1 removes the unused aliases); `body { margin-bottom: 60px }` from `site.css` is intentionally not carried over (it was always overridden to 0); the scoped template stylesheet is untouched here and still overrides the footer until Step 11.1 (known, R4 F-45).
- Tests: none.
- Verify: CI green. Brace balance equal for `base.css`, `app-theme.css`, `tokens.css`; `grep -c "site.css" src/MobileShop.Web/Pages/Shared/_Layout.cshtml` = 0 and `grep -c "base.css" …` = 1; `grep -rn --include=*.cshtml --include=*.cs --include=*.json --exclude-dir=bin --exclude-dir=obj "site.css" src | grep -v "/lib/"` prints nothing (the `bin`/`obj` static-web-asset manifests are build output, regenerated by CI, and are excluded on purpose); `grep -c "^:root" …app-theme.css` = 0; `grep -c "fs-h1" …tokens.css` = 1 with the clamp value; `git diff --stat`: `base.css` new, `site.css` deleted, `app-theme.css` deletions, `tokens.css` 1 line, `_Layout.cshtml` 1 line.
- Done when: the checks hold, CI green, owner confirms page frame, headings and footer look as before apart from rounder panel and slightly larger H1.
- Risk: MEDIUM (load-order sensitive; every page)
- Confidence: MEDIUM

## ~~[x] Step 9.2 — Surface family: cards, stat tiles, form sections, chart cards, page header, empty and loading states~~
- Task: COMP-007 · F-01, F-02, F-09 · Priority P1 · Effort M · Mandatory · Type V.
- Files: create: `src/MobileShop.Web/wwwroot/css/surfaces.css`; modify: `src/MobileShop.Web/wwwroot/css/app-theme.css`, `src/MobileShop.Web/wwwroot/css/components.css`, `src/MobileShop.Web/Pages/Shared/_Layout.cshtml`.
- Symbols: block `surfaces-cards-v1` (R27); app-theme.css rules `.card { … }`, `.card-title { … }` and the contiguous profit-chart group from `.profit-chart-card {` through `.profit-chart-empty { … }` (find both groups by their selector text, not by line number: earlier steps shift them); components.css rules `.page-head`, `.page-head__title`, `.page-head__sub`, `.page-head__actions`, `.toolbar`, `.toolbar .form-label`, `.stat-grid`, `.stat`, `.stat__value`, `.stat__label`, `.form-section`, `.form-section__title`, `.empty-state`, `.empty-state__title`, `.empty-state__hint`, `.loading-line`, `.num`, `td.num, th.num`, and the `@media (max-width: 575.98px)` block that holds `.toolbar > *` and `.page-head__actions`.
- Current -> Desired: three card idioms with different fills (`.card` panel, `.stat` and `.form-section` raised) and flat `box-shadow: none`. Desired: one family (material M2) with fill `--surface-2`, hairline edge, radius `--radius-lg`, `--shadow-1` plus the top edge highlight; page head, toolbar, stat grid, empty and loading states ported with spacing tokens; `.profit-chart-empty` (unused) dropped.
- Change: (1) create the file: `sed -n '/^\/\* BEGIN surfaces-cards-v1 \*\//,/^\/\* END surfaces-cards-v1 \*\//p' .agents/chat/plan.md | tr -d '\r' > src/MobileShop.Web/wwwroot/css/surfaces.css`. (2) Delete the listed rules from app-theme.css and components.css, each with one adjacent blank line. (3) Add `<link rel="stylesheet" href="~/css/surfaces.css" asp-append-version="true" />` immediately before the `navigation-menu.css` link.
- Depends on: Step 9.1.
- Edge cases / error handling: `.num` rules exist twice in components.css (generic `.num`, `td.num, th.num` go here; the `.data-table td.num` pair moves in 9.3, leave it); the delete list does NOT remove every rule that matches the surface pattern — `.form-section .action-bar` (components.css) stays, because Step 11.2 owns the action-bar family, and the four `.status-chip*` rules stay until Step 9.4 deletes them; the dashboard `card h-100` and the ProfitLoss chart cards must still render; no view markup changes in this step.
- Tests: none.
- Verify: CI green. Brace balance equal for the three CSS files; `grep -cE "^\.(page-head|toolbar|stat|form-section|empty-state|loading-line)" src/MobileShop.Web/wwwroot/css/components.css` = 5 — the survivors are the `.form-section .action-bar` rule (Step 11.2 owns it) and the four `.status-chip*` rules (Step 9.4 owns them; the pattern's `stat` alternative also matches `status-chip`); `grep -nE "^\.(page-head|toolbar|form-section|empty-state|loading-line)" src/MobileShop.Web/wwwroot/css/components.css | grep -v "action-bar"` prints nothing and `grep -nE "^\.stat" src/MobileShop.Web/wwwroot/css/components.css | grep -v "status-chip"` prints nothing (every rule this step deletes is gone); `grep -cE "^\.(card|profit-chart)" src/MobileShop.Web/wwwroot/css/app-theme.css` = 0; `grep -c "backdrop-filter" src/MobileShop.Web/wwwroot/css/surfaces.css` = 0; `grep -c "surfaces.css" …_Layout.cshtml` = 1.
- Done when: the checks hold, CI green, owner confirms Dashboard, a Create page and ProfitLoss.
- Risk: MEDIUM
- Confidence: MEDIUM

## ~~[x] Step 9.3 — One table idiom~~
- Task: COMP-008 · F-31, F-07 · Priority P1 · Effort M · Mandatory · Type V.
- Files: modify: `src/MobileShop.Web/wwwroot/css/surfaces.css`, `src/MobileShop.Web/wwwroot/css/app-theme.css`, `src/MobileShop.Web/wwwroot/css/components.css`, `src/MobileShop.Web/Pages/Products/Details.cshtml`.
- Symbols: block `surfaces-tables-v1` (R27); app-theme.css group from `.table {` through `.table-hover > tbody > tr:hover > * { … }` (`.table`, `.table > :not(caption) > * > *`, `.table thead th` with uppercase, `.table thead a`, `.table thead a:hover`, `.table tbody tr`, `.table-striped`, `.table-hover`; find the group by its selector text, not by line number); components.css `.data-table` rules (`.data-table`, `th`, `th a` if present, `td`, `tbody tr`, `tbody tr:hover`, `.data-table td.num, .data-table th.num`) and the `@media (prefers-reduced-motion: reduce)` block that holds `.data-table tbody tr`; Details.cshtml line `<table class="table">` (about line 141).
- Current -> Desired: eight tables use `.data-table` (medium header, 0.65rem 0.75rem cells) and one uses Bootstrap `.table` (uppercase headers, a second set of rules). Desired: every table is `.data-table`: bold muted header in title case, `0.75rem 1rem` cells (row about 3rem), hairline row separators, sort links muted with accent-free hover; the Bootstrap `.table` rules, `.table-hover` and `.table-striped` (unused) are deleted.
- Change: (1) append the block: `{ printf '\n'; sed -n '/^\/\* BEGIN surfaces-tables-v1 \*\//,/^\/\* END surfaces-tables-v1 \*\//p' .agents/chat/plan.md | tr -d '\r'; } >> src/MobileShop.Web/wwwroot/css/surfaces.css`. (2) Delete the anchored `.table` group from app-theme.css and the `.data-table` rules plus the reduced-motion block from components.css. (3) In Details.cshtml change `<table class="table">` to `<table class="data-table">`.
- Depends on: Step 9.2.
- Edge cases / error handling: before deleting confirm only Details used `.table`: `grep -rn 'class="table' src/MobileShop.Web/Pages --include=*.cshtml | grep -v table-responsive` shows only that one line; keep Bootstrap `.table-responsive` wrappers (they are Bootstrap's); the Details table keeps its `class="num"` cells; sorting markup on other pages is untouched here.
- Tests: none.
- Verify: CI green. Brace balance equal for the four CSS files; `grep -rn 'class="table"' src/MobileShop.Web/Pages --include=*.cshtml` prints nothing; `grep -c "data-table" src/MobileShop.Web/wwwroot/css/components.css` = 0; `grep -cE "^\.table" src/MobileShop.Web/wwwroot/css/app-theme.css` = 0; `grep -c "text-transform" src/MobileShop.Web/wwwroot/css/surfaces.css` = 0; `git diff --stat` lists those four files only.
- Done when: the checks hold, CI green, owner confirms Products list, Transactions, Customers and Product Details tables.
- Risk: MEDIUM
- Confidence: MEDIUM

## ~~[x] Step 9.4 — Chips and alerts~~
- Task: COMP-009 (chips), COMP-010 · F-03 · Priority P1 · Effort S · Mandatory · Type V.
- Files: modify: `src/MobileShop.Web/wwwroot/css/surfaces.css`, `src/MobileShop.Web/wwwroot/css/app-theme.css`, `src/MobileShop.Web/wwwroot/css/components.css`.
- Symbols: block `surfaces-feedback-v1` (R27); app-theme.css group from `.badge {` through the `.field-validation-valid`/`.validation-summary-valid` rule (`.badge`, `.alert`, the four `.alert-*` variants, `.text-danger`, `.field-validation-error`, `.validation-summary-errors`, the valid-state hide rule; find the group by its selector text, not by line number); components.css `.status-chip`, `.status-chip--available`, `.status-chip--sold`, `.status-chip--used`.
- Current -> Desired: chips are 0.5rem-round and regular weight; alerts carry a 16px backdrop blur over an opaque fill (no visible effect, F-03); `.badge` and `.alert-warning` are unused. Desired: pill chips with tinted fill plus text (bold), alerts radius `--radius-lg` without blur, danger and accent borders kept, validation utilities moved unchanged.
- Change: (1) append the block (same `{ printf '\n'; sed … ; } >>` pattern) to `surfaces.css`. (2) Delete the anchored group from app-theme.css and the four chip rules from components.css.
- Depends on: Step 9.3.
- Edge cases / error handling: `status-chip--*` class names are built at runtime in Razor (`status-chip--@…`), so a grep for the full names in views looks empty; do not rename them; the single `!important` rule in the moved validation utilities (app-theme.css:488) is moved unchanged (existing, not new); the `!important` count goes from 13 today (app-theme 11 + dock 2) to 12 after this stage (base.css 9 + surfaces.css 1 + dock 2); `.alert-warning` is dropped (`grep -rn "alert-warning" src/MobileShop.Web --include=*.cshtml --include=*.js | grep -v "/lib/"` must print nothing).
- Tests: none.
- Verify: CI green. Brace balance equal; `grep -c "backdrop-filter" src/MobileShop.Web/wwwroot/css/surfaces.css` = 0; `grep -cE "^\.(badge|alert|text-danger|field-validation|validation-summary)" src/MobileShop.Web/wwwroot/css/app-theme.css` = 0; `grep -c "^\.status-chip" src/MobileShop.Web/wwwroot/css/components.css` = 0.
- Done when: the checks hold, CI green, owner confirms status chips (available, sold, used) and a success and an error alert.
- Risk: LOW
- Confidence: HIGH

## ~~[x] Step 9.5 — Tabs and segmented control (ProfitLoss)~~
- Task: COMP-009 (segmented) · Priority P1 · Effort S · Mandatory · Type V.
- Files: modify: `src/MobileShop.Web/wwwroot/css/surfaces.css`, `src/MobileShop.Web/wwwroot/css/app-theme.css`, `src/MobileShop.Web/Pages/Reports/ProfitLoss.cshtml`.
- Symbols: block `surfaces-controls-v1` (R27); app-theme.css `.nav-tabs` rules (`.nav-tabs`, `.nav-tabs .nav-link`, hover/focus, `.active`; find them by selector text, not by line number); ProfitLoss.cshtml lines 94-102: `<div class="btn-group" role="group" aria-label="Date range mode">` with two radios `class="btn-check"` and two labels `class="btn btn-outline-primary"`.
- Current -> Desired: the range mode is a Bootstrap `btn-group` of outline buttons; the Profit/Distribution tabs are Bootstrap `nav-tabs` styled in app-theme.css. Desired: the mode control is a `.segmented` pill track with one selected pill (accent soft fill, bold), tabs ported with tokens.
- Change: (1) append the block to `surfaces.css`; delete the `.nav-tabs` rules from app-theme.css. (2) In ProfitLoss.cshtml change `class="btn-group"` to `class="segmented"` and both label classes from `btn btn-outline-primary` to `btn`; keep `role`, `aria-label`, ids, `name="Mode"`, `checked` logic and `autocomplete`.
- Depends on: Steps 8.3 and 9.4.
- Edge cases / error handling: confirm no script keys off the old classes: `grep -n "btn-outline-primary\|btn-group" src/MobileShop.Web/Pages/Reports/ProfitLoss.cshtml src/MobileShop.Web/wwwroot/js/profit-loss-chart.js` shows nothing after the edit; the Bootstrap tab JS (`data-bs-toggle="tab"`) is unchanged.
- Tests: none.
- Verify: CI green. Brace balance equal; `grep -c "nav-tabs" src/MobileShop.Web/wwwroot/css/app-theme.css` = 0; `grep -c "segmented" src/MobileShop.Web/Pages/Reports/ProfitLoss.cshtml` = 1; `grep -c "btn-group" …ProfitLoss.cshtml` = 0.
- Done when: the checks hold, CI green, owner confirms changing the range mode still reloads/updates as before and tabs still switch.
- Risk: LOW
- Confidence: HIGH

## ~~[x] Step 9.6 — Canvas shell and ambient layer (gated on owner decision Q-04)~~ — SKIPPED (Q-04 not approved)
- Task: SHELL-001 · F-02, F-05 · Priority P2 · Effort M · Optional (owner approval) · Type V.
- Gate: this step is GATED on owner decision Q-04 — run it only if the owner answered yes in the run instructions (the durable channel for the answer; `.agents/chat/audit.md` is overwritten by every Reviewer pass and is not a channel for it). Otherwise SKIP the step — do not implement it — tick it as skipped with the reason "Q-04 not approved" and continue. The Actor never invents the owner's answer.
- Files: modify: `src/MobileShop.Web/wwwroot/css/base.css`.
- Symbols: the rule `.container > main[role="main"]` and the block `shell-canvas-v1` (R27).
- Current -> Desired: the page content sits in a bordered, filled panel (card-on-card with the surface family). Desired from 768px up: the panel loses fill and border so content sits on the canvas, and one fixed ambient gradient layer (two radial gradients, no animation, no blur) gives the glass a surface to sample.
- Change: (1) append the block to `base.css`: `{ printf '\n'; sed -n '/^\/\* BEGIN shell-canvas-v1 \*\//,/^\/\* END shell-canvas-v1 \*\//p' .agents/chat/plan.md | tr -d '\r'; } >> src/MobileShop.Web/wwwroot/css/base.css`. No other edit.
- Depends on: Steps 9.1, 9.2.
- Edge cases / error handling: the layer uses `position: fixed; z-index: -1` and relies on the body background propagating to the canvas (`html` must not get a background); it is hidden under `prefers-contrast: more`, `prefers-reduced-transparency: reduce` and `html[data-surfaces="solid"]`; below 768px the panel stays as is.
- Tests: none.
- Verify: CI green. Brace balance equal for `base.css`; `grep -c "body::before" src/MobileShop.Web/wwwroot/css/base.css` = 3 (the ambient layer rule plus the two override selectors: the `prefers-contrast: more` / `prefers-reduced-transparency: reduce` override and the `html[data-surfaces="solid"]` override); `grep -c "^body::before" src/MobileShop.Web/wwwroot/css/base.css` = 1 (the layer rule alone) and `grep -c 'data-surfaces="solid"' src/MobileShop.Web/wwwroot/css/base.css` = 1 (the solid-surface override); `git diff --stat` = `base.css` only.
- Done when: the checks hold and the owner confirms the look on desktop and that tables and forms stay readable.
- Risk: MEDIUM
- Confidence: MEDIUM

## ~~[x] Step 9.7 — Stage 9 validation~~
- Task: QA (stage gate) · Mandatory.
- Files: none modified.
- Symbols: none.
- Current -> Desired: n/a (validation only).
- Change: confirm CI green on the last code commit and record `Action: #<run>` per code step; record the greps of 9.1-9.5 plus "9.6 skipped (Q-04 not approved)" when the owner did not approve Q-04; `git diff --stat` over the stage matches the file lists above.
- Manual check (owner, hard refresh): Dashboard (tiles, recent transactions), Products list (chips, table, filters), Customers and Sellers tables, Product Details table, a Create page (form sections), ProfitLoss (cards, tabs, segmented mode), a success and an error alert; headings and the page frame; repeat the key ones in Safari and Firefox. Anything else that changed is a defect.
- Depends on: Steps 9.1-9.6.
- Edge cases / error handling: if any check fails, stop and report; do not fix inside this step.
- Tests: none.
- Verify: the commands above, recorded in act.md.
- Done when: evidence recorded and the owner's manual check answered.
- Risk: LOW
- Confidence: HIGH

# Stage 10 — Overlays and pickers: glass modals and menus, scrim, picker keyboard support, Jalali date picker
Order rule: the visual family first (10.1), then markup and behaviour (10.2-10.4), then the date picker (10.5 visual, 10.6 behaviour). The dead create-product modal subsystem (R26) is not touched. Picker result lists stay opaque because they sit in the page flow; only modal sheets and the date-picker panel are glass.

## ~~[x] Step 10.1 — Modal sheets and scrim: create `overlays.css`~~
- Task: COMP-012 (visual), COMP-015 · F-04 · Priority P1 · Effort M · Mandatory · Type V.
- Files: create: `src/MobileShop.Web/wwwroot/css/overlays.css`; modify: `src/MobileShop.Web/wwwroot/css/app-theme.css`, `src/MobileShop.Web/Pages/Shared/_PersonPicker.cshtml`, `src/MobileShop.Web/Pages/Shared/_Layout.cshtml`.
- Symbols: block `overlays-modal-v1` (R27); app-theme.css overlay group from `.modal-content,` (shared with `.dropdown-menu, .popover`) through `.progress { … }` (currently `app-theme.css:189-248`; the anchor is the selector text, not the line numbers): `.modal-content, .dropdown-menu, .popover`, `.list-group-item`, the `.product-picker-results` rules, `.modal-backdrop` and `.show`, `.modal-header, .modal-footer`, `.dropdown-item*`, `.progress`; in `_PersonPicker.cshtml` the only `<div class="modal-content">` (about line 35).
- Current -> Desired: modals, dropdowns and popovers share one opaque fill and one shadow; the scrim is a plain 60% black; list-group hover is special-cased for the product picker only. Desired: `.modal-content` is glass overlay material (92% tint, blur 24, `--shadow-4`, edge highlight, radius `--radius-xl`), solid for scrollable dialogs and for `.modal-content--solid`; the scrim is one dimmed and blurred layer; one `.list-group-item-action` hover for both pickers; dropdown, popover and progress rules (no such components exist) are deleted.
- Change: (1) create the file: `sed -n '/^\/\* BEGIN overlays-modal-v1 \*\//,/^\/\* END overlays-modal-v1 \*\//p' .agents/chat/plan.md | tr -d '\r' > src/MobileShop.Web/wwwroot/css/overlays.css`. (2) Delete the anchored overlay group from app-theme.css plus one adjacent blank line. (3) In `_PersonPicker.cshtml` change `<div class="modal-content">` to `<div class="modal-content modal-content--solid">` (it holds a multi-field form). (4) Add `<link rel="stylesheet" href="~/css/overlays.css" asp-append-version="true" />` immediately before the `navigation-menu.css` link.
- Depends on: Steps 8.3, 9.2.
- Edge cases / error handling: confirm no dropdown, popover or progress component exists (`grep -rn "dropdown-menu\|popover\|class=\"progress\|dropdown-item" src/MobileShop.Web --include=*.cshtml --include=*.js | grep -v "/lib/"` prints nothing); the backdrop filter on `.modal-content` makes a containing block for fixed children, and no modal contains fixed elements; `.modal-backdrop.show` sets `opacity: 1` on purpose (the dim comes from the scrim token); z-index values now come from `--z-modal` and `--z-scrim`.
- Tests: none.
- Verify: CI green. Brace balance equal for `overlays.css` and `app-theme.css`; `grep -cE "^\.(modal|dropdown|popover|progress|product-picker|list-group)" src/MobileShop.Web/wwwroot/css/app-theme.css` = 0; `grep -c "modal-content--solid" src/MobileShop.Web/Pages/Shared/_PersonPicker.cshtml` = 1; `grep -c "overlays.css" …_Layout.cshtml` = 1.
- Done when: the checks hold, CI green, owner opens a Create-page modal (glass, readable) and the person-picker modal (solid) and sees the close X from Step 7.2.
- Risk: MEDIUM
- Confidence: MEDIUM

## ~~[x] Step 10.2 — Label every modal (`aria-labelledby` on 24 modals)~~
- Task: COMP-012 (markup) · F-17 · Priority P1 · Effort M · Mandatory · Type A.
- Files: modify: the Create pages that own the unlabeled modals: `src/MobileShop.Web/Pages/Products/CreateCable.cshtml` (2), `CreateCase.cshtml` (3), `CreateCharger.cshtml` (2), `CreateGlass.cshtml` (3), `CreateLaptop.cshtml` (4), `CreatePhone.cshtml` (1, the corporation modal), `CreatePortableStorage.cshtml` (3), `CreatePowerBank.cshtml` (2), `CreateSmartWatch.cshtml` (2), `CreateTablet.cshtml` (2).
- Symbols: every `<div class="modal fade" id="X" …>` without `aria-labelledby`, and the `<h5 class="modal-title">` inside it (list them with `grep -rn 'class="modal fade"' src/MobileShop.Web/Pages --include=*.cshtml | grep -v aria-labelledby`: 24 lines).
- Current -> Desired: 24 of the 29 modals have no accessible name (the person picker and four CreatePhone modals are labelled). Desired: each gets `aria-labelledby="X-title"` on the modal tag and `id="X-title"` on its title element, where `X` is that modal's own `id`.
- Change: for each of the 24 modals add the two attributes; change nothing else (keep `tabindex`, `aria-hidden`, classes, buttons, ids).
- Depends on: none (independent of 10.1).
- Edge cases / error handling: ids are unique per page because modal ids are; if a modal has no `<h5 class="modal-title">`, stop and report that file; markup is dense (several modals per line), so edit each tag precisely; do not reformat lines; the four already-labelled CreatePhone modals keep their existing `…ModalLabel` ids (`addManufacturerModalLabel`, `addModelModalLabel`, `addColorModalLabel`, `addPartNumberModalLabel`) and the person picker keeps its `aria-labelledby="@modalLabelId"` — leave them alone, only the 24 unlabeled modals get the new `…-title` scheme.
- Tests: none.
- Verify: CI green. `grep -rn 'class="modal fade"' src/MobileShop.Web/Pages --include=*.cshtml | grep -vc aria-labelledby` = 0; **per-page** id uniqueness — ids must be unique inside a page, not across the repo, because the same modal id may exist on other pages and does: `addManufacturerModal-title` and `addModelModal-title` each appear 7 times repo-wide, on different pages, so a repo-wide `uniq -d` check is meaningless here — run `for f in $(grep -rl 'aria-labelledby="[^"]*-title"' src/MobileShop.Web/Pages --include=*.cshtml); do grep -oE 'aria-labelledby="[^"]+-title"' "$f" | sort | uniq -d | sed "s|^|DUPLICATE $f: |"; done` → prints nothing (no `DUPLICATE` line); `grep -rhoE 'aria-labelledby="[^"]+-title"' src/MobileShop.Web/Pages/Products --include=*.cshtml | wc -l` = 24 (the 24 modals of this step; repo-wide the total is 25 because `profit-chart-title` on the ProfitLoss page already ends in `-title`); `git diff --stat` lists only the ten files above.
- Done when: the greps hold and CI is green.
- Risk: LOW
- Confidence: HIGH

## ~~[x] Step 10.3 — Combobox keyboard model: helper in `site.js`, wired into the person picker~~
- Task: COMP-013 (part 1) · F-19 · Priority P0 · Effort M · Mandatory · Type A.
- Files: modify: `src/MobileShop.Web/wwwroot/js/site.js`, `src/MobileShop.Web/wwwroot/js/person-picker.js`.
- Symbols: new `window.wireComboboxKeys(input, list, hooks)` in site.js, **defined as a top-level global assignment** (the pattern of the existing `window.parseMoneyInput` at `site.js:5`, executed at parse time — **not** inside the `DOMContentLoaded` ready block at `site.js:101`); load order in `_Layout.cshtml`: the page `Scripts` section is rendered at line 40, `site.js` at line 41, `person-picker.js` at line 43 — so site.js is parsed **after** `product-picker.js` but **before** `person-picker.js`, and only a top-level definition makes the helper exist before any `DOMContentLoaded` handler runs; in person-picker.js the existing `renderResults`, the focus, input and blur handlers and the result-visibility code.
- Current -> Desired: the search box declares `role="combobox"` but registers no key handling, and the blur handler hides the result list, so pressing Tab removes the results and a keyboard user cannot select anyone. Desired: ArrowDown and ArrowUp move an active option, Enter selects it, Escape closes the list; the active option is announced through `aria-activedescendant`.
- Change: (1) site.js: add `window.wireComboboxKeys(input, list, hooks)` returning `{ reset }` **at the top level of the file (outside the `DOMContentLoaded` ready block)**, so `window.wireComboboxKeys` already exists when `product-picker.js`'s earlier-registered `DOMContentLoaded` handler runs. `hooks` is `{ open, close }`, the picker's own show and hide functions (wrap the picker's inline show or hide code in small local functions first if needed), with this exact contract: `open()` removes `d-none` from the results list and sets `aria-expanded="true"` on the search input and changes nothing else (not `isFocused`, not the search text, not the selected product, not the price); `close()` adds `d-none` and sets `aria-expanded="false"`. Behaviour on `input` keydown: ArrowDown with the list closed and options present calls `hooks.open()`; ArrowDown or ArrowUp (list open) moves the active index (clamped to 0..n-1, no wrap), sets `aria-activedescendant` on the input to the active option's id, `aria-selected="true"` on it (false on others) and the class `is-active`, calls `scrollIntoView({ block: 'nearest' })` and `preventDefault()`; Enter with an active option calls `preventDefault()` and `stopPropagation()` (so the global Enter-to-submit handler does not run) and `option.click()`; Enter without an active option does nothing special; Escape with the list open calls `hooks.close()`, `preventDefault()` and `stopPropagation()`. Options are `list.querySelectorAll('[role="option"]')`, read on every key press; give each an id `<list id or "combo">-opt-<index>` if missing and `tabindex="-1"`. `reset()` clears the active index, `aria-activedescendant` and `is-active`. (2) person-picker.js: call `wireComboboxKeys(searchInput, results, { open, close })` once after the elements exist, call the returned `reset()` at the end of `renderResults`, and add a polite live region (`role="status"`, class `visually-hidden`, created in JS and inserted after the results element) whose text becomes "N results" or "No results" in `renderResults`.
- Depends on: Step 10.1 (the `.is-active` hover style lives in overlays.css).
- Edge cases / error handling: keep every existing behaviour (search on focus and input, `valueInput` reset to `0` on typing, selection by click); the helper must not throw when `list` is empty or hidden; the definition must stay at the top level of site.js — inside the ready block it would still be `undefined` when `product-picker.js`'s handler runs, and the guard in 10.4 would then silently skip the wiring; `hooks.open` must not touch `isFocused` (person-picker.js:18) nor the selected product or the price state (product-picker.js); do not change the markup file `_PersonPicker.cshtml`; the 150 ms blur delay of the product picker is a separate step (10.4).
- Tests: none (no JS test infrastructure).
- Verify: `node --check` passes for both files; `grep -c "wireComboboxKeys" src/MobileShop.Web/wwwroot/js/site.js src/MobileShop.Web/wwwroot/js/person-picker.js` >= 1 for each of the two files; `grep -c "aria-activedescendant" …site.js` >= 1; the helper is defined at the top level of site.js: run `awk '!d && /window\.wireComboboxKeys[[:space:]]*=/{d=NR} !r && /DOMContentLoaded/{r=NR} END{ if (d==0) print "FAIL: no definition found"; else if (r==0) print "OK: definition at line " d " (site.js has no ready block)"; else if (d<r) print "OK: definition at line " d " precedes the ready block at line " r; else print "FAIL: definition at line " d " sits inside the ready block (line " r ")" }' src/MobileShop.Web/wwwroot/js/site.js` → prints `OK: …`; CI green. Owner check: on a page with the person picker (Transactions > Buy or Sell), in the browser console `typeof window.wireComboboxKeys` → `function`; once the search list has options, `(() => { const i = document.querySelector('[data-person-search]'); i.focus(); i.dispatchEvent(new KeyboardEvent('keydown', { key: 'ArrowDown', bubbles: true })); i.dispatchEvent(new KeyboardEvent('keydown', { key: 'ArrowDown', bubbles: true })); return i.getAttribute('aria-activedescendant'); })()` → a non-empty option id (proves the keydown path is attached, not only the function defined); type two letters, press ArrowDown twice and Enter: the second person is selected; Escape closes the list; Tab no longer loses a pending choice because Enter selects first.
- Done when: the checks hold, CI green, owner keyboard check passes.
- Risk: MEDIUM (shared helper plus a behaviour change)
- Confidence: MEDIUM

## ~~[x] Step 10.4 — Product picker uses the same keyboard model~~
- Task: COMP-013 (part 2) · F-19 · Priority P0 · Effort S · Mandatory · Type A.
- Files: modify: `src/MobileShop.Web/wwwroot/js/product-picker.js` only.
- Symbols: `window.wireComboboxKeys` from site.js — defined at the **top level** of site.js in Step 10.3, so it already exists when this file's `DOMContentLoaded` handler runs; its `renderResults` (or the function that rebuilds result buttons), the input handlers and the show or hide code for the result list (`[data-product-search]`, `[data-product-results]`).
- Current -> Desired: same gap as the person picker (no key handling; blur hides the list after 150 ms). Desired: identical keyboard model through `wireComboboxKeys`.
- Change: call `wireComboboxKeys(<search input>, <results element>, { open, close })` once after the elements exist (wrap inline show or hide code in local functions first if needed); call the returned `reset()` at the end of the function that rebuilds results; add the same polite "N results" live region as in 10.3. Leave the `product-created` listener alone (it belongs to the dead subsystem, R26).
- Depends on: Step 10.3.
- Edge cases / error handling: this script is only included by the Sell page, inside the page's `Scripts` section, which the layout renders BEFORE `site.js` (`_Layout.cshtml:40` vs `:41`), so `window.wireComboboxKeys` is still undefined while this script is parsed; it is defined nevertheless, because Step 10.3 places it at the top level of site.js (parse time), so it exists before this script's `DOMContentLoaded` handler runs — do not move the definition into a ready block. Call the helper inside a `DOMContentLoaded` listener (or run the wiring immediately only when `document.readyState` is not `loading`) and keep the `if (window.wireComboboxKeys)` guard as a safety net, so a load-order mistake cannot break the page; the guard must never be the reason the wiring is skipped — a missing helper is a defect to report, not a silent no-op. `hooks.open` must not change the selected product, the product id or the price state. Keep the existing selection events, price filling and `blur` timing.
- Tests: none.
- Verify: `node --check src/MobileShop.Web/wwwroot/js/product-picker.js` passes; `grep -c "wireComboboxKeys" …product-picker.js` >= 1; the definition-site check of Step 10.3 still prints `OK: …`; CI green. Owner check on Transactions > Sell: in the browser console `typeof window.wireComboboxKeys` → `function`; once the product search shows options, `(() => { const i = document.querySelector('[data-product-search]'); i.focus(); i.dispatchEvent(new KeyboardEvent('keydown', { key: 'ArrowDown', bubbles: true })); i.dispatchEvent(new KeyboardEvent('keydown', { key: 'ArrowDown', bubbles: true })); return i.getAttribute('aria-activedescendant'); })()` → a non-empty option id (proves this picker's keydown path is attached); product search, ArrowDown, Enter selects the product.
- Done when: the checks hold, CI green, owner keyboard check passes.
- Risk: LOW
- Confidence: MEDIUM

## ~~[x] Step 10.5 — Jalali date picker: glass panel and token styling~~
- Task: COMP-014 (visual) · F-04, F-07 · Priority P1 · Effort M · Mandatory · Type V.
- Files: modify: `src/MobileShop.Web/wwwroot/css/overlays.css`, `src/MobileShop.Web/wwwroot/css/app-theme.css`.
- Symbols: block `overlays-jalali-v1` (R27, derived from the existing rules); in app-theme.css the whole Jalali group: from the section comment before `.jalali-picker-panel` to the end of the file (currently `app-theme.css:250-457`; the anchor is the selector text, not the line numbers) including `@keyframes jalaliFadeIn`.
- Current -> Desired: opaque panel with a literal `rgb()` shadow, literal radii (0.5-0.65rem), literal durations, hover scale and lift effects, uppercase weekday and time labels, weights 600-700. Desired: panel is glass overlay material (92% tint, blur 24) with `--shadow-3` and edge highlight, radius `--radius-xl`, day cells and controls `--radius-sm`, tokenized transitions and `jalaliFadeIn` duration, bold via `--fw-strong`, no uppercase or letter-spacing, no hover scale or lift, pressed state `scale(0.98)`.
- Change: (1) append the block to `overlays.css`: `{ printf '\n'; sed -n '/^\/\* BEGIN overlays-jalali-v1 \*\//,/^\/\* END overlays-jalali-v1 \*\//p' .agents/chat/plan.md | tr -d '\r'; } >> src/MobileShop.Web/wwwroot/css/overlays.css`. (2) Delete the anchored Jalali group from app-theme.css (it is the end of the file; leave the file ending with a single newline).
- Depends on: Step 10.1.
- Edge cases / error handling: the panel is appended to `document.body` and positioned with `position: fixed`, so the backdrop filter cannot capture fixed children; the date input classes (`jalali-*`) are not renamed; the feedback text class `.jalali-picker-feedback` keeps `color: var(--danger)`.
- Tests: none.
- Verify: CI green. Brace balance equal for both files; `grep -c "jalali" src/MobileShop.Web/wwwroot/css/app-theme.css` = 0; `grep -cE "uppercase|letter-spacing|rgb\(|scale\(1\.0" src/MobileShop.Web/wwwroot/css/overlays.css` = 0; `grep -c "jalaliFadeIn" …overlays.css` >= 2 (keyframes and use).
- Done when: the checks hold, CI green, owner opens the picker on Sell and on Edit and checks panel, today, selected day and time inputs.
- Risk: MEDIUM
- Confidence: MEDIUM

## ~~[x] Step 10.6 — Jalali date picker: keyboard reach, focus return, close on leave~~
- Task: COMP-014 (behaviour) · F-20 · Priority P2 · Effort M · Mandatory · Type A.
- Files: modify: `src/MobileShop.Web/wwwroot/js/jalali-datepicker.js` only.
- Symbols: `buildPanel`, `wire`, `writeValue`, `close`, the document `focusin` listener (wires inputs) and the document `keydown` listener (Escape).
- Current -> Desired: dates can already be typed into the input (the real keyboard path); the panel opens on focus or click, closes on outside mouse-down or Escape, and its day buttons cannot be reached by keyboard because the panel sits at the end of `<body>`; it also stays open when Tab leaves the field. Desired: a keyboard user can open the panel, move through days, pick one and land back in the input; Tab away closes the panel.
- Change (about 45 lines): (1) in `wire(input)` extend the input `keydown`: ArrowDown with the panel open for this input (or after `open(input)`) calls `preventDefault()` and focuses the first existing of `.jalali-picker-day.is-selected`, `.jalali-picker-day.is-today`, `.jalali-picker-day`. (2) In `buildPanel` add one delegated `keydown` on the panel for targets with class `jalali-picker-day`: ArrowLeft and ArrowRight focus the previous or next day button, ArrowUp and ArrowDown the day 7 buttons before or after, Home and End the first or last, all clamped inside the current month; PageUp and PageDown call `click()` on `[data-nav="-1"]` or `[data-nav="1"]` and then focus the first day button after the re-render; Escape captures `state.input`, calls `close()` and focuses that input. Enter and Space keep the native button click (`pickDay`). (3) In `writeValue`, if `document.activeElement` is inside the panel before `close()`, capture `state.input` **before** calling `close()` (which sets `state = null`), then focus the captured input afterwards. (4) Because focusing the input would reopen the panel, set `input.dataset.jalaliSkipOpen = '1'` before any programmatic focus and make the input `focus` handler consume the flag (`delete input.dataset.jalaliSkipOpen; return;`). (5) Extend the document `focusin` listener: if a panel is open and the focus target is neither inside the panel nor the open input, call `close()`. (6) Give the month title element `aria-live="polite"` once in `buildPanel`.
- Depends on: Step 10.5.
- Edge cases / error handling: never throw when the panel is closed or `state` is null; keep typed entry, `normalize` on blur, the invalid-feedback text and the submit-time normalisation exactly as is; date-time inputs (`data-jalali-datetime`) keep their hour and minute fields reachable by Tab inside the panel.
- Tests: none.
- Verify: `node --check src/MobileShop.Web/wwwroot/js/jalali-datepicker.js` passes; `grep -c "jalaliSkipOpen" …jalali-datepicker.js` >= 2 (the flag is set before a programmatic focus and consumed in the focus handler); `grep -c "ArrowDown" …jalali-datepicker.js` >= 2 (one in the input keydown, one in the panel keydown); CI green. Owner check: focus a date field, ArrowDown lands on a day, arrows move, Enter picks and returns to the field, Escape returns to the field, Tab away closes the panel, mouse use unchanged.
- Done when: the checks hold, CI green, owner keyboard check passes.
- Risk: MEDIUM
- Confidence: MEDIUM

## ~~[x] Step 10.7 — Stage 10 validation~~
Correction round (Job B HIGH): the optional `hooks.cancel()` hook and the request-id staleness guard were added after the Step 10.7 review, so an Escape during an in-flight search discards the late response instead of re-opening the dismissed list.
- Task: QA (stage gate) · Mandatory.
- Files: none modified.
- Symbols: none.
- Current -> Desired: n/a (validation only).
- Change: confirm CI green on the last code commit and record `Action: #<run>` per code step; record the greps of 10.1-10.6; `git diff --stat` over the stage matches the file lists above.
- Manual check (owner, hard refresh): open a Create-page modal and the person-picker modal (glass versus solid, close X, Esc, focus returns to the trigger); keyboard-only selection in both pickers; the date picker with mouse and keyboard on Sell; repeat the key checks in Safari and Firefox. Anything else that changed is a defect.
- Depends on: Steps 10.1-10.6.
- Edge cases / error handling: if any check fails, stop and report; do not fix inside this step; the OS "reduce transparency" setting is **not** a Stage 10 check — the overrides that turn the glass surfaces solid (`--glass-overlay-bg`, `--glass-overlay-filter`, `--scrim-bg`, `--border-glass`) live in `motion-v1` (Step 15.1), so under Stage 10 the modals stay glass even with that setting on; the solid-surface expectation belongs to Step 15.1 and is Chrome/Windows only (Safari/macOS and Firefox do not ship `prefers-reduced-transparency`, R6).
- Tests: none.
- Verify: the commands above, recorded in act.md.
- Done when: evidence recorded and the owner's manual check answered.
- Risk: LOW
- Confidence: HIGH

# Stage 11 — Navigation shell: dock token migration, flyout grouping, focus clearance, action bar, mobile safe areas
Order rule: remove the template override first (11.1), then move the remaining shell rules into `shell.css` and retire `components.css` (11.2), then the value-identical dock token swap (11.3), then the mobile shell (11.4). Flyout grouping (UX-001, Q-09) and cross-document view transitions (SHELL-009) are deferred: no step exists until the owner approves them (R22), at which point the Planner adds one.

## [ ] Step 11.1 — Remove the scoped template stylesheet (footer and link override, F-45)
- Task: SHELL-002 · F-45 · Priority P1 · Effort S · Mandatory · Type V.
- Files: delete: `src/MobileShop.Web/Pages/Shared/_Layout.cshtml.css`; modify: `src/MobileShop.Web/Pages/Shared/_Layout.cshtml` (the `MobileShop.Web.styles.css` link only).
- Symbols: the layout line `<link rel="stylesheet" href="~/MobileShop.Web.styles.css" asp-append-version="true" />` and the whole scoped file (template leftovers `a`, `.navbar-brand`, `.btn-primary`, `.nav-pills`, `.border-top`, `.border-bottom`, `.box-shadow`, `button.accept-policy`, `.footer`).
- Current -> Desired: ASP.NET rewrites each scoped selector with a `b-…` attribute, so `.footer[b-…]` (specificity 0,2,0) beats the app's `.footer` rule: the footer is `position: absolute; bottom: 0; width: 100%; white-space: nowrap; line-height: 60px`, and the footer Privacy link is `#0077cc` (4.01:1 on the canvas) with no hover change. Desired: the file and its link are gone; the base `.footer` rule from Step 9.1 applies (in normal flow, hairline top edge, muted text, accent link with hover).
- Change: (1) `git rm src/MobileShop.Web/Pages/Shared/_Layout.cshtml.css`. (2) Delete the `MobileShop.Web.styles.css` link line in `_Layout.cshtml`. Both in one commit (otherwise every page requests a missing file). No other edit.
- Depends on: Step 9.1.
- Edge cases / error handling: no other `*.cshtml.css` file may exist (`git ls-files '*.cshtml.css'` must print nothing after the change, otherwise ASP.NET still emits the bundle and the link is still needed: stop and report); the visible change is intended: the footer sits right after the content instead of at the page bottom.
- Tests: none.
- Verify: CI green. `git ls-files '*.cshtml.css'` prints nothing; `grep -rn --exclude-dir=bin --exclude-dir=obj "MobileShop.Web.styles" src .github docs README.md` prints nothing (the `--exclude-dir` flags skip the `bin`/`obj` static-web-asset manifests, which are build output and still list the scoped bundle after the link is deleted); `git diff --stat` = the deleted file and `_Layout.cshtml` (1 line).
- Done when: the checks hold, CI green, owner confirms in the browser inspector that the footer's computed `position` is `static` and the Privacy link is the accent colour (this settles R4 F-45, previously static analysis).
- Risk: MEDIUM (visible footer change; the effect is predicted from CSS, not yet seen)
- Confidence: MEDIUM

## [ ] Step 11.2 — `shell.css`: action bar, skip link, clearance; retire `components.css`
- Task: SHELL-004, SHELL-005 · F-21 · Priority P0 · Effort M · Mandatory · Type A.
- Files: create: `src/MobileShop.Web/wwwroot/css/shell.css`; delete: `src/MobileShop.Web/wwwroot/css/components.css`; modify: `src/MobileShop.Web/Pages/Shared/_Layout.cshtml`.
- Symbols: block `shell-v1` (R27); what remains in `components.css` after Steps 9.2-9.4: `.skip-link`, `.skip-link:focus`, `.action-bar`, `.form-section .action-bar`, the `@media (max-width: 575.98px)` block for `.form-section .action-bar .btn`, the two `.has-floating-navigation` media rules, `.has-floating-navigation`, plus header comments. Layout links for `components.css` and `navigation-menu.css`.
- Current -> Desired: the action bar is an opaque sticky bar and nothing reserves scroll space for the dock and the bar, so a focused field can end up entirely hidden (WCAG 2.4.11). Desired: the action bar is subtle glass (88% tint, blur 12, brightness 0.8, hairline top edge) that turns solid while a modal is open (the dock does too); `html { scroll-padding-bottom: 12rem; }` keeps focused elements clear of dock and bar; skip link restyled with tokens.
- Change: (1) create the file: `sed -n '/^\/\* BEGIN shell-v1 \*\//,/^\/\* END shell-v1 \*\//p' .agents/chat/plan.md | tr -d '\r' > src/MobileShop.Web/wwwroot/css/shell.css`. (2) Confirm `components.css` now holds only the rules listed above (`grep -cE "^[.@]" src/MobileShop.Web/wwwroot/css/components.css` = 8, the eight blocks enumerated under "Symbols"; the count matches only by coincidence, because the command cannot see the two `body.has-floating-navigation .action-bar` selectors — if the count is not 8, stop and report), then `git rm src/MobileShop.Web/wwwroot/css/components.css`. (3) In `_Layout.cshtml` remove the `components.css` link and add `<link rel="stylesheet" href="~/css/shell.css" asp-append-version="true" />` immediately before the `navigation-menu.css` link.
- Depends on: Steps 9.2, 9.3, 9.4, 10.1.
- Edge cases / error handling: the dock is the element `.navigation-dock` (the only dock element with a backdrop filter); the `body.modal-open` rule only forces solid, the dock's own state classes are untouched; `env(safe-area-inset-bottom)` is 0 until Step 11.4 adds `viewport-fit=cover`; the `shell-v1` block's own `.has-floating-navigation { padding-bottom: … }` rule is dead (the `body.has-floating-navigation` rule with the same intent wins on specificity, exactly as it does today in `components.css`) — carry it over unchanged, do not "fix" it in this step.
- Tests: none.
- Verify: CI green. Brace balance equal for `shell.css`; `git ls-files src/MobileShop.Web/wwwroot/css | grep -c components.css` = 0; `grep -rn --exclude-dir=bin --exclude-dir=obj "components.css" src --include=*.cshtml --include=*.cs --include=*.json | grep -v "/lib/"` prints nothing (the `--exclude-dir` flags skip the `bin`/`obj` static-web-asset manifests, which are build output and list the file even after the link is removed); `grep -c "scroll-padding-bottom" …shell.css` = 1; `grep -c "shell.css" …_Layout.cshtml` = 1; `git diff --stat`: `shell.css` new, `components.css` deleted, `_Layout.cshtml`.
- Done when: the checks hold, CI green, owner Tabs through a long form (Edit product) to the last field at 1280x720 and on a narrow window and the focused field is never hidden behind the dock or the bar.
- Risk: MEDIUM
- Confidence: MEDIUM

## [ ] Step 11.3 — Dock token migration (value-identical)
- Task: SHELL-003 · F-09 · Priority P1 · Effort S · Mandatory · Type V.
- Files: modify: `src/MobileShop.Web/wwwroot/css/navigation-menu.css` only.
- Symbols (line numbers before the step): `--nav-dock-radius: 1.9rem;` (13), `--nav-spring: linear(…);` (14), `--nav-ease-out: cubic-bezier(…);` (15), `--nav-ease-in-out: cubic-bezier(…);` (16), the three `--nav-glass-*` custom properties (18-20), `z-index: 1040;` on `.floating-navigation` (24), the `box-shadow` of `.navigation-dock` (61), `background: var(--panel-raised);` inside the `@supports not` block (85) and the `prefers-reduced-transparency` block (90), and in the eyebrow rule `letter-spacing: 0.13em;` plus `text-transform: uppercase;` (298-299).
- Current -> Desired: the dock carries its own copies of values that now exist as tokens. Desired: `--nav-dock-radius: var(--radius-2xl);` (2rem, 0.1rem rounder), `--nav-spring: var(--ease-spring);`, `--nav-ease-out: var(--ease-out);`, `--nav-ease-in-out: var(--ease-standard);`, `--nav-glass-bg: var(--glass-regular-bg);`, `--nav-glass-border: var(--border-glass);`, `--nav-glass-filter: var(--glass-regular-filter);`, `z-index: var(--z-dock);`, `box-shadow: var(--shadow-3), var(--edge-highlight);`, both solid fallbacks `background: var(--glass-solid-bg);`, and the eyebrow without uppercase and tracking. Every value except the radius resolves to exactly what it was.
- Change: apply those exact replacements; do not touch `--nav-spring-pop`, `--nav-compact-scale`, `--nav-flyout-h`, the `@supports not (transition-timing-function: linear(0, 1))` line, any selector, transition, state class or script.
- Depends on: Stage 7 (tokens), Step 11.2.
- Edge cases / error handling: Stage 5/6 invariants (R24) must still hold after the edit; the `@supports not` fallback for `--nav-spring` and `--nav-spring-pop` stays as is (it overrides on the same element); the radius change to `--radius-2xl` (2rem) is desktop-only — the `@media (max-width: 575.98px)` block keeps its own `--nav-dock-radius: 2.05rem`, so on a phone the dock keeps today's radius.
- Tests: none.
- Verify: CI green. `grep -c "::before" src/MobileShop.Web/wwwroot/css/navigation-menu.css` = 2; `grep -c "@keyframes" …navigation-menu.css` = 0; `grep -cE "#[0-9A-Fa-f]{3,8}\b" …navigation-menu.css` = 0; `grep -c "rgb(" …navigation-menu.css` = 0; `grep -c "text-transform" …navigation-menu.css` = 0; `grep -c "var(--ease-spring)" …navigation-menu.css` = 1; `grep -c "is-compact" …`, `"is-settling"` and `"is-initializing"` give the same counts as before the step (1, 1, 2); `git diff --stat` = `navigation-menu.css` only.
- Done when: the checks hold, CI green, owner repeats the Stage 6 dock interaction check (hover opens, idle compacts, flyouts, keyboard Tab) in Chromium, Safari and Firefox with no change in behaviour.
- Risk: MEDIUM (value-identical, but it is the most elaborate component)
- Confidence: HIGH

## [ ] Step 11.4 — Mobile shell: safe areas, theme colour, dynamic viewport height, font preload
- Task: SHELL-006 · F-26, F-36 · Priority P1 · Effort M · Mandatory · Type V.
- Files: modify: `src/MobileShop.Web/Pages/Shared/_Layout.cshtml`, `src/MobileShop.Web/wwwroot/css/base.css`, `src/MobileShop.Web/wwwroot/css/navigation-menu.css`.
- Symbols: the layout meta `<meta name="viewport" content="width=device-width, initial-scale=1.0" />` and the `color-scheme` meta added in Step 7.2; in base.css the `body` rule (`min-height: 100vh;`) and the `.container > main[role="main"]` rule (`min-height: calc(100vh - 10rem);`); in navigation-menu.css `.floating-navigation { … bottom: 1.25rem; … }` (line 27) **and** the phone override in the `@media (max-width: 575.98px)` block, `.floating-navigation { … bottom: max(0.65rem, env(safe-area-inset-bottom)); }` (line 480) — same specificity as line 27 and later in the file, so it wins on every phone and has to be edited too.
- Current -> Desired: no `viewport-fit=cover`, so `env(safe-area-inset-bottom)` is 0 on iPhone and the dock and action bar can sit under the home indicator; no `theme-color`; `100vh` overshoots under mobile browser toolbars; the two SF Pro Rounded faces load late (`swap`, no preload). Desired: `viewport-fit=cover`; `<meta name="theme-color" content="#0E1318" />` (the canvas colour, a meta value, not a stylesheet literal); `dvh` with a `vh` fallback line; dock bottom offset `max(1.25rem, calc(env(safe-area-inset-bottom, 0px) + 0.5rem))`; preload of the regular and bold SF Pro Rounded files.
- Change: (1) `_Layout.cshtml`: change the viewport content to `width=device-width, initial-scale=1.0, viewport-fit=cover`; add `    <meta name="theme-color" content="#0E1318" />` after the `color-scheme` meta; add two links in `<head>` before the first stylesheet: `<link rel="preload" href="~/fonts/sf-pro-rounded-regular.woff2" as="font" type="font/woff2" crossorigin />` and the same for `sf-pro-rounded-bold.woff2` (same URLs the `@font-face` rules request, no version suffix). (2) base.css: add `min-height: 100dvh;` as a new line directly after `min-height: 100vh;` in `body`, and `min-height: calc(100dvh - 10rem);` directly after the `calc(100vh - 10rem)` line in the main rule (browsers without `dvh` ignore the new line and keep the old one). (3) navigation-menu.css: replace `bottom: 1.25rem;` in the `.floating-navigation` rule (line 27) with `bottom: max(1.25rem, calc(env(safe-area-inset-bottom, 0px) + 0.5rem));` **and** replace the phone override in the `@media (max-width: 575.98px)` block (line 480) with `bottom: max(0.65rem, calc(env(safe-area-inset-bottom, 0px) + 0.5rem));` — without this second edit the dock stays flush with the home indicator on every phone, because line 480 has the same specificity as line 27 and comes later.
- Depends on: Steps 7.2, 9.1, 11.3.
- Edge cases / error handling: keep the BOM and LF of `_Layout.cshtml`; the dock edit must keep `0.5rem` of clearance only when an inset exists (on desktop the inset is 0, so the offset stays 1.25rem); the phone rule keeps its own `0.65rem` floor (smaller than the desktop 1.25rem), so on a phone without an inset the dock sits 0.65rem above the bottom edge and with an inset it sits at `inset + 0.5rem`; Vazirmatn is not preloaded (Persian text is data, not above the fold); the font files must exist under `wwwroot/fonts` with exactly those names; the two preload URLs must match the `@font-face` srcs in `base.css` exactly (same file names, no `asp-append-version` on a preload link, no query string).
- Tests: none.
- Verify: CI green. `grep -c "viewport-fit=cover" src/MobileShop.Web/Pages/Shared/_Layout.cshtml` = 1; `grep -c 'rel="preload"' …_Layout.cshtml` = 2; `grep -c "theme-color" …_Layout.cshtml` = 1; `grep -c "100dvh" src/MobileShop.Web/wwwroot/css/base.css` = 2; `grep -c "safe-area-inset-bottom" src/MobileShop.Web/wwwroot/css/navigation-menu.css` = 3 — the desktop rule (line 27), the `body.has-floating-navigation` padding rule (line 474) and the phone rule (line 480) — and both `bottom` declarations must read `max(…, calc(env(safe-area-inset-bottom, 0px) + 0.5rem))`; `grep -cE "^[[:space:]]*bottom: max\(" src/MobileShop.Web/wwwroot/css/navigation-menu.css` = 2 (exactly those two declarations, nothing else); `head -c3 …_Layout.cshtml | od -An -tx1` prints ` ef bb bf`; `git diff --stat` lists the three files.
- Done when: the checks hold, CI green, owner checks on an iPhone (or Safari responsive mode with a notch device) that dock and action bar clear the home indicator and the browser chrome takes the canvas colour, and on desktop that nothing moved.
- Risk: MEDIUM
- Confidence: MEDIUM

## [ ] Step 11.5 — Stage 11 validation
- Task: QA (stage gate) · Mandatory.
- Files: none modified.
- Symbols: none.
- Current -> Desired: n/a (validation only).
- Change: confirm CI green on the last code commit and record `Action: #<run>` per code step; record the greps of 11.1-11.4; `git diff --stat` over the stage matches the file lists above; record the Stage 6 dock checks against R24.
- Manual check (owner, hard refresh): footer position and link; action bar over scrolling content (blurred, readable) on Edit and Sell; open any modal (dock and bar go solid); Tab through a long form; dock behaviour as in Stage 6; iPhone or responsive-mode safe areas; repeat the key ones in Safari and Firefox. Anything else that changed is a defect.
- Depends on: Steps 11.1-11.4.
- Edge cases / error handling: if any check fails, stop and report; do not fix inside this step.
- Tests: none.
- Verify: the commands above, recorded in act.md.
- Done when: evidence recorded and the owner's manual check answered.
- Risk: LOW
- Confidence: HIGH

# Stage 12 — Pages A: Login, Profile, Dashboard, People
Order rule: the shared password toggle first (12.1), then the login stylesheet that also retires `app-theme.css` (12.2), then Dashboard (12.3) and People (12.4). People details and create pages (CustomerDetails, SellerDetails, CreateCustomer, CreateSeller) need no step: they already use `page-head`, `form-section`, `data-table` and `_EmptyState` and inherit Stages 8-11. Security-sensitive pages (R25): Login and Profile change markup only; no handler, token or field name changes.

## [ ] Step 12.1 — `_PasswordToggle` partial (4 duplicated eye buttons)
- Task: ROUTE-001/002 (part) · F-28 · Priority P2 · Effort S · Mandatory · Type V.
- Files: create: `src/MobileShop.Web/Pages/Shared/_PasswordToggle.cshtml`; modify: `src/MobileShop.Web/Pages/Account/Login.cshtml`, `src/MobileShop.Web/Pages/Account/Profile.cshtml`.
- Symbols: the `<button class="btn btn-outline-secondary password-visibility-toggle" …>…</button>` block with its inline `<svg>` (Login lines 32-45; Profile lines 31-44 and two more copies for `NewPassword` and `ConfirmPassword`); the only part that differs is `data-password-toggle="<field name>"`.
- Current -> Desired: the same 14-line button plus SVG is pasted four times. Desired: one partial taking the field name as its model: first line `@model string`, then the block with `data-password-toggle="@Model"`; each page calls `<partial name="_PasswordToggle" model='"Password"' />` (Profile: `"CurrentPassword"`, `"NewPassword"`, `"ConfirmPassword"`).
- Change: (1) create the partial by copying the button block from `Login.cshtml` exactly (attributes `type="button"`, `aria-label="Show password"`, `title="Show password"`, `aria-pressed="false"`, the SVG with both `password-eye-*` groups) and replacing the literal `Password` in `data-password-toggle` by `@Model`. (2) Replace the block in `Login.cshtml` and the three blocks in `Profile.cshtml` by the partial calls. Inputs, labels, validation spans and `autocomplete` attributes stay untouched.
- Depends on: none (independent of the CSS stages).
- Edge cases / error handling: `site.js` finds the toggle by `data-password-toggle` and the input by that name, so the rendered HTML must be identical to before (compare the rendered button of one page in the browser source); keep the BOM and LF conventions of the new file (no BOM needed); do not touch `site.js`.
- Tests: none (existing page-model tests do not render views).
- Verify: CI green (the build compiles the Razor files). `grep -c "password-visibility-toggle" src/MobileShop.Web/Pages/Account/Login.cshtml src/MobileShop.Web/Pages/Account/Profile.cshtml` prints 0 for both; `grep -c "_PasswordToggle" …Login.cshtml` = 1 and `…Profile.cshtml` = 3; `grep -c "<svg" src/MobileShop.Web/Pages/Shared/_PasswordToggle.cshtml` = 1; `git diff --stat`: the new partial and two views.
- Done when: the checks hold, CI green, owner confirms all four eye buttons still toggle and announce state.
- Risk: LOW
- Confidence: HIGH

## [ ] Step 12.2 — `login.css` and retirement of `app-theme.css`
- Task: ROUTE-001 (visual) · F-03, F-07, F-09 · Priority P1 · Effort M · Mandatory · Type V.
- Files: create: `src/MobileShop.Web/wwwroot/css/login.css`; delete: `src/MobileShop.Web/wwwroot/css/app-theme.css`; modify: `src/MobileShop.Web/Pages/Shared/_Layout.cshtml`.
- Symbols: block `login-v1` (R27, derived from the existing rules); after Steps 8.3, 8.4, 9.1-9.5 and 10.1, 10.5 the only content left in `app-theme.css` is the login group (`.account-login-page` … `.account-login-message`, including the password toggle and validation rules); the layout link for `app-theme.css`.
- Current -> Desired: the login card has a 28px backdrop blur over an opaque fill on a flat page (no visible effect, F-03), an uppercase tracked eyebrow, literal radii and heights. Desired: glass overlay card (92% tint, blur 24, `--shadow-3`) over a local two-gradient ambient background, radius `--radius-xl`, title-case eyebrow, field and submit height `--size-control-lg`, tokenized transition; same structure and class names.
- Change: (1) create the file: `sed -n '/^\/\* BEGIN login-v1 \*\//,/^\/\* END login-v1 \*\//p' .agents/chat/plan.md | tr -d '\r' > src/MobileShop.Web/wwwroot/css/login.css`. (2) Confirm `app-theme.css` now holds only login rules (`grep -E "^[.@:]" src/MobileShop.Web/wwwroot/css/app-theme.css | grep -vc "^\.account-login\|^\.password-visibility\|^@media\|^@keyframes"` = 0 apart from comment lines; if any other rule remains, stop and report), then `git rm` it. (3) In `_Layout.cshtml` remove the `app-theme.css` link and add `<link rel="stylesheet" href="~/css/login.css" asp-append-version="true" />` immediately before the `navigation-menu.css` link.
- Depends on: Steps 8.3, 8.4, 9.1-9.5, 10.1, 10.5, 11.2 (so that nothing else remains in `app-theme.css`).
- Edge cases / error handling: the stylesheet order is now `bootstrap, tokens, base, controls, surfaces, overlays, shell, login, navigation-menu`; the legacy `--app-*` aliases in `base.css` are no longer read by any rule after this step (Step 16.1 removes them); the glass card only shows a blur effect over the page gradient, and `motion.css` (Stage 15) switches the gradient off for reduced transparency and more contrast.
- Tests: none.
- Verify: CI green. Brace balance equal for `login.css`; `git ls-files src/MobileShop.Web/wwwroot/css | grep -c app-theme.css` = 0; `grep -rn "app-theme.css" src --include=*.cshtml --include=*.cs --include=*.json | grep -v "/lib/"` prints nothing; `grep -cE "uppercase|letter-spacing|var\(--r-" …login.css` = 0; `grep -c "login.css" …_Layout.cshtml` = 1.
- Done when: the checks hold, CI green, owner confirms the login page (card readable, text contrast, focus rings, error message) and that every other page still renders as after Stage 11.
- Risk: MEDIUM
- Confidence: MEDIUM

## [ ] Step 12.3 — Dashboard heading order
- Task: ROUTE-003 · F-35 · Priority P2 · Effort S · Mandatory · Type A.
- Files: modify: `src/MobileShop.Web/Pages/Index.cshtml` only.
- Symbols: line 41 `<h5 class="card-title">@transaction.ProductLabel</h5>` under `<h2>Recent transactions</h2>` (line 28).
- Current -> Desired: heading levels jump from h2 to h5. Desired: `<h3 class="card-title h5">@transaction.ProductLabel</h3>` (the `h5` utility class keeps today's size).
- Change: that one tag; nothing else.
- Depends on: none.
- Edge cases / error handling: confirm `grep -n "card-title" src/MobileShop.Web/Pages/Index.cshtml` shows exactly one line before the edit.
- Tests: none.
- Verify: CI green. `grep -c "<h5" src/MobileShop.Web/Pages/Index.cshtml` = 0; `grep -c 'card-title h5' …Index.cshtml` = 1.
- Done when: the checks hold and CI is green.
- Risk: LOW
- Confidence: HIGH

## [ ] Step 12.4 — People: segmented filter, sortable headers with state, empty state
- Task: ROUTE-004 · F-23, F-24 · Priority P1 · Effort M · Mandatory · Type A.
- Files: modify: `src/MobileShop.Web/Pages/People/Index.cshtml`, `src/MobileShop.Web/Pages/People/Customers.cshtml`, `src/MobileShop.Web/Pages/People/Sellers.cshtml`.
- Symbols: People/Index.cshtml lines 24-37 (the `d-flex flex-wrap gap-3` wrapper with three `form-check` radios named `type`, ids `peopleAll`, `peopleCustomers`, `peopleSellers`, each with `onchange="this.form.submit()"`); Customers.cshtml and Sellers.cshtml: the three sortable `<th scope="col"><a asp-route-sortBy="…" …>…</a></th>` cells (Name, Phone, Purchased or Sold) and the table block (`<div class="table-responsive">` … `</div>`).
- Current -> Desired: the filter is three plain radios; sortable headers are links without `aria-sort` or direction cue (Transactions/Index.cshtml already has both); the two lists render an empty table when there is no data. Desired: (a) the radios become a segmented control: `<div class="segmented">` containing, for each option, `<input class="btn-check" type="radio" name="type" id="…" value="…" checked="@(Model.Type == "…")" onchange="this.form.submit()" />` followed by `<label class="btn" for="…">Label</label>`; ids, values and the `fieldset`/`legend` stay. (b) Each sortable `<th>` becomes `<th scope="col" aria-sort="@(Model.SortBy == "Name" ? (Model.Ascending ? "ascending" : "descending") : "none")"><a …existing attributes…>Name@if (Model.SortBy == "Name") { <span aria-hidden="true"> @(Model.Ascending ? "↑" : "↓")</span> }</a></th>` (use the key that cell already passes to `asp-route-sortBy`; copy the indicator markup style from Transactions/Index.cshtml). (c) Wrap the table block: `@if (!Model.Customers.Any()) { <partial name="_EmptyState" model='("No customers yet", "Add the first customer to get started.")' /> } else { …existing table block… }` (Sellers: `Model.Sellers`, "No sellers yet", "Add the first seller to get started.").
- Change: those three edits per file as described; no model, handler or route change.
- Depends on: Steps 8.4, 9.3, 9.5 (segmented CSS), 9.4.
- Edge cases / error handling: the segmented CSS expects `.btn-check` inputs as direct children of `.segmented`, each directly followed by its label; keep `onchange` (UX-002 default: keep auto-apply, Q-05); the empty-state call syntax matches People/Index.cshtml line 59; if a model property is not an `IEnumerable`, use its `Count` property instead of `Any()`.
- Tests: none (page models unchanged).
- Verify: CI green. `grep -c "segmented" src/MobileShop.Web/Pages/People/Index.cshtml` = 1; `grep -c "form-check" …People/Index.cshtml` = 0; `grep -c "aria-sort" src/MobileShop.Web/Pages/People/Customers.cshtml src/MobileShop.Web/Pages/People/Sellers.cshtml` prints 3 for each; `grep -c "_EmptyState" …Customers.cshtml …Sellers.cshtml` prints 1 for each; `git diff --stat` lists those three files.
- Done when: the checks hold, CI green, owner confirms filter switching, sorting (arrow shows, screen reader reads the sort state) and the empty state when a list has no rows.
- Risk: LOW
- Confidence: MEDIUM

## [ ] Step 12.5 — Stage 12 validation
- Task: QA (stage gate) · Mandatory.
- Files: none modified.
- Symbols: none.
- Current -> Desired: n/a (validation only).
- Change: confirm CI green on the last code commit and record `Action: #<run>` per code step; record the greps of 12.1-12.4; `git diff --stat` over the stage matches the file lists above; confirm `app-theme.css` and `components.css` and `site.css` no longer exist.
- Manual check (owner, hard refresh): sign in and out; Login (card, ambient, errors); Profile (three eye buttons, appearance of fields); Dashboard (tiles, recent transactions headings); People (segmented filter, tables, sorting, empty state); repeat the key ones in Safari and Firefox. Anything else that changed is a defect.
- Depends on: Steps 12.1-12.4.
- Edge cases / error handling: if any check fails, stop and report; do not fix inside this step.
- Tests: none.
- Verify: the commands above, recorded in act.md.
- Done when: evidence recorded and the owner's manual check answered.
- Risk: LOW
- Confidence: HIGH

# Stage 13 — Pages B: Products list, details, edit and the 11 create forms
Stages 8-12 restyle these pages through shared classes (`page-head`, `toolbar`, `form-section`, `data-table`, `status-chip`, buttons, fields, modals, action bar), so this stage is mostly verification plus one markup fix. The 11 `_ProductCreate*Form` partials, `product-create-modal.js` and `ProductCreateRegistry.cs` are dead code (R26) and are neither restyled nor deleted here. Inline `style="display:none"` attributes are JS-toggled state, not visual debt, and stay.

## [ ] Step 13.1 — Products list: type buttons become a labelled navigation that wraps
- Task: ROUTE-006 · F-32 (filters), F-27 (part) · Priority P2 · Effort S · Mandatory · Type A.
- Files: modify: `src/MobileShop.Web/Pages/Products/Index.cshtml` only.
- Symbols: line 34 `<div class="btn-group" role="group">` and its closing `</div>` on line 62 (around the `@foreach (var (key, label, _, _) in tabs)` loop of 11 type links).
- Current -> Desired: 11 joined buttons in a `btn-group` that cannot wrap, so they overflow narrow screens, and the group has no accessible name. Desired: `<nav class="d-flex flex-wrap gap-2 mb-3" aria-label="Product type">` … `</nav>`; the links keep `btn btn-primary` or `btn btn-outline-primary` and `aria-current="page"` exactly.
- Change: replace the opening tag on line 34 and the matching closing tag on line 62; nothing inside the loop changes.
- Depends on: Step 8.3 (button system).
- Edge cases / error handling: the loop body (both branches, with and without `partNumberId`) is untouched; `mb-3` replaces the spacing the joined group never had, so the toolbar below moves down by 1rem (intended); do not convert these links into a segmented control (they are navigation, not radios).
- Tests: none.
- Verify: CI green. `grep -c "btn-group" src/MobileShop.Web/Pages/Products/Index.cshtml` = 0; `grep -c 'aria-label="Product type"' …Index.cshtml` = 1; `git diff --stat` = that file, 2 changed lines.
- Done when: the checks hold, CI green, owner confirms the 11 buttons wrap on a phone and the active one still shows.
- Risk: LOW
- Confidence: HIGH

## [ ] Step 13.2 — Stage 13 validation (includes the owner visual pass)
- Task: QA (stage gate) · Mandatory.
- Files: none modified.
- Symbols: none.
- Current -> Desired: n/a (validation only).
- Change: confirm CI green on the last code commit and record `Action: #<run>`; record the greps of 13.1; `git diff --stat` over the stage = `Products/Index.cshtml`.
- Manual check (owner, hard refresh): Products list for phone and two other types (buttons, filters, table, chips, wrapping on a phone); SecondHand; Product Details (sections, the Apple ID password still shown as plain `code`, transactions table); Edit (all sections, finished price, action bar, Tab to the last field); all 11 Create pages (section order, "Add new" modals open with the glass sheet and a visible close X, finished-price field, action bar); Persian text in names renders with the system fallback stack — the Vazirmatn face cannot load before Step 15.2 repairs the tracked font assets, so the fallback is expected here and the repair is scheduled in Stage 15, not a Stage 13 defect; repeat the key ones in Safari and Firefox. Anything else that changed is a defect; list defects in act.md for a fix step.
- Depends on: Step 13.1.
- Edge cases / error handling: if any check fails, stop and report; do not fix inside this step.
- Tests: none.
- Verify: the commands above, recorded in act.md.
- Done when: evidence recorded and the owner's manual check answered.
- Risk: LOW
- Confidence: HIGH

# Stage 14 — Pages C: Transactions, Reports, Error and Privacy
Sell and Transactions/Details need no step: they inherit buttons, fields, surfaces, tables, the glass date picker, the pickers' keyboard model and the action bar from Stages 8-11. Privacy keeps the owner's placeholder sentence (policy text is the owner's decision, R22 Q-11); only its inline width style changes.

## [ ] Step 14.1 — Transactions: visible result count
- Task: ROUTE-009 (part) · F-22 (feedback) · Priority P2 · Effort S · Optional (UX, additive) · Type U.
- Files: modify: `src/MobileShop.Web/Pages/Transactions/Index.cshtml` only.
- Symbols: the `else` branch after `@if (Model.Transactions.Count == 0)` (line 63) that opens `<div class="table-responsive">` (line 73).
- Current -> Desired: after the filters reload the page there is no statement of how many rows are shown. Desired: a line `<p class="text-muted small">@Model.Transactions.Count transactions shown</p>` directly before `<div class="table-responsive">`.
- Change: add that one line; no handler, filter or script change.
- Depends on: Step 9.3.
- Edge cases / error handling: plain text, not a live region (the page reloads, so a live region would not announce); use the same collection the table iterates (`Model.Transactions`).
- Tests: none.
- Verify: CI green. `grep -c "transactions shown" src/MobileShop.Web/Pages/Transactions/Index.cshtml` = 1; `git diff --stat` = that file, 1 line.
- Done when: the checks hold and CI is green.
- Risk: LOW
- Confidence: HIGH

## [ ] Step 14.2 — ProfitLoss: describe the trend chart
- Task: ROUTE-011 · Priority P2 · Effort S · Mandatory · Type A.
- Files: modify: `src/MobileShop.Web/Pages/Reports/ProfitLoss.cshtml` only.
- Symbols: `<canvas id="profitLossChart" … aria-label="Profit and loss by …"></canvas>` (about lines 187-193) inside `<div class="profit-chart-canvas">` (line 186).
- Current -> Desired: the canvas has an `aria-label` only. Desired: `aria-describedby="profit-chart-desc"` on the canvas and, directly after the closing `</div>` of `.profit-chart-canvas`, `<p id="profit-chart-desc" class="visually-hidden">Line chart of profit and loss over the selected date range. The table below lists profit and loss per product.</p>`.
- Change: those two additions; keep every existing attribute, the chart script and the table.
- Depends on: none.
- Edge cases / error handling: apply it to the trend chart only (`profitLossChart`); if the page has a second chart in the Distribution tab, leave it unchanged; confirm the table described really is on the same tab (`grep -n "data-table" …ProfitLoss.cshtml`).
- Tests: none.
- Verify: CI green. `grep -c 'aria-describedby="profit-chart-desc"' src/MobileShop.Web/Pages/Reports/ProfitLoss.cshtml` = 1; `grep -c 'id="profit-chart-desc"' …` = 1.
- Done when: the checks hold and CI is green.
- Risk: LOW
- Confidence: HIGH

## [ ] Step 14.3 — Error page copy and the `measure` class
- Task: ROUTE-012 · F-33, F-30 (part) · Priority P2 · Effort S · Mandatory · Type V.
- Files: modify: `src/MobileShop.Web/Pages/Error.cshtml`, `src/MobileShop.Web/Pages/Privacy.cshtml`, `src/MobileShop.Web/wwwroot/css/surfaces.css`.
- Symbols: Error.cshtml `<div style="max-width: 70ch;">` and the block from `<h2>Development Mode</h2>` through the end of the second `<p>` that follows it (the paragraph ending "and restarting the app."); Privacy.cshtml the same `<div style="max-width: 70ch;">`.
- Current -> Desired: end users see developer instructions about the Development environment on every error. Desired: those heading and two paragraphs are removed (the apology sentence and the `Request ID` block stay); both pages use `<div class="measure">` instead of the inline style; `surfaces.css` gets `.measure { max-width: var(--size-measure); }`.
- Change: (1) edit the two views as described. (2) Append to `surfaces.css`: a blank line then the rule `.measure {` / `  max-width: var(--size-measure);` / `}`.
- Depends on: Step 9.2.
- Edge cases / error handling: keep the BOM on both views; the Request ID paragraph is shown only when `Model.ShowRequestId` and must stay; do not touch `Error.cshtml.cs`.
- Tests: none.
- Verify: CI green. `grep -c "Development" src/MobileShop.Web/Pages/Error.cshtml` = 0; `grep -c "max-width: 70ch" src/MobileShop.Web/Pages/Error.cshtml src/MobileShop.Web/Pages/Privacy.cshtml` prints 0 for both; `grep -c 'class="measure"' …Error.cshtml …Privacy.cshtml` prints 1 for each; `grep -c "^\.measure" …surfaces.css` = 1; brace balance equal for `surfaces.css`.
- Done when: the checks hold, CI green, owner opens `/Error` and `/Privacy`.
- Risk: LOW
- Confidence: HIGH

## [ ] Step 14.4 — Stage 14 validation
- Task: QA (stage gate) · Mandatory.
- Files: none modified.
- Symbols: none.
- Current -> Desired: n/a (validation only).
- Change: confirm CI green on the last code commit and record `Action: #<run>` per code step; record the greps of 14.1-14.3; `git diff --stat` over the stage matches the file lists above.
- Manual check (owner, hard refresh): Transactions list (count line, filters, sort, row checkboxes read by a screen reader), Sell (pickers by keyboard, date picker, action bar), Transaction Details, ProfitLoss (segmented mode, tabs, chart cards, screen reader reads the chart description), `/Error`, `/Privacy`; repeat the key ones in Safari and Firefox. Anything else that changed is a defect.
- Depends on: Steps 14.1-14.3.
- Edge cases / error handling: if any check fails, stop and report; do not fix inside this step.
- Tests: none.
- Verify: the commands above, recorded in act.md.
- Done when: evidence recorded and the owner's manual check answered.
- Risk: LOW
- Confidence: HIGH

# Stage 15 — Motion and preference modes: motion tokens, press feedback, reduced motion/transparency/contrast, solid-surfaces switch
Press feedback and token-driven transitions already shipped with each family block (buttons in 8.3, tables and chips in 9.x, overlays in 10.x). This stage adds the preference modes in one place, the modal sheet entrance, the owner switch for solid surfaces, repairs the broken Vazirmatn font assets, and removes the font weight that no rule requests any more.
Order rule: the motion and preference modes first (15.1), then the Vazirmatn font-asset repair (15.2) — it must run before the Medium face is retired (15.3) and before the stage validation (15.5), because every owner check in this stage looks at Persian text — then the owner switch for solid surfaces (15.4).

## [ ] Step 15.1 — `motion.css`: preference modes and sheet entrance
- Task: MOTION-002/003/004 · F-41 · Priority P0 · Effort M · Mandatory · Type A.
- Files: create: `src/MobileShop.Web/wwwroot/css/motion.css`; modify: `src/MobileShop.Web/Pages/Shared/_Layout.cshtml`.
- Symbols: block `motion-v1` (R27); the layout `<link>` for `navigation-menu.css`.
- Current -> Desired: only the dock honours reduced transparency; every other glass surface ignores the OS settings and there is no high-contrast or forced-colors handling. Desired: because every material reads only the `--glass-*`, `--scrim-*` and `--border-glass` tokens, three override blocks (`prefers-reduced-transparency: reduce`, `html[data-surfaces="solid"]`, `prefers-contrast: more`) turn all glass solid in one place; the contrast block also lifts muted text, borders and the focus ring; a `forced-colors: active` block uses system colours; pressed scale is removed under reduced motion; the modal sheet enters with `translateY(12px) scale(0.98)` over `--dur-sheet` with the spring curve and the backdrop fades over `--dur-slow`.
- Change: (1) create the file: `sed -n '/^\/\* BEGIN motion-v1 \*\//,/^\/\* END motion-v1 \*\//p' .agents/chat/plan.md | tr -d '\r' > src/MobileShop.Web/wwwroot/css/motion.css`. (2) Add `<link rel="stylesheet" href="~/css/motion.css" asp-append-version="true" />` immediately before the `navigation-menu.css` link (after `login.css`).
- Depends on: Steps 8.3, 10.1, 12.2.
- Edge cases / error handling: `motion.css` must load after `tokens.css` and after every family file so its `:root` overrides win by order (same specificity); the global reduced-motion rule (0.01 ms) already lives in `base.css` and stays; Bootstrap's `.modal.fade .modal-dialog` default transform is overridden on purpose; no change touches the dock's own state machine.
- Tests: none.
- Verify: CI green. Brace balance equal for `motion.css`; `grep -c "html\[data-surfaces=\"solid\"\]" src/MobileShop.Web/wwwroot/css/motion.css` >= 2; `grep -c "prefers-contrast: more" …motion.css` >= 2; `grep -c "forced-colors" …motion.css` = 1; `grep -c "motion.css" src/MobileShop.Web/Pages/Shared/_Layout.cshtml` = 1; `git diff --stat`: `motion.css` new, `_Layout.cshtml` +1.
- Done when: the checks hold, CI green, owner confirms with the OS "reduce transparency" (or Windows "transparency effects" off) that modals, action bar and login card turn solid — Chrome/Edge on Windows only, since Safari/macOS and Firefox do not ship `prefers-reduced-transparency` (R6) and there the solid surfaces come from the `html[data-surfaces="solid"]` switch of Step 15.4 — with the OS high-contrast setting that borders and text strengthen, and with "reduce motion" that presses and the sheet entrance do not animate.
- Risk: MEDIUM
- Confidence: MEDIUM

## [ ] Step 15.2 — Repair the Vazirmatn font assets (the tracked `.woff2` files are HTML pages, not fonts)
- Task: F-06 (font assets), F-36 · Priority P0 · Effort M · Mandatory · Type V.
- Files: replace: `src/MobileShop.Web/wwwroot/fonts/vazirmatn-regular.woff2`, `src/MobileShop.Web/wwwroot/fonts/vazirmatn-medium.woff2`, `src/MobileShop.Web/wwwroot/fonts/vazirmatn-bold.woff2`; modify (fallback branch only): `src/MobileShop.Web/wwwroot/css/tokens.css`.
- Symbols: the three `@font-face` blocks of family Vazirmatn in `src/MobileShop.Web/wwwroot/css/tokens.css` (lines 1-23: weight 400 → `src: url('../fonts/vazirmatn-regular.woff2')`, weight 500 → `../fonts/vazirmatn-medium.woff2`, weight 700 → `../fonts/vazirmatn-bold.woff2`); the three tracked files `src/MobileShop.Web/wwwroot/fonts/vazirmatn-regular.woff2`, `vazirmatn-medium.woff2`, `vazirmatn-bold.woff2`.
- Current -> Desired: the three tracked `.woff2` files are GitHub HTML documents (about 269 KB each; `file` prints "HTML document"), not fonts: the browser rejects them, `document.fonts` reports Vazirmatn as `unloaded` and every Persian string in the app falls back to a system font, so the brand type stack of R2 ("SF Pro Rounded + Vazirmatn") is broken and about 807 KB of HTML is downloaded on demand. Desired: the three paths hold real Vazirmatn woff2 binaries (weights 400, 500, 700) — or, if the binaries cannot be obtained, the Vazirmatn `@font-face` rules and the files are gone and the app falls back to the token-defined stack (SF Pro Rounded 400/700, then the system stack). Which one applies is decided by the font policy of R11.C and R7: the plan keeps the Vazirmatn family with the weights 400 and 700 (only the unused 500 is retired, in Step 15.3), so the repair — not the removal — is the prescribed end state.
- Change: (1) Repair branch — get the binaries from the upstream Vazirmatn release (`rastikerdar/vazirmatn`, SIL Open Font License): the webfont files `fonts/webfonts/Vazirmatn-Regular.woff2`, `Vazirmatn-Medium.woff2` and `Vazirmatn-Bold.woff2` of a tagged release, copied over the three existing paths under the existing names (`vazirmatn-regular.woff2`, `vazirmatn-medium.woff2`, `vazirmatn-bold.woff2`); the `@font-face` srcs stay exactly as they are, so no CSS changes in this branch. The files in the repository today are HTML pages and must never be committed again — a `curl` of a GitHub page into `wwwroot/fonts` is how the defect was created. (2) Fallback branch — if the Actor cannot obtain the binaries (no network or no upstream copy at hand), do NOT re-commit the HTML files and do NOT leave a broken `@font-face`: delete the three Vazirmatn `@font-face` blocks from `tokens.css` and `git rm` the three files, so nothing broken is served and `--font-body` falls through to the system Persian face (the family name may stay in the stack: without a `@font-face` and without a local install the browser skips it). Record in act.md that the fallback was taken; Step 15.3 then only confirms that no weight-500 rule remains and that the medium file is gone. (3) In both branches nothing else changes: no font file name, no `@font-face` src, no CSS value, no layout change.
- Depends on: none (no other step owns the font assets); it must run before Step 15.3.
- Edge cases / error handling: the three files are tracked in git, so the replacement is a plain content commit of exactly those three paths (`git add` them by name, never `-A`); the weight-500 file is repaired here and deleted again in Step 15.3, which is why the Medium face is repaired first and retired second; the SF Pro Rounded files and their `@font-face` rules in `base.css` are never touched; Vazirmatn is not preloaded (Step 11.4 preloads only the Latin faces), so a repaired file is fetched on the first Persian text; if the Actor obtains the upstream variable file `Vazirmatn[wght].woff2` instead of the three static weights, it must be split into static 400/500/700 files or only the 400 and 700 files may be replaced — the `@font-face` weights stay 400, 500 and 700.
- Tests: none.
- Verify: CI green. Repair branch: `file src/MobileShop.Web/wwwroot/fonts/vazirmatn-regular.woff2 src/MobileShop.Web/wwwroot/fonts/vazirmatn-medium.woff2 src/MobileShop.Web/wwwroot/fonts/vazirmatn-bold.woff2` prints "Web Open Font Format" for all three; `head -c 4 src/MobileShop.Web/wwwroot/fonts/vazirmatn-regular.woff2 | od -An -tx1` prints ` 77 4f 46 32` (the `wOF2` signature); `grep -c "fonts/vazirmatn" src/MobileShop.Web/wwwroot/css/tokens.css` = 3. Fallback branch: `git ls-files src/MobileShop.Web/wwwroot/fonts` prints only `sf-pro-rounded-bold.woff2` and `sf-pro-rounded-regular.woff2`; `grep -rn --exclude-dir=bin --exclude-dir=obj "fonts/vazirmatn" src` prints nothing (the `bin`/`obj` manifests are build output and still list the old asset paths). Owner check (browser, hard refresh): a Persian name in the Products list renders in the Vazirmatn face (repair branch) or in the system Persian face with no failed font request in the console (fallback branch).
- Done when: the checks hold, CI green, and the owner confirms in the browser console that Vazirmatn is no longer reported as `unloaded` (repair branch) and that no font request failed.
- Risk: MEDIUM (binary assets the CI cannot inspect and the Actor may not be able to fetch them; the fallback branch changes the Persian face)
- Confidence: MEDIUM

## [ ] Step 15.3 — Retire the unused Vazirmatn Medium face
- Task: COMP-006 (remainder) · F-06, F-36 · Priority P2 · Effort S · Mandatory · Type V.
- Files: modify: `src/MobileShop.Web/wwwroot/css/tokens.css`; delete: `src/MobileShop.Web/wwwroot/fonts/vazirmatn-medium.woff2`.
- Symbols: the `@font-face` rule of family Vazirmatn with `font-weight: 500` and `src: url('../fonts/vazirmatn-medium.woff2')` (about lines 10-16).
- Current -> Desired: after Stages 8-12 no rule asks for weight 500, 600 or 650 (only 400 and 700 remain), so the Medium file (about 269 KB) is never selected. Desired: the face and the file are removed.
- Change: (1) first prove it: `grep -rnE "font-weight: *(500|600|650)" src/MobileShop.Web/wwwroot/css` must print nothing (the dock's `font-weight: 700` lines are fine); if anything prints, replace each remaining 500 by `var(--fw-regular)` and each 600 or 650 by `var(--fw-strong)` (those are the weights that render today) and re-run. (2) Delete the `@font-face` block for weight 500 from tokens.css. (3) `git rm src/MobileShop.Web/wwwroot/fonts/vazirmatn-medium.woff2`.
- Depends on: Steps 8.3, 8.4, 9.1-9.4, 10.5, 12.2, 15.2.
- Edge cases / error handling: confirm nothing else references the file (`grep -rn --exclude-dir=bin --exclude-dir=obj "vazirmatn-medium" src README.md docs` prints nothing after the edit; the `bin`/`obj` manifests are build output and still list the old asset path); Persian text that was Medium (500) now renders Regular or Bold, a small intended change; keep the Regular and Bold faces; in the fallback branch of Step 15.2 the file and the weight-500 block are already gone — confirm that, record it in act.md and change nothing.
- Tests: none.
- Verify: CI green. `grep -c "font-weight: 500" src/MobileShop.Web/wwwroot/css/tokens.css` = 0; `git ls-files src/MobileShop.Web/wwwroot/fonts | grep -c medium` = 0; brace balance equal for `tokens.css`.
- Done when: the checks hold, CI green, owner confirms Persian names in tables and cards still render.
- Risk: LOW
- Confidence: HIGH

## [ ] Step 15.4 — Solid-surfaces switch (Profile page, remembered per browser)
- Task: MOTION-005 · F-08 (parity) · Priority P1 · Effort M · Mandatory · Type U.
- Files: modify: `src/MobileShop.Web/Pages/Shared/_Layout.cshtml`, `src/MobileShop.Web/Pages/Account/Profile.cshtml`, `src/MobileShop.Web/wwwroot/js/site.js`.
- Symbols: in the layout `<head>` after the `color-scheme` meta; in Profile.cshtml the end of the second `<section class="form-section" aria-label="Change username or password">` (closing `</section>` on line 99, inside `<div class="col-md-6">`); in site.js the ready block.
- Current -> Desired: Safari and Firefox expose no reduced-transparency setting (only Chrome and Edge do), so their users cannot turn glass off. Desired: a checkbox on the Profile page toggles `data-surfaces="solid"` on `<html>`, remembered in `localStorage` under `mobileshop.surfaces`, applied before first paint.
- Change: (1) `_Layout.cshtml`: add in `<head>` after the `color-scheme` meta one inline script: `<script>try{if(localStorage.getItem('mobileshop.surfaces')==='solid')document.documentElement.setAttribute('data-surfaces','solid');}catch(e){}</script>`. (2) Profile.cshtml: after the closing `</section>` on line 99 add a new `<section class="form-section" aria-label="Appearance">` with `<h2 class="form-section__title">Appearance</h2>` and `<div class="form-check"><input class="form-check-input" type="checkbox" id="surfacesSolid" data-surfaces-toggle /><label class="form-check-label" for="surfacesSolid">Solid surfaces (turn off glass transparency)</label></div>`; it is outside the password form. (3) site.js: in the ready block find `[data-surfaces-toggle]`; if present set `checked` from `document.documentElement.dataset.surfaces === 'solid'`; on `change` set or delete `document.documentElement.dataset.surfaces` and write or remove the `localStorage` key, all storage access inside `try/catch`.
- Depends on: Step 15.1.
- Edge cases / error handling: storage can be unavailable or blocked, so every access is guarded and the page must work without it; no cookie, no server call, no change to the password form, its antiforgery token or its handler (Profile is a security-sensitive page, R25); keep the BOM of `_Layout.cshtml`.
- Tests: none.
- Verify: `node --check src/MobileShop.Web/wwwroot/js/site.js` passes; CI green. `grep -c "mobileshop.surfaces" src/MobileShop.Web/Pages/Shared/_Layout.cshtml src/MobileShop.Web/wwwroot/js/site.js` prints 1 and 2 or more; `grep -c "data-surfaces-toggle" src/MobileShop.Web/Pages/Account/Profile.cshtml` = 1 and `…site.js` >= 1; `head -c3 …_Layout.cshtml | od -An -tx1` prints ` ef bb bf`.
- Done when: the checks hold, CI green, owner ticks the box in Safari and Firefox: modals, action bar, login card and dock switch to solid immediately and stay solid after reload and on other pages; unticking restores glass.
- Risk: MEDIUM
- Confidence: MEDIUM

## [ ] Step 15.5 — Stage 15 validation
- Task: QA (stage gate) · Mandatory.
- Files: none modified.
- Symbols: none.
- Current -> Desired: n/a (validation only).
- Change: confirm CI green on the last code commit and record `Action: #<run>` per code step; record the greps of 15.1-15.4; `git diff --stat` over the stage matches the file lists above.
- Manual check (owner): the four modes of R12.B, each in at least two browsers: reduced motion, reduced transparency (and the Profile switch), more contrast, forced colors (Windows high contrast); focus rings visible in all of them; nothing unreadable; the dock keeps its Stage 6 behaviour in every mode; Persian text renders in the Vazirmatn face (or, in the fallback branch of Step 15.2, in the system Persian face without a failed font request).
- Depends on: Steps 15.1-15.4.
- Edge cases / error handling: if any check fails, stop and report; do not fix inside this step.
- Tests: none.
- Verify: the commands above, recorded in act.md.
- Done when: evidence recorded and the owner's manual check answered.
- Risk: LOW
- Confidence: HIGH

# Stage 16 — QA and handoff: browser harness decision, baselines, accessibility sweep, performance, final audit
The Actor does the mechanical clean-up (16.1) and prepares the evidence sheet; the owner runs the browser measurements (16.2), because the sandbox and CI have no browser. The optional Playwright and axe harness stays a separate owner decision (Q-07); if approved the Planner adds a step for it.

## [ ] Step 16.1 — Final consistency audit and clean-up
- Task: QA-006 · F-37, F-38 · Priority P1 · Effort M · Mandatory · Type V.
- Files: modify: `src/MobileShop.Web/Pages/Transactions/Sell.cshtml`, `src/MobileShop.Web/Pages/Products/Edit.cshtml`; possibly `src/MobileShop.Web/wwwroot/css/tokens.css` and `base.css` (only unused tokens, see below).
- Symbols: the page-level `<script src="~/js/jalali-datepicker.js" asp-append-version="true"></script>` lines inside the `Scripts` sections of Sell.cshtml (about line 47) and Edit.cshtml (the layout already loads that script on every page); legacy tokens `--r-control`, `--r-panel`, `--r-overlay` in tokens.css and the `--app-*` aliases in the `:root` block of base.css.
- Current -> Desired: the date picker script is requested twice on two pages (guarded, harmless but wasteful); legacy radius tokens and `--app-*` aliases may now be unreferenced. Desired: the two redundant includes are removed; each legacy token or alias that no stylesheet, view or script references any more is deleted; the end-state CSS file set is exactly `tokens.css`, `base.css`, `controls.css`, `surfaces.css`, `overlays.css`, `shell.css`, `login.css`, `motion.css`, `navigation-menu.css`.
- Change: (1) remove the two includes only after confirming `grep -n "jalali-datepicker" src/MobileShop.Web/Pages/Shared/_Layout.cshtml` still shows the layout include. (2) For each of `--r-control`, `--r-panel`, `--r-overlay` and each `--app-*` name run `grep -rn "var(--NAME)" src/MobileShop.Web --include=*.css --include=*.cshtml --include=*.js`; delete the definition only when nothing references it. (3) Run the audit gates below and record the numbers.
- Depends on: all earlier stages.
- Edge cases / error handling: the Sell page's own inline script runs in the `Scripts` section before `site.js`; it must not call the date picker at parse time (check with `grep -n "jalali" src/MobileShop.Web/Pages/Transactions/Sell.cshtml`); keep `--ground`, `--panel`, `--panel-raised`, `--line`, `--line-strong`, `--ink`, `--ink-muted` (the semantic tokens alias them); do not delete the dead create-product subsystem (R26) in this step.
- Tests: none.
- Verify: CI green. Audit gates (record each): `git ls-files src/MobileShop.Web/wwwroot/css` lists exactly the nine files above; hex literals in CSS outside `tokens.css`: `grep -rnE "#[0-9A-Fa-f]{3,8}\b" src/MobileShop.Web/wwwroot/css | grep -v tokens.css` prints nothing; `!important` count across all CSS is at most 12 (`grep -c "!important"` summed; baseline was 13); `grep -rn "backdrop-filter" src/MobileShop.Web/wwwroot/css | grep -v "overlays.css\|shell.css\|login.css\|motion.css\|navigation-menu.css\|tokens.css"` prints nothing; no `transform: translateY(-1px)` or `scale(1.0` hover effects remain (`grep -rnE "translateY\(-1px\)|scale\(1\.0" src/MobileShop.Web/wwwroot/css` prints nothing); total CSS bytes `cat src/MobileShop.Web/wwwroot/css/*.css | wc -c` at most 60,000; `grep -c "jalali-datepicker" src/MobileShop.Web/Pages/Transactions/Sell.cshtml src/MobileShop.Web/Pages/Products/Edit.cshtml` prints 0 for both.
- Done when: the gates are recorded, CI green, owner confirms the date picker still works on Sell and Edit.
- Risk: MEDIUM
- Confidence: MEDIUM

## [ ] Step 16.2 — Owner verification pack (browser evidence)
- Task: QA-001..005 · Priority P0 · Effort L · Mandatory · Type T.
- Files: none modified (evidence goes into act.md; screenshots stay outside the repository).
- Symbols: none.
- Current -> Desired: no browser evidence exists for any stage; the baseline set from R19 was captured before Stage 8 (if the owner followed the Step 7.2 precondition). Desired: before-and-after evidence for the whole redesign.
- Change: the Actor prepares the sheet; the owner fills it: (1) after-screenshots of the same routes, viewports (360x740, 768x1024, 1440x900) and states as the baseline, side by side. (2) Browser matrix: Chrome or Edge, Safari (macOS and iOS), Firefox; each stage's manual checks repeated once on the final build. (3) Keyboard-only flows without a mouse: sign in, record a sale (both pickers and the date picker), create a product, filter and sort People and Transactions, change a password field visibility, open and close modals; focus always visible and never hidden behind the dock or the action bar. (4) Accessibility scan: a browser extension run (axe or Accessibility Insights) on Login, Dashboard, Products list, Sell, Edit, ProfitLoss with 0 critical and 0 serious findings recorded; a short screen-reader spot check (VoiceOver or NVDA) of Login errors and the Sell form. (5) Contrast: run the snippet in R19 for the glass materials (muted text over a white and an accent strip) and compare with R10. (6) Performance: Lighthouse mobile on Login, Dashboard, Products list, Sell against the Stage 7 baseline (LCP, CLS, INP or TBT, transferred bytes); budgets in R7. (7) The four modes of R12.B once more. (8) Touch targets: every button and field is at least 44px tall with a coarse pointer.
- Depends on: Step 16.1.
- Edge cases / error handling: a failed item becomes a defect list in act.md; the Planner turns defects into fix steps; nothing is fixed inside this step.
- Tests: none.
- Verify: the filled evidence sheet in act.md with the counts requested above.
- Done when: every item has evidence and every defect is either fixed by a later step or accepted by the owner in writing.
- Risk: LOW
- Confidence: MEDIUM

## [ ] Step 16.3 — README: short UI design-system section
- Task: QA-007 · Priority P2 · Effort S · Optional · Type D.
- Files: modify: `README.md` only.
- Symbols: a new section "UI design system" near the existing project documentation.
- Current -> Desired: nobody can find the rules that keep the UI consistent once this plan is retired. Desired: a section of at most 25 lines: the CSS file list and load order; the token families (surfaces, text, borders, glass, shadow, radius, space, size, motion, z); the four materials and where each is allowed (R10); the state, motion and preference-mode rules (R12); the gates of Step 16.1; a pointer to `.agents/` for the pipeline.
- Change: write that section from R9-R13; no code, no screenshots, no other README edit.
- Depends on: Step 16.1.
- Edge cases / error handling: keep the README's existing tone and heading levels; do not copy the Reference wholesale.
- Tests: none.
- Verify: `git diff --stat` = `README.md`; the new section is at most 25 lines.
- Done when: the section exists and CI is green (CI ignores `*.md`).
- Risk: LOW
- Confidence: HIGH

## [ ] Step 16.4 — Redesign sign-off
- Task: QA (final gate) · Mandatory.
- Files: none modified.
- Symbols: none.
- Current -> Desired: n/a (validation only).
- Change: confirm CI green on the last code commit with its run number; confirm the redesign Definition of Done (R19) item by item with the evidence from 16.1 and 16.2; confirm every stage line in `TODO.md` is ticked by the Reviewer sign-offs; list open owner decisions (R22) and the dead-code cleanup proposal (R26) for a separate decision.
- Depends on: Steps 16.1-16.3.
- Edge cases / error handling: if any item is missing evidence, stop and report; do not sign off.
- Tests: none.
- Verify: the confirmation list recorded in act.md.
- Done when: evidence recorded and the owner's sign-off answered.
- Risk: LOW
- Confidence: HIGH

## Global Definition of Done
- Every stage ends with its own validation step (8.5, 9.7, 10.7, 11.5, 12.5, 13.2, 14.4, 15.5, 16.4; Stage 7 has 7.4): CI green with run numbers recorded in act.md, the greps of the stage recorded, `git diff --stat` matching the stage's file lists, and the owner's manual check answered.
- The whole redesign is done when the Definition of Done in R19 holds and Step 16.4 is signed off.

---

# Redesign Reference
Long-lived, non-actionable. The actionable work is the stage steps above (Stages 7-16, every step written out). Steps cite these sections by ID; CSS comes from the validated spec blocks in R27. Status: DISC and RES tasks done; nothing in this Reference has been implemented.

## R1 Executive summary
- Assessment: a coherent dark UI with a real token file, a small shared component layer and one well-engineered glass surface (the Stage 5/6 floating dock). Gaps: partial token coverage (surface steps only ~1.1:1, radius and colour literals, two table idioms, three card idioms), Bootstrap light defaults leaking into the dark theme (select chevron, modal close X, one bright hover), a handful of unnamed controls (12 read-only price labels, the row checkbox), custom comboboxes a keyboard user cannot complete, opaque overlays, a flat canvas, and a footer and link still styled by .NET template CSS.
- Direction: "quiet content, glass chrome". Opaque, low-noise content surfaces; Regular glass only on the navigation layer (dock, flyout), modal sheets, the date-picker panel, the action bar and the login card; concentric radii; elevation by shadow plus edge highlight; token-driven motion; every effect has a solid fallback, set in one place through the glass tokens, plus an owner switch because Safari and Firefox lack `prefers-reduced-transparency`.
- Stack decision: stay on .NET 10 Razor Pages + Bootstrap 5.3.3 + vanilla CSS/JS. No runtime dependency is added. The React/shadcn/Radix/Motion candidates do not fit the stack (R17). One new partial (`_PasswordToggle`) is the only new abstraction (D-11).
- Scale: 33 routable pages (32 render UI), 19 shared partials (11 are dead code, R26), 1 view component, 5 stylesheets plus 1 scoped template stylesheet today and 9 planned, 8 scripts (1 dead), 29 Bootstrap modals; 10 stages, 47 steps, 46 audit findings (R4).
- Method: each CSS step creates or extends a family file from a spec block in R27 (parse-checked, no undefined `var()`, no hex literals, selector coverage compared against the old rules) and deletes the replaced rules by anchor in the same commit; `app-theme.css`, `components.css`, `site.css` and the scoped template stylesheet all disappear by Step 12.2.
- First milestone: Stage 7 (tokens, dark-mode flag, dead-CSS cleanup), then Stage 8 (accessibility markup first, then buttons and fields).
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
`_Layout` (head, skip link, container/main, footer, dock invocation, scripts) · `_EmptyState` (title + hint, already used on Dashboard, People/Index, Products, SecondHand, Transactions, ProfitLoss) · `_PersonPicker` (customer/seller search combobox with its own modal, already labelled) · `_ProductPickerOptions`, `_ProductSellerPicker` · `_ProductEditForm` (447 lines, used by Edit) · `_ValidationScriptsPartial` · view component `NavigationMenu` (6 tabs, 6 flyout panels, 22 links; Products flyout has 13). DEAD (R26): the 11 `_ProductCreate{AppleId,Cable,Case,Charger,Glass,Laptop,Phone,PortableStorage,PowerBank,SmartWatch,Tablet}Form` partials and `_ProductCreateSuccess`' sibling subsystem: `product-create-modal.js` is included by no view, no markup defines `#create-product-modal`, and `ProductCreateRegistry` has no consumer.
Products type "tabs" are 11 link buttons (`btn btn-primary` or `btn btn-outline-primary` with `aria-current`), not Bootstrap tabs; `.nav-tabs` with `data-bs-toggle="tab"` exists only on ProfitLoss; the ProfitLoss date-range mode is a `btn-group` of radios with `btn-check`.
### Element usage counts [G]
Buttons: outline-secondary 59, primary 50, secondary 29, success 5, outline-primary 5, link 2, outline-warning 1, dark 1, danger 1 · `btn-close` 29 (none `btn-close-white`) · modals (`modal fade`) 29, triggers `data-bs-toggle="modal"` 33 · alerts 43 (39 danger, 3 success, 1 info) · `input-group` 50 · `form-check` 48 (12 checkbox, 5 radio inputs) · textarea 23 · tables: `.data-table` 8, Bootstrap `.table` 1, `.table-responsive` 9 · stat tiles 4 (dashboard) · `.action-bar` 18 · `.form-section` many · inline `style=` 16 · views with inline `<script>` 12 · none: dropdown, accordion, toast, offcanvas, tooltip, popover, spinner, progress, switch, range, file upload, date input, pagination, `aria-busy` in markup.

## R4 UI/UX audit findings (46)
Severity: HIGH blocks users or fails an AA criterion; MED degrades quality or risks AA; LOW polish; INFO keep or note. Evidence tags: see Pipeline note. "Fix" names the catalogue task (R18) or UX item (R16).

| ID | Sev | Finding | Evidence | Fix |
|---|---|---|---|---|
| F-01 | MED | Surface steps are almost invisible: ground to panel is 1.09:1 and panel to raised 1.11:1; separation relies on 1px borders (`--line` 1.42:1 on panel) and `box-shadow: none` on cards, stats and sections | [C][S] tokens.css, app-theme.css | FOUND-001, COMP-007 |
| F-02 | MED | Three card idioms with different fills: `.card` (panel), `.stat` and `.form-section` (raised) inside bordered `main` (panel): card-on-card, weak hierarchy | [S] app-theme.css, components.css | COMP-007, SHELL-001 |
| F-03 | MED | Backdrop blur on opaque surfaces does nothing: `.account-login-card` (blur 28px) and `.alert` (blur 16px) have opaque backgrounds over a flat page, yet still cost compositing | [S] app-theme.css | ROUTE-001, COMP-010 |
| F-04 | MED | Overlays (modal, popover, date picker) are opaque `--panel-raised` with one shared shadow; scrim is plain `rgb(0 0 0 / .6)`: no layered depth | [S] | COMP-012, COMP-014 |
| F-05 | LOW | No ambient layer: the canvas is one flat colour, so even the dock's glass samples only page content | [S] | SHELL-001 |
| F-06 | MED | SF Pro Rounded ships only 400 and 700, but CSS asks 500 (`.btn`, `.status-chip`, `.badge`, table headers), 600 (labels) and 650 (date picker); by CSS font matching these collapse to 400/700, so emphasis is inconsistent | [S] plus font-matching rule; [U] | COMP-006 |
| F-07 | LOW | Uppercase with tracking on the Bootstrap `.table` headers (only Products/Details uses `.table`), the login eyebrow, the dock "Navigate to" eyebrow and the date-picker weekday and time labels (5 uppercase rules, 7 letter-spacing declarations); Apple moved section headers to title-style capitalization | [G][W] | Steps 8.4, 9.3, 10.5, 11.3, 12.2 |
| F-08 | MED | Root font-size is 14px below 768px, so rem sizes shrink: `.form-control` min-height 2.65rem is about 37px and `.btn` about 38px (0.55rem x2 + 21px line + 2px border) on phones (above the 24px AA minimum, below the 44px comfort target) | [S] derived from CSS; [U] | Steps 8.3, 8.4 |
| F-09 | LOW | Radii are mixed: tokens `--r-control` .5rem, `--r-panel` .75rem, `--r-overlay` 1rem plus literals (.8, .9, .95, .65, .55, .5rem), 999px and the dock at 1.9rem; nesting is not concentric | [S] | FOUND-001, COMP-007 |
| F-10 | INFO | Positive: colour tokens pass AA. ink 15.0:1, muted 6.0 to 7.2:1, accent text 7.2 to 8.0:1, danger 5.4 to 6.0:1, warn 8.3:1, status chips 5.0 to 6.1:1, `--line-strong` control borders 3.8 to 4.1:1 | [C] | keep values |
| F-11 | LOW | Raw `rgba()` literals bypass tokens and use a blue palette that is not the brand (disabled and readonly fills, fieldset tint, finished-price field) | [S] app-theme.css lines 559-620 | COMP-002 |
| F-12 | HIGH | Select chevron uses Bootstrap's light stroke `#343a40`, 1.62:1 on the page ground (fails 1.4.11, 3:1); `data-bs-theme` is unset and the app never overrides `--bs-form-select-bg-img` | [C][S]; [U] | FOUND-002 (Step 7.2) |
| F-13 | HIGH | `.btn-close` (29 uses) is Bootstrap's black glyph at 50% opacity on `--panel-raised`, no `btn-close-white` and no dark theme: the close X is almost invisible | [G][S]; [U] | FOUND-002 (Step 7.2) |
| F-14 | MED | No visual invalid state: no CSS for `.input-validation-error` or `.is-invalid`; errors show only as `.text-danger` text | [G] | COMP-002, COMP-005 |
| F-15 | LOW | Labels without `for` or `asp-for`: 105 sit in the 11 dead `_ProductCreate*Form` partials (F-46); the 12 live ones are the same read-only "Finished price (Toman)" label, one on each of the 11 Create pages and on Edit (8 written `<label class="form-label">`, 4 written bare `<label>`; the first exact-string grep missed the bare ones) | [G][S] | Step 8.1 |
| F-16 | MED | No `aria-describedby`, `aria-invalid` or `aria-live` anywhere; error text is not linked to its field | [G] | COMP-005 |
| F-17 | MED | 24 of 29 `modal fade` lines have no `aria-labelledby` (line-level grep) | [G] | COMP-012 |
| F-18 | MED | Transactions row-select checkboxes have no accessible name | [S] Transactions/Index.cshtml:132 | ROUTE-009 |
| F-19 | HIGH | `person-picker.js` (read in full) and `product-picker.js` expose `role=combobox/listbox/option` and `aria-expanded` but register no key handling, and the blur handler hides the result list, so pressing Tab removes the results: a keyboard user cannot select anyone or any product (2.1.1). Other `keydown` listeners exist only in site.js, navigation-menu.js and jalali-datepicker.js | [G][S] | Steps 10.3, 10.4 |
| F-20 | LOW | Date picker: the date can be typed into the input (the real keyboard path), Escape closes the panel; its day buttons sit at the end of `<body>` and cannot be reached by keyboard, and the panel stays open when Tab leaves the field | [S] jalali-datepicker.js lines 257-410 | Steps 10.5, 10.6 |
| F-21 | MED | Focus Not Obscured (2.4.11, new in WCAG 2.2): fixed dock (z 1040) and sticky `.action-bar` (18 uses) with no `scroll-padding-*` in any stylesheet | [G][W] | SHELL-004 |
| F-22 | MED | Filters auto-submit on change: `onchange="this.form.submit()"` x11 (Products, People) plus script submit on Transactions; WCAG 3.2.2 risk, and arrow-keying a select can submit repeatedly in some browsers | [G]; [A] browser behaviour | UX-002 |
| F-23 | LOW | Customers and Sellers sortable headers are plain links: no `aria-sort`, no direction cue (Transactions has both) | [S] | ROUTE-004 |
| F-24 | LOW | Customers and Sellers render an empty table with no data; `_EmptyState` unused there | [G] | COMP-011, ROUTE-004 |
| F-25 | LOW | `<html lang="en">` while cells hold Persian text with only `dir="auto"` (no `lang`) | [S] | not planned: it needs a rule for which values are Persian (Q-08 assumes Persian is data only) |
| F-26 | LOW | `viewport-fit=cover` missing while CSS uses `env(safe-area-inset-bottom)` (insets stay 0 on iPhone without it); no `theme-color` | [S][W]; [U] | SHELL-006 |
| F-27 | MED | Products flyout lists 13 links (11 "Add a ..."), and the Products list also has per-type Create buttons: two creation paths, one long flat list | [S] | deferred (UX-001, Q-09) |
| F-28 | LOW | Password-eye SVG duplicated 4 times (Login 1, Profile 3) | [S] | ROUTE-001, ROUTE-002 |
| F-29 | MED | Create forms: 11 pages (CreatePhone 514 lines) with near-identical markup; `_ProductEditForm` is 447 lines; a second copy of the create forms exists as dead partials (F-46) | [S] | not a redesign task; R26 |
| F-30 | LOW | Inline `style=` x16 (11 are JS-toggled `display:none`, 2 are `max-width: 70ch` on Error and Privacy, 1 `max-height`, 1 `max-width: 150px`) and inline `<script>` blocks in 12 views | [G] | Step 14.3 handles the two width styles; the rest stay (state, not visual debt) |
| F-31 | LOW | Two table idioms: `.data-table` x8 and Bootstrap `.table` x1 (Products/Details) | [G] | Step 9.3 |
| F-32 | MED | 6 to 7 column tables scroll horizontally on phones (`.table-responsive`) | [S] | COMP-008, UX-005 |
| F-33 | LOW | Error page always prints "Development Mode" guidance to end users | [S] Error.cshtml | ROUTE-012 |
| F-34 | LOW | Privacy page is the template placeholder sentence | [S] Privacy.cshtml | ROUTE-012 |
| F-35 | LOW | Dashboard heading order: `h2` then `h5.card-title` (levels skipped) | [S] Index.cshtml | ROUTE-003 |
| F-36 | LOW | Fonts total about 1.8 MB (SF 1.0 MB, Vazirmatn 0.8 MB), `swap`, no preload | [S] | QA-004, R17 |
| F-37 | LOW | `jalali-datepicker.js` and `person-picker.js` load on every page; the date picker is also included again by Edit and Sell (harmless, guarded by `dataset.jalaliWired`); page `Scripts` sections render before `site.js` | [S] | Step 16.1 |
| F-38 | LOW | 6 render-blocking stylesheets plus Bootstrap 233 KB min; no bundling step | [S] | QA-004 |
| F-39 | INFO | No CSP header; inline scripts work today; any future CSP needs nonces | [S] | note |
| F-40 | MED | SF Pro Rounded (an Apple typeface) is self-hosted; whether Apple's font licence allows web self-hosting was not verified | [S]; [U] | RES-005, Q-02 |
| F-41 | LOW | `.btn:hover { transform: translateY(-1px) }` lift; the target model is press feedback (scale or brightness), no lift | [S] | MOTION-002 |
| F-42 | INFO | Positive: global reduced-motion rule (0.01 ms), skip link, `--focus-ring`, `aria-sort` on Transactions, labelled sections, `role=alert/status`, `aria-current` and `inert` in the dock | [S] | keep |
| F-43 | INFO | Positive: chart colours are read from CSS tokens (`profit-loss-chart.js`), so token changes reach the chart | [S] | keep |
| F-44 | MED | Person-picker results (`.list-group-item-action`) are outside `.product-picker-results`, so hover and focus use Bootstrap's light `#f8f9fa` in a dark UI (bright flash) | [S]; [U] | FOUND-002 (Step 7.2) |
| F-45 | MED | Scoped template CSS wins by specificity for layout-level elements: the footer gets `position:absolute; bottom:0; width:100%; white-space:nowrap; line-height:60px` (beats app-theme `.footer`), the Privacy link is `#0077cc` (4.01:1 on ground, 3.69:1 on panel) with no hover change | [S] specificity plus [C]; [U] computed style | SHELL-002 |
| F-46 | MED | Dead subsystem: 11 `_ProductCreate*Form` partials (389 lines, 105 unassociated labels, 115 controls without ids), `product-create-modal.js` (197 lines) and `ProductCreateRegistry.cs` (29 lines) are unreachable: no view includes the script, no markup defines `#create-product-modal`, nothing references the registry; `product-picker.js` still listens for its `product-created` event | [G][S] | R26 (owner decision, out of scope) |
## R5 Accessibility audit (target WCAG 2.2 AA)
| SC | Status | Evidence | Task |
|---|---|---|---|
| 1.1.1 Non-text content | Pass | no `<img>`; decorative SVG is `aria-hidden` | none |
| 1.3.1 Info and relationships | Minor fail | F-15 (12 live labels), F-18; heading skip F-35 | Steps 8.1, 12.3 |
| 1.4.1 Use of colour | Pass | chips carry text; links underlined | none |
| 1.4.3 Contrast (minimum) | Pass with exception | tokens pass (F-10); footer link F-45 | SHELL-002 |
| 1.4.11 Non-text contrast | Fail | chevron F-12, close X F-13; control borders and focus ring pass | FOUND-002 |
| 1.4.12 Text spacing | Unknown | not tested | QA-002 |
| 1.4.13 Content on hover or focus | Partial | dock flyout opens on hover, Escape closes it; persistence unverified | QA-002 |
| 2.1.1 Keyboard | Fail (pickers) | F-19 pickers; the date picker has a typed alternative (F-20) | Steps 10.3, 10.4, 10.6 |
| 2.4.1 Bypass blocks | Pass | skip link to `#main` | none |
| 2.4.3 Focus order | Unknown | natural DOM order; modals Bootstrap-managed | QA-002 |
| 2.4.7 Focus visible | Pass | `--focus-ring` on buttons, inputs, links; dock ring | QA-002 |
| 2.4.11 Focus not obscured (min) | At risk | F-21 | SHELL-004 |
| 2.5.8 Target size (min) | Pass (estimate) | 24px minimum met; coarse-pointer comfort F-08 | COMP-001, COMP-002 |
| 3.2.2 On input | At risk | F-22 | UX-002 |
| 3.3.1 Error identification | Partial | text errors and `role=alert` summary; no field state F-14 | COMP-005 |
| 3.3.2 Labels or instructions | Minor fail | F-15 (read-only price label) | Step 8.1 |
| 3.3.7 Redundant entry | N/A | no multi-step flow | none |
| 3.3.8 Accessible authentication | Pass | `autocomplete` on login fields, password toggle, no paste block found | none |
| 4.1.2 Name, role, value | Fail | F-18, F-19; F-15 (12 labels) | Steps 8.1, 10.3, 10.4 |
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
| `prefers-reduced-transparency` | 119 | not shipped | not shipped (flag) | not enough alone: add an in-app solid-surfaces switch (D-09) [W] Chrome blog, caniuse feature page |
| `prefers-contrast: more` | yes | yes | yes | map to solid surfaces and stronger borders [A] |
| `color-mix()`, `linear()` easing | yes | yes | yes | already used by Stage 5/6 with fallbacks [A] |
| Cross-document view transitions (`@view-transition`) | 126+ | 18.2+ | no | optional navigation polish only (SHELL-009) [W] Chrome docs |
| Same-document view transitions | 111+ | 18+ | 144 | not needed (multi-page app) [W] |
| `backdrop-filter: url(#svg)` refraction | yes | declaration parses but renders nothing, no reliable detection | no | rejected as baseline (D-08) [W] community write-ups |
| `backdrop-filter` side effects | creates a containing block for `position: fixed` descendants and a stacking context; stacked blurs are costly | same | same | keep modals and toasts outside glass parents; max 2 blur layers [W] |

Fallback strategy (all required): (1) `@supports not (backdrop-filter: blur(1px)) and not (-webkit-backdrop-filter: blur(1px))` gives solid surfaces; (2) `prefers-reduced-transparency: reduce` gives solid; (3) `prefers-contrast: more` gives solid plus `--ink-muted` borders; (4) `html[data-surfaces="solid"]` from the in-app switch (Stage 15) gives solid on every browser; (5) the page must stay fully legible and coherent with all four.

## R7 Performance and technical constraints
- Payload today [G]: app CSS 43 KB (+ Bootstrap 233 KB min), app JS 75 KB (+ jQuery 88 KB, Bootstrap bundle 81 KB, Chart.js 209 KB on ProfitLoss only), fonts about 1.8 MB (`swap`, no preload).
- Budgets (measure in Step 16.2 against the Stage 7 baseline): LCP <= 2.5 s, INP <= 200 ms, CLS <= 0.1 on a throttled mid-tier mobile profile [A thresholds]; at most 2 concurrent `backdrop-filter` layers in the viewport (dock plus the action bar, or the modal sheet plus the scrim while the dock and action bar are forced solid) and blur radius <= 24px; never animate `backdrop-filter`, `filter` or shadow blur; animate only `transform`, `opacity` and short colour fades; app CSS <= 60 KB uncompressed in exactly 9 files (`tokens.css`, `base.css`, `controls.css`, `surfaces.css`, `overlays.css`, `shell.css`, `login.css`, `motion.css`, `navigation-menu.css`); no new runtime JS library; the two SF Pro Rounded faces are preloaded (Step 11.4) and the unused Vazirmatn Medium file (about 269 KB) is removed (Step 15.3).
- Technical constraints [S]: Bootstrap is plain CSS (no Sass), so tokens live in plain custom properties and Bootstrap is overridden, not recompiled; no bundler (each new CSS file is one more request, acceptable on a local app, each linked with `asp-append-version`); CI cannot see CSS, so every CSS stage ends with an owner manual check; the stage 5/6 dock invariants must keep passing (R24); `backdrop-filter` creates a containing block for fixed children, so overlays are never placed inside a glass parent; page `Scripts` sections render before `site.js`, so any script loaded there that needs a `site.js` helper must wire it on `DOMContentLoaded` (Step 10.4).
- Debt that affects the redesign: 13 `!important` (11 in `app-theme.css`, 2 in the dock; the planned end state is at most 12); 10 `rgb()/rgba()` literals (9 in `app-theme.css`, 1 in the dock; the planned end state is 0 outside `tokens.css`); 15 lines with literal durations in `app-theme.css`; scoped template CSS (F-45); a dead create-product subsystem (R26). Hex literals: 0 in the five app stylesheets (the scoped template file has 9 lines).
- Migration pattern (D-05): no big-bang split and no hand-copying. Each CSS step extracts a spec block from R27 with the `sed` command in "Hard constraints", deletes the replaced rules from the old file by anchor, and links the new file immediately before `navigation-menu.css`. `site.css` is replaced by `base.css` (Step 9.1), `components.css` by `surfaces.css` and `shell.css` (Steps 9.2-11.2), `app-theme.css` by `controls.css`, `overlays.css` and `login.css` (Steps 8.3-12.2); the scoped template file disappears in Step 11.1.
## R8 References
Official Apple guidance is authoritative for principles; community items are inspiration only. "Read" states how much I actually read.
| Reference (URL) | Type, platform | Pattern | Why it works | Translation to MobileShop | Limits and read depth |
|---|---|---|---|---|---|
| WWDC25 219 Meet Liquid Glass https://developer.apple.com/videos/play/wwdc2025/219/ | Official, all Apple platforms | lensing; materialize and morph; adaptive tint and light/dark flip by element size; Regular vs Clear variants, never mixed, Clear only over media with dimming; glass on the navigation layer only; no glass on glass; tint only for primary actions; scroll-edge effect; accessibility modes (reduced transparency frostier, increased contrast black/white with border, reduced motion) | controls stay distinct from content and legible over anything | glass only for dock, flyout, overlay sheets; Regular only (no media in the app, Clear reserved); accent tint only on primary buttons and selected tab; soft scroll-edge fade under the action bar; four accessibility fallbacks (R6) | lensing and adaptive sampling need native compositing; web approximates with blur, saturate, tint. Read: full transcript |
| Adopting Liquid Glass https://developer.apple.com/documentation/technologyoverviews/adopting-liquid-glass | Official, Apple platforms | remove custom bar and sheet backgrounds; avoid overuse; concentric shapes; larger row heights and padding in lists, tables, forms; title-style section headers; sheets inset, more opaque when expanded; controls morph into menus; accessibility labels on icons; group glass for performance; test with accessibility settings | restraint plus consistent geometry | R10 material rules, R11 radii (concentric formula), larger control and row sizes, no all-caps headers, solid when a modal is large, one blur layer shared by a group | SwiftUI/UIKit APIs not applicable. Read: full page (Markdown version) |
| HIG Materials https://developer.apple.com/design/human-interface-guidelines/materials | Official, Apple platforms | two kinds of material: Liquid Glass for controls and navigation, standard materials (blur, vibrancy) for the content layer; do not put Liquid Glass in the content layer | clear layer model | content layer = opaque tokens (a subtle blur only if ever needed); glass only above it | The page needs JavaScript; the direct fetch returned no text, so this row rests on a third-party text mirror (glama.ai, a copy of the HIG). Re-read the official page in a browser before Stage 9. Read: mirror only |
| WWDC25 102 Platforms State of the Union https://developer.apple.com/videos/play/wwdc2025/102/ | Official | hierarchy, harmony, consistency; concentricity; larger, bolder, left-aligned type; softer shapes, taller controls; controls recede on scroll; content in glass monochrome by default | a single design vocabulary across surfaces | H1 larger and bolder, taller controls, concentric radii, dock already shrinks when idle (Stage 6) | native components only. Read: transcript (token-capped, design sections read) |
| WCAG 2.2 What's New https://w3.org/WAI/standards-guidelines/wcag/new-in-22/ | Standard | new criteria: AA 2.4.11 Focus Not Obscured (Minimum), 2.5.7 Dragging Movements, 2.5.8 Target Size (Minimum), 3.3.8 Accessible Authentication (Minimum); Level A 3.2.6 Consistent Help, 3.3.7 Redundant Entry | measurable acceptance | R5 mapping; Step 11.2 targets 2.4.11 for the dock and action bar | Read: page fetched for the criteria levels |
| prefers-reduced-transparency https://developer.chrome.com/blog/css-prefers-reduced-transparency and https://caniuse.com/wf-prefers-reduced-transparency | Standards and browser vendor | media query for solid surfaces | respects OS setting | used by the dock already; extended to all materials | Chrome/Edge only; Safari and Firefox not shipped, hence the in-app switch. Read: search-result level |
| View Transitions https://developer.chrome.com/docs/web-platform/view-transitions and .../cross-document | Browser vendor | `@view-transition { navigation: auto }` for multi-page navigation | smooth page change with tiny CSS | optional SHELL-009; dock gets a `view-transition-name` so it stays still | cross-document: Chrome 126+, Safari 18.2+, no Firefox. Read: search-result level |
| Liquid Glass on the web: 6 ways https://dev.to/devyatov/liquid-glass-on-the-web-6-ways-to-build-it-with-css-and-svg-3m07 | Community, web | blur-saturate-tint baseline; SVG displacement refraction via `backdrop-filter: url()`; pitfalls (containing block, stacked cost, Safari renders nothing and detection lies) | honest comparison of approximations | baseline only; refraction rejected (D-08) | Read: search-result level |
| rizroze/liquid-glass https://github.com/rizroze/liquid-glass; Z1Code/glass-refraction https://github.com/Z1Code/glass-refraction; deepika-builds/liquid-glass wiki https://github.com/deepika-builds/liquid-glass/wiki | Community, web | displacement-map refraction, specular edges | convincing lensing in Chromium | only the idea of a 1px edge highlight plus glass border is adopted (`--edge-highlight`, `--border-glass`) | Chromium-only effect, cost; repositories not inspected beyond search results |
| Dribbble collections https://dribbble.com/ryanontheroad/collections/7635955-Glass-liquid, https://dribbble.com/MaxMonastyrev/collections/7537867-Glass, https://dribbble.com/iamefe/collections/7563096-Liquid-glass | Community concepts | floating tab bars, segmented controls, glass sidebars, toggles | visual inspiration | segmented control for Products tabs and report ranges (R15) | concept art, no performance or contrast constraints; collection pages only, no individual shot analysed |
| Create with Swift, Exploring a new visual language: Liquid Glass https://www.createwithswift.com/exploring-a-new-visual-language-liquid-glass/ | Community explainer | summary of the visual language | quick orientation | none beyond Apple sources | search-result level |
| Deque axe-core NuGet https://www.github.com/dequelabs/axe-core-nuget; Deque.AxeCore.Playwright 4.9.0 https://nuget.org/packages/Deque.AxeCore.Playwright/4.9.0 | Tooling | automated a11y checks inside Playwright for .NET | repeatable WCAG scan | optional QA-002 | adds a test-only package (R17). Read: package pages in search results |

Not reached: Awwwards (the search returned no usable pages). Nothing here claims native Apple rendering can be reproduced in a browser.

## R9 Design principles
1. Quiet content, glass chrome: glass is the navigation and overlay layer only (dock, flyout, modal sheets, the date-picker panel, the action bar, the login card). Dense content (tables, forms, cards, in-flow picker lists) stays opaque and low-noise; the login card is the one deliberate exception (two fields over its own gradient).
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
| G3 Overlay glass | modal sheets (small), the date-picker panel, the login card | large or form-heavy modals, in-flow picker lists | `--glass-overlay-bg` 92% tint, `--glass-overlay-filter` blur 24, saturate 1.4, brightness .85 | `--border-glass`, `--shadow-4` (modal) or `--shadow-3`, `--edge-highlight` | muted 4.95:1 white strip, 5.38:1 accent strip (computed) | solid |
| G3-solid | scrollable dialogs (`.modal-dialog-scrollable`) and anything marked `.modal-content--solid` (the person-picker modal) | | `--glass-solid-bg` | same border and shadow as G3 | ink 15:1 | n/a (already solid); mirrors Apple's "sheets turn more opaque when expanded" |
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
  --success: var(--accent);
  --info: var(--accent);

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
| Caption `--fs-xs` | 0.75rem | 400 | `--lh-snug` | 0 | footnotes only; status chips keep `--fs-sm` bold because `--fs-xs` is 10.5px at the 14px root |
Rules: root is 14px below 768px and 16px from 768 (kept); maximum prose line `--size-measure` (70ch); single-column forms at most 40rem; headings left-aligned; numerals use `.num` (tabular); Persian runs follow the stack automatically, give Persian-heavy blocks `line-height: 1.7`.

### R11.D Spacing, radius, elevation, borders, sizing, icons, breakpoints, motion
- Spacing: `--space-1..12` (4 to 48px). Defaults: icon gap 1, stack tight 2, field row gap 3, card padding 5, section gap 6, page section 8, hero/empty-state 10 to 12. Table cell padding .75rem 1rem, row min-height 3rem (Apple asks for roomier lists, tables and forms). Grid: Bootstrap 12 columns with `g-3` gutters kept.
- Radius roles: xs chips inside tables, row hover; sm date-picker day cells, small buttons, menu items; md buttons, fields, alerts, segmented items; lg cards, stat tiles, form sections, empty-state well, floating picker lists; xl main panel (if kept), modal sheet, login card, date-picker panel, flyout panel; 2xl dock; pill chips, badges, tabs, segmented track, switch. Concentric examples: modal xl (24) with 8px inset holds cards at lg (16); card lg (16) with 4px inset holds pill chips; dock 2xl (32) with 6px inset holds tab items at calc(2rem - 6px).
- Elevation: 0 canvas and table rows; 1 `--shadow-1` plus edge highlight on M2 cards; 2 `--shadow-2` on menus, floating lists, action bar, tooltips; 3 `--shadow-3` on dock, flyout, toasts; 4 `--shadow-4` on modal sheets. On a dark canvas shadows read weakly, so the edge highlight and `--border-glass` carry separation.
- Borders: hairline 1px for separators (never the only state cue); control border 1px `--border-control`; glass border 1px `--border-glass` plus inset top highlight; error border `--danger` with `aria-invalid`; selected uses accent ring or fill plus `aria-selected/current/pressed`.
- Sizing: button md `--size-control` (44px at 16px root, 38.5px at the 14px root, so `@media (pointer: coarse) { min-height: var(--size-hit-min) }`), sm `--size-control-sm`, lg `--size-control-lg`; icon-only buttons square with an accessible name; fields as button md, textarea min 6rem; checkbox and radio box 1.25rem inside a 44px coarse row; segmented control `--size-control-sm`; chips 1.5rem; card padding `--space-5`; modal radius xl, max-height `calc(100dvh - 2rem)`; dock sizes stay as in Stage 5/6.
- Icons: inline SVG, 24x24 viewBox, 1.75 stroke, round caps and joins, `currentColor`, sizes 16/20/24 (`--size-icon-*`), decorative icons `aria-hidden="true" focusable="false"`, icon-only controls get `aria-label`. The four duplicated password-eye SVGs become one `_PasswordToggle` partial (Step 12.1); there is no sprite partial, icon font or icon library, and no SF Symbols (Apple asset). Lucide (ISC) path data may be copied inline for a future icon without a dependency.
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
- Composition: (1) a glass element never contains another glass element; (2) overlays are direct children of `body` or Bootstrap's modal root, never inside a glass parent (containing-block gotcha); (3) content surfaces never use `backdrop-filter`; (4) at most three nested rounded levels (panel, card, control) with concentric radii; (5) elevation only from `--shadow-*`; (6) colour only from role tokens (no hex anywhere outside `tokens.css`; `rgb()` only inside `tokens.css`); (7) z-index only from `--z-*`; (8) state classes `is-*`, hooks `data-*`, ARIA mirrors state; (9) no `outline: none` without a ring; (10) user data that can be Persian sits in `dir="auto"` containers.
- Naming: keep the existing BEM-like blocks (`page-head__title`, `form-section__title`, `navigation-*`, `account-login-*`); new classes introduced by the steps: `segmented`, `measure`, `modal-content--solid`, `is-active` (picker option). Tokens follow `--{role}-{variant}`. Legacy tokens stay as primitives that the semantic tokens alias.
- Reuse (D-11): one new partial, `_PasswordToggle` (four identical copies today). No `_FormField`, `_Modal`, `_Icons`, `_PageHead` or `_StatTile`: labels are fixed in place (12 live labels), modals get two attributes each, and AGENTS.md "minimal change" forbids speculative abstractions.
- Migration: spec blocks plus anchor deletion, as in R7. Legacy Bootstrap class names stay as aliases (`btn-success`, `btn-dark`, `btn-outline-warning`, `card`, `stat`, `form-section` all render through the new families), so Razor edits stay minimal.
- Review gate for every CSS step: no hex literal outside `tokens.css`, no new `!important` (end state at most 12), no literal duration or easing outside `tokens.css` and the dock (the reduced-motion `0.01ms` in `base.css` excepted), `backdrop-filter` only in `overlays.css`, `shell.css`, `login.css`, `motion.css`, the dock and `tokens.css`, no new z-index number, brace balance equal, the dock invariants of R24 still pass.
## R14 Route-by-route step map
Every route is covered by at least one stage; "inherits" means no markup step is needed because shared classes and behaviour from Stages 8-11 apply. Verification for each route is the owner manual check in its stage's validation step (Steps 8.5, 9.7, 10.7, 11.5, 12.5, 13.2, 14.4, 15.5) and 16.2.
| Route (files) | Steps | What changes |
|---|---|---|
| Layout, footer, shell (`_Layout.cshtml`) | 7.2, 9.1, 11.1, 11.2, 11.4, 15.1, 15.4 | dark flag and meta; base.css; template stylesheet removed (footer in flow, accent link); shell.css (action bar glass, focus clearance); viewport-fit, theme colour, preload; motion.css; solid-surfaces script |
| Account/Login | 8.3, 8.4, 12.1, 12.2, 15.1 | buttons and fields; `_PasswordToggle`; `login.css` glass card over its own gradient; preference modes |
| Account/Logout | inherits | buttons |
| Account/Profile | 8.3, 8.4, 12.1, 15.4 | buttons and fields; `_PasswordToggle` x3; Appearance checkbox (outside the password form) |
| Dashboard (Index) | 9.2, 9.4, 12.3 | surface family; chips; heading order |
| People/Index | 12.4 | segmented filter |
| People/Customers, Sellers | 9.3, 12.4 | table idiom; `aria-sort` and indicator; empty state |
| People/CustomerDetails, SellerDetails, CreateCustomer, CreateSeller | inherits | surfaces, tables, fields, buttons |
| Products/Index | 9.3, 9.4, 13.1 | table, chips; type buttons in a labelled, wrapping nav |
| Products/SecondHand | 9.3, 9.4 | table, chips |
| Products/Details | 9.3, 13.2 | `.table` becomes `.data-table`; Apple ID password stays plain `code` |
| Products/Edit (`_ProductEditForm`) | 8.1, 8.4, 11.2, 13.2 | finished-price label; fields; action bar |
| Products/Create x11 | 8.1, 8.4, 10.1, 10.2, 11.2, 13.2 | finished-price label; fields; glass modals with `aria-labelledby` on 24; action bar |
| Products/PersonPicker | none | handler only |
| Transactions/Index | 8.1, 9.3, 14.1 | row checkbox name; table; result count |
| Transactions/Sell | 10.3-10.6, 11.2 | pickers and date picker keyboard model; action bar |
| Transactions/Details | 9.2, 9.3 | surfaces, tables |
| Reports/ProfitLoss | 9.2, 9.5, 14.2 | cards; tabs and segmented mode; chart description |
| Error, Privacy | 14.3 | copy and width class |
| Dock (`NavigationMenu`) | 11.3, 11.4, 15.1 | token swap; safe area; preference modes |
## R15 Component-family specifications
All families use R11 tokens and R12 states. "Steps" lists where each is implemented.
| Family | Appearance | Interaction and states | Accessibility | Responsive | Decision (steps) |
|---|---|---|---|---|---|
| Dock and flyout | G1 regular glass, accent-soft selected tab | Stage 5/6 behaviour unchanged; solid while a modal is open | `nav` label, `aria-current`, `inert` closed flyout (kept); focus clearance | compact on touch, safe-area offset | RETAIN behaviour, tokens swapped (11.3, 11.4) |
| Products type buttons | link buttons: filled for the active type, outline otherwise | `aria-current="page"` kept | labelled `nav` landmark | wraps | REFACTOR markup (13.1) |
| Buttons | primary accent fill, secondary raised neutral, outline transparent, danger; radius md; regular weight | hover mix, press scale .98, focus ring | 44px minimum on coarse pointers, names kept | full width in narrow action bars | CONSOLIDATE 9 variants to 4 via aliases (8.3) |
| Links | accent, underlined in text, hover to text colour | focus ring | contrast 7:1+ | n/a | RETAIN (9.1; footer fixed in 11.1) |
| Text fields, selects, textarea | inset surface, `--border-control`, radius md, label above in muted regular | focus ring, red border for `.input-validation-error` | `aria-invalid` and `aria-describedby` through `site.js`, polite message spans | one column at xs | REFACTOR (8.2, 8.4) |
| Combobox pickers | in-flow opaque list below the input | ArrowUp/Down, Enter, Escape, `aria-activedescendant`, live count | combobox pattern | full width | REFACTOR (10.3, 10.4) |
| Checkbox, radio | 1.25rem box, accent when checked | label click | 44px label rows on coarse pointers; row checkbox named | n/a | REFACTOR (8.1, 8.4) |
| Segmented control | pill track, selected pill accent-soft and bold | radios with `btn-check`, arrow keys native | group label via `fieldset` and `legend` | wraps | INTRODUCE (9.5, 12.4) |
| Tabs (ProfitLoss) | Bootstrap tabs restyled with tokens | Bootstrap tab JS | tablist roles kept | n/a | RESTYLE (9.5) |
| Date picker (Jalali) | G3 glass panel, radius xl, day cells sm | arrows, Home/End, PageUp/PageDown, Escape returns focus | typed entry remains the primary path; live month title | fixed panel | REFACTOR (10.5, 10.6) |
| Cards, stat tiles, form sections, chart cards | one M2 family: raised fill, hairline, radius lg, `--shadow-1` | static | headings per section | grid reflows | CONSOLIDATE (9.2) |
| Tables | M1, bold muted header, rows about 3rem, hairline separators, hover fill | sort links with `aria-sort` and arrow | `scope=col` | horizontal scroll in `.table-responsive` | CONSOLIDATE (9.3, 12.4) |
| Chips | pill, tinted fill plus text, bold | static | text carries status | n/a | REFACTOR (9.4) |
| Alerts | raised fill, severity border, no blur | static | `role=alert` or `status` kept | full width | REFACTOR (9.4) |
| Modals | G3 glass sheet (solid when scrollable or marked), scrim blur, spring entrance | Bootstrap focus handling | `aria-labelledby` on all 29, close X visible | `100dvh` limits | REFACTOR (10.1, 10.2, 15.1) |
| Action bar | G2 subtle glass, hairline top edge | solid while a modal is open | clear of focus via scroll padding | above the dock on small screens | REFACTOR (11.2) |
| Empty and loading states | `_EmptyState` with title and hint; picker `.loading-line` | n/a | text only | n/a | RETAIN, used on Customers and Sellers (12.4) |
| Charts | Chart.js with token colours | Chart.js tooltips | text description of the trend chart | canvas resizes | RETAIN (14.2) |
Not introduced (no use in the app): breadcrumbs, sidebars, tab bar, command palette, sliders, file upload, tooltips, popovers, dropdowns, drawers, toasts, pagination, avatars, images.
## R16 Functional UX recommendations (tracked and tested separately from visual work)
Type tags in steps: V visual, A accessibility or markup behaviour, U UX or functional, T tooling or test, D documentation. Nothing changes behaviour without a step; the three flows that matter most are signing in, recording a sale and creating a product.
| ID | Change | Status |
|---|---|---|
| UX-001 | Group the 11 "Add a ..." links in the Products flyout under one "Create" entry (F-27) | deferred until the owner answers Q-09; no step |
| UX-002 | Filters: auto-apply stays; Transactions gets a visible result count (Step 14.1); an explicit Apply button only if the owner answers Q-05 that way | default applied (14.1) |
| UX-003 | Polite live result count in the two pickers | in Steps 10.3 and 10.4 |
| UX-004 | Optional toast after redirects | deferred, no step |
| UX-005 | Phone card layout for tables | deferred, no step |
| UX-006 | Solid-surfaces switch on the Profile page | Step 15.4 |
| UX-007 | Error page: production-safe wording | Step 14.3 |
Information architecture is already small (six dock tabs, one-page sell flow) and is kept.
## R17 Dependency evaluation
Rule: native CSS first. No library is added to make a glass card. Fields per candidate: purpose, why existing is insufficient, alternatives, compatibility, size and performance, accessibility and maintenance, verdict, migration risk, necessity.
| Candidate | Purpose and why existing is insufficient | Alternatives | Compatibility, size, performance | A11y and maintenance | Verdict and risk | Necessity |
|---|---|---|---|---|---|---|
| Any new runtime library | none needed; CSS custom properties, `linear()` easing and a 273-line dock script already do what the redesign needs | n/a | n/a | n/a | ADD NOTHING; risk none | n/a |
| React, shadcn/ui, Radix UI, React Aria | accessible components; the app is server-rendered Razor Pages with Bootstrap JS, no SPA toolchain | Bootstrap modal, hand-written ARIA combobox (about 150 lines each for the two pickers) | needs Node build and a client framework; hundreds of KB; not compatible with MPA pages | strong a11y, but framework replacement | AVOID; migration risk HIGH | not applicable |
| Motion for React, GSAP | spring and layout animation; CSS `linear()` springs and transitions already do this (dock proves it) | CSS, Web Animations API | 25 to 70 KB each, React variant impossible here | extra reduced-motion handling | AVOID; risk MED | optional never |
| Lucide or Phosphor icons | consistent icon set; today 17 hand-made inline symbols in the dock plus one eye icon | copy individual SVG paths inline; no icon font | zero runtime cost when copied; ISC (Lucide) and MIT (Phosphor) licences, confirm at adoption | decorative icons aria-hidden | OPTIONAL, copy-paste only; risk LOW | optional |
| Bootstrap 5.3.3 | grid, modal JS, validation classes, dark mode attribute | none worth the churn | already loaded (233 KB css, 81 KB js) | modal focus trap is built in | RETAIN; no version bump in this redesign; risk LOW | mandatory (existing) |
| jQuery and jquery-validation(-unobtrusive) | ASP.NET unobtrusive validation depends on them; `$.validator.setDefaults` is the hook for `aria-invalid` | native Constraint Validation | already loaded | maintained by the platform template | RETAIN; risk LOW | mandatory (existing) |
| Chart.js 4.5.1 | profit/loss trend chart | none | already loaded on one page | canvas needs a table alternative | RETAIN; risk LOW | mandatory (existing) |
| Microsoft.Playwright (.NET), test-only | screenshots, visual comparison, keyboard flows in Chromium, WebKit and Firefox; today there are no UI tests and the sandbox cannot run the app | manual screenshots, Selenium, Cypress (needs Node) | dev-only NuGet; CI would need browser install (extra minutes) and the app started against SQLite | not shipped to users; actively maintained | RECOMMENDED for Stage 16; risk LOW to MED (CI time, flaky pixels); needs approval Q-07 | optional |
| Deque.AxeCore.Playwright 4.9.0, test-only | automated WCAG scan per route | Accessibility Insights (manual), axe browser extension | dev-only; pairs with Playwright | open source engine, confirm licence at adoption | RECOMMENDED with Playwright; risk LOW | optional |
| Stylelint, PostCSS, Sass, Tailwind | linting and authoring | grep gates in R13 | adds a Node toolchain to a .NET-only repo; Tailwind would fight Bootstrap; Sass would need Bootstrap source compilation | n/a | AVOID; risk MED | not needed |
| Open rounded font (for example Nunito, OFL) | replacement if SF Pro Rounded cannot be self-hosted (Q-02) | `ui-rounded` plus system stack | font files comparable in size; adds 500 and 600 weights possibly | licence clarity | CONDITIONAL on Q-02; risk MED (metrics shift) | optional |
| Lighthouse CI or WebPageTest | repeatable performance baseline | Playwright tracing, manual Lighthouse | Node or hosted service | n/a | OPTIONAL; risk LOW | optional |
| View-transition polyfills | cross-browser page transitions | none (native only) | script weight | n/a | AVOID | not needed |

## R18 Phases and step index
### R18.A Phases and dependencies
| Stage | Mission phase | Depends on | Delivers |
|---|---|---|---|
| (done in this pass) | 0 Discovery, 1 Research and architecture | none | R1 to R17, R19 to R27 |
| 7 | 2 Foundation | none | tokens, dark-mode flag, dead-CSS cleanup |
| 8 | 3 Shared components (controls) | 7 | label naming, validation linking, buttons, fields, checks |
| 9 | 3 Shared components (surfaces) | 7, 8 | base.css, surface family, tables, chips and alerts, segmented control, canvas (gated) |
| 10 | 3 Shared components (overlays) | 8, 9 | glass modals, modal labels, pickers keyboard model, date picker |
| 11 | 4 Application shell | 9, 10 | template CSS removed, shell.css, dock tokens, mobile shell |
| 12, 13, 14 | 5 Page-by-page | 8 to 11 | all routes in three batches |
| 15 | 6 Motion and polish | 10 to 14 | preference modes, sheet entrance, Medium font retired, solid-surfaces switch |
| 16 | 7 Quality assurance | all | clean-up gates, owner evidence pack, README section, sign-off |
### R18.B Task-ID map (labels used in R4, R5 and in the steps' Task lines)
DISC-001..004, RES-001..004: done in this planning pass (R3 to R8) · DISC-005: owner precondition of Step 7.2 and Step 16.2 · RES-005: owner decision Q-02 · FOUND-001..004: Steps 7.1-7.4 · COMP-001: 8.3 · COMP-002, COMP-003: 8.4 · COMP-004 (a to c): 8.1; 004d: dropped (dead code, R26) · COMP-005: 8.2 · COMP-006: folded into 8.3, 8.4, 9.1, 9.3, 10.5, 12.2 and 15.3 · COMP-007: 9.2 · COMP-008: 9.3 · COMP-009: 9.4, 9.5 · COMP-010: 9.4 · COMP-011: 12.4 · COMP-012: 10.1, 10.2 · COMP-013: 10.3, 10.4 · COMP-014: 10.5, 10.6 · COMP-015: 10.1 · SHELL-001: 9.6 (gated) · SHELL-002: 11.1 · SHELL-003: 11.3 · SHELL-004, SHELL-005: 11.2 · SHELL-006: 11.4 · SHELL-007, SHELL-009: deferred (Q-09 and optional view transitions) · SHELL-008: dropped (the skip link already exists) · ROUTE-001, ROUTE-002: 12.1, 12.2 · ROUTE-003: 12.3 · ROUTE-004: 12.4 · ROUTE-005, ROUTE-010: inherit, no step · ROUTE-006: 13.1 · ROUTE-007, ROUTE-008: 13.2 verification (008c dropped, dead code) · ROUTE-009: 8.1, 14.1 · ROUTE-011: 9.5, 14.2 · ROUTE-012: 14.3 · MOTION-001: folded into the family blocks and the gates of 16.1 · MOTION-002: 8.3, 15.1 · MOTION-003, MOTION-004: 15.1 · MOTION-005: 15.4 · MOTION-006: deferred (double-submit guard is a behaviour change, no step) · QA-001..005: 16.2 · QA-006: 16.1 · QA-007: 16.3.
### R18.C Step index
- Step 7.1 — Add the design-token layer (additive and inert)
- Step 7.2 — Bootstrap dark mode flag and colour-scheme meta
- Step 7.3 — Remove dead rules from site.css
- Step 7.4 — Stage 7 validation
- Step 8.1 — Name the live unlabeled controls (12 labels, 1 checkbox)
- Step 8.2 — Link validation state and messages for assistive tech (`site.js`)
- Step 8.3 — Buttons: create `controls.css`
- Step 8.4 — Fields, checks and focus: append to `controls.css`
- Step 8.5 — Stage 8 validation
- Step 9.1 — `base.css`: merge `site.css` and the base rules, set the type scale
- Step 9.2 — Surface family: cards, stat tiles, form sections, chart cards, page header, empty and loading states
- Step 9.3 — One table idiom
- Step 9.4 — Chips and alerts
- Step 9.5 — Tabs and segmented control (ProfitLoss)
- Step 9.6 — Canvas shell and ambient layer (gated on owner decision Q-04)
- Step 9.7 — Stage 9 validation
- Step 10.1 — Modal sheets and scrim: create `overlays.css`
- Step 10.2 — Label every modal (`aria-labelledby` on 24 modals)
- Step 10.3 — Combobox keyboard model: helper in `site.js`, wired into the person picker
- Step 10.4 — Product picker uses the same keyboard model
- Step 10.5 — Jalali date picker: glass panel and token styling
- Step 10.6 — Jalali date picker: keyboard reach, focus return, close on leave
- Step 10.7 — Stage 10 validation
- Step 11.1 — Remove the scoped template stylesheet (footer and link override, F-45)
- Step 11.2 — `shell.css`: action bar, skip link, clearance; retire `components.css`
- Step 11.3 — Dock token migration (value-identical)
- Step 11.4 — Mobile shell: safe areas, theme colour, dynamic viewport height, font preload
- Step 11.5 — Stage 11 validation
- Step 12.1 — `_PasswordToggle` partial (4 duplicated eye buttons)
- Step 12.2 — `login.css` and retirement of `app-theme.css`
- Step 12.3 — Dashboard heading order
- Step 12.4 — People: segmented filter, sortable headers with state, empty state
- Step 12.5 — Stage 12 validation
- Step 13.1 — Products list: type buttons become a labelled navigation that wraps
- Step 13.2 — Stage 13 validation (includes the owner visual pass)
- Step 14.1 — Transactions: visible result count
- Step 14.2 — ProfitLoss: describe the trend chart
- Step 14.3 — Error page copy and the `measure` class
- Step 14.4 — Stage 14 validation
- Step 15.1 — `motion.css`: preference modes and sheet entrance
- Step 15.2 — Repair the Vazirmatn font assets (the tracked `.woff2` files are HTML pages, not fonts)
- Step 15.3 — Retire the unused Vazirmatn Medium face
- Step 15.4 — Solid-surfaces switch (Profile page, remembered per browser)
- Step 15.5 — Stage 15 validation
- Step 16.1 — Final consistency audit and clean-up
- Step 16.2 — Owner verification pack (browser evidence)
- Step 16.3 — README: short UI design-system section
- Step 16.4 — Redesign sign-off
## R19 Baseline and verification strategy
- Build, run, test (from README and project.md): `dotnet build src/MobileShop.slnx`, `dotnet run --project src/MobileShop.Web` (http://localhost:5043, SQLite, development seeding, documented default admin account in README). CI (`.github/workflows/dotnet.yml`) is the only gate; agents do not run build or test locally unless asked. CI ignores `*.md`, `.agents/**`, `.kilo/**` and `docs/**`.
- Known baseline [CI]: last code commit `597fba4` (Stage 6) run #618 success; #617 and #616 success; the later commits are documentation only. No pre-existing failure observed. No UI, markup or JS tests exist, so the CSS and JS steps are verified by greps, brace balance, `node --check` and the owner's manual checks.
- Not captured in the planning pass (blocked: the sandbox has no .NET SDK, no browser and no NuGet access): screenshots, interaction states, performance numbers. The owner captures the baseline BEFORE Step 7.2 (the first step with a visible effect): all 33 routes (PersonPicker has no UI) at 360x740, 768x1024, 1440x900 in dark Chromium; key states: login error, Customers and Sellers with no rows, validation errors on Sell and one Create page, an open modal, an open picker list, the date picker, the dock expanded with a flyout, ProfitLoss with data, Error page. Store outside the repo as `route-viewport-state.png`.
- Functional regression list (owner, every visual stage): sign in and out; dock navigation to all tabs; create a phone and one accessory by page; record a sale with both pickers and the date picker; Transactions filter, sort, select; People filter and sort; Profile page opens (do not change the real password); ProfitLoss range change; invoice or factor download still works.
- Automated option (needs Q-07): a Playwright for .NET test project with screenshots, an axe scan per route and keyboard flows, first as a non-blocking CI job; mask dates and ids; start with a pixel-ratio threshold of 0.2%.
- Contrast check for glass (Step 16.2, outside the repo), run with Python 3: `python3 - <<'EOF'` then
  `def lin(c): c/=255; return c/12.92 if c<=0.03928 else ((c+0.055)/1.055)**2.4`
  `def L(h): r,g,b=[int(h[i:i+2],16) for i in (1,3,5)]; return 0.2126*lin(r)+0.7152*lin(g)+0.0722*lin(b)`
  `def cr(a,b): x,y=sorted((L(a),L(b)),reverse=True); return (x+0.05)/(y+0.05)`
  `def over(tint,strip,pct,bright): t=[int(tint[i:i+2],16) for i in (1,3,5)]; s=[int(strip[i:i+2],16)*bright for i in (1,3,5)]; return '#%02X%02X%02X'%tuple(round(t[i]*pct+s[i]*(1-pct)) for i in range(3))`
  `for pct,br in ((.78,.60),(.88,.80),(.92,.85)):` `for strip in ('#FFFFFF','#36C5B0'): print(pct,strip,round(cr('#93A3B3',over('#1C252E',strip,pct,br)),2))` then `EOF`. Expected from the planning pass: 4.13 and 4.89 (G1), 4.54 and 5.12 (G2), 4.95 and 5.38 (G3).
- Performance baseline before Stage 8, after Stage 11 and after Stage 16: Lighthouse mobile on Login, Dashboard, Products/Index, Transactions/Sell (LCP, CLS, INP or TBT, transferred bytes, request count); one mid-tier Android scroll and animation check.
- Browser and OS assumptions: Chrome and Edge latest on Windows and macOS, Safari latest on macOS and iOS, Firefox latest, Chrome on a mid-tier Android phone.
- Failure handling: record pre-existing failures separately in act.md ("Pre-existing") so they are never counted as redesign regressions.
- Definition of done for the whole redesign: every route row in R14 covered; every R15 family implemented with all R12 states; the gates of Step 16.1 pass; responsive checks done at the baseline viewports; axe 0 critical and 0 serious, keyboard-only completes sign-in, sale and product creation; regression list passes; build, tests and CI green; performance within R7 budgets or recorded exceptions; fallbacks verified (no `backdrop-filter`, reduced transparency, increased contrast, forced colors, reduced motion, the Profile switch); this file's status lines updated after each milestone.
## R20 Decision log
| ID | Decision | Rationale | Status |
|---|---|---|---|
| D-01 | This file holds all ten stages plus the Reference | the owner asked for the whole redesign planned at once in plan.md; `planner.md` says "active stage only" | accepted by the owner's request; the Reviewer should note the size (Actor reads steps by slice, see Plan map) |
| D-02 | Stay on Razor Pages, Bootstrap 5.3.3, vanilla CSS and JS | framework change is out of scope and unjustified (R17) | decided |
| D-03 | Dark-first, brand palette preserved, light theme optional | brand identity; Q-03 | decided, light open |
| D-04 | Glass only on the navigation and overlay layer; Regular only; Clear reserved | Apple guidance (WWDC25 219, HIG); no media in the app | proposed |
| D-05 | CSS migration by validated spec blocks (R27) extracted with `sed`, anchor deletion of the old rules, `app-theme.css`, `components.css`, `site.css` retired by Step 12.2 | no hand copying, mechanical checks, every step leaves the app working | proposed |
| D-06 | `data-bs-theme="dark"` as the base for unthemed Bootstrap defaults | fixes F-12, F-13, F-44 in one line | proposed (Step 7.2) |
| D-07 | Keep SF Pro Rounded until the licence question is answered | F-40, Q-02 | open |
| D-08 | Rejected as baseline: SVG-displacement refraction, pointer-tracking lighting, WebGL, canvas, shaders | Chromium-only or costly, little value for a dense admin UI | proposed |
| D-09 | In-app solid-surfaces switch, and every preference mode implemented by overriding the glass tokens in one place | `prefers-reduced-transparency` is Chromium-only (web-features, Oct 2026) | proposed (Steps 15.1, 15.4) |
| D-10 | Cross-document view transitions deferred | Chromium and Safari only; must not disturb dock invariants | deferred |
| D-11 | One new partial only: `_PasswordToggle`; no `_FormField`, `_Modal`, `_Icons`, `_PageHead`, `_StatTile` | AGENTS.md minimal change; labels fixed in place | proposed |
| D-12 | New semantic token prefixes beside legacy primitives; legacy `--r-*` and unused `--app-*` aliases removed in Step 16.1 | additive foundation, no mass rename | proposed |
| D-13 | Scoped template CSS removed in Step 11.1 with an owner check of the footer | it changes footer layout (F-45) | proposed |
| D-14 | CI stays the only gate; browser harness and test packages optional | project rules; Q-07 | decided, packages open |
| D-15 | UI stays English LTR; no RTL mirror | Q-08 | assumption |
| D-16 | Stage 8 order: accessibility markup (8.1, 8.2) before the CSS families | the markup fixes are independent of the visuals; the stage title in TODO.md is fixed | proposed |
| D-17 | The dead create-product subsystem is neither restyled nor deleted in this plan | out of redesign scope; proposal in R26 | proposed |
| D-18 | Gated steps (9.6) default to skipped until the owner answers; deferred items have no step | no half-specified work | proposed |
## R21 Risks and trade-offs
| ID | Risk | Mitigation |
|---|---|---|
| RK-01 | Safari and Firefox parity (no reduced-transparency media query) | D-09 switch; every effect has a solid fallback; WebKit and Gecko checks in QA-005 |
| RK-02 | Backdrop blur cost on low-end phones | max 2 layers, no animated filters, QA-004 on a mid-tier phone |
| RK-03 | CI cannot see CSS, so visual regressions slip through | owner manual check list per stage, baseline capture before Stage 8, optional Playwright |
| RK-04 | Overriding plain-CSS Bootstrap relies on cascade order | new files link after Bootstrap, no `!important`, R13 gate |
| RK-05 | Dock regressions during token migration | R24 invariants as acceptance in SHELL-003; Stage 6 was measured in Chromium only |
| RK-06 | `backdrop-filter` makes a containing block for fixed children | overlays stay outside glass parents (R13 rule 2) |
| RK-07 | Live label work is small (12 labels, 1 checkbox) because the other 105 labels are in dead code | Step 8.1 only; R26 keeps the dead code out of scope |
| RK-08 | Replacing the font shifts metrics | decide Q-02 before Stage 8; recheck line heights |
| RK-09 | UX changes smuggled in with visuals | separate `UX-*` items, approvals, separate commits |
| RK-10 | Persian text and the new type scale | test with real Persian data; line-height 1.7 rule |
| RK-11 | Long plan burdens the Reviewer | plan map and grep helpers; trim the Reference at QA-007 |
Trade-offs: readable opaque content over a more "glassy" look; Bootstrap kept over a full custom system (less churn); no runtime library over faster authoring; ten small stages over fewer large ones so the app works after every commit.
| RK-12 | CSS migration cannot be seen by CI | validated spec blocks, brace balance, anchor greps, owner manual check at the end of every stage |
| RK-13 | Plan errors found late (the first version overstated label work and assumed Bootstrap tabs on Products) | claims re-verified while planning Stages 8-16 and listed in the Pipeline note; the Reviewer re-checks them |
| RK-14 | New files change cascade order | new files link immediately before `navigation-menu.css`; base.css takes the old `site.css` position |
| RK-15 | A new material that forgets the glass tokens would ignore the preference modes | R13 review gate: materials read only `--glass-*`, `--scrim-*`, `--border-glass` |
## R22 Open questions and assumptions (owner decisions)
| ID | Question | Default if unanswered | Status |
|---|---|---|---|
| Q-01 | Approve the ten stages and keeping everything in plan.md? | yes | answered by the owner (stages committed in TODO.md; all stages requested in plan.md) |
| Q-02 | SF Pro Rounded: keep, replace with an open rounded font, or `ui-rounded` plus system stack? | keep for now | open |
| Q-03 | Add a light theme or stay dark-only? | dark-only | open |
| Q-04 | Canvas shell (transparent `main` from 768px, Step 9.6)? | no: Step 9.6 is skipped | open |
| Q-05 | Filters: auto-apply (with the count line of Step 14.1) or an explicit Apply button? | auto-apply | open |
| Q-06 | Browser targets: last two majors of Chrome, Edge, Safari (macOS and iOS), Firefox? | as stated | open |
| Q-07 | Approve test-only Playwright and axe packages plus a non-blocking CI job? | no packages; owner-run evidence (Step 16.2) | open |
| Q-08 | Confirm UI stays English LTR (Persian as data only)? | yes | open |
| Q-09 | Group the Products flyout creation links (UX-001)? | no change, no step | open |
| Q-10 | Confirm PDF invoice and factor styling is out of scope? | yes | open |
| Q-11 | Privacy page text: the owner supplies the policy text; until then the placeholder sentence stays | placeholder stays | open |
| Q-12 | Delete the dead create-product subsystem (R26) in a separate stage after Stage 16? | not deleted | open |
Assumptions: A-01 dark is the primary theme; A-02 desktop first, phone and tablet secondary; A-03 local shop deployment, modest performance budgets; A-04 no CSP in use; A-05 the documented development admin account is used only locally.
## R23 Coverage: what was and was not inspected
- Read in full: `tokens.css`, `site.css`, `components.css`, `app-theme.css` (every rule, by block inventory and by reading each family), `_Layout.cshtml` and its scoped css, the NavigationMenu markup and view component, `site.js`, `person-picker.js`, `product-picker.js`, `product-create-modal.js` (structure); `navigation-menu.css` lines 1-100 plus every glass, radius, shadow, weight and literal line; `jalali-datepicker.js` lines 257-410 and a full outline; views Index, Login, Profile (structure), People/Index, Customers, Sellers, Transactions/Index (header, rows, filter), Sell (scripts), Details (table), Products/Index (tabs), SecondHand (classes), Error, Privacy, Logout, `_PersonPicker`, `_EmptyState`, the 12 live label sites; ProfitLoss controls; the 29 modal tags; the 11 dead partials by pattern.
- Read partially or by grep only: `navigation-menu.css` body (about 440 lines) and `navigation-menu.js`; `profit-loss-chart.js`, `create-product-pricing.js`; CreatePhone and the other create pages beyond labels and modals; `_ProductEditForm`; CustomerDetails and SellerDetails (head); `.cshtml.cs` handlers, test contents and `docs/*`.
- Used as navigation aid: the committed knowledge graph `graphify-out` (built at `6f2b88d`, covers C#, JS and `.cshtml`, not CSS); JS function inventories were taken from it and verified in the source.
- Not inspected: `MobileShop.Api`, QuestPDF output, EF and database code, scripts.
- Not done: no app run, no browser, no screenshots, no interaction states, no performance numbers, no keyboard or screen-reader test; computed styles for F-12, F-13, F-44, F-45 are confirmed by CSS analysis only; HIG Materials was read through a mirror; community repositories and Dribbble shots at search-result level; Awwwards not reached; Apple font licence not researched.
- Not changed: no production code; the stage list is committed in `TODO.md`; nothing else committed by the Planner.
- Pre-existing failures: none observed (CI #616 to #618 green).
## R24 Stage 5 and 6 carry-over (still binding)
- Behaviour: the dock is compact when idle (fine pointers) or while the page scrolls and springs to full size on hover, focus or an open flyout; touch: not compact at rest, compact while scrolling; reduced motion: never compact; keyboard Tab expands immediately; `is-initializing` keeps the first paint compact for two animation frames; clicks inside a 190 ms gate after expansion are dropped on purpose (do not lower it, the spring needs 175 to 197 ms to reach 99%).
- Glass (G1 values): 78% tint, blur 20, saturate 1.5, brightness .6, tuned by measurement in Chromium: muted label 5.0:1 over realistic content, 4.8:1 over a full-width teal strip, `--ink` at least 9.6:1. Known limits (documented, not defects): after a full page load with a stationary pointer the dock starts compact until the first pointer move; over an all-white strip the muted label is 4.3:1 measured (4.13:1 computed here) and the dark theme has none.
- Invariants that must still hold after Step 11.3 (checked by grep and by the owner): the spring and easing custom properties `--nav-spring`, `--nav-ease-out`, `--nav-ease-in-out` exist (now aliases of `--ease-spring`, `--ease-out`, `--ease-standard` with identical values); real-height flyout; focus ring on all 6 tabs; the staggered per-link `transition-delay` lists of the form `0ms, 0ms, <n>ms, <n>ms` (n between 45 and 230) are untouched; no `@keyframes` in the dock css; no hex literal; `grep -c "::before" navigation-menu.css` = 2; `.is-compact` and `.is-settling` appear once, `.is-initializing` in two rules (`.is-initializing` and `.is-initializing:not(:hover)`), so `grep -c "is-initializing"` = 2. Only Chromium was measured; Safari, iOS and Firefox were manual checks.
- Step 11.3 changes only the dock radius (1.9rem to 2rem), the eyebrow capitalization and the aliases; Step 11.4 adds a safe-area bottom offset that equals the old value when the inset is 0.
## R25 Security-sensitive UI (separate Reviewer attention)
The redesign changes markup and styling only; none of the following may change behaviour, field names, handlers or tokens.
- Login (Steps 12.1, 12.2) and Profile (Steps 12.1, 15.4): password fields, `_PasswordToggle`, the new Appearance checkbox is outside the password form; antiforgery hidden inputs and `autocomplete` attributes stay.
- Apple ID plaintext display (Products/Details `code`, create and edit forms): project.md keeps these passwords plain; no step masks, hides or "fixes" them (Steps 9.3 and 13.2 only restyle the surrounding table and sections).
- `site.js` (Steps 8.2, 10.3, 15.4): code near form submission (Enter-to-submit, validation); the Reviewer confirms submit logic is unchanged and that `stopPropagation` in the picker helper only applies to an active option.
- Factor and PDF download links on Transactions/Details: untouched.
- The inline head script of Step 15.4 reads one `localStorage` key and stores no data about people or money.
## R26 Dead code found while planning (out of redesign scope, owner decision Q-12)
Evidence (re-runnable): `grep -rn "product-create-modal" src --include=*.cshtml` finds no include; no markup defines `id="create-product-modal"`; `grep -rn "ProductCreateRegistry" src` finds only its own file; the 11 `_ProductCreate*Form` partials are referenced only by that registry; `jalali-datepicker.js` and `product-picker.js` still mention the modal and its `product-created` event. Size: 389 lines of partials, 197 lines of script, 29 lines of registry, 105 unassociated labels, 115 controls without ids. Effect on this plan: none of it is restyled or fixed; F-15 shrinks to 12 live labels. Proposal (needs approval, separate stage after Stage 16): delete the partials, the script, the registry and the `product-created` listener; CI would confirm nothing references them.
## R27 CSS spec blocks (validated; extracted by the steps with `sed`)
Each block parses without errors, has balanced braces, contains no hex literal and uses only defined custom properties. Compared with the rules they replace, the only selectors not carried over are dead or intentionally merged: `.alert-warning`, `.badge`, `.border-bottom`, `.btn-info`, `.btn-warning`, `.btn-outline-success`, `.btn-outline-danger`, `.btn-outline-dark`, `.btn-outline-info`, `.dropdown-*`, `.popover`, `.progress`, `.lead`, `.text-body`, `.text-dark`, `.form-control-plaintext`, `.profit-chart-empty`, the `.table*` rules (Details moves to `.data-table`), the button hover-lift rule `.btn:hover`, and the picker-only hover rules (merged into the generic `.list-group-item-action`). Do not edit a block in place after a step has used it; a change needs a new Planner pass.

### controls-buttons-v1
```css
/* BEGIN controls-buttons-v1 */
/* Buttons. Four looks: primary (accent fill), secondary (raised neutral), outline (transparent), danger.
   Legacy Bootstrap names stay as aliases: primary = btn-primary, btn-success; secondary = btn-secondary, btn-dark;
   outline = btn-outline-primary, btn-outline-secondary, btn-outline-warning; danger = btn-danger. */
.btn {
  --bs-btn-border-radius: var(--radius-md);
  padding: 0.55rem 0.9rem;
  font-weight: var(--fw-regular);
  box-shadow: none;
  transition:
    color var(--dur-fast) var(--ease-standard),
    background-color var(--dur-fast) var(--ease-standard),
    border-color var(--dur-fast) var(--ease-standard),
    box-shadow var(--dur-base) var(--ease-standard),
    transform var(--dur-instant) var(--ease-press);
}

.btn:active {
  transform: scale(0.98);
}

.btn-primary,
.btn-success,
.btn-danger,
.btn-dark {
  --bs-btn-color: var(--ink);
  --bs-btn-hover-color: var(--ink);
  --bs-btn-active-color: var(--ink);
  background-image: none;
}

.btn-primary,
.btn-success {
  --bs-btn-color: var(--accent-ink);
  --bs-btn-hover-color: var(--accent-ink);
  --bs-btn-active-color: var(--accent-ink);
  --bs-btn-bg: var(--accent);
  --bs-btn-border-color: var(--accent);
  --bs-btn-hover-bg: var(--accent-strong);
  --bs-btn-hover-border-color: var(--accent-strong);
  --bs-btn-active-bg: var(--accent);
  --bs-btn-active-border-color: var(--accent);
}

.btn-danger {
  --bs-btn-color: var(--danger-ink);
  --bs-btn-hover-color: var(--danger-ink);
  --bs-btn-active-color: var(--danger-ink);
  --bs-btn-bg: var(--danger);
  --bs-btn-border-color: var(--danger);
  --bs-btn-hover-bg: color-mix(in srgb, var(--danger) 88%, white);
  --bs-btn-hover-border-color: color-mix(in srgb, var(--danger) 88%, white);
  --bs-btn-active-bg: var(--danger);
  --bs-btn-active-border-color: var(--danger);
}

.btn-secondary,
.btn-dark {
  --bs-btn-bg: var(--surface-2);
  --bs-btn-border-color: var(--border-control);
  --bs-btn-hover-bg: var(--surface-3);
  --bs-btn-hover-border-color: var(--text-2);
  --bs-btn-active-bg: var(--surface-2);
  --bs-btn-active-border-color: var(--border-control);
}

.btn-outline-primary,
.btn-outline-secondary,
.btn-outline-warning {
  --bs-btn-color: var(--ink);
  --bs-btn-hover-color: var(--ink);
  --bs-btn-active-color: var(--ink);
  --bs-btn-bg: transparent;
  --bs-btn-hover-bg: color-mix(in srgb, var(--ink) 6%, transparent);
  --bs-btn-active-bg: transparent;
  --bs-btn-border-color: var(--border-control);
  --bs-btn-hover-border-color: var(--border-control);
  --bs-btn-active-border-color: var(--border-control);
}

.btn-outline-primary:hover,
.btn-outline-primary:active {
  --bs-btn-hover-border-color: var(--accent);
  --bs-btn-active-border-color: var(--accent);
}

@media (pointer: coarse) {
  .btn {
    min-height: var(--size-hit-min);
  }
}
/* END controls-buttons-v1 */
```

### controls-fields-v1
```css
/* BEGIN controls-fields-v1 */
/* Focus, labels, fields, checks, fieldset. Replaces the focus and form rules that lived in app-theme.css. */
.btn:focus-visible,
.form-control:focus-visible,
.form-select:focus-visible,
.form-check-input:focus-visible,
a:focus-visible {
  outline: none;
  box-shadow: var(--focus-ring);
}

.btn:focus,
.btn:active:focus,
.btn-link.nav-link:focus,
.form-control:focus,
.form-check-input:focus {
  box-shadow: var(--focus-ring);
}

.form-label,
label {
  color: var(--text-2);
  font-size: var(--fs-sm);
  font-weight: var(--fw-regular);
}

.form-control,
.form-select {
  min-height: var(--size-control);
  border: 1px solid var(--border-control);
  border-radius: var(--radius-md);
  color: var(--text-1);
  background-color: var(--surface-inset);
}

.form-control[data-finished-price] {
  color: var(--text-1);
  background-color: color-mix(in srgb, var(--surface-inset) 60%, black);
  opacity: 1;
}

.form-control::placeholder {
  color: var(--text-2);
  opacity: 1;
}

.form-control:disabled,
.form-control[readonly],
.form-select:disabled {
  border-color: var(--hairline);
  color: var(--text-2);
  background-color: color-mix(in srgb, var(--surface-2) 70%, var(--surface-inset));
  opacity: 1;
  -webkit-text-fill-color: var(--text-2);
  cursor: not-allowed;
}

.form-control:disabled::placeholder {
  color: var(--text-2);
  opacity: 1;
  -webkit-text-fill-color: var(--text-2);
}

.form-control:focus,
.form-select:focus {
  border-color: var(--accent);
  color: var(--text-1);
  background-color: var(--surface-inset);
  box-shadow: var(--focus-ring);
}

.form-control.input-validation-error,
.form-select.input-validation-error,
.form-control.is-invalid,
.form-select.is-invalid {
  border-color: var(--danger);
}

.form-select option {
  color: var(--text-1);
  background: var(--surface-1);
}

.form-check-input {
  border-color: var(--border-control);
  background-color: var(--surface-inset);
}

.form-check-input:checked {
  border-color: var(--accent);
  background-color: var(--accent);
}

fieldset {
  min-width: 0;
  padding: 0.85rem 1rem;
  border: 1px solid var(--hairline);
  border-radius: var(--radius-lg);
  background: color-mix(in srgb, var(--ink) 2%, transparent);
}

legend {
  float: none;
  width: auto;
  margin: 0;
  padding: 0 0.3rem;
  color: var(--text-1);
}

.input-group-text {
  border-color: var(--border-control);
  border-radius: var(--radius-md);
  color: var(--text-2);
  background: var(--surface-2);
}

@media (pointer: coarse) {
  .form-control,
  .form-select {
    min-height: var(--size-hit-min);
  }

  .form-check-label {
    display: inline-block;
    padding-block: 0.6rem;
  }
}
/* END controls-fields-v1 */
```

### base-v1
```css
/* BEGIN base-v1 */
/* Fonts, document, typography, links, utilities, footer, narrow-screen panel, reduced motion.
   Replaces site.css and the base rules of app-theme.css. */
/* SF Pro Rounded, self-hosted (only Regular 400 and Bold 700 exist upstream, there is no italic). */
@font-face {
  font-family: 'SF Pro Rounded';
  font-style: normal;
  font-weight: 400;
  font-display: swap;
  src: url('../fonts/sf-pro-rounded-regular.woff2') format('woff2');
}

@font-face {
  font-family: 'SF Pro Rounded';
  font-style: normal;
  font-weight: 700;
  font-display: swap;
  src: url('../fonts/sf-pro-rounded-bold.woff2') format('woff2');
}

:root {
  color-scheme: dark;
  --app-ink: var(--ink);
  --app-muted: var(--ink-muted);
  --app-accent: var(--accent);
  --app-accent-strong: var(--accent);
  --app-surface: var(--panel);
  --app-surface-raised: var(--panel-raised);
  --app-edge: var(--line);
  --app-edge-strong: var(--line-strong);
  --bs-body-font-family: var(--font-body);
  --bs-body-bg: var(--ground);
  --bs-body-color: var(--ink);
  --bs-secondary-color: var(--ink-muted);
  --bs-tertiary-color: var(--ink-muted);
  --bs-border-color: var(--line);
  --bs-primary: var(--accent);
  --bs-primary-rgb: 54, 197, 176;
  --bs-link-color: var(--accent);
  --bs-link-hover-color: var(--ink);
}

html {
  position: relative;
  min-height: 100%;
  font-size: 14px;
  color-scheme: dark;
  scroll-behavior: smooth;
}

@media (min-width: 768px) {
  html {
    font-size: 16px;
  }
}

body {
  min-height: 100vh;
  margin-bottom: 0;
  color: var(--text-1);
  background: var(--surface-canvas);
}

.container {
  max-width: 1320px;
}

.container > main[role="main"] {
  min-height: calc(100vh - 10rem);
  margin-top: clamp(1rem, 3vw, 2.25rem) !important;
  margin-bottom: 2rem;
  padding: clamp(1.15rem, 3vw, 2.25rem) !important;
  border: 1px solid var(--hairline);
  border-radius: var(--radius-xl);
  background: var(--surface-1);
}

h1,
h2,
h3,
h4,
h5,
h6 {
  color: var(--text-1);
  letter-spacing: -0.01em;
}

h1 {
  margin-bottom: 1.15rem;
  font-size: var(--fs-h1);
  font-weight: var(--fw-strong);
  line-height: var(--lh-tight);
  letter-spacing: var(--tracking-tight);
}

h2 {
  font-size: var(--fs-h2);
  font-weight: var(--fw-strong);
}

a {
  color: var(--accent);
  text-underline-offset: 0.18em;
  transition: color var(--dur-fast) var(--ease-standard);
}

a:hover {
  color: var(--text-1);
}

.text-muted,
.form-text {
  color: var(--text-2) !important;
}

.border,
.border-top {
  border-color: var(--hairline) !important;
}

.footer {
  position: static;
  width: auto;
  margin-top: 1rem;
  padding-block: 0.8rem;
  border-top: 1px solid var(--hairline);
  color: var(--text-2) !important;
  line-height: 1.5;
  white-space: normal;
}

@media (max-width: 767.98px) {
  .container > main[role="main"] {
    min-height: 0;
    border-radius: var(--radius-lg);
  }
}

@media (max-width: 575.98px) {
  .container > main[role="main"] {
    padding: 1rem !important;
    border-left: 0;
    border-right: 0;
    border-radius: 0;
  }
}

@media (prefers-reduced-motion: reduce) {
  html {
    scroll-behavior: auto;
  }

  *,
  *::before,
  *::after {
    transition-duration: 0.01ms !important;
    animation-duration: 0.01ms !important;
    animation-iteration-count: 1 !important;
  }
}
/* END base-v1 */
```

### shell-canvas-v1
```css
/* BEGIN shell-canvas-v1 */
/* Canvas shell (gated, plan Step 9.6): from 768px the content panel melts into the canvas; one fixed ambient layer gives glass something to sample. */
@media (min-width: 768px) {
  .container > main[role="main"] {
    border-color: transparent;
    background: transparent;
  }
}

body::before {
  content: "";
  position: fixed;
  inset: 0;
  z-index: -1;
  pointer-events: none;
  background:
    radial-gradient(60rem 36rem at 12% -8%, color-mix(in srgb, var(--accent) 12%, transparent), transparent 70%),
    radial-gradient(48rem 30rem at 100% 0%, color-mix(in srgb, var(--text-2) 7%, transparent), transparent 70%);
}

@media (prefers-contrast: more), (prefers-reduced-transparency: reduce) {
  body::before {
    display: none;
  }
}

html[data-surfaces="solid"] body::before {
  display: none;
}
/* END shell-canvas-v1 */
```

### surfaces-cards-v1
```css
/* BEGIN surfaces-cards-v1 */
/* One surface family (material M2): cards, stat tiles, form sections and chart cards share fill, edge, radius
   and elevation. Also page header, toolbar, stat grid, empty and loading states, numeric alignment. */
.card,
.stat,
.form-section,
.profit-chart-card {
  border: 1px solid var(--hairline);
  border-radius: var(--radius-lg);
  color: var(--text-1);
  background-color: var(--surface-2);
  box-shadow: var(--shadow-1), var(--edge-highlight);
}

.card {
  overflow: hidden;
}

.card-title {
  color: var(--text-1);
}

.page-head {
  display: flex;
  flex-wrap: wrap;
  justify-content: space-between;
  align-items: end;
  gap: var(--space-4);
  margin-bottom: var(--space-5);
  border-bottom: 1px solid var(--hairline);
  padding-bottom: var(--space-3);
}

.page-head__title {
  margin: 0;
}

.page-head__sub {
  margin: 0;
  color: var(--text-2);
  font-size: var(--fs-sm);
}

.page-head__actions {
  display: flex;
  flex-wrap: wrap;
  gap: var(--space-2);
}

.toolbar {
  display: flex;
  flex-wrap: wrap;
  gap: var(--space-3);
  align-items: end;
  margin-bottom: var(--space-4);
}

.toolbar .form-label {
  font-size: var(--fs-sm);
  color: var(--text-2);
}

.stat-grid {
  display: grid;
  grid-template-columns: repeat(auto-fit, minmax(11rem, 1fr));
  gap: var(--space-3);
}

.stat {
  padding: var(--space-4);
}

.stat__value {
  font-size: var(--fs-h2);
  font-weight: var(--fw-strong);
  font-variant-numeric: tabular-nums;
}

.stat__label {
  color: var(--text-2);
  font-size: var(--fs-sm);
}

.form-section {
  padding: var(--space-4) var(--space-5);
  margin-bottom: var(--space-4);
}

.form-section__title {
  font-size: var(--fs-lg);
  font-weight: var(--fw-strong);
  margin-bottom: var(--space-3);
}

.profit-chart-card {
  margin: 1.4rem 0 1.8rem;
  padding: clamp(1rem, 2vw, 1.5rem);
  overflow: hidden;
}

.profit-chart-heading {
  display: flex;
  align-items: flex-start;
  justify-content: space-between;
  gap: var(--space-4);
  margin-bottom: 0.8rem;
}

.profit-chart-heading h2 {
  margin-bottom: 0.25rem;
  font-size: 1.1rem;
}

.profit-chart-heading p {
  margin: 0;
  color: var(--text-2);
  font-size: 0.85rem;
}

.profit-chart-range {
  flex: 0 0 auto;
  padding: 0.35rem 0.65rem;
  border: 1px solid var(--hairline);
  border-radius: var(--radius-pill);
  color: var(--text-2);
  background: var(--surface-1);
  font-size: 0.73rem;
}

.profit-chart-canvas {
  position: relative;
  width: 100%;
  height: 360px;
}

.profit-chart-legend {
  display: flex;
  align-items: center;
  gap: var(--space-2);
  margin: 0.2rem 0 0 0.45rem;
  color: var(--text-2);
  font-size: 0.78rem;
}

.profit-chart-legend span {
  width: 1.1rem;
  height: 0.18rem;
  border-radius: var(--radius-pill);
  background: var(--accent);
}

.empty-state {
  display: flex;
  flex-direction: column;
  align-items: center;
  text-align: center;
  padding: 2.5rem var(--space-4);
  border: 1px dashed var(--border-control);
  border-radius: var(--radius-lg);
}

.empty-state__title {
  font-size: var(--fs-lg);
  margin: 0;
}

.empty-state__hint {
  color: var(--text-2);
  margin: 0;
}

.loading-line {
  color: var(--text-2);
  font-size: var(--fs-sm);
  padding: var(--space-2) var(--space-3);
}

.num {
  font-variant-numeric: tabular-nums;
}

td.num,
th.num {
  text-align: end;
}

@media (max-width: 575.98px) {
  .toolbar > * {
    flex: 1 1 100%;
  }

  .page-head__actions {
    width: 100%;
  }
}
/* END surfaces-cards-v1 */
```

### surfaces-tables-v1
```css
/* BEGIN surfaces-tables-v1 */
/* One table idiom (material M1 content). Roomier rows and cells (Apple: larger row height and padding). */
.data-table {
  width: 100%;
  border-collapse: collapse;
}

.data-table th {
  padding: var(--space-3) var(--space-4);
  border-bottom: 1px solid var(--border-control);
  color: var(--text-2);
  font-size: var(--fs-sm);
  font-weight: var(--fw-strong);
  text-align: start;
}

.data-table th a {
  color: inherit;
  text-decoration: none;
}

.data-table th a:hover,
.data-table th a:focus-visible {
  color: var(--text-1);
}

.data-table td {
  padding: var(--space-3) var(--space-4);
  border-bottom: 1px solid var(--hairline);
  vertical-align: middle;
}

.data-table tbody tr {
  transition: background-color var(--dur-fast) var(--ease-standard);
}

.data-table tbody tr:hover {
  background-color: var(--surface-2);
}

.data-table td.num,
.data-table th.num {
  text-align: end;
}

@media (prefers-reduced-motion: reduce) {
  .data-table tbody tr {
    transition: none;
  }
}
/* END surfaces-tables-v1 */
```

### surfaces-feedback-v1
```css
/* BEGIN surfaces-feedback-v1 */
/* Status chips (pill, tinted fill plus text, never colour alone), alerts (no blur), validation text. */
.status-chip {
  display: inline-flex;
  align-items: center;
  padding: 0.15rem 0.6rem;
  border-radius: var(--radius-pill);
  font-size: var(--fs-sm);
  font-weight: var(--fw-strong);
}

.status-chip--available {
  background-color: var(--accent-soft);
  color: var(--accent);
}

.status-chip--sold {
  background-color: color-mix(in srgb, var(--text-2) 16%, transparent);
  color: var(--text-2);
}

.status-chip--used {
  background-color: var(--warn-soft);
  color: var(--warn);
}

.alert {
  border-radius: var(--radius-lg);
  color: var(--text-1);
  background-color: var(--surface-2);
}

.alert-success,
.alert-info {
  border-color: var(--accent);
}

.alert-danger {
  border-color: var(--danger);
}

.text-danger,
.field-validation-error,
.validation-summary-errors {
  color: var(--danger) !important;
}

.validation-summary-valid,
.field-validation-valid {
  display: none;
}
/* END surfaces-feedback-v1 */
```

### surfaces-controls-v1
```css
/* BEGIN surfaces-controls-v1 */
/* Tabs (ProfitLoss) and the segmented control (pill track, one selected pill; markup: radios with .btn-check and labels with .btn). */
.nav-tabs {
  gap: var(--space-1);
  border-bottom-color: var(--hairline);
}

.nav-tabs .nav-link {
  border: 1px solid transparent;
  border-radius: var(--radius-md) var(--radius-md) 0 0;
  color: var(--text-2);
  transition:
    color var(--dur-fast) var(--ease-standard),
    background-color var(--dur-base) var(--ease-standard),
    border-color var(--dur-base) var(--ease-standard);
}

.nav-tabs .nav-link:hover,
.nav-tabs .nav-link:focus-visible {
  border-color: var(--border-control);
  color: var(--text-1);
  background: var(--surface-2);
}

.nav-tabs .nav-link.active,
.nav-tabs .nav-item.show .nav-link {
  border-color: var(--border-control) var(--border-control) transparent;
  color: var(--text-1);
  background: var(--surface-2);
  box-shadow: none;
}

.segmented {
  display: inline-flex;
  gap: 2px;
  padding: 3px;
  border: 1px solid var(--border-control);
  border-radius: var(--radius-pill);
  background: var(--surface-inset);
}

.segmented > .btn {
  border: 0;
  border-radius: var(--radius-pill);
  padding: 0.3rem 0.9rem;
  color: var(--text-2);
  background: transparent;
}

.segmented > .btn:hover {
  color: var(--text-1);
  background: color-mix(in srgb, var(--ink) 6%, transparent);
}

.segmented > .btn-check:checked + .btn {
  color: var(--accent);
  background: var(--accent-soft);
  font-weight: var(--fw-strong);
}

.segmented > .btn-check:focus-visible + .btn {
  box-shadow: var(--focus-ring);
}
/* END surfaces-controls-v1 */
```

### overlays-modal-v1
```css
/* BEGIN overlays-modal-v1 */
/* Modal sheets: glass overlay material (G3); solid (G3-solid) for scrollable dialogs and anything marked .modal-content--solid.
   Scrim: one dimmed, blurred layer. List groups (picker results) stay opaque because they sit in the page flow. */
.modal-content {
  border: 1px solid var(--border-glass);
  border-radius: var(--radius-xl);
  color: var(--text-1);
  background-color: var(--glass-overlay-bg);
  -webkit-backdrop-filter: var(--glass-overlay-filter);
  backdrop-filter: var(--glass-overlay-filter);
  box-shadow: var(--shadow-4), var(--edge-highlight);
}

.modal-dialog-scrollable .modal-content,
.modal-content--solid {
  background-color: var(--glass-solid-bg);
  -webkit-backdrop-filter: none;
  backdrop-filter: none;
}

@supports not ((backdrop-filter: blur(1px)) or (-webkit-backdrop-filter: blur(1px))) {
  .modal-content {
    background-color: var(--glass-solid-bg);
  }
}

.modal {
  z-index: var(--z-modal);
}

.modal-backdrop {
  z-index: var(--z-scrim);
}

.modal-backdrop.show {
  background-color: var(--scrim-bg);
  opacity: 1;
  -webkit-backdrop-filter: var(--scrim-filter);
  backdrop-filter: var(--scrim-filter);
}

.modal-header,
.modal-footer {
  border-color: var(--hairline);
}

.list-group-item {
  border-color: var(--hairline);
  color: var(--text-1);
  background-color: var(--surface-1);
}

.list-group-item-action:hover,
.list-group-item-action:focus,
.list-group-item-action.is-active {
  color: var(--text-1);
  background-color: var(--surface-3);
}
/* END overlays-modal-v1 */
```

### overlays-jalali-v1
```css
/* BEGIN overlays-jalali-v1 */
/* ──────────────────────────────────────────────────────────────────────────
   Jalali Date Picker – styled to match the dark app theme
   ────────────────────────────────────────────────────────────────────────── */
.jalali-picker-panel {
  position: fixed;
  z-index: var(--z-popover);
  min-width: 260px;
  padding: 0.75rem;
  border: 1px solid var(--border-glass);
  border-radius: var(--radius-xl);
  color: var(--text-1);
  background-color: var(--glass-overlay-bg);
  -webkit-backdrop-filter: var(--glass-overlay-filter);
  backdrop-filter: var(--glass-overlay-filter);
  box-shadow: var(--shadow-3), var(--edge-highlight);
  animation: jalaliFadeIn var(--dur-base) var(--ease-press);
}

@keyframes jalaliFadeIn {
  from { opacity: 0; transform: translateY(6px) scale(0.98); }
  to { opacity: 1; transform: translateY(0) scale(1); }
}

.jalali-picker-head {
  display: flex;
  align-items: center;
  justify-content: space-between;
  margin-bottom: 0.65rem;
  padding: 0 0.25rem;
}

.jalali-picker-nav {
  display: grid;
  place-items: center;
  width: 2.25rem;
  height: 2.25rem;
  border: 1px solid var(--border-control);
  border-radius: var(--radius-sm);
  color: var(--text-2);
  background: transparent;
  transition: color var(--dur-fast) var(--ease-standard), background-color var(--dur-fast) var(--ease-standard), border-color var(--dur-fast) var(--ease-standard), transform var(--dur-instant) var(--ease-press);
}

.jalali-picker-nav:hover {
  color: var(--text-1);
  background: var(--surface-1);
  border-color: var(--accent);
}

.jalali-picker-nav:active {
  transform: scale(0.98);
}

.jalali-picker-title {
  font-size: 0.85rem;
  font-weight: var(--fw-strong);
  color: var(--text-1);
}

.jalali-picker-weekdays {
  display: grid;
  grid-template-columns: repeat(7, 1fr);
  margin-bottom: 0.35rem;
  text-align: center;
  color: var(--text-2);
  font-size: 0.68rem;
  font-weight: var(--fw-strong);
}

.jalali-picker-grid {
  display: grid;
  grid-template-columns: repeat(7, 1fr);
  gap: 0.18rem;
}

.jalali-picker-day {
  display: grid;
  place-items: center;
  height: 2.35rem;
  border: 1px solid transparent;
  border-radius: var(--radius-sm);
  color: var(--text-1);
  background: transparent;
  font-size: 0.82rem;
  font-weight: var(--fw-strong);
  transition: color var(--dur-fast) var(--ease-standard), background-color var(--dur-fast) var(--ease-standard), border-color var(--dur-fast) var(--ease-standard), box-shadow var(--dur-fast) var(--ease-standard), transform var(--dur-instant) var(--ease-press);
}

.jalali-picker-day:hover {
  color: var(--text-1);
  background: var(--surface-1);
  border-color: var(--border-control);
  box-shadow: none;
}

.jalali-picker-day.is-today {
  color: var(--text-1);
  background: var(--surface-1);
  border-color: var(--border-control);
  box-shadow: none;
}

.jalali-picker-day.is-selected {
  color: var(--accent-ink);
  background-color: var(--accent);
  border-color: var(--accent);
  box-shadow: none;
}

.jalali-picker-day.is-selected:hover {
  color: var(--accent-ink);
  background-color: var(--accent);
}

.jalali-picker-time {
  display: flex;
  align-items: center;
  gap: 0.5rem;
  margin-top: 0.75rem;
  padding-top: 0.75rem;
  border-top: 1px solid var(--hairline);
}

.jalali-picker-time-label {
  color: var(--text-2);
  font-size: 0.72rem;
  font-weight: var(--fw-strong);
}

.jalali-picker-time input[type="number"] {
  width: 3.8rem;
  height: 2.25rem;
  padding: 0 0.5rem;
  border: 1px solid var(--hairline);
  border-radius: var(--radius-sm);
  color: var(--text-1);
  background: var(--surface-inset);
  font-size: 0.82rem;
  font-weight: var(--fw-strong);
  text-align: center;
  -moz-appearance: textfield;
}

.jalali-picker-time input[type="number"]::-webkit-outer-spin-button,
.jalali-picker-time input[type="number"]::-webkit-inner-spin-button {
  -webkit-appearance: none;
  margin: 0;
}

.jalali-picker-time input[type="number"]:focus {
  outline: none;
  border-color: var(--accent);
  background: var(--surface-inset);
  box-shadow: var(--focus-ring);
}

.jalali-picker-time-separator {
  color: var(--text-2);
  font-weight: var(--fw-strong);
}

.jalali-picker-action {
  display: inline-flex;
  align-items: center;
  justify-content: center;
  gap: 0.35rem;
  height: 2.25rem;
  padding: 0 0.75rem;
  border: 1px solid var(--hairline);
  border-radius: var(--radius-sm);
  color: var(--text-2);
  background: var(--surface-1);
  font-size: 0.76rem;
  font-weight: var(--fw-strong);
  transition: color var(--dur-fast) var(--ease-standard), background-color var(--dur-fast) var(--ease-standard), border-color var(--dur-fast) var(--ease-standard), transform var(--dur-instant) var(--ease-press);
}

.jalali-picker-action:hover {
  color: var(--text-1);
  background: color-mix(in srgb, var(--accent) 14%, transparent);
  border-color: color-mix(in srgb, var(--accent) 35%, transparent);
}

.jalali-picker-action:active {
  transform: scale(0.98);
}

.jalali-picker-footer {
  display: flex;
  justify-content: flex-end;
  gap: 0.5rem;
  margin-top: 0.75rem;
  padding-top: 0.5rem;
  border-top: 1px solid var(--hairline);
}

.jalali-picker-feedback {
  margin-top: 0.35rem;
  color: var(--danger);
  font-size: 0.76rem;
}
/* END overlays-jalali-v1 */
```

### shell-v1
```css
/* BEGIN shell-v1 */
/* Skip link, action bar (material G2 subtle glass), dock clearance, focus clearance (WCAG 2.4.11).
   Replaces the matching rules of components.css. */
html {
  scroll-padding-bottom: 12rem;
}

.skip-link {
  position: absolute;
  left: -10000px;
  top: auto;
  width: 1px;
  height: 1px;
  overflow: hidden;
}

.skip-link:focus {
  position: fixed;
  top: 0.75rem;
  left: 0.75rem;
  width: auto;
  height: auto;
  overflow: visible;
  background: var(--accent);
  color: var(--accent-ink);
  border-radius: var(--radius-md);
  padding: 0.5rem 0.75rem;
  z-index: var(--z-modal);
}

.action-bar {
  position: sticky;
  bottom: 0;
  z-index: 10;
  display: flex;
  gap: var(--space-2);
  justify-content: flex-end;
  border-top: 1px solid var(--hairline);
  background: var(--glass-subtle-bg);
  -webkit-backdrop-filter: var(--glass-subtle-filter);
  backdrop-filter: var(--glass-subtle-filter);
  padding-top: var(--space-4);
  padding-bottom: calc(var(--space-4) + env(safe-area-inset-bottom));
  margin-top: var(--space-5);
}

.form-section .action-bar {
  position: static;
  border-top: 0;
  background: transparent;
  -webkit-backdrop-filter: none;
  backdrop-filter: none;
  padding: 0;
  margin-top: var(--space-5);
}

@supports not ((backdrop-filter: blur(1px)) or (-webkit-backdrop-filter: blur(1px))) {
  .action-bar {
    background: var(--glass-solid-bg);
  }
}

body.modal-open .action-bar,
body.modal-open .navigation-dock {
  background: var(--glass-solid-bg);
  -webkit-backdrop-filter: none;
  backdrop-filter: none;
}

@media (max-width: 575.98px) {
  .form-section .action-bar .btn {
    flex: 1 1 auto;
  }
}

@media (max-width: 1199.98px) {
  body.has-floating-navigation .action-bar {
    bottom: 5.5rem;
  }
}

@media (max-width: 575.98px) {
  body.has-floating-navigation .action-bar {
    bottom: calc(max(0.65rem, env(safe-area-inset-bottom, 0px)) + 4.6rem);
  }
}

.has-floating-navigation {
  padding-bottom: calc(var(--nav-dock-height, 5rem) + 1rem);
}
/* END shell-v1 */
```

### login-v1
```css
/* BEGIN login-v1 */
/* Login page: glass overlay card (G3) over a local ambient gradient. Moved from app-theme.css with token edits. */
.account-login-page {
  display: grid;
  min-height: min(68vh, 44rem);
  place-items: center;
  padding: clamp(0.25rem, 3vw, 2rem);
  border-radius: var(--radius-xl);
  background:
    radial-gradient(38rem 24rem at 18% 8%, color-mix(in srgb, var(--accent) 14%, transparent), transparent 70%),
    radial-gradient(34rem 22rem at 86% 92%, color-mix(in srgb, var(--text-2) 8%, transparent), transparent 70%);
}

.account-login-card {
  position: relative;
  isolation: isolate;
  width: min(100%, 29rem);
  padding: clamp(1.6rem, 5vw, 2.8rem);
  overflow: hidden;
  border: 1px solid var(--border-glass);
  border-radius: var(--radius-xl);
  text-align: center;
  color: var(--text-1);
  background: var(--glass-overlay-bg);
  box-shadow: var(--shadow-3), var(--edge-highlight);
  -webkit-backdrop-filter: var(--glass-overlay-filter);
  backdrop-filter: var(--glass-overlay-filter);
}

.account-login-mark {
  display: grid;
  width: 3.5rem;
  height: 3.5rem;
  margin: 0 auto 1.25rem;
  place-items: center;
  border: 1px solid var(--border-control);
  border-radius: var(--radius-md);
  color: var(--accent);
  background: var(--surface-2);
  box-shadow: none;
}

.account-login-mark svg {
  width: 1.75rem;
  height: 1.75rem;
  stroke: currentColor;
  stroke-linecap: round;
  stroke-linejoin: round;
  stroke-width: 1.5;
}

.account-login-eyebrow {
  margin-bottom: 0.45rem;
  color: var(--text-2);
  font-size: 0.74rem;
  font-weight: 700;
}

.account-login-title {
  margin-bottom: 0.5rem;
  font-size: clamp(1.75rem, 5vw, 2.15rem);
}

.account-login-description {
  margin-bottom: 1.8rem;
  color: var(--text-2);
  font-size: 0.94rem;
}

.account-login-form {
  display: grid;
  gap: 1.15rem;
  text-align: left;
}

.account-login-field .form-label {
  margin-bottom: 0.45rem;
  font-size: 0.86rem;
}

.account-login-field .form-control {
  min-height: var(--size-control-lg);
  padding-inline: 0.95rem;
  background-color: var(--surface-inset);
}

.account-login-field .form-control:focus {
  border-color: var(--accent);
  box-shadow: var(--focus-ring);
}

.password-visibility-toggle {
  display: grid;
  min-width: 3rem;
  place-items: center;
  border-color: var(--border-control);
  color: var(--text-2);
  background: var(--surface-inset);
}

.password-visibility-toggle:hover {
  border-color: var(--accent);
  color: var(--text-1);
  background: var(--surface-1);
}

.password-visibility-toggle:focus-visible {
  z-index: 5;
  box-shadow: var(--focus-ring);
}

.password-visibility-toggle svg {
  width: 1.2rem;
  height: 1.2rem;
  fill: none;
  stroke: currentColor;
  stroke-linecap: round;
  stroke-linejoin: round;
  stroke-width: 1.7;
}

.password-visibility-toggle .password-eye-open {
  display: none;
}

.password-visibility-toggle.is-visible .password-eye-closed {
  display: none;
}

.password-visibility-toggle.is-visible .password-eye-open {
  display: inline;
}

.account-login-submit {
  display: flex;
  min-height: var(--size-control-lg);
  align-items: center;
  justify-content: center;
  gap: 0.6rem;
  margin-top: 0.3rem;
  border-radius: var(--radius-md);
  box-shadow: none;
}

.account-login-submit:hover {
  box-shadow: none;
}

.account-login-submit svg {
  width: 1.1rem;
  height: 1.1rem;
  stroke: currentColor;
  stroke-linecap: round;
  stroke-linejoin: round;
  stroke-width: 1.8;
  transition: transform var(--dur-base) var(--ease-press);
}

.account-login-submit:hover svg {
  transform: translateX(2px);
}

.account-login-validation.validation-summary-valid {
  display: none;
}

.account-login-validation.validation-summary-errors {
  padding: 0.75rem 0.9rem;
  border: 1px solid var(--danger);
  border-radius: var(--radius-md);
  color: var(--danger);
  background: color-mix(in srgb, var(--danger) 16%, transparent);
  font-size: 0.88rem;
}

.account-login-validation ul {
  margin: 0;
  padding-left: 1.15rem;
}

.account-login-field .field-validation-error {
  display: block;
  margin-top: 0.35rem;
  font-size: 0.82rem;
}

.account-login-message {
  margin: 1.25rem 0 0;
  text-align: left;
  overflow-wrap: anywhere;
}
/* END login-v1 */
```

### motion-v1
```css
/* BEGIN motion-v1 */
/* Preference modes. Materials read only the glass tokens, so each mode overrides the tokens in one place:
   solid surfaces for reduced transparency, more contrast, or the owner switch html[data-surfaces="solid"]. */
@media (prefers-reduced-transparency: reduce) {
  :root {
    --glass-regular-bg: var(--glass-solid-bg);
    --glass-subtle-bg: var(--glass-solid-bg);
    --glass-overlay-bg: var(--glass-solid-bg);
    --glass-regular-filter: none;
    --glass-subtle-filter: none;
    --glass-overlay-filter: none;
    --scrim-bg: rgb(0 0 0 / 0.72);
    --scrim-filter: none;
    --border-glass: var(--border-control);
  }
}

html[data-surfaces="solid"] {
  --glass-regular-bg: var(--glass-solid-bg);
  --glass-subtle-bg: var(--glass-solid-bg);
  --glass-overlay-bg: var(--glass-solid-bg);
  --glass-regular-filter: none;
  --glass-subtle-filter: none;
  --glass-overlay-filter: none;
  --scrim-bg: rgb(0 0 0 / 0.72);
  --scrim-filter: none;
  --border-glass: var(--border-control);
}

@media (prefers-contrast: more) {
  :root {
    --glass-regular-bg: var(--glass-solid-bg);
    --glass-subtle-bg: var(--glass-solid-bg);
    --glass-overlay-bg: var(--glass-solid-bg);
    --glass-regular-filter: none;
    --glass-subtle-filter: none;
    --glass-overlay-filter: none;
    --scrim-bg: rgb(0 0 0 / 0.72);
    --scrim-filter: none;
    --border-glass: var(--text-2);
    --hairline: var(--text-2);
    --border-control: var(--text-2);
    --text-2: var(--text-1);
    --focus-ring: 0 0 0 2px var(--ground), 0 0 0 5px var(--accent);
  }
}

@media (prefers-reduced-transparency: reduce), (prefers-contrast: more) {
  .account-login-page {
    background: none;
  }
}

html[data-surfaces="solid"] .account-login-page {
  background: none;
}

@media (forced-colors: active) {
  .btn,
  .form-control,
  .form-select,
  .card,
  .stat,
  .form-section,
  .modal-content,
  .action-bar,
  .account-login-card {
    border: 1px solid CanvasText;
    box-shadow: none;
    -webkit-backdrop-filter: none;
    backdrop-filter: none;
  }

  :focus-visible {
    outline: 2px solid Highlight;
  }
}

@media (prefers-reduced-motion: reduce) {
  .btn:active {
    transform: none;
  }
}

/* Modal sheet entrance: short spring, then settle; the backdrop fades. */
.modal.fade {
  transition: opacity var(--dur-base) var(--ease-standard);
}

.modal.fade .modal-dialog {
  transform: translateY(12px) scale(0.98);
  transition: transform var(--dur-sheet) var(--ease-spring);
}

.modal.show .modal-dialog {
  transform: none;
}

.modal-backdrop.fade {
  transition: opacity var(--dur-slow) var(--ease-out);
}
/* END motion-v1 */
```
## Final recommendation for the first implementation milestone
Before any code: the owner captures the baseline screenshots (R19) and answers Q-02 (font licence), Q-03 and Q-04 (they gate or shape later steps). Then run Stage 7 as three small commits (Steps 7.1, 7.2, 7.3) plus validation. Stage 8 starts with the two accessibility steps (8.1 label and checkbox names, 8.2 validation linking): independent of the visuals and small enough to review one file group at a time. The riskiest steps to review closely are the CSS migrations 8.3, 8.4, 9.1-9.4, 10.1, 10.5, 11.2 and 12.2.
