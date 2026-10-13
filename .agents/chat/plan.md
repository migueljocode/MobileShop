# Plan — Stage 6 — Navigation dock: compact when idle or scrolling, pop-up on hover, frosted glass

## Pipeline note
The roadmap file is `.agents/TODO.md`. The Stage 6 line was approved by Reviewer Job A and is added there with this plan.

## Reviewer Briefing
- Job A verdict on the first version: APPROVED WITH CORRECTIONS. Folded in: HIGH (default compact scale 0.88 and the click gate documented), MEDIUM (pre-JS compact rule to remove the load flash; Safari/iOS/Firefox manual check), LOW (settle gate cleared on compaction; `MediaQueryList` guard; step template fields; evidence ownership). Not folded, documented as out of scope: forwarding a click that lands during the gate to the tab aimed at, and narrowing the hover area.
- User request: the main navbar shrinks when the mouse is not over it or while the page scrolls, pops up like a scale when hovered, and has a semi-transparent blurry frosted-glass look.
- This deliberately reverses Stage 5's choice "the dock stays at its natural size and nothing scales on hover", but keeps Stage 5's guarantee (no hover jump). The old cause was a dock growing under a still pointer, so a tab moved and the wrong tab reacted. Stage 6 prevents it with three mechanisms: (1) the hover area is the UNSCALED natural footprint (a `::before` on the wrapper; transforms do not change layout), so growth starts before the pointer reaches the visible dock; (2) tab hover/open reactions are ignored until the dock has reached its size (`is-settling`, 190 ms), then the tab under the pointer is opened explicitly; (3) the pop overshoot is only +0.55% of the size, so tabs move at most about 2 px after the gate lifts.
- Aim rule behind the default scale: while the dock grows, the tab under a still pointer moves inward by (1 - scale) x its distance from the dock centre. That shift must stay below half the narrowest tab (about 43 px), or a pointer aimed at an outer tab ends on its edge or in the gap. At 0.84 the outermost tab was borderline (52 px shift, 51 px half-width, clicks dropped); at 0.88 the worst case is 39 px vs 51 px (12 px margin) and all six tabs keep the aimed tab under the pointer. Do not lower the default without re-checking this.
- Click gate (intended): for about 190 ms after the dock starts to expand, tabs ignore the pointer, so a click in that window is dropped; clicks from 200 ms on navigate (measured).
- Defaults chosen (tell the user, each is one value): compact scale `0.88`; shrink at rest only for fine pointers (on touch devices the dock shrinks only while scrolling); an open flyout or keyboard focus inside keeps it expanded; reduced-motion users never get the shrink.
- Glass was tuned by measurement, not taste: 62% tint with blur 18 gave muted-label contrast 2.8:1 over bright content; the chosen setting (78% tint, blur 20, saturate 1.5, brightness 0.6) gives 5.0:1 over realistic content and 4.8:1 over a full-width teal strip. It reads as smoked glass; more see-through costs contrast.
- Evidence base: every change below was applied in order to the real post-Stage-5 `navigation-menu.css` / `navigation-menu.js` and measured in headless Chromium; the corrected build also passed 24 randomized pointer/scroll/keyboard/focus runs with the dock always ending in the right state. Only Chromium could be measured: Safari/iOS (blur + `scale` + fixed positioning) and Firefox need the manual check in Step 6.4.
- Known limits (documented, not defects): after a full page load with the pointer stationary over the dock, the dock starts compact (the browser reports no hover yet) and expands at the first pointer move; over an all-white full-width strip behind the dock the muted-label contrast is 4.3:1 (the dark theme has no such area); the compact dock blocks the page across its natural footprint (about 21% more area than it shows).
- Stage 5 invariants that Stage 6 intentionally changes: `::before` count 0 -> 2 lines; `is-initializing` returns as CSS rules; the dock gains a `scale` transition. Everything else from Stage 5 (spring tokens, real-height flyout, focus ring, per-property delays, no keyframes) must still hold.
- Risk LOW-MEDIUM (a small JS state machine with four inputs: hover, focus, scroll, open flyout), confidence HIGH.

