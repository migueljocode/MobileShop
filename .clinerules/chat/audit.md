# Audit — Stage M — PartNumber

## Reviewer Reopening

**Status: REQUIRES REPLANNING — previous Stage M sign-off is revoked for the requested UX correction.**

The previous Step 1–4 implementation work is not being discarded. The stage is reopened because the delivered Products UX does not satisfy the owner's requested behavior, and the missing Create Phone PartNumber UX is now explicitly required for Stage M completion.

### Confirmed current gaps

1. **Products PartNumber selector**
   - Current Products/Index.cshtml renders the PartNumber selector for All, Phones, and Apple IDs.
   - It includes a visible Filter button.
   - The selector is populated from GetPartNumbersAsync() globally rather than from PartNumbers actually available in current phone inventory.
2. **Automatic filtering**
   - PartNumber selection currently requires form submission through the Filter button.
   - Required behavior is immediate application when the selected PartNumber changes.
3. **Products list**
   - ProductListItemViewModel currently has no PartNumber field.
   - Products table therefore has no Part number column.
4. **Create Phone**
   - CreatePhone.cshtml has Manufacturer/Model/Color/etc. selectors with Add New controls, but no PartNumber selector.
   - CreatePhoneInputModel has no PartNumberId.
   - CreatePhoneModel does not load/create PartNumbers for the selected Model.
   - CreatePhoneAsync does not persist a selected PartNumber.
5. **Previous sign-off**
   - Previous audit incorrectly marked Stage M complete because it validated the earlier implementation against an incomplete interpretation of the requested UX.
   - to-do.md must therefore be reopened and must not remain checked.

### Corrective scope

The rewritten plan.md is now the sole Actor handoff for the reopened Stage M and contains:
- exact files/symbols;
- current → desired behavior;
- Products phone-only automatic filtering;
- inventory-derived PartNumber options;
- PartNumber table column;
- Model-scoped Create Phone PartNumber combobox;
- Add New PartNumber modal flow;
- persistence and null behavior;
- focused tests;
- one-step/one-commit/Job-B gates;
- full final validation;
- honest Risk and Confidence ratings.

### Workflow

- Stage M is currently **open**.
- Step 1 is the active Actor assignment.
- Actor must implement Step 1 only, commit once, then stop for Job B.
- Actor must not modify plan.md, audit.md, or to-do.md.
- Reviewer owns those files.
- Stage M remains unchecked until final validation passes.

## Reviewer Gate

**Current gate: Step 1 pending implementation and Job B.**

No Stage M completion claim is valid until the reopened requirements in plan.md are implemented and reviewed.