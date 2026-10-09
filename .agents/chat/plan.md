# Plan — Full roadmap: Stage 1 (edit-save hardening) + Stages 2-4 (UI/UX overhaul)

> Full-roadmap plan requested by the user (deviates from "current stage only"). Reviewer Job A: APPROVED WITH CORRECTIONS; the HIGH fix and cheap MEDIUM notes are folded in below, and the four stages are now in `.agents/to-do.md`. Execute strictly in order; one step per Actor pass, Job B after each. When a stage signs off, trim its steps to a struck heading + one-line result.

## Assumptions (labelled, not facts — Reviewer/user may veto)
- A1: UI language and direction stay English / LTR (today: `<html lang="en">`, no `dir`, English labels). Persian appears only in data (names, notes) → handled with `dir="auto"` on data cells and a Persian-capable font fallback.
- A2: The dark theme stays; no light theme in scope.
- A3: Bootstrap 5 stays; no new npm/CDN dependency. `wwwroot/fonts/vazirmatn-{regular,medium,bold}.woff2` already exist but are referenced nowhere.
- A4: There is no visual-regression tooling. CI proves compile + tests only; visuals are checked by grep rules plus the user's manual check at 360 / 768 / 1280 px.

## Reviewer Briefing
- Stage 1: LOW risk, HIGH confidence; Services + one test only.
- Stages 2-4 touch only `src/MobileShop.Web` (css, js, cshtml). No C# logic, no schema, no Api, no auth.
- Highest risk: Steps 3.3-3.5 (large forms wired to `create-product-pricing.js`, `product-create-modal.js`, `jalali-datepicker.js`) and Step 4.3 (JS behavior). Both are "preserve every id/name/asp-*/data-* hook" steps; Job B must diff for removed hooks.
- Stage 2 changes the look of every page immediately (global CSS). The intermediate state must stay usable; that is why Stage 2 keeps the old `--app-*` variable names as aliases.
- Colour contrast below was computed (WCAG), not eyeballed.

## Hard constraints for ALL stages (unless a step says otherwise)
- Follow `AGENTS.md` + `.agents/project.md`. Apple ID inventory passwords stay plaintext and displayed exactly as today; no auth/cookie/claims boilerplate; never touch `src/MobileShop.Api`, DB init, migrations, bin/obj.
- Preserve every `id`, `name`, `asp-for`, `asp-page`, `asp-route-*`, `asp-items`, `data-*`, `onchange` and form-field name, and every element the JS files query. Never rename a handler, route or bind property.
- No new JS/CSS frameworks or CDN links. No unrelated refactors (mark OUT OF SCOPE). Follow the existing Razor style; use the newest C# syntax available (target-typed `new`, collection expressions, tuple deconstruction) in any Razor `@{ }` code you add; usings only via `GlobalUsings.cs`.
- Visuals are not CI-testable: every UI step's Verify = CI green + the listed grep checks + "Manual check (user)" line copied into act.md.

## Design direction (applies to Stages 2-4)
Subject: back-office for one phone shop; used all day at a counter, mostly desktop, sometimes a phone. Primary job: find an item or IMEI, sell it, record the money. Concept: **counter display** — flat shelf panels separated by hairlines; numbers (prices, IMEI, barcodes) are the stars. Accent colour is spent in four places only: primary action, "available" status, focus ring, active nav item. Deliberate cuts from today's look: body radial gradients + grid overlay, glow shadows, hover lift on cards, one-size 1.15-1.5rem radius, rainbow `text-bg-*` dashboard cards.

Tokens (define once in `tokens.css`; hex values are final):
```
--ground #0E1318   --panel #151C23   --panel-raised #1C252E
--line #2B3743 (decorative)   --line-strong #66788A (control borders)
--ink #ECF0F4   --ink-muted #93A3B3
--accent #36C5B0   --accent-ink #06201C
--warn #E5A94B   --warn-ink #2B1D05   --danger #F0707A   --danger-ink #2A0A0D
--r-control .5rem   --r-panel .75rem   --r-overlay 1rem
--fs-sm .875rem  --fs-body 1rem  --fs-lg 1.125rem  --fs-h2 1.375rem  --fs-h1 1.75rem
--focus-ring 0 0 0 2px var(--ground), 0 0 0 4px var(--accent)
--font-body 'SF Pro Rounded','Vazirmatn',-apple-system,BlinkMacSystemFont,'Segoe UI',system-ui,sans-serif
```
Computed contrast: ink 13.6-16.3:1 and ink-muted 6.0-7.2:1 on ground/panel/raised; accent 7.2-8.7; warn 7.5-9.0; danger 5.4-6.5; accent-ink on accent 7.9; warn-ink on warn 7.9; danger-ink on danger 6.4; line-strong 3.4-4.1 (meets 3:1 for control borders). Any new colour pair must be added to this list with its ratio.
Type: one family system (SF Pro Rounded for Latin, Vazirmatn picks up Persian glyphs by fallback). Weights 400/500/700; body line-height 1.5; numbers use `font-variant-numeric: tabular-nums` (class `.num`), not monospace. Copy: sentence case, buttons say what they do ("Save changes", "Create phone"), empty states say what to do next, errors say what failed and how to fix it.

