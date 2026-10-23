## Actor report — Stage 20, Step 3 — Fill SpecKey and SpecLabel in the data service

- Commit: `0e3fe0e` feat(services) — `TransactionsDataService`: `LoadInvoiceGraph` now uses a shared `IncludeProductGraph` helper; new `WithSpecsAsync` loads product graphs for all factor rows in one query and sets `SpecKey`/`SpecLabel` with `with`; applied to both `GetTransactionFactorPdfAsync` and `GenerateListFactorPdfAsync`. Helper adds `DeviceSpecProfile` (+ storage), and `ModelFits` on glass and case.
- Tests: `TransactionsDataServiceTests`: fake PDF generator records the last factor model; three new tests (identical products merge into one line of quantity 2; different colours stay separate; single factor row carries the spec key).
- Deviation: the invoice graph now also loads the extra spec includes (more joins, same output). Invoice output not changed in code; covered by existing invoice tests in CI.
- CI: run #701 Success on `0e3fe0e`.
