# Plan — Stage 19 — Cable edit validation and invoice phone display

## Proposed stages
- [ ] Stage 19 — Cable edit validation and invoice phone display

## Reviewer Briefing
- Stage 18 (Phone details by `ProductId`, `SuccessMessage` rendered on Details) is verified in the diff; nothing carried over.
- Step 1 root cause (HIGH confidence): `_ProductEditForm.cshtml` renders the cable length input with `step="0.1"`. The decimal column renders `1.00`; jQuery Validate's `step` rule rejects more decimal places than the step has ("Please enter a multiple of 0.1"). `2` passes because it has no decimals. Create cable already uses `step="0.01"`.
- Step 1 connectors (MEDIUM confidence): at HEAD the server already rejects null/undefined `Connector1`/`Connector2` (`Edit.cshtml.cs` + `UpdateProductAsync`), but the selects have no client-side `required`, so the form submits with the placeholder. Could not reproduce without running the app; if it still reproduces after this step, the Reviewer must ask for exact repro steps.
- Step 2 root cause (MEDIUM confidence): `ShopPhone` is rendered inside `ContentFromRightToLeft()` in a Text with ordinary spaces; spaces are neutral in bidi, so digit groups reorder (`0911 500 9109` -> `9109 500 0911`). Fix needs a rendered-PDF check; unit tests alone cannot prove it.
- Reviewer correction (MEDIUM): only the factor footer is actually right-to-left. `ComposePagePersian`/`RenderFooterPersian` (the Persian invoice) set no `ContentFromRightToLeft`, so their phone lines are unaffected and stay out of scope.
- Step 2 risk: zero-width marks may render as tofu in Vazirmatn. Preferred approach uses only NBSP (U+00A0); add LRM only if rendering proves it is needed.

## ~~[x] Step 1 — Cable length step and mandatory connectors (client side)~~
- Files: inspect: `src/MobileShop.Web/Pages/Shared/_ProductEditForm.cshtml`, `src/MobileShop.Web/Pages/Shared/_ProductCreateCableForm.cshtml`, `src/MobileShop.Web/Pages/Products/Edit.cshtml.cs`, `src/MobileShop.Models/ViewModels/Web/BindModels/EditProductInputModel.cs`; modify: `src/MobileShop.Web/Pages/Shared/_ProductEditForm.cshtml` (Cable section and the `DisplaySize` input only); do not touch: service layer, Edit page model, input models, API files, sample data.
- Symbols: Cable section of `_ProductEditForm.cshtml`: selects `Product.Connector1`, `Product.Connector2`, input `Product.CableLength`.
- Current -> Desired: length input has `step="0.1"` so `1.00`/`1.01` fail client validation; connector selects have the `-- Select connector --` placeholder and no `required`. Desired: length accepts two decimals (matches Create's `step="0.01"` and the decimal column); both connector selects require a real choice before submit.
- Change: set `step="0.01"` on the `Product.CableLength` input (keep `min="0"`); also set `step="0.01"` on the `Product.DisplaySize` input (~line 213) — same decimal-vs-step root cause (`Laptop.DisplaySize` is `decimal`); add the `required` attribute to both connector `<select>` elements; keep the placeholder option `value=""`, the existing `asp-validation-for` spans, and the server-side checks unchanged.
- Depends on: none.
- Edge cases / error handling: untouched `1.00` saves without error; `1.01` and `2` save; negative values still blocked by `min`; clearing a connector blocks submit client-side, and if client validation is bypassed the existing server errors still show on the same field.
- Tests: no new unit test (markup only); existing Edit/service tests must stay green. If an Edit page-model test already covers null connectors, leave it as is.
- Verify: CI green; diff shows only the `step` changes on `CableLength` and `DisplaySize` plus `required` on the two connector selects. Manual: `Products/Edit?id=15&type=Cable` -> Save untouched (no length error, "No changes were made." on Details); length `1.01` saves; choose placeholder for either connector -> browser blocks submit.
- Done when: all three manual checks pass and no other file changed.
- Risk: LOW
- Confidence: HIGH (length), MEDIUM (connectors, see Briefing)

## ~~[x] Step 2 — Keep spaced phone numbers in order on RTL invoices~~
- Result: NBSP approach (`ToBidiSafePhone`, commit 3c51d05) FAILED on the rendered factor. Replaced by `PhoneNumberExtensions.ToRtlDisplayPhone()` (commits 1705fd4, 226d6ac): it splits an unspaced 11-digit number 4-3-4, reverses the groups and joins them with single spaces. Applied to `_settings.ShopPhone` in `QuestPdfGenerator.RenderFactorFooter` and to the factor contact phone constant `FactorContactPhone` (`۰۹۱۱۵۰۰۹۱۰۹`), which is stored in natural order.
- Verification: CI #675 and #678 Success. Rendered check: user confirmed the reversed-typed value printed correctly; final-commit render still needs a re-check.
- Risk: MEDIUM
- Confidence: MEDIUM

## [ ] Step 3 — Full validation
- Files: no source changes; update `.agents/chat/act.md` only.
- Symbols: n/a.
- Current -> Desired: record evidence for Steps 1-2.
- Change: wait for GitHub Actions build/test; record the run number, the Step 1 manual results, and the Step 2 rendered-PDF check.
- Depends on: Steps 1-2.
- Edge cases / error handling: if the rendered check shows the order still wrong, report it to the Reviewer instead of guessing further.
- Tests: full suite via CI.
- Verify: CI green; diff touches only the files listed in Steps 1-2.
- Done when: evidence recorded and the Reviewer can sign off without re-running anything.
- Risk: LOW
- Confidence: HIGH

## Global Definition of Done
- Cable edit: untouched and two-decimal lengths save; both connectors must be chosen before submit.
- Spaced phone numbers print in typed order on RTL invoices, verified on a rendered PDF.
- Focused tests and GitHub Actions pass; no out-of-scope files changed; no API, schema, or seed-data changes.