---
# STAGE 1 — Product edit save hardening
(Files: `ProductsDataService.cs`, `ProductsDataServiceTests.cs` only.)

## [ ] Step 1.1 — Share one edit include list
- Files: inspect + modify: `src/MobileShop.Services/DataServices/Dal/ProductsDataService.cs`; do not touch: `IBaseRepo`, `BaseRepo`, `GlobalUsings.cs` (`System.Linq.Expressions` already global), Web, Api, tests.
- Symbols: `ProductsDataService.UpdateProductAsync`, `ProductsDataService.GetProductForEditAsync`, new `private static readonly Expression<Func<Product, object>>[] EditIncludes`.
- Current -> Desired: the same 21 include lambdas (`ModelNavigation` … `GlassProfile.ModelFits`) are written twice, each in its own `#pragma warning disable CS8603, CS8602` pair; `GetProductForEditAsync` lost its indentation (`public` at column 0). Desired: the list exists once.
- Change: (1) add `EditIncludes` after the existing `private const string` fields; copy the list verbatim, same order, from the current `UpdateProductAsync` call; wrap the declaration in `#pragma warning disable CS8603, CS8602 // Include lambda returns nullable nav; benign` / `#pragma warning restore CS8603, CS8602`. (2) `UpdateProductAsync`: `var product = await products.FindTrackedWithIncludesAsync(input.ProductId, EditIncludes);`, delete its inline list and pragma pair, keep a one-line comment (tracked, same includes as `GetProductForEditAsync`). (3) `public Task<Product?> GetProductForEditAsync(int id) => products.FindWithIncludesAsync(id, EditIncludes);` at 4-space indent; delete its body and pragma pair (drop `async`; return type unchanged so `IProductsDataService` still matches). (4) Change nothing else.
- Depends on: none.
- Edge cases: the include set must stay identical — a missing include silently nulls a profile and breaks the type switch ("Product is not a phone."). Compare old/new lists line by line.
- Tests: none added; existing `UpdateProductAsync_edits_phone_identifier_imei_and_stores_finished_price` and edit-load tests cover both paths.
- Verify: CI green, no new warnings, cite run number. `git diff --stat` = only `ProductsDataService.cs`. `grep -n "EditIncludes" <file>` = 1 declaration + 2 uses; the list appears once.
- Done when: both methods use `EditIncludes`, no duplicate list, correct indentation, CI green.
- Risk: LOW
- Confidence: HIGH

## [ ] Step 1.2 — Unchanged save succeeds
- Files: inspect: end of `UpdateProductAsync`, `src/MobileShop.Tests/Dal/BaseClass/TestDataHelpers.cs` (`CreateProduct`), the existing update test; modify: `ProductsDataService.cs`, `src/MobileShop.Tests/Services/DataServices/Dal/ProductsDataServiceTests.cs`; do not touch: Web `Edit.cshtml.cs`, repos, entities.
- Symbols: `ProductsDataService.UpdateProductAsync` (last lines), new test `UpdateProductAsync_succeeds_when_nothing_changed`.
- Current -> Desired: the method ends with `var updated = await products.SaveChangesAsync() > 0; if (!updated) return new ServiceResult(false, "The product could not be updated. Check the details and try again.", null, null);`. `SaveChangesAsync` returns 0 when nothing changed, so re-saving an unchanged edit shows that error. Desired: any save that does not throw is success.
- Change: (1) replace those three lines with `await products.SaveChangesAsync();` and keep the existing success `return new ServiceResult(true, "Product updated successfully.", null, input.ProductId);`. (2) append the test at the end of `ProductsDataServiceTests` (after the existing update test), seeded like the existing test (`SeedCatalog`, `TestDataHelpers.CreateProduct`, a `Phone` row with `IMEI1 = "345678901234567"`). Input built from stored values: `Identifier` = stored `Barcode`, `IMEI1` = `"345678901234567"`, `Price` = stored `Price`, `ProfitPercent`/`ProfitAmount` null (finished price = paid price), `Type = "Phone"`, `ModelId` null. Assert `result.Succeeded` and that `Barcode`, `Price`, `PhoneProfile.IMEI1` are unchanged when re-read with `AsNoTracking`.
- Depends on: Step 1.1.
- Edge cases: real failures (`DbUpdateException`, concurrency) propagate exactly as before; no new try/catch.
- Tests: the new test; the existing edit test passes unchanged.
- Verify: CI green (cite run). `grep -rn "could not be updated" src` → no match. `git diff --stat` = the two files only.
- Done when: unchanged save returns success, new test passes, CI green.
- Risk: LOW
- Confidence: HIGH

## [ ] Step 1.3 — Stage 1 validation
- Files: none modified.
- Change: confirm the last commit's CI run is green on the full suite; record `Action: #<run_number> — Success` in act.md.
- Done when: evidence recorded. Risk: LOW. Confidence: HIGH.

