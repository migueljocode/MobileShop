# Plan — UI/UX Enhancements — Step 8 of 7

Copied verbatim from .clinerules/to-do.md. Full stage list: to-do.md → # To-do — UI/UX Enhancements (7 Actionable Demands — Step 7 Already Implemented).

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

## Execution notes
- Work fast: combine independent shell commands (`&&`).
- Keep verification output short — only tail of build/test.
- Do not restate this plan in chat; write report to `act.md` per step.
- Stop after each step; wait for reviewer approval before next.
