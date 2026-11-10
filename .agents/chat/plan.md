# Plan — Soft-delete products; edit + soft-delete customers and sellers

## Proposed stages
- [ ] Stage 12 — Product Details: Remove (soft delete)
- [ ] Stage 13 — Customers and sellers: Edit + Remove (soft delete)

## Reviewer Briefing
- **Soft delete is already the platform rule:** `BaseEntity.IsDeleted`, `IBaseRepo.Delete`/`DeleteAsync` only set the flag, and `ApplySoftDeleteForEntities()` adds a global query filter so live lists/search omit deleted rows. **Never hard-delete.**
- **Stage 12** is smaller: Products/Details already has Edit; add Remove that soft-deletes the `Product` (and keeps historical transactions). Confirm list/details hide the product after delete.
- **Stage 13** is larger: no EditCustomer/EditSeller pages today; details only have “Back”. Need edit forms (person fields + role fields) and remove actions with the same soft-delete path. **Block soft-delete of Zone Mobile** shop seller/customer (and preferably the shop Person) — sales/purchases depend on it.
- UI pattern: details page actions (Edit + danger Remove with confirm); optional list-row links are nice-to-have, not required in v1 if details cover both.
- Do not touch Api, auth middleware, or DB init policy. Do not hard-delete transactions when removing a product or party.

---

## [ ] Step 1 — Service: soft-delete product by id
- Files: inspect: `IProductsDataService`, `ProductsDataService`, `IBaseRepo.DeleteAsync`, product list filters; modify: interface + Dal service (+ Api stub if required to compile); tests in `ProductsDataServiceTests` or web Details tests.
- Symbols: new `Task<ServiceResult> SoftDeleteProductAsync(int productId)` (name may match existing style, e.g. `DeleteProductAsync` but implementation must be soft only).
- Current -> Desired: given a live product id, mark `Product.IsDeleted = true` via repo `DeleteAsync`, return success; missing id → not-found failure. Subsequent inventory/list/details queries do not return it (query filter).
- Change: load product (tracked or via repo delete overload by id if one exists); call `products.DeleteAsync(...)`; do **not** cascade-delete transactions or profiles in SQL — profiles may remain orphaned under soft-delete filter or be soft-deleted only if an existing pattern already does; prefer product-only soft delete unless inspection shows list joins break without profile flags (query filter on Product is enough for Products list).
- Depends on: none.
- Edge cases: already-deleted id → not found (filter) or idempotent success; sold products still soft-deletable (inventory hide is the goal); concurrent edit.
- Tests: create product → delete → GetDetails/list no longer returns it; row still present in DB with `IsDeleted == true` if test uses IgnoreQueryFilters.
- Verify: unit tests green.
- Done when: service soft-deletes product and tests prove hide + flag.
- Risk: MEDIUM (transaction history still references product id — details of old transactions must not break)
- Confidence: HIGH

## [ ] Step 2 — Products/Details: Remove button + confirm + post handler
- Files: inspect: `Details.cshtml`, `Details.cshtml.cs`; modify: those two; do not touch Edit page beyond shared patterns.
- Symbols: page `OnPostDeleteAsync` (or `OnPostRemoveAsync`); form POST with antiforgery.
- Current -> Desired: Details header actions include **Remove** (danger/outline-danger) next to Edit; confirm before submit; on success redirect to Products Index for that type with optional success message; on failure show message on Details.
- Change: add form button; handler calls `SoftDeleteProductAsync`; use `confirm(...)` or a small Bootstrap modal consistent with existing UI (prefer native confirm if no shared confirm pattern exists — keep minimal).
- Depends on: Step 1.
- Edge cases: double-submit; missing product after concurrent delete.
- Tests: optional page test posting delete.
- Verify: manual Details → Remove → product gone from list; DB flag set if checked.
- Done when: Remove works end-to-end on Details.
- Risk: LOW
- Confidence: HIGH

