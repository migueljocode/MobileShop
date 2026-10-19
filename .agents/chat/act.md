# Actor — Step 8.5 — Stage 8 validation

- Commit: none — validation step, no code change; plan.md heading struck for the stage sign-off commit
- Verification:
  - CI, confirmed independently per commit via `GET /repos/migueljocode/MobileShop/actions/runs?head_sha={full_sha}` (`gh api`), re-confirmed at the end of this step:
    - `Action: #623 — Success` (workflow `.NET CI`, head_sha `54c94fd`, status `completed`, conclusion `success`) — Step 8.1, commit `54c94fd`
    - `Action: #624 — Success` (workflow `.NET CI`, head_sha `83b8e03`, status `completed`, conclusion `success`) — Step 8.2, commit `83b8e03`
    - `Action: #625 — Success` (workflow `.NET CI`, head_sha `8ff2f16`, status `completed`, conclusion `success`) — Step 8.3, commit `8ff2f16`
    - `Action: #626 — Success` (workflow `.NET CI`, head_sha `9475fcd`, status `completed`, conclusion `success`) — Step 8.4, commit `9475fcd`
    - `total_count=1` for each head_sha (one matching workflow run per commit; no pending or interrupted run recorded as success)
  - Step 8.1 greps (current tree):
    - `grep -rnE '<label( class="form-label")?>Finished price' src/MobileShop.Web/Pages --include=*.cshtml` → no output (exit 1) ✔
    - `grep -rc 'for="finished-price"' src/MobileShop.Web/Pages/Products --include=*.cshtml` → `1` in exactly 12 files (`CreateAppleId`, `CreateCable`, `CreateCase`, `CreateCharger`, `CreateGlass`, `CreateLaptop`, `CreatePhone`, `CreatePortableStorage`, `CreatePowerBank`, `CreateSmartWatch`, `CreateTablet`, `Edit`) and `0` in the 4 others (`PersonPicker`, `SecondHand`, `Index`, `Details`) ✔
    - `grep -rc 'id="finished-price"' src/MobileShop.Web/Pages/Products --include=*.cshtml` → identical result: `1` in the same 12 files, `0` in the other 4 ✔
    - `grep -c 'aria-label="Select transaction' src/MobileShop.Web/Pages/Transactions/Index.cshtml` = `1` ✔
  - Step 8.2 checks (current tree):
    - `node --check src/MobileShop.Web/wwwroot/js/site.js` → no output, exit `0` ✔
    - `grep -c "aria-invalid" src/MobileShop.Web/wwwroot/js/site.js` = `2` (>= 2) ✔
    - `grep -c "aria-describedby" src/MobileShop.Web/wwwroot/js/site.js` = `2` (>= 1) ✔
    - `grep -c "MutationObserver" src/MobileShop.Web/wwwroot/js/site.js` = `1` ✔
  - Step 8.3 checks (current tree):
    - Brace balance `awk '{o+=gsub(/\{/,"{");c+=gsub(/\}/,"}")} END{print o"/"c}' FILE`: `controls.css: 29/29`, `app-theme.css: 119/119` (two equal numbers each) ✔
    - `grep -c "^\.btn" src/MobileShop.Web/wwwroot/css/app-theme.css` = `0` — the Step 8.3 expectation was `4` **at the end of 8.3**; Step 8.4 then deleted the focus group `.btn:focus-visible,` / `.btn:focus,` / `.btn:active:focus,` / `.btn-link.nav-link:focus,` exactly as the 8.3 Verify text anticipated ("which Step 8.4 removes"), so `0` is the correct end-of-stage value and matches the Step 8.4 check ✔
    - `grep -c "^\.btn-primary" src/MobileShop.Web/wwwroot/css/app-theme.css` = `0` ✔
    - `grep -c "translateY(-1px)" src/MobileShop.Web/wwwroot/css/controls.css` = `0` ✔
    - `grep -c "^\.btn:hover" src/MobileShop.Web/wwwroot/css/app-theme.css` = `0` ✔
    - `grep -c "controls.css" src/MobileShop.Web/Pages/Shared/_Layout.cshtml` = `1` ✔
    - Link order in `_Layout.cshtml` (lines 9-16): `bootstrap.min.css` → `tokens.css` → `site.css` → `app-theme.css` → `components.css` → `controls.css` → `navigation-menu.css` → `MobileShop.Web.styles.css` ✔ (components.css → controls.css → navigation-menu.css, as required)
  - Step 8.4 checks (current tree):
    - Brace balance for both CSS files: `controls.css: 29/29`, `app-theme.css: 119/119` ✔
    - `grep -c "^\.form-control" src/MobileShop.Web/wwwroot/css/app-theme.css` = `0` ✔
    - `grep -c "^fieldset" src/MobileShop.Web/wwwroot/css/app-theme.css` = `0` ✔
    - `grep -c "input-validation-error" src/MobileShop.Web/wwwroot/css/controls.css` = `2` (>= 2) ✔
    - `grep -cE "rgba\(" src/MobileShop.Web/wwwroot/css/controls.css` = `0` ✔
    - `grep -c "^\.btn" src/MobileShop.Web/wwwroot/css/app-theme.css` = `0` ✔
  - Safety greps re-run: `grep -rn "form-control-plaintext" src/MobileShop.Web --include=*.cshtml --include=*.js | grep -v "/lib/"` → no output (exit 1) ✔; `grep -rnE "btn-(warning|info|outline-(success|danger|dark|info))" src/MobileShop.Web --include=*.cshtml --include=*.js | grep -v "/lib/"` → no output (exit 1) ✔
  - Stage scope — `git diff --stat 23cb4af..9475fcd` (exact output):
    ```
     .agents/chat/plan.md                               |   8 +-
     .../Pages/Products/CreateAppleId.cshtml            |   4 +-
     .../Pages/Products/CreateCable.cshtml              |   2 +-
     .../Pages/Products/CreateCase.cshtml               |   2 +-
     .../Pages/Products/CreateCharger.cshtml            |   2 +-
     .../Pages/Products/CreateGlass.cshtml              |   4 +-
     .../Pages/Products/CreateLaptop.cshtml             |   2 +-
     .../Pages/Products/CreatePhone.cshtml              |   4 +-
     .../Pages/Products/CreatePortableStorage.cshtml    |   2 +-
     .../Pages/Products/CreatePowerBank.cshtml          |   2 +-
     .../Pages/Products/CreateSmartWatch.cshtml         |   2 +-
     .../Pages/Products/CreateTablet.cshtml             |   2 +-
     src/MobileShop.Web/Pages/Products/Edit.cshtml      |   4 +-
     src/MobileShop.Web/Pages/Shared/_Layout.cshtml     |   1 +
     src/MobileShop.Web/Pages/Transactions/Index.cshtml |   2 +-
     src/MobileShop.Web/wwwroot/css/app-theme.css       | 240 ---------------------
     src/MobileShop.Web/wwwroot/css/controls.css        | 221 +++++++++++++++++++
     src/MobileShop.Web/wwwroot/js/site.js              |  58 +++++
     18 files changed, 301 insertions(+), 261 deletions(-)
    ```
    Exactly the expected set: the 13 files of Step 8.1 (11 Products Create pages + `Products/Edit.cshtml` + `Transactions/Index.cshtml`), `wwwroot/js/site.js`, `wwwroot/css/controls.css` (new, `A` in `--name-status`), `wwwroot/css/app-theme.css`, `Pages/Shared/_Layout.cshtml` and `.agents/chat/plan.md`. **Nothing unexpected** — no `tokens.css`, `site.css`, `components.css`, `navigation-menu.css`, `navigation-menu.js`, `site.js`-adjacent script or `src/MobileShop.Api` file changed.
  - `git log --oneline 23cb4af..9475fcd`:
    ```
    9475fcd feat(css): move fields, checks and focus rings onto the v1 tokens
    8ff2f16 feat(css): add the Liquid Glass button family in controls.css
    83b8e03 feat(web): link validation state and messages for assistive tech
    54c94fd fix(web): associate the finished-price labels and name the transaction checkbox
    ```
    (stage base `23cb4af` = `docs(plan): fold Stage 8 Job A corrections into the Stage 8 plan`; `origin/main` = `9475fcd` = local `HEAD`)
  - `git status --porcelain` (uncommitted, expected):
    ```
     M .agents/chat/plan.md
     M .agents/chat/act.md
     M .agents/chat/audit.md
     M graphify-out/GRAPH_REPORT.md
     M graphify-out/graph.json
    ```
    The two `graphify-out/` files are the expected incremental graph refresh (dirty graph files are expected, not churn).
  - **Owner's manual check — ANSWERED.** The plan assumed no browser was available; in fact the .NET SDK and a headless Firefox 156.0.1 were present, the app was run at `http://localhost:5043` (Development, seeded data) and driven with real mouse/keyboard input over WebDriver/BiDi. Full browser report: `/tmp/kilo/stage8-owner-check.md`; screenshots: `/tmp/kilo/stage8/01-dashboard-480.png`, `02-products-480.png`, `03-createphone-480.png`, `04-login-invalid.png`, `05-dashboard-desktop.png`, `06-products-desktop.png`, `07-createphone-desktop.png`, `08-dock-expanded.png`. The step's list ("buttons on 5 pages (hover, press, focus with Tab, disabled); a field in each state (focus, disabled, read-only finished price, invalid after submitting empty Login); selects and checkboxes; People filters; on a phone (coarse pointer) every button and field is at least 44px tall; in a narrow desktop window they are about 38px; repeat the key ones in Safari and Firefox") is answered item by item below.
    - Buttons — **PASS (with 2 concerns, see Findings)**: `.btn.btn-primary` (/Products) radius **12px** (= `--radius-md` .75rem), font-weight **400**, bg/border **rgb(54,197,176)** → **no gray edge**, transition-duration `0.15s ×3, 0.22s, 0.09s` and timing `cubic-bezier(0.4,0,0.2,1) ×4, cubic-bezier(0.22,1,0.36,1)` (token values, not Bootstrap defaults); `.btn-danger` (/Account/Logout) bg/border **rgb(240,112,122)**, radius 12px, fw 400 → no gray edge; /Transactions/Sell "Record sell" + "Create customer" and /Account/Profile "Save changes" radius 12px, fw 400, teal bg/border.
    - Button hover — **PASS**: real pointer move over primary and danger → `transform: none` (no `translateY(-1px)` lift), `:hover` matches, hover bg switches to the accent-strong/danger mix.
    - Button press — **PASS**: mousedown held → `transform: matrix(0.98, 0, 0, 0.98, 0, 0)` = **scale(0.98)** on primary **and** danger; `none` after release.
    - Button focus — **PASS**: keyboard Tab → `:focus-visible` matches, box-shadow `rgb(14, 19, 24) 0 0 0 2px, rgb(54, 197, 176) 0 0 0 4px` (= `--focus-ring`), `outline: none`.
    - Fields — **PASS (1 concern, see CONCERN-3)**: focused #Username border **rgb(54,197,176)** + ring `rgb(14,19,24)/rgb(54,197,176)`, h 42.7px @root 14px (48.8px @16px), radius 10.5px @14px (12px @16px); empty Login submit → class `form-control input-validation-error`, `aria-invalid="true"`, `aria-describedby="val-username Username-error"`, `span#val-username` with `aria-live="polite"` and text "The Username field is required." (#Password same pattern via `val-password`); **unfocused invalid border rgb(240, 112, 122)** (= `--danger`, visible red, 1px); **read-only** `#finished-price` → `readOnly=true`, fill rgb(24,32,39) (token fill, **not** a blue-tinted rgba), border rgb(43,55,67), text rgb(147,163,179), opacity 1, h 44px, radius 12px; **disabled** /Transactions/Sell selects → fill rgb(24,32,39), text rgb(147,163,179), border rgb(43,55,67), opacity 1, contrast ≈ 6.2:1.
    - Selects and checkboxes — **PASS (1 failure, see FAIL-1)**: `.form-select` background-image is the light-chevron **data-URI with `stroke='%23dee2e6'`** (not `none`, not `#343a40`), background-position `calc(100% - 12px) 50%`, size 16px 12px, `appearance: none`, padding-right 36px, h 44px, radius 12px; unchecked checkbox bg rgb(14,19,24) / border rgb(102,120,138); **checked** checkbox bg/border **rgb(54,197,176)** with a visible white check glyph (SVG 20×20, `stroke='%23fff'`); People radios #peopleAll/#peopleCustomers/#peopleSellers → checked teal bg with white dot glyph (`circle r='2' fill='%23fff'`), labels All/Customers/Sellers present.
    - Label association (Step 8.1) — **PASS**: a real click on the "Finished price (Toman)" label text on **/Products/CreatePhone** → `document.activeElement.id === "finished-price"`; same on **/Products/Edit?id=3&type=Phone** → `document.activeElement.id === "finished-price"` (`readOnly=true`); on /Transactions **26/26** row checkboxes carry an `aria-label` starting with "Select transaction" (e.g. "Select transaction: Apple iPhone 14", "Select transaction: Baseus Lightning to USB-A Cable").
    - Responsive size — **PASS (as designed)**: at a 480px viewport the root font is **14px**, so a `.btn` is **38.4px** tall and a `.form-control` **38.5px** (= `--size-control` 2.75rem × 14px), radius 10.5px (= `--radius-md` × 14px) — exactly the ~38px the plan predicts; the 44px floor lives in the coarse-pointer rules `controls.css:87` (`.btn { min-height: var(--size-hit-min) }`) and `controls.css:210` (`.form-control, .form-select`), with `--size-hit-min: 44px` in `tokens.css:123` (grep-verified; not emulable headless).
    - Dock — **PASS**: `.floating-navigation` + `.navigation-dock` present on Dashboard, Products, Create, Transactions, Profile; at rest @480 the dock is **470×58px**, flyout height 0, opacity 0, `visibility: hidden`, `aria-hidden="true"`, `inert`; hovering the "Products" `[data-navigation-trigger]` expands it (flyout height 0 → **308px**, opacity 0 → 1, visibility visible, `aria-hidden` false, `inert` false, `navigation-panel-products` visible, nav gains `.is-engaged`); @1366 dock 692×403px, flyout 690×341px.
    - Typography / no unstyled page — **PASS**: body font-family resolves to **"SF Pro Rounded", "Vazirmatn", -apple-system, …**; `document.styleSheets.length` = **8** (bootstrap.min, tokens, site, app-theme, components, controls, navigation-menu, scoped MobileShop.Web css) and a `.btn` rule is present in the loaded sheets; "SF Pro Rounded" 400/700 loaded (Vazirmatn registered, `unloaded` — no Persian glyphs needed on these pages).
    - Dashboard buttons — N/A: `document.querySelectorAll('.btn').length === 0` on `/` (the dashboard uses stat cards + the nav dock only), so there was no button to measure there.
