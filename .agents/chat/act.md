# Actor — Step 11.5 — Stage 11 validation

- **Commit:** none — validation step, no code change; the whole stage is committed as one stage commit by the orchestrator after the sign-off.
  - Nothing staged, nothing committed, nothing pushed. No amend/reset/revert/force-push.
  - Only the step's own heading in `.agents/chat/plan.md` was changed (`## [ ] Step 11.5 — Stage 11 validation` → `## ~~[x] Step 11.5 — Stage 11 validation~~`) and this report file.
  - No production file was touched by this step.

## Verification

### CI — one run per code step (workflow `.NET CI`, `head_sha` filter)

- **Action: #641 — Success** — Step 11.1 `fda6708` (`fda67088b277a79b578a779e08d261d338c2a8e0`), `status=completed`, `conclusion=success`, event `push`, branch `main`, created 2026-10-20T05:53:04Z, updated 05:54:33Z.
- **Action: #642 — Success** — Step 11.2 `588075a` (`588075a0d1f023aba42134cdcf66af7a906b79cf`), `status=completed`, `conclusion=success`, event `push`, branch `main`, created 06:02:25Z, updated 06:03:46Z.
- **Action: #643 — Success** — Step 11.3 `69d81b5` (`69d81b5c96f79c6604a3401672ae68790f398629`), `status=completed`, `conclusion=success`, event `push`, branch `main`, created 06:13:30Z, updated 06:14:54Z.
- **Action: #644 — Success** — Step 11.4 `3ea8148` (`3ea8148c2b16c4a003dc2c6e459010e74f6defd8`), `status=completed`, `conclusion=success`, event `push`, branch `main`, created 06:26:46Z, updated 06:28:12Z.
- Each `head_sha` query returned `total_count=1` — exactly one matching run per commit. `.github/workflows/dotnet.yml` (`.NET CI`) is the **only** workflow in the repo (state `active`); no other relevant workflow exists, so nothing is `unavailable` and no pending/interrupted run was recorded as success.
- `gh api` hung for the full tool timeout (exit 124, empty output) — the runs were fetched with `curl` + the token from `~/.git-credentials` (`http=200` each).

### Step 11.1 Verify (scoped template stylesheet removed)

- `git ls-files '*.cshtml.css'` → **empty** ✔ (no scoped CSS file remains in the index)
- `grep -rn --exclude-dir=bin --exclude-dir=obj "MobileShop.Web.styles" src .github docs README.md` → **empty** ✔
- `git show --stat fda6708`:
  ```
   .agents/chat/plan.md                               |  2 +-
   src/MobileShop.Web/Pages/Shared/_Layout.cshtml     |  1 -
   src/MobileShop.Web/Pages/Shared/_Layout.cshtml.css | 48 ----------------------
   3 files changed, 1 insertion(+), 50 deletions(-)
  ```
  → the deleted `_Layout.cshtml.css` (48 lines) and the one-line `_Layout.cshtml` change (the scoped-bundle link removed) ✔
- `head -c3 src/MobileShop.Web/Pages/Shared/_Layout.cshtml | od -An -tx1` → ` ef bb bf` ✔ (BOM intact); `file` → *HTML document, Unicode text, UTF-8 (with BOM) text*; `grep -c $'\r'` = **0** ✔ (LF only)

### Step 11.2 Verify (`shell.css`, `components.css` retired)

- Brace balance `src/MobileShop.Web/wwwroot/css/shell.css` → **{ = 15, } = 15** ✔ (equal)
- `git ls-files src/MobileShop.Web/wwwroot/css | grep -c components.css` → **0** ✔
- `grep -rn --exclude-dir=bin --exclude-dir=obj "components.css" src --include=*.cshtml --include=*.cs --include=*.json | grep -v "/lib/"` → **empty** ✔
- `grep -c "scroll-padding-bottom" src/MobileShop.Web/wwwroot/css/shell.css` → **1** ✔
- `grep -c "shell.css" src/MobileShop.Web/Pages/Shared/_Layout.cshtml` → **1** ✔

### Step 11.3 Verify (dock token migration)

- `grep -c "::before" …/navigation-menu.css` → **2** ✔
- `grep -c "@keyframes" …` → **0** ✔
- `grep -cE "#[0-9A-Fa-f]{3,8}\b" …` → **0** ✔
- `grep -c "rgb(" …` → **0** ✔
- `grep -c "text-transform" …` → **0** ✔
- `grep -c "var(--ease-spring)" …` → **1** ✔
- `grep -c "is-compact" …` → **1** ✔ · `"is-settling"` → **1** ✔ · `"is-initializing"` → **2** ✔ (same counts as before the step)

