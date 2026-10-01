# Plan — Stage I — Naming & structure cleanup (no behaviour change)

## Reviewer Briefing

- Step 1 is a low-behaviour-risk rename, but every concrete PasswordHasher consumer must be retargeted while the public IPasswordHasher contract stays unchanged.
- Step 2 is a test-only namespace/path move; the risk is limited to test discovery, namespace references, and accidental changes to the PDF implementation.
- Step 3 is the highest-risk step because it changes the physical and namespace location of the surviving generic repository abstraction used across DAL, Services, and Tests. Preserve the IBaseRepo<T> contract and DI registration exactly.
- The current repository search shows the generic repository implementation is concentrated in src/MobileShop.Dal/Repos/Base/, with consumers using the MobileShop.Dal.Repos.Base global using. Do not infer or redesign repository behavior during the move.
- Stage I is cleanup only: no API changes, no database/schema changes, no behavior changes, no dependency changes, and no changes to authentication or database initialization policy.

## ~~[x] Step 1 — Rename PasswordHasher to ArgonPasswordHasher~~

- Files: inspect src/MobileShop.Services/Security/PasswordHasher.cs, src/MobileShop.Services/Security/IPasswordHasher.cs, src/MobileShop.Services/ServiceCollectionExtensions.cs, src/MobileShop.Tests/Services/Security/PasswordHasherTests.cs, and src/MobileShop.Tests/Services/DataServices/Dal/AccountDataServiceTests.cs; modify/rename only the concrete hasher and its direct tests/consumers; do not touch IPasswordHasher.
- Symbols: PasswordHasher, IPasswordHasher, ServiceCollectionExtensions.AddMobileShopSecurity(), PasswordHasherTests, and the AccountDataServiceTests _hasher field.
- Current -> Desired:
  - PasswordHasher.cs / PasswordHasher -> ArgonPasswordHasher.cs / ArgonPasswordHasher.
  - PasswordHasherTests.cs / PasswordHasherTests -> ArgonPasswordHasherTests.cs / ArgonPasswordHasherTests.
  - DI changes only from IPasswordHasher, PasswordHasher to IPasswordHasher, ArgonPasswordHasher.
  - Existing concrete construction in tests changes to ArgonPasswordHasher.
  - IPasswordHasher remains the public abstraction and its Hash / Verify members remain unchanged.
- Change: rename the concrete class/file and all direct references; preserve the existing Argon2 implementation byte-for-byte in behavior. Do not rename or alter IPasswordHasher.
- Depends on: none.
- Edge cases / error handling: preserve hashing salt behavior, successful verification, failed verification, garbage-hash behavior, and all existing test inputs. Do not alter Argon2 configuration or password handling.
- Tests: rename/retarget the existing security tests; no new behavior is required.
- Verify: dotnet test src/MobileShop.Tests/MobileShop.Tests.csproj --nologo --filter "FullyQualifiedName~Services.Security.ArgonPasswordHasherTests|FullyQualifiedName~Services.DataServices.Dal.AccountDataServiceTests"
- Done when: no production/test reference to the old concrete PasswordHasher remains except intentional historical text in planning documentation; IPasswordHasher is unchanged; targeted tests pass.
- Risk: LOW
- Confidence: HIGH

## ~~[x] Step 2 — Move PDF tests under Services.PDF~~

- Files: rename src/MobileShop.Tests/PDF/QuestPdfGeneratorTests.cs to src/MobileShop.Tests/Services/PDF/QuestPdfGeneratorTests.cs; modify its namespace only as needed; do not modify QuestPdfGenerator, PDF production code, PDF configuration, or the skipped-test behavior.
- Symbols: MobileShop.Tests.PDF.QuestPdfGeneratorTests -> MobileShop.Tests.Services.PDF.QuestPdfGeneratorTests.
- Current -> Desired: the test file and namespace should reflect the existing production service namespace MobileShop.Services.PDF, while remaining under the test project's Services hierarchy.
- Change: perform a path/namespace move only. Preserve all test bodies, test names, assertions, fixtures, and skip attributes exactly.
- Depends on: none; independent of Step 1.
- Edge cases / error handling: ensure xUnit still discovers every test after the namespace/path change. Do not use this step to fix or unskip the Persian tests; those belong to Stage K.
- Tests: the same six PDF tests must remain present, including the two intentionally skipped Persian tests.
- Verify: dotnet test src/MobileShop.Tests/MobileShop.Tests.csproj --nologo --filter "FullyQualifiedName~Services.PDF.QuestPdfGeneratorTests"
- Done when: the old MobileShop.Tests.PDF namespace/path is gone, the new MobileShop.Tests.Services.PDF test class is discovered, and the test count/results match the pre-move behavior.
- Risk: LOW
- Confidence: HIGH

## ~~[x] Step 3 — Flatten and singularize the generic repository namespace~~

