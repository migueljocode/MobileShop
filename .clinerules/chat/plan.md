# Plan — Stage L — Quick UX Wins

## Reviewer Briefing

Stage L is the active stage. Previous-stage plans are intentionally omitted from this file.

### Scope

- Small Web UX and validation improvements only.
- No API changes.
- No database/schema/migration changes.
- No authentication changes.
- No data-service contract or behavior changes.
- No PDF changes.
- Execute exactly one step → commit → Job B review → next step.

## Step 1 — Make Transactions filters auto-apply

- Remove the **Apply filters** button from `src/MobileShop.Web/Pages/Transactions/Index.cshtml`.
- Preserve **Download Factor** and its existing selected-ID behavior.
- Direction changes submit the GET form immediately.
- Order changes submit the GET form immediately.
- Count submits after a short debounce (~300 ms); do not submit once per keystroke.
- Preserve query parameters: `direction`, `take`, `order`.
- Preserve server-side Count clamping to 1–500.
- Do not change the transaction data-service contract or filtering semantics.
- Prefer a small page-local/shared vanilla JS solution; no framework.
- Add focused regression coverage only where useful for the existing page-model contract.
- Do not edit `.clinerules/chat/plan.md`, `.clinerules/chat/audit.md`, or `.clinerules/to-do.md`.

**Risk:** MEDIUM  
**Confidence:** HIGH

### Step 1 Definition of Done

- Apply filters button is gone.
- Direction and Order auto-submit.
- Count is debounced.
- Download Factor remains a distinct submit action.
- Existing query parameter names and server-side clamping remain unchanged.
- Targeted tests pass.
- Full build/test passes with no new warnings or failures.
- No API/DB/auth/data-service/PDF changes.

**Authorization: Step 1 only. Stop for Job B after committing and reporting the exact SHA.**

## Step 2 — Add Return/Back navigation

Not authorized until Step 1 receives Job B PASS.

- Buy and Sell → `/Transactions/Index`
- CustomerDetails → `/People/Customers`
- SellerDetails → `/People/Sellers`
- Prefer deterministic Razor `asp-page` navigation.
- Do not alter POST behavior or data-service calls.

## Step 3 — Replace raw unselected-dropdown validation messages

Not authorized until Step 2 receives Job B PASS.

- Buy ProductId → **The product should be selected.**
- Buy SellerId → **The seller should be selected.**
- Sell ProductId → **The product should be selected.**
- Sell CustomerId → **The customer should be selected.**
- Preserve rejection of zero/invalid IDs.
- Inspect other required select fields using the same `Range(1, int.MaxValue)` pattern and correct equivalent raw messages where applicable.
- Add validation regression tests.

## Global Definition of Done

- All three Stage L steps are independently reviewed and pass.
- Transactions filters auto-apply as specified.
- Required return navigation exists.
- Requested validation messages replace the raw range message without weakening validation.
- Targeted tests and full solution build/test pass.
- No API, DB/schema/migration, authentication, data-service contract/behavior, or PDF changes.
- Reviewer signs off Stage L only after final Job B PASS.
