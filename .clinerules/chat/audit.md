# Audit — Job A (Plan Review): Stage V — Products list: Available / Sold filter

**Verdict: APPROVED. Step 1 is authorized.**
Reviewed `plan.md` (3 steps) against `main` at `eb33206`; claims verified by reading the code (no CI needed for a plan review).

## Verified against the repository
- **Existing rule:** `ProductsDataService.GetInventoryRowsAsync` builds `IsSold` as `Transactions.Any(t => t.Direction == TransactionDirection.Sell)` for phones (line 54), Apple IDs and glass; `Index.cshtml` already renders the Sold/Available badge from it, so filtering on the same `IsSold` keeps filter and badge consistent. No new column or migration is needed.
- **Signature:** `GetInventoryRowsAsync(string? type = null, int? partNumberId = null)` exists on the interface (line 12), the service (line 35) and the Api stub; an optional third parameter keeps every existing caller and test valid.
- **Page today:** `IndexModel.OnGetAsync(type, partNumberId)` with a part-number form shown only for phones, using `onchange="this.form.submit()"` (the precedent the plan reuses); type links currently carry `partNumberId` only for phones.
- **Tests:** there are no Products `IndexModel` tests (only the Create* page tests), so Step 2 creating `IndexModelTests` is right; `TestDataHelpers` has no transaction helper, so the plan correctly points the actor to the `TransactionsDataServiceTests` seeding pattern.
- **Smoke:** the route loops are at `production-smoke.sh:144` and the IRR loop below it; the seed has 17 products and 12 with a Sell transaction, so both lists are non-empty in the smoke. The plan asserts badge markup, not plain words, because the filter's own options contain "Sold" and "Available".
- **Usings:** the Web project already globally imports what the page needs; no new using is planned.

## Review notes
- The plan avoids the main risks seen in earlier stages: no new global usings, an optional parameter instead of a breaking signature, markup-level smoke assertions, and "wait for a green run before merging".
- **LOW (no action):** a product sold and later bought back stays Sold; this is existing behaviour and the plan records it as unchanged (D1).

## MEDIUM/LOW to append
None.

## Gate
Next: the actor does **Stage V Step 1** (availability filter in the service). One step → one commit → wait for a green run before merging → report `Action: #<run_number>` → STOP for Job B. Steps 2–3 are authorized one at a time after each PASS.
