# Audit — Job B (Execution Check): Stage U Step 4 correction (`35949ab`, PR #14)

**Verdict: FAIL (agreed with the earlier Step 4 audit). The actor can fix this in one small commit. Do not start Step 5.**
Checked independently on `main` at `97481fe`: the evidence below is from my own scan of the repository; CI results are the actor's report (Actions #405 on the PR and #406 on the merge, both recorded green).

## Findings
- **HIGH — 9 in-scope files still lack a final newline** (my scan matches the earlier audit exactly), all under `src/MobileShop.Web/wwwroot/lib/bootstrap/dist/`:
  - `css/bootstrap-grid.rtl.css`, `css/bootstrap-grid.rtl.min.css`
  - `css/bootstrap-reboot.rtl.css`, `css/bootstrap-reboot.rtl.min.css`
  - `css/bootstrap-utilities.rtl.css`, `css/bootstrap-utilities.rtl.min.css`
  - `css/bootstrap.rtl.css`, `css/bootstrap.rtl.min.css`
  - `js/bootstrap.esm.min.js`
  The correction touched 54 files but 9 of them were out of scope (`*.css.map` and `jquery/LICENSE.txt`, not in the planned extensions), so `act.md`'s claim of "54 remaining in-scope files" is inaccurate. All first-party files, `.editorconfig` and the rest are correct.
- **MEDIUM — carried into Step 5 (already appended to `plan.md`):** `.github/copilot-instructions.md` line 51 says feature files carry no `using` directives, but about 20 test files and three production files (`MoneyExtensions.cs`, `ProductsDataService.cs`, `QuestPdfGenerator.cs`) keep file-local usings. Step 5 fixes the sentence only; no more promotions.
- **LOW:** my own Step 4 plan did not exclude vendored libraries, which is why vendor files were in scope; the 9 files are still the minimal way to meet the plan's "no in-scope file lacks a final newline".

## What the actor must do (one commit)
Read this section as the instruction; do not wait for a separate prompt.
1. `git pull`.
2. Append one `\n` to exactly the 9 files listed above and change nothing else. Do not revert the already-touched `.map` files or `jquery/LICENSE.txt`.
3. Verify mechanically before pushing: `git diff --numstat` shows `1 1` for each of the 9 files, and re-run the scan "tracked files with extensions cs|cshtml|js|css|sh|ps1|yml|yaml|md|json|csproj|props|slnx, excluding `Migrations/` and `.clinerules/chat`, that lack a final newline" — it must print nothing.
4. Push on a branch/PR, **wait for a green Action before merging**, then record `Action: #<run_number> — <Success|Failure|Pending>` (PR and merge) and the scan output in `act.md`, checking every claim against `git show <hash> --stat`. Then STOP for Job B. Do not start Step 5 and do not touch `to-do.md`, `plan.md` or `audit.md`.

## Gate
Step 5 starts only after the 9 files end with a newline and the run is green.