- Limitations:
  - **Not verifiable in this environment** (the owner's list could not be fully closed):
    - **Safari / WebKit** — only Firefox 156.0.1 headless was available; the "repeat the key ones in Safari and Firefox" item was done in Firefox only.
    - **A real coarse pointer / touch device** — the `@media (pointer: coarse)` 44px minimum-height rules are verified by grep only (`controls.css:87`, `controls.css:210`); the dock hover was driven by a synthetic mouse, so tap behaviour is untested.
    - **Real touch input** — no touch events exercised.
    - **Screen-reader announcement** — `aria-live="polite"` / `aria-describedby` / checkbox names were verified as DOM attributes, not as spoken output.
    - Also out of scope for this check: forced-colors / high-contrast, print stylesheet, RTL/Persian text rendering, real-device performance, iOS/Android WebView, zoom ≠ 100%.
    - The 480px viewport was obtained through BiDi `browsingContext.setViewport` (Firefox's minimum window width is 500px); the numbers are real measurements but in an emulated viewport, not a real 480px window.
  - This step modified no production file: only the `Step 8.5` heading in `.agents/chat/plan.md` and this report. Per the run instructions, **no commit and no push** — the plan.md strike, this act.md, audit.md and the graphify-out refresh stay in the working tree for the Reviewer's stage sign-off commit.
- Friction noted:
  - `gh run list --commit <sha> --limit 5 --json runNumber,...` fails twice over in this environment: the field is `number` (not `runNumber`) in this gh version, and `--commit` with a short SHA returned `[]`. The reliable path is the full SHA against `gh api repos/{owner}/{repo}/actions/runs?head_sha={full_sha}` (as `AGENTS.md` prescribes), reading `run_number`, `status`, `conclusion`.
  - `gh auth status` reports the stored token as invalid and the shared outbound IP hits the unauthenticated API rate limit intermittently (`HTTP 403` on `gh run list`, `HTTP 401` on GraphQL). The REST `actions/runs` endpoint answered unauthenticated once the transient limit cleared. The Step 8.4 actor had to read the credential from `~/.git-credentials` into `GH_TOKEN`; this step did not need to.
  - The Step 8.3 Verify expectation `grep -c "^\.btn" app-theme.css` = `4` is only true at the end of Step 8.3; re-running it after Step 8.4 correctly yields `0`. A future stage gate must not treat the `0` as a regression — the 8.3 step text itself says those four rules "are the focus group … which Step 8.4 removes".
  - The plan's Stage-8 evidence limits (".NET SDK, browser … no browser in the sandbox") were **wrong**: the .NET SDK and a headless Firefox were both present, so the manual check was executable. The browser agent had to bypass the `firefox-devtools` MCP server (its `Process … unexpectedly closed with status 1` failure was caused by no `DISPLAY` plus the `/usr/bin/firefox` snap wrapper) by driving the same Firefox through geckodriver/WebDriver/BiDi over HTTP. Future stages should assume a real browser check is possible instead of deferring it to the owner.
  - `--headless --firefox-path /snap/firefox/8969/usr/lib/firefox/firefox` had to be written into the **global** `~/.config/kilo/kilo.jsonc` (not a repo file) to make the MCP server launch Firefox; stopping the stale server then lost the MCP tool registry for that session. Reusable, but worth recording.
- Problems:
  - The owner's manual check is answered and it **found two real defects**. They are recorded here as findings for the Reviewer's Job B — they are not fixed in this step (the step's edge cases say "if any check fails, stop and report; do not fix inside this step") and they are **not** a claim that the stage passed.
  - **FAIL-1 — `.form-check-input` (checkbox/radio) is 16×16px, not the 20px (1.25rem) target size.** `controls.css:177-179` styles colour only (`border-color`, `background-color`); Bootstrap's `width: 1em; height: 1em` still wins. Measured 16px × 16px on the /Transactions row checkboxes and on the /People radios (#peopleAll/#peopleCustomers/#peopleSellers); the other two radios are the /Reports/ProfitLoss `.btn-check` group. Affects every check/radio in the app, including the Create-form checkboxes.
  - **FAIL-2 — horizontal overflow on /Products at a 480px viewport.** `documentElement.scrollWidth` **841** vs `innerWidth` **480** (361px overflow). Culprit: the category-filter `.btn-group` row is **816px** wide with `overflow-x: visible` and Bootstrap `nowrap` (it does not wrap); the `.data-table` (654px) inside `.table-responsive` reaches the right edge at 678px. Side effect: the filter `.btn` labels wrap and their height inflates to **59.4px**. Dashboard (**468 ≤ 480**) and /Products/CreatePhone (**468 ≤ 480**) have no overflow.
  - **CONCERN-3 — a focused invalid field keeps the teal border/ring instead of the danger red on /Account/Login.** `app-theme.css:193` `.account-login-field .form-control:focus` (specificity 0-3-0) beats `controls.css:165` `.form-control.input-validation-error` (0-2-0). Measured: focused #Username border-color **rgb(54, 197, 176)** (accent); the **unfocused** invalid field is correctly **rgb(240,112,122)**. `aria-invalid="true"`, `aria-describedby="val-username Username-error"` and the `aria-live="polite"` message span are all present, so the semantics are right and only the colour is wrong.
  - **CONCERN-4 — the Login "Sign in" button keeps the legacy radius 8px** (`--r-control`, `app-theme.css:247` `.account-login-submit { border-radius: var(--r-control) }`, direct declaration) while every other button is 12px (`controls.css:5-6` sets the variable `--bs-btn-border-radius: var(--radius-md)`, which the later direct `border-radius` in app-theme.css overrides for that button). This is **by plan** — Step 8.3's edge cases explicitly keep the login submit's own rule — but the inconsistency is recorded.
  - **CONCERN-5 — the app has no natively `disabled` button anywhere**, so the disabled-button look could not be observed as shipped: `button[disabled]` count = 0 on /Account/Login, /Products, /Transactions/Sell and /Account/Profile (only transient JS in `product-create-modal.js:168` and `person-picker.js:129`). Forcing `disabled` in the DOM gives opacity 0.65 (Bootstrap `--bs-btn-disabled-opacity`) while the teal bg/border are kept — dimmed, but the border is not neutralised.
- Status: COMPLETE — the automated part of the validation is fully green (CI #623, #624, #625, #626 all completed/successful; every Step 8.1-8.4 grep, brace-balance and stage-scope check holds; the stage diff contains exactly the expected files) and the owner's manual check is now answered with real browser evidence. **Two FAIL findings (FAIL-1, FAIL-2) and three concerns (CONCERN-3..5) are open for the Reviewer's Job B** — the stage is not to be signed off as clean until they are dispositioned.

Next: the Reviewer's **Job B** for Step 8.5, with FAIL-1, FAIL-2 and CONCERN-3..5 as the findings to rule on; the stage sign-off then commits the state files (act.md, audit.md, TODO.md, plan.md) in one stage commit.
