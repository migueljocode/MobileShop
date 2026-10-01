# Plan — Stage M — PartNumber

## Planner / Reviewer Job A

Stage M is the next unchecked stage in `.clinerules/to-do.md`. It is a schema change, so implementation must be staged one step at a time: **one step → commit → Reviewer Job B → next step**.

### Design decisions

- Add a new `PartNumber` entity under `MobileShop.Models.Entities`.
- A PartNumber belongs to exactly one `Model`; a Model can have many PartNumbers.
  - Rationale: a part number such as `CH/ZAA` identifies a regional/SIM variant of a concrete device model, while multiple physical `Phone` rows can share that variant.
  - Do **not** put `PartNumberId` on `Product` or `Model`; the FK belongs on `Phone`.
- `Phone.PartNumberId` is nullable so all existing phones remain valid.
- PartNumber carries:
  - a required unique code/value;
  - `SupportsDualSim` boolean;
  - `SupportsEsim` boolean.
- Use the existing `BaseEntity` / soft-delete conventions.
- Put EF mapping in `PartNumberConfiguration` and extend `PhoneConfiguration`; register the DbSet in `AppDbContext`.
- The migration must be additive/non-destructive for production:
  - create `PartNumbers`;
  - add nullable `Phone.PartNumberId`;
  - add the FK/indexes;
  - never drop or rewrite existing phone/product rows.
- Development initialization remains destructive and continues using `sample-data.json`; preserve existing IDs and relationships while adding representative PartNumber records and assigning them to existing phone rows.

## Step 1 — Add the PartNumber schema and production-safe migration

- [ ] Step 1 — Add `PartNumber`, nullable `Phone.PartNumberId`, EF configuration/DbSet, and one additive EF migration.
- Keep the change limited to Models/DAL/migration files.
- Migration must be safe against an existing production database containing phones with no PartNumber.
- Do not change development database initialization policy.
- Add focused EF/mapping tests proving:
  - PartNumber → Model is many-to-one.
  - Phone → PartNumber is optional.
  - duplicate PartNumber codes are rejected by the configured unique constraint/index.
- Verification:
  - inspect migration Up/Down for destructive operations;
  - `dotnet build src/MobileShop.slnx --nologo`;
  - relevant DAL tests;
  - full test suite with `--no-build`;
  - inspect diff for API/auth/initialization-policy violations.
- **Reviewer Job B required before Step 2.**

## Step 2 — Add seed data and ProductsDataService part-number operations

- [ ] Step 2 — Add representative development PartNumbers, assign them to existing phone seed rows, and expose list/create operations through the existing Products data-service abstraction.
- Preserve all existing seed IDs and relationships.
- Add service methods for:
  - list PartNumbers for a Model;
  - create a PartNumber for a Model;
  - return the existing PartNumber instead of creating a duplicate when the same code already exists for that Model.
- Validate Model existence and require a non-empty code.
- Keep the API data-service path untouched; Stage J left those stubs intentionally inert.
- Add focused service tests for:
  - list by Model;
  - create;
  - duplicate reuse;
  - invalid Model;
  - empty code.
- Verification:
  - relevant service tests;
  - full build/test;
  - confirm sample-data JSON keeps existing IDs stable.
- **Reviewer Job B required before Step 3.**

## Step 3 — Add PartNumber filtering to Products

- [ ] Step 3 — Add an optional PartNumber filter to the Products list without changing the existing type filters.
- Filter semantics:
  - no PartNumber selected → existing Products behavior;
  - selected PartNumber → show only phone inventory rows whose Phone.PartNumber matches;
  - Apple IDs remain excluded from PartNumber-filtered results;
  - invalid/non-positive PartNumber IDs behave as no selection rather than causing an exception.
- Extend the Products data-service contract/implementation only as needed for the filter and dropdown options.
- Keep the existing `all` / `phone` / `appleid` routes and links working.
- Add focused PageModel/data-service regression tests.
- Verification:
  - selected filter returns only matching phones;
  - no selection preserves current results;
  - Apple IDs are not incorrectly matched;
  - existing type filters still work;
  - full build/test.
- **Reviewer Job B required before Step 4.**

## Step 4 — Show PartNumber on phone details

- [ ] Step 4 — Include the phone's PartNumber and SIM-option information on Product Details.
- Show a clear PartNumber value for phones; show `N/A` when the nullable FK is absent.
- Show the configured SIM options without changing Apple ID details.
- Add focused projection/service/UI tests for:
  - phone with PartNumber;
  - phone without PartNumber;
  - Apple ID details remain unchanged.
- Verification:
  - full build/test;
  - inspect UI diff for display-only scope;
  - no transaction/PDF/API/auth changes.
- **Reviewer Job B required before final Stage M sign-off.**

## Stage M Definition of Done

- [ ] All four implementation steps have passed Reviewer Job B.
- [ ] Production migration is additive/non-destructive and reviewed.
- [ ] Existing phone/product IDs and relationships remain stable in development seed data.
- [ ] PartNumber list/create operations are covered by focused tests.
- [ ] Products PartNumber filtering is covered by regression tests.
- [ ] Phone details show PartNumber/SIM options and handle null safely.
- [ ] Full build/test passes.
- [ ] No API, authentication, PDF, or database-initialization-policy changes.
- [ ] Reviewer gives final Stage M sign-off before `.clinerules/to-do.md` is updated.
