# Plan — Stage K — Persian PDF font support

## Reviewer Briefing

- Stage K is the next unchecked stage after Stage J.
- Stage I already moved the PDF tests to `MobileShop.Tests.Services.PDF`; Stage K now resolves the two intentionally skipped Persian tests.
- The repository currently contains Vazirmatn WOFF2 assets under `src/MobileShop.Web/wwwroot/fonts`, while the Stage K requirement calls for a TTF/OTF asset because QuestPDF needs a loadable font file for this test path.
- The existing PDF generator already uses the `Vazirmatn` family for Persian output; Stage K should provide/register the required font rather than redesign the PDF.
- The owner has explicitly accepted the Vazirmatn licence for this stage.
- `src/MobileShop.Api`, database/schema/migrations, authentication, data services, and unrelated UX remain out of scope.

- ~~[x] Step 1 — Bundle the QuestPDF-compatible Vazirmatn font~~

- Add a QuestPDF font asset such as `Vazirmatn-Regular.ttf` under the Services PDF area (for example `src/MobileShop.Services/PDF/Fonts/`), not under Web `wwwroot`: the PDF generator lives in Services and must not depend on Web static assets.
- Mark the font as content copied to the Services build output (or otherwise make it reliably available to the registration path); verify the actual test/runtime output contains the file.
- Preserve the existing font family name `Vazirmatn` and do not remove the existing WOFF2 web-font assets; they serve a different purpose.
- Pin the exact Vazirmatn source/version and retain the applicable font licence text/notice with the bundled asset or repository documentation.
- Do not change invoice layout or Persian content in this step.
- Verify the font file is present in the Services and test output path that the registration code will use.
- Risk: MEDIUM
- Confidence: HIGH
- Reviewer Job B: PASS — verified in commit `ad162453c0ebbf832c300ec6619445bd0b54d00c`.

- ~~[x] Step 2 — Register Vazirmatn for QuestPDF~~ (commit `107cc07`)

- Update `src/MobileShop.Services/PDF/Configuration/QuestPdfSetup.cs` (the existing startup hook) to register the bundled TTF/OTF with QuestPDF's `FontManager` exactly once before the first render.
- Use the bundled file/resource rather than Web `wwwroot` and do not rely on machine-installed fonts.
- Make the registration path usable by the existing direct `QuestPdfGenerator` unit tests as well as normal application startup; do not assume the Web host runs during unit tests.
- Preserve the existing `Vazirmatn` family name used by `FontFamily("Vazirmatn")`.
- Keep English PDF generation unchanged.
- Do not alter invoice layout, models, DI architecture, or database behavior.
- Verify both normal and Persian PDF generation paths.
- Risk: MEDIUM
- Confidence: MEDIUM

- [ ] Step 3 — Unskip and validate the two Persian tests

- Modify only `src/MobileShop.Tests/Services/PDF/QuestPdfGeneratorTests.cs` for the test setup/skip state/assertions needed to exercise the real PDF setup in direct unit tests.
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

- A pinned Vazirmatn TTF/OTF is bundled in the Services PDF area, copied into the relevant outputs, and its licence/source is documented.
- QuestPDF registers and resolves the `Vazirmatn` family from that bundled font in both application startup and direct PDF unit tests.
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
