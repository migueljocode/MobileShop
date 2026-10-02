# Audit — Stage Q — Job A Plan Review

## Findings

### HIGH — Step 1 leaves the API contract decision unresolved
**Location:** `.clinerules/chat/plan.md`, Step 1; `src/MobileShop.Services/DataServices/Interfaces/IProductsDataService.cs` and `src/MobileShop.Services/DataServices/Api/ApiProductsDataService.cs`.

The plan requires adding `CreateGlassesAsync` to `IProductsDataService` while also requiring the API implementation to remain untouched. The repository currently has `ApiProductsDataService : IProductsDataService`; adding a required interface member means that class no longer satisfies the interface and the solution will not compile. The plan's instruction to "stop for reviewer resolution" does not give the Actor an executable design.

**Fix:** revise Step 1 to explicitly choose a contract-compatible approach that preserves the repository's API-untouched invariant. If using a default interface member, specify its exact behavior (the existing API path should continue to throw `NotImplementedException`) and test the DAL path. Alternatively, define a separate capability contract and explicitly specify its DI behavior for both DAL and the existing API selection. Do not leave this as an Actor architectural decision.

### HIGH — Bulk success result is under-specified
**Location:** `.clinerules/chat/plan.md`, Step 2.

The page must show how many products were created, but the plan only names `CreateGlassesAsync` and does not define its return contract. The existing `ServiceResult` carries a single `EntityId`, described as the identifier of one created entity, so the Actor would have to invent how a bulk count is represented or introduce a new result type.

**Fix:** specify the exact return type/contract for the bulk operation, including the success count and the existing error-field/message behavior, and state how the PageModel consumes it.

## Missing Implementation Details

- State the exact service contract and result type for bulk creation.
- State the exact API-compatibility mechanism before implementation begins; the current "stop if needed" wording is not actor-ready.

## Approval Status

**APPROVED WITH CORRECTIONS**
