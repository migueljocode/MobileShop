# Audit — Job B (Execution Check): Stage U Step 4 (`e72db9d`, PR #13)

**Verdict: FAIL.** The actor can fix this directly. Do not start Step 5.

- **What is good:** `.editorconfig` matches the plan (`root = true`, `charset = utf-8`, `insert_final_newline = true`). The 46-file squash is mechanical: `git show e72db9d --ignore-space-at-eol --stat` is only `.editorconfig`; every other file is `1 1` on `--numstat`. No migrations, no `.clinerules/chat`. Action **#401 — Success** (PR) and **#402 — Success** (merge): build 0 warnings, 351 tests, smoke.
- **Gap:** Step 4 is done when *no* in-scope tracked text file lacks a final newline. At planning time that list was 99. This commit fixed 45. **54 remain**, last byte is not `\\n`:
  - First-party (30): entity configurations under `src/MobileShop.Models/Entities/Configuration/`, `PortableStorage.cs`, `StorageCapacity.cs`, `TransactionFactorExtensions.cs`, `CreateGlassInputModel.cs`, several Web view models, `ProfileModel.cs`, `CreateGlass.cshtml.cs`.
  - Vendor (24): `src/MobileShop.Web/wwwroot/lib/**` (bootstrap/jquery). The plan did not exclude them; the 99 = 45 + 54.
- **Exact fix:** one correction commit that appends a final `\\n` to every remaining in-scope file (same extensions, still excluding `Migrations/` and `.clinerules/chat/`, do not touch empty files). Record the new count in `act.md`. `git diff --ignore-space-at-eol --stat` must still show no content change besides `.editorconfig` already on main. Then wait for a green Action and STOP.
- **LOW (no action):** squash subject is not Conventional Commits and the body repeats “add final newlines” once per file.

## Gate
Step 5 is **not** authorized until the remaining 54 files (or a plan change excluding `wwwroot/lib`) are done and CI is green.