## [ ] Step 3 — Service: update + soft-delete customer and seller
- Files: inspect: `IPeopleDataService`, `PeopleDataService`, `Customer`/`Seller`/`Person` entities, `ShopLabels.ZoneMobile` / `ShopPartyResolver`; modify: interface + Dal (+ Api stubs); input models for edit; tests in People tests.
- Symbols: `UpdateCustomerAsync`, `UpdateSellerAsync`, `SoftDeleteCustomerAsync`, `SoftDeleteSellerAsync` (names per project style).
- Current -> Desired:
  - **Update** customer: NationalId + person FirstName/LastName/Phone/Notes (edit the linked Person).
  - **Update** seller: EntityType + same person fields.
  - **Soft delete** customer/seller: `DeleteAsync` on the **Customer** or **Seller** row (role disappears from lists). Do **not** soft-delete Person in v1 if the other role still exists; if the other role is absent, optional Person soft-delete is OUT OF SCOPE unless needed for search cleanliness — document choice: **v1 soft-deletes only the role entity**.
  - **Refuse** soft-delete when the party is the Zone Mobile shop customer or seller (match by person name `ShopLabels.ZoneMobile` or known ids from resolver). Clear error message.
- Change: implement load-by-id, validate, update fields, `SaveChanges`; delete path uses repo soft delete. Lists already use filtered queries.
- Depends on: none (parallel to Stage 12 after Step 2 if desired; ordered here after product for review simplicity).
- Edge cases: Zone Mobile blocked; missing id; validation lengths match create models; person shared by both roles — deleting customer keeps seller and person.
- Tests: update fields persist; delete hides from GetCustomerRows/GetSellerRows; Zone Mobile delete fails; IgnoreQueryFilters shows IsDeleted.
- Verify: tests green.
- Done when: people service supports edit + soft-delete with Zone Mobile guard.
- Risk: HIGH (shop party; shared Person)
- Confidence: MEDIUM

## [ ] Step 4 — Edit + Remove UI for customers and sellers
- Files: inspect: `CustomerDetails`, `SellerDetails`, `CreateCustomer`/`CreateSeller` forms for field parity; modify/create: `EditCustomer`/`EditSeller` pages (or edit sections on details — prefer dedicated Edit pages mirroring Create), details headers with Edit + Remove; list Details link stays.
- Symbols: new Razor pages under `Pages/People/`; POST delete handlers on Details or Edit.
- Current -> Desired: from Customer/Seller details, user can **Edit** (form: same fields as create, pre-filled) and **Remove** (confirm → soft delete → redirect to People index filtered by role). Zone Mobile remove shows error without deleting.
- Change: Edit GET loads view model; POST calls update service. Remove POST calls soft-delete service. Match existing btn styles (primary Edit, danger Remove, outline Back).
- Depends on: Step 3.
- Edge cases: validation errors redisplay form; concurrent delete.
- Tests: optional page tests; rely on service tests for rules.
- Verify: manual edit name/phone; remove non-shop customer/seller; attempt remove Zone Mobile fails safely.
- Done when: both roles have working Edit and Remove via soft delete.
- Risk: MEDIUM
- Confidence: MEDIUM

## [ ] Step 5 — Final validation
- Files: inspect only; `graphify update .` after code commits if required.
- Current -> Desired: Stages 12–13 complete; CI green.
- Depends on: Steps 1–4.
- Verify: `Action: #<run_number> — Success`; manual product remove + people edit/remove + Zone Mobile blocked.
- Done when: Global Definition of Done met.
- Risk: LOW
- Confidence: HIGH

## Global Definition of Done
- Products/Details has Remove; product is soft-deleted (`IsDeleted`) and disappears from live inventory/details.
- Customers and sellers can be edited and soft-deleted from the People area; Zone Mobile cannot be removed.
- No hard deletes; no Api/auth/DB-init policy changes; tests cover soft-delete flag + hide-from-list; CI green.

## Execution notes
One step → one commit → STOP for Job B. Actor must not edit plan/audit/TODO. After Reviewer APPROVES, Planner appends Stages 12–13 to `.agents/TODO.md` unchecked; Actor starts **Step 1** only. Stages 8–9/11 remain separate if still open.
