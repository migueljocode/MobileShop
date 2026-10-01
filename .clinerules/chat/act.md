# Act — Stage K — Step 3 — Unskip and validate the two Persian tests

## Summary
Step 3 removes the `Skip` attributes from the two Persian PDF tests and validates them pass. The font registration from Step 2 already provides the Vazirmatn TTFs to the test runtime via `ModuleInitializer`, so the tests pass without weakening any assertions.

## Changes
- `src/MobileShop.Tests/Services/PDF/QuestPdfGeneratorTests.cs`:
  - `GeneratePersian_returns_pdf_bytes_for_valid_invoice`: removed `[Skip = "Persian generation requires Vazirmatn font which may need commercial license"]` → `[Fact]`
  - `GeneratePersian_throws_for_null_model`: same Skip removal → `[Fact]`
  - No assertion changes — both test purposes preserved (PDF payload with correct signature bytes, null model throws `ArgumentNullException`)

## Verification
- Build: `dotnet build src/MobileShop.slnx --nologo --no-restore` → Build succeeded. 0 Warning(s), 0 Error(s).
- Targeted tests: `dotnet test src/MobileShop.slnx --no-build --no-restore --nologo --filter "FullyQualifiedName~QuestPdfGeneratorTests"` → Passed! Failed: 0, Passed: 6, Skipped: 0, Total: 6.
- Full suite: `dotnet test src/MobileShop.slnx --no-build --no-restore --nologo` → Passed! Failed: 0, Passed: 245, Skipped: 0, Total: 245.

## Changed files
- Modified: `src/MobileShop.Tests/Services/PDF/QuestPdfGeneratorTests.cs` — removed 2 Skip attributes
- Modified: `.clinerules/chat/plan.md` — marked Step 3 checkbox as done

## Commit
`6a4d63b` — test(pdf): unskip Persian PDF tests now that Vazirmatn font is registered

## Limitations
None.

## Friction noted
Full test suite (~35s) exceeds the 30s tool timeout. Ran in background and polled the output file.

## Problems
None.

## Status
COMPLETE — Step 3 (unskip and validate Persian tests) is implemented and verified. All 245 tests pass with 0 skipped. Stopping for Job B review.
