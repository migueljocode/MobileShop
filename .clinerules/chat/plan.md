# Plan — Stage L — Quick UX Wins

## Planner Review

Stage L is the active unchecked stage in `.clinerules/to-do.md`.

This plan has been rechecked against the current repository before authorization.

### Repository facts

- `Transactions/Index.cshtml` currently has one GET form containing Direction, Count, Order, row-selection checkboxes, and both the **Apply filters** and **Download Factor** submit buttons.
- `Transactions/Index.cshtml.cs` already preserves the `direction`, `take`, and `order` GET parameters and clamps Count with `Math.Clamp(take, 1, 500)`.
- Download Factor uses the same GET form and `selectedIds`; its handler must remain a distinct submit action.
- Buy and Sell use `0` placeholder values for required Product/Seller/Customer selects.
- Buy/Sell input models currently use `[Range(1, int.MaxValue)]`, which produces the raw default range message for the placeholder value.
- Buy currently has Product validation markup but no Seller validation span. Sell currently has Product validation markup but no Customer validation span.
- CustomerDetails and SellerDetails currently have no return navigation.
- Existing transaction PageModel coverage is in `src/MobileShop.Tests/Web/Pages/Transactions/RecordModelTests.cs`.
- `src/MobileShop.Web/wwwroot/js/site.js` currently contains no application JavaScript, so a small page-local script is acceptable.
- Stage L must not alter API, database/schema/migrations, authentication, data-service contracts/behavior, or PDF behavior.

## Execution rules

1. Execute **one step only**.
2. Inspect the relevant existing implementation before editing.
3. Make the smallest change that satisfies the step.
4. Preserve existing behavior/contracts unless this plan explicitly changes the UI behavior.
5. Add or update focused tests when they can verify the changed contract without introducing new test infrastructure.
6. Run targeted verification, then the required full build/test verification for the step.
7. Review the final diff for scope before committing.
8. Commit the step with a focused commit message and report the exact commit SHA.
9. **STOP after the commit. Do not start the next step.**
10. Wait for Reviewer Job B to PASS and explicitly authorize the next step.
11. Actor must not edit `.clinerules/chat/plan.md`, `.clinerules/chat/audit.md`, or `.clinerules/to-do.md`. Reviewer owns those files.
12. Do not change the Stage L checklist status yourself.

## Step 1 — Transactions filters auto-apply

- ~~[x] Step 1 — Make the Transactions list filters apply automatically~~ (commit `d3f2f82fdc5a3b7fba14d0dedc9ebf9b7a7626ea`)

### Required implementation

- File primarily in scope: `src/MobileShop.Web/Pages/Transactions/Index.cshtml`.
- Remove only the visible **Apply filters** submit button.
- Keep **Download Factor (PDF)** as a submit button with its current handler semantics.
- Keep the row `selectedIds` checkboxes and their current behavior.
- Direction changes must submit the existing GET form immediately.
- Order changes must submit the existing GET form immediately.
- Count must submit the existing GET form after a short debounce, approximately 300 ms.
- Count must **not** submit once for every keystroke.
- Preserve the existing parameter names exactly: `direction`, `take`, `order`.
- Preserve the existing server-side Count clamp of 1–500; do not move validation responsibility entirely to JavaScript.
- Use ordinary browser JavaScript only; do not introduce a JavaScript framework or dependency.
- Keep the implementation small and local to the Transactions page unless an existing shared mechanism is clearly more appropriate.
- Do not auto-submit when the user is interacting with the Download Factor action in a way that changes its current behavior.
- Do not alter `IndexModel.LoadAsync`, `GenerateListFactorPdfAsync`, or the data-service contract merely to implement the UI behavior.
- Do not change the meaning of empty `selectedIds` versus explicitly selected IDs.

### Verification

