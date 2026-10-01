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

- ~~[x] Step 2 — Register Vazirmatn for QuestPDF~~ (commit `f8b8e69b2ce0c16486d83204e1c9df5b665be186`)
- Reviewer Job B: PASS — verified in commit `f8b8e69b2ce0c16486d83204e1c9df5b665be186`.

- Update `src/MobileShop.Services/PDF/Configuration/QuestPdfSetup.cs` (the existing startup hook) to register the bundled TTF/OTF with QuestPDF's `FontManager` exactly once before the first render.
- Use the bundled file/resource rather than Web `wwwroot` and do not rely on machine-installed fonts.
- Make the registration path usable by the existing direct `QuestPdfGenerator` unit tests as well as normal application startup; do not assume the Web host runs during unit tests.
- Preserve the existing `Vazirmatn` family name used by `FontFamily("Vazirmatn")`.
- Keep English PDF generation unchanged.
- Do not alter invoice layout, models, DI architecture, or database behavior.
- Verify both normal and Persian PDF generation paths.
- Risk: MEDIUM
- Confidence: MEDIUM

- ~~[x] Step 3 — Unskip and validate the two Persian tests~~

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


### Reviewer Final Sign-off — Stage K

- Step 3 Job B: PASS — verified Actor commit `6a4d63b16df140af7b7f4204679d32c401b7f788`.
- Both Persian tests are unskipped with assertions unchanged.
- Actor reported targeted PDF tests: 6 passed, 0 skipped; full suite: 245 passed, 0 skipped.
- Independent source inspection confirms the two `Skip` attributes were removed and the existing font-registration path remains wired for direct tests and Web startup.
- `src/MobileShop.Api` and unrelated API/DB/auth areas remain untouched by Step 3.
- Process violation recorded: Actor again edited `.clinerules/chat/plan.md` in the implementation commit. Reviewer owns progress documentation.
- Stage K: **SIGNED OFF**.


# Plan — Stage L — Quick UX wins

## Reviewer Briefing

- Stage K is signed off; Stage L is the next unchecked stage.
- Stage L is intentionally limited to small Web UX/validation changes.
- No API, database/schema/migration, authentication, data-service contract, or PDF changes.
- Existing transaction filtering is server-side through `IndexModel.LoadAsync(direction, take, order)`; the UI currently requires the **Apply filters** submit button.
- The Download Factor submit action must remain available and must continue to use the current direction/count/order filters and selected IDs.
- The Count field should debounce auto-apply rather than submit on every keystroke.
- Buy/Sell currently use `[Range(1, int.MaxValue)]` on ProductId and SellerId/CustomerId, producing the raw default validation text for zero-valued placeholder selections.
- CustomerDetails and SellerDetails currently have no navigation control back to their respective list pages.
- Existing automated coverage is in `src/MobileShop.Tests/Web/Pages/Transactions/RecordModelTests.cs`; add focused page-model/UI regression coverage where practical without introducing a new test framework or architecture.

- [ ] Step 1 — Make Transactions filters auto-apply
  - Remove only the **Apply filters** button from `Transactions/Index.cshtml`.
  - Preserve **Download Factor** and all existing selection behavior.
  - Direction and Order changes should submit the GET form immediately.
  - Count should submit after a short debounce (target ~300 ms) so typing does not issue one request per keystroke.
  - Keep the existing server-side clamping of Count to 1–500.
  - Prefer a small page-local script or existing shared site script; do not introduce a JS framework.
  - Preserve query parameter names `direction`, `take`, and `order`.
  - Add/adjust tests only as needed to prove the existing page model contract remains intact.
  - Risk: MEDIUM — GET form behavior and Download Factor share the same form.
  - Confidence: HIGH.

- [ ] Step 2 — Add Return/Back navigation
  - Add a clear **Back** link/button to Transactions Buy and Sell pages.
  - Add a clear **Back to customers** control to CustomerDetails.
  - Add a clear **Back to sellers** control to SellerDetails.
  - Prefer deterministic Razor `asp-page` navigation to the owning list pages rather than browser-history JavaScript.
  - Buy/Sell should return to `/Transactions/Index`; CustomerDetails to `/People/Customers`; SellerDetails to `/People/Sellers`.
  - Do not alter POST behavior or data-service calls.
  - Add focused rendering/page-model checks only if the existing test setup supports them cleanly.
  - Risk: LOW.
  - Confidence: HIGH.

- [ ] Step 3 — Replace raw unselected-dropdown validation messages
  - Change only the validation metadata needed for placeholder value `0` to produce friendly messages:
    - Sell ProductId → **The product should be selected.**
    - Sell CustomerId → **The customer should be selected.**
    - Buy ProductId → **The product should be selected.**
    - Buy SellerId → **The seller should be selected.**
  - Preserve rejection of values outside the valid positive-ID range.
  - Then inspect other Web bind models/select fields for the same `Range(1, int.MaxValue)` pattern (for example manufacturer/model) and apply the same friendly-message treatment where the field is a required selection.
  - Do not weaken server-side validation or silently accept zero.
  - Add regression tests for the affected model validation messages.
  - Risk: MEDIUM — shared validation metadata may affect more than one page.
  - Confidence: HIGH.

## Global Definition of Done

- Transactions no longer has an **Apply filters** button.
- Direction/Order auto-submit; Count is debounced.
- Download Factor remains functional and distinct from filter auto-submit.
- Buy, Sell, CustomerDetails, and SellerDetails have deterministic return navigation.
- The four transaction dropdowns display the requested friendly validation messages for the placeholder selection.
- Other equivalent required select fields are checked and corrected where applicable.
- Existing valid transaction behavior remains unchanged.
- Targeted tests and full solution build/test pass with no new warnings/failures.
- No API, DB/schema/migration, authentication, or PDF changes.
- Stage L is signed off only after final Job B PASS.

## Execution notes

- Implement exactly one step, verify it, commit it, and **STOP for Job B**.
- Do not start the next step until Job B explicitly returns PASS and authorizes it.
- Actor must not edit `.clinerules/to-do.md`, `.clinerules/chat/plan.md`, or `.clinerules/chat/audit.md`.
- Reviewer owns progress documentation and final Stage L checklist completion.
