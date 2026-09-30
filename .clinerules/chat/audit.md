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

The Actor was authorized to implement **only Step 1**:

- migrate `AccountDataService` from `IUserRepo` to `IBaseRepo<User>`;
- migrate `ReportsDataService` from `ITransactionRepo` / `IEmployeeRepo` to `IBaseRepo<Transaction>` / `IBaseRepo<Employee>`;
- retarget the corresponding Account/Reports tests;
- preserve all documented behavior and error semantics;
- do not delete specialized repositories, obsolete entity services, or tests yet;
- do not modify `src/MobileShop.Api`;
- run the Step 1 verification;
- create the Step 1 implementation commit;
- stop for **Job B execution review**.

## Job B — Stage H Step 1 Execution Review

**Verdict**: **PASS**

Reviewed the Step 1 implementation commit `062f644e1c330999da17b0f6855cae2a99ad7bbd`.

### Verification

- The implementation changes exactly the intended Step 1 area services and their Account/Reports tests, plus the actor report.
- `AccountDataService` no longer depends on `IUserRepo`; it uses `IBaseRepo<User>`.
- `ReportsDataService` no longer depends on `ITransactionRepo` or `IEmployeeRepo`; it uses generic repository operations.
- Account credential lookup preserves the previous case-insensitive username behavior.
- Earliest transaction selection preserves the previous non-deleted filtering and date-only result semantics.
- Distribution employee selection preserves active filtering, ordering, and person-name data required by the existing calculation.
- No production files were deleted.
- `src/MobileShop.Api` was untouched.
- No specialized repositories, obsolete entity services, or their tests were deleted.
- The actor's required full verification reports **0 build errors, 0 warnings, 550 passed, 2 skipped, 0 failed**.
- The targeted Account/Reports verification reports **20 passed, 0 failed**.
- A production-source search confirms the three specialized repository interfaces remain only in their own repository code/registrations/tests and obsolete entity-service implementations; no surviving area service references them.
- The actor caught and corrected a date-only regression and preserved the existing missing-employee test before the final verification.

### Reviewer conclusion

Step 1 satisfies its plan-defined done condition and stays within the authorized scope. No blocking defect was found.

**Stage H Step 1 Job B — PASS.**

**Authorized next action: Stage H Step 2 only.**

Do not begin Step 3 or Step 4 until their respective execution/review gates are satisfied.
