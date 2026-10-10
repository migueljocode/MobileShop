# Plan — Stage 5 — Navigation dock: iOS-style motion without glitches; dock clearance; Profile "Save changes" bar; TODO.md rename in rules

## Pipeline file rename notice
`.agents/to-do.md` was renamed to `.agents/TODO.md` (commits b1e357f, f7a0c73). The rule files still say `to-do.md` (17 references in 6 files) and must obey the new name: Step 5.1 fixes them. Until Step 5.1 is committed every role reads and writes `.agents/TODO.md` (exact case); the Stage 5 line is added there, not to `to-do.md`.

## Reviewer Briefing
- Job A verdict on v3: APPROVED WITH CORRECTIONS. The HIGH finding (the larger resting dock no longer clears the sticky action bars) is Step 5.2 edit 5; the MEDIUM verify fix is Step 5.2 edit 4; the rule-file rename the user asked for is Step 5.1.
- Evidence base: every CSS/JS edit was applied in step order to a copy of the repo's `navigation-menu.css` / `navigation-menu.js` (each target string matched exactly once) and measured in headless Chromium against the real nav markup; the clearance fix was measured too. Results are the acceptance numbers in Step 5.7.
- Scope: `src/MobileShop.Web/wwwroot/css/navigation-menu.css`, `wwwroot/js/navigation-menu.js` (two numbers + two deleted blocks), `wwwroot/css/components.css` (clearance media rules, Profile rule), `Pages/Account/Profile.cshtml` (whitespace only), and the rule docs in Step 5.1. No other markup, no C#.
- Root causes confirmed by measurement on the current code: (a) dock radius `999px` -> `1.45rem` is transitioned (clamped, so it snaps late) and the dock scales 0.84 -> 1 with an overshoot curve; (b) the dock grows under a still pointer, so a hovered tab moved 11 px vertically and 17 px horizontally, and `.navigation-tab:hover` lifts itself `translateY(-0.12rem)` (a pointer on a tab's bottom edge kept hover for 12% of frames); (c) flyout `max-height` targets a fixed `24rem` but the real height is 19.4-24 rem, so visible growth finished in 110-190 ms of the declared 460 ms and hard-stopped; open and close shared one ease-out; (d) keyframe animations restart abruptly when interrupted; (e) keyboard focus ring never shows on nav tabs (`box-shadow: none` rules later in the file at equal specificity).
- Design rules that prevent those glitches: layout properties (`max-height`, `padding`) never overshoot and animate to the REAL height; the gentle spring (3.8% overshoot) drives only `transform`/`width` of non-hover-target elements (icon, indicator, flyout content, panel items, press feedback); hover targets never translate or scale up on hover; interrupting any open/close/switch reverses smoothly (transitions, not keyframes).
- Intentional visual change (tell the user): the dock stays at its natural size (no 84% -> 100% "breathing"), which makes the resting dock 19% larger in each dimension (height 51.5 -> 61.3 px desktop, 48.3 -> 57.5 px mobile). Step 5.2 re-derives the space it occupies so sticky bars and page padding still clear it.
- Risk LOW, confidence HIGH.

## Hard constraints
- Follow `AGENTS.md` + `.agents/project.md`. Touch only the files each step lists. Do not touch `Default.cshtml`, other CSS/JS, `tokens.css`, colours, dock position/width/z-index.
- Keep every class and data attribute the JS uses (`is-engaged`, `is-open`, `is-closing`, `is-expanded`, `is-current`, `is-switching`, `is-entering`, all `data-navigation-*`). The JS still toggles `is-initializing`; after Step 5.2 no CSS uses it. Leave that JS as is.
- Edit existing rules and delete dead ones (no commented-out code). No new dependency.
- Step 5.1 is a user-requested docs change, not an "unrelated rename".

## [ ] Step 5.1 — Rule files obey `TODO.md`
- Files: modify: `AGENTS.md` (2 references), `.agents/README.md` (2), `.agents/chatbot.md` (1), `.agents/roles/planner.md` (7), `.agents/roles/reviewer.md` (4), `.agents/roles/actor.md` (1); do not touch: `.agents/TODO.md`, `.agents/chat/*` (state files; `audit.md` is overwritten by the Reviewer), any other file.
- Current -> Desired: all 17 references to the pipeline roadmap file read `to-do.md`; the file is now `TODO.md`. Notably `reviewer.md` tells the Reviewer to run `git add .agents/to-do.md && git commit … -- .agents/to-do.md`, which now fails (the old path no longer exists), and on case-sensitive systems a model following the rules would recreate `to-do.md`.
- Change: in those six files replace every `to-do.md` with `TODO.md` (exact case, `.agents/to-do.md` -> `.agents/TODO.md`, `to-do.md` -> `TODO.md`). Every one of the 17 occurrences is the file name; change nothing else (no rewording, no reflow).
- Depends on: none.
- Edge cases: commit message/step text inside the rules like `docs(todo): complete Stage N` stay as they are.
- Tests: none.
- Verify: no CI run is triggered (docs-only, `paths-ignore`): report `Action: unavailable`. `grep -rn "to-do" AGENTS.md .agents --include=*.md --exclude-dir=chat` = none. `grep -o "TODO.md"` per file = 2 / 2 / 1 / 7 / 4 / 1 (AGENTS.md, README.md, chatbot.md, planner.md, reviewer.md, actor.md). `git diff --word-diff=porcelain -U0 | grep '^[-+][^-+]'` lists only the `to-do.md` -> `TODO.md` token pairs. `git diff --stat` = the six files.
- Done when: the rules name `TODO.md` everywhere and nothing else changed.
- Risk: LOW
- Confidence: HIGH

## [ ] Step 5.2 — Stable dock shape and size, and clearance for the larger dock
- Files: modify: `navigation-menu.css`, `components.css`.
- Edits:
  1. In the `.floating-navigation` rule add `--nav-dock-radius: 1.9rem;`. In the existing mobile (`max-width: 575.98px`) `.floating-navigation` rule add `--nav-dock-radius: 2.05rem;` (measured resting dock heights: 61.3 px desktop = 30.65 px half, 57.5 px mobile = 28.7 px half).
  2. `.navigation-dock`: `border-radius: 999px;` -> `border-radius: var(--nav-dock-radius);` delete `transform: scale(0.84);`, `transform-origin: center bottom;` and the whole `transition:` declaration (nothing transitions on the dock any more).
  3. Delete these rules entirely: `.floating-navigation.is-initializing .navigation-dock`, `.floating-navigation.is-initializing:hover .navigation-dock`, `.floating-navigation.is-initializing:hover .navigation-dock::before`, `.navigation-dock::before`, the rule that starts `.floating-navigation:hover .navigation-dock::before,` (opacity .55), the rule that starts `.floating-navigation:hover .navigation-dock,` (transform/box-shadow/radius), the `@supports not ((-webkit-backdrop-filter: blur(1px)) or (backdrop-filter: blur(1px)))` block, and, inside the mobile media block, `.floating-navigation.is-engaged .navigation-dock { width; max-width }` (identical to the `.navigation-dock` rule right above it).
  4. In the `prefers-reduced-motion` selector list delete the `.navigation-dock::before,` line (Step 5.5 rewrites the rest of that block).
  5. Clearance for the larger resting dock (Stage 4's sticky-bar offset only applied below 768 px and was sized for the scaled-down dock):
     - `components.css`: replace the rule `@media (max-width: 767.98px) { body.has-floating-navigation .action-bar { bottom: var(--nav-dock-height, 5rem); } }` with `@media (max-width: 1199.98px) { body.has-floating-navigation .action-bar { bottom: 5.5rem; } }` followed by `@media (max-width: 575.98px) { body.has-floating-navigation .action-bar { bottom: calc(max(0.65rem, env(safe-area-inset-bottom, 0px)) + 4.6rem); } }`. Leave `.has-floating-navigation { padding-bottom: calc(var(--nav-dock-height, 5rem) + 1rem); }` and the `--nav-dock-height` token untouched (the nav stylesheet's more specific `body.has-floating-navigation` rule overrides that padding; the token is now only used there).
     - `navigation-menu.css`, mobile block: `body.has-floating-navigation { padding-bottom: 5.75rem; }` -> `padding-bottom: calc(max(0.65rem, env(safe-area-inset-bottom, 0px)) + 5.2rem);`. Desktop `6.5rem` stays (dock top is 5.08 rem from the bottom).
- Depends on: none.
- Edge cases: the media rule order matters (the 575.98 px rule must come after the 1199.98 px rule so it wins on phones). Reasons for the numbers: wrapper bottom is `1.25rem` from 576 px up and `max(0.65rem, inset)` below; dock height 3.83 rem desktop / 4.11 rem mobile.
- Tests: none.
- Verify: CI green (cite run). `grep -c "::before" navigation-menu.css` = 0; `grep -n "scale(0.84)\|is-initializing\|@supports" navigation-menu.css` = none; the `.navigation-dock` rule has `border-radius: var(--nav-dock-radius)` and no `transition`. `grep -n "nav-dock-height" components.css` = only the `.has-floating-navigation` padding line; `grep -n "1199.98px\|575.98px" components.css` shows the two new media rules. `git diff --stat` = the two CSS files.
- Done when: radius and size never change on hover/focus/open, sticky bars and page padding clear the dock, CI green.
- Risk: LOW
- Confidence: HIGH

## [ ] Step 5.3 — Motion tokens, hover that never moves a hit box, visible focus ring
- Files: modify: `navigation-menu.css`.
- Edits:
  1. In the `.floating-navigation` rule add: `--nav-spring: linear(0, 0.044, 0.149, 0.284, 0.428, 0.565, 0.687, 0.789, 0.871, 0.933, 0.977, 1.007, 1.026, 1.035, 1.038, 1.037, 1.033, 1.028, 1.023, 1.018, 1.013, 1.009, 1.005, 1.003, 1.001, 1);` (damped spring, ratio 0.72, peak overshoot 3.8%), `--nav-ease-out: cubic-bezier(0.32, 0.72, 0, 1);` (iOS standard), `--nav-ease-in-out: cubic-bezier(0.4, 0, 0.2, 1);`. Directly after that rule add `@supports not (transition-timing-function: linear(0, 1)) { .floating-navigation { --nav-spring: cubic-bezier(0.3, 1.25, 0.5, 1); } }`.
  2. `.navigation-tab` `transition:` -> `color 200ms ease, background-color 240ms ease, box-shadow 200ms ease, transform 420ms var(--nav-spring);`.
  3. `.navigation-tab:hover`: delete `transform: translateY(-0.12rem);`. `.navigation-tab:active`: `transform: scale(0.95);`.
  4. `.navigation-tab.is-expanded`: delete `box-shadow: none;` and `transform: translateY(-0.12rem);`. `.navigation-tab.is-current` and `.navigation-tab.is-current:hover`: delete `box-shadow: none;`. (These `box-shadow: none` lines are redundant and are what hides the focus ring.)
  5. `.navigation-tab:focus-visible, .navigation-link:focus-visible`: delete `outline-offset: 3px;`.
  6. `.navigation-tab__icon` `transition:` -> `transform 560ms var(--nav-spring);`. Hover/focus-visible rule -> `transform: scale(1.1);` (delete `filter: none;` and `rotate(-3deg)`).
  7. `.navigation-tab__indicator`: add `transition: width 560ms var(--nav-spring), background-color 200ms ease;` (absolutely positioned and centred, so it never moves the tab).
  8. `.navigation-link` `transition:` -> `background-color 160ms ease, border-color 160ms ease, opacity 200ms ease, transform 560ms var(--nav-spring);`. In `.navigation-link:hover, .navigation-link.is-current` delete `box-shadow: none;` and `transform: translateY(-1px);`. Add after it: `.navigation-link:active { transform: scale(0.985); }`.
  9. Delete the no-op rule `.navigation-link + .navigation-link`.
  10. In the mobile block delete `.navigation-tab:hover { transform: translateY(-0.06rem); }` and `.navigation-tab.is-expanded { transform: translateY(-0.06rem); }`.
- Depends on: Step 5.2.
- Edge cases: leave `.navigation-tab.is-expanded .navigation-tab__indicator { box-shadow: none; }` as is (it is the indicator, not the tab).
- Tests: none.
- Verify: CI green. `grep -n "translateY(-0.12rem)\|translateY(-0.06rem)\|translateY(-1px)\|rotate(" navigation-menu.css` = none; the only `transform` inside a `:hover` rule is `.navigation-tab:hover .navigation-tab__icon`; the `linear(` literal appears in the token and in the `@supports` condition only.
- Done when: nothing hovered moves, one spring token, focus ring visible on tabs, CI green.
- Risk: LOW
- Confidence: HIGH

## [ ] Step 5.4 — Flyout animates to its REAL height
- Files: modify: `navigation-menu.css`.
- Edits:
  1. In the `.floating-navigation` rule add `--nav-flyout-h: min(clamp(18rem, 38vh, 22rem), calc(100dvh - 12rem));`. In the mobile `.floating-navigation` rule add `--nav-flyout-h: min(clamp(17rem, 38vh, 20rem), calc(100dvh - 12rem));`.
  2. `.navigation-flyout__content`: `height: var(--nav-flyout-h);` delete its `max-height: calc(100dvh - 12rem);`; add `transform: translateY(0.9rem) scale(0.97); transform-origin: center bottom; transition: transform 280ms var(--nav-ease-in-out);`. New rule after it: `.navigation-flyout.is-open .navigation-flyout__content { transform: none; transition: transform 560ms var(--nav-spring); }`. In the mobile block delete the `.navigation-flyout__content { height…; max-height…; }` rule (the token replaces it).
  3. `.navigation-flyout` (closed) `transition:` -> `max-height 280ms var(--nav-ease-in-out), padding 280ms var(--nav-ease-in-out), opacity 180ms ease 40ms, visibility 0s linear 280ms;`.
  4. `.navigation-flyout.is-open`: `max-height: calc(var(--nav-flyout-h) + 2rem);` (content + 2 x 1rem padding = the real height) and `transition:` -> `max-height 460ms var(--nav-ease-out), padding 460ms var(--nav-ease-out), opacity 220ms ease, visibility 0s;`.
  5. `.navigation-flyout.is-closing` `transition:` -> same value as (3).
- Depends on: Steps 5.2-5.3.
- Edge cases: the JS `transitionend` handler filters on `max-height`; it still fires once per close. Layout properties use only the two non-overshooting curves.
- Tests: none.
- Verify: CI green. `grep -n "24rem" navigation-menu.css` = none; `max-height` appears only as `0`, `0` and `calc(var(--nav-flyout-h) + 2rem)`; `sed -n '/^\.navigation-flyout/,/^}/p' navigation-menu.css | grep -c "cubic-bezier"` = 0.
- Done when: flyout open/close eases to the real height, CI green.
- Risk: LOW
- Confidence: HIGH

## [ ] Step 5.5 — Panel content as interruptible transitions; JS; reduced motion
- Files: modify: `navigation-menu.css`, `wwwroot/js/navigation-menu.js`.
- CSS edits:
  1. `.navigation-panel__heading` `transition:` -> `opacity 200ms ease, transform 560ms var(--nav-spring);`.
  2. `.navigation-panel__links`: delete its `transition: opacity 150ms ease, transform 150ms ease;`; padding `0.2rem 0.5rem 0.5rem 0.2rem` -> `0.3rem 0.5rem 0.5rem 0.3rem` (the 4 px focus ring was clipped by the 3.2 px padding).
  3. Replace the rule `.navigation-panel.is-switching .navigation-panel__heading, .navigation-panel.is-switching .navigation-panel__links { … }` with `.navigation-panel.is-switching .navigation-panel__heading, .navigation-panel.is-switching .navigation-link { opacity: 0; transform: translateY(0.6rem); transition: none; }`.
  4. Replace the two `.is-entering` animation rules and the six `nth-child` `animation-delay` lines with: `.navigation-panel.is-entering .navigation-panel__heading { transition-delay: 35ms; }` and, for n = 1..6, `.navigation-panel.is-entering .navigation-link:nth-child(n) { transition-delay: 0ms, 0ms, <d>, <d>; }` with d = 45, 82, 119, 156, 193, 230 ms. (The four values follow the link's transition list: background-color, border-color, opacity, transform, so hover colours are never delayed.)
  5. Replace the two `.is-closing` animation rules with `.navigation-panel.is-closing .navigation-panel__heading, .navigation-panel.is-closing .navigation-link { opacity: 0; transform: translateY(0.5rem); transition: opacity 160ms ease, transform 280ms var(--nav-ease-in-out); }` (no per-item delay).
  6. Delete the four `@keyframes` blocks.
  7. Replace the whole `@media (prefers-reduced-motion: reduce)` block with one whose selector list is `.navigation-tab, .navigation-tab__icon, .navigation-tab__indicator, .navigation-flyout, .navigation-flyout__content, .navigation-dock__tabs, .navigation-link, .navigation-panel__heading` and declarations `scroll-behavior: auto; transition-duration: 0.01ms !important; transition-delay: 0ms !important;`.
- JS edits (exactly these):
  - `}, 720);` -> `}, 800);` (the `is-entering` removal timer: 560 ms spring + 230 ms max delay).
  - `}, 480);` -> `}, 340);` (`closeCleanupTimer`: 280 ms close + 60 ms).
  - In `close()` delete the block `visiblePanel.querySelectorAll('.navigation-link').forEach(function (link, index, links) { link.style.setProperty('--collapse-delay', …); });`.
  - In `clearPanelCloseState` delete `panel.style.removeProperty('--collapse-delay');` and the `panel.querySelectorAll('.navigation-link').forEach(… removeProperty('--collapse-delay') …)` block, leaving `panel.classList.remove('is-closing');`.
  - (Why: with 13 Products items the old close delays were 291-419 ms, longer than the whole close, so the stagger was never visible.)
- Depends on: Steps 5.2-5.4.
- Edge cases: re-opening during `is-closing` goes through `clearPanelCloseState`; items return from their current state without a snap. Do not add keyframes back.
- Tests: none.
- Verify: CI green. `grep -c "@keyframes\|animation" navigation-menu.css` = 0; `grep -c "collapse-delay" navigation-menu.css navigation-menu.js` = 0 for both; `grep -n "cubic-bezier" navigation-menu.css` = exactly 3 lines (`--nav-ease-out`, `--nav-ease-in-out`, the `@supports` fallback); `git diff navigation-menu.js` = the two numbers and the deleted blocks above, nothing else.
- Done when: no keyframes remain, JS diff matches, CI green.
- Risk: LOW
- Confidence: HIGH

## [ ] Step 5.6 — Profile "Save changes" bar
- Files: inspect: `Pages/Account/Profile.cshtml`, `wwwroot/css/components.css`; modify: `wwwroot/css/components.css`, `Pages/Account/Profile.cshtml` (whitespace only); do not touch: `Profile.cshtml.cs`, any `asp-*`, `data-password-toggle`.
- Current -> Desired: `<div class="action-bar">` sits inside `<section class="form-section">`. Measured: `.action-bar` is `position: sticky`, `background: rgb(21, 28, 35)` (`--panel`), `border-top: 1px`; inside the `--panel-raised` card it draws a darker square strip with a short hairline (the "malformed border"), and on phones it sticks above the dock. Desired: inside a card the bar is plain.
- Edits:
  1. In `components.css`, directly after the `.action-bar` rule add `.form-section .action-bar { position: static; border-top: 0; background: transparent; padding: 0; margin-top: 1.25rem; }` and `@media (max-width: 575.98px) { .form-section .action-bar .btn { flex: 1 1 auto; } }`. Do not change the original `.action-bar` declarations or Step 5.2's media rules.
  2. `Profile.cshtml`: re-indent only (4 spaces per level; the `<dl>`, `<form>`, `@if` and the three inline `<svg>` blocks are over- or mis-indented). No content, attribute or order change.
- Depends on: none.
- Edge cases: only `Profile.cshtml` nests `.action-bar` inside a `<section class="form-section">`. Also grep `<div class="form-section">` pages (People/SellerDetails, CustomerDetails, Products/Details) and confirm none contains an `action-bar`; list the result in act.md. Top-level action bars (Create*, Edit, Sell, People lists) stay sticky and unchanged.
- Tests: none.
- Verify: CI green. `grep -c "asp-"`, `grep -c "data-password-toggle"` (3) and `grep -c "Save changes"` (1) equal before/after on `Profile.cshtml`; `git diff -w Profile.cshtml` is empty; `git diff --stat` = the two files only.
- Done when: the button sits in the card with no strip or border (right-aligned on desktop, full width at 360 px), CI green.
- Risk: LOW
- Confidence: HIGH

## [ ] Step 5.7 — Stage 5 validation
- Files: none modified.
- Change: confirm CI green on the last commit and record `Action: #<run>` per code step in act.md (Step 5.1 is docs-only: `Action: unavailable`); `git diff --stat` over the stage is exactly: the six rule files (Step 5.1), `navigation-menu.css`, `navigation-menu.js`, `components.css`, `Profile.cshtml` (+ `.agents/` state files).
- Reviewer measurements (headless browser, run by the Reviewer; numbers from the verified prototype, ±3 points):
  - Open (scrubbed over 460 ms), height as % of final at 25 / 50 / 75 / 90% of time: 80 / 95 / 99 / 100, identical at 1280×1080, 1280×800, 1280×700, 1280×400, 360×844, 360×640, 360×420; final height equals the `max-height` target.
  - Close (280 ms): % of starting height remaining at 20 / 50 / 80% of time: 84 / 22 / 3.
  - Hover: a hovered tab's rect does not move (0 px range); a pointer on a tab's bottom edge keeps hover for 100% of frames, 0 toggles.
  - Link 2 during `is-entering`: `transition-delay` = `0s, 0s, 0.082s, 0.082s`.
  - Focus: all 6 tabs show the ring when keyboard-focused; a focused flyout link's ring is not clipped (gap ≥ 4 px).
  - Dock: transform `none`; radius 30.4 px (desktop, half height 30.65 px) and 28.7 px (mobile, half height 28.74 px).
  - Close: link opacity 0 when the flyout becomes hidden; rapid tab switching and re-open during close end with all items opaque and no `is-entering`/`is-switching`.
  - Clearance: a stuck `.action-bar` sits 6.7-6.9 px above the dock at 360, 500, 768, 800, 1024 and 1199 px (also with a 34 px safe-area inset) and never overlaps the Save button; mobile page bottom clearance is ≥ 15 px with and without the inset.
- Manual check (user), desktop 1280 px: hover each tab slowly from its bottom and top edge, no jump or flicker; the dock's corners look identical at rest, on hover and open; open/close the flyout repeatedly and switch tabs quickly, soft spring, no snap; press and release a tab, small spring back; keyboard Tab, arrows, Escape (focus ring visible); reload with the pointer already over the dock. Tablet 768-1024 px and mobile 360 px: a long form's sticky Save button stays visible above the dock; tap each tab. Profile at 360 / 768 / 1280 px: the Save button sits cleanly in the card. OS "reduce motion" on: instant.
- Done when: evidence recorded.
- Risk: LOW
- Confidence: HIGH

## Global Definition of Done
- Rule files name `TODO.md` everywhere and nothing else in them changed.
- Dock radius and size constant in every state; no hovered element changes its hit box; layout properties animate to the real height with no overshoot; the single spring token (3.8% overshoot) drives only transform/width of icon, indicator, flyout content, panel items and press feedback.
- No keyframes and no `--collapse-delay` remain; interrupting open/close/switch reverses smoothly; keyboard focus ring is visible on tabs and links.
- Sticky action bars and page bottom padding clear the larger resting dock at every width.
- Profile "Save changes" renders without the sticky strip or border; every other action bar is otherwise unchanged.
- Only the listed files changed; CI green on the final code commit with run numbers in act.md; no unresolved CRITICAL/HIGH finding.
