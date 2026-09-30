# Plan — Stage E: Transactions — TransactionsDataService + transaction/invoice/PDF

Stage D is signed off. This file plans **Stage E only** (next unchecked stage in `to-do.md`).

## Process (binding)
- **One step → commit → Job B review → next step.** Do not run Steps 1–4 in a single continuous actor session without reviewer sign-off between steps (Stage D process note).
- Only the **reviewer** ticks Stage E in `to-do.md` after the final step PASSes.

## Locked decisions (carried + Stage E)
L1 Boundaries — pages inject area interfaces only. No EF entity crosses the page boundary.
L2 One area service per page, local name `dataService`. **No** second data service and **no** page-level `IPdfGenerator` after migration — PDF generation lives **inside** `TransactionsDataService` (it may inject `IPdfGenerator`).
L3 DI — **this stage removes nothing.** Keep entity `ITransactionDataService`, `IInvoiceDataService`, phone/apple/customer/seller services (Home cards, Reports P/L, any remaining callers).
L4 Invoice/PDF ownership is this area (already on `ITransactionsDataService`).
L5 Reports owns profit/loss — **do not** move `GetProfitLoss*` here; leave on entity `TransactionDataService` until Stage F.
L6 Logging — `ILogger<TransactionsDataService>`; log create/record success and failure only.
L7 Api — `ApiTransactionsDataService` already exists. Any **signature change** updates the Api stub in the **same step**. Never touch `src/MobileShop.Api`. `UseApi` stays false.
L8 No auth, no schema/migrations, no DatabaseInitializer policy change, no bin/obj.
L9 Tests — `RepoTestBase` + real `TransactionsDataService` over `BaseRepo<T>` (+ `IPdfGenerator` real or thin test double only if required). Port coverage; do not delete tests for types that still exist.
L10 **Full interface in Step 1** — `TransactionsDataService` must implement **every** `ITransactionsDataService` member (no NIE on Dal). Members not yet fully wired for multi-select factor get a complete implementation as specified below in the same step they are introduced.

## Scope
Four pages: `Transactions/Index`, `Details`, `Buy`, `Sell`.
No dedicated Invoice Razor page today — still implement `GetInvoiceAsync` / `GenerateInvoicePdfAsync` on the area service (port `InvoiceDataService`) so PDF can be used later and the interface is honest.

## Current page inventory (verified)
| Page | Injects today | Behavior to preserve |
|------|---------------|----------------------|
| Index | `ITransactionDataService`, `IPdfGenerator` | List via `GetListAsync(direction, take, ascending)`; normalize direction/order/take; **factor PDF** via selection or full filtered list (`ResolveFactorRows` single-snapshot rules, error messages, `transactions-factor.pdf`) |
| Details | `ITransactionDataService`, `IPdfGenerator` | `GetDetailsAsync`; NotFound; single-row factor PDF via `ToFactorRow` |
| Buy | `ITransactionDataService`, `ISellerDataService`, `IPhoneDataService`, `IAppleIdDataService` | Party sellers + selectable products (phone∪appleId, Buy direction, OrderBy Name); `RecordBuyAsync(productId, sellerId, price, date)` → model error or success Message |
| Sell | same pattern with customers + Sell direction | `RecordSellAsync` |

Shop sentinels in entity service: `ShopSellerId = 1`, `ShopCustomerId = 1` (sample-data person/seller/customer id 1). Port as private constants on the area service with the same TODO comment.

## Contract gap (must fix in plan)
`ITransactionsDataService` already has list/details/parties/selectable/record/invoice/single-id factor PDF, but **Index multi-select factor** is not expressible as `GetTransactionFactorPdfAsync(int)`. **Add one member** (update Api stub same step):

```csharp
/// <summary>Builds a transaction-factor PDF from filters and optional selected ids (Index download).</summary>
Task<FactorPdfResult> GenerateListFactorPdfAsync(
    string? direction, int take, bool ascending, IReadOnlyList<int> selectedIds);
```

New record in Models (e.g. `MobileShop.Models.ViewModels.Web/FactorPdfResult.cs`):
`public sealed record FactorPdfResult(bool Succeeded, byte[]? Bytes, string? Error);`

Semantics — port `IndexModel.ResolveFactorRows` + `OnGetDownloadFactorAsync`:
1. Load the same list snapshot as `GetListAsync(direction, take, ascending)`.
2. If `selectedIds` empty → factor rows from entire snapshot (`ToFactorRow`).
3. If any id ≤ 0 → failure Error = `"Selected transaction identifiers must be positive numbers."`
4. Distinct selected ids; missing from snapshot → failure listing missing ids (never partial factor, never per-id extra fetch).
5. On success `Bytes = pdfGenerator.GenerateTransactionFactor(new TransactionFactorViewModel(rows, DateTime.UtcNow))`.

Single-id `GetTransactionFactorPdfAsync(id)`: load details; null → empty array or throw is wrong — return empty bytes only if callers expect File always; **prefer** same pattern as Details today: page returns NotFound when details null; service returns `byte[]?` **or** keep `byte[]` and page checks details first. **Locked:** service `GetTransactionFactorPdfAsync` returns `byte[]?` null when transaction missing; page maps null → NotFound. **Update interface + Api stub** if changing from non-nullable `byte[]`.

