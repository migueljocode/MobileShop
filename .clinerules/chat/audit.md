# Audit — Job A (Plan Review): Stage D — People

**Verdict**: **APPROVED WITH CORRECTIONS**

Architecture and stage split are sound (lists/creates → details product rows → six pages → validation). L3 (no DI removal) and entity-leak fix are correct. One HIGH compile gap must be fixed in `plan.md` before the actor starts; then execute without a second review round.

## HIGH

**H1 — Step 1 cannot implement `IPeopleDataService` without the two details members.**
- Location: Step 1 Change item 3 (“Do not implement details…”) vs `IPeopleDataService` (six members, including `GetCustomerDetailsAsync` / `GetSellerDetailsAsync`).
- Problem: a class that implements the interface must provide all members or Step 1 does not compile.
- Fix (bind in plan): In Step 1, implement both details methods as **header-only** — same projection as today’s entity `GetDetailsAsync`, and `Products = []` (requires the defaulted `Products` member on both details records in Step 1, or construct with an empty list). Step 2 only **fills** product rows (ids + inventory-shaped filter). Do not leave details as `throw`/`NotImplementedException`.

## MEDIUM (optional)

- **Step 2 fork:** “extend VM **or** separate list” leaves an architectural choice. Lock **extend** `CustomerDetailsViewModel` / `SellerDetailsViewModel` with `IReadOnlyList<ProductListItemViewModel> Products = []` so Step 3 is one call + `Products = details.Products` and matches the interface XML (“details and the customer's product rows”).
- **Ctor:** `IBaseRepo<Product> products` is unused until Step 2 — fine to include in Step 1 once details return empty `Products`, or add the products repo only in Step 2; either is OK if H1 is fixed.
- **Page tests:** `CustomersModelTests` / `SellersModelTests` exist; Step 3 must rewire them to `PeopleDataService` (plan already says so — keep explicit).

## LOW

- Step 4 route list omits detail URLs (`/People/CustomerDetails?id=…`) — optional smoke; not required for approval.
- Invalid `sortBy` already maps to Name via the existing switch’s `_` arm — no extra code needed beyond a verbatim port.

## Approval Status
**APPROVED WITH CORRECTIONS** — apply H1 (and preferably lock the VM extension) in `plan.md`, then actor may start Step 1.
