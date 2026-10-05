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

## ~~[x] Step 3 — Smoke-script repair and final Stage V validation~~
- **Done** — Job B PASS (`35545bd`; Actions #436/#437). `production-smoke.sh` mode `100755`; diff vs `8415a3f` only the availability assertion block; Stage V signed off.

## Global Definition of Done
- The Products list can be filtered All / Available / Sold, live, together with type and part number; the badge and the filter use the same rule.
- No schema, entity, migration, Api, auth or Development-initialization change; clean build with 0 warnings, 0 skipped tests, whole suite and Production smoke green.

## Carry-over to later stages
- **Stage AA / AB:** new product types extend `GetInventoryRowsAsync` blocks; because the filter runs on the finished `rows`, they inherit the Availability filter as long as their rows carry `IsSold`.
- **Stage W:** Transactions sorting is independent of this stage.

## Execution notes
Stage V complete. Next: planner writes Stage W (Transactions list sorting); actor waits for Job A APPROVED.
