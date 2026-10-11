# Audit — Stage 5 plan v4 (Job A, plan review)

## Scope
Reviewed `plan.md` Stage 5 v4 (Steps 5.1–5.7) against the repo. All steps Risk LOW / Confidence HIGH: sanity-checked plus verified the load-bearing claims (reference counts, clearance rules, JS timers, selector texts).

## Findings
- Step 5.1 count verified: `to-do.md` occurs exactly 2/2/1/7/4/1 across AGENTS.md, README.md, chatbot.md, planner.md, reviewer.md, actor.md (17 occurrences, 13 lines); `docs(todo)` contains no hyphen so the `grep -rn "to-do" … --exclude-dir=chat` verify is safe. Claimed v3→v4 corrections all present: Step 5.2 edit 5 clearance rules, Step 5.2 edit 4 reduced-motion `::before` deletion with `grep -c "::before" = 0` verify (all 6 `::before` sites are in rules Step 5.2 deletes), Step 5.1 rename.
- Step 5.2 base rule confirmed (`@media (max-width: 767.98px) … bottom: var(--nav-dock-height, 5rem)` at `components.css:228`; `--nav-dock-height` used only in the padding line after the edit); replacement breakpoints ordered 1199.98px before 575.98px as required. Desktop arithmetic consistent (3.83 + 1.25 = 5.08 rem dock top vs 6.5 rem padding).
- Step 5.5 JS numbers confirmed in `navigation-menu.js`: `}, 720);` (line 62), `}, 480);` (line 122), `--collapse-delay` set (line 111) and removed (lines 70–72); replacements 800 (560 + 230 + margin) / 340 (280 + 60) consistent. CSS side confirmed: keyframes, per-item `animation-delay` 45–230 ms, `collapse-delay` default (line 382), and the `::before` cubic-bezier at line 69 lives inside the deleted `.navigation-dock::before` rule, so the "exactly 3 cubic-bezier lines" verify is achievable.
- Steps 5.3/5.4/5.6 selector claims confirmed: `translateY(-0.12rem/-0.06rem/-1px)` + `rotate(` sites, `outline-offset: 3px`, the three tab `box-shadow: none` lines (indicator's preserved per edge case), mobile flyout-content override, and Profile's `.action-bar` nested inside `section.form-section`.
- LOW: Step 5.6 edge case says "confirm none contains an `action-bar`" for the `<div class="form-section">` pages, but `Sell.cshtml` file-level does contain one (line 42, a sibling after `</form>`, not nested) — read as a nesting check it resolves correctly and the descendant selector leaves it unaffected, but the wording is literally imprecise.

## Status
APPROVED (one LOW wording note above; no CRITICAL/HIGH).

Next: Actor — execute Step 5.1.
