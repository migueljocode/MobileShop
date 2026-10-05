# Plan — Stage V — Products list: Available / Sold filter

## Requirements
- Functional
  - The Products list gets an **Availability** filter with three choices: All (default, today's behaviour), Available, Sold.
  - It applies live: changing the choice reloads the list immediately (same `onchange="this.form.submit()"` pattern as the existing part-number filter).
  - It works together with the existing type buttons (All / Phones / Apple IDs / Glasses) and the part-number filter; switching type keeps the chosen availability.
  - The filter and the existing status badge can never disagree: both use the same rule.
- Non-functional: no schema change, no migration, no new column; CI evidence for every step; no change to the Api host, authentication, entities or Development initialization.
- Constraints (project-specific-rules.md): API untouched, no auth changes, Apple ID inventory passwords stay plaintext, EF configuration centralized, project-wide usings live in `GlobalUsings.cs`, no `bin`/`obj` changes.

## Decisions (labelled)
- **D1 (single source of truth):** availability reuses the rule behind the existing badge: `ProductListItemViewModel.IsSold` is `Transactions.Any(t => t.Direction == TransactionDirection.Sell)` (soft-deleted transactions are already hidden by the global query filter). The filter is applied on `IsSold` after the rows are built, so badge and filter cannot diverge. Existing behaviour that stays unchanged: a product sold and later bought back still shows as Sold.
- **D2:** values are lowercase strings `"available"` and `"sold"` (same style as the `type` values `"phone"`, `"appleid"`, `"glass"`); `null`, `"all"` and anything unrecognised mean no filter.
- **D3:** the default stays All, and the Second-hand list page (`Products/SecondHand`) is out of scope.
- **D4:** no new column, entity, migration or JavaScript file.
- **A1:** GitHub Actions is the build/test gate. The actor does not run `dotnet` locally; each step is verified by the pushed commit's run, reported as `Action: #<run_number> — <Success|Failure|Pending>`; wait for a green run before merging.
- **A2 (lessons from earlier stages):** check a project's `GlobalUsings.cs` before using a type; compile errors only show in CI; compare each report with `git show <hash> --stat` before writing it. Razor output is not unit-testable here, so Step 2 adds smoke assertions on the rendered badge markup.

## Reviewer Briefing
- **Step 1 is LOW risk / HIGH confidence:** one defaulted parameter threaded through interface, implementation and Api stub, plus a post-filter.
- **Step 2 is MEDIUM / MEDIUM:** Razor markup and the smoke assertions can only be proven by CI. Check that the smoke script asserts both directions (the sold list contains the Sold badge and no Available badge, and vice versa), because the filter's own `<option>` text contains the words "Sold" and "Available".
- The seeded data has 17 products, 12 with a Sell transaction, so both lists are non-empty in the Production smoke.
- No `IndexModel` page tests exist for Products today; Step 2 creates the first one.

## ~~[x] Step 1 — Availability filter in the service~~
- **Done** — Job B PASS (PRs #18/#19, `853014e`; Action #427). `GetInventoryRowsAsync(type, partNumberId, availability)` on interface, service and Api stub; filter applied once on `IsSold` after the blocks; six planned tests.

## ~~[x] Step 2 — Products page: live Availability filter, route carry-over, tests and smoke~~
- **Done** — Job B PASS with one MEDIUM carried to Step 3 (`88d8ee2`; Action #432). Always-rendered GET form with the Availability select, `AvailabilityRoute` on the type links, five `IndexModelTests`, three smoke assertions on badge markup.

## [ ] Step 3 — Smoke-script repair and final Stage V validation
- Files: modify `.github/scripts/production-smoke.sh`; no other file.
- **Repair (MEDIUM, carried from the Step 2 Job B — do this first):** the Step 2 commit (`88d8ee2`) changed lines it was not supposed to touch.
  1. Four `printf` format strings were rewritten from `\n` to `\\n` (the local-run guard message near line 11, the backup-count error listing near line 131, and the `Backup:` listing in the artifact summary near line 182). In bash a single-quoted `\\n` prints a literal backslash-n, so those listings no longer break lines. Restore them exactly as they were in `8415a3f` (`printf "  %s\n" …` and `printf '%s\n' …`).
  2. The file mode changed from `100755` to `100644` (executable bit lost). Restore `100755`; `git ls-files -s .github/scripts/production-smoke.sh` must show `100755`. If the tool used to write files cannot set the mode, say so in `act.md` instead of guessing.
  3. After the repair, `git diff 8415a3f..HEAD -- .github/scripts/production-smoke.sh` must show only the added availability assertion block (the `SOLD_PRODUCTS` / `AVAILABLE_PRODUCTS` / phone+sold lines) and nothing else. When editing shell scripts through the GitHub API, check backslashes and the file mode in the resulting diff before reporting.
- Verify (record all evidence in `act.md`)
  - the final workflow run is green (build with 0 warnings, full suite with 0 skipped, Bash and PowerShell log tests, Production smoke with the availability assertions); run number recorded; wait for it to be green before merging;
  - `git diff --stat <stage-start>..HEAD` shows no change under `src/MobileShop.Api`, authentication, entities, migrations or initialization code, and the Second-hand and Transactions pages are unchanged;
  - every statement in `act.md` (Limitations, Problems) is checked against `git show <hash> --stat` and the actual diff.
- Done when: the script repair is verified by the diff and the mode, the final run is green and recorded, and the reviewer signs Stage V off (only the reviewer ticks `to-do.md`).
- Risk: LOW
- Confidence: HIGH

## Global Definition of Done
- The Products list can be filtered All / Available / Sold, live, together with type and part number; the badge and the filter use the same rule.
- No schema, entity, migration, Api, auth or Development-initialization change; clean build with 0 warnings, 0 skipped tests, whole suite and Production smoke green.

## Carry-over to later stages
- **Stage AA / AB:** new product types extend `GetInventoryRowsAsync` blocks; because the filter runs on the finished `rows`, they inherit the Availability filter as long as their rows carry `IsSold`.
- **Stage W:** Transactions sorting is independent of this stage.

## Execution notes
Work compact: chain dependent commands with `&&`, group read-only checks in one `{ ...; }` call, keep reports short, do not restate this plan in chat. One step → one commit → wait for a green run before merging → STOP for Job B. Do not run `dotnet build`/`dotnet test` locally; push and report `Action: #<run_number> — <Success|Failure|Pending>` per step. Never touch `to-do.md`, `plan.md` or `audit.md`; never amend; check each report against `git show <hash> --stat` before recording it.
