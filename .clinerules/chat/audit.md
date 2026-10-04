# Audit — Job A (Plan Review): Stage U — Cleanup sweep

**Verdict: APPROVED. Step 1 is authorized.**
Reviewed `plan.md` (5 steps) against `main` at `2372b2c`; claims verified by reading the code (no CI needed for a plan review).

## Verified against the repository
- **Phone filter:** `ProductsDataService.cs:403` checks `CategoryNavigation.Name == "Phone"`; `GetModelsAsync` (line 238) lists every model of a manufacturer. Callers are `CreatePhone` and `CreateGlass` only (plus the Api stub and interface), so the optional `categoryName = "Phone"` parameter keeps both unchanged. Page tests seed Phone-category models in `CreatePhoneModelTests`; `CreateGlassModelTests.SeedModel()` must be checked (the plan says so).
- **Stale text:** `.github/copilot-instructions.md` still says `MobileShop.Dal/Repos` (line 49) and `IUserDataService.EnsureAdminUser()` (line 67; the real owner is `IAccountDataService`); the ERD files still show `int FinishedPrice`/`int Price`; the README still contains `&amp;`; `ProfitLoss.cshtml:103` lacks the unit and lines 103/110 use `ToString("N0")`.
- **Usings:** file-local `using` directives exist in `DatabaseMigrator.cs`, `Pages/Products/Index.cshtml.cs` and about ten test files; `global using MobileShop.Web;` and `global using MobileShop.Tests.Dal;` import namespaces with no declared types.
- **Whitespace:** 99 of 318 tracked text files lack a final newline; none contain CRLF; there is no `.editorconfig`; the over-indentation scan found nothing to fix (the earlier logger-summary line is gone).
- **Skips:** no `[Fact(Skip=…)]` remains in the tests.

## Review notes
- The plan is scoped to evidence found in the repo; it does not promise unprovable dead-code removal (D2 is explicit).
- Step 3's fallback for `CS0104`-type conflicts and Step 4's "differs only by the final newline" check keep the two riskier steps recoverable.
- **LOW (no action needed):** Step 4 will add a trailing newline to Razor files; this adds at most whitespace after the last tag and cannot change behaviour.

## MEDIUM/LOW to append
None.

## Gate
Next: the actor does **Stage U Step 1** (phone-category consistency in the model dropdown). One step → one commit → report `Action: #<run_number>` → STOP for Job B. Steps 2–5 are authorized one at a time after each PASS.
