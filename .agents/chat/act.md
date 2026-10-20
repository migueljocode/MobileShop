# Actor — correction round (Step 10.7 Job B HIGH finding)

- **Commit:** `19066f0` — `fix(web): discard a stale combobox response after the list is dismissed`
  (body: `Correction round for the Step 10.7 Job B HIGH finding.`; Conventional Commits, no `Co-authored-by`)

## Verification

### CI

- `Action: #639 — Success` (workflow `.NET CI`, event `push`, `status: completed`, `conclusion: success`; read back with `gh api repos/migueljocode/MobileShop/actions/runs?head_sha=19066f0b83df6dde6e3e80bb810febdd8084b815` — exactly one run matched, no other workflow).

### Commands (real output, run from the repo root)

- `node --check src/MobileShop.Web/wwwroot/js/site.js` → OK (no syntax errors)
- `node --check src/MobileShop.Web/wwwroot/js/person-picker.js` → OK (no syntax errors)
- `node --check src/MobileShop.Web/wwwroot/js/product-picker.js` → OK (no syntax errors)
- Step 10.3 definition-site check (`awk` over `site.js`) → `OK: definition at line 37 precedes the ready block at line 205` → **holds** (the helper is still a top-level global in `site.js`, outside the `DOMContentLoaded` block; the ready block only moved from line 198 to 205 because of the added lines above it).
- `grep -c "cancel" src/MobileShop.Web/wwwroot/js/site.js` → `3` (>= 1) — **holds**
- `grep -c "latestSearchId" src/MobileShop.Web/wwwroot/js/person-picker.js` → `4` (>= 3) — **holds**
- `grep -c "latestSearchId" src/MobileShop.Web/wwwroot/js/product-picker.js` → `5` (>= 1) — **holds**
- `git diff --stat` of the commit → `.agents/chat/plan.md` (3 ++-), `wwwroot/js/person-picker.js` (12 ++--), `wwwroot/js/product-picker.js` (6 ++-), `wwwroot/js/site.js` (7 ++++) — the three JS files and the plan note only. No CSS, no views, no other script.

### Exact lines changed

`src/MobileShop.Web/wwwroot/js/site.js` (7 lines added, nothing removed)
- `:44-46` — new optional hook next to the existing ones:
  ```js
  // Optional hook: lets a consumer invalidate a request that is still in flight,
  // so a late response cannot re-open the list the user just dismissed.
  const cancelList = hooks && typeof hooks.cancel === 'function' ? hooks.cancel : () => { };
  ```
- `:125-131` — the Escape branch of the keydown handler:
  ```js
  // Escape dismisses the list, so any search that is still in flight is
  // cancelled as well — even when the list is still hidden, because the
  // pending response would otherwise open it a moment later.
  cancelList();
  if (!isOpen) return;
  closeList();
  event.preventDefault();
  event.stopPropagation();
  ```
  `hooks.open` / `hooks.close` are untouched; `cancel` is an *additional, optional* hook — a consumer that does not pass it gets the no-op `() => { }`, so nothing throws and nothing changes.

`src/MobileShop.Web/wwwroot/js/person-picker.js` (12 ++--)
- `:19` — `var latestSearchId = 0;` (next to `var timer;` / `var isFocused;`).
- `:70-74` — the wiring call now passes the hook:
  ```js
  combobox = window.wireComboboxKeys(searchInput, results, {
      open: openResults,
      close: closeResults,
      cancel: function () { latestSearchId++; }
  });
  ```
- `:101` — `var searchId = ++latestSearchId;` as the first statement of `search(query)` (before the `fetch`).
- `:110-112` — the staleness guard before the render:
  ```js
  var options = await response.json();
  if (searchId !== latestSearchId) return;
  renderResults(options);
  ```
  The `finally { clearSearchLoading(); }` is unchanged, so `aria-busy` and the loading line are still cleared for a discarded response.

`src/MobileShop.Web/wwwroot/js/product-picker.js` (6 ++--)
- `:211-215` — the `wireComboboxKeys(...)` call now passes `cancel: function () { latestSearchId++; }`; the existing staleness check at `:240` (`if (searchId !== latestSearchId || …) return;`) drops the late response. No other change in this file.

`.agents/chat/plan.md` — one line added under the (already struck) `## ~~[x] Step 10.7 — Stage 10 validation~~` heading, noting the correction round. No existing line was changed, no heading was re-struck or rewritten.

### Logical proof that the race is fixed

1. The user presses Escape in the search input. The keydown handler in `wireComboboxKeys` reaches the Escape branch and calls `cancelList()` → the picker's `cancel` hook runs `latestSearchId++`. (The hook is guarded: `hooks && typeof hooks.cancel === 'function'`, so a consumer without the hook gets a no-op and the helper still never throws when the list is empty or hidden.)
2. `closeList()` then adds `d-none` and sets `aria-expanded="false"` — the open/close contract is unchanged.
3. The in-flight response arrives afterwards. In the person picker the awaited payload is now assigned to `options` and the guard `if (searchId !== latestSearchId) return;` is true (the counter was bumped after the request was issued), so `search()` returns **before** `renderResults(options)` — the list is never re-opened and the live region is not re-announced. In the product picker the pre-existing check `if (searchId !== latestSearchId || …) return;` is true for the same reason, so `renderResults()` is not reached.
4. Therefore `aria-expanded` stays `"false"` and `d-none` stays on the list, in both pickers, when the late response lands. The `finally { clearSearchLoading(); }` still removes `aria-busy` and the loading line, so the picker does not get stuck in a "loading" state.

