# Act — Stage K — Step 2 — Register Vazirmatn for QuestPDF

## Summary
Step 2 registers the bundled Vazirmatn TTF fonts with QuestPDF's `FontManager`. The `RegisterFonts()` method was already present in the working tree (added alongside the Step 1 font bundling) but was left uncommitted. This step commits the font registration and wires it up in all required startup paths.

## Inspection findings
- `QuestPdfSetup.cs` already contains `RegisterFonts()` (lines 23–28) that calls `QuestPDF.Drawing.FontManager.RegisterFontsFromDirectory` on the bundled TTF directory.
- `RegisterFonts()` is already called from:
  - `src/MobileShop.Tests/ModuleInitializer.cs` (direct unit tests — for the PDF generator tests)
  - `src/MobileShop.Web/Extensions/WebApplicationBuilderExtensions.cs` (Web host startup, called before `AddMobileShop()`)
- The font files are valid TTF with `Vazirmatn` family name (matching `FontFamily("Vazirmatn")` calls in `QuestPdfGenerator`).
- `UseCommunityLicense()` is called alongside `RegisterFonts()` in both test and Web startup paths.
- `ServiceCollectionExtensions.cs` calls `UseCommunityLicense()` but does NOT call `RegisterFonts()` directly — however the Web host path calls `RegisterFonts()` in `WebApplicationBuilderExtensions` before `AddMobileShop()`, and tests call it via `ModuleInitializer`.

## Verification
- Build: `dotnet build src/MobileShop.slnx --nologo --no-restore` → Build succeeded. 0 Warning(s), 0 Error(s).
- Tests: `dotnet test src/MobileShop.slnx --no-build --no-restore --nologo --filter "FullyQualifiedName~QuestPdfGeneratorTests"` → Passed! Failed: 0, Passed: 4, Skipped: 2, Total: 6.
- Note: The 2 Persian tests remain SKIPPED — they will be unskipped and validated in Step 3.

## Changed files
- Modified: `src/MobileShop.Services/PDF/Configuration/QuestPdfSetup.cs` — added `RegisterFonts()` method
- Modified: `src/MobileShop.Tests/ModuleInitializer.cs` — added `QuestPdfSetup.RegisterFonts()` call
- Modified: `src/MobileShop.Web/Extensions/WebApplicationBuilderExtensions.cs` — added `QuestPdfSetup.RegisterFonts()` call
- Modified: `.clinerules/chat/plan.md` — marked Step 2 checkbox as done

## Commit
`f8b8e69` — feat(pdf): register Vazirmatn fonts with QuestPDF FontManager for Persian rendering

## Limitations
None.

## Friction noted
None.

## Problems
None.

## Status
COMPLETE — Step 2 (font registration) is implemented and verified. The RegisterFonts() method exists and is wired up in all required paths. Stopping for Job B review before proceeding to Step 3 (unskip tests).
