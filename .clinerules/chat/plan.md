# Plan — Stage M — PartNumber

## Reviewer Briefing

- The previous Stage M sign-off is reopened because the Products UX does not satisfy the requested behavior; the prior PASS must not be treated as completion evidence for this correction.
- Step 1 is the main UI/data-flow correction: the PartNumber selector is phone-only, applies immediately without a Filter button, and its options come from the available phone inventory rather than every PartNumber in the database.
- Step 2 covers the missing Create Phone PartNumber selector and Add New flow explicitly requested by the owner. This overlaps the existing Stage N roadmap wording, but is included here because the owner has now made it a Stage M completion requirement; Stage N must not duplicate the implementation later.
- Every implementation step is one commit followed by Reviewer Job B. The Actor must not edit plan.md, audit.md, or to-do.md.
- The final step is the only point at which full Stage M validation and final sign-off are allowed.

## [ ] Step 1 — Correct Products PartNumber filtering and inventory display

- Files
  - Inspect/modify:
    - src/MobileShop.Web/Pages/Products/Index.cshtml — Products filter controls and inventory table.
    - src/MobileShop.Web/Pages/Products/Index.cshtml.cs — selected type/PartNumber state and option population.
    - src/MobileShop.Services/DataServices/Dal/ProductsDataService.cs — inventory projection and PartNumber option query.
    - src/MobileShop.Models/ViewModels/Web/ProductListItemViewModel.cs — PartNumber value exposed to the table.
    - The existing IProductsDataService contract file wherever it is actually located; do not invent a second service contract.
  - Tests:
    - src/MobileShop.Tests/Services/DataServices/Dal/ProductsDataServiceTests.cs
    - Existing Products web/Page tests if present; inspect before adding coverage.
  - Do not touch API files, authentication, PDF code, migrations/schema, or unrelated product pages.

- Symbols / behavior
  - Products.IndexModel.OnGetAsync(...)
  - ProductsDataService.GetInventoryRowsAsync(...)
  - ProductsDataService.GetPartNumbersAsync(...) or a narrowly scoped replacement/addition if the existing method cannot express the required inventory-derived options.
  - ProductListItemViewModel
  - Products/Index.cshtml

- Current -> desired
  - Current: PartNumber selector is shown for every Products type, has a visible Filter button, loads all PartNumbers globally, and the table has no PartNumber column.
  - Desired:
    1. Render the PartNumber selector only when Phones is the selected product type. It must not appear for All or Apple IDs.
    2. Remove the Filter submit button completely.
    3. Selecting a PartNumber automatically submits/applies the GET filter. Do not require a second click.
    4. Selecting All part numbers automatically removes the PartNumber restriction.
    5. Existing All / Phones / Apple IDs type navigation must continue preserving the selected PartNumber only where that selection is meaningful; switching away from Phones must clear/ignore the PartNumber filter rather than filtering Apple IDs.
    6. PartNumber selector options must represent the PartNumbers available in the current phone inventory list, not every PartNumber row in the database. To prevent a selected PartNumber from collapsing the choices to itself, derive options from the current phone inventory with the PartNumber restriction omitted, then preserve the current selection if it is valid.
    7. Only active/non-deleted PartNumbers attached to currently available phone inventory may appear. Do not show unused catalog PartNumbers.
    8. Keep a leading All part numbers option.
    9. Add a Part number column to the Products table.
    10. For a phone with a PartNumber, show its code.
    11. For a phone without a PartNumber, show N/A.
    12. Apple ID rows remain supported by existing All/Apple ID routes and show N/A in the Part number column; Apple IDs must never become eligible for the PartNumber filter.
    13. Keep existing Details links, type routes, ordering, sold/second-hand status, and other columns unchanged.

- Implementation constraints
  - Reuse the existing Phone.PartNumberNavigation relationship.
  - Do not introduce a second database relationship or a client-side hard-coded PartNumber list.
  - Keep filtering in the Products data/service layer; the Razor page only owns presentation and automatic-submit interaction.
  - Use the existing repository/service abstraction and async methods.
  - Do not turn the Products page into an API-driven page just for this filter.
  - Treat null/zero/negative PartNumber query values as no selection.
  - A stale positive PartNumber ID that is not available in current phone inventory must not produce an inconsistent selector state; treat it as no selection.
  - Preserve existing type behavior for all, phone, and appleid.

