# Audit — Stage M — PartNumber

## Reviewer Job A

**Status: APPROVED — plan only.**

Repository review confirms the current design supports this stage cleanly:

- `Phone` currently has no PartNumber relation.
- `Model` already owns the device model identity and is the appropriate parent for reusable PartNumbers.
- `PhoneConfiguration` and `ModelConfiguration` are the existing EF relationship-mapping points.
- `AppDbContext` centralizes DbSets and applies configurations from the Models assembly.
- `ProductsDataService` is the existing DAL-backed Products orchestration point.
- Products list/details already flow through `IProductsDataService`.
- `CreatePhoneInputModel` currently has no PartNumber field; Stage M therefore does not add the phone-form dropdown. That belongs to Stage N as specified by the roadmap.
- Development seeding is intentionally destructive, while the new production migration must remain additive/non-destructive.
- API implementations/stubs remain out of scope.

### Important design choice

**PartNumber → Model is many-to-one; Phone → PartNumber is optional.**

This keeps one regional/SIM variant reusable across many physical phones while allowing existing phones to remain without a PartNumber.

### Step sequence

1. Schema/entity + additive migration.
2. Seed data + ProductsDataService list/create.
3. Products list filter.
4. Phone details display.

No implementation should begin until the Actor follows this Step 1 plan and waits for Reviewer Job B before continuing.

## Constraints

- Preserve existing IDs and seed relationships.
- Do not modify API, authentication, PDF, or development initialization policy.
- Do not add the PartNumber create/dropdown UI to CreatePhone in Stage M; that is Stage N.
- Reviewer only updates `.clinerules/to-do.md` after final Stage M validation passes.
