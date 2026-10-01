# Audit — Job B (Execution Check): Stage O Step 2 correction (`546440f`, `b5df8fe`)

**Reviewed**: `bee5ba9` → `b5df8fe` against the Step 2 Job B HIGH finding and the correction prompt.
**Method**: static review only (the sandbox can't restore NuGet packages or run the app). I rely on the actor's recorded curl/grep evidence, which I read critically. Re-run the build and tests locally to confirm the reported 293 passed.

**Verdict: PASS (corrections closed, with one MEDIUM carried into Step 3). The original HIGH is resolved; Step 3 is authorized with the carry-over below as a mandatory part of it.**

## The HIGH finding is resolved
- **Tag helpers restored on Buy:** `<select asp-for="Input.ProductId">`, `<input asp-for="Input.Price">` and `asp-validation-for` spans are back. The evidence shows `data-val="true"` on 3 controls (client validation restored) and the friendly messages "The product should be selected." and "The seller should be selected." rendered server-side.
- **Posted price is kept:** `value="777"` appears in the re-rendered Price input after a failed post.
- **Shared parts are now shared correctly:** `_ProductPickerOptions.cshtml` emits only the options plus the suggested-price display, and `wwwroot/js/product-picker.js` holds the behaviour (loaded on DOMContentLoaded). The old `_ProductPicker.cshtml` and the unused `FieldPrefix` are gone. Sell, Api, auth, PDF and schema are untouched.

## Finding

### MEDIUM — the posted product is probably still not re-selected after a failed post (carry into Step 3)
The actor's own evidence line for the product-11 failed post is `<option value="11" data-suggested-price="800000">`: **no `selected` attribute**. The comment "(selected by asp-for after round-trip)" is not supported by it. Its friction note says the `selected` attribute was found on `value="0"`, but that option lives in `Buy.cshtml` itself. The `<option>` elements that carry the products come from the partial, and the `OptionTagHelper` only gets the select's current value when the options are rendered in the same view, not in a separately executed partial. So the select probably falls back to "Select product" while the price keeps the typed value. That is a mismatch, though no error message is hidden and no data is lost, so I rate it MEDIUM rather than HIGH.
**Fix (cheap, applies to Buy and Sell, so do it in Step 3):** pass the selected id into the partial (for example `ProductPickerViewModel(Products, SelectedProductId)` with `Input.ProductId`) and render `selected="@(product.ProductId == Model.SelectedProductId ? "selected" : null)"` on each option. Then re-check it in the rendered page: POST with a valid product and `SellerId=0`/`CustomerId=0` and confirm the posted product's `<option>` carries `selected`.

## Findings (LOW)
- **Commit hygiene:** `546440f` still contains the old `_ProductPicker.cshtml` (which referenced the removed `FieldPrefix`), so that commit doesn't build on its own. The deletion landed in the follow-up `b5df8fe`. Harmless on `main`, but it hurts bisecting.
- **Act report mismatch:** the report names the report commit as `8a1c2f9`, but the real commit is `e8b82a7`. `act.md` also still carries the correction-prompt text below the report.
- **Open from the first audit:** the comma-decimal culture check on the prefilled price is still unverified; do it once in Step 3.

## Gate
**Step 3 (Sell page uses the same partial) is authorized**, with the MEDIUM carry-over above made part of Step 3: the partial must render `selected`, and the actor must show the rendered-page evidence for both Buy and Sell.

## Reviewer checklist
- [x] Job B; one verdict; only `audit.md` written; `to-do.md` untouched.
- [x] No further correction loop on Step 2: the remaining item is MEDIUM and rides with Step 3.
