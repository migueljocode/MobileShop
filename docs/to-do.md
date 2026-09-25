
# MobileShop execution checklist

This file is the authoritative work order for the next act-mode agent. Follow it
literally and complete the stages in order.

## Strict rules for the agent

1. Work only on the first unchecked task whose prerequisites are complete. Do not
   skip ahead, combine unrelated stages, or redesign completed work.
2. Read the relevant existing code, tests, configuration, and seed-data structure
   before editing. Follow the repository's existing naming, layering, global-usings,
   repository, service, Razor Pages, and test conventions.
3. The scope is the development Web application only. Do not modify
   `src/MobileShop.Api`, API service stubs, API configuration, or API endpoints
   unless a later task explicitly adds that scope.
4. Do not add production authentication, authorization, cookie middleware, claims,
   login security, or other security boilerplate in these stages. Profile behavior
   is intentionally development-adapted so it can be exercised without real
   authentication.
5. Apple ID inventory passwords are intentionally stored as plaintext because the
   shop must be able to recover them for customers. Do not hash, encrypt, hide from
   the inventory workflow, or otherwise change this requirement. This does not
   change the separate hashed-password behavior for application users.
6. Preserve existing behavior outside the task. Do not perform broad refactors,
   rename public contracts unnecessarily, change database initialization policy, or
   edit generated build output, `bin`, or `obj` files.
7. Use async repository/service methods when an async equivalent already exists.
   Add async methods only when they fit the existing abstraction and can be used
   end-to-end. Use `IAsyncEnumerable` only for genuinely streaming/deferred
   collection flows; do not force it into Razor Page request handlers, small
   in-memory selections, or APIs that require materialized lists.
8. Every behavior change must have focused tests where practical. Run the smallest
   relevant test first, then run:
   `dotnet build src/MobileShop.slnx --nologo`
   and
   `dotnet test src/MobileShop.slnx --nologo`.
   Do not claim a task is complete when validation is failing.
   If a command can exceed the agent's 30-second command window, do not cancel it
   or report a timeout as a test failure: start it as an attached background
   command, keep its returned shell/session identifier, and read that same session
   until it finishes. Do not use shell `&`, `nohup`, `disown`, or detached
   processes. If the command still cannot be completed reliably, run the focused
   test project or several narrow `--filter` commands separately, then run the
   full build/test through the same attached-background procedure before completion.
9. Never silently swallow errors or return success-shaped fallbacks. Preserve the
   repository's logging and user-facing error patterns, and report missing data
   explicitly.
10. Do not delete an existing checklist item or its completion evidence. Add new
    work at the end of the appropriate stage. If a requirement changes, append a
    clarification instead of rewriting history.
11. Only after implementation and validation are complete, change that task from
    `- [ ]` to `- [x]` and wrap the complete task text in Markdown strikethrough:
    `- [x] ~~task text~~`. Add a short indented completion note immediately below
    it containing the files changed, validation performed, and any intentional
    limitation.
12. Never check a parent task while any acceptance criterion or dependent task is
    unfinished. Do not check a task merely because the page compiles or a button
    renders; verify the requested behavior.
13. If a requirement is ambiguous or implementation would conflict with these
    rules, stop before editing and report the exact conflict. Do not invent a new
    product decision.
14. After each completed task, create and persist a separate Git commit before
    starting the next task. Use a clear Conventional Commits message in the
    existing project style, for example `fix(web): remove implicit Apple ID model`
    or `feat(web): generate factors for selected transactions`. The commit must
    contain only that task's related changes, must be created only after focused
    validation passes, and must include this trailer unless the user explicitly
    says otherwise:
    `Co-authored-by: Copilot <223556219+Copilot@users.noreply.github.com>`.
15. Do not push, reset, amend, or revert commits unless the user explicitly asks
    for that operation. If unrelated pre-existing changes are present, do not
    include them in the task commit.

## Stage 1 — Remove the implicit Apple ID model input

