# Plan — Tasks 4+5 — Color + Corporation dropdowns with "Add New" on Create Phone

## Reviewer Briefing
- HIGH-risk / MEDIUM-confidence: Option B changes input contract (`string Color` → `int? ColorId`); POST + tests must move together.
- MEDIUM-risk: `PopulateDropdownsAsync()` gap — Colors never loaded; validation failure empties dropdown unless fixed in same step.
- LOW-risk: modal/JS reuse from Manufacturer/Model; Colors are global (no cascade).
- Reviewer corrections applied (audit APPROVED WITH CORRECTIONS): Step 2 uses `FindAllAsync` + in-memory Distinct (no `SelectAllAsync` exists); Step 1 repoints validation span; Step 2 Verify runs full suite; no-DB-write rule promoted into Change 2.

## ~~[x] Step 1 — Option B ColorId dropdown + Add New modal + repopulation fix~~
- Files: inspect: `CreatePhone.cshtml:12-32,58-62,111-160,216-260`, `WebApplicationBuilderExtensions.cs:11-38`; modify: `CreatePhoneInputModel.cs`, `CreatePhone.cshtml.cs`, `CreatePhone.cshtml`, `CreatePhoneModelTests.cs`; create: none; do not touch: services, repos, migrations, API, auth, DB init.
- Symbols: `CreatePhoneInputModel.ColorId`, `CreatePhoneModel.Colors`, `CreatePhoneModel.OnPostCreateColorAsync(string name)`, `CreatePhoneModel.PopulateDropdownsAsync()`, `CreatePhoneModel.AddColorAsync` (delete).
- Current -> Desired: free-text `Input.Color` (`string?`) + silent auto-create → nullable `Input.ColorId` dropdown of 6 seeded Colors (Midnight, Black, Green, White, Blue, Silver) + Add New modal → JSON `{id,name}` → select; validation failure keeps dropdown populated.
- Change:
  1. Input model: replace `string? Color` with `int? ColorId` + `[Range(1,int.MaxValue)]`; keep nullable (Task 7 N/A).
  2. PageModel: reuse existing `IColorRepo colorRepo` (already injected); add `Colors` property; load in `OnGetAsync`; always load in `PopulateDropdownsAsync()`.
  3. Add `OnPostCreateColorAsync`: trim, blank→400, duplicate (trim + exact `c.Name == trimmed`, same as Manufacturer/Model)→return existing, else `AddAsync` + `{id,name}`.
  4. `OnPostAsync`: resolve by `ColorId` via `FindAsync(id)`; null ColorId → null (allowed); unknown id → ModelState error + repopulate; delete `AddColorAsync`.
  5. View: replace text input with `<select asp-for="Input.ColorId" asp-items="@(new SelectList(Model.Colors,"Id","Name"))">` + placeholder + Add New button/modal mirroring Manufacturer/Model; repoint `asp-validation-for` span (`CreatePhone.cshtml:61`) from `Input.Color` to `Input.ColorId`; unique ids `colorSelect`/`addColorModal`/`saveColorBtn`/`newColorName`; JS mirrors `:216-260`.
  6. Antiforgery: mirror existing fetch POST pattern (no token); confirm in `WebApplicationBuilderExtensions.ConfigureBuilder/ConfigureApp` (not `Program.cs` — 3-line shim) that no global antiforgery change is needed.
- Depends on: none.
- Edge cases / error handling: no seeds → placeholder only (6 seeded, fine); validation failure → repopulated; duplicate casing/whitespace → return existing; empty modal → 400; unknown ColorId → ModelState error, no silent create.
- Tests: extend `CreatePhoneModelTests.cs` (exists — modify, not create): create-new-color, duplicate-returns-existing, blank→400, validation-failure-repopulates-Colors, POST persists selected ColorId + null-allowed.
- Verify: `dotnet build src/MobileShop.slnx --nologo && dotnet test src/MobileShop.slnx --nologo --no-build --filter "CreatePhoneModelTests"` plus served-HTML check (options on GET, modal adds+selects without reload).
- Done when: dropdown shows seeded colors; modal creates+selects; POST persists via ColorId; null still allowed; validation failure keeps dropdown; build 0/0; tests pass.
- Risk: HIGH
- Confidence: MEDIUM

