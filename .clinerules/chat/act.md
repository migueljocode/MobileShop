# Act Report — Stage E Step 4 (Stage E validation)

- Commit: (filled in after commit)
- Verification — Chain 1 (build + full suite, one chained call):
  - `dotnet build src/MobileShop.slnx --nologo && dotnet test src/MobileShop.slnx --nologo --no-build` -> **Build succeeded, 0 errors, 1 warning**; **Failed: 0, Passed: 529, Skipped: 2, Total: 531** (EXIT=0).
  - The single warning is the **pre-existing** `CS9124` in `src/MobileShop.Services/DataServices/Dal/ProductsDataService.cs(26,62)` ("Parameter 'IBaseRepo<Transaction> transactions' is captured..."). It is in a file this stage never touched and is the same baseline warning the Stage D report already recorded; not introduced here and deliberately not fixed (out of scope, no unrelated refactors).
- Verification — Chain 2 (Production host + smoke):
  - Host started with `ASPNETCORE_ENVIRONMENT=Production dotnet run --project src/MobileShop.Web --no-launch-profile --urls http://localhost:5199`; log confirms `Hosting environment: Production` and `Now listening on: http://localhost:5199`. Host shut down afterwards (post-shutdown probe: connection refused, curl exit 7).
  - **10/10 routes 200**: Stage E `/Transactions`, `/Transactions/Buy`, `/Transactions/Sell`, `/Transactions/Details?id=1` (seeded); regression `/`, `/Products`, `/People/Customers`, `/People/Sellers`, `/Reports/ProfitLoss`, `/Account/Login`.
  - Data-path proof: `/Transactions` body contains 27 factor/download references (list rows rendered, not an empty shell); `/Transactions/Buy` renders both `Input.ProductId` and `Input.SellerId` selects; `/Transactions/Sell` renders `Input.CustomerId` and `Input.ProductId` selects — so the area service's party/selectable projections reach the view.
  - Optional factor-download check (exercised, passed): unfiltered download -> **200 `application/pdf`, 29 869 bytes, magic bytes `%PDF`**; single selected id -> **200 `application/pdf`, 22 721 bytes**; a selection containing a missing id -> `text/html` with the rendered error "no longer exist: 99999." and **no PDF emitted** — proving the L11 failure path surfaces through the page instead of producing a partial factor.
  - Non-destructiveness: Production log has **0** matches for `InitializeForDevelopment|EnsureDeleted|EnsureCreated`, **0** for `seed`, **0** for `ERR |FTL |Exception`. Row-count fingerprint `Products|Phones|Transactions` = `17|7|26` before **and** after the pass (read-only `mode=ro`).
  - DoD spot-checks: all four Transactions page models inject `(ITransactionsDataService dataService)` only; grep for `IPdfGenerator|ITransactionDataService|ISellerDataService|ICustomerDataService|IPhoneDataService|IAppleIdDataService|Repo` across `Pages/Transactions/*.cshtml.cs` = no match; `git diff --stat` on `Pages/Transactions/*.cshtml` empty (no view edits); entity `ITransactionDataService`/`IInvoiceDataService`/`IPhoneDataService`/`IAppleIdDataService`/`ICustomerDataService`/`ISellerDataService` DI **all still registered** in the Dal branch (L3), and `GetProfitLoss*` left on the entity `TransactionDataService` (L5, untouched).
- Files changed: **none in `src/`** — this step only ticks plan.md and writes act.md.
- Limitations: None. Stage E Global Definition of Done is satisfied; Stage E was **not** ticked in `to-do.md` — per the rules only the reviewer ticks stages, after this sign-off.
- Friction noted: build (~21 s) and the full suite (~39 s) both exceed the ~30 s command window, so Chain 1 was launched detached via `setsid` and the log polled until the summary plus `EXIT=` line appeared. An initial dropdown check used guessed element ids (`sellerId`/`productId`) and reported empty; the actual markup is tag-helper based (`Input.SellerId`, `Input.ProductId`) because the `.cshtml` was intentionally never edited — re-checked against the view source rather than assuming a regression.
- Problems: None.
- Status: COMPLETE

---

# Act Report — Stage E Step 2 (SKIPPED)

Step 2 was conditional: it existed only if Step 1 deferred invoice `Include`s. Step 1 shipped the complete invoice graph (all profile Includes, same shape as `InvoiceDataService`), and the list factor was implemented in full. Per the plan's own instruction — "If invoice + list factor already done, **skip** and record reason in `act.md` — invent no extra work" — **no code was written for Step 2**. Job B (`34ce512`) independently confirmed "Step 2 may be skipped (invoice complete)".