- [x] ~~Remove the Model field from the Create Apple ID workflow. Update
  `CreateAppleIdInputModel`, `Pages/Products/CreateAppleId.cshtml`, and
  `CreateAppleId.cshtml.cs` together. Apple IDs must continue to be associated
  with the seeded Apple ID catalog category and an implicit Apple/iPhone model
  without requiring user input. Remove obsolete validation, lookup, and creation
  logic, while preserving price, email, plaintext password, notes, duplicate-email
  validation, and successful product creation.~~
  - Completed: `src/MobileShop.Models/ViewModels/Web/BindModels/CreateAppleIdInputModel.cs`,
    `src/MobileShop.Web/Pages/Products/CreateAppleId.cshtml`,
    `src/MobileShop.Web/Pages/Products/CreateAppleId.cshtml.cs`,
    `src/MobileShop.Tests/Web/Pages/Products/CreateAppleIdModelTests.cs`. Commit `2f10565`.
  - Validation: `dotnet test --filter FullyQualifiedName~CreateAppleIdModelTests` 5/5 passed;
    `dotnet build src/MobileShop.slnx` 0 errors / 0 warnings; `dotnet test src/MobileShop.slnx`
    311 passed, 2 skipped, 0 failed.
  - Notes: The implicit model is a single shared `Apple iPhone` row inside the seeded `AppleId`
    category (found or created once and reused). `grep` confirms the rendered page has no Model
    label, input, validation message, or hidden field. Remaining manual verification of the live
    page is deferred to the final validation task.

  Acceptance criteria:
  - The rendered page contains no Model label, input, validation message, or
    hidden Model field.
  - Posting valid data succeeds without a Model value.
  - Posting an existing email is still rejected.
  - The created Apple ID still has a valid product/model/category relationship.
  - Focused tests cover the changed model/handler behavior where practical.

## Stage 2 — Make transaction PDF/factor generation work for selected records

- [x] ~~Refactor transaction PDF generation so it represents real transaction data,
  not a fabricated single invoice summary. Keep the existing English/Persian PDF
  generator boundaries and create the smallest appropriate report/invoice view
  model or document composition needed for multiple transactions.~~
  - Completed: `src/MobileShop.Models/ViewModels/Web/TransactionFactorRowViewModel.cs`,
    `src/MobileShop.Models/ViewModels/Web/TransactionFactorViewModel.cs`,
    `src/MobileShop.Models/Extensions/TransactionFactorExtensions.cs`,
    `src/MobileShop.Services/PDF/IPdfGenerator.cs`,
    `src/MobileShop.Services/PDF/Configuration/QuestPdfGenerator.cs`,
    `src/MobileShop.Services/DataServices/Dal/TransactionDataService.cs`,
    `src/MobileShop.Web/GlobalUsings.cs`,
    `src/MobileShop.Web/Pages/Transactions/Index.cshtml`,
    `src/MobileShop.Web/Pages/Transactions/Index.cshtml.cs`,
    `src/MobileShop.Web/Pages/Transactions/Details.cshtml`,
    `src/MobileShop.Web/Pages/Transactions/Details.cshtml.cs`. Commit `8723671`.
  - Validation: focused tests across `IndexModelTests`, `DetailsModelTests`,
    `TransactionDataServiceTests`, `QuestPdfGeneratorTests`, and
    `TransactionFactorExtensionsTests` (20 passed, 2 skipped Persian);
    `dotnet build src/MobileShop.slnx` 0 errors / 0 warnings;
    `dotnet test src/MobileShop.Tests/MobileShop.Tests.csproj` 327 passed, 2 skipped, 0 failed.
  - Notes: Factor includes date, direction, product, price, and relevant party
    (Customer on sales, Seller on purchases). Print returns inline printable PDF,
    Download returns named attachment `transactions-factor-YYYYMMdd-HHmmss.pdf`.
    Multi-select works via checkboxes, details page provides single-record factor,
    missing IDs trigger validation and redisplay without PDF emission, empty
    selection falls back to direction/count/order filters. `MobileShop.Api` untouched.

- [x] ~~Add focused tests for the transaction selection, filtering, person association,
  PDF data composition, empty selection behavior, and PDF response metadata. Test
  the actual required output shape/data rather than only checking that a byte array
  is non-empty.~~
  - Completed: `src/MobileShop.Tests/Models/Extensions/TransactionFactorExtensionsTests.cs`,
    `src/MobileShop.Tests/Services/DataServices/Dal/TransactionDataServiceTests.cs`,
    `src/MobileShop.Tests/PDF/QuestPdfGeneratorTests.cs`,
    `src/MobileShop.Tests/Web/Pages/Transactions/DetailsModelTests.cs`,
    `src/MobileShop.Tests/Web/Pages/Transactions/IndexModelTests.cs`,
    `src/MobileShop.Tests/Dal/BaseClass/TestDataHelpers.cs`,
    `src/MobileShop.Tests/GlobalUsings.cs`. Commit `8723671`.
  - Validation: 20 focused tests verify factor row mappings, total price calculation,
    PDF generator input shape, deduplication, unknown-ID error reporting, filter fallback,
    single-transaction factor, printable vs download responses, and validation redisplay.
  - Notes: Tests inspect the captured `TransactionFactorViewModel` and response metadata
    directly instead of only asserting non-empty byte arrays. All 327 tests pass.