## [ ] Step 2 — Task 5 (strings, no new entity): Corporation dropdown + Add New modal
- Files: inspect: `CreatePhone.cshtml:94-100` (guarantee block), `Guarantee.cs:12-14` (Corporation: Required/MaxLength 100 — no FK, no Corporation entity), `CreatePhone.cshtml.cs:112-117` (Guarantee POST block with "Shop Warranty" default); modify: `CreatePhone.cshtml.cs`, `CreatePhone.cshtml`, `CreatePhoneModelTests.cs`; create: none; do not touch: input model contract (`GuaranteeCorporation` stays `string?`), entities, repos, migrations, services, API, auth, DB init.
- Symbols: `CreatePhoneModel.Corporations` (new `IEnumerable<string>`), `CreatePhoneModel.OnPostCreateCorporationAsync(string name)` (new, NO DB write), `CreatePhoneModel.PopulateDropdownsAsync()` (extend for both lists).
- Current -> Desired: free-text `Input.GuaranteeCorporation` (`string?`, max 100) + `"Shop Warranty"` fallback at `:116` → `<select>` of distinct existing `Guarantee.Corporation` values (seeded: Apple, Samsung — 4 guarantee rows) + "Add New" modal that appends client-side only (a corporation exists only once a Guarantee row uses it); POST behavior unchanged.
- Change:
  1. PageModel: inject `IGuaranteeRepo` (exists, registered in `ServiceCollectionExtensions.cs:73`, NOT currently injected in CreatePhoneModel — follow `colorRepo` ctor pattern); add `Corporations` property; load via `await guaranteeRepo.FindAllAsync()` then `.Select(g => g.Corporation).Distinct()` in memory (honest shape: `IBaseRepo` has no async `SelectAllAsync`; `SelectAll` materializes via `.ToList()` in `BaseRepo.cs:33-38`; table is tiny — 4 seeded rows); load in `OnGetAsync` + always in `PopulateDropdownsAsync()` alongside Colors.
  2. Add `OnPostCreateCorporationAsync(string name)`: trim, blank→400, duplicate (case-insensitive check against `Corporations` — strings have no row identity so exact-match would allow "apple"/"Apple" dupes)→return `{name}` of existing-cased value, else return `{name: trimmed}` with NO `AddAsync` and NO other DB write. NEVER call `AddAsync` here: a Guarantee requires a ProductId, so persisting from the modal would invent a ProductId-less orphan row. New names persist only on the next phone POST via the unchanged Guarantee block. Return shape `{name}` only (no id — no row exists).
  3. View: replace text input (`:94-100`) with `<select asp-for="Input.GuaranteeCorporation">` + placeholder + Add New button/modal mirroring Step 1 (build options with a plain string loop or `new SelectList(Model.Corporations)` — no object SelectList needed); `asp-validation-for` span stays `GuaranteeCorporation` (contract unchanged); unique ids `corporationSelect`/`addCorporationModal`/`saveCorporationBtn`/`newCorporationName`; JS mirrors Step 1 but appends `opt.value = result.name` (string, not id). Use async repo calls (`FindAllAsync`/`FindAsync`) throughout — no sync variants needed.
  4. `OnPostAsync` Guarantee block (`:112-117`): UNCHANGED — `IsNullOrWhiteSpace → "Shop Warranty"` default stays. Dropdown submits a string; empty selection still hits the default.
- Depends on: Step 1 (same files; rebase on its commit; do not re-touch Color code).
- Edge cases / error handling: empty Guarantees table → placeholder only; validation failure → both Colors and Corporations repopulated; modal blank → 400; duplicate different-casing → return existing-cased value, no dup option; `HasGuarantee=false` → select irrelevant, POST ignores (unchanged).
- Tests: extend `CreatePhoneModelTests.cs`: corporations-loaded-on-GET, create-returns-name-without-DB-write (assert `Context.Guarantees.Count()` unchanged), duplicate-casing-returns-existing, blank→400, validation-failure-repopulates-Corporations.
- Verify: `dotnet build src/MobileShop.slnx --nologo && dotnet test src/MobileShop.slnx --nologo --no-build` (FULL suite — this is the stage's last step, reviewer signs off from this evidence) plus served-HTML check (corporation options on GET, modal appends+selects without reload, POST with new corp persists `Guarantee.Corporation`).
- Done when: dropdown shows distinct corporations; modal appends+selects without DB write; POST persists via existing Guarantee block; "Shop Warranty" default intact; validation failure keeps both dropdowns; build 0/0; tests pass.
- Risk: MEDIUM
- Confidence: MEDIUM

## Global Definition of Done
- Color dropdown (ColorId) + Corporation dropdown (strings) + both Add New modals working on Create Phone.
- POST persists color via ColorId (null allowed, Task 7 safe) and corporation via unchanged Guarantee block ("Shop Warranty" default intact).
- Validation failure repopulates Manufacturers, Models, Colors, Corporations.
- `dotnet build src/MobileShop.slnx --nologo` → 0 errors, 0 warnings; final step runs FULL `dotnet test src/MobileShop.slnx --nologo --no-build` (not just filter) per project rules.
- Served-HTML evidence for both dropdowns (options on GET, modal adds+selects without reload).
- No service/repo/migration/API/auth/DB-policy changes; no new entity for Corporation; `AddColorAsync` deleted, nothing else removed.

## Execution notes
- One step, one commit. Chain build && test; keep `colorRepo` reuse; never make ColorId `[Required]`; follow modal pattern exactly.
- Sequential steps, one commit each; Step 2 rebases on Step 1 and never re-touches Color code.
- Never make GuaranteeCorporation `[Required]` either; Corporation handler writes NOTHING to the DB — a new name persists on next phone POST, not from the modal.

