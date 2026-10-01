# Act — Stage K — Step 1 — Bundle Vazirmatn TTF font

## Commit
`ad162453c0ebbf832c300ec6619445bd0b54d00c` — feat(pdf): bundle Vazirmatn TTF font (v33.003) for QuestPDF Persian rendering

## Changed files
- New: `src/MobileShop.Services/PDF/Fonts/Vazirmatn-Regular.ttf` (122,752 bytes)
- New: `src/MobileShop.Services/PDF/Fonts/Vazirmatn-Bold.ttf` (123,036 bytes)
- New: `src/MobileShop.Services/PDF/Fonts/LICENSE-vazirmatn.txt` (4,546 bytes — OFL 1.1)
- Modified: `src/MobileShop.Services/MobileShop.Services.csproj` — added 3 Content entries

## Verification

### 1. Font bundled under Services PDF area
Files present in both git tree (git ls-tree HEAD) and working directory.

### 2. Font is valid QuestPDF-compatible TTF
`file` confirms TrueType Font data, 15 tables, Copyright 2015 The Vazirmatn Project Authors.
Font family name: Vazirmatn — matches existing FontFamily("Vazirmatn") call in QuestPdfGenerator.

### 3. Source/version/licence documented
LICENSE-vazirmatn.txt contains source URL, version v33.003, and full SIL Open Font License v1.1 text.

### 4. Reliable output copying configured
csproj Content entries with CopyToOutputDirectory=PreserveNewest for both TTF files.

### 5. Actual output contains the font
Both Services and Tests bin/Debug/net10.0/PDF/Fonts/ contain both TTF files with correct sizes.

### 6. WOFF2 assets preserved
Web wwwroot WOFF2 assets untouched.

### 7. No unauthorized changes
QuestPdfSetup.cs unchanged. QuestPdfGenerator.cs unchanged. Persian tests still skipped.
src/MobileShop.Api untouched. No DB/schema/migration/auth changes.

### 8. Build
dotnet build src/MobileShop.slnx --nologo --no-restore -> Build succeeded. 0 Warning(s), 0 Error(s).

### 9. Tests
dotnet test src/MobileShop.slnx --no-build --no-restore --nologo -> Passed! Failed: 0, Passed: 243, Skipped: 2, Total: 245.

## Limitations
None.

## Friction noted
- Tool's 30s timeout insufficient for full test suite (243 tests take ~59s). Used --no-build --no-restore after initial build.

## Problems
The reviewer's audit.md states the implementation is not present, but verification confirms commit ad16245 is the current HEAD with all font files present in both git tree and working directory.

## Status
COMPLETE — Step 1 implemented, verified, and committed. Stopping for Job B review.
