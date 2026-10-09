# Actor Report — Phone/Product Edit Save fix

## Commit
`0125bdd` — `fix(products): load full graph tracked when editing a product`

## What the step did
`UpdateProductAsync` loaded the product with `FindAsync`, which does not include
`PhoneProfile`, so the phone row was never loaded and every phone edit failed with
"Product is not a phone." (nothing was written).

1. Load with the same includes as `GetProductForEditAsync`, **tracked**, via a new
   `FindTrackedWithIncludesAsync` on `IBaseRepo`/`BaseRepo` (a tracked twin of the
   existing `FindWithIncludesAsync`; the existing one is `AsNoTracking`, so a tracked
   include-load was not expressible through the old contract).
2. Persist by saving the changed tracked entity (`SaveChangesAsync`) — no `Table.Update`
   on a half-loaded graph.
3. `product.Barcode = input.Identifier` (was `product.Barcode = product.Barcode`, so the
   Identifier box could never save).
4. Finished price stored via `TryComputeFinishedPrice(input.Price, input.ProfitPercent,
   input.ProfitAmount)` — the create rule (amount-first, then percent, then paid price).
   Profit is no longer ignored.
5. Removed the `SupportsDualSim`/`SupportsEsim` assignments — the edit page has no such
   checkboxes, so binding them forced both to `false`.
6. One service test: `UpdateProductAsync_edits_phone_identifier_imei_and_stores_finished_price`
   edits a phone (identifier, IMEI, paid price 1000 + 10% profit) and asserts the phone
   row (IMEI, SIM flags preserved) and `Product.Price` (1100) actually change.

## Verification
- Local: `dotnet build src/MobileShop.slnx` — 0 warnings, 0 errors; full suite
  `dotnet test` — 425/425 passed (106 in ProductsDataServiceTests).
- `Action: #591 — Success` (jobs: `test`, `PowerShell log utility (Windows)`).

## Limitations
- The tracked include-load required adding `FindTrackedWithIncludesAsync` to the shared
  `IBaseRepo`/`BaseRepo` contract. The step prescribed a tracked load, but the existing
  `FindWithIncludesAsync` is `AsNoTracking`, so the mechanism was not expressible without
  it. Flagging in case the Planner wants it folded into a formal stage.
- Second-hand/guarantee profile removal (`product.SecondHandProfile = null` /
  `product.GuaranteeProfile = null`) remains a no-op on the DB (the FK lives on the
  profile side). Pre-existing behavior, untouched by this step.

## Friction noted
None.

## Problems
None.

## Status
COMPLETE

## Next
Reviewer Job B (`verify`) — please verify this step. Note for the Planner: this fix is not
yet registered as a stage/step in `to-do.md`/`plan.md` (Stage AB is signed off); the
Planner should register it so the to-do list stays accurate.
