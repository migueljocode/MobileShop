# Reviewer — Job B (re-run on the Step 10.7 correction round, commit `19066f0`)

**Verdict: PASS** — the Step 10.7 HIGH finding is fixed, verified in code and in a browser re-test, CI is green (`Action: #639 — Success`), the stage scope is intact. **Stage 10 is signed off**: `.agents/TODO.md` was ticked and committed (see "Stage sign-off" below). Nothing was pushed by me (the Reviewer never pushes).

## CI evidence (read back myself, never inferred)

`GET /repos/migueljocode/MobileShop/actions/runs?head_sha=19066f0b83df6dde6e3e80bb810febdd8084b815` → exactly one run, workflow `.NET CI`, event `push`, `status: completed`, `conclusion: success`:

| Step | Commit | Run | conclusion |
|---|---|---|---|
| 10.1 | `81ee5f0` | **Action: #633 — Success** | success |
| 10.2 | `453265f` | **Action: #634 — Success** | success |
| 10.3 | `9de02d0` | **Action: #635 — Success** | success |
| 10.4 | `865e980` | **Action: #636 — Success** | success |
| 10.5 | `2062d8c` | **Action: #637 — Success** | success |
| 10.6 | `237849d` | **Action: #638 — Success** | success |
| 10.7 correction | `19066f0` | **Action: #639 — Success** | success |

Re-read back per commit just now: #633–#638 are still the runs of record for steps 10.1–10.6 (all `completed`/`success`, none pending or interrupted).

## What I verified (read, not taken on trust)

