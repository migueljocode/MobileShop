## Reviewer Audit — Stage 23, Job A (plan review)

**Status: APPROVED.** Proposed stage (Stage 23) is one coherent, behaviour-neutral UI step in dependency order; it covers the reported misalignment and keeps every form field, JS hook and the live-preview maths untouched.

Verified against the repo: the modal is a 3-column `.data-table` with a wrapping amount cell; `formatAmount()` appends ` Toman` to every number; no CSS exists for `is-changed` or the editor; all JS hooks listed in the plan exist; `overlays.css` uses `BEGIN/END name-vN` blocks and the cited tokens exist; the test project has no render/JS harness, so the manual check is correctly assigned to the user.

MEDIUM (fixed in the plan, no extra round): the pair-hiding mechanism was left as "pick one" — now fixed to wrapper `<div class="share-row__line">` + CSS only; JS delta text/class for an invalid input is specified.

Next: Actor, Step 1 (after the Planner commit of `plan.md`, `audit.md` and the Stage 23 line in `TODO.md`).
