# Audit — Job B: Stage X Step 1 (CI failure)

**Verdict: FAIL** (reopens Step 1 until CI is green)

**Action #464 — Failure** on `6664ed63` (also #465 on the audit-only commit).

## Cause

```
JalaliDateExtensionsTests.ToJalaliDateString_formats_default_date_without_throwing [FAIL]
System.ArgumentOutOfRangeException: Specified time is not supported in this calendar.
It should be between 03/22/0622 … and 12/31/9999 … (Parameter 'time')
Actual value was 0.
```

`default(DateTime)` is Gregorian year 1. `PersianCalendar` only supports dates from **622-03-22** onward, so `GetYear` throws.

Known conversions (1403/01/01, 1404/01/01, 1404/07/13) and DateTime tests **passed** (374/375).

## Required fix (pick one; keep scope tiny)

**Preferred:** Delete `ToJalaliDateString_formats_default_date_without_throwing` — it is not a realistic app input.

**Optional stronger:** In `ToJalaliDateString` / `ToJalaliDateTimeString`, if `value` is outside `PersianCalendar` min/max, fall back to Gregorian `yyyy/MM/dd` (or `yyyy/MM/dd HH:mm`) instead of throwing — then keep a test for that fallback.

Do **not** start Step 2 until Action is green on the repair.

## Gate

Step 1 **not** complete. Actor: one focused repair commit → green Action → STOP for Job B.
