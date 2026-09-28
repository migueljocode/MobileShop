# Act Report — Task 3 (plan round 4), Step 2

## Commit
- Step 1 already landed as `cf34904` — fix(reports): render valid ISO date values for From/To inputs
- This step: plan.md Step 2 tick + this report; **no source file changed**

## Verification (run against the real served page — not a build log)
- `dotnet build src/MobileShop.slnx --nologo` -> Build succeeded, 0 Warning(s), 0 Error(s)
- Production run (`ASPNETCORE_ENVIRONMENT=Production`, `--no-launch-profile`, `--urls http://localhost:5201`), HTML captured with `curl`:
  - `?Mode=Manual` -> `<input type="date" id="From" name="From" class="form-control" value="2026-01-05" />`
    and `<input type="date" id="To" name="To" class="form-control" value="2026-09-28" />`
  - `grep -c 'value="yyyy-MM-dd"'` -> `0`  (Defect 1 gone)
  - `?Mode=Manual&From=&To=` -> **identical** values `2026-01-05` / `2026-09-28` (Defect 2 gone: the model value now
    beats the empty query string, where `asp-for` previously let `ModelState.AttemptedValue` win)
  - `<form` count -> `1`
- Full suite, `--no-build`, namespace chunks run to completion:
  - `~MobileShop.Tests.Web` -> 52 passed
  - `~MobileShop.Tests.Dal` -> 284 passed
  - `~MobileShop.Tests.Services` -> 51 passed
  - `~Models|~Pdf|~Logging|~Security` -> 20 passed, 2 skipped (pre-existing)
  - **407 passed, 0 failed, 2 skipped**
- No lingering process and no listener left on 5199/5201/5043 after the captures.

## Limitations
- **Browser-locale glyphs.** The picker displays `01/05/2026`-style text, not `2026-01-05`. A native
  `<input type="date">` always renders its value in the browser/OS locale; only the emitted and submitted
  value can be `yyyy-MM-dd`. Visual confirmation in a browser remains the owner's call.
- Defect 1's mechanism is evidenced by the served-HTML capture, not by reading
  `DefaultHtmlGenerator.GenerateInput`: that file truncated mid-method on every retrieval attempt, so the
  composite-format inference is still unread at source level. The captured HTML is the decisive evidence.

## Friction noted
- The whole-solution `dotnet test` still exceeds the ~30s shell window; namespace chunking is still required,
  and the chunks must run **serially** (running the Services chunk in parallel with Dal also timed out).
- Fetching large ASP.NET Core source files truncates in the middle, which is exactly where the method of
  interest lives — the root-cause work had to lean on API docs and issue trackers instead.

## Problems
- **The dev database was dropped and recreated at 20:43:23** by a Development run (log line
  `Hosting environment: Development` at 20:39:08) that started *after* the safe Production captures.
  This is the documented, intentional `InitializeForDevelopment` behaviour, not an accident to fix.
  **Verified no data loss** by full logical dump diff (`sqlite3 ... .dump`): only 4 diff lines, entirely the
  dev-only admin password hash in `Users`, which `EnsureAdminUser()` regenerates with a fresh Argon2 salt.
  Every other row in every table is identical.
- Pre-run copies remain at `/tmp/MobileShop.db.bak` (20:29), `.bak2`, `.bak3`, `.bak4` (20:29-20:35)
  should the owner want the earlier file.

## Status
COMPLETE
