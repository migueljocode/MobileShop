# Audit — Stage X Step 2 (pending CI)

**Status:** implemented on `main`, **await Action green** before Job B PASS.

**Commits:**
- `42065d6` — initial factor Jalali + polish (accidentally broke Resolve/presentation records)
- `ba81e15` — **fix:** restored `OwnershipTransferred` mapping + external `InvoicePresentation` / `PersianInvoicePresentation` records; kept Jalali factor dates and light polish

**Expected on green CI:**
- Factor header + row dates use `ToJalaliDateTimeString()`
- Invoice English/Persian dates still Gregorian
- PDF tests still pass

**Gate:** Actor/reviewer waits for Action on `ba81e15` → then Job B.
