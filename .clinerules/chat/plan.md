# Stage T — IRR money foundation

## Decisions
- **D1:** money widened to `long` across the stack; percentages stay `decimal`.
- **D2:** unit is IRR (Rial) everywhere; Persian "ریال" on the factor PDF; English "IRR" on Razor displays and table headers.
- **D3:** whole numbers only for money inputs (`step="1"`, `min="0"`, `max="@MoneyLimits.MaxRials"`); no fractional Rials.
- **D4:** Production migration policy: the widening migration is non-destructive, but existing values are assumed to already be in Rials (the PDF's "ریال" label assumed it); no automatic ×10 data conversion is performed.
- **D5:** Seeded values are Toman-scale, so sample data is multiplied by 10.
- **D6:** SQLite stores `int` and `long` as 64-bit `INTEGER`; widening uses a no-op migration `WidenMoneyToLong` to update the EF model snapshot.
- **A1:** GitHub Actions is the build/test gate (`Action: #<run_number> — <Success|Failure|Pending>`).

## Reviewer Briefing
- **Step 1:** HIGH risk / MEDIUM confidence (completed).
- **Step 2:** boundary tests (completed).
- **Step 3:** MEDIUM risk / HIGH confidence (completed).
- **Step 4:** MEDIUM risk / MEDIUM confidence (completed).
- **Step 5:** LOW risk / HIGH confidence (completed).

## [x] Step 1 — Widen money to `long` end to end (no behaviour change)
- Completed: CI Action #325 — Success; 330/330 .NET tests passed, build had 0 warnings/errors, and Production smoke passed.
- Carry-over: none.

## [x] Step 2 — Money limit and overflow-boundary tests
- Completed: CI Action #353 — Success; final correction verified the large-Rial report percentage assertion while preserving exact Bought/Sold/Profit/total-profit checks.
- Carry-over: none.

## [x] Step 3 — Seed data in Rials
- Completed: CI Actions #364 and #366 — Success; sample data scaled to IRR and asserted >= 1,000,000, <= MaxRials, and multiple of 10.
- Carry-over: none.

## [x] Step 4 — IRR indicator on every page and integer-only price inputs
- Completed: CI Actions #370 (PR #9) and #381 (merge into `main`) — Success; invariant `MoneyExtensions`, integer-only inputs, Razor/JS/PDF IRR formatting, and production smoke greps passed.
- Carry-over: none.

## [x] Step 5 — Docs and final Stage T validation
- Completed: CI Action #382 — Success; `README.md` Money section documented, full test suite and production smoke passed on `main`, 0 build warnings, diff stat hygiene verified.
- Carry-over: none.

## Global Definition of Done
- [x] Money is `long` in entities, view models, bind models, services, PDF DTOs and the snapshot; a no-op `WidenMoneyToLong` migration is in the chain and every history count is 7.
- [x] No overflow in sums, factor totals, profit/loss or the distribution; the boundary tests cross `int.MaxValue`.
- [x] The IRR unit appears on every money display and label; inputs are integer-only; seeded data is in Rials; the PDF keeps "ریال".
- [x] The Stage S harness and the Production smoke stay green; no Api, auth, entity-shape or Development-initialization changes.

## Carry-over to later stages
- **Stage U:** README still carries the personal "stage 3" note and a literal `&amp;`; the `Phone`-category filter default (keep it and restrict the phone model dropdown to Phone-category models) is recorded in Stage U's line.