## Hard constraints
- Follow `AGENTS.md` + `.agents/project.md`. Touch only `src/MobileShop.Web/wwwroot/css/navigation-menu.css` and `src/MobileShop.Web/wwwroot/js/navigation-menu.js`. Do not touch `Default.cshtml`, other CSS/JS or `tokens.css`; no new dependency; no hex colour literals.
- One step per pass and per commit. Edit existing rules, no commented-out code.
- Keep every class and data attribute the JS uses (`is-engaged`, `is-open`, `is-closing`, `is-expanded`, `is-current`, `is-switching`, `is-entering`, `data-navigation-*`) and all Stage 5 behaviour.

## ~~[x] Step 6.1 — Frosted-glass dock (CSS only)~~
- Files: modify: `navigation-menu.css`.
- Symbols: `.floating-navigation` (custom properties), `.navigation-dock`, new `@supports not (backdrop-filter)` and `prefers-reduced-transparency` blocks.
- Current -> Desired: the dock is an opaque `var(--panel-raised)` panel with a `--line-strong` border. Desired: a translucent, blurred glass panel with solid fallbacks.
- Change:
  1. In the `.floating-navigation` rule, directly after the `--nav-flyout-h: …;` line, add:
     `--nav-glass-bg: color-mix(in srgb, var(--panel-raised) 78%, transparent);`
     `--nav-glass-border: color-mix(in srgb, var(--ink) 16%, transparent);`
     `--nav-glass-filter: blur(20px) saturate(1.5) brightness(0.6);`
  2. In the `.navigation-dock` rule: `border: 1px solid var(--line-strong);` -> `border: 1px solid var(--nav-glass-border);`; `background: var(--panel-raised);` -> `background: var(--nav-glass-bg);` followed by two new lines `-webkit-backdrop-filter: var(--nav-glass-filter);` and `backdrop-filter: var(--nav-glass-filter);`; `box-shadow: 0 1rem 2rem rgb(0 0 0 / .45);` -> `box-shadow: 0 1rem 2rem rgb(0 0 0 / .45), inset 0 1px 0 color-mix(in srgb, var(--ink) 10%, transparent);`.
  3. Directly after the `.navigation-dock` rule add:
     ```css
     @supports not ((-webkit-backdrop-filter: blur(1px)) or (backdrop-filter: blur(1px))) {
       .navigation-dock { background: var(--panel-raised); }
     }

     @media (prefers-reduced-transparency: reduce) {
       .navigation-dock {
         background: var(--panel-raised);
         -webkit-backdrop-filter: none;
         backdrop-filter: none;
       }
     }
     ```
- Depends on: none.
- Edge cases: the flyout and links sit inside the dock, so the whole panel is glass; links keep their own opaque backgrounds. The brightness filter is what keeps labels readable over bright content; do not remove it to make the glass "clearer".
- Tests: none.
- Verify: CI green (cite run). `grep -c "var(--nav-glass-" navigation-menu.css` = 4; no hex literal in the file (`grep -n "#[0-9A-Fa-f]\{3,6\}\b"` = none); `grep -c "::before"` = 0; `git diff --stat` = `navigation-menu.css` only.
- Done when: the dock is translucent with blur, solid fallbacks exist, CI green.
- Risk: LOW
- Confidence: HIGH

