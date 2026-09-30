# ACTOR PROMPT — Stage H Step 3 (delete specialized repositories)

> This file is an instruction. After you finish, replace its entire content with your Act Report, using the same format as your previous reports.

You are the ACTOR. Read `.clinerules/actor.md`, `.clinerules/project-specific-rules.md`, `.clinerules/to-do.md`, `.clinerules/chat/plan.md` (Step 3 and the Global Definition of Done) and `.clinerules/chat/audit.md`. Run `git pull` first. Do **Stage H Step 3 only**, then STOP for Job B review. Do not start Step 4. Do not touch `.clinerules/to-do.md`.

## Scope
Delete the specialized repositories, their interfaces, their tests and their DI registrations. `IBaseRepo<T>` / `BaseRepo<T>` survive. Do not modify `src/MobileShop.Api`, entities, EF configuration, migrations, sample data, initialization policy, or any area service. Do **not** fix the CS9124 warning or any other backlog item.

## Consumer proof (verify it yourself before deleting anything)
My static search found that, outside `MobileShop.Dal/Repos/` itself, each of the 15 specialized repos is referenced only by:
- its own test file under `src/MobileShop.Tests/Dal/Repos/`, and
- its registration in `AddMobileShopRepository()` in `src/MobileShop.Services/ServiceCollectionExtensions.cs`.

`EmployeeRepo` has no test file. No Web page, area service, initialization code or test helper references them. Re-run a word-boundary search (`\b(I)?(User|Customer|Seller|Product|Transaction|AppleId|Phone|SecondHand|Guarantee|Manufacturer|Model|Category|Color|Employee|Person)Repo\b`) over `src`, excluding `bin/obj` and `Dal/Repos/`, and record the result in your report. If any surviving production consumer turns up, keep that repo and report it instead of deleting it.

## Steps
1. **Delete** in `src/MobileShop.Dal/Repos/`: the 15 `*Repo.cs` implementations and the whole `Interfaces/` folder (15 `I*Repo.cs` files). Keep `Base/BaseRepo.cs` and `Base/IBaseRepo.cs` exactly as they are.
2. **Delete** the 14 test files in `src/MobileShop.Tests/Dal/Repos/` (`AppleId`, `Category`, `Color`, `Customer`, `Guarantee`, `Manufacturer`, `Model`, `Person`, `Phone`, `Product`, `SecondHand`, `Seller`, `Transaction`, `User` `RepoTests.cs`). Keep everything else under `src/MobileShop.Tests/Dal/` (`BaseClass/RepoTestBase.cs`, `BaseClass/BaseRepoTests.cs`, `BaseClass/TestDataHelpers.cs`, `Initialization/`). Before deleting, count the `[Fact]` tests and each `[Theory]` × `[InlineData]` row in those files, so you can reconcile the test-count drop.
3. **Edit** `AddMobileShopRepository()` in `ServiceCollectionExtensions.cs`: remove the 15 specialized `AddScoped<I…Repo, …Repo>()` lines and keep `services.AddScoped(typeof(IBaseRepo<>), typeof(BaseRepo<>));`. Update the method's XML summary so it no longer says remainders are removed in Stage H.
4. **Expected forced build break** (same as Step 2): the namespace `MobileShop.Dal.Repos.Interfaces` will no longer exist, so the four `global using MobileShop.Dal.Repos.Interfaces;` lines will fail with CS0234. They are in `Dal`, `Web`, `Tests` and `Services` `GlobalUsings.cs`. Remove **only those four lines**, and only as required to compile. Leave every other global using alone (Step 4 owns the rest, including `MobileShop.Dal.Repos`, which still exists because `Repos.Base` remains).
5. Do not delete anything else.

## Verify
- Dead-reference search again after the deletions: no reference to any deleted repo type or to `Repos.Interfaces` remains.
- Confirm `src/MobileShop.Dal/Repos/` now contains only `Base/BaseRepo.cs` and `Base/IBaseRepo.cs`.
- `git diff --stat -- src/MobileShop.Api` is empty, and `git diff --diff-filter=D --name-only` lists only the planned deletions.
- Required chain: `dotnet build src/MobileShop.slnx --nologo && dotnet test src/MobileShop.slnx --nologo --no-build`. The baseline is 522 passed / 2 skipped, so reconcile the drop against your count from step 2. Report the warning count honestly; CS9124 at `ProductsDataService.cs:26` is expected to reappear on a full recompile. Use the detached-launch + poll pattern if the commands exceed the time window.

## Commit and report
- Commit: `git commit -m "refactor(dal): delete specialized repositories, interfaces, and their tests"` (Conventional Commits, no Co-authored-by, never amend, never push).
- Then replace this file's content with your Act Report (commit hash, verification numbers, consumer-proof result, what was deleted, what was modified, limitations, problems), and record the hash in a separate commit: `git commit -m "docs(act): record Stage H Step 3 commit hash"`.
- STOP.

## If something doesn't match
If a repo turns out to have a surviving consumer, a file in the list doesn't exist, or the build breaks anywhere other than the four `Repos.Interfaces` using lines, do not improvise: stop and report an ARCHITECTURAL BLOCKER with the evidence.
