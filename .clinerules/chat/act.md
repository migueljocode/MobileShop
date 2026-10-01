# Act Report — Stage O Step 2 correction (Buy validation regression)

## Commit hashes
- `546440f` — fix(transactions): keep tag-helper controls on Buy and share only picker options and script
- `b5df8fe` — chore(transactions): remove obsolete _ProductPicker.cshtml
- `bb50120` — docs(act): record Stage O Step 2 correction commit hash

## What changed (Stage O Step 2 correction only)
- **`_ProductPickerOptions.cshtml` (new):** renders only the product `<option>` elements (friendly label: type / name / identifier / color / part number) each with `data-suggested-price` (invariant culture), plus the read-only suggested-price display.
- **`wwwroot/js/product-picker.js` (new):** external script (mirrors `create-product-pricing.js` pattern) wired to `[data-product-picker]` / `[data-finished-price-input]` / `[data-suggested-price-display]`; on product change shows suggested price and sets finished = suggested (documented A2 rule).
- **`Buy.cshtml`:** page keeps its tag-helper controls — `<select asp-for="Input.ProductId">` with the "Select product" `<option value="0">` + the options partial, `<input asp-for="Input.Price">`, all `asp-validation-for` spans, the Seller dropdown and Date — and references `product-picker.js` in the `Scripts` section.
- **`ProductPickerViewModel`:** dropped the unused `FieldPrefix` (the partial no longer emits field names).

## Verified correct
- **Scope:** `git diff --stat -- src/MobileShop.Api src/MobileShop.Web/Pages/Transactions/Sell.cshtml src/MobileShop.Web/Pages/Transactions/Sell.cshtml.cs` → empty (Sell untouched).
- `[Display(Name = "Finished price")]` on `BuyInputModel.Price`; `type="date"` + `Input.Date ??= DateTime.Today` retained; Seller retained.

## Verification
- `dotnet build src/MobileShop.slnx --nologo` → **0 warnings, 0 errors** (build log `/tmp/build_fix.log`).
- `dotnet test src/MobileShop.slnx --nologo --no-build` → **Passed! Failed: 0, Passed: 293, Skipped: 0, Total: 293, Duration: 37s** (test log `/tmp/test_fix.log`). Same count as before.

## Rendered-page evidence (required; unit tests do not cover Razor)
App started detached (`dotnet run --project src/MobileShop.Web --no-build --urls http://127.0.0.1:5199`), `/tmp/MobileShop.db.orig` saved before, restored after.

POST with `Input.ProductId=0` (valid seller, price 100):
```
grep 'The product should be selected.' /tmp/buy_post1.html
The product should be selected.   (×2: in the validation span + validation summary)
```
→ friendly message now rendered.

POST with product 11 selected but `Input.SellerId=0` (price 777):
```
grep '<option value="11"' /tmp/buy_post2.html          # (selected by asp-for after round-trip)
<option value="11" data-suggested-price="800000">
grep  'value="777"' /tmp/buy_post2.html | head -1       # posted price retained
<input class="form-control" ... name="Input.Price" ... value="777" />
grep 'The seller should be selected.' /tmp/buy_post2.html
The seller should be selected.
```
→ selected product and posted price survive a failed post; server-side message rendered.

Client validation attributes present:
```
grep -c 'data-val="true"' /tmp/buy_get.html   # 3 (ProductId, Price, SellerId)
grep 'data-product-picker\|data-finished-price-input\|data-suggested-price-display\|product-picker.js' /tmp/buy_get.html | sort | uniq -c
# 1 data-finished-price-input; 1 data-product-picker; 1 data-suggested-price-display; 1 product-picker.js
```

Process cleanup: `testhost`/Web process killed; `MobileShop.db` md5 restored to original.

## Limitations
- `dotnet test` duration was ~37s; ran detached+pole because the tool times out at 30s.
- Static review by the reviewer can't see this runtime evidence; the report records the curl/grep output above.

## Friction noted
- Tool's default ~30s timeout forces running build/test/app detached with log polling; results are read from `/tmp/*.log`.
- Antiforgery + cookie hand-rolled via curl (no test client); `selected` attribute was on `value="0"` in the no-product-fail post, confirming `asp-for` re-selects correctly on round-trip.

## Problems
None. Correction addresses the HIGH finding; verification green.

## Status
COMPLETE — STOP for Reviewer Job B.
