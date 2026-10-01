# Stage K — Job A — Plan Review

**Verdict**: **APPROVED** — Stage K plan is technically feasible after the required clarification below.

## Review basis

Reviewed the Stage K plan, current repository structure, QuestPDF setup, PDF generator, and Persian PDF tests.

- Stage K is the next unchecked stage.
- `QuestPdfGenerator` already references the `Vazirmatn` family for Persian rendering.
- `QuestPdfSetup.UseCommunityLicense()` is the existing central QuestPDF startup hook and is invoked from `AddMobileShopPdf`.
- The unit tests instantiate `QuestPdfGenerator` directly, so application startup cannot be assumed during test execution.
- The repository currently has only Vazirmatn WOFF2 web assets under `src/MobileShop.Web/wwwroot/fonts`.
- QuestPDF's current font-management documentation supports bundled TTF/OTF fonts and manual registration through `FontManager`; it also supports automatic deployment-directory discovery when files are copied to output. citeturn0search1

## Required plan clarification — now incorporated

The original plan said the new font should preferably live alongside Web `wwwroot` assets and did not explicitly account for direct unit-test construction of `QuestPdfGenerator`.

The plan was updated in commit `afd1c5fbcc67d9c51827f2ea5c83a60b11e946fd` to require:

1. Bundle the QuestPDF font under the Services PDF area, not Web `wwwroot`, so the Services PDF generator does not depend on Web static assets.
2. Ensure the TTF/OTF is copied into the relevant Services/test output.
3. Pin/document the exact font source/version and applicable licence notice.
4. Register it through the existing `QuestPdfSetup` path using QuestPDF's font manager.
5. Ensure the registration path is exercised by direct PDF unit tests as well as normal application startup.

This removes the main feasibility ambiguity before implementation.

## Authorized scope

**Step 1 only** is authorized:

- add the pinned Vazirmatn TTF/OTF and its licence/source documentation;
- configure its build/output inclusion;
- do not register it yet;
- do not unskip tests yet;
- do not change PDF layout/content;
- do not touch `src/MobileShop.Api`, DB/schema/auth, or unrelated areas.

After Step 1, the actor must verify, commit, report the exact SHA, and STOP for Job B.

**Step 2 is not authorized yet.**

## Risks

- **Medium:** binary font packaging/output behavior must be verified rather than assumed.
- **Medium:** QuestPDF registration is process-wide/one-time setup and must not be duplicated unsafely.
- **Low:** the existing generator already uses the intended family name, so no PDF layout change is required.

## Job A conclusion

**APPROVED — Stage K Step 1 only.**

The updated plan is sufficiently concrete for Step 1 implementation. Final Stage K sign-off remains contingent on Step 2 and Step 3 Job B validation.