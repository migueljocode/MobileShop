# Audit — Stage M — PartNumber

## Reviewer Job B — Step 3

**Status: PASS — Step 3 approved.**

Steps 1–3 are complete. Step 4 is now the active Actor assignment.

### Step 4 handoff

The active plan has been expanded into an executable specification so the Actor should not need to infer the intended behavior.

The required result is limited to the existing phone-details flow:

- Display PartNumber code plus Dual SIM and eSIM capabilities when a PartNumber exists.
- Safely handle a null PartNumber and show N/A rather than false/empty capability values.
- Preserve the distinction between a real PartNumber with false capabilities and no PartNumber.
- Leave Apple ID details unchanged.
- Do not add PartNumber selection to Create Phone; that belongs to Stage N.
- Reuse existing architecture and EF navigation rather than introducing duplicate lookup mechanisms.
- Add focused regression coverage for populated PartNumber, null PartNumber, false capabilities, and unchanged Apple ID details.
- Run focused tests, the full suite, and the full build before handoff.
- Make one implementation commit, then stop for Reviewer Job B.
- The Actor must not edit plan.md, audit.md, or to-do.md to mark progress.

### Previously verified Stage M state

- Step 1 PASS: additive PartNumber schema/migration and existing-phone migration safety verified.
- Step 2 PASS: seed data and PartNumber list/create operations verified.
- Step 3 PASS: Products PartNumber filtering and existing type routing verified.
- Latest Step 3 validation: 47 focused ProductsDataService tests passed; full suite 267 passed; build 0 warnings / 0 errors.
- No API behavior, authentication, PDF, or database-initialization-policy changes were introduced by Step 3.

### Workflow history

The Actor edited .clinerules/chat/plan.md in implementation commit 2c58380bf710c9361cc76138aa5fe4e6b5e47a59 to mark Step 3 complete before Reviewer Job B. This remains recorded as a workflow violation; no implementation rework was required because Step 3 passed review.

### Gate

**Step 3 PASS. Step 4 may begin.**

No .clinerules/to-do.md change is made until the entire Stage M Definition of Done has passed final Reviewer validation.
