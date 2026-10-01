# ACTOR PROMPT — Stage O Step 2 correction (restore validation, selected value and posted price on Buy)

> This file is an instruction. After you finish, replace its entire content with your Act Report, using the same format as your previous reports.

You are the ACTOR. Read `.clinerules/actor.md`, `.clinerules/project-specific-rules.md`, `.clinerules/chat/plan.md` (Stage O) and `.clinerules/chat/audit.md` (Job B on Step 2: one HIGH finding). Run `git pull` first. Do **one correction pass for that HIGH finding only**, then STOP. Do not start Step 3. Do not touch the Sell page, `to-do.md`, `plan.md` or `audit.md`.

## Problem
`src/MobileShop.Web/Pages/Shared/_ProductPicker.cshtml` replaced the Buy page's tag-helper controls with hand-built HTML. Result on Buy: no client validation, the Stage L messages ("The product should be selected." etc.) never render (a manual `data-valmsg-for` span is not filled server-side, and the page uses `asp-validation-summary="ModelOnly"`), and after a failed post the selected product and the typed price are lost.

## Task (Web layer only; no service, entity, Api, auth, PDF or schema changes)
1. Restructure so the **page keeps tag helpers** and only genuinely shared parts live in shared files:
   - In `Buy.cshtml` restore: `<select asp-for="Input.ProductId" class="form-select" data-product-picker>` with the "Select product" `<option value="0">`, the product `<option>` list, `<span asp-validation-for="Input.ProductId" class="text-danger">`, and `<input asp-for="Input.Price" class="form-control" data-finished-price-input />` with `<span asp-validation-for="Input.Price" class="text-danger">`. Labels use `asp-for` (so the label reads "Finished price").
   - Shared partial: change `_ProductPicker.cshtml` (or rename it, for example `_ProductPickerOptions.cshtml`) so it renders **only the product `<option>` elements** (friendly text: type / name / identifier / color / part number, each with `data-suggested-price` in invariant culture) plus the read-only **Suggested price** display. It no longer renders the `<select>`, the price `<input>` or any validation span. Adjust `ProductPickerViewModel` only if needed (the `FieldPrefix` property can go if unused).
   - Shared script: move the inline `<script>` into `src/MobileShop.Web/wwwroot/js/product-picker.js` (same pattern as `create-product-pricing.js`). Keep the documented A2 rule (on product change, show the suggested price and set the finished price to it). Use `[data-product-picker]`, `[data-finished-price-input]` and `[data-suggested-price-display]` hooks, and reference it from Buy.cshtml's `Scripts` section next to `_ValidationScriptsPartial`.
2. Keep unchanged: the Seller dropdown, the `type="date"` Date input and `Input.Date ??= DateTime.Today`, the `[Display(Name = "Finished price")]` on `BuyInputModel.Price`.
3. The partial and script must be written so Step 3 can reuse them on the Sell page without changes (do not touch Sell now).

## Verify
- Build and tests as a chain, detached + polled if over the time window: `dotnet build src/MobileShop.slnx --nologo && dotnet test src/MobileShop.slnx --nologo --no-build`. Expected 0 warnings and the same pass count as before (293), plus any test you add.
- **Rendered-page check (required, because unit tests don't cover Razor output):** run the Web app in Development, GET `/Transactions/Buy`, then POST it with a valid antiforgery token and `Input.ProductId=0` (and a valid seller and price). Confirm the response HTML contains "The product should be selected." Then POST with a valid product but `Input.SellerId=0` and confirm the response keeps the posted product `selected` and the posted price in the Price input. Record the commands and the grep evidence (short) in the report. Stop the app afterwards and make sure `MobileShop.db` is not left modified (restore it if it is).
- `git diff --stat -- src/MobileShop.Api src/MobileShop.Web/Pages/Transactions/Sell.cshtml src/MobileShop.Web/Pages/Transactions/Sell.cshtml.cs` is empty.

## Commit and report
- Commit: `git commit -m "fix(transactions): keep tag-helper controls on Buy and share only picker options and script"` (Conventional Commits, no Co-authored-by, never amend, never push).
- Then replace this file's content with your Act Report (commit hash, verification numbers, the rendered-page evidence, limitations, problems) and record the hash in a separate commit: `git commit -m "docs(act): record Stage O Step 2 correction commit hash"`.
- STOP for Job B.
