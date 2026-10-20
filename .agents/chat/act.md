# Actor report — Step 15.6 (Stage 15 validation)

- Commit: none — the stage code is committed as one stage commit `fc81aa1` (`feat(web): add preference modes, repair the Vazirmatn fonts and a solid-surfaces switch`); this validation changes no production file. Nothing was committed, staged or pushed by this step; the `plan.md` heading strike and this `act.md` stay uncommitted for the orchestrator's records commit after the sign-off.
- Verification:
  - **CI**: `GET /repos/migueljocode/MobileShop/actions/runs?head_sha=fc81aa159c438b5230aa175cd77662d7e25a8acd` → one run only: `run_number: 648`, `name: ".NET CI"`, `status: "completed"`, `conclusion: "success"`, `head_sha: fc81aa159c438b5230aa175cd77662d7e25a8acd`. **`Action: #648 — Success`** (completed **and** successful — the claim is valid). No local `dotnet build` / `dotnet test` was run (CI is the only build/test gate) and no browser or dev server was started.
  - **Stage scope — `git show --stat --no-renames fc81aa1`** (exact output):
    ```
     src/MobileShop.Web/Pages/Account/Profile.cshtml    |   7 +
     src/MobileShop.Web/Pages/Shared/_Layout.cshtml     |   2 +
     src/MobileShop.Web/wwwroot/css/login.css           |   4 +
     src/MobileShop.Web/wwwroot/css/motion.css          | 103 ++++
     src/MobileShop.Web/wwwroot/css/tokens.css          |   9 +-
     .../wwwroot/fonts/vazirmatn-bold.woff2             | Bin 268979 -> 51020 bytes
     .../wwwroot/fonts/vazirmatn-medium.woff2           | 530 ---------------------
     .../wwwroot/fonts/vazirmatn-regular.woff2          | Bin 269016 -> 50684 bytes
     src/MobileShop.Web/wwwroot/js/site.js              |  22 +
     10 files changed, 144 insertions(+), 543 deletions(-)
    ```
    (plus `.agents/chat/plan.md | 10 +-` in the same commit — the amended R27 blocks and the stage's own heading strikes.)
    Expected list vs. actual: `motion.css` (new, 103 lines) ✅ · `tokens.css` ✅ · `login.css` ✅ · `Pages/Shared/_Layout.cshtml` ✅ · `Pages/Account/Profile.cshtml` ✅ · `wwwroot/js/site.js` ✅ · `vazirmatn-regular.woff2` (replaced) ✅ · `vazirmatn-bold.woff2` (replaced) ✅ · `vazirmatn-medium.woff2` (deleted) ✅ · `.agents/chat/plan.md` ✅ · **graphify-out refresh — NOT in the commit**: `git show --stat fc81aa1` lists no `graphify-out` file. The two tracked graph files are dirty in the working tree instead (`git status --porcelain` → ` M graphify-out/GRAPH_REPORT.md`, ` M graphify-out/graph.json`), which AGENTS.md states is expected after hooks/incremental updates; flagged here as the only deviation from the expected file list, not as a defect of the stage.
  - `git log --oneline -3` → `fc81aa1 feat(web): add preference modes, repair the Vazirmatn fonts and a solid-surfaces switch` · `4a8a91f docs(plan): fold Stage 15 Job A corrections into the Stage 15 plan` · `82764d6 docs(stage-14): record CI #647, Job B PASS and the skipped owner visual pass`.
  - **Step 15.1 (motion.css)** — all checks hold:
    - brace balance `awk '{o+=gsub(/\{/,"{");c+=gsub(/\}/,"}")} END{print o"/"c}' src/MobileShop.Web/wwwroot/css/motion.css` → `17/17` (equal).
    - `grep -c 'html\[data-surfaces="solid"\]' …/motion.css` → `3` (required ≥ 2).
    - `grep -c "prefers-contrast: more" …/motion.css` → `2` (required ≥ 2).
    - `grep -c "forced-colors" …/motion.css` → `1` (required = 1).
    - `grep -c "motion.css" src/MobileShop.Web/Pages/Shared/_Layout.cshtml` → `1`.
    - `grep -c "scrim-solid-bg" src/MobileShop.Web/wwwroot/css/tokens.css` → `1`.
    - `grep -cE "rgb\(0 0 0 / 0\.72\)" …/motion.css` → `0` (the literal was not extracted).
    - `grep -rnE "rgba?\(|#[0-9A-Fa-f]{3,8}\b" src/MobileShop.Web/wwwroot/css | grep -v "tokens.css"` → **no output** (no colour literal outside `tokens.css`).
  - **Step 15.2 (Vazirmatn fonts)** — all checks hold:
    - `file …/fonts/vazirmatn-regular.woff2 …/fonts/vazirmatn-bold.woff2` → `Web Open Font Format (Version 2), TrueType, length 50684, version 33.197` and `Web Open Font Format (Version 2), TrueType, length 51020, version 33.197` — "Web Open Font Format" for both.
    - `head -c 4 …/fonts/vazirmatn-regular.woff2 | od -An -tx1` → ` 77 4f 46 32` (the `wOF2` signature).
    - `grep -c "fonts/vazirmatn" src/MobileShop.Web/wwwroot/css/tokens.css` → `2` (400 and 700 after Step 15.4 retired the Medium face).
  - **Step 15.3 (login.css)** — all checks hold:
    - `diff <(sed -n '/^\/\* BEGIN login-v1 \*\//,/^\/\* END login-v1 \*\//p' .agents/chat/plan.md | tr -d '\r') src/MobileShop.Web/wwwroot/css/login.css` → **no output** (the file is exactly the amended block).
    - `grep -c "input-validation-error:focus" …/login.css` → `1`.
    - brace balance for `login.css` → `28/28` (equal).
    - `grep -c "login.css" …/_Layout.cshtml` → `1` (one link, unchanged place).
  - **Step 15.4 (retire the Medium face)** — all checks hold:
    - `grep -c "font-weight: 500" …/tokens.css` → `0`.
    - proof awk `awk 'BEGIN{ff=0} /@font-face/{ff=1} !ff && /font-weight: *(500|600|650)/{print FILENAME": "FNR": "$0} ff && /}/{ff=0}' src/MobileShop.Web/wwwroot/css/*.css` → **no output**; the plain `grep -rnE "font-weight: *(500|600|650)" src/MobileShop.Web/wwwroot/css` also → **no output**.
    - `grep -rn --exclude-dir=bin --exclude-dir=obj "vazirmatn-medium" src README.md docs` → **no output**.
    - `git ls-files src/MobileShop.Web/wwwroot/fonts` → `sf-pro-rounded-bold.woff2`, `sf-pro-rounded-regular.woff2`, `vazirmatn-bold.woff2`, `vazirmatn-regular.woff2` — exactly the two SF Pro Rounded and the two repaired Vazirmatn files; the Medium file is gone.
    - brace balance for `tokens.css` → `6/6` (equal).
  - **Step 15.5 (solid-surfaces switch)** — all checks hold:
    - `node --check src/MobileShop.Web/wwwroot/js/site.js` → exit `0`, no output (the script parses).
    - `grep -c "mobileshop.surfaces" …/_Layout.cshtml …/site.js` → `_Layout.cshtml:1`, `site.js:2` (required: 1 and ≥ 2).
    - `grep -c "data-surfaces-toggle" …/Pages/Account/Profile.cshtml …/site.js` → `Profile.cshtml:1`, `site.js:1` (required: 1 and ≥ 1).
    - `head -c3 …/_Layout.cshtml | od -An -tx1` → ` ef bb bf` (BOM kept).
  - **Stage diff** — `git diff --stat` over the stage (the commit `fc81aa1`) matches the file lists of Steps 15.1-15.5 as recorded above; the only working-tree changes left are the uncommitted records (`.agents/chat/act.md`, `.agents/chat/audit.md`, `.agents/chat/plan.md` heading strike) and the dirty `graphify-out` refresh.
  - **The owner's manual check for this step is NOT PERFORMED (owner decision: the browser-based owner visual pass is skipped for the remaining stages — too slow).** It is recorded as an OPEN owner item and is **not** claimed as passed. No browser and no dev server were started. Every item below is "not performed — owner skipped the browser pass":
    1. With the OS "reduce transparency" (or Windows "transparency effects" off) modals, action bar and login card turn solid — Chrome/Edge on Windows only; Safari/macOS and Firefox do not ship `prefers-reduced-transparency`, and there the solid surfaces come from the `html[data-surfaces="solid"]` switch of Step 15.5 — not performed — owner skipped the browser pass.
    2. With the OS high-contrast setting, borders and text strengthen — not performed — owner skipped the browser pass.
    3. With "reduce motion", the presses and the sheet entrance do not animate — not performed — owner skipped the browser pass.
    4. `--scrim-solid-bg` exists exactly once in `tokens.css` and `motion.css` carries no colour literal — **verified mechanically** (the greps of 15.1 above: `1` and no colour literal outside `tokens.css`); the visual part is not performed — owner skipped the browser pass.
    5. The Vazirmatn face is no longer reported as `unloaded` and no font request fails — not performed — owner skipped the browser pass.
    6. Persian text renders in Vazirmatn — not performed — owner skipped the browser pass.
    7. The Profile checkbox turns the surfaces solid, stays solid after reload and on other pages, and unticking restores glass — not performed — owner skipped the browser pass.
- Limitations:
  - The owner's manual check of this step (the seven items listed above) is **skipped by owner decision** and stays an OPEN owner item — it is not claimed as passed. The whole Stage 15 is therefore validated by the greps of Steps 15.1-15.5 and by the green CI run `Action: #648 — Success` only; the visual behaviour of the preference modes, the repaired fonts and the solid-surfaces switch has **no** browser evidence.
  - The step's "Manual check (owner)" also names the four R12.B modes in at least two browsers, the focus rings visible in all of them, nothing unreadable, the dock keeping its Stage 6 behaviour in every mode, the Persian text in the Vazirmatn face, and a wrong password on the Login page keeping the invalid field danger red while focused — none of these were exercised; the last one is covered mechanically only by the `input-validation-error:focus` rule (0,4,0) present in `login.css`.
  - **Deferred open items carried from earlier stages (still open, not closed by this step):**
    - **Q-13** — at ≥ 1200px the sticky action bar and the fixed dock overlap in a 692×57px band; the owner answered "no — keep today's geometry" (the geometry is pre-existing and nothing is hidden), so the overlap remains by owner decision; status open.
    - The People-specific empty-state strings ("No sellers yet" / "No customers yet") were never exercised live (both lists have seeded rows and expose no filter that can empty them); the shared empty-state markup and CSS were verified elsewhere. Still open.
    - The deferred browser-based owner visual pass for stages 12, 13, 14 and 15 (the seven items of the Stage 14 pass, the seven items of the Stage 13 pass, the Stage 12 items not verifiable in the sandbox) remains outstanding; the plan places it in the final stage.
- Friction noted: the stage's expected file list mentions a "graphify-out refresh" inside the stage commit, but `fc81aa1` contains no `graphify-out` file — the graph outputs are dirty in the working tree instead (a hook or `graphify update .` regenerated them after the commit), so the commit's file list and the expected list differ on that point and both facts are recorded above. The `plan.md` heading strike and this report could not be committed per the owner's commit policy, so `git status --porcelain` stays dirty on purpose.
- Problems: None. Every automated check of Steps 15.1-15.5 holds and CI is green; the only outstanding gaps are the owner's skipped browser pass and the deferred open items listed under Limitations.
- Status: COMPLETE
