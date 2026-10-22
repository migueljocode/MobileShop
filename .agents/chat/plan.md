# Plan — Stage 18 — Fix Phone details identity and edit feedback

## Reviewer Briefing
- HIGH risk, high confidence: Phone inventory links and edit redirects use the base `Product.Id`, but `GetDetailsAsync` currently queries the Phone profile by `Phone.Id`. Use the product ID contract and test mismatched/colliding IDs.
- MEDIUM risk, high confidence: Edit stores `TempData["SuccessMessage"]`, but Details does not render it. Reuse the existing success partial.
- Keep the four LOW audit findings out of scope: edit-form formatting, per-manufacturer query count, Glass compatibility display, and paid-price transaction semantics.

## ~~[x] Step 1 — Fix Phone details identity and render edit feedback~~
- Files: inspect: `src/MobileShop.Web/Pages/Products/Index.cshtml`, `src/MobileShop.Web/Pages/Products/Edit.cshtml.cs`, `src/MobileShop.Services/DataServices/Dal/ProductsDataService.cs`, `src/MobileShop.Web/Pages/Products/Details.cshtml.cs`, `src/MobileShop.Web/Pages/Products/Details.cshtml`, `src/MobileShop.Web/Pages/Shared/_ProductCreateSuccess.cshtml`, and `src/MobileShop.Tests/Services/DataServices/Dal/ProductsDataServiceTests.cs`; modify only `ProductsDataService.cs`, `Details.cshtml.cs`, `Details.cshtml`, and `ProductsDataServiceTests.cs`. Do not touch API files/stubs/config/endpoints, sample-data JSON, or unrelated findings.
- Symbols: `ProductsDataService.GetDetailsAsync(int id, string type)`, `DetailsModel`, and Phone details tests in `ProductsDataServiceTests`.
- Current -> Desired: Phone links and edit redirects send the base `Product.Id`, but the Phone details branch queries by `Phone.Id`. Edit sets `TempData["SuccessMessage"]`, but Details never renders it.
- Change: query the Phone using `phone.ProductId == id` only; add `[TempData] public string? SuccessMessage { get; set; }` to `DetailsModel`; render `/Pages/Shared/_ProductCreateSuccess.cshtml` with `Model.SuccessMessage`; update Phone detail tests to pass `Product.Id` and add a regression where the requested product ID differs from its Phone profile ID and a colliding profile ID belongs to another phone.
- Depends on: none.
- Edge cases / error handling: unknown product IDs still return null/NotFound; never combine profile and product IDs with an `Id == id || ProductId == id` predicate; empty TempData must not show an alert. Preserve message text, existing transaction projection, and all unrelated behavior.
- Tests: focused Phone details tests in `ProductsDataServiceTests.cs`, including distinct and colliding IDs. No schema, seed-data, or API changes.
- Verify: inspect exact diff and wait for GitHub Actions build/test. Do not run local `dotnet build/test` unless CI is unavailable or requested.
- Done when: Phone Details/Edit resolve the correct product independently of profile IDs; post-save and no-op feedback renders on Details; regression tests pass; CI is green; no out-of-scope files change.
- Risk: HIGH
- Confidence: HIGH

## Global Definition of Done
- Canonical base-product ID lookup for Phone details with mismatched/colliding-ID regression coverage.
- Edit success/no-op feedback renders through the existing partial and TempData.
- Focused tests and GitHub Actions pass; scope restrictions are respected.