---
# STAGE 2 — UI/UX design system foundation
(Files under `src/MobileShop.Web/wwwroot/css`, `_Layout.cshtml` link tags, `navigation-menu.css`. No page markup yet.)

## ~~[x] Step 2.1 — Tokens, fonts, alias layer~~
- Files: create: `wwwroot/css/tokens.css`; modify: `Pages/Shared/_Layout.cshtml` (one `<link>`), `wwwroot/css/app-theme.css` (`:root` block only), `wwwroot/css/site.css` (font stack line only); inspect: `navigation-menu.css`; do not touch: JS, other cshtml.
- Symbols: CSS custom properties from "Design direction"; `@font-face` for Vazirmatn.
- Current -> Desired: no token file; Vazirmatn unused; `--bs-body-font-family` is SF Pro Rounded only. Desired: `tokens.css` holds every token above plus `@font-face` blocks for Vazirmatn 400 / 500 / 700 (`font-display: swap`, urls `../fonts/vazirmatn-regular.woff2`, `-medium`, `-bold`); `site.css` sets `--bs-body-font-family: var(--font-body)`.
- Change: add `<link rel="stylesheet" href="~/css/tokens.css" asp-append-version="true" />` immediately after the bootstrap link and before `site.css`. In `app-theme.css` `:root`, re-point the old names so nothing else breaks: `--app-ink: var(--ink)`, `--app-muted: var(--ink-muted)`, `--app-accent: var(--accent)`, `--app-accent-strong: var(--accent)`, `--app-surface: var(--panel)`, `--app-surface-raised: var(--panel-raised)`, `--app-edge: var(--line)`, `--app-edge-strong: var(--line-strong)`, `--bs-body-bg: var(--ground)`, `--bs-body-color: var(--ink)`, `--bs-primary: var(--accent)`, `--bs-primary-rgb: 54, 197, 176`, `--bs-link-color: var(--accent)`, `--bs-link-hover-color: var(--ink)`, `--bs-border-color: var(--line)`, `--bs-tertiary-color: var(--ink-muted)`. Keep `color-scheme: dark`.
- Edge cases: `grep -rn "var(--app-" wwwroot` first and confirm every referenced name is still defined.
- Tests: none.
- Verify: CI green. `grep -c "@font-face" wwwroot/css/tokens.css` = 3; every token name above appears in `tokens.css`; `tokens.css` link precedes `site.css` in `_Layout.cshtml`.
- Done when: app renders with the new palette through aliases, fonts declared, CI green.
- Risk: LOW
- Confidence: MEDIUM (visual result not CI-verified)

## ~~[x] Step 2.2 — Page surface and typography~~
- Files: modify: `wwwroot/css/app-theme.css` (selectors: `body`, `body::before`, `.container`, `.container > main[role="main"]`, `h1`-`h6`, `.lead`, `a`, `.card`, `.card:hover`, `.card-title`, `.footer`); do not touch: other selectors.
- Current -> Desired: body radial gradients + fixed grid overlay, glowing 1.5rem-radius main panel, cards with hover lift and glow. Desired: `body` = flat `var(--ground)`, no gradient, delete the `body::before` grid; main panel = `var(--panel)`, `1px solid var(--line)`, `var(--r-panel)`, no box-shadow, keep the existing `clamp()` padding; `.card` = `var(--panel)`, `1px solid var(--line)`, `var(--r-panel)`, no shadow, **delete** hover transform/shadow/glow rules; headings use the type scale (`h1` `--fs-h1` 700, `h2` `--fs-h2` 700, letter-spacing `-0.01em`), no gradient text; links `var(--accent)` with underline offset kept, hover `var(--ink)`; footer = top hairline, `var(--ink-muted)`.
- Edge cases: delete rules, don't comment them out; keep the `prefers-reduced-motion` block.
- Verify: CI green. `grep -n "radial-gradient\|body::before\|translateY" wwwroot/css/app-theme.css` shows none inside the selectors listed (picker/nav keep theirs until 2.5).
- Done when: pages show flat panels, no glow/gradient, CI green.
- Risk: LOW
- Confidence: MEDIUM