## Reviewer Briefing
- **HIGH — multi-select factor.** Exact Index snapshot rules; one contract addition + Api update.
- **HIGH — RecordBuy/Sell.** Port rejection rules (negative price; existing Buy/Sell on product); ShopCustomerId/ShopSellerId; map to `ServiceResult` for pages.
- **MEDIUM — selectable products.** Merge phone + AppleId `GetSelectableProductsAsync` ports; OrderBy Name for the page.
- **MEDIUM — invoice.** Port `InvoiceDataService.GetInvoice` / `GeneratePdf` without `AppDbContext` on the area service if possible — prefer `IBaseRepo<Transaction>` + same projections, or inject `AppDbContext` only if Include graph is too heavy for Select (acceptable exception if documented; prefer repos).
- **Do not** remove entity transaction/invoice DI (L3). Do not move profit/loss (L5).

## [ ] Step 1 — Contract + `TransactionsDataService` core + Dal registration
- Files: create `TransactionsDataService.cs`, `FactorPdfResult.cs` (if new), `TransactionsDataServiceTests.cs`; modify `ITransactionsDataService`, `ApiTransactionsDataService`, `ServiceCollectionExtensions` (Dal line only); optionally BindModels global usings.
- Ctor: `(IBaseRepo<Transaction> transactions, IBaseRepo<Seller> sellers, IBaseRepo<Customer> customers, IBaseRepo<Phone> phones, IBaseRepo<AppleId> appleIds, IPdfGenerator pdfGenerator, ILogger<TransactionsDataService> logger)` — add further repos only if invoice projection needs them; **do not** inject entity data services.
- Implement **all** interface members:
  1. `GetListAsync` — port `TransactionDataService.GetListAsync` (direction filter, clamp take 1–500, date order).
  2. `GetDetailsAsync` — port details projection.
  3. `GetSellersAsync` / `GetCustomersAsync` — port `SellerDataService` / `CustomerDataService` `GetPartyOptionsAsync`.
  4. `GetSelectableProductsAsync` — phone + AppleId selectable ports concatenated, `OrderBy(Name)`.
  5. `RecordBuyAsync` / `RecordSellAsync` — map input → entity `RecordBuyAsync`/`RecordSellAsync` rules; return `ServiceResult` (Succeeded false + Message matching page strings on failure).
  6. `GetInvoiceAsync` / `GenerateInvoicePdfAsync` — port invoice assembly + `pdfGenerator.Generate`.
  7. `GetTransactionFactorPdfAsync` — single transaction factor; null bytes if missing.
  8. `GenerateListFactorPdfAsync` — full Index factor semantics above.
- Register `ITransactionsDataService` → `TransactionsDataService` in Dal branch only.
- Tests: list filter/order/take; details null/known; record buy/sell success + duplicate rejection; selectable non-empty shape; list factor empty selection / missing id error / success bytes non-empty when data exists; party options ordered.
- Verify: `dotnet build src/MobileShop.slnx --nologo && dotnet test src/MobileShop.slnx --nologo --no-build`
- Done when: interface + Api + Dal compile; **no page changes yet**.
- Risk: HIGH. Confidence: MEDIUM.

## [ ] Step 2 — (reserved only if Step 1 splits invoice)
If Step 1 lands without invoice Includes working cleanly, use this step solely to finish invoice projection/tests. **If Step 1 already completes invoice + list factor, mark this step skipped in act.md with reason and do not invent work.**
- Risk: LOW. Confidence: HIGH.

## [ ] Step 3 — Migrate four Transactions pages + tests
- Files: `Index.cshtml.cs`, `Details.cshtml.cs`, `Buy.cshtml.cs`, `Sell.cshtml.cs`; tests under `Tests/Web/Pages/Transactions/`.
- Change:
  - Each ctor: `(ITransactionsDataService dataService)` only.
  - Index: load via `GetListAsync`; download handler calls `GenerateListFactorPdfAsync` with normalized direction/take/order + `SelectedIds`; on failure add ModelState error and return Page; on success `File(bytes, "application/pdf", "transactions-factor.pdf")`.
  - Details: details + factor via service; NotFound when null.
  - Buy/Sell: load parties + selectable from service; post → `Record*Async(Input)` → Message / model error; preserve success strings.
- Do not edit `.cshtml` unless binding forces it (stop and report).
- Verify: build + full suite.
- Risk: MEDIUM. Confidence: HIGH.

## [ ] Step 4 — Stage E validation
- Verify chain 1: build + full test suite.
- Verify chain 2: Production host; curl **200** `/Transactions`, `/Transactions/Buy`, `/Transactions/Sell`, `/Transactions/Details?id=1` (if exists), regression Home/Products/People/Reports/Account.
- Optional: download factor endpoint returns `application/pdf` for a known filter.
- Non-destructiveness: no `InitializeForDevelopment` in Production log; optional row-count fingerprint stable.
- Done when: four pages use only `dataService`; no page injects `IPdfGenerator` or entity transaction/phone/apple/customer/seller services; entity DI still present; suite + smoke green.
- Risk: LOW. Confidence: HIGH.

## Global Definition of Done
- `TransactionsDataService` implements full `ITransactionsDataService` (including list factor) and is Dal-registered.
- Index/Details/Buy/Sell depend on a single `dataService`.
- Factor and invoice PDFs generated only via the area service + existing `IPdfGenerator`.
- No entity service/repo registration removed; profit/loss remains on entity transaction service until Stage F.
- Build + full suite green; Production smoke green.

## Execution notes
- One step per commit; Conventional Commits; no Co-authored-by.
- Port RecordBuy/Sell and list projections; do not “simplify” shop sentinel ids.
- Stop after each step for reviewer Job B.
- OUT OF SCOPE: Reports/ProfitLoss page migration, Home dashboard cards migration, deleting entity Transaction/Invoice services.
