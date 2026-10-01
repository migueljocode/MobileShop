# Audit — Stage O Step 4 validation task for Actor

**Purpose:** collect the remaining validation evidence required for the Reviewer to complete Stage O Step 4.  
**Scope:** validation/evidence only. Do not change production implementation files.

## Actor task

Run the final Stage O validation and record concise evidence in `act.md`.

### 1. Clean build and full test suite

Run:

```bash
dotnet clean src/MobileShop.slnx --nologo
dotnet build src/MobileShop.slnx --nologo --no-incremental
dotnet test src/MobileShop.slnx --nologo --no-build
```

Record:
- build result;
- warning/error counts;
- test passed/failed/skipped counts.

The required result is **0 warnings, 0 errors, all tests passing**.

### 2. Finished-price culture check

Using the running application/browser, verify that changing a product with a suggested price prefills **Finished price** correctly.

Check:
- the application's configured culture;
- a comma-decimal culture if one is configured/available;
- the displayed Finished price;
- the value posted/bound when the form is submitted;
- that the decimal value is not corrupted by culture formatting.

Record the observed culture, input/display value, and result.

### 3. Final Buy UI check

Open the Buy page and confirm:
- Product selector is present;
- Suggested price is shown for products that have one;
- Finished price is present and labeled **Finished price**;
- Date is present and defaults to today on GET;
- Seller is present;
- Back button is present;
- product-picker.js is loaded once.

Also verify a failed post preserves the posted Product and Finished price and renders the relevant validation message.

### 4. Final Sell UI check

Open the Sell page and confirm:
- Product selector is present;
- Suggested price is shown for products that have one;
- Finished price is present and labeled **Finished price**;
- Date is present and defaults to today on GET;
- Seller is **not** present;
- Back button is present;
- product-picker.js is loaded once.

Also verify a failed post preserves the posted Product and Finished price and renders the relevant validation message.

### 5. Regression/scope check

Confirm:
- `src/MobileShop.Api` has no changes from the Stage O implementation;
- no services/entities/schema/PDF/auth changes were introduced;
- no unrelated implementation files were changed during validation.

## Evidence format

Append a concise section to `act.md` containing:
- commands and results;
- culture/browser result;
- Buy UI result;
- Sell UI result;
- scope/regression result;
- any failure or limitation.

Do **not** mark Step 4 complete in `plan.md`, `audit.md`, or `to-do.md`. The Reviewer performs the final sign-off after reviewing the evidence.

## Pass criteria

Step 4 is ready for Reviewer sign-off only when every check above passes and the evidence is recorded in `act.md`.
