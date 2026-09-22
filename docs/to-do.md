<details open>
<summary>✅ Step 3 — Document how the AppLogging section is bound (2026-09-22)</summary>

- Files: `src/MobileShop.Services/Logging/Settings/AppLoggingSettings.cs`, `src/MobileShop.Services/Logging/Configuration/LoggingsConfiguration.cs`
- Commit message: `docs(services): document how the AppLogging section is bound`
- Build: 0 errors / 0 warnings. Tests: 271 passed, 0 failed.
- Checks: XML-comment-only diff; `dotnet build src/MobileShop.slnx --no-incremental` → Build succeeded.
- Deviations: none

</details>

<details open>
<summary>✅ Refactoring QuestPDF composition callbacks for single responsibility</summary>

- Files: `src/MobileShop.Services/PDF/QuestPdfGenerator.cs`, `src/MobileShop.Services/GlobalUsings.cs`, `src/MobileShop.Web/Extensions/WebApplicationBuilderExtensions.cs`
- Work: Normalize `Create`, `Page`, `Header().Column`, `Content().Column`, and `Footer().Text` through named delegate variables; retain the static `Create` call through the global using; remove the temporary PDF probe endpoint.
- Build: 0 errors / 0 warnings. Tests: 271 passed, 0 failed.
- Checks: `dotnet build src/MobileShop.slnx --no-incremental` → Build succeeded; `dotnet test src/MobileShop.slnx --nologo` → 271 passed, 0 failed; no `pdf-smoke` references remain; `docs/to-do.txt` was renamed to `docs/to-do.md`.
- Deviations: none

</details>
