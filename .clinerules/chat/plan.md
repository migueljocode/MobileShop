# Plan — Stage N — Create Phone & Create Apple ID UX

## Reviewer Briefing

- Stage M is complete (including Create Phone **PartNumber** combobox + Add New). **Do not re-implement PartNumber UI** in this stage; Stage N inherits that work.
- Highest risk is the **finished-price** contract: today `Input.Price` is written straight to `Product.Price`. Stage N needs a clear paid-price + profit → read-only finished price, with the same rule on the server as in the browser.
- Second-hand / guarantee **Notes** exist on entities but are not on `CreatePhoneInputModel` or the form; only Phone needs those conditional sections (Apple ID already has product-level Notes and has no second-hand/guarantee create UI).
- Prefer one shared client asset for pricing (and toggles if reused) rather than copy-pasting divergent scripts on both pages.
- No schema/migration; no `MobileShop.Api` project edits; no production auth; no DB init policy changes.

## Assumptions (labeled)

- **A1:** “Paid price” is the cost the shop paid; UI field may stay bound as `Price` or be renamed for labels only. **Finished price** = paid + profit and is what is stored in `Product.Price`.
- **A2:** Profit is either **percent** or **amount**, not both. If both are provided, prefer **amount** (document in service). If neither, finished = paid.
- **A3:** Conditional second-hand / guarantee UX applies to **Create Phone** only. Create Apple ID gets the **shared pricing** UX only.
- **A4:** PartNumber + Add New on Create Phone is **done** (Stage M Step 2) and is out of scope here.

## [ ] Step 1 — Bind models + server finished-price + notes persistence

- Files
  - Inspect/modify:
    - `src/MobileShop.Models/ViewModels/Web/BindModels/CreatePhoneInputModel.cs`
    - `src/MobileShop.Models/ViewModels/Web/BindModels/CreateAppleIdInputModel.cs` (only if price field naming/docs need alignment; no second-hand fields)
    - `src/MobileShop.Services/DataServices/Dal/ProductsDataService.cs` — `CreatePhoneAsync`, `CreateAppleIdAsync`
    - `src/MobileShop.Services/DataServices/Interfaces/IProductsDataService.cs` — only if signatures/docs change (prefer no signature break)
    - `ApiProductsDataService` stubs only if interface members change
  - Tests: `ProductsDataServiceTests` for create price/notes cases
  - Do not touch: schema, migrations, sample-data.json, Api **project**, auth, PDF, CreatePhone PartNumber handlers

- Symbols
  - `CreatePhoneInputModel` — add `SecondHandNotes`, `GuaranteeNotes` (`string?`, length ≤ 500 to match entities)
  - `CreatePhoneAsync` / `CreateAppleIdAsync`
  - Optional private helper e.g. `ComputeFinishedPrice(paid, percent, amount)` used by both creates

- Current → Desired
  - Current: `Product.Price = input.Price` with no server-side profit fold-in; second-hand/guarantee profiles omit `Notes`.
  - Desired:
    1. Compute finished price on the server from paid + profit (A1–A2) and assign **that** to `Product.Price`.
    2. Reject negative finished price / invalid paid (keep existing range attributes).
    3. When `IsSecondHand`, set `SecondHand.Notes` from input (trim empty → null); still set `TestPeriodDays` as today.
    4. When `HasGuarantee`, set `Guarantee.Notes` from input; keep corporation/expiry behavior.
    5. When flags are false, profiles remain null (no empty profiles).
    6. Apple ID create uses the **same** finished-price formula; existing `Notes` field behavior unchanged.

- Edge cases
  - Both profit fields set → amount wins (A2).
  - Percent only → `paid + paid * percent/100` (decimal-safe).
  - Whitespace notes → null.

- Tests
  - Phone: percent-only and amount-only finished price.
  - Phone: both profits → amount wins.
  - Phone: second-hand notes persisted; guarantee notes persisted.
  - Phone: flags false → no SecondHand/Guarantee rows.
  - Apple ID: finished price with profit amount/percent.

- Verify: focused `ProductsDataServiceTests` create cases; full suite deferred to final step unless compile fails.

- Done when: server is source of truth for finished price and notes; Job B PASS.

