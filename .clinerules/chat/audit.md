# Audit — Job B: Stage Y Step 2

**Verdict: FAIL** (HIGH functional risk)

**Feature:** `dd2678e` handlers · **test ctor fix:** `6779278` · **CI:** Action **#482 — Success** (compile only)

## What is OK
- Buy `OnPostCreateSellerAsync` / Sell `OnPostCreateCustomerAsync` call `IPeopleDataService` and return `DropdownCreateResult` + `EntityId` / display name.
- Api host / schema untouched; ctor tests updated.

## HIGH — `ModelState.IsValid` on the create handlers

Both handlers start with:

```csharp
if (!ModelState.IsValid)
    return CreateErrorResult("Please correct the … details.");
```

The same page models also have `[BindProperty] public BuyInputModel/SellInputModel Input`.

An AJAX POST that only sends create-person fields will still populate **ModelState errors for `Input.*`** (product id, party id, price, etc.). The handler will often return **400** even when `CreateSellerInputModel` / `CreateCustomerInputModel` is valid.

**Required fix (pick one pattern):**
1. Validate **only** the create model: `TryValidateModel(input)` after clearing unrelated keys, or
2. `ModelState.Clear()` then `TryValidateModel(input)`, or
3. Bind with a distinct prefix and ignore `Input` for these handlers.

Add at least one test that invokes the create handler with a valid create model **without** a valid `Input` and expects **200** + `EntityId`.

## Gate

Step 2 **not** complete. Actor: focused ModelState fix + test → green Action → STOP for Job B. **Do not start Step 3.**