- Confirm the Apply filters button is absent.
- Confirm Direction and Order controls submit the GET form immediately.
- Confirm Count waits for the debounce interval before submitting.
- Confirm changing Count repeatedly resets the debounce rather than producing multiple immediate requests.
- Confirm Download Factor remains a separate submit action.
- Confirm query parameters remain `direction`, `take`, and `order`.
- Run focused tests relevant to Transactions.
- Run full solution build/test.
- Inspect `git diff` and confirm no API/DB/auth/data-service/PDF changes.

**Reviewer Job B: PASS — verified commit `d3f2f82fdc5a3b7fba14d0dedc9ebf9b7a7626ea`. Step 2 is now authorized.**

## Step 2 — Return/Back navigation

- [ ] Step 2 — Add deterministic return navigation

**Authorization:** blocked until Step 1 Job B is PASS.

### Required implementation

- Add a clear **Back** control to `Transactions/Buy.cshtml` linking to `/Transactions/Index`.
- Add a clear **Back** control to `Transactions/Sell.cshtml` linking to `/Transactions/Index`.
- Add a clear **Back to customers** control to `People/CustomerDetails.cshtml` linking to `/People/Customers`.
- Add a clear **Back to sellers** control to `People/SellerDetails.cshtml` linking to `/People/Sellers`.
- Prefer Razor `asp-page` navigation over browser-history JavaScript.
- Do not change PageModel POST/GET behavior, model binding, or data-service calls.
- Keep the controls deterministic and available from the rendered page.

### Verification

- Confirm each link targets the exact owning list page.
- Confirm Buy/Sell POST behavior is unchanged.
- Run focused tests and full build/test as appropriate.
- Inspect the diff for UI/navigation-only scope.

**Stop for Job B after committing.**

## Step 3 — Friendly required-selection validation

- [ ] Step 3 — Replace raw zero-selection validation messages

**Authorization:** blocked until Step 2 Job B is PASS.

### Required implementation

Update only the validation metadata/messages needed for required select fields whose placeholder value is `0`.

The required messages are:

- Buy `ProductId` → **The product should be selected.**
- Buy `SellerId` → **The seller should be selected.**
- Sell `ProductId` → **The product should be selected.**
- Sell `CustomerId` → **The customer should be selected.**

Requirements:

- Preserve rejection of `0` and other invalid/non-positive IDs.
- Do not replace server-side validation with client-only validation.
- Ensure the Razor pages actually render validation messages for all four affected fields; add the missing Seller/Customer validation spans where required.
- Inspect the other Web bind models for required select fields using the same `Range(1, int.MaxValue)` pattern, including manufacturer/model fields.
- Where the same pattern represents a required dropdown selection, give it an explicit friendly selection message rather than the raw numeric range message.
- Do not change fields where the numeric range is not a required dropdown selection.
- Add regression tests that validate both rejection and exact friendly error messages for affected models.

### Verification

- Validate the four transaction fields with `0` and confirm the exact requested messages.
- Confirm valid positive IDs still pass validation.
- Confirm invalid/non-positive values remain rejected.
- Confirm equivalent manufacturer/model select fields found during inspection are handled consistently.
- Run focused tests and full solution build/test.
- Inspect the diff for validation-only scope.

**Stop for final Job B after committing.**

## Stage L Definition of Done

- [ ] Transactions filters auto-apply as specified, with Count debounced.
- [ ] Download Factor remains functional and distinct from filter auto-submit.
- [ ] Buy/Sell and customer/seller detail pages have deterministic return navigation.
- [ ] Required transaction dropdowns show the requested friendly messages.
- [ ] Other equivalent required select fields are checked and corrected where applicable.
- [ ] Existing valid behavior remains unchanged.
- [ ] Targeted tests and full solution build/test pass with no new failures or warnings.
- [ ] No API, DB/schema/migration, authentication, data-service contract/behavior, or PDF changes.
- [ ] Reviewer gives final Job B PASS before Stage L is marked complete in `.clinerules/to-do.md`.
