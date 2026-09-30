# Act Report — Stage G Step 1

- Commit: `fcae247` — feat(account): add AccountDataService with credential operations
- Verification:
  - `dotnet build src/MobileShop.slnx --nologo` -> **Build succeeded, 0 errors, 1 warning**. The warning is the **pre-existing** `CS9124` in `src/MobileShop.Services/DataServices/Dal/ProductsDataService.cs(26,62)` — untouched by this stage, identical to every baseline since Stage E.
  - `dotnet test src/MobileShop.slnx --nologo --no-build` -> **Failed: 0, Passed: 546, Skipped: 2, Total: 548** (EXIT=0).
  - Targeted first: `--filter FullyQualifiedName~AccountDataServiceTests` -> **8 passed, 0 failed** (~15 s, because the real Argon2 hasher runs).
  - Done-when check: Dal registers the area service; **no page/startup changes** (`git diff --stat` on `src/MobileShop.Web/` is empty).
- What was implemented (plan Step 1 scope only):
  - **`AccountDataService`** (new, `src/MobileShop.Services/DataServices/Dal/`) — implements **all four** `IAccountDataService` members (L10, no NIE). Ctor `(IUserRepo users, IPasswordHasher passwordHasher, ILogger<AccountDataService> logger)`. It injects **no** `IUserDataService` (plan's explicit prohibition), and **no** entity service at all.
  - **Ported from `UserDataService`**, semantics preserved: `DefaultAdminUsername = "admin"` / `DefaultAdminPassword = "Admin@123"` kept as public consts (plan allowed public for tests); `EnsureAdminUser` finds the admin by default username, **throws `InvalidOperationException` with the same message** when missing, sets a fresh hash, and updates; `ValidateCredentialsAsync` / `ChangePasswordAsync(username, ...)` keep the same verify/hash flow and the same `LogWarning`/`LogInformation` messages as the entity service (L6).
  - **`GetAdminUsernameAsync`** — new area-only member returning just the username string (`admin?.Username`); **no `User` entity is exposed**, which is what L1 requires the Profile page to consume in Step 2.
  - **Password hashing stays behind `IPasswordHasher` inside the service (L5)** — pages will pass plain passwords only.
  - **Dal DI gap fixed (plan HIGH)** — added `services.AddScoped<IAccountDataService, AccountDataService>();` in the Dal branch only. The `if (useApi)` branch and its `ApiAccountDataService` line are untouched (L7); no interface signature changed, so no Api stub update was required.
  - **L3 respected** — `IUserDataService` -> `UserDataService` (Dal) and `ApiUserDataService` (Api) registrations **both still present**; nothing removed.
  - **Tests** — `AccountDataServiceTests` (8) over `RepoTestBase` + the real service with `UserRepo` and the **real `PasswordHasher`** (no double): `GetAdminUsernameAsync` null when missing and `"admin"` when seeded; `EnsureAdminUser` stores a hash that differs from the stale value and verifies against the default password; `EnsureAdminUser` throws (message asserted) when the admin was not seeded; validate accepts the correct password; validate rejects wrong password and unknown user; `ChangePasswordAsync` stores a verifiable hash such that the new password validates and the old one no longer does; change-password returns false for an unknown user.
- Limitations: None. **No authentication was added** (L4) — this service only validates credentials and changes passwords, exactly as the entity service did.
- Friction noted: the full suite duration grew from ~35 s to **~53 s** because these tests exercise real Argon2 hashing rather than a stub; still inside a detached-launch + poll loop, so no truncation. No build friction beyond the standing `setsid` pattern.
- Problems: one self-inflicted compile slip while writing the tests — `EnsureAdminUser_sets_a_hash_that_verifies_against_the_default_password` was declared `void` while containing `await`; corrected to `async Task` **before** the first build, so it never reached verification as a failure.
- Status: COMPLETE
