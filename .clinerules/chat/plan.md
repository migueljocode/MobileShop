# Plan — Stage P — Printable factor redesign

## Scope
- Preserve the existing factor pipeline: transactions data → factor view model → IPdfGenerator.GenerateTransactionFactor → QuestPdfGenerator.
- Presentation redesign only; no transaction semantics, API/auth, schema/migration, or unrelated changes.
- Keep QuestPDF, A4 printability, existing Vazirmatn/RTL setup, and both single + selected/filtered factor entry points.
- Reviewer performs final Stage P sign-off after all steps pass Job B and rendered validation.

## ~~[x] Step 1 — Define the factor presentation contract~~
**PASS — Actor a27c09ea79eefa3ca8ab82c2be4d7575c74c73a9; Actions #17 succeeded.**

## ~~[x] Step 2 — Redesign the QuestPDF factor layout~~
**PASS — Actor f2fe3c8d13dd492eadd9115a67f4f2505bcd40c2; CI passed.**
Rendered inspection carried forward to Step 5; existing invoice rendering remains separate.

## ~~[x] Step 3 — Preserve and verify both factor entry points~~
**PASS — Actor fd9ee7ab2d14431990ee4febd6ac65db1e4cf08c; Actions #41 succeeded.**
Selected-factor coverage verifies mixed Buy/Sell roles; single-factor coverage remains intact.

## ~~[x] Step 4 — Expand PDF regression tests~~
**PASS — Actor 74fa05b733fbc44fb4d1e65faeb5e8e771c722ac; Actions #53 succeeded.**
Coverage includes single/multi-row factors, totals, valid PDFs, Persian/RTL, long text, decimal/large values, empty data, null input, and existing invoice/Persian paths.

## ~~[x] Step 5 — Rendered PDF inspection~~
**PASS — Actor 7b1c1203ac41e473b527f6343c077ab3a906cbc4; GitHub Actions #59 succeeded.**
Both representative PDFs were rendered and inspected: single Persian/RTL factor and selected 12-row mixed Buy/Sell factor. Both are clean one-page A4 output with readable hierarchy, parties, transaction table, totals, signatures, footer/shop info, Persian/RTL and mixed Persian/Latin wrapping, decimal/large-value formatting, and no overlap/clipping. The signature-area pagination issue found during inspection was corrected and rechecked.

## [ ] Step 6 — Final Stage P validation
- Reviewer validates all stage evidence, CI, both entry points, rendered English/Persian PDFs, and scope compliance.
- Require 0 warnings/errors and passing GitHub CI.
- Only after all criteria pass may Reviewer mark Step 6 and Stage P complete in plan.md and .clinerules/to-do.md.

## Customer-facing factor requirements
- Use a credible commercial-document hierarchy: shop identity → title/date → parties/context → line items → total → signatures → footer/contact.
- Prioritize readable typography, spacing, alignment, table proportions, whitespace, and financial clarity.
- Use configured shop identity intentionally; never invent business data.
- Keep realistic multi-row, mixed Buy/Sell, long Persian/Latin, and decimal/large-value cases coherent.
- Treat Persian/RTL as a complete layout requirement, including direction, alignment, mixed text, dates, and numbers.
- Preserve A4 margins, page breaks, repeated table headers where needed, footer placement, and signature areas.
- Reuse the existing verified Vazirmatn/font-registration path.
- Keep existing invoice PDF behavior unchanged unless a shared infrastructure change is strictly required and verified.

## Definition of Done
- Existing factor pipeline and both entry points remain intact.
- Professional printable factor with header, parties, line items, total, signatures, footer.
- Persian/RTL remains correct; existing invoice PDFs remain intact.
- Regression/full-solution tests pass and representative PDFs are visually inspected without defects.
- GitHub CI passes; no API/auth/schema/unrelated changes.

## Execution
- One implementation step → one Actor commit → Job B → next step.
- Actor does not modify plan.md, audit.md, or to-do.md to mark progress.
- Reviewer compacts completed steps after Job B.
