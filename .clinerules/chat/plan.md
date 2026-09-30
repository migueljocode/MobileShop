# Plan — Stage G: Account — AccountDataService + login/profile

Stage F is signed off. This file plans **Stage G only** (next unchecked stage in `to-do.md`).

## Process (binding)
- **One step → commit → Job B review → next step.**
- Only the **reviewer** ticks Stage G in `to-do.md` after the final step PASSes.

## Locked decisions
L1 Pages inject area interfaces only. No EF `User` (or other entities) on the page path.
L2 One area service per page, local name `dataService`. After migration, Login and Profile inject **only** `IAccountDataService`. Logout stays dependency-free (redirect only).
L3 DI — **this stage removes nothing.** Keep `IUserDataService` / `UserDataService` registered until Stage H (other callers may still exist).
L4 Auth middleware stays **off** (dev stage). Login still only validates credentials and shows the existing message — do **not** wire cookie/JWT auth.
L5 Password hashing stays behind `IPasswordHasher` inside the area service (port from `UserDataService`). Pages pass **plain** passwords only.
L6 Logging — `ILogger<AccountDataService>`; log validate success/failure and password-change outcomes (same spirit as `UserDataService`).
L7 Api — `ApiAccountDataService` already matches the interface with NIE. Signature changes (if any) update Api in the same step. Never touch `src/MobileShop.Api`.
L8 No auth restore, no schema/migrations, no DatabaseInitializer policy change, no bin/obj.
L9 Tests — real `AccountDataService` over repos + real or thin hasher; cover ensure-admin, validate, change-password, GetAdminUsername.
L10 **Full interface in Step 1** — implement every `IAccountDataService` member on Dal (no NIE).

## Gap (must fix in Step 1)
`IAccountDataService` is registered under the **Api** branch only. The **Dal** branch has **no** `IAccountDataService` → `AccountDataService` line. Add it in Step 1.

## Scope
- Pages: `Account/Login`, `Account/Profile` (and `ProfileModel.cs`).
- Startup: Development `EnsureAdminUser` call in `WebApplicationBuilderExtensions.ConfigureApp` currently uses `IUserDataService` — switch to `IAccountDataService` in the page/startup migration step.
- **Logout** — no service work; leave as-is.

## Current behavior (verified)
| Surface | Injects today | Behavior |
|---------|---------------|----------|
| Login | `IUserDataService` | ValidateCredentialsAsync; on fail model error `"Invalid username or password."`; on success Message about auth not enabled |
| Profile | `IUserDataService` | OnGet loads user via `FindByUsernameAsync("admin")` → **User entity** on page; OnPost validates current password, ChangePasswordAsync `"admin"` |
| ConfigureApp (Dev) | `IUserDataService` | `EnsureAdminUser()` after sample seed |
| Logout | none | POST → redirect Index |

Admin constants on entity service: `DefaultAdminUsername = "admin"`, `DefaultAdminPassword = "Admin@123"` — port as private constants on `AccountDataService` (or public const if tests need them).

## Interface (already present — implement as-is)
```csharp
void EnsureAdminUser();
Task<string?> GetAdminUsernameAsync();
Task<bool> ValidateCredentialsAsync(string username, string plainPassword);
Task<bool> ChangePasswordAsync(string username, string plainNewPassword);
```

### Implementation notes
- Port logic from `UserDataService` / `IUserRepo` + `IPasswordHasher`.
- Prefer `IUserRepo` (or `IBaseRepo<User>` + specialized username methods) — **do not** inject `IUserDataService` into the area service.
- `EnsureAdminUser`: find admin by default username; throw if missing; set hash of default password; update.
- `GetAdminUsernameAsync`: return the admin username string if the user exists, else null — **no User entity to the page**.
- `ValidateCredentialsAsync` / `ChangePasswordAsync`: same verify/hash semantics as entity service.
- Profile page must stop calling `FindByUsernameAsync` that returns `User`; use `GetAdminUsernameAsync` (or keep displaying the known admin name via that method).

## Reviewer Briefing
- **HIGH:** Dal DI for `IAccountDataService` is missing today.
- **HIGH:** Profile must not surface `User` entities (L1).
- **MEDIUM:** Startup EnsureAdmin switches to area service; keep entity `IUserDataService` registered (L3).
- **Do not** enable authentication middleware (L4).

## ~~[x] Step 1 — `AccountDataService` + Dal registration~~
- Files: create `src/MobileShop.Services/DataServices/Dal/AccountDataService.cs`, `src/MobileShop.Tests/Services/DataServices/Dal/AccountDataServiceTests.cs`; modify `ServiceCollectionExtensions` (Dal: `services.AddScoped<IAccountDataService, AccountDataService>();`).
- Ctor: `(IUserRepo users, IPasswordHasher passwordHasher, ILogger<AccountDataService> logger)` (or equivalent `IBaseRepo<User>` if username helpers exist).
- Implement all four interface members (L10).
- Tests: EnsureAdmin sets a verifiable hash; Validate true/false; ChangePassword then Validate with new password; GetAdminUsernameAsync non-null when seeded.
- Verify: build + full suite.
- Done when: Dal registers area service; **no page/startup changes yet**.
- Risk: MEDIUM. Confidence: HIGH.

## [ ] Step 2 — Migrate Login, Profile, and Dev EnsureAdmin
- Files: `Pages/Account/Login.cshtml.cs`, `Pages/Account/ProfileModel.cs`; `Web/Extensions/WebApplicationBuilderExtensions.cs`; any Account page tests.
- Change:
  - Login: `(IAccountDataService dataService)` only; call `ValidateCredentialsAsync`.
  - Profile: `(IAccountDataService dataService)` only; `OnGet` uses `GetAdminUsernameAsync`; post uses validate + `ChangePasswordAsync` (still admin username from service or constant via service).
  - ConfigureApp Dev: `GetRequiredService<IAccountDataService>().EnsureAdminUser()`.
  - Preserve user-visible messages and ModelState behavior.
- Do not edit `.cshtml` unless forced (stop and report).
- Logout: no change.
- Verify: build + full suite.
- Risk: LOW. Confidence: HIGH.

## [ ] Step 3 — Stage G validation
- Chain 1: build + full suite.
- Chain 2: Production host; **200** `/Account/Login`, `/Account/Profile` if reachable; regression Home/Products/People/Transactions/Reports.
- Optional Development smoke: EnsureAdmin still runs without throw after seed.
- Non-destructiveness: Production log no wipe; optional fingerprint stable.
- Done when: Login/Profile use only `dataService`; no page `IUserDataService`; entity `IUserDataService` still registered; suite + smoke green.
- Risk: LOW. Confidence: HIGH.

## Global Definition of Done
- `AccountDataService` implements full `IAccountDataService` and is Dal-registered.
- Login and Profile depend on a single `dataService`; no `User` entity on pages.
- Dev EnsureAdmin uses `IAccountDataService`.
- No entity service registration removed; auth stays disabled.
- Build + suite + Production smoke green.

## Execution notes
- One step per commit; Conventional Commits; no Co-authored-by.
- Stop after each step for Job B.
- OUT OF SCOPE: enabling authentication, Stage H cleanup, deleting `UserDataService`.