- Risk: HIGH (price contract)
- Confidence: MEDIUM

## [ ] Step 2 — Create Phone UI: toggles, notes, read-only finished price

- Files
  - `src/MobileShop.Web/Pages/Products/CreatePhone.cshtml`
  - `src/MobileShop.Web/Pages/Products/CreatePhone.cshtml.cs` only if needed for display helpers
  - Prefer new shared script under `src/MobileShop.Web/wwwroot/js/` (e.g. `create-product-pricing.js`) introduced here or in Step 3 — if introduced here, Create Phone must reference it
  - Do not touch: PartNumber modal/handlers (already correct)

- Current → Desired
  1. **Second-hand block:** `TestPeriodDays` and new **Second-hand notes** textarea visible only when `IsSecondHand` is checked (show/hide via JS; keep fields in DOM or disable when hidden so postback is clean).
  2. **Guarantee block:** corporation, expiry, and new **Guarantee notes** visible only when `HasGuarantee` is checked.
  3. **Pricing:** clear labels — paid/cost input (existing `Price` or display name “Paid price”); profit % and profit amount as today; **Finished price** is a **read-only** display (not a second writable bound field that fights the server). On input change, recompute finished with the same rule as Step 1.
  4. On submit, either post paid + profits only (server computes) **or** post finished as `Price` only if it matches the formula — **prefer post paid + profits; server always recomputes** so the client cannot lie.
  5. Do not break manufacturer/model/color/PartNumber Add New flows.

- Tests: page-model tests only if the repo already covers CreatePhone toggles; otherwise service tests remain authoritative and Job B checks markup.

- Verify: focused create/page tests as applicable.

- Done when: conditional sections and finished-price UX work on Create Phone; Job B PASS.

- Risk: MEDIUM
- Confidence: MEDIUM

## [ ] Step 3 — Shared pricing script + Create Apple ID UX

- Files
  - Shared: `wwwroot/js/create-product-pricing.js` (or partial `_CreateProductPricing.cshtml` if the repo prefers Razor-owned script — pick one pattern and stick to it)
  - `CreateAppleId.cshtml` / `.cs` — wire the same pricing labels + read-only finished display
  - Ensure Create Phone uses the **same** shared asset (no duplicated formula)
  - Do not add second-hand/guarantee UI to Apple ID

- Current → Desired
  1. One shared client formula matching Step 1 (A2).
  2. Create Apple ID: paid + profit % / amount + read-only finished price; existing Email/Password/Notes unchanged.
  3. Create Phone references the same script for pricing (toggles may stay page-local if not needed on Apple ID).

- Tests: Apple ID create finished-price service coverage if not done in Step 1; no API tests.

- Verify: focused tests + smoke both create pages if feasible.

- Done when: both pages share one pricing implementation; Job B PASS.

- Risk: MEDIUM
- Confidence: MEDIUM

## [ ] Step 4 — Final Stage N validation (Reviewer sign-off)

- No production changes expected from the actor except fixing FAIL items.
- Run: `dotnet build src/MobileShop.slnx --nologo && dotnet test src/MobileShop.slnx --nologo --no-build`
- Record exact pass/fail/skip and warning/error counts.
- Checklist: conditional second-hand/guarantee on Phone; notes persisted; finished price server+client aligned; Apple ID pricing only; PartNumber untouched; no Api/auth/PDF/migration scope creep.
- Reviewer ticks Stage N in `to-do.md` only after PASS.

- Risk: HIGH | Confidence: HIGH

## Global Definition of Done

- Create Phone: Test days + second-hand notes only when second-hand is checked.
- Create Phone: Guarantee corporation, expiry, guarantee notes only when guarantee is checked.
- Notes persisted on `SecondHand` / `Guarantee` when applicable.
- Finished price = paid + profit (shared client + server); stored as `Product.Price`.
- Create Apple ID uses the same pricing UX; no PartNumber work in this stage.
- Full build/test green; each step Job B PASS; then Stage N checked.

## Execution notes

Implement **one** step, commit once, write `act.md`, **STOP** for Job B. Do not edit `plan.md` / `audit.md` / `to-do.md`. Do not re-open PartNumber. Do not touch `src/MobileShop.Api` project.
