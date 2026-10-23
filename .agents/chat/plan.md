# Plan — Stage 20 — Factor lines grouped by product specs with quantity and paid amount

## Requirements
- Quantity is **not stored**. It is the number of factor rows that are the same product, merged into one line (e.g. `20× iPhone 17 glass`).
- Products that differ in specs (e.g. iPhone 17 blue vs green) stay on separate lines.
- New columns: `تعداد` (quantity) and `مبلغ پرداختی (ریال)` = `تعداد` × `فی (ریال)`. `فی` is the per-unit price (`FinishedPrice`) — confirmed by the user.
- Must not change: DB schema, entities, migrations, `Transaction`, `src/MobileShop.Api`, `Services/DataServices/Api/**`, sample data, the Persian invoice (`ComposePagePersian`), `RenderFactorSummary`.

## Facts found in the code (verified)
- A factor row is one transaction (`TransactionFactorRowViewModel`). `ProductLabel` is only manufacturer + model name, so it does not show color or specs.
- `Product.Barcode` and `Phone.IMEI1` are unique indexes: every `Product` row is one physical unit, so "same product" cannot mean the same `ProductId`, Barcode or IMEI.
- The list snapshot (`TransactionListItemViewModel`) is an EF projection; spec data needs navigations and cannot be computed inside that selector.
- `TotalPrice` sums `FinishedPrice` and the header `تعداد اقلام` uses `model.Rows.Count`; both stay correct if `Rows` stays one-per-transaction and lines are derived from it.
- `RenderFactorContent` runs inside `ContentFromRightToLeft()` (`QuestPdfGenerator.cs` line 83): the **first declared column is the rightmost**.

## Design (no DB change)
- Keep `Rows` one-per-transaction. Add `SpecKey` and `SpecLabel` to each row and derive `TransactionFactorViewModel.Lines` by grouping. The PDF renders `Lines`.
- The total is unchanged: a line has one unit price, so `quantity × unit price` equals the sum of its rows.

## Assumptions (labelled, not facts)
- A: **Spec identity** = `Product.ModelId` + `Product.ColorId` + the fields in the Step 1 table.
- B: Excluded from spec identity: Barcode, IMEI1/IMEI2, Apple ID email, `Notes`, second-hand used duration and test period, guarantee corporation/dates.
- C: A line needs one `فی`, one party and one direction, so rows merge only when `FinishedPrice`, `Direction` and `PersonLabel` also match. This narrows the user's "only specs differ" wording and is **not yet confirmed**. It is one `GroupBy` key tuple in Step 2; relaxing it means removing members from that tuple.
- D: The line's product text is `ProductLabel` plus `SpecLabel` (color name, and built-in storage when the product has a `DeviceSpec`); otherwise blue and green iPhones would print identically.
- E: Placement of the new columns is a layout choice the user did not specify: they go right after `فی` in declaration order (Step 4). Changing it only reorders declarations.

## Reviewer Briefing
- Supersedes the earlier Stage 20 draft (it added a `Quantity` column and migration; the user rejected that). No schema, migration or form change remains.
- Assumptions C and E are the user-visible decisions still open; both are isolated (one tuple, one declaration order).
- Step 1 is MEDIUM/MEDIUM: the spec table was checked against the entity files (property names below are real).
- Step 3 is MEDIUM risk: it extracts the product `Include` chain of `LoadInvoiceGraph` into a shared helper; invoice output must stay identical.
- Step 4 is MEDIUM risk: six columns in an RTL table; the column order and widths below are explicit, and only a rendered PDF proves they fit.
- Open state note: `TODO.md` shows Stage 19 ticked while the last audit said its sign-off was NOT MET; overwriting `chat/plan.md` drops Stage 19's open Step 3.

## ~~[x] Step 1 — Spec identity helpers (pure, Models layer)~~
- Files: inspect: `src/MobileShop.Models/Entities/Product.cs`, `DeviceSpec.cs`, `Phone.cs`, `Laptop.cs`, `Cable.cs`, `Charger.cs`, `PowerBank.cs`, `PortableStorage.cs`, `Glass.cs`, `Case.cs`, `src/MobileShop.Models/Extensions/InvoiceProfileExtensions.cs` (style reference); create: `src/MobileShop.Models/Extensions/ProductSpecExtensions.cs`, `src/MobileShop.Tests/Models/Extensions/ProductSpecExtensionsTests.cs`; do not touch: entities, configurations, `GetInvoiceExtras` methods.
- Symbols: `ProductSpecExtensions.ToSpecKey(this Product)` returns `string`; `ProductSpecExtensions.ToSpecLabel(this Product)` returns `string`.
- Current -> Desired: no notion of product spec identity. Desired: `ToSpecKey` joins these segments (null profile contributes an empty segment, never throws):
  - always: `ModelId`, `ColorId`
  - `DeviceSpecProfile`: `Ram`, `StorageCapacityId`, `ChargeSpeed`
  - `PhoneProfile`: `PartNumberId`, `SupportsDualSim`, `SupportsEsim`
  - `LaptopProfile`: `CpuId`, `GpuId`, `DisplaySize`
  - `CableProfile`: `Connector1`, `Connector2`, `Length`
  - `ChargerProfile`: `Wattage`, `PortCount`, `Pd`
  - `PowerBankProfile`: `CapacityMah`, `MaxWattage`, `PortCount`, `Pd`
  - `PortableStorageProfile`: `Kind`, `StorageCapacityId`, `Speed`
  - `GlassProfile` / `CaseProfile`: the sorted `ModelId` values of `ModelFits`
  - `SecondHandProfile`: only whether it is present
  `ToSpecLabel` returns `ColorNavigation.Name` and, when `DeviceSpecProfile` is loaded, `"{StorageCapacityNavigation.Gb} GB"`, joined by ` · `; empty when neither exists.