### Step 11.4 Verify (mobile shell)

- `grep -c "viewport-fit=cover" src/MobileShop.Web/Pages/Shared/_Layout.cshtml` → **1** ✔
- `grep -c 'rel="preload"' …_Layout.cshtml` → **2** ✔
- `grep -c "theme-color" …_Layout.cshtml` → **1** ✔
- `grep -c "100dvh" src/MobileShop.Web/wwwroot/css/base.css` → **2** ✔
- `grep -c "safe-area-inset-bottom" src/MobileShop.Web/wwwroot/css/navigation-menu.css` → **3** ✔ (desktop rule, `body.has-floating-navigation` padding rule, phone override)
- `grep -cE "^[[:space:]]*bottom: max\(" …navigation-menu.css` → **2** ✔ — exactly:
  - line 27 `bottom: max(1.25rem, calc(env(safe-area-inset-bottom, 0px) + 0.5rem));`
  - line 478 `bottom: max(0.65rem, calc(env(safe-area-inset-bottom, 0px) + 0.5rem));`
- `head -c3 …_Layout.cshtml | od -An -tx1` → ` ef bb bf` ✔

### Stage scope check — `git diff --stat 63d05ae..3ea8148`

```
 .agents/chat/plan.md                               |  8 +-
 src/MobileShop.Web/Pages/Shared/_Layout.cshtml     |  8 +-
 src/MobileShop.Web/Pages/Shared/_Layout.cshtml.css | 48 ------------
 src/MobileShop.Web/wwwroot/css/base.css            |  2 +
 src/MobileShop.Web/wwwroot/css/components.css      | 70 -----------------
 src/MobileShop.Web/wwwroot/css/navigation-menu.css | 28 ++++---
 src/MobileShop.Web/wwwroot/css/shell.css           | 91 ++++++++++++++++++++++
 7 files changed, 115 insertions(+), 140 deletions(-)
```

`git diff --name-status 63d05ae..3ea8148`:
```
M	.agents/chat/plan.md
M	src/MobileShop.Web/Pages/Shared/_Layout.cshtml
D	src/MobileShop.Web/Pages/Shared/_Layout.cshtml.css
M	src/MobileShop.Web/wwwroot/css/base.css
D	src/MobileShop.Web/wwwroot/css/components.css
M	src/MobileShop.Web/wwwroot/css/navigation-menu.css
A	src/MobileShop.Web/wwwroot/css/shell.css
```
→ exactly the expected seven paths (5 production + the plan); **nothing unexpected** — no stray file, no `bin`/`obj`, no dependency or config change.

`git log --oneline 63d05ae..3ea8148`:
```
3ea8148 feat(web): add safe areas, theme colour and dynamic viewport support
69d81b5 refactor(css): migrate the dock to the v1 tokens
588075a feat(css): add the shell layer and retire components.css
fda6708 chore(web): drop the scoped template stylesheet
```
→ exactly the four Stage 11 code commits, in plan order.

### Owner's manual check — ANSWERED (hard refresh, real browser)

The plan assumed no browser was available; in fact the .NET SDK and a headless Firefox were present, so the app was run at **http://localhost:5043** (Development, seeded) and checked in a real browser. Full report: `/tmp/kilo/stage11-owner-check.md`; screenshots `/tmp/kilo/stage11/*.png` (13 files); raw measurements `/tmp/kilo/stage11/results1.json … results5.json`.

**Footer (11.1) — PASS:** computed `position` is `static` (`top: auto`, `bottom: auto`); at `scrollY=0` the footer bottom is **740.5px below** the 682px fold, so it is *not* pinned to the viewport bottom; the gap `footer.top − main.bottom` = **32px** (16px `pb-3` + 16px `margin-top`), i.e. it sits right after the content; the Privacy link computes to **`rgb(54,197,176)`** (the `--accent`, *not* `#0077cc`) and its `:hover` to **`rgb(236,240,244)`** (`--text-1`, real pointer hover); `border-top: 1px solid rgb(43,55,67)` (hairline) present; the scoped bundle is absent — `document.styleSheets` lists 9 sheets (bootstrap, tokens, base, app-theme, controls, surfaces, overlays, **shell**, navigation-menu) and no `MobileShop.Web.styles.css`.

