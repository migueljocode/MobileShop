# MobileShop — Development Simplification Plan

> **Status: planning only — no code has been changed.**
> This document explains one line of existing code, then lists the boilerplate we can strip
> out so the Web app stays simple during the first development stage.

---

## 1. The line you asked about

File: `MobileShop.Web/DatabaseInitializer.cs`

```csharp
private static void EnsureDefaultAdminPassword(IServiceProvider services, AppDbContext context)
{
    const string Placeholder = "REPLACE_WITH_REAL_HASH"; // value shipped inside the sample data
    const string DefaultPassword = "Admin@123";           // dev-only default login

    var admin = context.Users.FirstOrDefault(user => user.Username == "admin");
    if (admin is null || admin.PasswordHash != Placeholder)
    {
        return; // nothing to do
    }

    var hasher = services.GetRequiredService<IPasswordHasher>();
    admin.PasswordHash = hasher.Hash(DefaultPassword);
    context.SaveChanges();
}
```

### What `admin is null || admin.PasswordHash != Placeholder` means

Read it as a safety guard: **"If there is no admin user, OR the admin user no longer has the
placeholder hash → stop right here and do nothing."**

Only when **both** parts are false does the method actually write a real password hash:

| # | Condition | What it protects against |
|---|-----------|--------------------------|
| 1 | `admin is null` | Crash protection — don't call `.PasswordHash` on a user that doesn't exist. |
| 2 | `admin.PasswordHash != Placeholder` | Re-run protection — don't overwrite a password that was already configured. |

(The `||` also short-circuits: if condition 1 is true, condition 2 is never even evaluated.)

### Why the second condition (the one you asked about) is required

1. **This method runs on every app start in Development mode.** `InitializeForDevelopment`
   is called from `Program.cs` each launch.
2. **Argon2 hashing is salted** — the *same* password produces a different hash every time.
   If we always re-hashed on startup, the admin's password would silently reset to
   `Admin@123` on **every single restart**, discarding anything the user had changed.
3. **The placeholder is a "first run only" marker.** The sample data ships with the literal
   string `REPLACE_WITH_REAL_HASH`. Checking *"are we still exactly at the placeholder?"*
   is how the code knows it is the very first run and is allowed to write a real hash once.
4. **It respects passwords that were set later.** As soon as the stored value is a real
   Argon2 hash (whether we set it, or the owner changed it), the condition becomes true and
   the code leaves it alone forever.

**In one sentence:** the `||` guard makes sure we auto-set a default password **only on a
fresh database**, and never touch the admin's password again — even though the method runs
---

## 2. Boilerplate we can remove to keep development simple

**Goal:** a bare-bones dev app — **no cookies, no login, no CSRF tokens, no HSTS / error-page
plumbing** — with short `// TODO(...)` notes where the production stuff goes later.

### 2.1 `MobileShop.Web/Program.cs`

**Remove:**
- the cookie-authentication block: `builder.Services.AddAuthentication(...)...AddCookie(...)`
- `app.UseAuthentication();` and `app.UseAuthorization();`
- the whole
  `if (app.Environment.IsDevelopment()) { ... } else { app.UseExceptionHandler(...); app.UseHsts(); }`
  — replace it with a single short comment: `// TODO(production): add HSTS, HTTPS redirect and an error page once the first dev stage is done.`

**Keep:**
- `builder.Services.AddRazorPages();`
- `builder.Services.AddMobileShop(builder.Configuration);` ← your DI wiring (DbContext, repos, hasher, data services)
- `DatabaseInitializer.InitializeForDevelopment(app);` ← creates + seeds the SQLite DB so pages have data
- `app.UseRouting(); app.MapStaticAssets(); app.MapRazorPages().WithStaticAssets(); app.Run();`

Resulting skeleton (what `Program.cs` would look like):

```csharp
var builder = WebApplication.CreateBuilder(args);

builder.Services.AddRazorPages();
builder.Services.AddMobileShop(builder.Configuration);

var app = builder.Build();

// TODO(production): add HTTPS redirection, HSTS and a custom error page once the first
// development stage is done. For now we run plain HTTP in Development mode.

DatabaseInitializer.InitializeForDevelopment(app);

app.UseRouting();
app.MapStaticAssets();
app.MapRazorPages().WithStaticAssets();

app.Run();
```

### 2.2 Login, cookies and CSRF

**Remove:**
- `Pages/Account/Login.cshtml` and `Pages/Account/Login.cshtml.cs`
- `Pages/Account/Logout.cshtml` and `Pages/Account/Logout.cshtml.cs`  → i.e. the whole `Pages/Account/` folder
- `[Authorize]` above `IndexModel` (`Pages/Index.cshtml.cs`)
- the "Sign in / Sign out" block in `Pages/Shared/_Layout.cshtml`
- the auth-related usings in `GlobalUsings.cs`:
  `Microsoft.AspNetCore.Authentication`, `Microsoft.AspNetCore.Authentication.Cookies`,
  `Microsoft.AspNetCore.Authorization`, `System.Security.Claims`

**CSRF / antiforgery:** ASP.NET Core 10 automatically validates a CSRF token on any
`method="post"` form. For dev we don't want that, so we would:
1. remove `asp-antiforgery="true"` from the forms, and
2. turn off the automatic validation — the exact one-line config switch will be confirmed
   while implementing, so all future forms can stay plain `<form method="post">`.

### 2.3 `MobileShop.Web/DatabaseInitializer.cs`

- **Keep:** `Migrate()` + `SeedIfEmpty(...)` — this is what makes the app runnable with sample data.
- **Remove:** `EnsureDefaultAdminPassword(...)` — it exists only for the login feature; without
  login it is dead code. Replace it with a note like
  `// TODO(security): when login returns, add a proper first-run admin password setup here.`

on every startup.