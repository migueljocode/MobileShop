# Plan — Stage F: Reports — ReportsDataService + profit/loss

Stage E is signed off. This file plans **Stage F only** (next unchecked stage in `to-do.md`).

## Process (binding)
- **One step → commit → Job B review → next step.**
- Only the **reviewer** ticks Stage F in `to-do.md` after the final step PASSes.

## Locked decisions
L1 Pages inject area interfaces only. No EF entities on the page path.
L2 One area service per page, local name `dataService`. After migration, ProfitLoss injects **only** `IReportsDataService` — not `ITransactionDataService`, not `IEmployeeDataService`, not `IOptions<DistributionSettings>`.
L3 DI — **this stage removes nothing.** Keep entity `ITransactionDataService`, `IEmployeeDataService`, and all other entity services (Home cards still use transaction service; Stage H deletes leftovers).
L4 Profit/loss **ownership moves here** (Stage E L5 is fulfilled by this stage). Port the projections from entity `TransactionDataService`; leave those methods on the entity interface until Stage H (callers may still compile against them).
L5 Date-range **UI** (`DateRangeMode`, `AutomaticPreset`, `ResolveBoundsAsync`, `From`/`To` bind props) **stays on the page**. The service only answers data questions for already-resolved bounds.
L6 Logging — `ILogger<ReportsDataService>`; optional info on empty range only if useful; no noise.
L7 Api — `ApiReportsDataService` already implements the interface with NIE. No signature change expected; if any is required, update Api in the same step. Never touch `src/MobileShop.Api`.
L8 No auth, schema, migrations, DatabaseInitializer policy, bin/obj.
L9 Tests — `RepoTestBase` + real `ReportsDataService`; port/extend coverage for P/L rows, total, earliest date, distribution (seed must include active Mikaeeil/Anis employees as today).
L10 **Full interface in Step 1** — implement every `IReportsDataService` member on Dal (no NIE).

## Gap (must fix in Step 1)
`IReportsDataService` is registered under the **Api** branch only. The **Dal** branch currently has **no** `IReportsDataService` → `ReportsDataService` line. Add it in Step 1 or Production DI will fail once the page depends on it.

## Scope
Single page: `Pages/Reports/ProfitLoss`.

## Current page behavior (verified)
Ctor today: `(ITransactionDataService, IEmployeeDataService, IOptions<DistributionSettings>)`.

`OnGetAsync`:
1. `ResolveBoundsAsync` — Automatic presets (Today/Week/Month/Year/All using earliest) or Manual (`From`/`To` with defaults); may set `EmptyDatabaseNote`.
2. `Rows = GetProfitLossRowsAsync(EffectiveFrom, EffectiveTo)`.
3. `TotalProfit = GetProfitLossTotalAsync(EffectiveFrom, EffectiveTo)`.
4. Active employees + `DistributionCalculator.Calculate(TotalProfit, employees, settings)` → `DistributionRows`.

Preserve all of the above semantics after migration; only the data sources change.

## Interface (already present — implement as-is)
```csharp
Task<IReadOnlyList<ProfitLossRowViewModel>> GetProfitLossRowsAsync(DateTime? from, DateTime? to);
Task<decimal> GetProfitLossTotalAsync(DateTime? from, DateTime? to);
Task<DateTime?> GetEarliestTransactionDateAsync();
Task<IReadOnlyList<DistributionRow>> GetDistributionRowsAsync(decimal totalProfit);
```

### Implementation notes
- **P/L rows/total** — port `TransactionDataService.GetProfitLossRowsAsync` / `GetProfitLossTotalAsync` (group by product, buy/sell sums, date filter inclusive on `.Date`). Prefer `IBaseRepo<Transaction>` + `SelectAllAsync` projections (same shape as entity service).
- **Earliest date** — port `TransactionRepo.GetEarliestTransactionDateAsync` semantics (non-deleted, order by date ascending, date-only). Use `IBaseRepo<Transaction>` ordered select **or** `ITransactionRepo` if the specialized method is the cleanest path; document choice in act.md. Soft-delete filters must still apply.
- **Distribution** — load active employees with `PersonNavigation` (required by `DistributionCalculator`), then `DistributionCalculator.Calculate(totalProfit, employees, settings.Value)`. Inject `IOptions<DistributionSettings>` **into the service**, not the page. For employees: prefer `IBaseRepo<Employee>` if Include/person names work via Select; otherwise `IEmployeeRepo.FindAllActiveAsync()` is acceptable until Stage H (same Include graph as today). **Do not** inject `IEmployeeDataService` into the area service if a repo path works.

## Reviewer Briefing
- **HIGH:** Dal DI registration missing today — must add in Step 1 before page migration.
- **HIGH:** Distribution still requires named active employees; do not change calculator rules.
- **MEDIUM:** Date-range resolution stays on the page (L5).
- **Do not** delete entity transaction/employee DI (L3).

## ~~[x] Step 1 — `ReportsDataService` + Dal registration~~
- Files: create `src/MobileShop.Services/DataServices/Dal/ReportsDataService.cs`, `src/MobileShop.Tests/Services/DataServices/Dal/ReportsDataServiceTests.cs`; modify `ServiceCollectionExtensions` (Dal branch only: `services.AddScoped<IReportsDataService, ReportsDataService>();`).
- Ctor: `(IBaseRepo<Transaction> transactions, …employees…, IOptions<DistributionSettings> distributionSettings, ILogger<ReportsDataService> logger)`.
- Implement all four interface members (L10).
- Tests: rows/total for a known range; empty range; earliest null vs seeded; distribution three rows 40/50/10 when profit > 0 (use seeded names).
- Verify: `dotnet build src/MobileShop.slnx --nologo && dotnet test src/MobileShop.slnx --nologo --no-build`
- Done when: Dal registers area service; **no page changes**.
- Risk: MEDIUM. Confidence: HIGH.

## ~~[x] Step 2 — Migrate `ProfitLoss` page + tests~~
- Files: `Pages/Reports/ProfitLoss.cshtml.cs`; any `Tests/Web/Pages/Reports/*`.
- Change:
  - Ctor: `(IReportsDataService dataService)` only.
  - Keep `ResolveBoundsAsync` / enums / bind properties on the page.
  - Replace transaction/employee/settings calls with `dataService.GetEarliestTransactionDateAsync`, `GetProfitLossRowsAsync`, `GetProfitLossTotalAsync`, `GetDistributionRowsAsync(TotalProfit)`.
- Do not edit `.cshtml` unless forced (stop and report).
- Verify: build + full suite.
- Risk: LOW. Confidence: HIGH.

## ~~[x] Step 3 — Stage F validation~~
- Chain 1: build + full suite.
- Chain 2: Production host; **200** `/Reports/ProfitLoss` (and with query presets if easy); regression Home/Products/People/Transactions/Account.
- Non-destructiveness: no init wipe; optional row-count fingerprint stable.
- Done when: page uses only `dataService`; entity DI still present; suite + smoke green.
- Risk: LOW. Confidence: HIGH.

## Global Definition of Done
- `ReportsDataService` implements full `IReportsDataService` and is Dal-registered.
- ProfitLoss depends on a single `dataService`.
- Date-range UI remains on the page; P/L + distribution data come from the area service.
- No entity service/repo registration removed.
- Build + suite + Production smoke green.

## Execution notes
- One step per commit; Conventional Commits; no Co-authored-by.
- Stop after each step for Job B.
- OUT OF SCOPE: Account pages, deleting entity services, Home dashboard migration, changing DistributionCalculator fixed shares.