## Stage 3 — Fix the development profile page UX and behavior

- [x] ~~Polish `/Account/Profile` for development mode without adding authentication
  middleware or security boilerplate. Use the seeded development admin explicitly
  and consistently instead of relying on an unavailable authenticated identity.
  Fix malformed markup, improve layout and labels, preserve validation errors,
  provide clear success/error feedback, and make the password-change test flow
  actually call the existing user data-service change method.~~

  Acceptance criteria:
  - GET reliably displays the development admin profile.
  - The page does not depend on `User.Identity.Name` being populated.
  - Current-password validation, new-password confirmation, and required input
    validation are clear and preserved after redisplay.
  - A valid development password change persists and can be verified through the
    existing user data service.
  - The page clearly states that this is development-only behavior without implying
    production authentication exists.
  - No cookie authentication, claims, authorization attributes, or API changes are
    introduced.
  - Profile markup is valid and responsive using the existing site styling.

  - Completion note:
  - Changes: `src/MobileShop.Web/Pages/Account/Profile.cshtml` (fixed both `dd`/`dt`
    markup mismatches — Username and Email rows now close with `</dd>`, added
    `asp-validation-summary="All"`, success-message alert, dev-only
    footnote, `_validationScriptsPartial`), `src/MobileShop.Web/Pages/Account/ProfileModel.cs`
    (GET resolves seeded dev admin via `FindByUsernameAsync("admin")` — no
    `User.Identity.Name`; POST validates required/length/confirmation attributes,
    verifies current password via `ValidateCredentialsAsync`, calls
    `ChangePasswordAsync` for persistence, sets success message and clears fields;
    removed unused `IPasswordHasher` and placeholder messaging).
  - Validation: build 0 warnings/0 errors; 6 focused `ProfileModelTests` pass
    (GET resolves admin, successful change with message, invalid current password,
    mismatched confirmation, short password, missing password); full suite run
    through a persistent session until completion: 333 passed, 2 skipped, 0 failed.
  - Notes: No cookie auth, claims, authorization attributes, or API changes
    introduced. Password hashing remains owned by the data service.

- [x] ~~Add or update focused profile/user-service tests for the successful change,
  invalid current password, mismatched confirmation, and redisplay behavior.~~

  - Completion note:
  - Changes: `src/MobileShop.Tests/Web/Pages/Account/ProfileModelTests.cs` (new, 6 tests
    using Moq `IUserDataService`; catch-all mock setups registered before specific
    setups so exact-argument setups win; success test explicitly verifies
    `ChangePasswordAsync("admin", "NewPass123")` was called exactly once).
  - Validation: `--filter FullyQualifiedName~ProfileModelTests` → 6/6 passed.
  - Notes: Attribute-driven validation is asserted through a `ValidateModel` helper
    that runs `Validator.TryValidateProperty` into `ModelState`, mirroring MVC model
    binding since page-model unit tests bypass the binder.

## Stage 4 — Expand realistic development seed data

