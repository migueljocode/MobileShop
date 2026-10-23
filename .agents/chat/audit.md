## Reviewer Audit — Stage 20, Job B (Step 2)

**PASS.** Verified commit `92340102`: the grouping key is exactly (`ProductLabel`, `SpecKey`, `Direction`, `FinishedPrice`, `PersonLabel`) as planned; `TotalPrice` is unchanged; `PaidAmount` = `UnitPrice` × `Quantity`; `ToFactorRow` overloads still compile with the new optional parameters. Tests cover identical merge, spec split, price split, party split, first-appearance order, empty spec label, and total = sum of line paid amounts. GitHub Actions **#700 — Success**. Only files named in Step 2 changed.
