# Plan — People list: fix ugly Phone column alignment

## Proposed stages
- [ ] Stage 11 — People list Phone column left-aligned (drop numeric `.num` styling)

## Reviewer Briefing
- Stage 10 column visibility is already done. Remaining complaint: Phone looks sparse/ugly because `th`/`td` use `class="num"`.
- `.num` in `surfaces.css` sets `text-align: end` (and tabular-nums). Fine for pure counts; phone strings read as identity labels next to Name and should align **start**, same as Name / National ID.
- Markup-only change on `People/Index.cshtml`. Do not change global `.num` rules (other tables still want end-aligned numbers).

## ~~[x] Step 1 — Remove `.num` from People Phone header and cells~~
- Files: inspect: `src/MobileShop.Web/Pages/People/Index.cshtml`, `src/MobileShop.Web/wwwroot/css/surfaces.css` (`.num`); modify: `Index.cshtml` only; do not touch: global CSS, other pages’ `.num` usage.
- Symbols: Phone `<th class="num">`, `<td dir="auto" class="num">`.
- Current -> Desired: Phone column is start-aligned under its header, visually consistent with Name; no large empty gap to the left of the digits.
- Change: Remove `class="num"` from the Phone `<th>` and from the Phone `<td>`. Keep `dir="auto"` on the phone cell for RTL-friendly numbers. Leave Purchased/Sold activity counts without `.num` unless they already use it (they currently do not).
- Depends on: none (Stage 10 already merged for column visibility).
- Edge cases: `type=all|customers|sellers` all share the same Phone cells; long numbers still wrap/overflow per existing table rules.
- Tests: none (markup-only).
- Verify: `/People?type=customers`, `type=sellers`, `type=all` — Phone values sit under the Phone header on the start edge, not flush right.
- Done when: Phone column no longer uses end alignment; other columns unchanged.
- Risk: LOW
- Confidence: HIGH

## [ ] Step 2 — Final validation
- Files: inspect only.
- Verify: manual three filters; CI if the push triggers workflows.
- Done when: Global Definition of Done met.
- Risk: LOW
- Confidence: HIGH

## Global Definition of Done
- People list Phone column is start-aligned (no `class="num"` on phone header/cells).
- Customers/sellers/all filters unchanged aside from Phone alignment.
- Only `Index.cshtml` touched for this stage.

## Execution notes
One step → one commit → STOP for Job B. After Reviewer APPROVES, Planner adds Stage 11 to `.agents/TODO.md` unchecked; Actor starts Step 1 only.
