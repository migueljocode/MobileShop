# Audit — Job A (Plan Review)

**Plan reviewed**: `.clinerules/chat/plan.md` — "Live Profit Calculation (Task 2 of 8)"
**Risk/Confidence in plan**: `LOW / HIGH` → reviewer would normally do a sanity check only.

## Approval Status: REQUIRES REPLANNING

The plan's core premise is factually false: **the feature is already implemented and committed.**

## Findings

### CRITICAL — The step is a no-op; the work already exists

`CreatePhone.cshtml` **already contains** the exact feature the plan asks the actor to add, committed and clean in the working tree:

- `src/MobileShop.Web/Pages/Products/CreatePhone.cshtml:163` — `@section Scripts {`
- lines 166-192 — `updateFromPercent()`, `updateFromAmount()`, and both `input` listeners, using `[data-price]` / `[data-percent]` / `[data-amount]`
- The markup hooks already exist too: `data-price` (line 36), `data-percent` (line 43), `data-amount` (line 53)

Introduced by commit `5ae227c` — *"feat(web): add profit estimation (percent ↔ dollars) fields on Create Phone & Create Apple ID pages"* — which touched both pages simultaneously.

**Direct evidence that the goal state is met:** the script block in `CreatePhone.cshtml` (lines 166-192) and the one in `CreateAppleId.cshtml` (lines 68-94) are **byte-identical** (`diff` returns empty). The plan's stated objective — "Create Phone page shows live profit calculation matching Create Apple ID behavior" — is *already true in the repository as committed.*

### HIGH — Wrong file path; the cited "reference implementation" is misidentified

Plan line 15 cites `CreateAppleId.cshtml` at **`Pages/AppleIds/`**. No such directory exists. The file is at `src/MobileShop.Web/Pages/Products/CreateAppleId.cshtml` — same folder as `CreatePhone.cshtml`. An actor following the plan literally would fail to find it, and the same wrong path is implied for the CreatePhone target.

### HIGH — "Copy/paste the Scripts section" is an active hazard

Execution Notes (line 44) instruct the actor to *copy/paste and adapt* the Scripts section from `CreateAppleId.cshtml`, and line 20 says to *add* an `@section Scripts` block. `CreatePhone.cshtml` already has one, and it also contains unrelated manufacturer/model `fetch` logic (lines 194+). Pasting the reference section in wholesale would **duplicate the profit handlers (double-firing `input` events, so `12` typed into percent would compound to `1212.00`)** and risk clobbering the dropdown logic. This is the same class of defect as the distribution test blocks and the `<header>` title: prose instructing a destructive edit that contradicts the code's actual state.

### MEDIUM — Nullable-models claim is right but irrelevant as justification

Plan line 4 cites "server-side models already support nullable `ProfitPercent`/`ProfitAmount`" as justification for LOW risk. Verified true — `CreatePhoneInputModel.cs:16,19` and `CreateAppleIdInputModel.cs:10,13` are both `decimal?` — but it supports a conclusion the plan draws from the wrong premise. The fact that the server side is already correct is further evidence the task was completed, not evidence that it is safe to redo.

### LOW — Verification story is the Task 1 anti-pattern

Plan line 32 gates on `dotnet build` alone and line 31 says "Tests: None (UI-only). Manual verify in browser." That is precisely the gate that let the broken navbar (`16b1f99`) ship. `dotnet build` cannot execute JavaScript at all, so it can never verify this task.

## Missing Implementation Details

- **No statement of what remains to be done.** The plan should open by establishing that the feature already exists and state the actual delta — which, on the evidence, is nothing, or whatever narrow defect the owner is actually reporting.
- **No root-cause step.** The owner's Task 2 entry is written as a *request*, but a request does not imply absence. The planner should have checked whether the checkbox is stale.
- **Known real defect, unaddressed.** The shared `updateFromPercent`/`updateFromAmount` implementation has a usability flaw: there is no guard against the two fields ping-ponging. Both listeners are attached, so auto-filling the other field does not re-trigger (`input` is not fired by programmatic assignment, so the immediate risk is low) — **but** if a user then types into the auto-filled field, the other field is overwritten, and the user cannot type a value into `Profit $` without also being constrained by the percent. More importantly, `toFixed(2)` on percent yields values like `33.33` that immediately re-derive a slightly different amount. If the owner is reporting a real symptom, this is where to look — the plan never asks.

## What I recommend

1. **Do not run this plan as written.** It would duplicate working handlers.
2. **Confirm with the owner what is actually wrong** on the Create Phone page, if anything. Task 2 in `to-do.md` appears to be a stale checkbox carried over from the old stage list — `5ae227c` already delivered it.
3. If the feature genuinely misbehaves, replan as a **fix to the existing** `updateFromPercent`/`updateFromAmount` block in `CreatePhone.cshtml:171-189` (and consider whether `CreateAppleId.cshtml` needs the same fix, since the code is duplicated rather than shared), naming the observed symptom as the target behaviour.
4. If the owner confirms the feature works, tick Task 2 with `5ae227c` as evidence and move to Task 3 (Manual Date Range) — which, unlike this one, describes concrete observable defects and is worth planning properly.

## Scope

`to-do.md` untouched — no stage or task ticked, per reviewer rules.