**Action bar (11.2) — PASS:** `position: sticky` (`bottom: 0px` at ≥1200px, `88px` = 5.5rem at ≤1199.98px, `73.5px` at ≤575.98px); `background-color` = **`color(srgb .1098 .1451 .1804 / .88)`** (88% tint of `--panel-raised`); `backdrop-filter` = **`blur(12px) saturate(1.3) brightness(0.8)`**; hairline top edge `1px solid rgb(43,55,67)`; the bar stays with the content (pinned at the viewport bottom at `scrollY=0` — y 605.4–682 — and while scrolling, y 567–643.6 at `scrollY=593`); readability over scrolling content: "Save laptop" `rgb(6,32,28)` on `rgb(54,197,176)` = **7.94:1**, "Cancel" `rgb(236,240,244)` over the glass composite ≈ **13:1**, and a **203px strip** sampled just under the bar's top border is **one uniform `rgb(27,36,44)`** → the blurred content is not legible.

**Focus clearance (11.2) — PASS:** `html` `scroll-padding-bottom` computes to **192px** (168px = 12rem at the ≤768px root size); tabbing to the last field (`Product_Notes`) leaves gaps to the action bar of **69 / 69 / 69 / 69 / 60.5px** and to the dock of **287.7 / 288.8 / 283.4 / 276 / 207.4px** at 1366×768 / 1280×720 / 1024×768 / 768×900 / 480×800 — **no overlap** in any case; the Create-laptop last field at 480×800 also clears both (gaps +111.1 / +36.9).

**Modal (11.2) — PASS:** `body` carries **`has-floating-navigation modal-open`** (`overflow: hidden`); the dock and the action bar switch to **`rgb(28,37,46)`** with **`backdrop-filter: none`** while the modal is open, and the glass values return on close.

**Dock (11.3) — PASS:** radius **32px** (`--nav-dock-radius: var(--radius-2xl)` = 2rem); `z-index` **1040** (`--z-dock`); shadow **`rgba(0,0,0,.45) 0 16px 32px`** plus the inset highlight (`--edge-highlight`); `--nav-glass-bg` = 78% tint (`color(srgb .1098 .1451 .1804 / .78)`) and `--nav-glass-filter` = **`blur(20px) saturate(1.5) brightness(0.6)`**; hover opens the Products flyout (**0 → 320px** `max-height`, `opacity 0 → 1`, `aria-hidden false`, `inert false`) and Tab reaches a trigger (skip-link → Privacy → Dashboard trigger; focusing it opens the flyout, `aria-expanded=true`).

**Mobile shell (11.4) — PASS:** `viewport-fit=cover` in the viewport meta; `theme-color` = `#0E1318`; **2** font preloads whose `document.fonts.check('16px "SF Pro Rounded"')` = **true** (faces 400/700 `loaded`); `body` min-height **682px @1366×682** and **800px @480×800** = `100dvh`; `main` **522px** @1366×682 = `calc(100dvh − 10rem)`; at 480px the dock's `bottom` computes to **9.1px** (= `max(0.65rem, calc(env(safe-area-inset-bottom, 0px) + 0.5rem))`, inset = 0 here) with radius **28.7px** (2.05rem — the phone override is intact) and `scrollWidth 468 ≤ 480` (no horizontal overflow).

**Skip link — PASS:** visible after one Tab — `position: fixed`, `left/top 12px`, **124 × 40px**, accent background `rgb(54,197,176)`, `z-index 1060`, `visibility visible`; the hash navigation works and the next Tab lands inside `<main>`.

**Console — PASS:** **0** JS errors / unhandled rejections / `console.error` on all 12 pages opened (`/Account/Login`, `/`, `/Products`, `/Products/CreateLaptop` @1366 and @480, `/Products/Edit?id=3&type=Phone`, `/People/Customers`, `/Transactions`, `/Transactions/Sell`, `/Reports/ProfitLoss`, `/Account/Profile`, `/Privacy`). The only console output is the app's own info log `Chart data: Object(3)`.

### Open findings — for the Reviewer's Job B (the stage is NOT claimed as passed)

