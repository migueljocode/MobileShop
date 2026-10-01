# Act — Stage J & Stage K — Step 1

## Commit
- **Stage J Step 1**: `3bde83c` — refactor(api): remove unused entity-service API registrations from useApi branch
- **Stage J Step 2**: `ce66d01` — refactor(api): remove unused entity-service API stubs and interfaces
- **Stage K Step 1**: `ad16245` — feat(pdf): bundle Vazirmatn TTF font (v33.003) for QuestPDF Persian rendering

## Verification

**Stage J Step 1 (registration removal):**
- `dotnet build src/MobileShop.slnx --nologo --no-restore` → Build succeeded. 0 Warning(s), 0 Error(s).
- `dotnet test src/MobileShop.slnx --nologo --no-build --no-restore` → Passed! Failed: 0, Passed: 243, Skipped: 2, Total: 245.
- Confirmed: exactly six API area registrations remain in the useApi branch.

**Stage J Step 2 (stub/interface deletion + GlobalUsings fix):**
- Initial build failed: CS0234 errors from GlobalUsings.cs referencing Interfaces.Base and Api.Base namespaces.
- Fix: removed two stale global using entries, keeping MobileShop.Models.Entities.Base intact.
- Build succeeded. 0 errors, 0 warnings.
- Full test suite: Passed! Failed: 0, Passed: 243, Skipped: 2, Total: 245.
- 18 files deleted: 8 API stubs, 8 interfaces, IDataService<T>, ApiDataServiceBase<T>.

**Stage K Step 1 (Vazirmatn TTF bundling):**
- Downloaded Vazirmatn-Regular.ttf (122,752 bytes) and Vazirmatn-Bold.ttf (123,036 bytes) from rastikerdar/vazirmatn v33.003 via jsDelivr CDN.
- Verified with `file`: both are valid TrueType Font data (15 tables, 13 names), Copyright 2015 The Vazirmatn Project Authors.
- Font family name in the TTF metadata: `Vazirmatn` — matches the existing `FontFamily("Vazirmatn")` call in QuestPdfGenerator line 99.
- Created `LICENSE-vazirmatn.txt` with the SIL Open Font License, Version 1.1 (OFL).
- Added both TTF files as `<Content>` with `CopyToOutputDirectory=PreserveNewest` in MobileShop.Services.csproj.
- License file marked `CopyToOutputDirectory=Never` (documentation only, not needed in runtime output).
- `dotnet build src/MobileShop.slnx --nologo --no-restore` → Build succeeded. 0 Warning(s), 0 Error(s).
- Verified font files present in both output directories:
  - `src/MobileShop.Services/bin/Debug/net10.0/PDF/Fonts/` ✓ (both TTF, correct sizes)
  - `src/MobileShop.Tests/bin/Debug/net10.0/PDF/Fonts/` ✓ (both TTF, correct sizes)
- Ran targeted PDF tests: 4 passed, 2 skipped (Persian tests still skipped — will be unskipped in Step 2).
- Did not modify QuestPdfSetup.cs or QuestPdfGenerator.cs — no font registration or layout changes in this step.
- Did not touch src/MobileShop.Web wwwroot/fonts WOFF2 assets — those remain for browser use.

## Commit SHA
Stage K Step 1: `ad162453c0ebbf832c300ec6619445bd0b54d00c`

## Limitations
None.

## Friction noted
1. The tool's 30s timeout is insufficient for the full test suite (243 tests take ~36s). Used --no-build --no-restore after initial build with background process polling.
2. The `read_files` tool returned stale content when reading plan.md/QuestPdfSetup.cs mid-edit during the session — used `run_commands` with `cat`/`sed` as a fallback.

## Problems
None remaining.

## Status
COMPLETE — Stage J (all steps done, signed off) and Stage K Step 1 done. Stopping for Job B review before Step 2.
