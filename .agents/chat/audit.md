# Reviewer Audit — Job B: Stage 5 Steps 5.1–5.6 (commit 34aa970)

## Findings

### HIGH — No Stage 5 evidence on record
- `act.md` still holds the Stage 4 final report (`Status: COMPLETE`); there is no Stage 5 report, no commit list and no `Action: #<run>` for `34aa970`. GitHub API was rate-limited for me; the `dotnet.yml` badge on `main` reads passing, but a run number is not retrievable. Step 5.7 (record evidence) is not done, so Stage 5 cannot be signed off.
- Fix: the Actor writes the Stage 5 report in `act.md` (commit, `Action: #<run>`, greps run) and finishes Step 5.7.

### MEDIUM / LOW
- MEDIUM: Steps 5.1–5.6 were done in one `feat` commit mixing docs and code (rule: exactly one step per pass). I verified each step separately against the plan, so there is no impact here; use one commit per step next time.
- LOW: formatting differs from the plan (`@supports` block and `.navigation-link:active` on one line, tokens in a different order); behaviour is identical.

## Verified (evidence)
- Files changed = the plan's list exactly (6 rule files, `navigation-menu.css`, `navigation-menu.js`, `components.css`, `Profile.cshtml`, plan headings struck).
- 5.1: 17 references → `TODO.md` (per file 2 / 2 / 1 / 7 / 4 / 1); no `to-do` left; only name tokens changed.
- 5.2–5.5: every plan Verify passes on the real files (`::before` 0; no `scale(0.84)`/`is-initializing`; no lift/rotate; `linear(` 2 lines; `max-height` only `0`, `0`, `calc(var(--nav-flyout-h) + 2rem)`; `cubic-bezier` exactly 3 lines; no `@keyframes`/`animation`; no `collapse-delay` in CSS or JS). CSS equals the plan-as-written build apart from token order and formatting; JS equals it exactly.
- Browser measurements on the real files: open 80 / 95 / 99 / 100% at 25 / 50 / 75 / 90% of 460 ms at 1280×1080, 800, 700 and 360×844, 640; close 84 / 22 / 3%; hovered tab rect fixed; bottom-edge hover 100% of frames; link delay `0s, 0s, 0.082s, 0.082s`; focus ring on all 6 tabs; radius 30.4 px / 28.7 px; link opacity 0 when the flyout hides; rapid switching and re-open during close end clean; no page errors.
- Clearance: stuck `.action-bar` 6.7–6.9 px above the dock at 360, 500, 768, 800, 1024, 1199 px (also with a 34 px inset) with no vertical overlap on the Save button; mobile page bottom clearance 15.3 px with and without inset.
- 5.6: `git diff -w Profile.cshtml` empty; `asp-` 14/14, `data-password-toggle` 3/3, "Save changes" 1/1.
- Not verifiable by me: the manual visual/keyboard checks (user).

## Status
Steps 5.1–5.6: **PASS** (CI evidence pending) · Step 5.7: **open** · Stage 5: not signed off.

## Gate
Next: Actor — Step 5.7 (record evidence in `act.md`), then Reviewer sign-off.
