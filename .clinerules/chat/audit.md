# Audit — Job A (Plan Review): Tasks 4+5 combined — Color + Corporation dropdowns

**Reviewed**: `.clinerules/chat/plan.md` (Tasks 4+5 combined, Step 1 Task 4 / Step 2 Task 5)
**Checklist source**: `.clinerules/to-do.md` — Task 4, Task 5
**Supersedes**: prior Task-4-only APPROVED verdict below + planner handoff (that plan is replaced by this combined plan; re-review starts fresh)

## Verdict

**APPROVED WITH CORRECTIONS** — one HIGH finding below is narrow (a wrong repo-API call the actor cannot invent around). Fix it in plan.md, then execute without another review round.

## Checks performed (HIGH-risk steps scrutinized)
- Step 1 claims verified: `string? Color` (`CreatePhoneInputModel.cs:28-29`), `asp-for="Input.Color"` (`CreatePhone.cshtml:59-61`), `AddColorAsync` (`CreatePhone.cshtml.cs:146-151`), `PopulateDropdownsAsync` (`:139-144`), `IColorRepo colorRepo` already injected (`:8`). All exist as claimed.
- Step 2 claims verified: `GuaranteeCorporation string?` (`CreatePhoneInputModel.cs:38-39`), guarantee block `CreatePhone.cshtml:94-100`, `"Shop Warranty"` fallback (`CreatePhone.cshtml.cs:116`), `Corporation` Required/MaxLength-100 with no FK/entity (`Guarantee.cs:12-14`), `IGuaranteeRepo` registered (`ServiceCollectionExtensions.cs:73`) but not injected in `CreatePhoneModel`. All correct.
- Project rules: Web/Razor layer only, no API/auth/DB-policy change. Compliant.
- Scope: covers both to-do items fully; Step 2 correctly leaves the Guarantee POST block untouched. No scope creep.

## Findings

### HIGH
- **Step 2, Change 1 names a repo API that does not exist.** `SelectAll(g => g.Corporation).Distinct()` — `IBaseRepo.SelectAll` returns materialized `IEnumerable<TResult>` (`BaseRepo.cs:33-38` calls `.ToList()` inside), so `.Distinct()` runs client-side on the full column set, and there is no async `SelectAllAsync` overload at all. The plan's "stays IQueryable, never pull into memory" is unimplementable as written. Fix: `FindAllAsync()` (exists, `AsNoTracking`) then `.Select(g => g.Corporation).Distinct()` in memory — honest about the shape, fits the existing async convention, and the table is tiny (4 seeded rows). One-line plan fix; no architecture change.

### MEDIUM
- **Step 2 duplicate rule contradicts itself.** Change 2 says case-insensitive dedupe against `Corporations`, but Done-when/edge wording implies the modal "persists" the name — it cannot (no row is written; a ProductId-less Guarantee would be an orphan). Tighten: handler returns existing-cased `{name}` on case-insensitive match, else echoes trimmed `{name}` with no DB write; persistence happens only on the next phone POST. The Execution notes already say this — promote it into Change 2 so the actor doesn't invent an `AddAsync`.
- **Step 1 still omits the validation-span repoint.** Carried over from the prior review: `asp-validation-for="Input.Color"` (`CreatePhone.cshtml:61`) must become `Input.ColorId`, or ColorId errors render nowhere. One line.
- **Final-step validation is filtered-only in both steps' Verify lines.** The Global DoD correctly demands the full chain; make Step 2's Verify run it (`dotnet build src/MobileShop.slnx --nologo && dotnet test src/MobileShop.slnx --nologo --no-build`) since Step 2 is the stage's last step — the reviewer signs off from that evidence.

### LOW
- Async-convention nit: Step 2 should name `FindAllAsync` / `FindAsync` (async) throughout rather than implying sync calls; the `GuaranteeCorporation` select needs no `SelectList` of objects — a plain string `asp-items` loop or `SelectList(Model.Corporations)` suffices. Actor-level detail, no plan block.

## Missing Implementation Details
- The HIGH item above (exact distinct-query call). Everything else the actor needs is present: exact files, lines, symbols, return shapes, ids, edge cases.

## Approval Status
**APPROVED WITH CORRECTIONS** — apply the HIGH fix (FindAllAsync + in-memory Distinct) plus the three MEDIUM tightenings directly in plan.md; no second review needed.


---

## Planner handoff — Task 4 advice (ADVISORY ONLY, not a verdict)

> Added at owner request so plan mode has verified input for Task 4.
> The Job B PASS verdict above is unchanged and remains the only verdict
> in this file. Planner: overwrite `.clinerules/chat/plan.md` fresh for
> Task 4; do not treat this section as an approved plan.

### Next stage (from to-do.md)

- Task 4 — Color dropdown with "Add New" on Create Phone page. Same
  pattern as Manufacturer/Model: `<select>` from existing Colors,
  "Add New" button -> modal -> POST handler -> JSON refresh.

