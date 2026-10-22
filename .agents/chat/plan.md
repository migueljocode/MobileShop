# Plan — Stage 17 — Repair product edit loading, binding, and update integrity

## Completed steps
- ~~[x] Step 1 — Case/Glass navigation loading~~ Case and Glass edit mapping safely loads current eligible fits.
- ~~[x] Step 2 — Apple ID Details lookup~~ Details navigation uses the canonical product identity and rejects unsupported type keys.
- ~~[x] Step 3 — Cable connector validation~~ Connector options round-trip and invalid connector values are rejected server-side.
- ~~[x] Step 4 — Type-safe, no-op-safe updates~~ Validation precedes mutation; unrelated profiles and unchanged product state are preserved.
- ~~[x] Step 5 — Power Bank ports and accessory dropdowns~~ Port collections and counts stay synchronized; accessory options use validated model IDs.
- ~~[x] Step 6 — Cross-type regression coverage and CI~~ Edit mapping/update regressions cover all 11 product types; CI passed.

## Global Definition of Done
- Apple ID save returns to the correct product Details page.
- Case and Glass edit pages handle missing navigation safely and retain valid compatibility selections.
- Cable connectors, Power Bank ports, Portable Storage kind, and Case/Glass model options validate and round-trip.
- No-op updates preserve product pricing and unrelated profiles.
- Cross-type regression suite and GitHub Actions pass.
- Scope restrictions were respected; no API or sample-data changes.

## Handoff
Stage 17 is complete. No carry-over is recorded. Next role: Planner to identify the next stage from `.agents/TODO.md`.