## ~~[x] Step 2.3 — Component classes (new file, unused until Stage 3)~~
- Files: create: `wwwroot/css/components.css`; modify: `_Layout.cshtml` (one `<link>` after `app-theme.css`).
- Symbols & spec (all colours from tokens only; all transitions ≤ 150 ms and wrapped so `prefers-reduced-motion` disables them):
  - `.page-head`: flex, wrap, `justify-content: space-between`, `align-items: end`, gap `1rem`, margin-bottom `1.25rem`, bottom hairline `--line` + padding-bottom `.75rem`. `.page-head__title` (h1 reset margin), `.page-head__sub` (`--ink-muted`, `--fs-sm`), `.page-head__actions` (flex, wrap, gap `.5rem`).
  - `.toolbar`: flex wrap, gap `.75rem`, align end, margin-bottom `1rem`; its `.form-label` is `--fs-sm`, `--ink-muted`.
  - `.data-table`: width 100%, `border-collapse: collapse`; `th` = `--fs-sm`, weight 500, `--ink-muted`, left-aligned, bottom border `--line-strong`; `td` = padding `.65rem .75rem`, bottom border `--line`, vertical-align middle; row hover `--panel-raised`; `.num` right-aligned cells.
  - `.num`: `font-variant-numeric: tabular-nums;` (and `text-align: end` when on `td`/`th`).
  - `.stat-grid`: CSS grid, `repeat(auto-fit, minmax(11rem, 1fr))`, gap `.75rem`; `.stat`: `--panel-raised`, `--line` border, `--r-panel`, padding `1rem`; `.stat__value` `--fs-h2` 700 `.num`; `.stat__label` `--ink-muted` `--fs-sm`.
  - `.form-section`: `--panel-raised` background, `--line` border, `--r-panel`, padding `1rem 1.25rem`, margin-bottom `1rem`; `.form-section__title` `--fs-lg` 700, margin-bottom `.75rem`.
  - `.action-bar`: flex, gap `.5rem`, `justify-content: flex-end`, border-top `--line`, padding-top `1rem`, margin-top `1.25rem`.
  - `.status-chip` (+ `--available`, `--sold`, `--used`): inline-flex, padding `.15rem .6rem`, `--r-control`, `--fs-sm`, weight 500; background = `color-mix(in srgb, <colour> 16%, transparent)`, text = the colour: available → `--accent`, sold → `--ink-muted`, used → `--warn`.
  - `.empty-state`: centered column, padding `2.5rem 1rem`, dashed `--line-strong` border, `--r-panel`; `.empty-state__title` `--fs-lg`; `.empty-state__hint` `--ink-muted`.
  - `.skip-link`: positioned off-screen; on `:focus` fixed top-left `0.75rem`, `--accent` background, `--accent-ink` text, `--r-control`, z-index above the nav dock.
  - `.loading-line`: `--ink-muted`, `--fs-sm`, padding `.5rem .75rem`.
- Tests: none.
- Verify: CI green. `grep -c "^\.\(page-head\|toolbar\|data-table\|num\|stat-grid\|stat\|form-section\|action-bar\|status-chip\|empty-state\|skip-link\|loading-line\)" wwwroot/css/components.css` ≥ 12. No hex colour literals in the file (`grep -n "#[0-9A-Fa-f]\{3,6\}\b" wwwroot/css/components.css` → none).
- Done when: file exists, linked, no visual change to existing pages, CI green.
- Risk: LOW
- Confidence: HIGH

## ~~[x] Step 2.4 — Bootstrap component overrides~~
- Files: modify: `wwwroot/css/app-theme.css` (selectors present today: `.btn`, `.btn-primary`, `.btn-success`, `.btn-secondary`, `.btn-danger`, `.btn-warning`, `.btn-info`, `.btn-dark`, all `.btn-outline-*`, `.form-control`, `.form-select`, `.form-label`, `.form-check-input`, `.input-group-text`, `.form-text`, `.table`, `.badge`, `.alert*`, `.list-group-item`, `.nav-tabs`, `.progress`, `.field-validation-error`, `.validation-summary-errors`, `.text-danger`).
- Desired: `.btn` radius `--r-control`, weight 500, no shadow; `.btn-primary` and `.btn-success` = `--accent` bg + `--accent-ink` text, hover `color-mix(in srgb, var(--accent) 88%, white)`; `.btn-outline-primary` = transparent, `--line-strong` border, `--ink` text, hover border `--accent`; `.btn-danger` = `--danger` + `--danger-ink`; `.btn-warning` = `--warn` + `--warn-ink`; other `.btn-outline-*` map to `--line-strong` border and `--ink` text; `.btn-secondary/-info/-dark` = `--panel-raised` bg, `--line-strong` border, `--ink`. Inputs: `.form-control`, `.form-select`: bg `--ground`, 1px `--line-strong`, `--r-control`, text `--ink`, placeholder `--ink-muted`; `:focus` → border `--accent` + `--focus-ring` (replace the white/blue `box-shadow` in `site.css` `.btn:focus ...` rule too — that file edit is allowed here, one rule). `.form-label` `--fs-sm` `--ink-muted`. `.table`: `--bs-table-bg: transparent`, `--bs-table-striped-bg: transparent`, `--bs-table-hover-bg: var(--panel-raised)`, borders `--line`. `.badge` and `.text-bg-success/-secondary/-warning/-info/-primary/-danger`: tinted style like `.status-chip` (accent / ink-muted / warn / accent / accent / danger). `.alert-*`: `--panel-raised` background, 1px border in the semantic colour, text `--ink`. `.field-validation-error`, `.text-danger`: `--danger`.
- Edge cases: keep every selector's specificity as is; edit declarations, don't reorder rules.
- Verify: CI green. `grep -n "#7388ff\|#aebdff\|#258cfb\|rgba(115, 136, 255" wwwroot/css/app-theme.css wwwroot/css/site.css` shows only lines inside the Jalali picker / `.account-login-*` / `.profit-chart-*` / nav selectors (handled in 2.5 and 3.8).
- Done when: buttons, inputs, tables, badges, alerts use tokens, CI green.
- Risk: LOW
- Confidence: MEDIUM