### Current state (verified from source this session)

- `src/MobileShop.Web/Pages/Products/CreatePhone.cshtml:58-62` — Color
  is a free-text `<input asp-for="Input.Color">`, NOT a `<select>`.
  Manufacturer (`:12-21`) and Model (`:23-32`) are the `<select>` +
  `input-group` + `Add New` button + Bootstrap modal pattern to copy.
- `src/MobileShop.Models/ViewModels/Web/BindModels/CreatePhoneInputModel.cs:28-29`
  — `Color` is `string?` + `[StringLength(50)]`; there is NO `ColorId`.
- `src/MobileShop.Web/Pages/Products/CreatePhone.cshtml.cs:96-99` —
  POST resolves the string via
  `colorRepo.FindAsync(c => c.Name == colorName) ?? AddColorAsync(colorName)`
  (silent auto-create, exact-match). `IColorRepo colorRepo` is already
  injected (`:8`).
- `Color` entity (`src/MobileShop.Models/Entities/Color.cs`) is just
  `Name` + `Products` nav; `IColorRepo`/`ColorRepo` are trivial
  `IBaseRepo<Color>`.
- `OnGetAsync()` (`:15-18`) loads only `Manufacturers`;
  `PopulateDropdownsAsync()` (`:139-144`) reloads `Manufacturers` +
  conditional `Models` — **Colors are never loaded**, so a new dropdown
  goes empty on validation failure unless both paths are fixed.
- `src/MobileShop.Tests/Web/Pages/Products/CreatePhoneModelTests.cs`
  covers manufacturer/model handlers + POST but has **zero Color
  coverage**.

### Decision the plan must make (do NOT leave to the actor)

- **Option A (minimal):** keep `Input.Color` as string, render `<select>`
  of existing color *names*. Smallest diff, preserves POST logic — but
  keeps free-text mismatch risk and invites duplicates ("Red" vs "red").
- **Option B (cleaner, bigger):** add `Input.ColorId int?`, bind the
  select to it, resolve via `FindAsync(id)`. Breaks the input contract;
  touches validation + all POST tests + the auto-create path.
- Either way state what happens to the **silent `AddColorAsync` path**
  (keep as fallback? delete?) and define the duplicate rule explicitly:
  current lookup is exact-match (`c.Name == colorName`), so
  casing/whitespace duplicates are likely — require `Trim()` + a rule.

### Pattern to reuse (exact references)

- Modal markup `CreatePhone.cshtml:111-160`; JS fetch POST
  `?handler=CreateManufacturer/CreateModel` (`:216-260`): trim,
  empty-guard, `response.ok`, append `<option>`, set `select.value`,
  `dispatchEvent(change)`, `bootstrap.Modal.hide()`, clear input.
- Handler shape `CreatePhone.cshtml.cs:26-39`: `400` on blank, trim,
  return-existing-on-duplicate, else `AddAsync` + `{id, name}` JSON. New
  `OnPostCreateColorAsync(string name)` must mirror this or justify
  deviating.
- New `Colors` page property + `id="colorSelect"` / `addColorModal` /
  `saveColorBtn` / `newColorName` ids must be unique on the page.

### Planner must inspect before writing steps

1. **Antiforgery on fetch POSTs** — existing manufacturer/model
   `fetch(..., {method:'POST', headers:{'Content-Type':'application/json'}})`
   sends no `RequestVerificationToken`. Check `Program.cs` / antiforgery
   config to see why that works; require the Color handler to do exactly
   the same (fixing all three together is scope creep — call it out).
2. **Seed data** (`sample-data.json`, `SampleDataLoader`) — are Colors
   seeded? If empty, the dropdown renders placeholder-only.
3. **Task 7 interaction** — Task 7 changes null-Color display to "N/A".
   Keep Color nullable end-to-end; do NOT make the new dropdown
   `[Required]`.
4. **No cascade** — Colors are global, unlike Models (per-manufacturer):
   no `OnGetColorsAsync(manufacturerId)` equivalent. Do not copy that part.
5. **Project rules** — stays in Web/Razor layer; no API, auth, or
   DB-init-policy change.

### Tests + verification the plan must require

- Extend `CreatePhoneModelTests.cs`: create-new-color,
  duplicate-returns-existing, blank -> 400, and
  "validation failure repopulates Colors" (the `PopulateDropdownsAsync`
  gap above is the highest-risk regression).
- Include **served-HTML/browser evidence** (Task 3 proved build+tests
  cannot see rendered widgets): options present on GET, modal adds +
  selects without reload, POST persists. Plus the standard chain
  `dotnet build src/MobileShop.slnx --nologo && dotnet test
  src/MobileShop.slnx --nologo --no-build`.

### Risks to label honestly

- MEDIUM: the `string` vs `ColorId` contract choice — wrong pick means
  rework of POST + tests.
- LOW-MEDIUM: duplicate color names via casing/whitespace.
- LOW: modal/JS id collisions; validation-failure empty dropdown.