- Files: rename/move src/MobileShop.Dal/Repos/Base/BaseRepo.cs -> src/MobileShop.Dal/Repo/BaseRepo.cs and src/MobileShop.Dal/Repos/Base/IBaseRepo.cs -> src/MobileShop.Dal/Repo/IBaseRepo.cs; modify src/MobileShop.Dal/GlobalUsings.cs, src/MobileShop.Services/GlobalUsings.cs, src/MobileShop.Tests/GlobalUsings.cs, src/MobileShop.Services/ServiceCollectionExtensions.cs, and any other actual source/test reference found by the final repository-wide search; update repository tests that directly name the old namespace/path if any are found.
- Symbols: MobileShop.Dal.Repos.Base.BaseRepo<T>, MobileShop.Dal.Repos.Base.IBaseRepo<T>, BaseRepo<T>, IBaseRepo<T>, ServiceCollectionExtensions.AddMobileShopRepository(), BaseRepoTests<TEntity,TRepo>, and BaseRepoPersonTests.
- Current -> Desired:
  - namespace MobileShop.Dal.Repos.Base -> MobileShop.Dal.Repo.
  - directory src/MobileShop.Dal/Repos/Base -> src/MobileShop.Dal/Repo.
  - all imports/references of the old namespace -> MobileShop.Dal.Repo.
  - the generic DI registration remains IBaseRepo<> -> BaseRepo<>; only its namespace resolution changes.
- Change: move the two generic repository files, change their file-scoped namespace to MobileShop.Dal.Repo, retarget the affected GlobalUsings and tests, and leave every repository method and contract unchanged. The Repos directory/namespace is eliminated in favor of the singular Repo; there must not be a second compatibility namespace or forwarding type.
- Depends on: Step 1 and Step 2 are independent; Step 3 can follow either one. Do not delete or redesign the generic repository abstraction.
- Edge cases / error handling:
  - Preserve IBaseRepo<T> method signatures, soft-delete behavior, synchronous/asynchronous behavior, projections, ordering, and persistence semantics.
  - Preserve AddMobileShopRepository() registration lifetime and open-generic mapping.
  - Do not touch the deleted specialized-repository history from Stage H or introduce any specialized repositories.
  - Do not modify src/MobileShop.Api.
  - Do not modify database initialization, EF configuration, schema, migrations, authentication, or application behavior.
  - Search all src/ source and test files for MobileShop.Dal.Repos.Base and MobileShop.Dal.Repos before finishing. Current repository inspection identifies old Base imports in the DAL, Services, and Tests GlobalUsings; update every real occurrence rather than relying only on that current list.
- Tests: preserve and run BaseRepoTests<TEntity,TRepo> and BaseRepoPersonTests; no new repository behavior tests are needed because this is a namespace/path move.
- Verify: dotnet build src/MobileShop.slnx --nologo && dotnet test src/MobileShop.slnx --nologo --no-build
- Done when: the old src/MobileShop.Dal/Repos/Base path and MobileShop.Dal.Repos.Base namespace are gone; BaseRepo<T> and IBaseRepo<T> exist only under MobileShop.Dal.Repo; DI and all consumers compile; repository tests pass; no unrelated source changes exist.
- Risk: HIGH
- Confidence: MEDIUM

## Global Definition of Done

- Stage I contains only naming, namespace, and test-structure cleanup; runtime behavior is unchanged.
- PasswordHasher is renamed to ArgonPasswordHasher everywhere required while IPasswordHasher remains unchanged.
- The PDF test namespace/path is MobileShop.Tests.Services.PDF; the existing skipped Persian tests remain skipped until Stage K.
- BaseRepo<T> and IBaseRepo<T> live directly under MobileShop.Dal.Repo; the old MobileShop.Dal.Repos.Base namespace/path is gone.
- The generic repository DI registration remains equivalent and all repository consumers/tests resolve the new namespace.
- No specialized repository/entity-service/API cleanup is performed beyond the already-completed Stage H state.
- src/MobileShop.Api is untouched.
- No database/schema/migration, initialization, authentication, package, or configuration behavior changes are introduced.
- The final step's full validation passes: dotnet build src/MobileShop.slnx --nologo && dotnet test src/MobileShop.slnx --nologo --no-build.
- Final repository searches show no stale old concrete hasher, PDF test namespace/path, or generic repository namespace references.
- The final diff contains only Stage I files and the plan/report files required by the workflow.

## Execution notes

- Implement exactly one step, verify it, commit it, and stop for Job B review before the next step.
- Use filesystem-aware rename/move operations so Git records the changes as renames where appropriate; do not recreate files with duplicated compatibility types.
- Keep commits conventional and scoped to one step; never use git add -A or sweep unrelated changes into a commit.
- Do not edit .clinerules/to-do.md; the reviewer ticks Stage I only after the final step passes the Global Definition of Done.
- Do not change .clinerules/chat/audit.md as the actor; the reviewer owns that file.
- Keep verification output compact and report the exact commit SHA, verification result, limitations, friction, problems, and status in .clinerules/chat/act.md.
