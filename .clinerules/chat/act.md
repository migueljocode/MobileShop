# ACTOR PROMPT — Stage H Step 3 correction (restore BaseRepo<T> direct tests)

> This file is an instruction. After you finish, replace its entire content with your Act Report, using the same format as your previous reports.

You are the ACTOR. Read `.clinerules/actor.md`, `.clinerules/chat/audit.md` (Job B on Step 3: one HIGH finding) and `.clinerules/chat/plan.md`. Run `git pull` first. Do **one correction pass for that HIGH finding only**, then STOP. Do not start Step 4. Do not touch `.clinerules/to-do.md`, `.clinerules/chat/plan.md`, or `audit.md`.

## Problem
`src/MobileShop.Tests/Dal/BaseClass/BaseRepoTests.cs` is an `abstract` generic class (`BaseRepoTests<TEntity, TRepo>`) with 26 `[Fact]`s. All its concrete subclasses were deleted in `e3a6622`, so none of its tests run any more, and `BaseRepo<T>`, the only surviving repository abstraction, has no direct tests.

## Task (tests only; no production code changes)
1. Recover the old Person subclass for reference: `git show e3a6622^:src/MobileShop.Tests/Dal/Repos/PersonRepoTests.cs`.
2. Add one concrete class in `src/MobileShop.Tests/Dal/BaseClass/`, for example `BaseRepoPersonTests.cs`:
   `public class BaseRepoPersonTests : BaseRepoTests<Person, BaseRepo<Person>>`
   - `CreateRepo()` returns `new BaseRepo<Person>(Context)`.
   - `CreateValidEntity()` is adapted from the recovered `PersonRepoTests` (a valid `Person` with unique values, following the base class's expectations).
   - Use the repo's minimal modern C# style (file-scoped namespace, expression-bodied members, existing global usings). Do not add `using` lines that a global using already covers.
3. Do not modify `BaseRepoTests.cs`, `RepoTestBase.cs`, any production file, or any other test.

## Verify
- `dotnet build src/MobileShop.slnx --nologo && dotnet test src/MobileShop.slnx --nologo --no-build`. Expected: **0 errors, 240 passed / 2 skipped** (214 + 26). Report the actual numbers. If one or more of the 26 inherited tests fails against `BaseRepo<Person>`, do not change `BaseRepo` or weaken the test: STOP and report the failing test names and messages as a finding.
- Run the suite filtered to the new class, for example `--filter "FullyQualifiedName~BaseRepoPersonTests"`, and confirm it runs exactly 26 tests.
- `git diff --stat -- src/MobileShop.Services src/MobileShop.Web src/MobileShop.Dal src/MobileShop.Api` is empty.
- Report the warning count honestly. CS9124 at `ProductsDataService.cs:26` is expected on a full recompile. Do not fix it; it stays in the cleanup backlog.

## Commit and report
- Commit: `git commit -m "test(dal): restore BaseRepo<T> direct tests with a concrete Person subclass"` (Conventional Commits, no Co-authored-by, never amend, never push).
- Then replace this file's content with your Act Report (commit hash, verification numbers, what was added, limitations, problems), and record the hash in a separate commit: `git commit -m "docs(act): record Stage H Step 3 correction commit hash"`.
- STOP.