**1 — The correction commit.** `git show --stat 19066f0` → 4 files: the three JS files plus `.agents/chat/plan.md` (2 ++-, the note + the Actor's own step-heading strike, see 2). Message `fix(web): discard a stale combobox response after the list is dismissed`, body `Correction round for the Step 10.7 Job B HIGH finding.`, Conventional Commits, no `Co-authored-by`. No CSS, no view, no markup, no other script.

**2 — plan.md scope.** The diff is exactly the struck heading `## ~~[x] Step 10.7 — Stage 10 validation~~` (the Actor's own step heading, which the state-file table in AGENTS.md gives to the Actor, and which was **already struck in the working tree before the correction round** — I recorded it as uncommitted in my previous verdict) plus **one** new short note line under it. No other plan.md line, no other step heading touched. Not a finding.

**3 — The fix, in the code.**
- `site.js:46` — `const cancelList = hooks && typeof hooks.cancel === 'function' ? hooks.cancel : () => { };` — optional, no-op default, so a consumer that does not pass it cannot break.
- `site.js:37` — `window.wireComboboxKeys = function (...)` is still a **top-level global**; the Step 10.3 definition-site check prints `OK: definition at line 37 precedes the ready block at line 205`. `hooks.open` / `hooks.close` are untouched.
- `person-picker.js:19` `var latestSearchId = 0;`, `:101` `var searchId = ++latestSearchId;` before the fetch, `:111` `if (searchId !== latestSearchId) return;` **before** `renderResults(options)`, `:73` `cancel: function () { latestSearchId++; }`. The `finally { clearSearchLoading(); }` still clears `aria-busy` and the loading line, so a discarded response leaves no stuck loading state.
- `product-picker.js:214` `cancel: function () { latestSearchId++; }`; the pre-existing staleness check at `:244` drops the late response. Only the hooks object changed in that file.
- Unchanged as required: open/close behaviour, key handling, the live region, selection, price filling, the 150 ms blur timing (the 200 ms debounce and the 150 ms blur predate the stage — `git show 944104c:…` confirms `}, 200);` and `}, 150);`).
- Greps re-run by me: `node --check` OK for site.js / person-picker.js / product-picker.js / jalali-datepicker.js; `wireComboboxKeys` = 1 / 1 / 3; `aria-activedescendant` = 2; `cancel` in site.js = 3; `latestSearchId` = 4 (person) / 5 (product). The 10.1, 10.2, 10.5, 10.6 greps also still hold (braces 37/37 and 27/27; `^\.(modal|dropdown|popover|progress|product-picker|list-group)` = 0; `modal-content--solid` = 1; `overlays.css` = 1; `class="modal fade"` without `aria-labelledby` = 0; per-page duplicate `-title` loop prints nothing; 24 `-title` labels under `Pages/Products`; `jalali` in app-theme.css = 0; `uppercase|letter-spacing|rgb\(|scale\(1\.0` in overlays.css = 0; `jalaliFadeIn` = 2; `jalaliSkipOpen` = 3; `ArrowDown` = 2).

**4 — Stage scope.** `git diff --stat 944104c..19066f0` → exactly 19 files: `plan.md`, the ten `Pages/Products/Create*.cshtml`, `_Layout.cshtml`, `_PersonPicker.cshtml`, `app-theme.css`, `overlays.css` and the four `wwwroot/js/*.js`. The three JS files the correction touched were already in the stage's file list, so no new file entered the stage. Nothing else.

**5 — Browser re-test** (`/tmp/kilo/stage10-escape-race-retest.md`, screenshot `/tmp/kilo/stage10/20-escape-race-person.png` — I read both). It covers exactly my finding: ~2 s latency injected two ways (BiDi `network.addIntercept` hold of **1999–2001 ms**, and an in-page `fetch` delay), a **no-Escape control** proving the late response *does* re-open the list, and **Escape during the in-flight window** leaving the list closed in **both** pickers (`d-none`, `display:none`, `aria-expanded="false"`, `aria-busy` cleared, **0 option nodes added**, live region empty, still closed **+4.5 s**). The strongest variant (list **open** *and* request in flight) also passes. The normal path still works: person "ah" + ↓↓ + Enter → "Omid Bahrami", `#Input_CustomerId=4`; product "le" + ↓↓ + Enter → "Phone: Apple iPhone 11 …", `#Input_ProductId=3`, price `180 000 000`. **0 console errors.** The screenshot shows the person search input holding "ah" with no dropdown open 4.5 s after the discarded response.

## Judgement on the disclosed deviation — CORRECT, and necessary

The Actor placed `cancelList()` **before** the `if (!isOpen) return;` guard. I verified the claim: both result lists start hidden — `_PersonPicker.cshtml:23` and `_ProductPickerOptions.cshtml:3` carry `d-none` — and are opened only by `renderResults()` → `openResults()`. So while a request is in flight the list is still `d-none`, `isOpen` is false, and my original snippet (`cancelList()` behind the guard) would **never** cancel: the HIGH finding would have survived the fix. The deviation is the minimal correct resolution and is behaviour-preserving:
- Escape with the list **open** still closes it: `closeList()`, `event.preventDefault()` and `event.stopPropagation()` are still reached **only** when the list was open (`site.js:128-132`) — confirmed by the code and by the re-test (test #5: list open → Escape → `d-none`, `aria-expanded=false`, still closed +2.5 s).
- The only behavioural delta is that Escape now always invalidates an in-flight search of that input, even when the list is hidden — which is precisely the intent of the finding; the hook is a pure counter bump in both pickers, so pressing Escape with nothing in flight is effect-free.
- The helper remains safe for a consumer that does not pass `cancel` (no-op default), and the only two consumers of `wireComboboxKeys` both pass it.

## Judgement on the residual the re-test found — MEDIUM, expected, carried forward

**E1 — Escape inside the 200 ms input-debounce window still lets the list re-open ~200 ms later.** The `cancel` hook invalidates requests that already *exist*; a request only *scheduled* when Escape is pressed is created afterwards and gets a fresh id, so it is not stale. Rate: **MEDIUM** — a real but narrow timing window (≈200 ms between keystroke and request creation), and **pre-existing behaviour, not a regression**: before the correction the same window also re-opened the list (the 200 ms debounce predates the stage). The repo rule allows only **one** correction round, so this is **carried forward, not fixed here**. Proposed fix for a future step: clear the debounce timer on Escape and/or set a "dismissed" flag that suppresses the next render instead of only bumping `latestSearchId`.

## Carry-forward list

1. **MEDIUM — pre-existing, not blocking — the person-picker modal does not return focus to its trigger.** The create button has no `data-bs-toggle` (`_PersonPicker.cshtml:19` carries only `data-person-create`) and the modal is opened through the Bootstrap JS API (`person-picker.js:16`, `:154`), so Bootstrap 5.3's data-api focus restorer is never registered; after Escape the focus lands on `BODY` (WCAG 2.4.3). The open path predates Stage 10 (Stage 10 changed only the modal's CSS class and the combobox wiring). One-liner for a future step: `modal.addEventListener('hidden.bs.modal', function () { createButton.focus(); });`. The Actor correctly left it out of the correction round (only the HIGH was in scope). **Must go to the Planner as a new step — no Stage 8-10 step owns it.**
2. **LOW — pre-existing, not blocking — `/Reports/ProfitLoss` reloads the page as soon as a day is picked** (`submitFilters()` → `filterForm.requestSubmit()`, `ProfitLoss.cshtml:148`, wired to `#From`/`#To`/`#Preset` change). The file is **not** in the stage diff (`git diff --name-only 944104c..19066f0 | grep -c ProfitLoss` = 0). The Jalali picker's "focus returns to the field" is defeated on that page only — the page's filter design, not the picker. Carry as an owner note (auto-submit vs. an explicit Apply button).
3. **HIGH for the app, pre-existing, OUT OF SCOPE for Stage 10 — the `vazirmatn-*.woff2` files are GitHub HTML, not fonts.** Verified by me now: `file src/MobileShop.Web/wwwroot/fonts/vazirmatn-{regular,medium,bold}.woff2` → "HTML document, Unicode text, UTF-8 text" (the content starts with `<!DOCTYPE html>` and is a GitHub page), while `sf-pro-rounded-{regular,bold}.woff2` are "Web Open Font Format (Version 2)"; `git ls-files` shows all three Vazirmatn files are **tracked in git**, so the bad bytes are in the repository. Impact: every Persian string in the app falls back to a system font app-wide (the browser downloads ~270 KB of HTML per weight) — the same for the whole redesign, not for Stage 10. It is touched by **no** stage 8-10 step, and the later stages 11-16 do not own it either → **it must be carried to the Planner as a dedicated asset step**: replace the three files with the genuine Vazirmatn woff2 weights (the TTF sources for the PDF layer exist under `src/MobileShop.Services/PDF/Fonts/`, but the Medium weight has no TTF there and must come from the Vazirmatn release).
4. **LOW — not verifiable in this environment (Firefox 156 headless only):** Safari, a real coarse-pointer/touch device, real touch input, a real screen-reader announcement of the live region, and a real (non-emulated) 480 px window. No evidence of a defect; the Safari leg of the stage's owner check stays open (R6 defers the Safari-specific `prefers-reduced-transparency` work to Step 15.1).

## Stage sign-off — performed

Global Definition of Done, stage item: *CI green with run numbers recorded in act.md, the greps of the stage recorded, `git diff --stat` matching the stage's file lists, and the owner's manual check answered.*
- CI green with run numbers — **met** (#633–#638 for 10.1–10.6, **#639** for the correction; all `completed`/`success`).
- The greps of the stage recorded and re-run by me — **met**.
- `git diff --stat` matching the stage's file lists — **met** (exactly the 19 files).
- The owner's manual check answered — **met** (the browser re-test report answers the pickers' keyboard/Escape checks and the normal path; the Safari leg remains unverified and is carried as LOW, item 4 above). The HIGH defect that blocked the previous verdict is fixed and independently confirmed.

`.agents/TODO.md` — the Stage 10 line was changed to `- [x] ~~Stage 10 — Overlays and pickers: glass modals and menus, scrim, picker keyboard support, Jalali date picker~~` (that line only; no other stage added, rewritten, reordered or unticked) and committed on its own as `docs(todo): complete Stage 10`. `act.md`, `audit.md` and `plan.md` were not committed; nothing was pushed.

**Next:** the Planner continues with Stage 11 (Step 11.1) and adds the carry-forward items above as new steps where they are not already owned.
