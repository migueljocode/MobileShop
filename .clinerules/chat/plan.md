# Plan — Stage K — Persian PDF font support

## Reviewer Briefing

- Stage K is the next unchecked stage after Stage J.
- Stage I already moved the PDF tests to `MobileShop.Tests.Services.PDF`; Stage K now resolves the two intentionally skipped Persian tests.
- The repository currently contains Vazirmatn WOFF2 assets under `src/MobileShop.Web/wwwroot/fonts`, while the Stage K requirement calls for a TTF/OTF asset because QuestPDF needs a loadable font file for this test path.
- The existing PDF generator already uses the `Vazirmatn` family for Persian output; Stage K should provide/register the required font rather than redesign the PDF.
- The owner has explicitly accepted the Vazirmatn licence for this stage.
- `src/MobileShop.Api`, database/schema/migrations, authentication, data services, and unrelated UX remain out of scope.

## Step 1 — Bundle the QuestPDF-compatible Vazirmatn font

- Add the required Vazirmatn TTF/OTF asset to the existing project font location, preferably alongside the existing `wwwroot/fonts` assets.
- Preserve the existing font family name `Vazirmatn`.
- Do not remove the existing WOFF2 web-font assets; they serve a different purpose.
- Record the font source/version and licence information in the repository documentation appropriate to the existing project conventions.
- Do not change invoice layout or Persian content in this step.
- Verify the font file is included in the relevant build/test output path and is accessible to the code that registers it.
- Risk: MEDIUM
- Confidence: HIGH

## Step 2 — Register Vazirmatn for QuestPDF

- Update only the PDF configuration/setup needed to register the bundled TTF/OTF with QuestPDF before Persian document generation.
- Use the project's existing PDF initialization/configuration path rather than introducing a separate runtime service.
- Preserve the existing `Vazirmatn` family name used by `FontFamily("Vazirmatn")`.
- Keep English PDF generation unchanged.
- Do not alter invoice layout, models, DI architecture, or database behavior.
- Verify both normal and Persian PDF generation paths.
- Risk: MEDIUM
- Confidence: MEDIUM

## Step 3 — Unskip and validate the two Persian tests

- Modify only `src/MobileShop.Tests/Services/PDF/QuestPdfGeneratorTests.cs` for the test assertions/skip state.
- Remove the two `Skip` attributes and their obsolete commercial-license wording.
- Preserve both test purposes:
  - Persian invoice generation returns a non-empty PDF payload with a PDF signature.
  - Persian generation still throws `ArgumentNullException` for a null model.
- Do not weaken assertions merely to make the tests pass.
- Run the targeted PDF tests first.
- Then run the full solution build and test suite.
- Expected result: the two previously skipped tests become passing; no existing test regressions.
- Risk: LOW
- Confidence: HIGH

## Global Definition of Done

- A Vazirmatn TTF/OTF is bundled in the repository and its licence/source is documented.
- QuestPDF can resolve the `Vazirmatn` family from the bundled font.
- Both Persian PDF tests are unskipped and pass.
- Existing English PDF tests remain passing.
- Full solution build/test passes with no new warnings or failures.
- No PDF layout redesign is performed; that belongs to Stage P.
- No database/schema/migration/auth/API-host changes.
- `src/MobileShop.Api` remains untouched.
- Stage K is signed off only after the final Job B PASS.

## Execution notes

- Implement exactly one step, verify it, commit it, and **STOP for Job B**.
- Do not start the next step until Job B explicitly returns PASS and authorizes it.
- Report the exact commit SHA, verification, limitations, friction, and problems in `.clinerules/chat/act.md`.
- Actor must not edit `.clinerules/to-do.md`, `.clinerules/chat/plan.md`, or `.clinerules/chat/audit.md` to mark progress.
- Reviewer owns `.clinerules/chat/audit.md` and final stage completion.
