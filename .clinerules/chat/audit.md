# Audit — Stage M — PartNumber

## Reviewer Job A

**Status: APPROVED — plan clarified.**

Repository review confirms the stage fits the current architecture:

- `Phone` currently has no PartNumber relation.
- `Model` is the appropriate parent for reusable PartNumbers.
- `PhoneConfiguration` and a new `PartNumberConfiguration` are the EF mapping points.
- `AppDbContext` uses `ApplyConfigurationsFromAssembly(...)`, so the new configuration will be discovered automatically.
- `ProductsDataService` is the existing DAL-backed Products orchestration point.
- Products list/details already flow through `IProductsDataService`.
- `CreatePhoneInputModel` currently has no PartNumber field; the create-form dropdown remains Stage N.
- Development initialization is intentionally destructive, while the Stage M production migration must be additive/non-destructive.
- API implementations/stubs remain out of scope.

### Important design clarification

**PartNumber → Model is many-to-one; Phone → PartNumber is optional; uniqueness is Model + Code.**

The original plan wording could be read as globally unique PartNumber codes while Step 2 only described duplicate handling within a Model. The plan now makes the intended constraint explicit: duplicate **Model + Code** is rejected, while the same code under another Model is permitted.

### Migration safety

Because this is the first production schema change in the stage, Job B for Step 1 must verify the actual migration path against a database that already contains a Phone. A successful build or `EnsureCreated` test alone is not sufficient evidence that the migration is production-safe.

### Step sequence

1. Schema/entity + additive migration.
2. Seed data + ProductsDataService list/create.
3. Products list filter.
4. Phone details display.

No implementation should proceed beyond Step 1 until Reviewer Job B passes Step 1.

## Constraints

- Preserve existing IDs and seed relationships.
- Do not modify API, authentication, PDF, or development initialization policy.
- Do not add the PartNumber create/dropdown UI to CreatePhone in Stage M; that is Stage N.
- Reviewer only updates `.clinerules/to-do.md` after final Stage M validation passes.
