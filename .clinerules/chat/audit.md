# Audit — Job B: Stage Z Step 3

**Verdict: FAIL**

**Commit:** `b3c23a9` — create-product modal  
**CI:** Action **#501 — Failure** (build)

## Cause
`BuyModel` now takes `IProductsDataService` as a third constructor parameter. Existing tests still construct:

```csharp
new BuyModel(CreateService(), null!)
```

**CS7036** in:
- `RecordModelTests.cs` (multiple sites)
- `PersonCreateHandlerTests.cs` (Buy constructions)

## Required fix
Pass `null!` (or a stub) for `productsDataService` wherever `BuyModel` is constructed in tests that do not exercise product create. Any new Step 3 handler test should inject a real/fake `IProductsDataService` as needed.

## Gate

Step 3 **not** complete. Actor: ctor fix → green Action → STOP for Job B. **Do not start Step 4.**
