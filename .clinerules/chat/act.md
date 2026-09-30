# Act Report — Stage E Step 3

- Commit: `84a3ced` — refactor(transactions): migrate four Transactions pages to ITransactionsDataService
- Verification:
  - Chain: `dotnet build src/MobileShop.slnx --nologo` -> exit 0; **0 errors, 0 warnings**.
  - Chain: `dotnet test src/MobileShop.slnx --nologo --no-build` -> **Failed: 0, Passed: 529, Skipped: 2, Total: 531** (EXIT=0).
  - Targeted first: `--filter FullyQualifiedName~Transactions` -> **47 passed, 0 failed**.
  - DoD spot-checks: all four Transactions page models inject `(ITransactionsDataService dataService)` only (grep for `IPdfGenerator|ITransactionDataService|ISellerDataService|ICustomerDataService|IPhoneDataService|IAppleIdDataService|Repo` across `Pages/Transactions/*.cshtml.cs` = **no match**); `git diff --stat` on `Pages/Transactions/*.cshtml` is **empty** (no view edits).
- What was implemented (plan Step 3 scope only):
  - **Index** — ctor now `(ITransactionsDataService dataService)`. `LoadAsync` still runs first and still normalizes `Direction`/`Order`/`Take` (1-500), so the list renders even when the factor fails. The download handler now calls `dataService.GenerateListFactorPdfAsync(Direction, Take, Order == "asc", SelectedIds)`; on `!Succeeded` it adds the service `Error` to `ModelState` and returns `Page()`; on success `File(factor.Bytes!, "application/pdf", "transactions-factor.pdf")`. The former private `ResolveFactorRows` and the page-level `IPdfGenerator` dependency were removed — that logic now lives in the service (L2).
  - **Details** — ctor `(ITransactionsDataService dataService)`. `OnGetAsync` unchanged semantics; `OnGetFactorAsync` calls `GetTransactionFactorPdfAsync(id)` and maps **null -> NotFound**, otherwise `File(pdfBytes, "application/pdf")` with no download name (inline/printable). No page-level `IPdfGenerator` (L12).
  - **Buy / Sell** — ctor `(ITransactionsDataService dataService)`. Parties from `GetSellersAsync`/`GetCustomersAsync`; selectable products from `GetSelectableProductsAsync(TransactionDirection.Buy|Sell)` (the phone+AppleId union and name ordering now live in the service). Post calls `RecordBuyAsync(Input)` / `RecordSellAsync(Input)` and adds `result.Message` as the model error on failure; success `Message` strings unchanged (`"Buy recorded successfully."` / `"Sale recorded successfully."`).
  - **Page tests** — `IndexModelTests` and `DetailsModelTests` rewired from Moq to the real `TransactionsDataService` over `BaseRepo<T>` with a thin `IPdfGenerator` double (records the last factor). `IndexModelTests` keeps all prior assertions (exact selection order, dedupe, non-positive rejection, missing-id rejection, filtered/ordered/limited snapshot, summary total) and adds a filtered-download case. `DetailsModelTests` adds a details-render/NotFound case. New `RecordModelTests` covers Buy/Sell load-selections, post success, and post rejection surfaced as a model error.
- Limitations: None. Scope stayed exactly on the four pages plus their tests; `to-do.md` untouched.
- Friction noted: build (~21 s) and the full suite (~41 s) both exceed the ~30 s command window, so each was launched detached via `setsid` and the log polled until the summary plus `EXIT=` line appeared. Also `InvoiceViewModel` (namespace `MobileShop.Models.ViewModels`) and the `IPdfGenerator` implementations required an explicit `using MobileShop.Models.ViewModels;` in three test files — a recurring small papercut when writing a PDF double.
- Problems: First build failed with 5 `CS1729` errors because the page tests still used the two-argument page ctors; expected, and resolved by the rewire above. A second build failed with 6 `CS0246`/`CS0535` errors for the missing `InvoiceViewModel` namespace, fixed with the added using.
- Status: COMPLETE

---

# Act Report — Stage E Step 2 (SKIPPED)

Step 2 was conditional: it existed only if Step 1 deferred invoice `Include`s. Step 1 shipped the complete invoice graph (all profile Includes, same shape as `InvoiceDataService`), and the list factor was implemented in full. Per the plan's own instruction — "If invoice + list factor already done, **skip** and record reason in `act.md` — invent no extra work" — **no code was written for Step 2**. Job B (`34ce512`) independently confirmed "Step 2 may be skipped (invoice complete)".