- Change: one small private helper per profile (SRP), selected the same way `TransactionsDataService.GetProductExtras` selects a profile.
- Depends on: none.
- Edge cases / error handling: product with no profile (key is model + color only); null color; unloaded navigations treated as absent; set-valued specs sorted before joining.
- Tests: same specs with different Barcode/IMEI/Notes give equal keys; different color gives different keys; different DeviceSpec storage gives different keys; different cable length gives different keys; glass fitting a different model gives a different key; second-hand vs new gives different keys; label shows color and storage.
- Verify: CI green; diff shows only the two new files.
- Done when: all new tests pass in CI.
- Risk: MEDIUM
- Confidence: MEDIUM (the definition of "spec" is Assumption A)

## ~~[x] Step 2 — Group rows into factor lines (view models)~~
- Files: inspect: `src/MobileShop.Models/ViewModels/Web/TransactionFactorRowViewModel.cs`, `TransactionFactorViewModel.cs`, `src/MobileShop.Models/Extensions/TransactionFactorExtensions.cs`; modify: `TransactionFactorRowViewModel.cs`, `TransactionFactorViewModel.cs`; create: `src/MobileShop.Models/ViewModels/Web/TransactionFactorLineViewModel.cs`; do not touch: `TransactionListItemViewModel`, `TransactionDetailsViewModel`, `ToFactorRow` signatures.
- Symbols: `TransactionFactorRowViewModel` (new trailing optional parameters `string SpecKey = ""`, `string SpecLabel = ""`); `TransactionFactorLineViewModel(string ProductText, int Quantity, long UnitPrice, string PersonLabel)` with `long PaidAmount => UnitPrice * Quantity`; `TransactionFactorViewModel.Lines`.
- Current -> Desired: the PDF iterates rows. Desired: `Lines` groups `Rows` by the tuple (`ProductLabel`, `SpecKey`, `Direction`, `FinishedPrice`, `PersonLabel`), keeps first-appearance order, and sets `Quantity` to the group size. `ProductText` is `ProductLabel` plus `" · " + SpecLabel` when `SpecLabel` is non-empty. `Rows`, `TotalPrice` and the `Rows.Count` header stay unchanged.
- Change: add the optional row parameters (existing callers and tests keep compiling); implement `Lines` with one `GroupBy`; add the line record.
- Depends on: none.
- Edge cases / error handling: empty factor gives zero lines; rows with empty `SpecKey` group by label + price + party; `PaidAmount` is a partial sum of the existing total, so it adds no overflow risk.
- Tests: in `TransactionFactorExtensionsTests` or a new `TransactionFactorViewModelTests`: three identical rows give one line with Quantity 3 and PaidAmount 3× unit price; different `SpecKey` gives two lines; same specs at a different price gives two lines; different party gives two lines; `TotalPrice` equals the sum of line `PaidAmount`; first-appearance order kept.
- Verify: CI green; existing factor tests green (update any that now merge identical rows and say so in `act.md`).
- Done when: CI green and `Lines` behave as above.
- Risk: LOW
- Confidence: HIGH

