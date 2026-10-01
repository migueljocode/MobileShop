# Audit — Job A: Stage N plan

**Verdict: APPROVED WITH CORRECTIONS**

## What is solid
- Stage selection matches first open `to-do.md` item; Stage M PartNumber correctly marked **out of scope**.
- Project rules respected (no Api project, no auth, no migration, no DB-init change; Apple ID password not touched).
- Notes on `SecondHand` / `Guarantee` match existing entity fields (≤500).
- Server as source of truth for finished price is the right call.
- Phone-only toggles; Apple ID pricing-only is a coherent split of the roadmap line.
- Steps are ordered; final step has full build/test DoD.

## HIGH — fix before Act

### 1. Profit fields vs existing client behavior (A2)
**Repo today:** both Create Phone and Create Apple ID scripts **keep percent and amount in sync** (edit one → rewrite the other). They are two views of one profit, not exclusive modes.

**Plan A2:** treats them as alternatives (“amount wins if both”).

That will fight the existing UX and confuse Job B.

**Required correction to fold into `plan.md`:**
- Keep the **linked** percent ↔ amount behavior on the client (extract into the shared script).
- Server finished price: `paid + profitAmount` when amount is present; else `paid + paid * percent/100` when percent is present; else `paid`.
- When the client stays in sync, both branches agree; “amount wins” is only a server safety net for stale/partial posts, not a product decision to break the dual fields.

### 2. Pin shared-script ownership to one step
Steps 2 and 3 both say the shared pricing file may be introduced “here or in Step 3.”

**Required:** Introduce `wwwroot/js/create-product-pricing.js` (or the chosen partial) in **Step 2** with Create Phone; Step 3 only **reuses** it on Create Apple ID. No second formula copy.

## MEDIUM (optional)
- Display label “Paid price” vs property name `Price` — display-only rename is fine; do not rename the bind property unless tests are updated.
- Explicitly preserve `Guarantee.StartDate = DateTime.Today` when adding Notes (current service behavior).

## Gate
Planner folds HIGH #1–#2 into `plan.md`. Then **Step 1 is authorized**.

Actor: Step 1 only → one commit → `act.md` → STOP for Job B.
