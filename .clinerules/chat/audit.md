# Act Report — Stage O Step 2 correction (Buy validation regression)

## Commit hashes
- `546440f` — fix(transactions): keep tag-helper controls on Buy and share only picker options and script
- `b5df8fe` — chore(transactions): remove obsolete _ProductPicker.cshtml
- `8a1c2f9` — docs(act): record Stage O Step 2 correction commit hash

## What changed (correction of the HIGH finding, Web layer only)
- **Removed** `src/MobileShop.Web/Pages/Shared/_ProductPicker.cshtml` (the hand-built HTML that replaced asp-for controls).
- **Created** `src/MobileShop.Web/Pages/Shared/_ProductPickerOptions.cshtml`: renders only the product `<option>` elements (friendly label: type / name / identifier / color / part number) with `data-suggested-price` in invariant culture, plus the read-only suggested-price display element `[data-suggested-price-display]`. Does not render any `<select>`, price input, or validation span.
- **Created** `src/MobileShop.Web/wwwroot/js/product-picker.js`: external script referencing it from Buy.cshtml's `Scripts` section next to `_ValidationScriptsPartial`. Uses `[data-product-picker]` / `[data-finished-price-input]` / `[data-suggested-price-display]` hooks and applies the documented A2 rule (on product change, show the suggested price and set the finished price to it).
- **`Buy.cshtml`**: keeps asp-for tag helpers — original select `Product`, `Finished price` label+input, and `Seller` dropdown — and only pulls the shared `<option>` list from the partial.
- **`ProductPickerViewModel`**: removed the unused `FieldPrefix` property (the partial no longer emits field names).

## Verified correct
- Scope: `git diff --stat -- src/MobileShop.Api src/MobileShop.Web/Pages/Transactions/Sell.cshtml src/MobileShop.Web/Pages/Transactions/Sell.cshtml.cs` → empty.
- `[Display(Name = "Finished price")]` on `BuyInputModel.Price`, `type="date"` + `Input.Date ??= DateTime.Today`, and the Seller dropdown are unchanged.

## Verification
- `dotnet build src/MobileShop.slnx --nologo` → **0 warnings, 0 errors**
- `dotnet test src/MobileShop.slnx --nologo --no-build` → **Passed! Failed: 0, Passed: 293, Skipped: 0, Total: 293, Duration: 37s**

## Rendered-page evidence (unit tests do not cover Razor)
App run detached (`dotnet run --project src/MobileShop.Web --no-build --urls http://127.0.0.1:5199`); `MobileShop.db` md5 saved before, restored after.

POST with `Input.ProductId=0` (valid seller, price 100):
```
$ grep 'The product should be selected.' /tmp/buy_post1.html
The product should be selected.
```
Friendly Stage-L message now renders.

POST with product 11 selected but `Input.SellerId=0` (price 777):
```
$ grep '<option value="11"' /tmp/buy_post2.html
<option value="11" data-suggested-price="800000">
$ grep 'value="777"' /tmp/buy_post2.html | head -1
<input ... name="Input.Price" ... value="777" />
$ grep 'The seller should be selected.' /tmp/buy_post2.html
The seller should be selected.
```
Selected product and posted price survive a failed post; server-side message rendered.

Client validation attributes present (3):
```
$ grep -c 'data-val="true"' /tmp/buy_get.html
3
$ grep 'data-product-picker\|data-finished-price-input\|data-suggested-price-display\|product-picker.js' /tmp/buy_get.html | sort | uniq -c
      1 data-finished-price-input
      1 data-product-picker
      1 data-suggested-price-display
      1 product-picker.js
```

## Limitations
- `dotnet test` takes ~37s, over the tool's ~30s window → build/test/app launched detached and polled from /tmp logs.
- Reviewer static review cannot see runtime evidence; the curl/grep output above records it.

## Friction noted
- `dotnet build`/`dotnet test` hang through the tool even after completion → detour via `setsid nohup … &` + poll log.
- Antiforgery token + cookie hand-rolled via curl (no in-proc test client).

## Problems
None. Correction addresses the HIGH finding; verification green.

## Status
COMPLETE — STOP for Reviewer Job B.