## ~~[x] Step 3 — Fill SpecKey and SpecLabel in the data service~~
- Files: inspect: `src/MobileShop.Services/DataServices/Dal/TransactionsDataService.cs` (`GenerateListFactorPdfAsync`, `GetTransactionFactorPdfAsync`, `LoadInvoiceGraph`); modify: `TransactionsDataService.cs` only; do not touch: `ApiTransactionsDataService`, `ITransactionsDataService`, `IPdfGenerator`.
- Symbols: `TransactionsDataService.LoadInvoiceGraph(int)`, a new private shared include helper for the product graph, a new private method that loads the product graphs for a set of transaction ids and returns `Dictionary<int, (string Key, string Label)>`.
- Current -> Desired: factor rows carry no spec data. Desired: once the final rows are known, load their product graphs in one query and set `SpecKey`/`SpecLabel` on each row with `row with { ... }`; the single-transaction path does the same so a one-row factor is labelled consistently.
- Change: extract the product `Include` chain of `LoadInvoiceGraph` into one shared helper (invoice output must stay identical); add the includes the key needs and the chain lacks: `DeviceSpecProfile` with `StorageCapacityNavigation`, and `ModelFits` on `GlassProfile` and `CaseProfile`; query `context.Transactions.Where(t => ids.Contains(t.Id))` once, never per row.
- Depends on: Steps 1-2.
- Edge cases / error handling: an id missing from the loaded set keeps empty `SpecKey`/`SpecLabel` (no exception); the existing missing-id rejection and the single-snapshot guarantee (`GetListAsync` first) are unchanged.
- Tests: in `src/MobileShop.Tests/Services/DataServices/Dal/TransactionsDataServiceTests.cs`: two sales of the same-model glass produce one line of quantity 2 (check via the model passed to the PDF generator); blue and green phones produce two lines; the single-transaction factor still generates; `GenerateInvoicePdfAsync`/`GetInvoiceAsync` output is unchanged.
- Verify: CI green; diff shows only the helper extraction, the new includes and the row mapping.
- Done when: CI green and the data-service tests pass.
- Risk: MEDIUM
- Confidence: MEDIUM

## ~~[x] Step 4 — Render the new columns in the factor table~~
- Files: inspect: `src/MobileShop.Services/PDF/Configuration/QuestPdfGenerator.cs` (`RenderFactorTable`, `HeaderCell`, `BodyCell(cell, text, background, amount, numeric)`), `src/MobileShop.Tests/Services/PDF/QuestPdfGeneratorTests.cs`; modify: `QuestPdfGenerator.cs` (`RenderFactorTable` only) and `QuestPdfGeneratorTests.cs`; do not touch: `RenderFactorSummary`, the header block, `ComposePagePersian`, fonts.
- Symbols: `QuestPdfGenerator.RenderFactorTable(IContainer, TransactionFactorViewModel)`.
- Current -> Desired: four columns over `model.Rows`, declared `فی` 105, `طرف معامله` 1.15 relative, `محصول` 2.25 relative, `#` 35. Desired: six columns over `model.Lines`, declared in this order (first declared is rightmost): `فی (ریال)` ConstantColumn(105), `تعداد` ConstantColumn(40), `مبلغ پرداختی (ریال)` ConstantColumn(120), `طرف معامله` RelativeColumn(1.15f), `محصول` RelativeColumn(2.25f), `#` ConstantColumn(35). Header cells follow the same order.
- Change: `فی` cell shows `line.UnitPrice`, `تعداد` shows `line.Quantity`, `مبلغ پرداختی` shows `line.PaidAmount`, all with `.ToGroupedDigits().ToPersianDigits()` (quantity without grouping) and `BodyCell(..., amount: true)` for the two money cells and `numeric: true` for quantity; `محصول` shows `line.ProductText`; `#` shows the line number `index + 1`.
- Depends on: Step 2.
- Edge cases / error handling: long product text wraps in the relative column; one-line and empty factors still render; a very large paid amount must stay on one line in the 120pt column.
- Tests: existing `GenerateTransactionFactor_*` tests stay green; add a test with 3 identical lines' worth of rows that returns a valid PDF; extend `GenerateTransactionFactor_writes_rendered_inspection_artifacts` with sample rows: 20 identical glass rows, one blue and one green phone row, and one row with a very large price.
- Verify: CI green; rendered artifact shows `20`, `فی` and `مبلغ پرداختی` = 20 × `فی`, two separate phone lines, Persian digits, no clipping, summary total unchanged and equal to the sum of the raw rows.
- Done when: CI green and the rendered PDF matches; if widths clip, report to the Reviewer instead of guessing.
- Risk: MEDIUM
- Confidence: MEDIUM

## ~~[x] Step 5 — Full validation~~
- Files: no source changes; update `.agents/chat/act.md` only.
- Symbols: n/a.
- Current -> Desired: record evidence for Steps 1-4.
- Change: wait for GitHub Actions; record the run number and the rendered factor check.
- Depends on: Steps 1-4.
- Edge cases / error handling: if the rendered check shows clipping or an unwanted column order, report it instead of guessing.
- Tests: full suite via CI.
- Verify: CI green; diff touches only the files listed above; no migration or Api file changed.
- Done when: evidence recorded and the Reviewer can sign off without re-running anything.
- Risk: LOW
- Confidence: HIGH

## Global Definition of Done
- The factor shows `تعداد` and `مبلغ پرداختی (ریال)`; identical-spec products appear once with the right quantity; products differing in specs (e.g. color) appear on separate lines; paid amount = quantity × `فی`.
- Factor total and `تعداد اقلام` are unchanged for any selection.
- No schema, migration, entity, form, Api or Persian-invoice change; CI green; rendered factor checked.