- Tests
  - PartNumber selector data contains only PartNumbers attached to current phone inventory.
  - An unused PartNumber is excluded from selector options.
  - A selected valid PartNumber filters phones correctly.
  - Apple IDs are excluded when a PartNumber is selected.
  - null, zero, and negative PartNumber values mean no filter.
  - A phone with a PartNumber exposes that code in the list projection.
  - A phone without a PartNumber exposes the expected missing value so the UI can render N/A.
  - Existing All/Phone/Apple ID routing remains unchanged.
  - Add Page/Razor test coverage for visibility/automatic-submit markup if the repository already has an established page-test mechanism; otherwise keep service tests authoritative and verify rendered markup during Job B.

- Verify
  - Run focused ProductsDataService tests covering the above cases.
  - Full build/test is deferred to the final step except when needed to establish compilation.

- Done when
  - Products page behavior exactly matches the requested filter interaction and list display.
  - No visible Filter button remains.
  - Selector is phone-only and auto-applies.
  - Options reflect available phone inventory.
  - Part number is a visible list column.
  - Job B passes.

- Risk: MEDIUM
- Confidence: HIGH

## [ ] Step 2 — Add the missing PartNumber selector and Add New flow to Create Phone

- Files
  - Inspect/modify:
    - src/MobileShop.Web/Pages/Products/CreatePhone.cshtml
    - src/MobileShop.Web/Pages/Products/CreatePhone.cshtml.cs
    - src/MobileShop.Models/ViewModels/Web/BindModels/CreatePhoneInputModel.cs
    - src/MobileShop.Services/DataServices/Dal/ProductsDataService.cs
    - The existing IProductsDataService contract file wherever it is actually located.
    - Existing Create Phone tests and/or ProductsDataService tests covering CreatePhoneAsync.
  - Use the existing Bootstrap/modal and AJAX patterns already present on Create Phone.
  - Do not create a parallel PartNumber management page.

- Symbols / behavior
  - CreatePhoneInputModel
  - CreatePhoneModel.PopulateDropdownsAsync()
  - CreatePhoneModel.OnGet... handlers for model-dependent dropdowns.
  - CreatePhoneModel.OnPost... handlers for existing Add New controls.
  - ProductsDataService.GetPartNumbersAsync(...)
  - ProductsDataService.CreatePartNumberAsync(...)
  - ProductsDataService.CreatePhoneAsync(...)

- Current -> desired
  - Current: Create Phone has Manufacturer, Model, Color and other fields, but no PartNumber selection and no way to add a missing PartNumber from the form.
  - Desired:
    1. Add nullable PartNumberId to the existing Create Phone input model.
    2. Show a PartNumber combobox/select beside an Add New button.
    3. PartNumber options must be scoped to the currently selected Model; never show PartNumbers belonging to another Model.
    4. When Manufacturer changes, the existing model reload must also reset/reload the PartNumber selector.
    5. When Model changes, load that Model's current PartNumbers.
    6. If no PartNumbers exist for the selected Model, keep a clear -- Select part number -- state and keep Add New available.
    7. Add New must be disabled or reject cleanly until a Model is selected; never create a PartNumber without a Model.
    8. Add New opens the existing modal pattern and collects PartNumber code, Dual SIM capability, and eSIM capability.
    9. Saving a new PartNumber reuses CreatePartNumberAsync(modelId, code, supportsDualSim, supportsEsim).
    10. On success, insert/select the returned PartNumber immediately in the combobox without a page reload.
    11. Duplicate Model + Code reuses the existing PartNumber according to the established service behavior and selects it.
    12. Blank code, unknown Model, and invalid requests return the existing structured error response and leave the form usable.
    13. Submitting Create Phone persists the selected nullable PartNumberId on the new Phone.
    14. Leaving PartNumber unselected remains valid and creates a phone with PartNumberId == null.
    15. Do not change IMEI, manufacturer/model, color, guarantee, second-hand, price, or Apple ID behavior except where required to add this field.
    16. Keep PartNumber capability data as stored booleans; do not invent additional phone-level SIM fields.

- Implementation constraints
  - Reuse existing PartNumber list/create service methods from Stage M.
  - Do not add a new entity or migration.
  - Do not add PartNumber handling to Create Apple ID.
  - Use the existing Create Phone inline JavaScript/modal pattern; do not add a new frontend framework.
  - Preserve server-side validation and existing error behavior.
  - Keep the PartNumber relationship nullable.

