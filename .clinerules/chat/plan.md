# Plan — Stage O — Transactions Buy/Sell form rework

## Reviewer Briefing

- First open stage in `to-do.md` after Stage N.
- Goal: friendlier **shared product picker**, clear **Finished price** vs **suggested catalog price** (`Product.Price`), **date defaults to today**. Buy keeps seller; Sell keeps customer only (no seller).
- Today: plain `<select>` of products; label is “Price” with no suggested price; date unbound/empty on first load.
- `ProductListItemViewModel` has **no price field** — must extend it (init-only, default-safe) and fill in `GetSelectableProductsAsync` projections.
- Prefer **one Razor partial** used by both Buy and Sell for the product dropdown + suggested-price display.
- No schema/migration; no Api project; no auth; no PDF redesign (Stage P).

## Assumptions (labeled)

- **A1:** Transaction line **Price** bind property name stays `Price` on `BuyInputModel` / `SellInputModel`. UI label becomes **Finished price**. Display-only **Suggested price** = inventory `Product.Price` for the selected product (read-only, not posted).
- **A2:** Selecting a product **prefills** Finished price from suggested price; user may still override. Changing product again overwrites only if the user has not edited finished price **or** always overwrite — **prefer always set finished = suggested on product change** (simpler KISS; document in UI).
- **A3:** Date control is `type="date"` (or datetime if repo already uses full DateTime consistently). On GET, if `Input.Date` is null, set **today** (local/date-only as the page already stores).
- **A4:** Shared partial owns product `<select>` markup + optional small script hook; party dropdowns (seller/customer) stay page-local.
- **A5:** “Friendlier” in this stage means: clearer option text (type, name, identifier, color, part number if useful), suggested price beside the control, optional `data-suggested-price` on options — **not** a full typeahead library unless already in the solution.

## [ ] Step 1 — Suggested price on selectable products

- Files
  - `ProductListItemViewModel.cs` — add init-only `decimal? SuggestedPrice` (or `CatalogPrice`) default null so existing constructions stay compatible
  - `TransactionsDataService.GetSelectableProductsAsync` — project `Product.Price` into that field for phone and Apple ID rows
  - Tests: assert selectable products carry price when product has price
  - Do not touch RecordBuy/RecordSell logic yet beyond what projections need

- Done when: list items expose suggested price; suite focused green; Job B PASS.

- Risk: LOW
- Confidence: HIGH

## [ ] Step 2 — Shared product-picker partial + Buy page

- Files
  - **Create** e.g. `Pages/Shared/_ProductPicker.cshtml` (parameters: products list, asp-for ProductId name/id)
  - Options: include `data-suggested-price="@p.SuggestedPrice"` (invariant culture)
  - Read-only **Suggested price** span/input next to picker
  - Small script (inline in partial or `wwwroot/js/product-picker.js`): on change, show suggested + set Finished price input to that value
  - `Buy.cshtml`: use partial; label **Finished price** for `Input.Price`; date input default today; keep **Seller**
  - `Buy.cshtml.cs`: on GET set `Input.Date ??= DateTime.Today` (or DateTime.Now.Date)
  - Display attribute / label on bind model optional (`[Display(Name = "Finished price")]`)

- Done when: Buy form shows picker + suggested + finished label + date default; Job B PASS.

- Risk: MEDIUM
- Confidence: HIGH

## [ ] Step 3 — Sell page uses same partial

- Files: `Sell.cshtml` / `.cs` only
- Same finished label, date default, shared partial; **Customer** stays; **no seller field**
- Do not fork picker markup

- Done when: Sell matches Buy UX for product/price/date; Job B PASS.

- Risk: LOW
- Confidence: HIGH

## [ ] Step 4 — Final Stage O validation

- Full build + test; smoke Buy/Sell GET if feasible
- Checklist: shared partial once; suggested vs finished distinct; date today; Buy seller / Sell customer; no Api/auth/PDF/schema scope creep
- Reviewer ticks Stage O in `to-do.md` only after PASS

## Global Definition of Done

- One shared product picker partial on Buy and Sell
- Finished price labeled; suggested catalog price visible and drives prefill
- Date defaults to today on GET
- Party fields unchanged in role (Buy seller, Sell customer)
- Build/test green; Stage O checked

## Execution notes

One step → one commit → `act.md` → **STOP** for Job B. Do not edit plan/audit/todo. Do not touch `src/MobileShop.Api`.

**Step 1 is ready for Job A** (reviewer approval before Act).
