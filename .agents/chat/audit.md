# Reviewer Audit — Job A: Stage 6 plan (compact / pop-up / frosted-glass dock)

Method: plan applied literally in step order to the real files (identical to a measured prototype), then measured in headless Chromium: clicks, aim, load flash, 26 fuzz runs. Harness fonts are wider than Segoe UI, so widths are overstated.

## Findings

### HIGH — First click on the dock is unreliable at the default compact scale
- Location: Step 6.2 `--nav-compact-scale: 0.84`, Step 6.3 settle gate, Step 6.4 acceptance.
- Evidence: (a) tabs ignore the pointer for 190 ms after it lands (`pointer-events: none`), so a click in that window lands on the dock and is dropped: clicks at 40 / 100 / 150 ms never navigated. (b) While the dock grows, the tab under the pointer moves inward by (1 − scale) × its distance from the centre. At 0.84 the aim point of the outermost tab ends 52 px from its centre in a tab with a 51 px half-width, and clicks at 260 and 400 ms landed on the gap and did nothing. At 0.88 the worst margin is 12 px (39 vs 51); at 0.90 it is 19 px. The shop PCs use Segoe UI, narrower than my fallback font, which helps but leaves the margin thin.
- Fix: default `--nav-compact-scale: 0.88`, and state the rule "(1 − scale) × half the dock width must stay below half the narrowest tab (43 px)". Update the acceptance number (rest height 53.9 px instead of 51.5 px). Document in Step 6.3/6.4 that a click within ~190 ms of the dock expanding is intentionally ignored, and add a manual check: flick onto the far-right tab and click. Optional: forward a click that lands during the gate to the tab aimed at on entry.

### MEDIUM
- A full-size dock flashes on slow loads. The nav script is the last parser-blocking script (after jQuery, Bootstrap, site.js). With it delayed 450 ms, the dock painted full size for 430 ms, then snapped compact. Fix, measured: add to Step 6.2, after the `.is-initializing` rule, `@media (hover: hover) and (pointer: fine) and (prefers-reduced-motion: no-preference) { .floating-navigation.is-initializing:not(:hover) .navigation-dock { scale: var(--nav-compact-scale); } }`. The dock is then compact from the first frame and nothing changes when the JS runs. Without JS it stays compact, which is acceptable.
- Only Chromium was measured. WebKit (Safari/iOS: `backdrop-filter` + `scale` + fixed positioning) and Firefox were not testable here. Add a Safari/iOS and Firefox manual check to Step 6.4 and say so in the Briefing.

### LOW
- `is-settling` is not bounded by 190 ms: when the pointer flaps in and out, the timer is re-armed without being cleared on compaction. Fuzzing found it lasting 267–383 ms in 5 runs. Validated fix (0 failures on the same seeds): in `applyCompact`, when `compact` is true, call `window.clearTimeout(settleTimer); navigation.classList.remove('is-settling');` before returning.
- The compact dock blocks 28.6% more area than it shows (about 61 px each side at the bottom of the viewport). Clicking page content beside it expands the dock instead. Acceptable; narrow `::before` to `inset: 0 1.5rem` if it annoys.
- `MediaQueryList.addEventListener` does not exist on Safari ≤ 13, and the TypeError would stop the whole menu script. Guard with `if (reduceMotion.addEventListener)`.
- Template: steps use "Edits" and omit `Symbols` and `Current -> Desired` (planner.md). Step 6.4 assigns browser measurements to the Reviewer, while reviewer.md says re-run nothing heavy and sign off from recorded evidence; the repo has no browser harness. Make the Actor's evidence CI + greps + the user's manual checks, and mark browser numbers as an optional deep check.

## Verified (evidence)
- Every target string matches once and each step's Verify passes (`::before` 0 then 2, `applyCompact` 10, `scheduleCompact` 4, `cubic-bezier` 3 lines, `linear(` 3 lines). After Step 6.2 alone (old JS) all Stage 5 measurements hold.
- Compact at rest (0.84, 51.5 px vs natural 61.3 px); entering the hover area expands within 6 ms, reaches 99% at 173–183 ms, peaks at 1.0074; the gate ends at 190–200 ms; the far tab moves ≤ 2.75 px afterwards. Landing straight on the first, a middle and the last tab opens only the tab under the pointer, after the gate, with no hover style during it.
- Leaving compacts 170 ms after the flyout closes; scroll compacts and re-expands ≤ 750 ms after idle; an open flyout and keyboard focus stay expanded; reduced motion never compacts; touch compacts only while scrolling and the hover area is hidden; a point 4 px outside the box hits the page.
- Glass contrast (final setting): muted label 5.0:1 realistic / 4.8:1 teal strip / 4.3:1 white strip; ink ≥ 9.6:1. The weaker 62% tint gave 2.8:1.
- Stage 5 invariants still hold (open 80/95/99/100, close 84/22/3, link delays, focus ring ×6, rapid switching).
- The Proposed Stage 6 is a single, correctly scoped stage; approved.

## Status
**APPROVED WITH CORRECTIONS**

## Gate
Next: Actor — Step 6.1. The HIGH, both MEDIUM and the cheap LOW fixes are already folded into `plan.md` (commit 9428b1d) and the Stage 6 line is in `.agents/TODO.md`; a second Job A is not needed.
