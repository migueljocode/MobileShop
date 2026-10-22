# Plan — Stage 19 — Cable edit validation and invoice phone display

## Proposed stages
- [ ] Stage 19 — Cable edit validation and invoice phone display

## Reviewer Briefing
- Stage 18 (Phone details by `ProductId`, `SuccessMessage` rendered on Details) is verified in the diff; nothing carried over.
- Step 1 root cause (HIGH confidence): `_ProductEditForm.cshtml` renders the cable length input with `step="0.1"`. The decimal column renders `1.00`; jQuery Validate's `step` rule rejects more decimal places than the step has ("Please enter a multiple of 0.1"). `2` passes because it has no decimals. Create cable already uses `step="0.01"`.
- Step 1 connectors (MEDIUM confidence): at HEAD the server already rejects null/undefined `Connector1`/`Connector2` (`Edit.cshtml.cs` + `UpdateProductAsync`), but the selects have no client-side `required`, so the form submits with the placeholder. Could not reproduce without running the app; if it still reproduces after this step, the Reviewer must ask for exact repro steps.
- Step 2 root cause (MEDIUM confidence): `ShopPhone` is rendered inside `ContentFromRightToLeft()` in a Text with ordinary spaces; spaces are neutral in bidi, so digit groups reorder (`0911 500 9109` -> `9109 500 0911`). Fix needs a rendered-PDF check; unit tests alone cannot prove it.
- Step 2 risk: zero-width marks may render as tofu in Vazirmatn. Preferred approach uses only NBSP (U+00A0); add LRM only if rendering proves it is needed.

## [ ] Step 1 — Cable length step and mandatory connectors (client side)
- Files: inspect: `src/MobileShop.Web/Pages/Shared/_ProductEditForm.cshtml`, `src/MobileShop.Web/Pages/Shared/_ProductCreateCableForm.cshtml`, `src/MobileShop.Web/Pages/Products/Edit.cshtml.cs`, `src/MobileShop.Models/ViewModels/Web/BindModels/EditProductInputModel.cs`; modify: `src/MobileShop.Web/Pages/Shared/_ProductEditForm.cshtml`; do not touch: service layer, Edit page model, input models, API files, sample data.
- Symbols: Cable section of `_ProductEditForm.cshtml`: selects `Product.Connector1`, `Product.Connector2`, input `Product.CableLength`.
- Current -> Desired: length input has `step="0.1"` so `1.00`/`1.01` fail client validation; connector selects have the `-- Select connector --` placeholder and no `required`. Desired: length accepts two decimals (matches Create's `step="0.01"` and the decimal column); both connector selects require a real choice before submit.
- Change: set `step="0.01"` on the `Product.CableLength` input (keep `min="0"`); add the `required` attribute to both connector `<select>` elements; keep the placeholder option `value=""`, the existing `asp-validation-for` spans, and the server-side checks unchanged.
- Depends on: none.
- Edge cases / error handling: untouched `1.00` saves without error; `1.01` and `2` save; negative values still blocked by `min`; clearing a connector blocks submit client-side, and if client validation is bypassed the existing server errors still show on the same field.
- Tests: no new unit test (markup only); existing Edit/service tests must stay green. If an Edit page-model test already covers null connectors, leave it as is.
- Verify: CI green; diff shows only the three attribute changes in the Cable section. Manual: `Products/Edit?id=15&type=Cable` -> Save untouched (no length error, "No changes were made." on Details); length `1.01` saves; choose placeholder for either connector -> browser blocks submit.
- Done when: all three manual checks pass and no other file changed.
- Risk: LOW
- Confidence: HIGH (length), MEDIUM (connectors, see Briefing)

## [ ] Step 2 — Keep spaced phone numbers in order on RTL invoices
- Files: inspect: `src/MobileShop.Services/PDF/Configuration/QuestPdfGenerator.cs`, `src/MobileShop.Models/Extensions/MoneyExtensions.cs`, `src/MobileShop.Tests/Services/PDF/QuestPdfGeneratorTests.cs`; create: `src/MobileShop.Models/Extensions/PhoneNumberExtensions.cs`, `src/MobileShop.Tests/Models/PhoneNumberExtensionsTests.cs` (use the Tests folder where `MoneyExtensions` tests already live); modify: `QuestPdfGenerator.cs`, `QuestPdfGeneratorTests.cs`; do not touch: English-layout phone lines, PDF settings, appsettings, API files.
- Symbols: new `PhoneNumberExtensions.ToBidiSafePhone(this string value)`; call sites in `QuestPdfGenerator`: the factor footer `text.Span(_settings.ShopPhone)` (RTL footer, ~line 371), `ComposePagePersian` party line `تلفن : {p.PartyPhone}` (~line 575), and the Persian footer `| {_settings.ShopPhone}` (~line 628).
- Current -> Desired: spaces inside a phone are neutral characters, so the bidi algorithm reverses the digit groups in RTL text. Desired: the phone prints exactly as typed (`0911 500 9109`, Persian or Latin digits), left-to-right.
- Change: `ToBidiSafePhone` trims the value, collapses each whitespace run into a single no-break space (U+00A0) so the whole number stays one digit run, leaves digits and `+` untouched, and returns an empty string for null/blank. Apply it at the three RTL call sites only. Do not convert digits (no `ToPersianDigits`). Only if the rendered check below still shows a leading `+` on the wrong side, additionally wrap the result in LRM (U+200E) and re-check for missing glyphs.
- Depends on: none (independent of Step 1).
- Edge cases / error handling: null/blank phone keeps the current "skip the line" behavior; tabs/multiple spaces collapse to one; hyphenated numbers (`0911-500-9109`) pass through unchanged; Persian digits typed by the user stay Persian; value is never mutated in settings or stored data.
- Tests: unit tests for `ToBidiSafePhone` (spaced Latin, spaced Persian digits, multiple spaces, blank/null, `+98 911 500 9109`); in `QuestPdfGeneratorTests` add a generation case with `ShopPhone = "0911 500 9109"` asserting a non-empty PDF and no exception.
- Verify: CI green. Rendered check (required): generate the Persian factor with `ShopPhone` set to `0911 500 9109` and to the Persian-digit equivalent, rasterize page 1 (e.g. `pdftoppm -r 150`) and confirm the footer reads `0911 500 9109` in that order and no tofu boxes appear. Record the result in `act.md`.
- Done when: both spaced variants render in the typed order on the factor and Persian invoice; unit tests pass; only the listed files changed.
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
