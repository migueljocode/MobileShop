# Act — Stage L — Step 2 — Return/Back buttons on Buy, Sell, CustomerDetails, SellerDetails

## Summary
Step 2 adds a Back navigation link to all four detail/entry pages, using Razor `asp-page` navigation (deterministic links, no browser-history JS). Each link targets the exact owning list page.

## Changes
- `src/MobileShop.Web/Pages/Transactions/Buy.cshtml`: Added `<a asp-page="/Transactions/Index" class="btn btn-secondary mb-3">Back</a>` before the form.
- `src/MobileShop.Web/Pages/Transactions/Sell.cshtml`: Added `<a asp-page="/Transactions/Index" class="btn btn-secondary mb-3">Back</a>` before the form.
- `src/MobileShop.Web/Pages/People/CustomerDetails.cshtml`: Added `<a asp-page="/People/Customers" class="btn btn-secondary mt-3">Back to customers</a>` after the product list.
- `src/MobileShop.Web/Pages/People/SellerDetails.cshtml`: Added `<a asp-page="/People/Sellers" class="btn btn-secondary mt-3">Back to sellers</a>` after the product list.

## Verification
- Build: `dotnet build src/MobileShop.slnx --nologo --no-restore` → Build succeeded. 0 Warning(s), 0 Error(s).
- Full suite: `dotnet test src/MobileShop.slnx --no-build --no-restore --nologo` → Passed! Failed: 0, Passed: 245, Skipped: 0, Total: 245.
- Confirmed `/People/Customers` and `/People/Sellers` Razor pages exist.
- No PageModel POST/GET behavior changed — links are pure GET navigation.

## Changed files
- `src/MobileShop.Web/Pages/Transactions/Buy.cshtml`
- `src/MobileShop.Web/Pages/Transactions/Sell.cshtml`
- `src/MobileShop.Web/Pages/People/CustomerDetails.cshtml`
- `src/MobileShop.Web/Pages/People/SellerDetails.cshtml`

## Commit
`17302cc` — feat(web): add Back buttons to Buy, Sell, CustomerDetails, SellerDetails

## Limitations
None.

## Friction noted
Full test suite (~35s) exceeds the 30s tool timeout; ran in background and polled output file.

## Problems
None.

## Status
COMPLETE — Step 2 (Back buttons) is implemented and verified. All 245 tests pass. Stopping for Job B review.
