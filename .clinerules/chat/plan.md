# Plan — Stage L — Quick UX Wins

## Planner Review

Stage L is the active unchecked stage in `.clinerules/to-do.md`.

### Current repository facts
- Transactions filters were completed in Step 1.
- Buy and Sell now have deterministic Back links to `/Transactions/Index`.
- CustomerDetails now links back to `/People/Customers`.
- SellerDetails now links back to `/People/Sellers`.
- Step 2 changed only the four Razor pages; no PageModel behavior, model binding, or data-service calls changed.
- Existing full-suite result reported by the Actor: 245 passed, 0 skipped, 0 failed.
- Step 3 remains the only unexecuted implementation step.

## Execution rules

1. Execute one step only.
2. Inspect the relevant implementation before editing.
3. Make the smallest change that satisfies the step.
4. Preserve existing behavior/contracts unless this plan explicitly changes the UI.
5. Add or update focused tests when useful without new infrastructure.
6. Run targeted verification, then full build/test.
7. Review the final diff for scope before committing.
8. Commit the step with a focused message and report the exact SHA.
9. Stop after the commit.
10. Wait for Reviewer Job B PASS and explicit authorization before the next step.
11. Actor must not edit `.clinerules/chat/plan.md`, `.clinerules/chat/audit.md`, or `.clinerules/to-do.md`.
12. Reviewer owns progress documentation.

## Step 1

- ~~[x] Step 1 — Make Transactions list filters apply automatically~~ (commit `d3f2f82fdc5a3b7fba14d0dedc9ebf9b7a7626ea`)

**Reviewer Job B: PASS.**

## Step 2

- ~~[x] Step 2 — Add deterministic return navigation~~ (commit `17302ccc914e0afdad635e2f78492338b90af344`)

### Verified scope
- Buy → `/Transactions/Index`
- Sell → `/Transactions/Index`
- CustomerDetails → `/People/Customers`
- SellerDetails → `/People/Sellers`
- Navigation uses Razor `asp-page`.
- No PageModel GET/POST, model binding, or data-service behavior changes.

**Reviewer Job B: PASS. Step 3 is now authorized.**

## Step 3 — Friendly required-selection validation

- [ ] Step 3 — Replace raw zero-selection validation messages

### Required messages
- Buy `ProductId` → **The product should be selected.**
- Buy `SellerId` → **The seller should be selected.**
- Sell `ProductId` → **The product should be selected.**
- Sell `CustomerId` → **The customer should be selected.**

### Requirements
- Preserve rejection of `0` and other invalid/non-positive IDs.
- Keep server-side validation.
- Ensure all four fields render validation messages.
- Inspect other Web bind models using the same `Range(1, int.MaxValue)` pattern, including manufacturer/model fields.
- Change only required dropdown fields that use the numeric placeholder pattern.
- Add regression coverage for exact messages and valid positive IDs where useful.
- Do not change unrelated numeric ranges.

### Verification
- Exact requested messages for all four transaction fields.
- Valid positive IDs still pass.
- Invalid/non-positive values remain rejected.
- Equivalent required manufacturer/model selects are handled consistently where applicable.
- Focused tests and full solution build/test pass.
- Diff is validation-only; no API/DB/auth/data-service/PDF changes.

**Stop for final Job B after committing.**

## Stage L Definition of Done
- [ ] Step 3 PASS.
- [ ] All Stage L implementation changes verified.
- [ ] Targeted tests and full build/test pass.
- [ ] No API, DB/schema/migration, authentication, data-service contract/behavior, or PDF changes.
- [ ] Reviewer gives final Job B PASS before Stage L is marked complete in `.clinerules/to-do.md`.
