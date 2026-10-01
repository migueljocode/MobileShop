# Plan — Stage L — Quick UX Wins

## Planner Review

Stage L is the active unchecked stage in `.clinerules/to-do.md`.

## Step 1

- ~~[x] Step 1 — Make Transactions list filters apply automatically~~ (commit `d3f2f82fdc5a3b7fba14d0dedc9ebf9b7a7626ea`)

**Reviewer Job B: PASS.**

## Step 2

- ~~[x] Step 2 — Add deterministic return navigation~~ (commit `17302ccc914e0afdad635e2f78492338b90af344`)

**Reviewer Job B: PASS.**

## Step 3 — Friendly required-selection validation

- ~~[x] Step 3 — Replace raw zero-selection validation messages~~ (implementation `a8079108d5ebfe878d522fd74e7d82289161c4ae`; coverage fix `23aa1c17aad4ded6175a15ef676eaf4ede395ded`)

### Final Job B Review

**PASS.**

Verified:
- Buy ProductId → `The product should be selected.`
- Buy SellerId → `The seller should be selected.`
- Sell ProductId → `The product should be selected.`
- Sell CustomerId → `The customer should be selected.`
- Missing Seller/Customer validation spans are present.
- `Range(1, int.MaxValue)` rejection remains intact.
- All four exact messages now have direct regression assertions.
- Targeted `RecordModelTests`: **11 passed, 0 skipped, 0 failed**.
- Full suite: **251 passed, 0 skipped, 0 failed**.
- Build: **0 warnings, 0 errors**.
- No API, DB/schema/migration, authentication, data-service contract, or PDF changes in the Step 3 implementation/coverage fix.

## Stage L Definition of Done

- ~~[x] Step 3 final Job B PASS.~~
- ~~[x] All Stage L implementation changes verified.~~
- ~~[x] Targeted tests and full build/test pass.~~
- ~~[x] No API, DB/schema/migration, authentication, data-service contract/behavior, or PDF changes.~~
- [ ] Reviewer gives final Stage L sign-off before `.clinerules/to-do.md` is updated.

**Next action:** Reviewer may now perform Stage L final sign-off and update `.clinerules/to-do.md`; no further implementation rework is required.
