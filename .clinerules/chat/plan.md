# Plan — Stage E: Transactions — TransactionsDataService + transaction/invoice/PDF

Stage D is signed off. Incorporates Job A audit **H1** (empty list-factor error message) and optional notes (Index still loads list on factor failure; single-id factor returns `byte[]?`).

## Process (binding)
- **One step → commit → Job B review → next step.** Do not run the whole stage in one continuous actor session without reviewer sign-off between steps.
- Only the **reviewer** ticks Stage E in `to-do.md` after the final step PASSes.

## Locked decisions (carried + Stage E)
L1 Boundaries — pages inject area interfaces only. No EF entity crosses the page boundary.
L2 One area service per page, local name `dataService`. **No** second data service and **no** page-level `IPdfGenerator` after migration — PDF generation lives **inside** `TransactionsDataService` (it may inject `IPdfGenerator`).
L3 DI — **this stage removes nothing.** Keep entity `ITransactionDataService`, `IInvoiceDataService`, phone/apple/customer/seller services (Home cards, Reports P/L, any remaining callers).
L4 Invoice/PDF ownership is this area.
L5 Reports owns profit/loss — **do not** move `GetProfitLoss*` here; leave on entity `TransactionDataService` until Stage F.
L6 Logging — `ILogger<TransactionsDataService>`; log record success/failure only.
L7 Api — any **signature change** updates `ApiTransactionsDataService` in the **same step**. Never touch `src/MobileShop.Api`. `UseApi` stays false.
L8 No auth, no schema/migrations, no DatabaseInitializer policy change, no bin/obj.
L9 Tests — `RepoTestBase` + real `TransactionsDataService` over `BaseRepo<T>` (+ real `IPdfGenerator` or thin test double only if required).
L10 **Full interface in Step 1** — implement every `ITransactionsDataService` member on the Dal class (no NIE).
L11 **List-factor errors (locked, audit H1)** — `GenerateListFactorPdfAsync` must never return `Succeeded: true` with empty/null bytes. Exact failure messages:
   - empty selection **and** empty snapshot → `"No transactions match the current filters."`
   - any selected id ≤ 0 → `"Selected transaction identifiers must be positive numbers."`
   - selected ids missing from snapshot → `$"These selected transactions no longer exist: {string.Join(", ", missing)}."`
L12 **Single-id factor** — `GetTransactionFactorPdfAsync` returns `Task<byte[]?>`; **null** when the transaction is missing (page maps null → NotFound).

## Scope
Four pages: `Transactions/Index`, `Details`, `Buy`, `Sell`.
Implement `GetInvoiceAsync` / `GenerateInvoicePdfAsync` on the area service (port `InvoiceDataService`) even though there is no Invoice Razor page yet.

## Current page inventory (verified)
| Page | Injects today | Behavior to preserve |
|------|---------------|----------------------|
| Index | `ITransactionDataService`, `IPdfGenerator` | List + factor download (selection or filtered list; single-snapshot rules; `transactions-factor.pdf`) |
| Details | `ITransactionDataService`, `IPdfGenerator` | Details + single-row factor PDF |
| Buy | transaction + seller + phone + appleId services | Sellers + selectable (Buy); `RecordBuyAsync` |
| Sell | transaction + customer + phone + appleId services | Customers + selectable (Sell); `RecordSellAsync` |

Shop sentinels: `ShopSellerId = 1`, `ShopCustomerId = 1` — port as private constants with the same TODO comment.

## Contract changes (Step 1)
Add (update Api stub same step):

```csharp
Task<FactorPdfResult> GenerateListFactorPdfAsync(
    string? direction, int take, bool ascending, IReadOnlyList<int> selectedIds);
```

New record `MobileShop.Models.ViewModels.Web.FactorPdfResult`:
`public sealed record FactorPdfResult(bool Succeeded, byte[]? Bytes, string? Error);`

Change `GetTransactionFactorPdfAsync` to `Task<byte[]?>` (L12).

### `GenerateListFactorPdfAsync` semantics (complete)
1. Load the same list snapshot as `GetListAsync(direction, take, ascending)` (clamp take 1–500 as today).
2. If `selectedIds` is null or empty:
   - if snapshot is empty → **fail** L11 empty-filter message;
   - else rows = snapshot mapped with `ToFactorRow()`.
3. Else (selection present):
   - if any id ≤ 0 → **fail** positive-numbers message;
   - distinct ids; resolve **only** against the snapshot dictionary; any missing → **fail** missing-ids message (never partial factor, never per-id extra fetch);
   - else rows = resolved factor rows in request order.
4. On success: `Bytes = pdfGenerator.GenerateTransactionFactor(new TransactionFactorViewModel(rows, DateTime.UtcNow))`, `Succeeded = true`, `Error = null`.

## Reviewer Briefing
- **H1 applied:** empty snapshot + no selection fails with the Index filter message; never succeed with empty PDF.
- **HIGH residual:** RecordBuy/Sell rejection rules + shop sentinels; invoice Include graph (prefer repos; `AppDbContext` only if unavoidable — note in act.md).
- **MEDIUM:** selectable phone∪AppleId; Step 3 Index still `LoadAsync` on factor failure.

