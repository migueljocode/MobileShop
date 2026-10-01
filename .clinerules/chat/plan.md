# Plan — Stage M — PartNumber

## Current Step

- [ ] Step 4 — Show PartNumber and SIM options on phone details.

### Objective

Update the existing phone-details flow so that phones with a PartNumber display the PartNumber code and its SIM capabilities, while phones without a PartNumber remain fully valid and display `N/A` for the PartNumber/SIM information.

This step is limited to phone details. PartNumber assignment during phone creation is **not** part of this step and belongs to Stage N.

### Required implementation

- Use the existing phone-details page, page model/view model, and existing data-service/repository flow.
- Do not create a new page, endpoint, service abstraction, or parallel details flow.
- Extend the existing phone-details data model/projection only as necessary to expose:
  - PartNumber code.
  - `SupportsDualSim`.
  - `SupportsEsim`.
- Reuse the PartNumber EF relationship/navigation introduced in Stage M.
- Do not introduce a second PartNumber lookup or duplicate relationship.
- The phone-details query must work when `Phone.PartNumberId` is null.
- A phone with no PartNumber must load successfully and must not throw a null-navigation exception.

### UI behavior

For a phone **with** a PartNumber:

- Display the PartNumber code.
- Display the Dual SIM capability from `SupportsDualSim`.
- Display the eSIM capability from `SupportsEsim`.
- Use the existing application's established boolean presentation convention.

For a phone **without** a PartNumber:

- Display `N/A` for the PartNumber.
- Display `N/A` for the SIM capability information.
- Do not convert a missing PartNumber into `false`.
- Keep every existing phone-detail field and behavior working as before.

A PartNumber that exists but has `SupportsDualSim == false` or `SupportsEsim == false` is different from a missing PartNumber. Preserve the actual false values for an existing PartNumber.

### Apple ID constraint

- Apple ID details must remain unchanged.
- Do not add PartNumber or SIM fields to Apple ID details.
- Do not introduce an Apple ID PartNumber concept.
- Do not change Apple ID-specific fields, labels, routing, navigation, or behavior.

### Explicitly out of scope

Do not make changes to:

- Create Phone PartNumber selection or assignment.
- Product/Phone creation or editing behavior unrelated to this details display.
- Transactions.
- People.
- API behavior or API contracts.
- Authentication.
- PDF functionality.
- Development database initialization/seeding policy.
- Unrelated architecture cleanup or refactoring.
- Any other Stage M step.

Preserve existing routes, authorization, contracts, and behavior unless a minimal internal change is required to display the new information.

### Required tests

Add or update focused tests for the existing phone-details flow.

The tests must explicitly cover:

1. A phone with a PartNumber exposes/displays the correct PartNumber code.
2. The same phone exposes/displays the correct `SupportsDualSim` value.
3. The same phone exposes/displays the correct `SupportsEsim` value.
4. A phone with `PartNumberId == null` loads successfully.
5. A phone with `PartNumberId == null` displays `N/A` for PartNumber/SIM information.
6. An existing PartNumber with false capability values is not treated as a missing PartNumber.
7. Apple ID details remain unchanged and do not receive PartNumber/SIM fields.

Use the existing test framework, fixtures, helpers, naming conventions, and assertion style. Do not introduce broad test infrastructure for this step.

### Workflow — mandatory

- Implement **Step 4 only**.
- Do not implement another roadmap step in the same change.
- Make **one implementation commit** for Step 4.
- After that commit, **stop** and wait for Reviewer Job B.
- Do not make a second implementation commit before Reviewer Job B.
- Do not mark Step 4 complete yourself.
- Do not edit `.clinerules/chat/plan.md`, `.clinerules/chat/audit.md`, or `.clinerules/to-do.md` to record progress or completion.
- The Reviewer owns those planning/audit/checklist files.
- Do not silently change the architecture when an ambiguity is encountered; preserve the existing contract and report the ambiguity in the handoff.

### Required validation before handoff

Before handing the single implementation commit to Reviewer Job B:

- Run the focused phone-details tests.
- Run the full test suite.
- Run the full build.
- Report exact test counts: passed, failed, skipped.
- Report exact build warning/error counts.
- Verify the null-PartNumber case explicitly.
- Verify Apple ID details remain unchanged.
- Verify no API, authentication, PDF, or database-initialization-policy changes were introduced.
- Report the implementation commit SHA.
- Report the changed files.
- Do not state that Step 4 passed or is approved. Reviewer Job B determines PASS/FAIL.

### Completion gate

Step 4 is complete only after Reviewer Job B verifies the implementation and gives PASS.

Stage M is complete only after all four steps have passed Reviewer Job B and the Reviewer has performed final Stage M validation. Only then may the Reviewer update `.clinerules/to-do.md` to mark Stage M complete.

