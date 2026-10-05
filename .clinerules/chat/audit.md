# Audit — Job B: Stage W Step 2

**Verdict: FAIL**

**PR #23** (`actor/stage-w-step-2`, head `f6c771c`) · CI Action **#452 — Success** (per act) does not override the functional gap.

## What is OK
- Clickable headers for Date / Product / Price / Seller / Customer with toggle asc↔desc
- Indicator + `aria-sort` only on active column
- Direction header plain
- Order dropdown removed
- `LoadAsync` + `OnGetDownloadFactorAsync` pass `Sort` into the service
- Per-row Download Factor links include `sort` and `order`

## HIGH — fix before re-review

**Filter form drops `sort` / `order`.**

The GET form only posts `direction` and `take`. Missing:

```html
<input type="hidden" name="sort" value="@Model.Sort" />
<input type="hidden" name="order" value="@Model.Order" />
```

Effects:
1. **Direction** `onchange` submit (and Count submit) **resets** sort to defaults (`date`/`desc`).
2. Main **Download Factor (PDF)** button runs without `sort`/`order`, so the bulk factor can disagree with the sorted table (row links are fine).

Plan D4/D5 and Step 2 require filters and factor to keep the current sort.

## Fix (narrow)
- Add the two hidden fields to the Transactions Index form.
- Re-run CI; do not expand scope.

## Gate
Step 2 **not** authorized complete. Actor: one focused repair commit on PR #23 → green Action → STOP for Job B again.
