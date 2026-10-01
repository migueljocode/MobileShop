# Plan — Stage N — Create Phone & Create Apple ID UX

## Reviewer Briefing

- Stage M PartNumber work is out of scope.
- Shared pricing JS created in Step 2; Step 3 only reuses it.
- No schema/Api/auth/PDF changes.

## Assumptions

- **A1–A4** as previously approved (paid bind, linked %↔$, Phone toggles only, PartNumber done).

## ~~[x] Step 1 — Bind models + server finished-price + notes persistence~~

- **Done** — `ab7c8ff`

## ~~[x] Step 2 — Create Phone UI: toggles, notes, shared pricing script~~

- **Done** — Job B PASS (`152f234`). `create-product-pricing.js` + Create Phone toggles/notes/finished display.

## [ ] Step 3 — Create Apple ID reuses shared pricing script

- Files
  - `CreateAppleId.cshtml` — remove duplicate inline pricing sync; reference `create-product-pricing.js`
  - Do **not** recreate or fork the formula
  - No second-hand/guarantee UI on Apple ID

- Desired: same paid / profit / read-only finished display as Phone; Email/Password/Notes unchanged.

- Verify: focused tests if needed; both pages load one script.

- Done when: single client formula; Job B PASS.

- Risk: LOW
- Confidence: HIGH

## [ ] Step 4 — Final Stage N validation (Reviewer sign-off)

- Full build + test; checklist; tick Stage N in `to-do.md` only after PASS.

## Global Definition of Done

- Phone toggles + notes; finished price client+server; one shared pricing JS; Apple ID pricing only; PartNumber untouched; suite green.

## Execution notes

**Step 3 is authorized.** One commit → act.md → STOP.
