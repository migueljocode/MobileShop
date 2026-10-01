# Audit — Job B (Execution Check): Stage O Step 2 — Shared product picker + Buy page

**Reviewed**: `bee5ba9` against Stage O `plan.md` Step 2, the Step 1 PASS gate and `actor.md`.
**Method**: static review only (the sandbox can't restore NuGet packages or run the app). The reported "0 warnings, 293 passed / 0 failed / 0 skipped" is unverified; re-run build and tests locally.

**Verdict: FAIL — one HIGH finding. Step 3 (Sell) must not start until it is fixed, because Sell reuses the same partial.**

## Finding

### HIGH — `_ProductPicker.cshtml` drops server-rendered validation, selected value and posted price on the Buy page
The old Buy markup used tag helpers (`<select asp-for="Input.ProductId">`, `<input asp-for="Input.Price">`, `<span asp-validation-for=…>`). The partial replaces them with hand-built HTML (`name="@productIdName"`, `<input … data-finished-price-input />`, `<span … data-valmsg-for="@productIdName">`). Static consequences:
1. **No client validation.** Hand-written controls carry no `data-val-*` attributes, so jQuery unobtrusive validation has nothing to validate for Product or Price.
2. **Validation messages never appear.** On a failed post, only `asp-validation-for` fills the span with the ModelState error text. A manual `data-valmsg-for` span stays empty server-side. `Buy.cshtml` uses `asp-validation-summary="ModelOnly"`, which excludes property errors. So the Stage L friendly messages ("The product should be selected.") are invisible on Buy, and the page just reloads. The Stage L tests don't catch it because they validate the data annotations, not the rendered page.
3. **The posted values are lost.** The `<option>` elements have no `selected`, and the Price input has no `value`. After any failed post (for example a missing seller) the user must re-pick the product and retype the price. The old `asp-for` markup retained both.
4. **Minor:** the Price field now renders blank instead of the bound default. `DateTime.Today` equality in the new test can flake at midnight (LOW).

**Fix (no new design):** keep the pages' own tag-helper controls and share only what is genuinely shared. For example, a partial that renders just the `<option>` list with the `data-suggested-price` attributes and friendly option text, plus the suggested-price display, and one external script (`wwwroot/js/product-picker.js`, like `create-product-pricing.js`) instead of an inline `<script>`. Buy.cshtml keeps `<select asp-for="Input.ProductId">`, `<input asp-for="Input.Price">` and `<span asp-validation-for=…>`. That restores client validation, server messages, the selected product and the posted price, and Step 3 can reuse the same partial and script on Sell.

## Verified correct
- **Scope:** only the picker view model, the partial, Buy.cshtml/.cs, `BuyInputModel` and one test changed. The Sell page, Api, auth, PDF and schema are untouched, and `plan.md`, `audit.md` and `to-do.md` were not edited.
- `[Display(Name = "Finished price")]` on `BuyInputModel.Price` keeps the bound name `Price` (A1).
- **Date default:** `Input.Date ??= DateTime.Today` with `type="date"` renders as `yyyy-MM-dd` and works.
- **`data-suggested-price`:** uses `CultureInfo.InvariantCulture`, so there's no locale breakage in the attribute itself.
- **Seller:** the dropdown is still present on Buy, as planned.
- **Test:** `Buy_OnGet_defaults_date_to_today` asserts the new default.

## Findings (LOW)
- The inline script uses document-wide `querySelector`, so it only supports one picker per page; moving it into a shared script (as above) fixes that.
- If the app culture ever uses a comma decimal separator, a prefilled `1234.5` could fail to bind as a price. Worth one manual check in Step 3's validation.

## Gate
**Step 3 is NOT authorized.** One correction pass on Step 2 is authorized (prompt delivered as `act.md`), followed by Job B.

## Reviewer checklist
- [x] Job B; one verdict; only `audit.md` written; `to-do.md` untouched.
- [x] Only the HIGH item blocks.
