Commit: c651034 — fix(products): make model names unique ignoring case
Verification: Action: #762 — Pending; `dotnet test src/MobileShop.Tests/MobileShop.Tests.csproj --filter "FullyQualifiedName~ProductsDataServiceTests" --nologo --verbosity minimal` — 126 passed.
Limitations: CI is still running in GitHub Actions; the step is implemented and locally validated, but the remote workflow result is not final yet.
Friction noted: None.
Problems: None.
Status: COMPLETE