# Plan — Stage Q — Glass page with bulk add

## Reviewer Briefing
- Bulk persistence must save all Product + Glass + GlassModelFit graphs in one SaveChanges operation.
- Glass model/category validation is service-side; allowed categories are Phone, Tablet, SmartWatch.
- Reuse the existing Stage N finished-price calculation; do not duplicate pricing logic.
- Keep API source/stubs, schema, migrations, authentication, and unrelated product flows untouched.

## ~~[x] Step 1 — Bulk glass contract and DAL creation~~
**PASS — Reviewer Job B; GitHub Actions #72 succeeded on the final Step 1 fix.**
- Files: create `src/MobileShop.Models/ViewModels/Web/BindModels/CreateGlassInputModel.cs`; modify `src/MobileShop.Services/DataServices/Interfaces/IProductsDataService.cs`, `src/MobileShop.Services/DataServices/Dal/ProductsDataService.cs`, and focused `ProductsDataServiceTests`; do not touch API source/stubs.
- Add ManufacturerId, ModelId, paid Price, ProfitPercent/ProfitAmount, and Count (1–500).
- Validate manufacturer/model ownership and ensure the model category is Phone, Tablet, or SmartWatch.
- Compute finished price once using the existing `ComputeFinishedPrice` amount-first rule.
- Build exactly Count Product + Glass + GlassModelFit graphs in memory, with the same finished price and distinct 12-character barcodes.
- Persist the complete batch with one Product-repository AddRange/save path; never save individual items in a loop.
- Keep API implementation files untouched. If an interface change cannot preserve that constraint, stop for reviewer resolution instead of modifying the API stub.
- Tests: count 1 and multi-count; exact graph counts; identical prices; distinct barcodes; amount/percent pricing; invalid count/model/category; manufacturer mismatch; rejected input creates no rows.
- Verify through GitHub Actions, not local build/test.
- Done when bulk creation is atomic through one save path and all invariants are covered.
- Risk: MEDIUM
- Confidence: MEDIUM

## [ ] Step 2 — Create Glass page and pricing UX
- Files: create `src/MobileShop.Web/Pages/Products/CreateGlass.cshtml.cs`, `CreateGlass.cshtml`, and focused `CreateGlassModelTests`; reuse CreatePhone dropdown patterns and `wwwroot/js/create-product-pricing.js`.
- Form fields: Manufacturer, Model, Paid price, Profit %, Profit amount, read-only Finished price, Count, Save, Back.
- Reuse existing manufacturer/model service methods and pricing JS hooks; keep pricing server-authoritative.
- On valid POST call CreateGlassesAsync; propagate service errors to fields and repopulate dropdowns; on success show how many products were created and stay on the page.
- Do not perform EF/repository access in the PageModel.
- Tests: GET/dropdown behavior, invalid redisplay, successful bulk POST, service-error propagation, model validation behavior.
- Verify with the matching GitHub Actions run.
- Done when the complete validated bulk-create workflow works without duplicated pricing logic.
- Risk: LOW
- Confidence: HIGH

## [ ] Step 3 — Products navigation and integration coverage
- Files: modify `src/MobileShop.Web/Pages/Products/Index.cshtml`; inspect existing navigation conventions; extend focused tests only where needed.
- Add a clearly labeled Create glass action beside existing product creation actions using the existing Razor `asp-page` convention.
- Keep navigation local to Products; no unrelated layout redesign.
- Verify the new page is reachable and successful bulk-created products remain ordinary inventory products.
- Verify with GitHub Actions and a changed-file scope review.
- Done when Create Glass is discoverable and existing product flows remain intact.
- Risk: LOW
- Confidence: HIGH

## [ ] Step 4 — Final Stage Q validation
- Inspect all Stage Q diffs, Actor evidence, audit evidence, and CI evidence; do not change implementation during validation.
- Confirm Count 1/500 boundaries, rejection of 0/501, invalid/disallowed models, same finished price, distinct barcodes, exact Product/Glass/Fit counts, one-save persistence, and no API/schema/unrelated changes.
- Full solution validation must use GitHub Actions.
- Record the human-visible Actions run number and conclusion for the final Actor commit.
- Done when all Global Definition of Done items are evidenced and Reviewer can perform final sign-off.
- Risk: MEDIUM
- Confidence: HIGH

## Global Definition of Done
- `/Products/CreateGlass` exists and is reachable from Products.
- Form supports Manufacturer → Model, Paid price, Profit %, Profit amount, read-only Finished price, and Count 1–500.
- Finished price is calculated server-side by the existing `ProductsDataService.ComputeFinishedPrice` rule.
- One submit creates exactly Count Products, each with one Glass and one GlassModelFit for the selected model.
- All Products share the same finished price and have distinct 12-character barcodes.
- Persistence uses one batch save; no per-item persistence loop.
- Server rejects invalid manufacturer/model relationships and models outside Phone/Tablet/SmartWatch.
- No schema/migration/API/auth/unrelated changes.
- Focused tests and full GitHub Actions CI pass.

## Execution notes
- One implementation step → one Actor commit → Job B → next step.
- Actor must read `.clinerules/actor.md` and must not modify `plan.md`, `audit.md`, or `to-do.md` to mark progress.
- Do not use local `dotnet build`/`dotnet test` as the final Actor verification.