## ~~[x] Step 2.5 — Overlays, date picker, navigation dock~~
- Files: modify: `wwwroot/css/app-theme.css` (`.modal*`, `.dropdown-menu`, `.dropdown-item`, `.popover`, `.jalali-picker-*`, `.password-visibility-toggle`), `wwwroot/css/navigation-menu.css`; do not touch: any JS, dock geometry (width/height/position/z-index/transform values).
- Desired: replace hard-coded colours/glows with tokens only. `.modal-content`, `.popover`, `.dropdown-menu`, `.jalali-picker-panel` = `--panel-raised`, `1px --line-strong`, `--r-overlay`, a single shadow `0 1rem 2rem rgb(0 0 0 / .45)`; backdrop `rgb(0 0 0 / .6)`; picker day hover = `--panel`; selected day = `--accent` / `--accent-ink`; nav dock surface `--panel-raised`, border `--line-strong`; active nav item uses `--accent` (indicator + icon), inactive `--ink-muted`; remove radial-gradient/glow layers.
- Verify: CI green. `grep -n "rgba(115, 136, 255\|#7388ff\|#aebdff\|radial-gradient" wwwroot/css/navigation-menu.css wwwroot/css/app-theme.css` → none outside `.account-login-*` and `.profit-chart-*`. `git diff` shows no change to lines containing `position`, `width`, `height`, `z-index`, `transform`, `inset` in `navigation-menu.css`.
- Done when: overlays and dock match tokens, behavior untouched, CI green.
- Risk: LOW
- Confidence: MEDIUM

## ~~[x] Step 2.6 — Stage 2 validation~~
- Files: none modified. Record the CI run. Manual check (user): open Dashboard, Products, one Create page, Transactions/Sell, People, Profit/Loss at 1280 px — all readable, nothing broken, nav dock opens.
- Risk: LOW. Confidence: HIGH.

---
# STAGE 3 — UI/UX page redesign
(Razor markup + small JS colour hooks. Each step: inspect the real page first; apply Stage 2 classes; change no behavior.)

## [ ] Step 3.1 — Layout shell and dashboard
- Files: modify: `Pages/Shared/_Layout.cshtml`, `Pages/Index.cshtml`; do not touch: `Index.cshtml.cs`, nav component.
- Change: `_Layout`: title → `@ViewData["Title"] - MobileShop`; add `<a class="skip-link" href="#main">Skip to content</a>` as first child of `<body>`; `<main id="main" role="main" ...>`; footer reduced to one muted line (`© 2026 MobileShop` + the existing Privacy link). Keep scripts and their order. `Index.cshtml`: replace the centered `display-4` hero and the `text-bg-*` coloured cards with `<div class="page-head"><div><h1 class="page-head__title">Dashboard</h1><p class="page-head__sub">Inventory and sales</p></div></div>` + `<div class="stat-grid">` of `<div class="stat"><div class="stat__value num">@Model.Stock.X</div><div class="stat__label">label</div></div>`. Carry over every existing card with the same binding, label and order.
- Edge cases: before editing run `grep -c "@Model\." Pages/Index.cshtml` and `grep -c "ToIrr\|ToGroupedDigits" Pages/Index.cshtml` and record both in act.md; after = same counts (money formatting is never re-done in markup).
- Verify: CI green; binding count equal; no `text-bg-` left in `Index.cshtml`; `id="main"` exists exactly once.
- Done when: dashboard uses `.stat` grid, skip link present, CI green.
- Risk: LOW. Confidence: HIGH.

