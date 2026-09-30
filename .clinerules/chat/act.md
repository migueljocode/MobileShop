# Act Report — Stage F Step 3 (Stage F validation)

- Commit: `4ee6c2d` — docs(reports): complete Stage F validation
- Verification — Chain 1 (build + full suite, one chained call):
  - `dotnet build src/MobileShop.slnx --nologo && dotnet test src/MobileShop.slnx --nologo --no-build` -> **Build succeeded, 0 errors, 1 warning**; **Failed: 0, Passed: 538, Skipped: 2, Total: 540** (EXIT=0).
  - The single warning is the **pre-existing** `CS9124` at `src/MobileShop.Services/DataServices/Dal/ProductsDataService.cs(26,62)` — untouched by this stage, identical to the Stage E and Stage F Step 1/2 baselines; deliberately not fixed.
- Verification — Chain 2 (Production host + smoke):
  - Host started with `ASPNETCORE_ENVIRONMENT=Production dotnet run --project src/MobileShop.Web --no-launch-profile --urls http://localhost:5199`; log confirms `Hosting environment: Production` and `Now listening on: http://localhost:5199`. Shut down afterwards (port 5199 released; post-shutdown probe refused, curl exit 7).
  - **Target route 200**: `/Reports/ProfitLoss` -> **200**.
  - **All five automatic presets 200** (the plan's "with query presets if easy"): `Mode=Automatic&Preset=Today|Week|Month|Year|All` -> **200** each.
  - **Data-path proof**: the `Preset=All` response body renders the distribution section with both required employees (`Mikaeeil Jorjany`, `Anis Sahabi`) and the fixed shares `>40%`, `>50%`, `>10%`, plus the `Total profit:` line. This exercises the whole migrated chain — `GetEarliestTransactionDateAsync` -> `GetProfitLossRowsAsync`/`GetProfitLossTotalAsync` -> `GetDistributionRowsAsync` — through `IReportsDataService`, and confirms the seeded employee lookup still satisfies the fixed 40/50/10 rules (plan HIGH).
  - **Regression routes 200**: `/`, `/Products`, `/People/Customers`, `/People/Sellers`, `/Transactions`, `/Transactions/Buy`, `/Transactions/Sell`, `/Account/Login` — **8/8**.
  - **Non-destructiveness**: Production log has **0** matches for `InitializeForDevelopment|EnsureDeleted|EnsureCreated`, **0** for `seed`, **0** for `ERR |FTL |Exception`. Row-count fingerprint `Products|Phones|Transactions|Employees` = `17|7|26|6` before **and** after the pass (read-only `mode=ro`); Employees added to the fingerprint this stage because distribution now depends on that table.
  - **DoD spot-checks**: `ProfitLossModel` ctor is `(IReportsDataService dataService)` only; a word-boundary grep for `ITransactionDataService|IEmployeeDataService|IOptions<|DistributionCalculator|Repo<` on `ProfitLoss.cshtml.cs` returns **no match** (the earlier loose grep matched only the substring "Reports" in the namespace/`IReportsDataService`); entity `ITransactionDataService`, `IInvoiceDataService`, and `IEmployeeDataService` registrations **all still present** in the Dal branch (L3), and the profit/loss methods remain on the entity `TransactionDataService` for Stage H as planned.
- Files changed: **none in `src/`** — this step only ticks plan.md and writes act.md.
- Limitations: None. Stage F Global Definition of Done is satisfied. Stage F was **not** ticked in `to-do.md` — per the rules only the reviewer ticks stages, after this sign-off.
- Friction noted: build (~21 s) and the full suite (~35 s) exceed the ~30 s command window, so Chain 1 was launched detached via `setsid` and the log polled until the summary plus `EXIT=` line appeared. One self-inflicted false positive: a loose grep pattern reported "matches" on `ProfitLoss.cshtml.cs` that were really the substring `Reports` inside the namespace and `IReportsDataService`; re-ran with word boundaries (`\b...\b`) to get a trustworthy answer rather than accepting the misleading output.
- Problems: None.
- Status: COMPLETE
