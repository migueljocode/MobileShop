# Audit — Job B: Stage Z Step 1

**Verdict: FAIL**

**Commit:** `751913b` — `SearchSelectableProductsAsync`  
**CI:** Action **#494 — Failure** (1 test)

## Implementation (OK in spirit)
- Interface + Dal filter via `GetSelectableProductsAsync` then query match on Name/Type/Identifier/Color/PartNumberLabel.
- Empty query → take first N; take clamped.
- Api NIE expected if present.

## Failure
`SearchSelectableProductsAsync_filters_by_direction_and_query`:

```
Assert.Equal() Failure: Expected: 2 Actual: 1
```

at asserting **`buyResults[0].EntityId`** against **`appleIdProduct.Id`** (product id).

`ProductListItemViewModel` has both **`EntityId`** (profile row id) and **`ProductId`**. For Apple ID / Phone, those differ. Transaction forms bind **product id**.

**Fix:** assert `ProductId` (not `EntityId`) in that test (and any sibling asserts that compare to `CreateProduct` ids). Re-check Sell branch the same way.

## Gate

Step 1 **not** complete. Actor: fix test (or projection if ProductId wrong) → green Action → STOP for Job B. **Do not start Step 2.**
