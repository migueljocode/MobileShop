# Plan — Fix Sticky Navbar (Task 1 of 8)

## Reviewer Briefing
- **HIGH-risk**: The plan is a refactor of a previously failed change (same symptom: gap above bar, jump on pin). Root cause is `<header>` wrapper + `padding-top: 56px` on body. Both must be fixed.
- **HIGH-confidence**: Root cause confirmed — `<header>` collapses to navbar height, trapping `sticky-top`; `padding-top` creates gap/jump. Fix is surgical.

---

## ~~[x] Step 1 — Move `sticky-top` to `<header>`, delete body `padding-top`, clean up redundant styles~~
- **Files**: 
  - modify: `src/MobileShop.Web/Pages/Shared/_Layout.cshtml`
  - modify: `src/MobileShop.Web/wwwroot/css/site.css`
  - do not touch: any other files
- **Symbols**: `<header class="sticky-top">`, `<nav class="navbar ...">`, `body { margin-bottom: 60px; }`
- **Current → Desired**: 
  - **Current**: Navbar wrapped in `<header>` → `sticky-top` fails (trapped in zero-height container). `body { padding-top: 56px }` creates 56px gap at rest and jump on pin.
  - **Desired**: `sticky-top` moved to `<header>` (direct child of `<body>`) → works relative to viewport. No `padding-top` → bar sits flush at top at rest, pins smoothly on scroll. No gap, no jump. `mb-3` and inline `z-index` removed. `banner` landmark preserved via `<header>`.
- **Change**: 
  1. In `_Layout.cshtml` line 13-58: **Move `sticky-top` from `<nav>` to `<header>`**. Keep `<header>` and `</header>` tags (lines 13 and 58). Add `class="sticky-top mb-0"` to `<header>`. Remove `sticky-top`, `mb-3`, and `style="z-index: 1030;"` from `<nav>`. Do NOT add inline `z-index` to `<header>` (Bootstrap's `.sticky-top` already sets `z-index: 1020`).
  2. In `site.css` line 42-45: **Delete `padding-top: 56px;`** from `body` rule. Keep `margin-bottom: 60px;`. Result: `body { margin-bottom: 60px; }`.
- **Edge cases**: 
  - Mobile collapse: `sticky-top` on `<header>` works with Bootstrap 5 dropdowns inside navbar
  - Footer overlap: not an issue (footer is static)
  - No JavaScript needed — pure CSS fix
  - Accessibility: `<header>` landmark preserved
- **Tests**: Manual browser verification only (build cannot detect CSS positioning defects):
  - Bar stays pinned on scroll
  - No gap above or below bar at rest (scroll position 0)
  - No visible jump when bar pins/unpins
  - Content not obscured by bar at any scroll position
  - Hamburger menu usable <576px
  - Dropdowns still overlay correctly
- **Verify**: `dotnet build src/MobileShop.slnx --nologo` (mechanical gate only)
- **Done when**: All manual checks pass; navbar flush at top at rest, pins smoothly on scroll, no gap/jump, content visible, dropdowns work.
- **Risk**: LOW (after corrections) | **Confidence**: HIGH

---

## Global Definition of Done
- Navbar flush at top at rest; pins smoothly on scroll; no gap, no jump
- Content not obscured by bar at any scroll position
- Dropdowns/hamburger work on mobile
- `dotnet build src/MobileShop.slnx --nologo` → 0 errors, 0 warnings
- No modifications to other files

## Execution Notes (for Actor)
- Two file edits: `_Layout.cshtml` (move sticky-top to header with mb-0, drop mb-3/z-index), `site.css` (delete padding-top from body)
- No JavaScript, no new CSS rules
- Manual browser verification is the real gate — build is necessary but not sufficient
