## Reviewer Audit — Stage 20, Job B (Step 3)

**PASS.** Verified commit `0e3fe0e`: `WithSpecsAsync` runs one query for all factor rows (no per-row query); both factor paths call it; the shared `IncludeProductGraph` helper keeps every invoice include and adds `DeviceSpecProfile` (+ storage), `GlassProfile.ModelFits` and `CaseProfile.ModelFits`, which the spec key needs. Rows with no transaction keep empty spec values. Tests cover the planned behaviour: identical products merge to quantity 2, different colours stay separate, single-row factor carries the key. GitHub Actions **#701 — Success** on `0e3fe0e`. Only the two files named in Step 3 changed.

Note (not blocking): the invoice query now loads a few more joins. Output is unchanged; the performance cost is small and the Step 3 plan allowed it.
