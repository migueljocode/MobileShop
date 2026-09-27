# Act Brief — Plan Mode Input for Task 1 (Sticky Navbar)

*Written by the reviewer. Purpose: give plan mode everything it needs to write `plan.md` for Task 1, and record what was already verified so it doesn't re-derive it — while leaving the plan decisions to plan mode. This file is a brief, not a plan.*

## 1. Authoritative state

- **`.clinerules/to-do.md` is the new owner-written checklist.** The old `## [ ] Step N` stage format is gone. Task 1 is now `### [ ] Task 1 — Fix sticky navbar (currently malformed, fades on scroll)`, under `## HIGH Priority (Broken Core Features)`.
- **Old Step 8 is superseded** by this Task 1. The old stage (Steps 1–7) is closed.
- `plan.md` is currently **empty**; plan mode owns it. `audit.md` holds the Step 6 PASS verdict — stale but historical.
- **Watch for:** any write to `to-do.md` must preserve the new `### [ ] Task N` format and priority headings, *not* reintroduce `## [ ] Step N`.

## 2. What I already verified (do not re-derive; cite as evidence)

**The regression came from commit `16b1f99` "feat(web): make navbar sticky on scroll"**, which made exactly two changes:
- `_Layout.cshtml` line 14: appended `sticky-top` + `style="z-index: 1030;"` to the `<nav>`.
- `site.css` `body`: added `padding-top: 56px;`.

**Root cause of "disappears on scroll" — structural, not a z-index or padding problem:**

```html
<body>
  <header>                                    <!-- parent collapses to navbar height -->
    <nav class="... sticky-top" ...>          <!-- sticky, but zero travel range -->
```

`position: sticky` moves an element only *within its parent's box*. `<header>` has no other content, so it is exactly as tall as the navbar → the navbar's sticky range is **0px** → it scrolls away. No `z-index` value fixes this. Bootstrap's rule is `.sticky-top{position:sticky;top:0;z-index:1020}`.

**Root cause of "uglier and taller":**
- `padding-top: 56px` on `body` — the navbar is **in normal flow, not fixed**, so it never overlapped content; this padding solved a non-problem and only pushes page content down, creating dead space above the bar. **Remove it, don't tune it.**
- `mb-3` on the `<nav>` — harmless when the navbar scrolled away, but with a sticky bar it leaves a permanent ~16px band of empty page under a bar meant to look attached to the top.

**Currently pointless:** the inline `style="z-index: 1030;"` duplicates what `.sticky-top` already sets (1020).

**Ruled out (so plan mode doesn't chase them):**
- No bare `header{}` selector in Bootstrap — the 7 `…body{overflow-y:auto}` hits are all `.modal-body` / modal-scoped. **Nothing sets `overflow` on the real `body`**, so overflow is not blocking sticky.
- `site.css` contains no `overflow`, no `transform`, no `height` on `header`/`body`/`html` that would clip a sticky child.
- `MobileShop.Web.styles.css` (referenced at `_Layout.cshtml` line 10) has no file on disk — it's the standard scoped-CSS bundle generated at build. Not a factor.

## 3. Styling direction — decided by the owner

**Keep the navbar's current overall style exactly as-is** (white background, `border-bottom`, `box-shadow`, same height, same fonts). The owner will polish the UI later; **this task is UX-only**. So: no restyling, no new colours, no font/height changes. Preserve `navbar navbar-expand-sm navbar-toggleable-sm navbar-light bg-white border-bottom box-shadow` minus whatever is genuinely part of the bug.

## 4. Decisions plan mode should make (recommendation, not a mandate)

1. **Where to put the sticky element.** Two viable shapes:
   - **(Recommended)** Put `sticky-top` on the `<header>` itself, leave `<nav>` unstyled. Parent is a direct child of `<body>` and spans the page → full travel range. Smallest diff, keeps the semantic wrapper and the banner landmark for screen readers.
   - *(Alternative)* Delete the `<header>` wrapper and make `<nav class="... sticky-top">` a direct child of `<body>`. Also correct, slightly larger markup change, loses the `<header>` landmark.
   Plan mode should pick one and justify it.
2. **Fate of `body { padding-top: 56px; }`.** Recommend **delete it** (reason in §2). Consequence: content returns to sitting directly under the bar. If plan mode hesitates, the fallback is a much smaller value *only* if some page proves real overlap — nothing in the current markup suggests one does.
3. **Fate of `mb-3` on the nav.** Recommend **remove**, so the bar has no dead band beneath it. Confirm no page depends on that 16px gap.
4. **Inline `z-index: 1030`.** Recommend **delete**; `.sticky-top` supplies 1020. If a higher layer is genuinely needed (e.g. dropdowns overflowing the bar), prefer a class in `site.css` over an inline style.

## 5. Edge cases the plan must address

- **Mobile (<576px):** `navbar-expand-sm` collapses to the hamburger. A sticky bar must keep the expanded `.navbar-collapse` usable and not clip it. Bootstrap's collapse is `position: static` by default, so the menu pushes content rather than trapping under the bar — confirm, don't assume.
- **Dropdown menus** (`Products`, `People`) open as `.dropdown-menu` with a high z-index; verify they still layer above the sticky bar.
- **Long pages** — bar must not detach or flicker at the very bottom of the document.
- **Print stylesheet** — a sticky bar can repeat on every printed page; low priority, but worth a one-line note.
- **`asp-append-version="true"`** on `site.css` (line 9) cache-busts the CSS automatically — no manual versioning needed.

## 6. Verification reality-check (important)

The previous Step 8 was ticked with **only** `dotnet build`, and its own plan said *"Tests: None (visual). Manual verify."* That is exactly how a broken navbar shipped. Plan mode should:
- Keep `dotnet build src/MobileShop.slnx --nologo` as the mechanical gate (pure markup/CSS, no C# changes, so the test suite adds nothing).
- **Be explicit that build success does not verify stickiness.** The real acceptance criterion is a manual browser scroll test, spelled out exactly: bar stays pinned at top when scrolling down; no dead gap above or below it; content starts flush under the bar; hamburger opens and is usable at mobile width; dropdowns still overlay correctly; footer unaffected.
- Say plainly if automated assertion isn't possible for CSS layout, rather than implying the build proves it.

## 7. Scope guard

**IN SCOPE:** `_Layout.cshtml` (the `<header>`/`<nav>` sticky class, removal of the inline `z-index`, removal of `mb-3`), and `site.css` (removal of `body { padding-top }`).
**OUT OF SCOPE:** any restyling, fonts, colours, navbar height, the closed Steps 1–7 work, Tasks 2–8, and the `MobileShop.Api` project. Do not touch other Razor pages.

## 8. Files to read before writing the plan

- `.clinerules/to-do.md` (authoritative, new format)
- `.clinerules/chat/plan.md` (currently empty — plan mode owns it)
- `src/MobileShop.Web/Pages/Shared/_Layout.cshtml` (lines 12–18 are the hot spot)
- `src/MobileShop.Web/wwwroot/css/site.css` (lines 42–45, the `body` rule)
- `git show 16b1f99` (the exact regression diff)

## Status

Brief COMPLETE — awaiting plan mode to author `plan.md` for Task 1. No code changed by this brief.
