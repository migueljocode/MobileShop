# Actor report — Step 12.5 (Stage 12 validation)

- Commit: none — the stage code is committed as one stage commit `9875771` (`feat(web): restyle the login, dashboard and people pages`); this validation changes no production file. Nothing was staged, committed or pushed by this step. The uncommitted working tree holds `.agents/chat/act.md` (this file), the `plan.md` heading strike of Step 12.5, and the pre-existing `.agents/chat/audit.md` + `graphify-out/` edits.

## Verification

### CI (stage commit `9875771`, sha `9875771a45db3d6e2d59e49ab739df320bb7e0b8`)
- `GET /repos/migueljocode/MobileShop/actions/runs?head_sha=9875771…` → `total_count: 1`; workflow `.NET CI` (`.github/workflows/dotnet.yml`), `run_number: 645`, `status: completed`, `conclusion: success`.
- **`Action: #645 — Success`** (the only run for that head_sha; it covers all four code steps 12.1-12.4 in the single stage commit).

### Step 12.1 verify commands (re-run over the current tree) — all as expected
- `grep -c "password-visibility-toggle" src/MobileShop.Web/Pages/Account/Login.cshtml src/MobileShop.Web/Pages/Account/Profile.cshtml` → `Login.cshtml:0`, `Profile.cshtml:0` (the duplicated inline button markup is gone from both pages).
- `grep -c "_PasswordToggle" …Login.cshtml …Profile.cshtml` → `Login.cshtml:1`, `Profile.cshtml:3`.
- `grep -c "<svg" src/MobileShop.Web/Pages/Shared/_PasswordToggle.cshtml` → `1`.

### Step 12.2 verify commands — all as expected
- Brace balance `src/MobileShop.Web/wwwroot/css/login.css`: `{` = 27, `}` = 27 (equal).
- `git ls-files src/MobileShop.Web/wwwroot/css | grep -c app-theme.css` → `0`.
- `grep -rn --exclude-dir=bin --exclude-dir=obj "app-theme.css" src --include=*.cshtml --include=*.cs --include=*.json | grep -v "/lib/"` → no output (no dangling reference).
- `grep -cE "uppercase|letter-spacing|var\(--r-" src/MobileShop.Web/wwwroot/css/login.css` → `0`.
- `grep -c "login.css" src/MobileShop.Web/Pages/Shared/_Layout.cshtml` → `1`.
- `grep -c 'tabindex="-1"' src/MobileShop.Web/Pages/Shared/_Layout.cshtml` → `1`.
- Stylesheet link order in `_Layout.cshtml` (lines 12-20): `bootstrap.min.css` → `tokens.css` → `base.css` → `controls.css` → `surfaces.css` → `overlays.css` → `shell.css` → `login.css` → `navigation-menu.css` — matches the required order.

### Step 12.3 verify commands — as expected
- `grep -c "<h5" src/MobileShop.Web/Pages/Index.cshtml` → `0`.
- `grep -c 'card-title h5' …Index.cshtml` → `1`.

### Step 12.4 verify commands — as expected
- `grep -c "segmented" src/MobileShop.Web/Pages/People/Index.cshtml` → `1`.
- `grep -c "form-check" …People/Index.cshtml` → `0`.
- `grep -c "aria-sort" …People/Customers.cshtml …People/Sellers.cshtml` → `3` for each.
- `grep -c "_EmptyState" …Customers.cshtml …Sellers.cshtml` → `1` for each.

### Stage retiring check
- `ls src/MobileShop.Web/wwwroot/css/` → `base.css  controls.css  login.css  navigation-menu.css  overlays.css  shell.css  surfaces.css  tokens.css` — **no `app-theme.css`, no `components.css`, no `site.css`**.

### Stage scope — `git show --stat 9875771`
```
 .agents/chat/plan.md                               |   8 +-
 graphify-out/GRAPH_REPORT.md                       | 468 ++++++++++-----------
 graphify-out/graph.json                            |   2 +-
 src/MobileShop.Web/Pages/Account/Login.cshtml      |  15 +-
 src/MobileShop.Web/Pages/Account/Profile.cshtml    |  45 +-
 src/MobileShop.Web/Pages/Index.cshtml              |   2 +-
 src/MobileShop.Web/Pages/People/Customers.cshtml   |  43 +-
 src/MobileShop.Web/Pages/People/Index.cshtml       |  20 +-
 src/MobileShop.Web/Pages/People/Sellers.cshtml     |  43 +-
 src/MobileShop.Web/Pages/Shared/_Layout.cshtml     |   4 +-
 .../Pages/Shared/_PasswordToggle.cshtml            |  15 +
 .../wwwroot/css/{app-theme.css => login.css}       |  55 +--
 12 files changed, 361 insertions(+), 359 deletions(-)
```
- `git show --name-status --no-renames 9875771` confirms exactly the expected set: `M` the six modified pages + `_Layout.cshtml` + `.agents/chat/plan.md` + the two `graphify-out/` files, `A` `Pages/Shared/_PasswordToggle.cshtml` (new) and `wwwroot/css/login.css` (new), `D` `wwwroot/css/app-theme.css` (deleted). The stat above shows `app-theme.css → login.css` only because git's rename detection fired; `--no-renames` shows the true delete + add.
- The `plan.md` hunk in the commit strikes the headings of Steps 12.1, 12.2, 12.3, 12.4 — the four code steps are in this single commit. Nothing unexpected: no `Api`, `Dal`, `Models`, `Services`, `Tests`, `bin` or `obj` paths, no page-model/handler/route change.
- `git log --oneline -3`:
  - `9875771 feat(web): restyle the login, dashboard and people pages`
  - `4e89e3c docs(plan): fold Stage 12 Job A corrections into the Stage 12 plan`
  - `1c85972 docs(stage-11): record CI runs, Job B PASS and the owner browser check`

