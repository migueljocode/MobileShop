# Plan — Stage N — Create Phone & Create Apple ID UX

## Reviewer Briefing

- Stage M is complete (including Create Phone **PartNumber** combobox + Add New). **Do not re-implement PartNumber UI**.
- Highest risk: **finished-price** contract — `Input.Price` (paid) + profit → stored `Product.Price` (finished), same rule client and server.
- Existing pages **link** profit % ↔ profit $ (edit one updates the other). Shared script must **preserve** that; do not switch to exclusive modes.
- Second-hand / guarantee **Notes** on Phone only; Apple ID keeps product-level Notes and shared pricing only.
- Shared pricing JS is **created in Step 2** and only **reused** in Step 3.
- No schema/migration; no `src/MobileShop.Api` project; no production auth; no DB init policy changes.

## Assumptions (labeled)

- **A1:** Bind property stays `Price` = **paid** cost. Display label may say “Paid price”. **Finished price** is computed and stored in `Product.Price`.
- **A2 (corrected):** Client keeps **linked** percent ↔ amount (current behavior). Server finished price:
  1. if `ProfitAmount` has a value → `paid + amount`
  2. else if `ProfitPercent` has a value → `paid + paid * percent / 100`
  3. else → `paid`
  Amount-first on the server is only a **safety net** for partial/stale posts; when the client stays in sync both branches match.
- **A3:** Conditional second-hand / guarantee UX = **Create Phone only**. Create Apple ID = shared pricing only.
- **A4:** PartNumber + Add New on Create Phone is **done** (Stage M) — out of scope.

## [ ] Step 1 — Bind models + server finished-price + notes persistence

- Files
  - Modify:
    - `src/MobileShop.Models/ViewModels/Web/BindModels/CreatePhoneInputModel.cs`
    - `src/MobileShop.Services/DataServices/Dal/ProductsDataService.cs` — `CreatePhoneAsync`, `CreateAppleIdAsync`
    - `CreateAppleIdInputModel` only if XML/docs need “paid” clarification (no new fields required)
  - Tests: `ProductsDataServiceTests`
  - Do not touch: schema, migrations, sample-data, Api **project**, auth, PDF, PartNumber handlers, UI scripts (Step 2+)

- Symbols
  - `CreatePhoneInputModel`: add `SecondHandNotes`, `GuaranteeNotes` (`string?`, `[StringLength(500)]`)
  - Private helper e.g. `ComputeFinishedPrice(decimal paid, decimal? percent, decimal? amount)` used by both creates
  - `CreatePhoneAsync` / `CreateAppleIdAsync`

- Current → Desired
  1. `Product.Price = ComputeFinishedPrice(input.Price, input.ProfitPercent, input.ProfitAmount)` per **A2**.
  2. Keep existing range validation on paid/percent/amount; do not allow a negative finished result (guard if needed).
  3. When `IsSecondHand`: set `SecondHand.Notes` (trim empty → null); keep `TestPeriodDays` / `UsedDurationDays` as today.
  4. When `HasGuarantee`: set `Guarantee.Notes` (trim empty → null); **preserve** `StartDate = DateTime.Today`, expiry, corporation defaults.
  5. Flags false → profiles stay null.
  6. Apple ID create uses the same finished-price helper; Email/Password/Notes unchanged (plaintext password policy untouched).

- Tests
  - Phone: percent-only; amount-only; both present → amount path (safety net).
  - Phone: second-hand notes / guarantee notes persisted; flags false → no profiles.
  - Apple ID: finished price with percent and with amount.

- Verify: focused `ProductsDataServiceTests` create cases.

- Done when: server is source of truth for finished price + notes; Job B PASS.

- Risk: HIGH
- Confidence: HIGH (after A2 correction)

## [ ] Step 2 — Create Phone UI: toggles, notes, shared pricing script

- Files
  - **Create** `src/MobileShop.Web/wwwroot/js/create-product-pricing.js` (this step owns creation)
  - Modify `CreatePhone.cshtml` (and `.cs` only if required)
  - Do not touch PartNumber modal/handlers

- Shared script must
  - Keep **percent ↔ amount** two-way sync (extract from current inline script)
  - Recompute **read-only finished price** display with the same rule as **A2**
  - Target elements via existing `data-price` / `data-percent` / `data-amount` plus a finished-price display hook (e.g. `data-finished-price`)

- Create Phone UI
  1. Second-hand block: `TestPeriodDays` + **Second-hand notes** visible only when `IsSecondHand` is checked.
  2. Guarantee block: corporation, expiry, **Guarantee notes** visible only when `HasGuarantee` is checked.
  3. Labels: paid price; profit % / $; **Finished price** read-only (not a competing bind that overrides server).
  4. Post **paid + profits**; server always recomputes finished price.
  5. Do not break manufacturer/model/color/PartNumber Add New.

- Verify: focused tests if any; Job B checks markup + script reference.

- Done when: toggles + notes + shared script live on Create Phone; Job B PASS.

- Risk: MEDIUM
- Confidence: HIGH

## [ ] Step 3 — Create Apple ID reuses shared pricing script

- Files
  - `CreateAppleId.cshtml` — remove duplicate inline pricing sync; reference `create-product-pricing.js`
  - Do **not** recreate or fork the formula
  - No second-hand/guarantee UI on Apple ID

- Desired: same paid / profit / read-only finished display as Phone; Email/Password/Notes unchanged.

- Verify: focused tests if needed; both pages load one script.

- Done when: single client formula; Job B PASS.

- Risk: LOW
- Confidence: HIGH

## [ ] Step 4 — Final Stage N validation (Reviewer sign-off)

- `dotnet build src/MobileShop.slnx --nologo && dotnet test src/MobileShop.slnx --nologo --no-build`
- Record exact counts.
- Checklist: Phone toggles + notes; finished price client+server; Apple ID pricing only; PartNumber untouched; no Api/auth/PDF/migration scope creep.
- Reviewer ticks Stage N only after PASS.

- Risk: HIGH | Confidence: HIGH

## Global Definition of Done

- Phone: test days + second-hand notes only when second-hand checked.
- Phone: guarantee fields + guarantee notes only when guarantee checked.
- Notes persisted on profiles when applicable; `Guarantee.StartDate` still today.
- Finished price = A2 rule (linked client + server); stored as `Product.Price`.
- One shared `create-product-pricing.js`; Apple ID pricing only; no PartNumber work this stage.
- Full build/test green; every step Job B PASS; then Stage N checked in `to-do.md`.

## Execution notes

One step → one commit → `act.md` → **STOP** for Job B. Do not edit plan/audit/todo. Do not re-open PartNumber. Do not touch `src/MobileShop.Api`.

**Step 1 is authorized.**
