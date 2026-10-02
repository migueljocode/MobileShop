# Plan — Stage P — Printable factor redesign

## Assumptions
- **A1** Keep the existing PDF pipeline: `ITransactionsDataService → TransactionFactorViewModel → IPdfGenerator.GenerateTransactionFactor → QuestPdfGenerator`.
- **A2** Redesign presentation only; do not change transaction recording semantics or database/schema.
- **A3** Keep QuestPDF as the PDF renderer and preserve the existing Persian/RTL font setup.
- **A4** The factor must support both existing entry points: one transaction and selected/filtered transactions.
- **A5** Prefer extending existing factor view models over exposing EF entities to the PDF layer.
- **A6** No API, authentication, schema/migration, or unrelated service changes.

## Reviewer Briefing
- Stage O is complete and signed off.
- Stage P is focused on making the existing transaction factor a proper printable document.
- Reviewer owns final Stage P sign-off after all implementation steps have passed Job B and final rendered-PDF validation passes.

## [ ] Step 1 — Define the factor presentation contract
- Inspect the existing `TransactionFactorViewModel`, `TransactionFactorRowViewModel`, mapping extensions, and data-service usage.
- Extend the presentation model only where the printable layout genuinely needs additional data.
- Ensure the model can represent shop/header information, factor/generated date, parties, transaction rows, finished prices, total, and signature/footer content without introducing EF dependencies.
- Preserve the existing single-transaction and selected/filtered-transaction data flows.
- Add/update focused model/mapping tests where needed.
- Do not change transaction recording behavior.
- Job B must verify the implementation against this step before Step 2 starts.

## [ ] Step 2 — Redesign the QuestPDF factor layout
- Rework `QuestPdfGenerator.GenerateTransactionFactor` / factor composition into a professional printable A4 layout.
- Include:
  - shop/header section;
  - factor title and generated date;
  - buyer/customer and seller/shop context;
  - transaction table with date, direction, product, person, and finished price;
  - prominent total;
  - notes/terms area where applicable;
  - signature areas;
  - footer/shop information where configured.
- Use reusable QuestPDF composition methods rather than one monolithic renderer.
- Use proper tables, spacing, borders, alignment, wrapping, and pagination.
- Preserve Persian/RTL rendering and the existing Vazirmatn setup.
- Keep invoice PDF generation unchanged.
- Job B must verify the implementation and PDF tests before Step 3 starts.

## [ ] Step 3 — Preserve and verify both factor entry points
- Verify the Details-page single-transaction factor still uses the same generator and produces the redesigned document.
- Verify the Transactions-page selected/filtered factor still uses the same generator and produces the redesigned document.
- Preserve the existing snapshot/selection semantics of `GenerateListFactorPdfAsync`.
- Add/update service/page tests only where required to prove both entry points continue to pass the correct factor model.
- Do not duplicate PDF rendering logic.
- Job B must verify both paths before Step 4 starts.

## [ ] Step 4 — Expand PDF regression tests
- Update `QuestPdfGeneratorTests` and relevant factor/model tests.
- Cover at minimum:
  - one-row factor;
  - multiple Buy/Sell rows;
  - total calculation;
  - valid generated PDF payload;
  - Persian/RTL factor generation;
  - long product/person text and wrapping;
  - decimal/large prices;
  - empty/edge-case factor data where the existing contract permits it;
  - header/footer/signature content paths.
- Preserve all existing invoice and Persian PDF tests.
- Job B must require all tests to pass and no unrelated regression.

## [ ] Step 5 — Rendered PDF inspection
- Generate representative PDFs from both factor entry points.
- Inspect the actual rendered documents, not only PDF bytes/unit-test results.
- Verify:
  - A4 printable layout;
  - readable header and title;
  - clear party information;
  - aligned transaction table;
  - prominent total;
  - signature area;
  - footer/shop information;
  - Persian text readability and RTL direction;
  - correct wrapping/pagination for long or multi-row content;
  - no overlap, clipping, or corrupted decimal values.
- Record concise evidence in `act.md`.
- This is validation evidence; do not mark Stage P complete here.

## [ ] Step 6 — Final Stage P validation
- Reviewer validates the complete stage after all Actor implementation steps have passed Job B.
- Run:
  `dotnet clean src/MobileShop.slnx --nologo`
  `dotnet build src/MobileShop.slnx --nologo --no-incremental`
  `dotnet test src/MobileShop.slnx --nologo --no-build`
- Require 0 warnings, 0 errors, and all tests passing.
- Confirm both factor entry points and rendered English/Persian PDFs.
- Confirm no API, authentication, schema/migration, or unrelated changes.
- Confirm GitHub Actions passes the final Actor commit.
- Record the final evidence in `audit.md`.
- Only after all criteria pass may Reviewer mark Step 6 and Stage P complete in `plan.md` and `.clinerules/to-do.md`.



## Actor design requirements — customer-facing factor UX
- Treat the factor as a **real customer-facing commercial document**, not merely a technically valid PDF or a developer report.
- The final document must follow a familiar **industry-standard factor/invoice information hierarchy** while using this application's actual data and terminology.
- Design the visual hierarchy intentionally: shop identity → document title/identity → parties/context → transaction line items → totals → optional notes/terms → signatures → footer/contact information.
- Make the document polished and beautiful but business-like: it should look credible when a customer receives, prints, or forwards it.
- Prioritize readability and scanning: the customer should quickly understand what the document is, who the parties are, what transactions/products are included, and the final amount.
- Use deliberate typography, spacing, alignment, borders, table proportions, section separation, and whitespace. Avoid cramped content, arbitrary decoration, excessive whitespace, or visually unbalanced pages.
- Integrate configured shop identity/contact information into the design as intentional branding rather than simply dumping values into a header or footer.
- Make financial information especially clear: finished prices and the final total must be easy to locate and distinguish visually.
- Design for realistic data, not only the smallest test case. Multi-row, mixed Buy/Sell, long Persian/Latin text, and large/decimal values must remain visually coherent.
- Treat Persian/RTL as a complete layout requirement: direction, alignment, table ordering, mixed Persian/Latin content, dates, and numeric values must remain natural and readable.
- Preserve A4 print usability: margins, page breaks, repeated table headers where needed, footer placement, and signature areas must behave like a real printable business document.
- Do not invent business information. If a desirable section has no legitimate data source, keep it optional or omit it rather than fabricating content.
- Actor must inspect representative rendered PDFs during implementation and iterate on the layout based on the actual visual result. Passing unit tests or merely producing a valid PDF is not sufficient evidence of acceptable UX.
- Prefer reusable, composable PDF layout sections so the visual system remains consistent across pages and future factor changes.
- Keep the existing invoice PDF behavior separate and unchanged unless a shared PDF infrastructure change is strictly required and verified.

## Definition of Done
- Existing transaction-factor pipeline remains intact.
- Factor has a professional printable layout with header, parties, line items, totals, and signature area.
- Both single-transaction and selected/filtered factor generation work.
- Persian/RTL rendering remains correct.
- Existing invoice PDF behavior remains intact.
- PDF regression tests and full solution tests pass.
- Rendered PDFs have been visually inspected with no clipping, overlap, pagination, or formatting defects.
- GitHub CI passes.
- No API/auth/schema/unrelated implementation changes.

## Execution notes
- One implementation step → one commit → Job B → next step.
- Actor does not modify `.clinerules/chat/plan.md`, `.clinerules/chat/audit.md`, or `.clinerules/to-do.md` to mark progress.
- Completed steps are compacted to outcome/evidence only after Job B.
- Reviewer performs final Stage P sign-off only after rendered-PDF and CI evidence is complete.
