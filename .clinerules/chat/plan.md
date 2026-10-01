# Plan — Stage O — Transactions Buy/Sell form rework

## Assumptions
- **A1** The label reads "Finished price"; the bound property stays `Price` (no rename of `Input.Price`).
- **A2** On product change the suggested (catalog) price is shown read-only and the finished price is set to it; the user may overwrite it afterwards. The server stays the source of truth.
- **A3** Date inputs are `type="date"` and default to today on GET.
- **A4** Shared pieces: one options partial and one script; pages keep their own tag-helper controls (so client/server validation, the selected value and the posted price survive round-trips).
- **A5** No typeahead library.
- **A6** Buy keeps the external Seller dropdown; Sell has no seller field (the shop is the seller).

## Reviewer Briefing
- Step 3 is MEDIUM risk / MEDIUM confidence: Razor output is not covered by unit tests, so the actor must record rendered-page evidence for Buy **and** Sell.
- Open carry-over from the Step 2 Job B: the posted product was probably not re-selected after a failed post, because the `<option>` list comes from a partial. Fix it in Step 3 for both pages.
- Do not fork the picker: Sell must reuse `_ProductPickerOptions.cshtml` and `wwwroot/js/product-picker.js` unchanged apart from the selected-value fix.

## ~~[x] Step 1 — Suggested price on selectable products~~
- **Done** — Job B PASS (`8660d69`). `SuggestedPrice` on the view model and the selectable-product projections.

## ~~[x] Step 2 — Shared product-picker partial + Buy page~~
- **Done** — Job B PASS after one correction (`bee5ba9`, `546440f`, `b5df8fe`). Delivered: `Pages/Shared/_ProductPickerOptions.cshtml`, `wwwroot/js/product-picker.js`, Buy on tag-helper controls, `[Display(Name = "Finished price")]` on `BuyInputModel.Price`, date default today.
- Carried into Step 3: selected-product re-selection after a failed post.

## ~~[x] Step 3 — Sell page on the shared picker (+ selected-product fix on both pages)~~
- Files
  - inspect: `src/MobileShop.Web/Pages/Transactions/Sell.cshtml`, `Sell.cshtml.cs`, `Buy.cshtml`, `Pages/Shared/_ProductPickerOptions.cshtml`, `wwwroot/js/product-picker.js`, `ProductPickerViewModel.cs`, `SellInputModel.cs`, `RecordModelTests.cs`
  - modify: `ProductPickerViewModel.cs`, `_ProductPickerOptions.cshtml`, `Buy.cshtml`, `Sell.cshtml`, `Sell.cshtml.cs`, `SellInputModel.cs`, `RecordModelTests.cs`
  - do not touch: `src/MobileShop.Api`, any service, entity, schema, PDF, auth, `to-do.md`
- Symbols
  - `ProductPickerViewModel(IReadOnlyList<ProductListItemViewModel> Products)` → add `int SelectedProductId` as the second parameter.
  - `_ProductPickerOptions.cshtml`: on each `<option>` render `selected` when `product.ProductId == Model.SelectedProductId` (use a Razor conditional attribute so it is omitted otherwise).
  - `Buy.cshtml`: pass `Model.Input.ProductId` as the selected id.
  - `Sell.cshtml`: replace the hand-written product options and plain price input with the same pattern as Buy: `<select asp-for="Input.ProductId" data-product-picker>` + "Select product" `<option value="0">` + the options partial; `<input asp-for="Input.Price" data-finished-price-input />` with its `asp-validation-for` span; keep the Customer select, the Date input (`type="date"`) and the Back button; add `<script src="~/js/product-picker.js"></script>` in the `Scripts` section next to `_ValidationScriptsPartial`.
  - `SellModel.OnGetAsync`: `Input.Date ??= DateTime.Today` before loading selections.
  - `SellInputModel.Price`: add `[Display(Name = "Finished price")]`.
- Current → Desired: Sell shows the same picker, suggested price, "Finished price" and a date defaulting to today as Buy; after a failed post both Buy and Sell keep the posted product selected and the posted price.
- Edge cases
  - Products with no suggested price: option carries an empty `data-suggested-price`; the script leaves the finished price untouched.
  - Sell must not gain a seller field.
  - Do not change the recorded-price semantics: the server still validates and records `Input.Price`.
- Tests: add `Sell_OnGet_defaults_date_to_today` (mirror the Buy test); no Razor-markup unit tests.
- Verify
  - `dotnet build src/MobileShop.slnx --nologo && dotnet test src/MobileShop.slnx --nologo --no-build` (run detached and poll if over the tool time window). Expect 0 warnings and all tests passing.
  - **Rendered-page check (required for both pages):** run the Web app in Development, GET the page, POST with a valid antiforgery token:
    1. Buy: `ProductId=0` → response contains "The product should be selected."; valid product + `SellerId=0` → the posted product's `<option>` carries `selected`, the Price input keeps its posted value, and "The seller should be selected." renders.
    2. Sell: `ProductId=0` → "The product should be selected."; valid product + `CustomerId=0` → the posted product's `<option>` carries `selected`, the Price keeps its posted value, and "The customer should be selected." renders.
    3. Confirm both GET pages contain `data-val="true"` controls and the single `product-picker.js` reference.
    Record the commands and the short grep output in `act.md`; restore `MobileShop.db` afterwards and stop the app.
  - `git diff --stat -- src/MobileShop.Api` is empty and no other Sell-unrelated service/entity file changed.
- Done when: Sell uses the shared options partial and script, both pages re-select the posted product after a failed post, the rendered-page evidence is recorded, the build is clean and the suite passes; Job B PASS.
- Risk: MEDIUM
- Confidence: MEDIUM

## [ ] Step 4 — Final Stage O validation
- Files: none expected (validation only).
- Verify (record the evidence in `act.md`)
  - Clean build with 0 warnings: `dotnet clean src/MobileShop.slnx --nologo && dotnet build src/MobileShop.slnx --nologo --no-incremental && dotnet test src/MobileShop.slnx --nologo --no-build`.
  - One manual check of the prefilled finished price with the app's configured culture (a comma-decimal culture could break binding of a value like `800000.5`); report the result.
  - Confirm Buy and Sell both show Product, Suggested price, Finished price, Date (default today) and Back; Buy keeps Seller, Sell has no seller.
- Done when: all of the above pass and are recorded; the reviewer signs Stage O off and ticks it in `to-do.md` (only the reviewer ticks).
- Risk: LOW
- Confidence: HIGH

## Global Definition of Done
- Buy and Sell use the shared picker (options partial + script) with the page-owned tag-helper controls.
- Client and server validation messages render; the posted product and price survive a failed post on both pages.
- "Finished price" label, the suggested price clearly separate, date defaulting to today, Back button on both pages.
- Build is clean, the full suite passes, no Api/auth/PDF/schema changes.

## Execution notes
**Step 3 is authorized.** One commit → `act.md` → STOP for Job B. Work compact: chain dependent commands with `&&`, group read-only checks in one `{ ...; }` call, and run build/test/app detached (`setsid nohup … &`) with log polling because the tool window is about 30 s. Never amend, never push, do not touch `to-do.md`.