## ~~[x] Step 1 — Contract + `TransactionsDataService` + Dal registration~~
- Files: create `src/MobileShop.Services/DataServices/Dal/TransactionsDataService.cs`, `src/MobileShop.Models/ViewModels/Web/FactorPdfResult.cs`, `src/MobileShop.Tests/Services/DataServices/Dal/TransactionsDataServiceTests.cs`; modify `ITransactionsDataService`, `ApiTransactionsDataService`, `ServiceCollectionExtensions` (one Dal line); optionally shorten BindModels FQNs.
- Ctor: `(IBaseRepo<Transaction> transactions, IBaseRepo<Seller> sellers, IBaseRepo<Customer> customers, IBaseRepo<Phone> phones, IBaseRepo<AppleId> appleIds, IPdfGenerator pdfGenerator, ILogger<TransactionsDataService> logger)` — add repos only if invoice needs them; **no** entity data services.
- Implement all members:
  1. `GetListAsync` — port entity list (direction, take clamp, date order).
  2. `GetDetailsAsync` — port details projection.
  3. `GetSellersAsync` / `GetCustomersAsync` — port party-option projections.
  4. `GetSelectableProductsAsync` — phone + AppleId selectable ports, then `OrderBy(Name)`.
  5. `RecordBuyAsync` / `RecordSellAsync` — port rejection rules (negative price; existing Buy/Sell on product); shop sentinels; `ServiceResult` with page-matching failure messages.
  6. `GetInvoiceAsync` / `GenerateInvoicePdfAsync` — port invoice assembly + `pdfGenerator.Generate` (prefer Select/repos; document if `AppDbContext` is required).
  7. `GetTransactionFactorPdfAsync` → `byte[]?` (null if missing).
  8. `GenerateListFactorPdfAsync` — full semantics under Contract changes (including L11).
- Register `ITransactionsDataService` → `TransactionsDataService` in the Dal branch only.
- Tests: list filter/order/take; details; record success + duplicate reject; selectable shape; list factor — empty snapshot message, non-positive id, missing id, success non-empty bytes; party options.
- Verify: `dotnet build src/MobileShop.slnx --nologo && dotnet test src/MobileShop.slnx --nologo --no-build`
- Done when: interface + Api + Dal green; **no page changes**.
- Risk: HIGH. Confidence: MEDIUM.

## [ ] Step 2 — Invoice follow-up (optional)
Only if Step 1 deferred invoice Includes. If invoice + list factor already done, **skip** and record reason in `act.md` — invent no extra work.
- Risk: LOW. Confidence: HIGH.

## [ ] Step 3 — Migrate four Transactions pages + tests
- Files: all four `Pages/Transactions/*.cshtml.cs`; `Tests/Web/Pages/Transactions/*`.
- Change:
  - Each ctor: `(ITransactionsDataService dataService)` only.
  - **Index:** always `LoadAsync` first (so the list is populated even when factor fails); download handler calls `GenerateListFactorPdfAsync` with normalized direction/take/order + `SelectedIds`; on `!Succeeded` add `ModelState` error from `Error` and `return Page()`; on success `File(Bytes!, "application/pdf", "transactions-factor.pdf")`.
  - **Details:** details + `GetTransactionFactorPdfAsync`; null → NotFound.
  - **Buy/Sell:** parties + selectable from service; post → `Record*Async(Input)`; failure model error; success Message strings unchanged.
- Do not edit `.cshtml` unless forced (stop and report).
- Verify: build + full suite.
- Risk: MEDIUM. Confidence: HIGH.

## [ ] Step 4 — Stage E validation
- Chain 1: build + full suite.
- Chain 2: Production host; **200** on `/Transactions`, `/Transactions/Buy`, `/Transactions/Sell`, `/Transactions/Details?id=1` (if seeded), regression Home/Products/People/Reports/Account.
- Optional: factor download returns `application/pdf` for a non-empty filter.
- Non-destructiveness: no `InitializeForDevelopment` in Production log; optional row-count fingerprint stable.
- Done when: four pages use only `dataService`; no page `IPdfGenerator` or entity transaction/phone/apple/customer/seller injects; entity DI still registered; suite + smoke green.
- Risk: LOW. Confidence: HIGH.

## Global Definition of Done
- Full `ITransactionsDataService` (including list factor + L11 messages) on Dal; registered.
- Index/Details/Buy/Sell depend on a single `dataService`.
- Factor/invoice PDFs only via area service + existing `IPdfGenerator`.
- No entity service/repo registration removed; profit/loss stays on entity transaction service until Stage F.
- Build + suite + Production smoke green.

## Execution notes
- One step per commit; Conventional Commits; no Co-authored-by.
- Stop after each step for Job B.
- Audit H1 is applied — actor may start **Step 1 only** after this plan.
- OUT OF SCOPE: Reports/ProfitLoss migration, Home cards migration, deleting entity Transaction/Invoice services.