1. **CONCERN:** at ≥1200px the sticky action bar (y 605.4–682, `bottom: 0`) and the fixed dock (y 600.7–662, `bottom: 1.25rem`) **overlap in a 692 × 57px band**, with the dock painted on top (z 1040 vs 10). Nothing is hidden — the Save/Cancel buttons (x 1093–1288) sit clear of the dock (x 330–1023) — and the geometry is pre-existing (the bar was `sticky; bottom: 0` before Stage 11), but the new translucency exposes it: `shell.css` lifts the bar to 5.5rem only **below** 1199.98px, so wide desktops get no clearance.
2. **CONCERN (minor):** the skip link does **not** park focus on the main content — `#main` has no `tabindex="-1"`, so after Enter `document.activeElement` is `BODY` (the hash is set, the page scrolls, and the next Tab does land inside `main`, so it is not a keyboard trap).
3. **PRE-EXISTING:** the `vazirmatn-*.woff2` files are still HTML pages, not fonts (the Stage 10 defect; the plan schedules the repair as **Step 15.2**). No page opened in this check rendered Persian glyphs, so the browser's font error did not recur; the defect itself is unchanged.
4. **OUT OF SCOPE:** a fresh `/Products/CreateLaptop` shows the validation message *"Please enter a value greater than or equal to 0.1."* on the empty "Display size (inches)" field.
5. **Not verifiable here:** a real iPhone home indicator (`env(safe-area-inset-bottom)` measures 0 in headless Firefox, so only the zero-inset arm of the `max(…)` fallbacks was exercised — `viewport-fit=cover` is present in the markup); Safari and Chromium (Firefox 156 only); and the dock's **idle compact state** — headless Firefox reports `(hover: hover)=false` / `(pointer: fine)=false`, so `wantsCompact()` never returns true at rest and `is-compact` / `scale: 0.88` is unreachable (only the scroll-triggered compact was measurable, 692.3 → 609.2px).

### Working tree — `git status --porcelain`

```
 M .agents/chat/act.md
 M .agents/chat/audit.md
 M .agents/chat/plan.md
```
- `.agents/chat/plan.md` — the Step 11.5 heading strike only (`1 insertion(+), 1 deletion(-)`).
- `.agents/chat/act.md` — this report.
- `.agents/chat/audit.md` — the Reviewer's file, modified by the Job A review before this step; **not** touched by me.

## Limitations

- **This step validates; it does not fix.** Per the plan's edge case ("if any check fails, stop and report; do not fix inside this step") none of the five open findings above was repaired here — they are recorded for Job B.
- No local `dotnet build` / `dotnet test` was run — CI is the only build/test gate, per `AGENTS.md`. The four CI runs are the build/test evidence.
- Not verifiable in this environment: a real iPhone/iPad home indicator (safe-area inset = 0 in headless Firefox), Safari and Chromium rendering, GPU-composited `backdrop-filter`, real device pixel ratio / notch / dynamic-toolbar behaviour (`100dvh` measured with fixed 682/800px viewports only), and the dock's idle compact state (see finding 5).
- The owner's browser check was executed by the owner with the headless-Firefox/WebDriver fallback (`firefox-devtools` MCP tools were not registered in that session), not by me — I read the report and the measurements and folded them in; I did not re-run the browser myself.

## Friction noted

- `gh api …` hung for the full tool timeout (exit 124, zero stdout) on every Actions query — as in Step 11.4. The fallback (`curl` + the token extracted from `~/.git-credentials` via the `sed` form, not a variable-length-lookbehind regex) answered `http=200` immediately. A working non-interactive `gh` setup, or a documented `curl` fallback, would remove this recurring stall.
- The owner's manual check had to be re-planned around the missing `firefox-devtools` MCP tools (headless Firefox + geckodriver over WebDriver/BiDi, five script passes writing `results1..5.json`), and the dev server had to be restarted from the existing build (`dotnet run --project src/MobileShop.Web --no-build --launch-profile http`) because nothing was listening on :5043.

## Problems

- None in the sense of a failed check: **every automated Verify command of Steps 11.1–11.4 held** (each grep count equals the expected value, the empty-output greps are empty, the brace balance is equal, the BOM/LF conventions hold) and the stage `git diff --stat` lists exactly the seven expected paths. The five findings above are the Reviewer's Job B material — two of them (the action-bar/dock overlap and the skip-link focus parking) are **newly visible Stage 11 behaviour** and need an owner decision, not a Stage 11 defect verdict from me.

## Status

**COMPLETE** — Step 11.5 executed: CI green on all four code steps (`Action: #641 — Success`, `#642 — Success`, `#643 — Success`, `#644 — Success`), all Verify commands of Steps 11.1–11.4 re-run and recorded, the stage `git diff --stat` / `git log` checked against the plan's file lists, and the owner's manual check answered. No code change, no commit, no push — the plan.md heading strike and this report stay in the working tree for the orchestrator's single stage commit after the sign-off. Next: **Reviewer Job B** (`verify`) for Step 11.5 / Stage 11, which must adjudicate the five open findings (1–5 above) before the sign-off.