### Owner's manual check (answered) — report: `/tmp/kilo/stage12-owner-check.md`, screenshots `/tmp/kilo/stage12/`, console sweep `/tmp/kilo/stage12/console.json`
The plan assumed no browser was available; in fact the .NET SDK and a headless Firefox were present, so the app was run at `http://localhost:5043` (Development, seeded data) and checked with a real browser (Firefox 156 via the `firefox-devtools` MCP plus a second headless Firefox through geckodriver/WebDriver BiDi for the console sweep). Passing evidence:
- **Login card glass**: `background-color: color(srgb .109804 .145098 .180392 / .92)` (92% tint of `--glass-overlay-bg`), `backdrop-filter: blur(24px) saturate(1.4) brightness(0.85)`, `border-radius: 24px` (= `--radius-xl`), `box-shadow: rgba(0,0,0,.45) 0 16px 32px` + inset `rgba(236,240,244,.1) 0 1px 0` (= `--shadow-3` + `--edge-highlight`).
- **Ambient gradients visible outside the card**: container pixels (24,41,47) / (25,48,52) / (29,37,44) differ from the card fill (27,36,44); body strip flat (14,19,24). Eyebrow "MobileShop workspace" computes `text-transform: none`.
- **Controls sized**: username input, password input and submit button are each 52px tall (= `--size-control-lg`, 3.25rem).
- **Password eye**: button 50×52, toggles the field `password` → `text` and back, `aria-pressed` false → true, label "Show password" → "Hide password", svg colour `rgb(147,163,179)` (= `--text-2`); both glyphs of the new partial render.
- **Focus rings**: after the 0.15 s transition every focusable control (username, password, eye button, submit) computes `box-shadow: rgb(14,19,24) 0 0 0 2px, rgb(54,197,176) 0 0 0 4px` (= `--focus-ring`).
- **Served HTML**: the login page contains exactly **1** `password-visibility-toggle` and exactly **1** `aria-pressed` (no duplication); the Profile page has **3** toggles and each flips only its own field (CurrentPassword / NewPassword / ConfirmPassword independently).
- **Dashboard headings**: `h1` "Dashboard" 32px → `h2` "Recent transactions" 22px → `h3` card title 20px with the `card-title h5` class (the old `h5` size kept).
- **People filter**: segmented control with `border-radius: 999px`, track `rgb(14,19,24)`, 235×42, inline-flex, gap 2px, padding 3px; exactly one pill selected — selected bg `color(srgb .211765 .772549 .690196 / .16)` (accent 16%), colour `rgb(54,197,176)`, `font-weight: 700`; unselected pills transparent, `rgb(147,163,179)`, `font-weight: 400`.
- **Filter auto-submits**: clicking "Customers" → `/People?type=customers&sortBy=name&ascending=true` with 5 customer rows (seller rows gone).
- **`aria-sort`**: baseline Phone ascending (↑, Name/Purchased `none`), then Name ascending (↑), then Name descending (↓); the arrow appears only on the active column; only the 3 sortable headers carry `aria-sort` (National ID / Type / Actions carry none).
- **Empty state**: the shared partial renders live with `.empty-state` (`role="status"`, title + hint, radius 16px).
- **Skip-link focus fix confirmed** (closes the Stage 11 finding): fresh load → Tab → skip link → Enter → `document.activeElement` is `MAIN#main` (not BODY), `location.hash === "#main"`.
- **Console**: 0 JS console entries (no errors, no warnings) across all 10 pages (BiDi `log.entryAdded` sweep; `/tmp/kilo/stage12/console.json` shows an empty array per page).
- Sign in / sign out round-trip works (admin → `/` → sign out → `/Account/Login` → sign in → Dashboard).

