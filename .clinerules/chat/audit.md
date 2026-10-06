# Audit — Job B: Stage Z Step 1 (repair in flight)

**Status:** test fix pushed; **await Action green** before PASS.

**Feature:** `751913b` — `SearchSelectableProductsAsync`  
**Repair:** `b0128df` — assert `ProductId` (not `EntityId`) in direction/query test  
**CI:** Action **#496** on the repair commit (in progress at write time)

## Gate

Reviewer re-checks #496. **Do not start Step 2** until PASS.
