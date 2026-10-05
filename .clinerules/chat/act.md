# Stage Y — Step 2 Act Summary

## Scope
Repaired the Stage Y Step 2 AJAX create handlers so unrelated transaction-page ModelState errors do not reject valid create-person requests.

## Changes
- Buy seller creation now clears page-level ModelState and validates only the supplied CreateSellerInputModel.
- Sell customer creation now clears page-level ModelState and validates only the supplied CreateCustomerInputModel.
- Extended handler tests to prove valid create requests still return HTTP 200 + EntityId when an unrelated Input.* ModelState error is present.

## Verification
- Reviewer Job B found HIGH functional risk in the original handlers.
- Previous repair Action: #482 — Success (compile-only CI result recorded by Reviewer).
- Local dotnet build/test was not run because GitHub Actions is the CI gate.
- Repair Action: pending.

## Limitations
- None.

## Friction noted
- None.

## Problems
- Original handlers used page-level ModelState.IsValid, which can contain errors for unrelated Buy/Sell Input fields during an AJAX create-only POST.

## Status
STOPPED — awaiting the repair commit's GitHub Action result.
