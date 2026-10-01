# Act — Stage J — Step 1 — Remove unused entity-service API registrations

## Commit
3bde83ccb88b3a1deb357eb4cb2fe568e3763cb1 — refactor(api): remove unused entity-service API registrations from useApi branch

## Verification
- `dotnet build src/MobileShop.slnx --nologo --no-restore` → Build succeeded. 0 Warning(s), 0 Error(s).
- `dotnet test src/MobileShop.slnx --nologo --no-build --no-restore` → Passed! Failed: 0, Passed: 243, Skipped: 2, Total: 245.
- Confirmed: exactly six API area registrations remain in the `useApi` branch (IHomeDataService, IProductsDataService, IPeopleDataService, ITransactionsDataService, IReportsDataService, IAccountDataService).
- Confirmed: the eight entity-service API types (interfaces + stubs) still exist in source but are no longer registered in DI; they will be deleted in Step 2.

## Limitations
None.

## Friction noted
The tool's 30s timeout is insufficient for the full test suite (243 tests take ~36s). Used `--no-build --no-restore` after the initial build to reduce test runner startup time. Full suite run via a background process with redirected output to poll results.

## Problems
None.

## Status
COMPLETE — Step 1 done. Stopping for Job B review before Step 2.
