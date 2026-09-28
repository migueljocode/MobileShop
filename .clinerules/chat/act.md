# Act Report — Task 1 Step 1: Sticky navbar via `<header>` wrapper fix

## Commit
- **Hash**: pending (recorded in follow-up)
- **Message**: fix(web): pin navbar with sticky-top on header wrapper

## Status
COMPLETE

## Changes
- `_Layout.cshtml`: `<header>` → `<header class="sticky-top mb-0">` (sticky element is now the direct child of `<body>`, full viewport travel; `mb-0` kills the leftover margin band). `<nav>` loses `mb-3`, `sticky-top`, and `style="z-index: 1030;"` — Bootstrap's `.sticky-top` supplies `z-index: 1020` automatically.
- `site.css`: deleted `padding-top: 56px;` from `body`; kept `margin-bottom: 60px;`.
- `<header>`/`</header>` wrapper and `banner` landmark preserved as required.

## Verification
- `dotnet build src/MobileShop.slnx --nologo` → Build succeeded, 0 Warning(s), 0 Error(s)
- Manual browser checks (pin on scroll, no gap/jump, content visible, hamburger <576px, dropdowns) cannot be run headless — flagged as limitation below.

## Limitations
- Manual browser verification not performed (headless environment); build is a mechanical gate only, per plan. Owner should eyeball the six checks in plan.md.

## Friction noted
- None

## Problems
- None

## Status
COMPLETE
