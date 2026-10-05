# Stage Y — Step 3 Act Summary

## Scope
Implemented the shared searchable person picker and create-person modal on Record Buy and Record Sell.

## Changes
- Added shared `_PersonPicker.cshtml` markup for customer/seller role-specific comboboxes and Bootstrap create modals.
- Added shared `person-picker.js` for debounced server search, selection, AJAX creation, antiforgery token submission, and post-create selection.
- Added `OnGetSearchSellersAsync` and `OnGetSearchCustomersAsync` Razor Page handlers backed by `IPeopleDataService`.
- Wired both transaction pages to the shared picker and script while leaving product-picker behavior unchanged.
- Customer modal includes national ID; seller modal includes entity type and no national ID.

## Verification
- Local dotnet build/test was not run because GitHub Actions is the CI gate.
- Implementation commit: pending.
- Action: pending.

## Limitations
- Existing GET party lists remain as the initial progressive-enhancement results; server search is the primary path.
- The shared modal uses the existing Bootstrap runtime; no new dependency was added.

## Friction noted
- None.

## Problems
- None.

## Status
STOPPED — awaiting GitHub Action result.