## ~~[x] Step 6.2 — Compact / pop-up motion, hover area and load state (CSS only; inert until Step 6.3)~~
- Files: modify: `navigation-menu.css`.
- Symbols: `.floating-navigation` (custom properties), `.floating-navigation::before`, `.navigation-dock`, new `.floating-navigation.is-compact .navigation-dock`, `.floating-navigation.is-initializing …`, `.floating-navigation.is-settling .navigation-tab`, the `prefers-reduced-motion` selector list.
- Current -> Desired: the dock never changes size. Desired: it can render at `--nav-compact-scale` and spring back to full size, its hover area stays the full natural footprint, and it renders compact from the first paint on fine pointers.
- Change:
  1. In the `.floating-navigation` rule, directly after the glass tokens add `--nav-compact-scale: 0.88;` and `--nav-spring-pop: linear(0, 0.053, 0.179, 0.336, 0.497, 0.644, 0.769, 0.867, 0.94, 0.99, 1.022, 1.039, 1.046, 1.045, 1.04, 1.033, 1.026, 1.019, 1.013, 1.008, 1.004, 1.001, 0.999, 0.998, 0.998, 1);` (damped spring, ratio 0.7: the dock overshoots its natural size by only 0.55%).
  2. The existing one-line `@supports not (transition-timing-function: linear(0, 1)) { … }` gains `--nav-spring-pop: cubic-bezier(0.34, 1.56, 0.64, 1);` inside its braces. Directly after that line add:
     ```css
     .floating-navigation::before {
       position: absolute;
       inset: 0;
       content: "";
       pointer-events: auto;
     }

     @media (hover: none), (pointer: coarse) {
       .floating-navigation::before { display: none; }
     }
     ```
     (The wrapper is never scaled, so this box is the dock's NATURAL footprint: it is the hover area while the visible dock is compact.)
  3. In the `.navigation-dock` rule, after `pointer-events: auto;` add `transform-origin: center bottom;` and `transition: scale 520ms var(--nav-spring-pop);`.
  4. Directly after the `.navigation-dock` rule (before the `@supports not ((-webkit-backdrop-filter…` block from Step 6.1) add:
     ```css
     .floating-navigation.is-compact .navigation-dock {
       scale: var(--nav-compact-scale);
       transition: scale 360ms var(--nav-ease-in-out);
     }

     .floating-navigation.is-initializing .navigation-dock {
       transition: none;
     }

     @media (hover: hover) and (pointer: fine) and (prefers-reduced-motion: no-preference) {
       .floating-navigation.is-initializing:not(:hover) .navigation-dock { scale: var(--nav-compact-scale); }
     }

     .floating-navigation.is-settling .navigation-tab {
       pointer-events: none;
     }
     ```
     (The `is-initializing` media rule makes the dock compact from the first paint: the nav script is the last parser-blocking script, so without it a slow load shows the full-size dock until the JS runs.)
  5. In the `prefers-reduced-motion` selector list add `.navigation-dock,` directly before `.navigation-dock__tabs,`.
- Depends on: Step 6.1.
- Edge cases: use the independent `scale` property, not `transform` (the wrapper's `translateX(-50%)` and everything else stay untouched). Expanding uses the spring, shrinking the calm ease-in-out (the more specific `.is-compact` rule overrides the transition for that direction). `is-initializing` is in the server markup and removed by the JS; without JS a fine-pointer desktop dock stays compact, which is acceptable. No JS adds `is-compact` yet, so after this step the only visible change is the compact first paint.
- Tests: none.
- Verify: CI green. `grep -c "::before" navigation-menu.css` = 2 (the hit-area rule and its touch override); the rules `.is-compact .navigation-dock`, `.is-initializing .navigation-dock`, `.is-settling .navigation-tab` each appear once and `is-initializing:not(:hover)` once; `grep -c "transition: scale 520ms var(--nav-spring-pop)"` = 1; `grep -n "cubic-bezier" navigation-menu.css` = exactly 3 lines; `grep -c "@keyframes\|animation"` = 0; `linear(` appears on exactly 3 lines (spring, pop, `@supports` condition).
- Done when: the rules exist, nothing else changed, CI green.
- Risk: LOW
- Confidence: HIGH

## ~~[x] Step 6.3 — State machine (JS)~~
- Files: modify: `wwwroot/js/navigation-menu.js`.
- Symbols: new `wantsCompact`, `applyCompact`, `scheduleCompact`; existing `open`, `finishClose`, the trigger `mouseenter` handler, the `pointerenter`, `pointerleave` and `focusout` handlers; new `focusin`, `pointermove`, `scroll` and `MediaQueryList` listeners; the end of the DOMContentLoaded callback.
- Current -> Desired: the JS only manages the flyout. Desired: it also decides when the dock is compact (hover, focus, scroll, open flyout), gates tab reactions while the dock expands, and removes `is-initializing` after the first two frames.
- Change (exact; all inside the existing DOMContentLoaded callback):
  1. Replace the line `if (!suppressInitialHover) navigation.classList.remove('is-initializing');` with:
     ```js
     var reduceMotion = window.matchMedia('(prefers-reduced-motion: reduce)');
     var finePointer = window.matchMedia('(hover: hover) and (pointer: fine)');
     var compactTimer;
     var scrollTimer;
     var settleTimer;
     var scrolling = false;
     var lastX = 0;
     var lastY = 0;

     function wantsCompact() {
         if (reduceMotion.matches || openedId !== null || navigation.classList.contains('is-engaged')) return false;
         if (navigation.contains(document.activeElement)) return false;
         if (scrolling) return true;
         return finePointer.matches && !navigation.matches(':hover');
     }

     function applyCompact() {
         var compact = wantsCompact();
         if (compact === navigation.classList.contains('is-compact')) return;
         navigation.classList.toggle('is-compact', compact);
         if (compact) {
             window.clearTimeout(settleTimer);
             navigation.classList.remove('is-settling');
             return;
         }
         navigation.classList.add('is-settling');
         window.clearTimeout(settleTimer);
         settleTimer = window.setTimeout(function () {
             navigation.classList.remove('is-settling');
             var element = document.elementFromPoint(lastX, lastY);
             var trigger = element && element.closest('[data-navigation-trigger]');
             if (trigger && navigation.matches(':hover')) open(trigger.dataset.navigationTrigger);
         }, 190);
     }

     function scheduleCompact(delay) {
         window.clearTimeout(compactTimer);
         compactTimer = window.setTimeout(applyCompact, delay);
     }
     ```
  2. In `open()`, directly after `navigation.classList.add('is-engaged');` add `applyCompact();`.
  3. In `finishClose()`, directly after `navigation.classList.remove('is-engaged');` add `scheduleCompact(160);`.
  4. In the trigger `mouseenter` handler change `if (suppressInitialHover) return;` to `if (suppressInitialHover || navigation.classList.contains('is-settling')) return;` (the CSS gate only applies after the next style recalculation, so this guard is needed for a pointer that lands directly on a tab).
  5. In the `pointerenter` handler add, after `window.clearTimeout(closeTimer);`: `window.clearTimeout(compactTimer);` and `applyCompact();`.
  6. In the `pointerleave` handler add `scheduleCompact(160);` after `scheduleClose();`.
  7. Rewrite the `focusout` handler's body as `if (!navigation.contains(event.relatedTarget)) { scheduleClose(); scheduleCompact(160); }`.
  8. Directly before `document.addEventListener('pointerdown', …` add:
     ```js
     navigation.addEventListener('focusin', applyCompact);
     navigation.addEventListener('pointermove', function (event) {
         lastX = event.clientX;
         lastY = event.clientY;
         if (navigation.classList.contains('is-compact')) applyCompact();
     }, { passive: true });
     window.addEventListener('scroll', function () {
         scrolling = true;
         applyCompact();
         window.clearTimeout(scrollTimer);
         scrollTimer = window.setTimeout(function () {
             scrolling = false;
             applyCompact();
         }, 450);
     }, { passive: true });
     if (reduceMotion.addEventListener) reduceMotion.addEventListener('change', applyCompact);
     ```
  9. As the last statements of the callback (after the `pointerdown` listener) add:
     ```js
     applyCompact();
     window.requestAnimationFrame(function () {
         window.requestAnimationFrame(function () {
             navigation.classList.remove('is-initializing');
         });
     });
     ```
- Depends on: Steps 6.1-6.2.
- Edge cases:
  - The settle gate is cleared when the dock compacts, so it never lasts longer than one expansion (190 ms). Do not lower 190 ms: the spring needs 175-197 ms to reach 99% of its size.
  - `is-initializing` stays on for the first two frames so the first compact state is applied without animation. After a real page navigation the first painted frame is already compact (measured, also with the script delayed 450 ms).
  - Scrolling with the pointer over a tab leaves the dock expanded, because the open flyout keeps it engaged (by design).
  - A click that lands on a tab within the 190 ms gate is dropped on purpose (tabs are `pointer-events: none` then); from 200 ms on it navigates. Out of scope: forwarding such a click to the tab aimed at.
- Tests: none (no JS test infrastructure in the repo).
- Verify: CI green. `node --check navigation-menu.js` passes if Node is available; `grep -o "applyCompact" navigation-menu.js | wc -l` = 10; `grep -o "scheduleCompact" … | wc -l` = 4; `grep -c "'is-compact'"` = 3 lines, `'is-settling'` = 4 lines, `'is-initializing'` = 2 lines; the `scroll` listener is registered with `{ passive: true }`; `git diff --stat` = `navigation-menu.js` only.
- Done when: the dock is compact at rest on fine pointers, expands on hover/focus, shrinks on scroll, CI green.
- Risk: MEDIUM
- Confidence: HIGH

## ~~[x] Step 6.4 — Stage 6 validation~~
- Files: none modified.
- Change: confirm CI green on the last code commit and record `Action: #<run>` per code step in act.md; record the greps from Steps 6.1-6.3; `git diff --stat` over the stage is exactly `navigation-menu.css` and `navigation-menu.js` (+ `.agents/` state files). Browser measurements are optional (the repo has no browser harness): the Reviewer runs them only when the user asks for a deep check, with the numbers below.
- Deep-check numbers (headless Chromium with a fine hover-capable pointer, from the verified build):
  - Rest, pointer away: `is-compact`, dock `scale` 0.88, visible height 53.9 px vs the unchanged natural box 61.3 px.
  - Pointer enters the hover area (left padding or top edge): `is-compact` removed within 7 ms; scale reaches 0.99 at 173-197 ms and peaks at 1.0055; `is-settling` ends at 178-198 ms; no tab opens before it ends; afterwards the far-edge tab moves at most 2.1 px.
  - Pointer jumps straight onto the first, a middle and the last compact tab: expanded sequence is `[none, tab under the pointer]`, first open after the gate lifts, no hover style during the gate. For every one of the six tabs the aimed tab is still under the pointer after the pop (worst offset 39 px vs 51 px half-width).
  - Clicks: a click within 150 ms of landing is dropped; from 200 ms on it navigates (middle and far tab).
  - Pointer leaves: compact ≥ 160 ms after the flyout is fully closed, never while `is-engaged`.
  - Scroll: pointer away: compact during and after; pointer on the dock padding plus wheel: compact during, expanded ≤ 750 ms after the last scroll event; with a flyout open: stays expanded.
  - Keyboard `Tab` into the nav: expanded immediately. Reduced motion: never compact. Touch (`hover: none`, coarse): not compact at rest, compact while scrolling, expanded after idle, hover area `display: none`.
  - Hit area: a point 6 px inside the natural box edge hits the wrapper; points 4 px outside the left and top edges hit the page.
  - Glass: computed `backdrop-filter` `blur(20px) saturate(1.5) brightness(0.6)`, background alpha 0.78; minimum `--ink-muted` label contrast ≥ 5.0:1 over realistic content (teal button plus white text) and ≥ 4.8:1 over a full-width teal strip; `--ink` ≥ 9.6:1.
  - Page load: after a real navigation the first painted frame is already compact, also with the nav script delayed 450 ms.
  - Stage 5 invariants still hold: open curve 80 / 95 / 99 / 100% at 25 / 50 / 75 / 90% of 460 ms, close 84 / 22 / 3%, link delay `0s, 0s, 0.082s, 0.082s`, focus ring on all 6 tabs, rapid switching and re-open during close end clean; the natural box and the sticky-bar clearance are unchanged. 24 random pointer/scroll/keyboard/focus runs end in the correct state.
- Manual check (user): move onto the dock from above, from the sides and straight onto a tab: it pops up smoothly and only then highlights the tab you ended on; flick onto the far-right tab and click (a click in the first fifth of a second is ignored on purpose, later ones work); move away: it shrinks after about a fifth of a second; scroll with the wheel and with a trackpad: it shrinks and pops back up when you stop and are still over it; look at the glass over a table, a teal button and the dashboard cards; keyboard Tab; reload and switch pages, also with the browser's network throttled (Slow 3G): no full-size dock flash; a phone or tablet: shrinks only while scrolling; OS reduce-motion and reduce-transparency on. **Also check Safari/iOS and Firefox** (blur, scale and fixed positioning could not be tested here).
- Done when: evidence recorded.
- Risk: LOW
- Confidence: HIGH

## Global Definition of Done
- The dock is a frosted-glass panel; compact when idle (fine pointers) or while the page scrolls; springs to full size on hover, focus or an open flyout.
- Hovering never makes the wrong tab react: reactions wait until the dock has reached its size, then the tab under the pointer opens; the aimed tab stays under the pointer for all six tabs.
- No full-size flash on page load; all Stage 5 behaviour still holds; the unscaled natural footprint (and with it the sticky-bar clearance) is unchanged.
- Only `navigation-menu.css` and `navigation-menu.js` changed; CI green on the final code commit with run numbers in act.md; the Safari/iOS and Firefox check done by the user; no unresolved CRITICAL/HIGH finding.
