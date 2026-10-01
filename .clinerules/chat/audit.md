# Audit — Stage M replan (Job A verification before Act)

**Verdict: APPROVED WITH CORRECTIONS**

Independent review of `to-do.md`, `plan.md`, `audit.md` (reopening note), and current Products/Create Phone code on `main`.

## Repo facts (confirmed)

| Claim | Evidence |
|-------|----------|
| PartNumber selector shown for All/Phones/Apple IDs | `Products/Index.cshtml` always renders the GET form |
| Visible **Filter** button | submit button in that form |
| Options from global catalog | `IndexModel` calls `GetPartNumbersAsync()` with no inventory scope |
| No Part number column | `ProductListItemViewModel` has no PartNumber field; table headers omit it |
| Create Phone has no PartNumber UI | no PartNumber in `CreatePhone.cshtml` / `CreatePhoneInputModel` |
| Schema/service already exist | `PartNumber` entity + `GetPartNumbersAsync` / `CreatePartNumberAsync` / filter on `GetInventoryRowsAsync` |
| Stage M unchecked | `to-do.md` line is `[ ]` — correct |

Previous “Stage M done” was invalid for the owner’s UX bar. Reopening is correct.

## Plan vs owner requirements

| Owner requirement | Plan coverage |
|-------------------|---------------|
| Filter only when **Phones** selected | Step 1 desired #1 |
| No Filter button; apply on select | Step 1 #2–#4 |
| Combobox = part numbers on **current phone inventory** | Step 1 #6–#8 (omit active PartNumber filter when building options) |
| Part number **column** | Step 1 #9–#12 |
| Combobox + **Add New** if missing | Step 2 on **Create Phone** (Model-scoped) |

**Assumption (acceptable):** “Add New” is on Create Phone, not on the Products list filter. Matches Stage N language and existing Add New patterns. Products Index remains filter-only.

## HIGH corrections (must be in plan before Act)

1. **ViewModel + all projections:** Add an explicit list field (e.g. `PartNumberLabel` / `string? PartNumberCode`) on `ProductListItemViewModel` and update **every** `new ProductListItemViewModel(...)` site (phones, Apple IDs, any other callers) so the record still compiles. Apple IDs / null phones → display value that the view shows as `N/A`.
2. **Type toggles:** All / Apple IDs links must **not** keep a selected `partNumberId` (clear query). Phones-only may preserve it. Today all three links pass `asp-route-partNumberId`.
3. **Auto-apply mechanism:** Prefer GET form + `onchange` submit on the select (or equivalent). No Filter button. No new SPA/API page.
4. **Selector visibility:** When `Type != "phone"`, do not render the PartNumber form at all (not merely empty options).

## MEDIUM (optional; plan already mostly covers)

- Inventory-derived options may need a dedicated service method; do not overload Model-scoped `GetPartNumbersAsync(modelId)` for the Index filter.
- Stale `partNumberId` not in available set → treat as no selection (plan already says this).

## Process

- One step → one commit → Job B → next step.
- Actor does not edit `plan.md` / `audit.md` / `to-do.md`.
- Stage M stays unchecked until final Step 3 Job B PASS.

## Gate

**Step 1 is authorized only after plan.md incorporates the HIGH corrections above** (planner or reviewer may fold them). Then Actor implements **Step 1 only** and stops.
