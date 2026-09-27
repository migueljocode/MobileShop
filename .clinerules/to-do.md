# To-do — UI/UX Enhancements (7 Actionable Demands — Step 7 Already Implemented)

## Reviewer Briefing
- **HIGH-risk steps**: Step 1 (Back button URL mapping for Apple ID), Step 2 (Manufacturer/Model dropdowns with add-new modals) — requires new repo queries, AJAX/modal wiring, cascading dropdown logic, explicit modal POST handlers, and server-side category resolution; Step 3 (profit estimation dual-field) — requires JS calculation binding on two pages with nullable validation; Step 5 (sorting on People pages) — requires query changes, URL state, header UI, new web test files; Step 6 (Distribution sort) — requires updating 4 existing test blocks that currently expect insertion order.
- **MEDIUM-confidence steps**: Step 4 (count columns) — touches list VMs, service projections (both sync + async via repo Select, no DbContext in services), requires subquery counts to avoid client eval, and explicit decision on distinct vs raw count + soft-delete handling.
- **LOW-risk steps**: Steps 8 — pure Razor/HTML/CSS changes.
- **NOTE**: Step 7 was already implemented in the repository (manual mode defaults to earliest transaction date / today). Not an actionable step.

---

## ~~[x] Step 1 — Add "Back to Products" button on Product Details page~~
   Verified: `dotnet build src/MobileShop.slnx --nologo` → Build succeeded, 0 Warning(s), 0 Error(s) (re-run by the reviewer). Change: 1 line in `Details.cshtml` inside the `@if (Model.Product is not null)` guard — `<a asp-page="/Products/Index" asp-route-type="@(Model.Product.Type == "Apple ID" ? "appleid" : "phone")" class="btn btn-outline-secondary mb-3">Back to Products</a>`; no other source file touched. Committed as `feat(web): add "Back to Products" button on Product Details page`.

---

## ~~[x] Step 2 — Manufacturer & Model dropdowns with "Add New" on Create Phone page~~
     Verified: `dotnet build src/MobileShop.slnx --nologo && dotnet test src/MobileShop.slnx --nologo --filter "CreatePhoneModelTests"` → Build succeeded, 0 Warning(s), 0 Error(s); 9 passed, 0 failed. Committed as `feat(web): manufacturer & model dropdowns with add-new modals on Create Phone page`

---

## ~~[x] Step 3 — Profit estimation (percent ↔ dollars) on Create Phone & Create Apple ID~~
     Verified: `dotnet build src/MobileShop.slnx --nologo` → Build succeeded, 0 Warning(s), 0 Error(s). Committed as `feat(web): add profit estimation (percent ↔ dollars) fields on Create Phone & Create Apple ID pages`

---

## ~~[x] Step 4 — Purchased/Sold count columns on Customers & Sellers list pages~~
   Verified: `dotnet build src/MobileShop.slnx --nologo && dotnet test src/MobileShop.slnx --nologo --filter "CustomerDataServiceTests|SellerDataServiceTests"` → Build succeeded, 0 Warning(s), 0 Error(s); 6 passed, 0 failed. Committed as `feat(web): add Purchased/Sold count columns on Customers & Sellers list pages`

---

## ~~[x] Step 5 — Sorting on Customers & Sellers pages (Name, Phone, Count)~~
   Verified: commit `3de5feb` — `dotnet build src/MobileShop.slnx --nologo` → 0 Warning(s), 0 Error(s); `dotnet test src/MobileShop.slnx --nologo --filter "CustomerDataServiceTests|SellerDataServiceTests|CustomersModelTests|SellersModelTests"` → 14 passed, 0 failed. Full suite → 405 passed, 0 failed, 2 skipped (pre-existing QuestPDF skips). Intentional deviation: the Api stubs did need a one-line sync `GetListRows` signature fix (CS0535 — the plan wrongly said no edit needed); both still throw `NotImplementedException`.

---

## ~~[x] Step 6 — Reports page: Total Profit sign/color + Distribution sort by Share%~~
   Verified: commit `2340390` — `dotnet build src/MobileShop.slnx --nologo` → 0 Warning(s), 0 Error(s); `dotnet test src/MobileShop.slnx --nologo --filter "DistributionCalculatorTests"` → 11 passed, 0 failed. Full suite → 405 passed, 0 failed, 2 skipped (pre-existing QuestPDF skips). Intentional deviation: deleting the early `return rows;` alone broke control flow (the loss branch fell through and appended the profit rows, producing 6 rows), so the profit block is gated with `if (totalProfit > 0)` — this keeps the plan's single-sorted-exit requirement. Manual visual check of the Reports page was NOT performed (headless environment); noted as the one outstanding item.

---

## ~~[x] Step 7 — Already Implemented (Manual Date Range Defaults)~~
- Manual date range defaults already work: `ResolveBoundsAsync` calls `GetEarliestTransactionDateAsync()`, sets `From = EffectiveFrom; To = EffectiveTo;`. Tests pass. No changes needed.

---

## [ ] Step 8 — Sticky/fixed navbar on scroll
- **Files**: 
  - modify: `src/MobileShop.Web/Pages/Shared/_Layout.cshtml`
  - modify: `src/MobileShop.Web/wwwroot/css/site.css`
- **Symbols**: `<nav class="navbar ...">` — add `sticky-top` Bootstrap class; ensure `z-index` > content
- **Current → Desired**: Navbar scrolls away → stays fixed at top with shadow.
- **Change**: 
  1. In `_Layout.cshtml` line 14: change `<nav class="navbar navbar-expand-sm navbar-toggleable-sm navbar-light bg-white border-bottom box-shadow mb-3">` to `<nav class="navbar navbar-expand-sm navbar-toggleable-sm navbar-light bg-white border-bottom box-shadow mb-3 sticky-top" style="z-index: 1030;">`
  2. In `site.css`: append `padding-top: 56px;` to the existing `body { margin-bottom: 60px; }` block (do not add a second `body` selector).
- **Edge cases**: 
  - Mobile collapse: sticky works with Bootstrap 5 `sticky-top`
  - Footer overlap: not an issue
- **Tests**: None (visual). Manual verify.
- **Verify**: `dotnet build src/MobileShop.slnx --nologo`
- **Done when**: Navbar remains visible at top when scrolling any page; content not hidden.
- **Risk**: LOW | **Confidence**: HIGH

---

## Global Definition of Done
- All 7 actionable demands implemented per acceptance criteria above (Step 7 skipped)
- `dotnet build src/MobileShop.slnx --nologo` → 0 errors, 0 warnings
- `dotnet test src/MobileShop.slnx --nologo` → all tests pass (existing + new)
- No modifications to `MobileShop.Api` endpoints; API stubs use default interface implementation and need no edit
- No migrations, no database initialization policy changes
- Apple ID plaintext passwords unchanged; user passwords remain Argon2-hashed
- Conventional commit per step (after Actor execution)

## Execution Notes (for Actor)
- Work fast: combine independent shell commands (`&&`).
- Keep verification output short — only tail of build/test.
- Do not restate this plan in chat; write report to `act.md` per step.
- Stop after each step; wait for reviewer approval before next.
