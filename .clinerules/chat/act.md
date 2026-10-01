# Act Report — Stage O Step 3 (Sell on shared picker + selected round-trip fix)

## Commit
`a7e9419` — feat(transactions): Sell on shared product picker + selected-product round-trip fix

## What changed (Stage O Step 3)
- `_ProductPickerOptions.cshtml`: added `var price` and an `if/else` block rendering the matching `<option>` with a literal `selected` attribute (no `@` expression in the tag-helper attribute area — that triggered RZ1031) and all others without it. This fixes the carried-over MEDIUM: the posted product is now re-selected after a failed post on **both** Buy and Sell.
- `ProductPickerViewModel`: added `int SelectedProductId = 0` so the partial knows which option is current.
- `Buy.cshtml`: passes `Model.Input.ProductId` to the partial (enables Buy re-selection too).
- `Sell.cshtml`: replaced the hand-written product options with the shared partial (`FieldPrefix`-free `new ProductPickerViewModel(Model.Products, Model.Input.ProductId)`), added the `data-finished-price-input` Price input + `asp-validation-for` span, kept the Customer select, Date (`type="date"`) and Back button, and loaded `product-picker.js` in `Scripts`.
- `Sell.cshtml.cs`: `OnGetAsync` now sets `Input.Date ??= DateTime.Today`.
- `SellInputModel.Price`: added `[Display(Name = "Finished price")]`.
- `RecordModelTests.cs`: added `Sell_OnGet_defaults_date_to_today` (mirrors the Buy test).

## Reused unchanged from Step 2
- `_ProductPickerOptions.cshtml`, `wwwroot/js/product-picker.js`, `ProductPickerViewModel` (only extended, not restructured).

## Verification
- `dotnet build src/MobileShop.slnx --nologo` → **0 warnings, 0 errors**
- `dotnet test src/MobileShop.slnx --nologo --no-build` → **Passed! Failed: 0, Passed: 294, Skipped: 0, Total: 294, Duration: 42s** (293 + 1 new test)

## Rendered-page evidence (Razor not covered by unit tests)
App detached; `/tmp/MobileShop.db.step3` saved before, restored after (md5 `85a239ceab685ea73e9d03646b818ad6` verified identical to original).

Sell GET (`/Transactions/Sell`):
```
grep -c 'data-val="true"' sell_get.html          # 3 (ProductId, CustomerId, Price)
grep 'data-product-picker|data-finished-price-input|data-suggested-price-display|product-picker.js' sell_get.html | sort | uniq -c
# 1 each; product-picker.js bundled as product-picker.yhj6tab3vg.js
grep -o 'Finished price' sell_get.html | head -1  # label present
```

Sell POST `ProductId=0`:
```
grep 'The product should be selected.' sell_post1.html
The product should be selected.
grep 'The customer should be selected.' sell_post1.html
The customer should be selected.
```

Sell POST valid product (16) + `CustomerId=0` + `Price=250`:
```
grep -o '<option value="16"[^>]*selected[^>]*>' sell_post2.html
<option value="16" data-suggested-price="800000" selected>
grep -o 'name="Input.Price"[^>]*value="[^"]*"' sell_post2.html
name="Input.Price" value="250"
grep 'The customer should be selected.' sell_post2.html
The customer should be selected.
```
→ posted product re-selected, posted price retained, server-side message rendered.

Buy POST (same fix) with product 11 + `SellerId=0` + `Price=777`:
```
grep -o '<option value="11"[^>]*selected[^>]*>' buy_post2.html
<option value="11" data-suggested-price="800000" selected>
grep -o 'name="Input.Price"[^>]*value="[^"]*"' buy_post2.html
name="Input.Price" value="777"
grep 'The seller should be selected.' buy_post2.html
The seller should be selected.
```

`git diff --stat -- src/MobileShop.Api` → 0 changes.

## Scope
Untouched: `src/MobileShop.Api`, any service/entity/schema/auth/PDF file, `to-do.md`, `plan.md`.

## Limitations
- `dotnet build`/`dotnet test` run past the tool's ~30s window → launched detached with `setsid nohup` and polled `/tmp/*.log`.
- Static reviewer review can't run the app; curl/grep output above is the rendered-page evidence.

## Friction noted
- RZ1031: any `@` expression in a tag-helper `option`'s attribute declaration area fails to compile. `value="@…"` and `data-suggested-price="@price"` are inside quoted values so they're fine, but a bare `@Html.Raw(...)` or `@selected` attribute is blocked → resolved by an `if/else` choosing between two `<option>` lines, one with a literal `selected`.
- The tool's editor auto-indents `var` lines under `@foreach`, fighting manual alignment → normalized with a python pass.
- `pkill -f "dotnet…"` self-terminates the shell command via SIGTERM because the pattern matches the shell's own command line → used a separate pgrep pass to confirm the Web process was gone.

## Problems
None. Build and suite green; rendered-page evidence records the selected-fix on both pages.

## Status
COMPLETE — STOP for Reviewer Job B (Stage O Step 3).
