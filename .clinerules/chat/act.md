# Act — Stage L — Step 3 — Friendly required-selection validation

## Summary
Step 3 replaces the raw numeric range validation messages on Buy/Sell required dropdown fields with friendly selection messages, adds the missing validation spans for SellerId and CustomerId, and adds regression tests.

## Changes

### src/MobileShop.Models/ViewModels/Web/BindModels/BuyInputModel.cs
- `ProductId` Range: added `ErrorMessage = "The product should be selected."`
- `SellerId` Range: added `ErrorMessage = "The seller should be selected."`

### src/MobileShop.Models/ViewModels/Web/BindModels/SellInputModel.cs
- `ProductId` Range: added `ErrorMessage = "The product should be selected."`
- `CustomerId` Range: added `ErrorMessage = "The customer should be selected."`

### src/MobileShop.Web/Pages/Transactions/Buy.cshtml
- Added validation span for `Input.SellerId`

### src/MobileShop.Web/Pages/Transactions/Sell.cshtml
- Added validation span for `Input.CustomerId`

### src/MobileShop.Tests/Web/Pages/Transactions/RecordModelTests.cs
- Added `ValidateInput` helper to run data-annotation validation against the Input property
- Added `Zero_selection_rejected_with_friendly_message` theory: tests ProductId=0/SellerId=0 in Buy and CustomerId=0 in Sell, asserts exact friendly messages
- Added `Valid_positive_ids_pass_validation` theory: tests valid positive IDs, asserts ModelState.IsValid

## Verification
- Build: `dotnet build src/MobileShop.slnx --nologo --no-restore` → Build succeeded. 0 Warning(s), 0 Error(s).
- Targeted tests: `--filter "FullyQualifiedName~RecordModelTests"` → Passed! Failed: 0, Passed: 9, Skipped: 0, Total: 9.
- Full suite: Passed! Failed: 0, Passed: 249, Skipped: 0, Total: 249.
- `[Range(1, int.MaxValue)]` pattern only exists on BuyInputModel and SellInputModel.

## Changed files
- src/MobileShop.Models/ViewModels/Web/BindModels/BuyInputModel.cs
- src/MobileShop.Models/ViewModels/Web/BindModels/SellInputModel.cs
- src/MobileShop.Web/Pages/Transactions/Buy.cshtml
- src/MobileShop.Web/Pages/Transactions/Sell.cshtml
- src/MobileShop.Tests/Web/Pages/Transactions/RecordModelTests.cs

## Commit
`a807910` — feat(web): friendly validation messages for required dropdown selections

## Limitations
None.

## Friction noted
Full test suite (35s) exceeds 30s tool timeout; ran in background and polled output file.

## Problems
None.

## Status
COMPLETE — Step 3 (friendly validation messages) is implemented and verified. All 249 tests pass. Stage L Definition of Done is met. Stopping for final Job B review.