## [ ] Step 3.2 — Products list
- Files: modify: `Pages/Products/Index.cshtml` (rewrite markup readable, multi-line); create: `Pages/Shared/_EmptyState.cshtml` (`@model (string Title, string? Hint)`: `<div class="empty-state" role="status"><p class="empty-state__title">@Model.Title</p>@if (Model.Hint is not null){<p class="empty-state__hint">@Model.Hint</p>}</div>`); do not touch: `Index.cshtml.cs`.
- Change: in `@{ }` declare `var tabs = new (string Key, string Label, string? CreatePage, string? CreateLabel)[] { ("all","All",null,null), ("phone","Phones","/Products/CreatePhone","Create phone"), ("appleid","Apple IDs","/Products/CreateAppleId","Create Apple ID"), ("glass","Glasses","/Products/CreateGlass","Create glass"), ("tablet","Tablets","/Products/CreateTablet","Create tablet"), ("smartwatch","Smart Watches","/Products/CreateSmartWatch","Create smart watch"), ("laptop","Laptops","/Products/CreateLaptop","Create laptop"), ("cable","Cables","/Products/CreateCable","Create cable"), ("charger","Chargers","/Products/CreateCharger","Create charger"), ("powerbank","Power Banks","/Products/CreatePowerBank","Create power bank"), ("portablestorage","Portable Storages","/Products/CreatePortableStorage","Create portable storage"), ("case","Cases","/Products/CreateCase","Create case") };` and render the 12 tab links with one `@foreach (var (key, label, _, _) in tabs)`, keeping each link's `asp-route-type`, `availability`, `manufacturerId`, `modelId`, `sortBy`, `sortDirection` exactly, and `asp-route-partNumberId` **only on the phone tab** as today; active tab = `btn-primary` + `aria-current="page"`, others `btn-outline-primary`. The "Create …" button for the current type comes from the same array (`asp-page="@create"` class `btn btn-success`); Second-hand link stays `btn btn-outline-warning`. Layout: `.page-head` (h1 "Products" + actions = Second-hand link + create button), tab row, `.toolbar` containing the existing filter `<form>` (all selects keep `onchange="this.form.submit()"` — inline handlers are OUT OF SCOPE), total line, then `.data-table` (same 7 columns). Status cell = `<span class="status-chip status-chip--@(p.IsSold ? "sold" : "available")">`, plus `status-chip--used` "Second-hand" when `p.IsSecondHand`. Name and Identifier cells get `dir="auto"`; Identifier also `.num`. Empty → `<partial name="_EmptyState" model='("No products match these filters", "Change a filter or create a product.")' />`.
- Edge cases: sort/filter links must produce the same URLs as before — compare `asp-route-*` sets per tab.
- Tests: run the existing suite. Web tests are PageModel-only (no Razor markup assertions), so none should change; if one fails, stop and report instead of editing it.
- Verify: CI green; `grep -c "asp-route-type" Pages/Products/Index.cshtml` ≤ previous count (loop collapses 12→1); `grep -c "onchange=" Pages/Products/Index.cshtml` equal before/after (record both in act.md); `partNumberId` appears only inside the phone-only branch.
- Done when: list readable, tabs looped, empty state partial used, CI green.
- Risk: MEDIUM. Confidence: HIGH.

## [ ] Step 3.3 — Product details and edit
- Files: modify: `Pages/Products/Details.cshtml`, `Pages/Products/Edit.cshtml`, `Pages/Shared/_ProductEditForm.cshtml`; do not touch: `Edit.cshtml.cs`, `Details.cshtml.cs`, `jalali-datepicker.js`, `create-product-pricing.js`.
- Change: Details: `.page-head` (title = product name, sub = type, actions = Edit / Back), one `.form-section` per existing block rendered as a two-column definition list (`<dl class="row">` with `dt.col-sm-4 text-muted`, `dd.col-sm-8`), prices `.num`, availability as `.status-chip`; Apple ID password field displayed exactly as today. Edit + `_ProductEditForm`: wrap existing field groups in `.form-section` with `.form-section__title` (Identity, Pricing, then the profile-specific block per type, Second-hand, Guarantee), group order unchanged; validation summary at top; buttons in a bottom `.action-bar`: primary "Save changes", secondary "Cancel" (link to Details).
- Edge cases: do not reorder fields inside a group; do not alter `asp-for`, `id`, `name`, `data-*`; keep any `hidden` inputs; keep Jalali date inputs' classes/attributes the picker queries.
- Verify: CI green; record `grep -c "asp-for" ` before/after for the three files — equal; `grep -c "data-" ` equal; `grep -c "ToIrr\|ToGroupedDigits" Pages/Products/Details.cshtml` equal.
- Done when: both pages use sections + action bar, hook counts equal, CI green.
- Risk: MEDIUM. Confidence: MEDIUM.

