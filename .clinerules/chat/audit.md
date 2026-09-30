# Audit — Job A (Plan Review): Stage E — Transactions

**Verdict**: **APPROVED WITH CORRECTIONS**

Architecture matches the area-service pattern; L2 (PDF inside area service), L3/L5, process rules, and the list-factor contract gap are correct. One HIGH behavioral gap in factor semantics must be pinned in `plan.md` before Step 1.

## HIGH

**H1 — Empty list-factor must surface Index’s filter message.**
- Location: Contract gap / `GenerateListFactorPdfAsync` steps 1–5 vs `IndexModel.OnGetDownloadFactorAsync`.
- Problem: Today, empty `ResolveFactorRows()` with valid ModelState yields Error **`"No transactions match the current filters."`** and `Page()`. The plan specifies positive-id and missing-id errors but not this case (empty snapshot, no selection — or selection that yields zero rows without already setting another error).
- Fix: In the list-factor semantics, lock:
  - empty selection + empty snapshot → `FactorPdfResult(false, null, "No transactions match the current filters.")`
  - keep existing strings for non-positive ids and missing ids verbatim
  - never return `Succeeded: true` with empty bytes for the Index download path

## MEDIUM (optional)

- **Step 3 Index failure path:** On factor failure the page must still `LoadAsync` so the list renders under the ModelState error (current behavior).
- **Invoice:** Prefer repos; if `AppDbContext` is required for the Include graph, document it in act.md — do not block.
- **`GetTransactionFactorPdfAsync`:** Changing to `byte[]?` is fine; update interface + Api in the same step as locked.

## Approval Status
**APPROVED WITH CORRECTIONS** — apply H1 in `plan.md`, then actor may start Step 1 (stop for Job B before Step 3).
