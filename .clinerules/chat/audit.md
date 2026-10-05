# Audit — Job A: Stage W plan

**Verdict: APPROVED**

No CRITICAL/HIGH issues.

| Check | Result |
|-------|--------|
| Matches open Stage W in `to-do.md` | **OK** |
| Five columns + server-side + indicator | **OK** |
| Direction/take kept; Order dropdown replaced by headers | **OK** |
| VM fields exist for sort keys | **OK** (`Date`, `ProductLabel`, `FinishedPrice`, `SellerLabel`, `CustomerLabel`) |
| `sortBy` optional default keeps existing `GetListAsync(..., ascending)` call sites | **OK** if added as last optional param |
| Factor PDF same order (D5) | **OK** — extend `GenerateListFactorPdfAsync` in Step 1 |
| Api host / auth / schema left alone | **OK** |
| Risk/Confidence honest | **OK** |

## Notes (not blocking)
- Prefer `GetListAsync(..., bool ascending, string? sortBy = null)` so current tests compile with only an optional arg.
- Step 2: every factor/download URL must carry `sort` + `order` or the PDF drifts from the table after a non-date sort.

## Gate

**Step 1 authorized.** Actor: one commit → green Action → `act.md` → STOP for Job B.