- Tests
  - PartNumber can be omitted.
  - PartNumber is listed only for the selected Model.
  - Changing Model refreshes PartNumber options.
  - Add New creates/selects a PartNumber.
  - CreatePhone with valid PartNumber persists Phone.PartNumberId.
  - CreatePhone without PartNumber persists null.
  - CreatePhone with PartNumber from another Model is rejected.
  - Existing PartNumber create/reuse behavior remains intact.
  - Do not add API tests or API implementation.

- Verify
  - Run focused Create Phone / ProductsDataService tests.
  - Confirm existing Add New controls still work after the PartNumber addition.

- Done when
  - User can select an existing PartNumber for the selected Model.
  - User can create a missing PartNumber from the same form and have it immediately selected.
  - Selected PartNumber persists on the new Phone.
  - Leaving it empty remains valid.
  - Job B passes.

- Risk: HIGH
- Confidence: MEDIUM

## [ ] Step 3 — Final Stage M regression validation and reviewer sign-off

- Files: no production implementation changes expected. Only reviewer-owned .clinerules/chat/plan.md, .clinerules/chat/audit.md, and finally .clinerules/to-do.md may be updated by the Reviewer after validation. The Actor must not edit these files.

- Required validation
  - Run focused tests for Steps 1–2.
  - Run dotnet build src/MobileShop.slnx --nologo.
  - Run dotnet test src/MobileShop.slnx --nologo --no-build.
  - Record exact passed/failed/skipped counts and exact build warning/error counts.
  - Verify Products: All selected means no PartNumber selector; Phones selected means selector visible; Apple IDs selected means selector absent; selecting a PartNumber applies immediately with no Filter button; selector options correspond to PartNumbers available in phone inventory; PartNumber column is present and correct.
  - Verify Create Phone: Model-scoped PartNumber selector; Add New works when desired PartNumber is absent; newly created PartNumber becomes selected; selected PartNumber persists to Phone; empty selection remains valid.
  - Verify existing phone-details PartNumber/SIM display remains green, including null PartNumber N/A and unchanged Apple ID details.
  - Verify no API, authentication, PDF, database-initialization-policy, migration, or unrelated architecture changes were introduced.
  - Review changed-file list for scope violations.
  - Record every implementation commit SHA.

- Workflow gate
  - Each implementation step is exactly one implementation commit.
  - After each implementation commit, Actor stops and waits for Reviewer Job B.
  - Reviewer Job B must PASS before the next step begins.
  - Actor must not edit plan.md, audit.md, or to-do.md.
  - Reviewer owns progress/checklist/sign-off files.
  - Do not mark Stage M complete until final validation passes.

- Done when
  - Steps 1 and 2 each pass Job B.
  - Full build/test validation is green.
  - All requested Products and Create Phone PartNumber UX requirements are verified.
  - Reviewer writes final PASS to audit.md.
  - Only then may Reviewer mark Stage M complete in to-do.md.

- Risk: HIGH
- Confidence: HIGH

## Global Definition of Done

- Products PartNumber filtering is phone-only and automatically applied on selection.
- Products page has no PartNumber Filter button.
- PartNumber options represent PartNumbers actually available in the current phone inventory context.
- Products list includes a Part number column.
- Create Phone has a Model-scoped PartNumber combobox with Add New.
- Add New can create a PartNumber with code, Dual SIM, and eSIM values and immediately select it.
- A selected PartNumber persists to the created Phone; no selection remains valid.
- Existing phone-details PartNumber/SIM display remains correct.
- Existing All/Phone/Apple ID routing remains correct.
- No API/authentication/PDF/database-initialization-policy changes.
- No destructive migration or schema changes are introduced by these corrective UX steps.
- Focused tests and full build/test pass with exact evidence recorded.
- Reviewer Job B has passed every implementation step.
- Reviewer performs final Stage M validation before changing to-do.md.
- No known unresolved issues remain.

## Execution notes

Actor: implement only the currently approved step. Make one implementation commit, stop, and hand it to Reviewer Job B. Do not edit plan.md, audit.md, or to-do.md. Keep changes compact and within the named symbols/files; do not perform unrelated refactors. Use the repository's existing Razor, service, repository, Bootstrap/modal, and xUnit conventions. Report changed files, commit SHA, focused validation, and any ambiguity without silently changing scope.