### Deviation from the Reviewer's snippet (deliberate, one line)

The Reviewer's snippet put `cancelList()` **after** the `if (!isOpen) return;` guard. That placement only cancels when the list happens to be open at the moment of the Escape. In the reproduction the Reviewer described — and in the exact re-test the orchestrator asked for ("type two letters, press Escape during the in-flight window") — the list is still `d-none` at the Escape, because no response has rendered yet: the result lists start hidden (`_PersonPicker.cshtml:23`, `_ProductPickerOptions.cshtml:3` both carry `d-none`) and only `renderResults()` opens them. With the guard first, `cancelList()` would never run in that window, the late response would still be current, and the list would still pop open — the HIGH finding would survive the fix.

The cancel call is therefore placed **before** the `!isOpen` guard, still inside the Escape branch and still before `closeList()`, which is what the orchestrator's instruction asks for ("call an OPTIONAL `hooks.cancel()` before `closeList()` in the Escape branch"). Consequences, all intended:
- Escape now always invalidates the in-flight search of that input, whether or not the list is currently visible — which is the point of the finding ("Escape is not honoured while a search request is in flight").
- `closeList()`, `event.preventDefault()` and `event.stopPropagation()` are still reached only when the list was open, so the key handling, the live region, the selection, the price filling and the 150 ms blur timing are untouched.
- `hooks.cancel` is a pure counter bump in both pickers, so pressing Escape with nothing in flight is a no-op in effect (the next search simply gets a higher id).

Everything else in the Reviewer's "Exact fix" is implemented literally: the optional hook in `site.js`, the `cancel` hook in `product-picker.js` (the staleness check there already existed), and the three changes in `person-picker.js` (`latestSearchId`, the captured `searchId`, the guard before `renderResults`, the `cancel` hook).

## Limitations

- **The browser re-test of the race is still OPEN — no browser is available in this environment** (no Firefox/Chromium, no geckodriver/Playwright), so the fix is proven by code inspection and logic only, not by observation. The orchestrator should run exactly this check:
  1. Open `/Transactions/Sell`.
  2. Throttle the network so a search request takes ~2 s (e.g. 2 s latency in the devtools network throttling, or a delayed `window.fetch`).
  3. Focus the customer (person picker) search input, type two letters (e.g. `Ma`).
  4. While the request is still in flight (`aria-busy="true"`), press **Escape**.
  5. Confirm the list stays closed for more than 4 s: `d-none` present, `aria-expanded="false"`, and still closed when the response arrives (no options rendered, no re-announcement in the live region). Repeat the same steps for the product picker search input.
  6. Also confirm the normal path still works: type → options appear → ArrowDown/ArrowUp move the active option → Enter selects (person picker: customer id set; product picker: product id set and the price autofilled) → the list closes and `aria-expanded="false"`.
- The person-picker modal focus-restoration one-liner (Reviewer finding 2, MEDIUM, pre-existing) was **not** included: only the HIGH finding was in scope for this correction round.
- `/Reports/ProfitLoss` reload-on-day-pick (finding 3) and the broken `vazirmatn-*.woff2` files (finding 4) are untouched — out of scope.
- Per the project rules CI is the only build/test gate — no local `dotnet build` / `dotnet test` was run.
- `graphify update .` was run after the change; the `graphify-out/` files (`graph.json`, `GRAPH_REPORT.md`) are dirty in the working tree and were deliberately **not** committed.

## Friction noted

- The Reviewer's exact fix was internally inconsistent with its own acceptance criterion: the snippet's `cancelList()` sits behind the `if (!isOpen) return;` guard, while the acceptance criterion ("Escape pressed during the in-flight window must leave the list closed … and the list must still be closed when the response arrives") is only satisfiable when the cancel also runs while the list is still hidden. The one-line placement deviation above is the resolution; a re-read of the finding's own reproduction (`d-none` present at the Escape) confirms it.
- The `git diff --stat` of the stage is large because `graphify-out/graph.json` is rewritten whole on every build; committing it is not wanted, but its dirtiness obscures the real scope of a change. A `git diff --stat -- . ':(exclude)graphify-out'` filter would read better.

## Problems

- None blocking. The one deliberate deviation from the Reviewer's literal snippet is documented above and in the plan note, with the reason.

## Status

COMPLETE — the Step 10.7 Job B HIGH finding is fixed in the three JS files only, `Action: #639 — Success` is green, all required checks hold, and the commit is pushed to `main`. The browser re-test of the race remains open for the orchestrator (steps above). **Next: the Reviewer re-runs Job B on the correction commit `19066f0` and, if it passes, signs off Stage 10.**
