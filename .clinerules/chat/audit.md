# Audit — Job A: Stage H Plan Review

**Verdict**: **APPROVED**

Reviewed Stage H `plan.md` against the repository state and the Stage G sign-off. The plan is authorized for execution, beginning with **Stage H Step 1 only**.

## Approval basis

- Stage G is complete and signed off; Stage H is the next unchecked stage.
- The plan preserves the established workflow: **one step → commit → Job B review → next step**.
- Step 1 correctly identifies the remaining specialized-repository consumers in the surviving area services:
  - `AccountDataService` → `IUserRepo`
  - `ReportsDataService` → `ITransactionRepo`, `IEmployeeRepo`
- Step 1 explicitly migrates those consumers to `IBaseRepo<T>` **before** any specialized repository deletion.
- The plan preserves the API branch and does not authorize changes to `src/MobileShop.Api`.
- The plan explicitly preserves `IDataService<T>`, `ApiDataServiceBase<T>`, and API-facing entity-service contracts still required by API stubs.
- The plan preserves `BaseRepo<T>` / `IBaseRepo<T>` as the surviving repository abstraction.
- Database/schema/migration policy, authentication state, Apple ID password handling, and development initialization policy remain unchanged.
- The plan requires behavior-preserving tests for Account and Reports before proceeding to destructive cleanup.
- Later deletion steps are gated by production-consumer searches, avoiding speculative deletion.
- Final validation includes full build/test, production smoke, production non-destructiveness, and development EnsureAdmin smoke.

## Step 1 authorization

The Actor is authorized to implement **only Step 1** now:

- migrate `AccountDataService` from `IUserRepo` to `IBaseRepo<User>`;
- migrate `ReportsDataService` from `ITransactionRepo` / `IEmployeeRepo` to `IBaseRepo<Transaction>` / `IBaseRepo<Employee>`;
- retarget the corresponding Account/Reports tests;
- preserve all documented behavior and error semantics;
- do not delete specialized repositories, obsolete entity services, or tests yet;
- do not modify `src/MobileShop.Api`;
- run the Step 1 verification;
- create the Step 1 implementation commit;
- stop for **Job B execution review**.

## Reviewer findings

No HIGH-risk architectural ambiguity blocks Step 1.

The destructive cleanup in Steps 2–4 remains appropriately gated behind Step 1 and subsequent Job B reviews. The plan's MEDIUM confidence on Step 1 is acceptable because the actor is required to prove behavior through the existing tests and full solution validation before the next cleanup step.

## Result

**Stage H Job A — APPROVED.**

**Authorized next action: Stage H Step 1 only.**
