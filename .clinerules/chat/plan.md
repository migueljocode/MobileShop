# Plan — Stage Q — Glass page with bulk add

## Reviewer Briefing
- Bulk persistence must save all Product + Glass + GlassModelFit graphs in one SaveChanges operation.
- Glass model/category validation is service-side; allowed categories are Phone, Tablet, SmartWatch.
- Reuse the existing Stage N finished-price calculation; do not duplicate pricing logic.
- API data-service counterparts may be synchronized with DAL counterparts per author authorization; no schema, migrations, authentication, or unrelated changes.

## ~~[x] Step 1 — Bulk glass contract and DAL creation~~
**PASS — Reviewer Job B; GitHub Actions #72 succeeded on the final Step 1 fix.**
- Added the bulk glass input/contract and DAL creation path.
- Count requires a positive value only; no artificial upper cap.
- Service validates manufacturer/model ownership and allowed model categories, reuses the existing finished-price calculation, builds exactly Count Product + Glass + GlassModelFit graphs, generates distinct 12-character barcodes, and persists through one batch AddRange/save path.
- Focused tests cover count, graph counts, pricing, barcode uniqueness, invalid relationships/categories, and rejected input.

## ~~[x] Step 2 — Create Glass page and pricing UX~~
**PASS — Actor implementation merged; Actions #78 succeeded on the final main commit.**
- Added /Products/CreateGlass with Manufacturer → Model, Paid price, Profit %, Profit amount, read-only Finished price, positive Count, Save, and Back.
- Reused the shared pricing script and kept server-side pricing authoritative.
- Added focused PageModel tests for GET/dropdown behavior, invalid redisplay, successful bulk POST, and service-error propagation.

## ~~[x] Step 3 — Products navigation and integration coverage~~
**PASS — Reviewer Job B; Actions #85 and #86 succeeded.**
- Added the clearly labeled Create glass action to Products using the existing Razor asp-page convention.
- No unrelated navigation/layout changes.

## ~~[x] Step 4 — Final Stage Q validation~~
**PASS — Reviewer final validation; the final Actor/documentation commit is verified by GitHub Actions.**
- Confirmed positive Count validation with no artificial upper cap and rejection of zero.
- Confirmed service-side manufacturer/model/category validation, shared finished price, distinct 12-character barcodes, exact Product/Glass/Fit graph counts, and one batch persistence path.
- Confirmed /Products/CreateGlass and Products navigation are present, focused tests exist, and the full GitHub Actions gate passes.
- Confirmed no Stage Q schema/migration/auth changes and no unrelated implementation files in the Stage Q diff.
- Final evidence: implementation Actions #85 succeeded; Actor report Actions #86 succeeded; final Stage Q validation commit is covered by its matching Actions run.

## Global Definition of Done
- /Products/CreateGlass exists and is reachable from Products.
- Form supports Manufacturer → Model, Paid price, Profit %, Profit amount, read-only Finished price, and any positive Count.
- Finished price is calculated server-side by the existing ProductsDataService.ComputeFinishedPrice rule.
- One submit creates exactly Count Products, each with one Glass and one GlassModelFit for the selected model.
- All Products share the same finished price and have distinct 12-character barcodes.
- Persistence uses one batch save; no per-item persistence loop.
- Server rejects invalid manufacturer/model relationships and models outside Phone/Tablet/SmartWatch.
- No schema/migration/auth/unrelated implementation changes.
- Focused tests and full GitHub Actions CI pass.

## Execution notes
- Stage Q is complete.
- Do not use local dotnet build/dotnet test as the final verification gate.