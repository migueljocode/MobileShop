# Stage Z — Step 3 Act Summary

## Scope
Repairing the Stage Z Step 3 CI failure by supplying the new IProductsDataService constructor dependency in existing BuyModel tests.

## Changes
- Updated RecordModelTests BuyModel constructions to pass null! for the unused product service.
- Updated PersonCreateHandlerTests BuyModel construction to pass null! for the unused product service.

## Verification
- GitHub Actions is the CI gate; local dotnet build/test was not run.
- Commit: this repair commit.
- Action: pending.

## Limitations
- None.

## Friction noted
- Job B identified stale BuyModel constructor call sites after the Step 3 dependency was added.

## Problems
- Previous Step 3 Action #501 failed at build because these existing tests still used the two-parameter BuyModel constructor.

## Status
STOPPED — awaiting GitHub Action result.