## [ ] Step 3.4 — Create forms, batch 1 (Apple ID, Glass, Laptop, Phone, Smart watch, Tablet)
- Files: modify: `Pages/Shared/_ProductCreate{AppleId,Glass,Laptop,Phone,SmartWatch,Tablet}Form.cshtml` and `Pages/Products/Create{AppleId,Glass,Laptop,Phone,SmartWatch,Tablet}.cshtml`; do not touch: `.cshtml.cs`, JS.
- Change: same pattern as 3.3 — `.page-head` (title unchanged), field groups in `.form-section` (Product, Pricing, Details, Second-hand/Guarantee where they exist), bottom `.action-bar` with "Create <thing>" (same text as today's button) + "Cancel" link. Pure wrapper/class changes.
- Edge cases: forms posted by `product-create-modal.js` or priced by `create-product-pricing.js` keep their wrapper ids/classes.
- Verify: CI green; per-file `asp-for` and `data-` and `id="` counts equal before/after (table in act.md).
- Done when: six forms restyled, counts equal, CI green.
- Risk: MEDIUM. Confidence: MEDIUM.

## [ ] Step 3.5 — Create forms, batch 2 (Cable, Case, Charger, Portable storage, Power bank) + success partial
- Files: modify: `Pages/Shared/_ProductCreate{Cable,Case,Charger,PortableStorage,PowerBank}Form.cshtml`, `Pages/Products/Create{Cable,Case,Charger,PortableStorage,PowerBank}.cshtml`; inspect only: `Pages/Shared/_ProductCreateSuccess.cshtml` (5-line `alert alert-success` partial with `role="status"`, no links) — do **not** modify it; it picks up Stage 2's `.alert-success` styling.
- Change/Edge/Verify/Done: same as Step 3.4. - Depends on: 3.4 (same patterns).
- Risk: MEDIUM. Confidence: MEDIUM.

## [ ] Step 3.6 — Transactions
- Files: modify: `Pages/Transactions/Index.cshtml`, `Details.cshtml`, `Sell.cshtml`, `Pages/Shared/_ProductPickerOptions.cshtml`, `_ProductSellerPicker.cshtml`; do not touch: `product-picker.js`, `.cshtml.cs`.
- Change: `.page-head`; list table → `.data-table` (amounts `.num`, right-aligned); details as definition-list sections (Parties, Items, Totals); Sell form in `.form-section`s with a bottom `.action-bar` ("Record sale" — keep today's text); picker partials: result rows use `list-group-item` styling already tokenised in 2.4, add no new selectors.
- Edge cases: money stays displayed via existing `ToIrr()`/`ToGroupedDigits()` calls — never reformat in markup.
- Verify: CI green; hook counts equal (`asp-for`, `data-`, `id="`); `grep -c "ToIrr\|ToGroupedDigits"` equal before/after.
- Done when: pages restyled, counts equal, CI green.
- Risk: MEDIUM. Confidence: MEDIUM.

## [ ] Step 3.7 — People
- Files: modify: `Pages/People/{Index,Customers,Sellers,CustomerDetails,SellerDetails,CreateCustomer,CreateSeller}.cshtml`, `Pages/Products/PersonPicker.cshtml`, `Pages/Shared/_PersonPicker.cshtml`; do not touch: `person-picker.js`, `.cshtml.cs`.
- Change: same patterns (`.page-head`, `.data-table`, `.form-section`, `.action-bar`, definition-list details). Names/phones `dir="auto"`; phone numbers also `.num`.
- Verify: CI green; hook counts equal.
- Risk: MEDIUM. Confidence: MEDIUM.

## [ ] Step 3.8 — Reports, account, remaining pages
- Files: modify: `Pages/Reports/ProfitLoss.cshtml`, `wwwroot/js/profit-loss-chart.js`, `Pages/Account/{Login,Logout,Profile}.cshtml`, `Pages/Error.cshtml`, `Pages/Privacy.cshtml`, `Pages/Products/SecondHand.cshtml`; modify CSS: `app-theme.css` selectors `.account-login-*`, `.profit-chart-*` (tokens only).
- Change: ProfitLoss: `.page-head`, summary numbers as `.stat-grid`, both tables `.data-table`. `profit-loss-chart.js`: replace hard-coded series colours with values read once via `getComputedStyle(document.documentElement).getPropertyValue('--accent' | '--danger' | '--ink' | '--ink-muted')` (income = accent, expense = danger, net = ink, grid/axis = ink-muted); no change to data, scales or options. Login: keep form, restyle through tokens, no new auth UI. SecondHand: same list pattern as 3.2 incl. `_EmptyState`. Error/Privacy: `.page-head` + readable `max-width: 70ch`.
- Edge cases: chart must still render with `Chart` from `wwwroot/lib/chart.js`; do not touch the lib.
- Verify: CI green; `grep -n "#[0-9A-Fa-f]\{6\}\|rgba(" wwwroot/js/profit-loss-chart.js` → none; `.account-login-*` / `.profit-chart-*` contain no hex literals except inside `var()` fallbacks.
- Risk: MEDIUM. Confidence: MEDIUM.

## [ ] Step 3.9 — Stage 3 validation
- Files: none modified. Record the CI run. Run: `grep -rn "text-bg-\|btn-outline-dark\|display-4" src/MobileShop.Web/Pages src/MobileShop.Web/Views` → list leftovers (allowed only where the page was explicitly out of scope). Manual check (user): every page at 1280 px.
- Risk: LOW. Confidence: HIGH.

---
# STAGE 4 — Accessibility, responsive layout, states

## [ ] Step 4.1 — Focus, keyboard, semantics
- Files: modify: `wwwroot/css/app-theme.css`/`components.css` (global `:focus-visible`), `Pages/Products/Index.cshtml`, all `<table>` pages (8: ProfitLoss ×2, People/{Index,Sellers,Customers}, Products/{Index,Details,SecondHand}, Transactions/Index), `wwwroot/js/navigation-menu.js` **only if** the check below fails.
- Change: global `:where(a, button, input, select, textarea, summary, [tabindex]):focus-visible { outline: none; box-shadow: var(--focus-ring); }`; every `<th>` in those tables gets `scope="col"` (row headers `scope="row"`); the empty actions-column `<th></th>` (Products/Index, People/Index, Sellers, Customers, SecondHand) becomes `<th scope="col"><span class="visually-hidden">Actions</span></th>`; icon-only buttons/links get `aria-label` (find with `grep -rn "<button" Pages | grep -v ">[A-Za-z]"` and inspect). Nav dock: verify in `navigation-menu.js` that Escape closes the flyout and returns focus to its trigger; if missing, add the smallest handler for that, nothing else.
- Verify: CI green; `grep -rn "<th>\|<th></th>" Pages` → none left; `grep -rn "outline: none\|outline: 0" wwwroot/css` only in the `:focus-visible` rule above.
- Done when: every interactive element shows the ring on keyboard focus, tables have scoped headers, CI green.
- Risk: LOW. Confidence: MEDIUM.

## [ ] Step 4.2 — Responsive layout
- Files: modify: `components.css`, `app-theme.css` (main padding), the 8 table pages (wrapper only), `_Layout.cshtml` (none expected), `navigation-menu.css` (one custom property read).
- Change: wrap each `<table class="data-table">` in `<div class="table-responsive">`; `<576px`: main panel padding `1rem`, border-radius `0`, no side border; `.toolbar` children `flex: 1 1 100%`; `.page-head__actions` full width; `.action-bar` becomes `position: sticky; bottom: 0; background: var(--panel); padding-bottom: calc(1rem + env(safe-area-inset-bottom))`; keep content clear of the floating nav: `.has-floating-navigation { padding-bottom: calc(var(--nav-dock-height, 5rem) + 1rem); }` — Actor reads the real dock height from `navigation-menu.css` and sets `--nav-dock-height` to that value in `tokens.css` (do not change the dock itself).
- Edge cases: sticky `.action-bar` must not sit under the dock — give it `bottom: var(--nav-dock-height, 5rem)` on `<768px` when the dock is present.
- Verify: CI green; `grep -rn "<table" Pages | wc -l` equals `grep -rn "table-responsive" Pages | wc -l`. Manual check (user): 360 / 768 / 1280 px on Products, Sell, Edit — no sideways page scroll, actions reachable above the dock.
- Risk: LOW. Confidence: MEDIUM.

## [ ] Step 4.3 — Empty, loading, error and notice states
- Files: modify: the list pages not yet using `_EmptyState` (grep `No .* found|empty` → People ×3, Transactions/Index, ProfitLoss, SecondHand), `wwwroot/js/person-picker.js`, `product-picker.js`, `product-create-modal.js`, pages that render `TempData["SuccessMessage"]` (grep).
- Change: lists → `<partial name="_EmptyState" model='("<what is empty>", "<next action>")' />` with specific copy per page (e.g. "No customers yet", "Create the first customer to start selling."). JS: while a fetch is in flight set `aria-busy="true"` on the results container and show one `<div class="loading-line">Loading…</div>`; on failure replace results with `<div class="alert alert-danger" role="alert">Couldn't load results. Try again.</div>`; clear both on completion. Do not change request URLs, payloads or selection behavior. `_ProductCreateSuccess.cshtml` already has `role="status"` — leave it. Only inline alerts that lack a role gain one (`role="status"` for success, `role="alert"` for error; attribute only).
- Edge cases: first inspect how each script handles fetch errors today (silent / console) and preserve successful-path behavior byte for byte.
- Verify: CI green; `grep -rn "No .* found" Pages` → none left that are plain `<p>`; `grep -n "aria-busy" wwwroot/js/*.js` ≥ 3.
- Risk: MEDIUM. Confidence: MEDIUM.

## [ ] Step 4.4 — Motion and colour audit
- Files: modify: `components.css`, `app-theme.css`, `navigation-menu.css`, any leftovers.
- Change: ensure every `transition`/`animation` outside the existing `@media (prefers-reduced-motion: reduce)` block is disabled by it (extend the block, don't duplicate it); remove leftover hard-coded colours.
- Verify: CI green. `grep -rn "#7388ff\|#aebdff\|#258cfb\|rgba(115, 136, 255\|radial-gradient" src/MobileShop.Web/wwwroot/css src/MobileShop.Web/wwwroot/js src/MobileShop.Web/Pages` → none. Any hex literal outside `tokens.css` is listed in act.md with its contrast ratio against its background, ≥ 4.5:1 for text and ≥ 3:1 for control borders.
- Risk: LOW. Confidence: HIGH.

## [ ] Step 4.5 — Final validation (whole roadmap)
- Files: none modified.
- Change: full CI green on the last commit (record `Action: #N — Success`); fill the checklist in act.md: skip link works; focus ring visible on nav, filters, tables' links, forms, modals; keyboard-only path Dashboard → Products → Details → Edit → Save works; 360 / 768 / 1280 px on every page group; empty/loading/error states seen once each (user's manual check).
- Risk: LOW. Confidence: HIGH.

## Global Definition of Done
- Stage 1: `EditIncludes` is the only copy of the include list; unchanged edit saves succeed with a passing test.
- Stages 2-4: all pages use the tokens and component classes; no hard-coded legacy colours; every id/name/asp-*/data-* hook preserved (counts recorded per step); skip link, focus ring, scoped table headers, responsive wrappers, empty/loading/error states in place.
- No change to Api, auth, migrations, DB init, Apple ID plaintext handling, `IBaseRepo`/`BaseRepo`.
- CI green on the final commit; run numbers recorded in act.md for every step; no unresolved CRITICAL/HIGH finding.
