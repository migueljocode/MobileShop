# Plan — Stage O — Transactions Buy/Sell form rework

## Assumptions

- **A1–A5** as approved (Finished price label, suggested prefill, date today, shared partial, no typeahead library).

## ~~[x] Step 1 — Suggested price on selectable products~~

- **Done** — Job B PASS (`8660d69`). `SuggestedPrice` on VM + selectable projections.

## [ ] Step 2 — Shared product-picker partial + Buy page

- Files
  - **Create** e.g. `Pages/Shared/_ProductPicker.cshtml`
  - Options with `data-suggested-price` (invariant culture)
  - Read-only **Suggested price** display; script prefills Finished price on change
  - `Buy.cshtml` / `.cs`: partial; **Finished price** label; date default today; keep **Seller**
  - Optional `[Display(Name = "Finished price")]` on bind models

- Done when: Buy shows picker + suggested + finished + date default; Job B PASS.

- Risk: MEDIUM | Confidence: HIGH

## [ ] Step 3 — Sell page uses same partial

- Sell only; Customer; no seller; no forked picker.

## [ ] Step 4 — Final Stage O validation

- Full suite + checklist; tick Stage O in `to-do.md` only after PASS.

## Execution notes

**Step 2 is authorized.** One commit → act.md → STOP.