### Open findings for the Reviewer's Job B (the stage is NOT claimed as passed)
1. **CONCERN (carried from the Stage 8 and Stage 10 checks, still OPEN): a FOCUSED invalid login field shows the accent teal `rgb(54,197,176)` instead of the danger red `rgb(240,112,122)`; after blur it settles on the danger red.** The unfocused invalid control IS danger-red and carries `aria-invalid="true"`, `aria-describedby="val-username Username-error"` and the class `input-validation-error`. Cause: `src/MobileShop.Web/wwwroot/css/login.css:87` `.account-login-field .form-control:focus` (specificity 0,3,0) outranks `src/MobileShop.Web/wwwroot/css/controls.css:165` `.form-control.input-validation-error` (0,2,0), and `login.css` is loaded after `controls.css`. The Stage 10 Job B had assigned this fix to Step 12.2, but the plan's Step 12.2 never carried it, so it is still open.
2. **CONCERN: the login validation summary never shows content.** `login.css:163-176` styles `.account-login-validation` / `.account-login-validation ul` / `.validation-summary-errors`, and the markup DOES exist — `src/MobileShop.Web/Pages/Account/Login.cshtml:20` is `<div asp-validation-summary="ModelOnly" class="account-login-validation" role="alert"></div>` — but because the summary is `ModelOnly` and an empty submit raises only field-level errors, the div stays empty and only the two per-field messages render ("The Username field is required." / "The Password field is required."). Practically the CSS is unreachable today; no functional impact, no visual regression. (Correction to the owner report, which recorded it as "no element in the DOM": the element is in the DOM, it is simply empty.)
3. **CONCERN (pre-existing, introduced in commit `648034c`, not a Stage 12 change): on `/Reports/ProfitLoss` → Distribution tab the three share rows are `<tr class="table-danger">` and their cells compute `color: rgb(0,0,0)` — black text on the dark surface, unreadable.** Bootstrap's `.table-danger` is a whole-table variant and does nothing on a `<tr>`. Visibly broken, out of the Stage 12 file list.
4. **NOTE (not a defect): on `/Account/Login` the first Tab does not reach the skip link** because the username input carries `autofocus` (Shift+Tab twice reaches it; Enter then focuses `MAIN#main`). The skip-link focus fix itself is verified correct on the dashboard.
5. **Not verifiable here:** Safari / WebKit (only Firefox 156 desktop was exercised), a real iPhone/Android device, mobile `backdrop-filter` and touch behaviour, screen-reader announcement of `aria-sort` / `role="status"`, and the People-specific "No sellers yet" / "No customers yet" strings (both lists have seeded rows and expose no filter that can empty them; the empty-state markup and CSS were verified live on an empty list elsewhere — `/Products?type=phone&manufacturerId=6`).

### Current working tree
`git status --porcelain` (final state, after this step):
```
 M .agents/chat/act.md
 M .agents/chat/audit.md
 M .agents/chat/plan.md
 M graphify-out/GRAPH_REPORT.md
 M graphify-out/graph.json
```
`.agents/chat/plan.md` carries only the Step 12.5 heading strike (`## ~~[x] Step 12.5 — Stage 12 validation~~`); `.agents/chat/audit.md` and the two `graphify-out/` files are pre-existing edits from earlier steps. All of it is intentionally left uncommitted for the orchestrator's records commit after the sign-off.

## Limitations
- This step is validation only: it changes no production file, and every Verify command of Steps 12.1-12.4 was re-run read-only over the current tree.
- `dotnet build` / `dotnet test` were NOT run locally (CI is the only build/test gate): CI evidence is `Action: #645 — Success` for the single stage commit `9875771`.
- The manual check was executed by the owner, not by the actor; the actor folded the owner's report (`/tmp/kilo/stage12-owner-check.md`) into this record and cross-checked the CSS/markup facts that the findings depend on.
- The items in "Not verifiable here" above could not be covered by this step.

## Friction noted
- `git show --stat 9875771` renders the CSS change as a rename (`{app-theme.css => login.css}`); `--no-renames` is needed to see the true delete + add. Both forms are recorded above.
- The Stage 12 commit policy (one stage commit) meant the CI run is a single run for the whole stage, so per-step `Action: #<run>` values collapse to one run number (645) for Steps 12.1-12.4 — the per-step evidence is the re-run Verify commands recorded above.
- The dev server had been shut down before the owner's check and had to be restarted (`dotnet run --project src/MobileShop.Web --launch-profile http --no-build`) — no rebuild, no local build/test.

## Problems
None blocking this step. Open items for Job B are the four findings above (1-3 are CONCERNS, 4 is a NOTE); this report does not claim the stage passed.

## Status
COMPLETE (validation step executed; evidence recorded, the owner's manual check answered, open findings passed to the Reviewer's Job B — the stage itself is NOT claimed as passed)
