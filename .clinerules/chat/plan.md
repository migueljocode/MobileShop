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

- [ ] Step 3 — Replace raw zero-selection validation messages

### Implementation reviewed

Commit:
`a8079108d5ebfe878d522fd74e7d82289161c4ae`

Implemented:
- Friendly `Range` messages on Buy ProductId/SellerId.
- Friendly `Range` messages on Sell ProductId/CustomerId.
- Missing Seller/Customer validation spans added.
- Regression tests added.
- Actor reports targeted tests 9/9 and full suite 249/249.

### Job B finding

**NOT PASS — regression coverage is incomplete.**

The implementation has the requested four validation messages, but the new regression test only asserts exact friendly messages for:
- Buy ProductId
- Sell CustomerId

It does **not** assert the exact requested messages for:
- Buy SellerId → `The seller should be selected.`
- Sell ProductId → `The product should be selected.`

Because Step 3 explicitly requires regression tests for the affected models/messages, the final Job B cannot PASS yet.

### Required rework

Add focused regression assertions covering all four affected fields and exact messages. Keep the existing implementation otherwise unchanged unless the additional inspection finds another required-dropdown field using the same pattern.

Then rerun:
- `RecordModelTests`
- full solution build
- full solution test

Commit the test correction and stop for final Job B review.

**Stage L remains incomplete. Do not change `.clinerules/to-do.md`.**

## Stage L Definition of Done

- [ ] Step 3 final Job B PASS.
- [ ] All Stage L implementation changes verified.
- [ ] Targeted tests and full build/test pass.
- [ ] No API, DB/schema/migration, authentication, data-service contract/behavior, or PDF changes.
- [ ] Reviewer gives final Job B PASS before Stage L is marked complete in `.clinerules/to-do.md`.