- [x] ~~Expand `src/MobileShop.Dal/Initialization/sample-data.json` using the
  existing loader/schema. Add realistic but deterministic data covering multiple
  manufacturers and models, phone colors/specifications, Apple IDs, customers,
  sellers, employees, guarantees, bought and sold products, and varied
  transactions suitable for inventory, profile, profit/loss, filtering, and PDF
  testing.~~

  Rules for seed data:
  - Respect every configured relationship, required field, enum, unique index, and
    foreign key.
  - Keep passwords for Apple ID inventory records plaintext as required.
  - Keep application-user password hashes in the existing hashed format.
  - Use stable IDs/references and values that can be loaded repeatedly by the
    development initializer.
  - Do not put secrets, real people's personal data, or production credentials in
    the file.
  - Do not change destructive development initialization or production behavior.

  Acceptance criteria:
  - Development startup can recreate and seed the database successfully.
  - All relevant pages have enough data to exercise empty, single, filtered, and
    multi-record cases.
  - Seed-data loading has focused coverage or validation proving relationships
    and required records are valid.

  - Completion note:
  - Changes: `src/MobileShop.Dal/Initialization/sample-data.json` expanded from
    5 to 17 products and 5 to 26 transactions (14 Buy / 12 Sell, total profit
    +6,230,000 so distribution shows a profitable period): 7 people (3 suppliers /
    customers + inactive employee), 3 sellers (Real + Legal), 4 customers,
    6 manufacturers, 7 categories (Phone/Charger/Glass/AppleId/Cable/Case/PowerBank),
    6 colors, 4 storage capacities, 16 models incl. implicit AppleId `iPhone`
    model (matches Stage 1's find-or-create), 3 Apple IDs (plaintext passwords,
    sold + unsold states), 7 phones (unique IMEIs, dual-SIM, ownership-transferred),
    4 guarantees (3 active + 1 expired for date filtering), 7 device specs,
    2 second-hand units (sold + available), 2 cables, 2 chargers, 1 power bank,
    1 case + fit, 2 glasses + fits, 4 employees (3 active summing to exactly 100%
    + 1 inactive excluded from distribution). All sentinel rules hold: seller 1
    and customer 1 are person 1 (shop), every Buy has customerId 1, every Sell has
    sellerId 1. `src/MobileShop.Tests/Dal/Initialization/SampleDataSeedTests.cs`
    (new, 6 tests against a real SQLite database via the literal
    `DatabaseInitializer.InitializeForDevelopment` entry point).
  - Validation: a standalone validation script checked every FK target, unique
    index (barcode/username/manufacturer+model/GB/IMEI/fit pairs), required field,
    enum value, string length bound, date range, and sentinel rule before the
    commit; build 0 warnings/0 errors; focused `SampleDataSeedTests` 6/6 passed;
    full suite run through a persistent session until completion: 339 passed,
    2 skipped, 0 failed.
  - Notes: dev startup recreate+seed is proven by the test invoking
    `DatabaseInitializer.InitializeForDevelopment` (EnsureDeleted → EnsureCreated →
    SeedIfEmpty) on SQLite; idempotency proven by re-running SeedIfEmpty (no-op)
    and `ClearAndReseedDatabase` on a fresh context (matching how scoped dev
    tooling calls it - the initializer's destructive path is unchanged). Loader and
    initializer code untouched. The pre-existing `appleInfos` array remains
    intentionally unused: the existing loader has no `AppleInfos` collection, and
    extending it would change initialization behavior beyond this stage's scope.
    Seed contains no secrets, real personal data, or production credentials
    (example.com emails, sequential 0912xxxxxxxx numbers, fake national IDs).

## Stage 5 — Async consistency and performance review

- [ ] Audit the Web-used DAL/service call paths after Stages 1–4. Replace avoidable
  synchronous database calls with the existing async repository/service variants,
  carrying cancellation where the current project patterns support it. Keep
  business behavior and transaction boundaries unchanged.

- [ ] Introduce `IAsyncEnumerable` only where it provides measurable deferred or
  streaming benefit and the full call chain can consume it safely. Materialize
  data deliberately at Razor Page/PDF boundaries where rendering requires a
  stable snapshot. Do not convert every method mechanically.

  Acceptance criteria:
  - No sync-over-async or fake async wrappers are introduced.
  - Query results used by PDF generation are consistent for one request.
  - Existing synchronous public contracts remain only where compatibility requires
    them, with documented rationale.
  - Focused tests cover the changed async behavior and cancellation/error paths.

## Final validation

- [ ] Run the full solution build and test commands from the strict rules, inspect
  failures rather than masking them, and verify the development Web flows manually:
  Create Apple ID, Transactions filtering, single transaction factor, multi-select
  factor, Print, Download PDF, Profile GET, and development password change.
- [ ] Review the final diff for accidental API changes, generated files, secrets,
  plaintext application-user passwords, unchecked completed tasks, or checklist
  formatting violations. Only then mark this final task complete.

## Checklist completion format

Every completed item must look like this:

```text
- [x] ~~Implement the completed task text.~~
  - Completed: `path/to/file.cs`, `path/to/test.cs`.
  - Validation: `dotnet test ...`; behavior manually verified.
  - Notes: intentional limitations or follow-up, if any.
```

Never use `- [x]` without `~~...~~`, never leave completed text unstruck, and
never remove the completion note.
