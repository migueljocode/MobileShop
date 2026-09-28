# Plan — Task 3: Fix Manual Date Range mode (round 4 — the pickers never get a VALID `value`)

## Reviewer Briefing

- **The previous three rounds all reported COMPLETE and all three were wrong**, because every round
  verified with `dotnet build` + unit tests. Those can **never** observe a rendered `value` attribute.
  This plan's verification reads the **served HTML** instead, and captures it **before** the edit so
  the root cause is confirmed rather than assumed. If the baseline does not match the diagnosis,
  the step STOPS — it does not apply the fix blind.
- **Owner-reported symptom (the diagnostic fingerprint)**: pickers show the empty-state placeholder
  `mm/dd/yyyy`, **yet clicking Apply still lists products.** Those two facts together are explained
  only by the two defects below — the filter works and is *far wider* than the UI admits.
- **HIGH confidence on both defects**; see Root Cause for the exact evidence and its limits.
- **Step 1 is the only step that changes code** (one file, two lines). Step 2 is validation only.
- **Do not** add a datepicker library, change `Html5DateRenderingMode`, or touch
  `DatabaseInitializer` — all out of scope and/or forbidden by project rules.

---

## Root Cause — two compounding defects in how the two date inputs get their value

`src/MobileShop.Web/Pages/Reports/ProfitLoss.cshtml:30-31` currently read:

    <input asp-for="From" type="date" asp-format="yyyy-MM-dd" class="form-control" />

### Defect 1 — `asp-format="yyyy-MM-dd"` emits the literal text `yyyy-MM-dd`

`asp-format` is passed **verbatim** as the format argument into
`DefaultHtmlGenerator.GenerateInput`. That argument is a **composite** format string. Evidence:

- `InputTagHelper._rfc3339Formats` maps the date input type to a composite format:
  `{ "date", "{0:yyyy-MM-dd}" }`.
- `InputTagHelper.GetFormat(...)` returns that same `{0:...}` shape as its fallback, and so does
  `ModelMetadata.EditFormatString` (fed by `[DisplayFormat(DataFormatString = "{0:...}")]`).
- The `InputTagHelper.Format` API doc links to the *Composite Formatting* article and states Format
  is forwarded to `GenerateTextBox(..., format: Format, ...)`.

With **no `{0}` placeholder**, `string.Format("yyyy-MM-dd", value)` returns the string
`"yyyy-MM-dd"` itself. Razor therefore emits `<input type="date" value="yyyy-MM-dd" />`, which is
**not a valid HTML5 date string**, so the browser discards it and paints the empty-state placeholder
— exactly the `mm/dd/yyyy` the owner sees. This also explains why commit `03e2185` (which *added*
`asp-format`) changed nothing.

### Defect 2 — `asp-for` renders the raw query-string value, not the model property

Confirmed by **dotnet/aspnetcore#7653**, closed **"Resolution: By Design"**:

> "When a QueryString use ModelBinding then the InputTagHelper will not use the value from Model.
> The issue caused by the ModelState, ModelBinding add a AttemptedValue to the ModelState"
> — repro `?A=666` with model `A = 11` renders **666**.

So on an Apply round trip (`?Mode=Manual&From=&To=`), `ModelState["From"].AttemptedValue == ""`
**beats** the non-null `Model.From` written back at `ProfitLoss.cshtml.cs:55`. The picker stays blank
even when the model value is correct — i.e. the input can never show the range it filtered on.

### Why Apply still lists products

Empty inputs submit `From=` / `To=`. In the Manual branch `ProfitLoss.cshtml.cs:51-52`
(`EffectiveFrom = From ?? (earliest ?? today)`) null-coalesces back to `earliest` .. `today` — the
**widest possible range** — so every product is returned. The filter is not broken; it is silently
running a different range from the one the UI displays.

### Ruled out

- **Stale build**: `bin/Debug/net10.0/MobileShop.Web.dll` (20:05:57) is newer than
  `ProfitLoss.cshtml` (19:59:00), so the round-3 Step 1 pre-population IS compiled in. The bug is in
  the current code, not a caching or rebuild artifact.
- **Server-side bounds logic**: `ProfitLossTests` (16 passing) already proves `From`/`To` are
  populated and `EffectiveFrom`/`EffectiveTo` stay preset-governed. `ProfitLoss.cshtml.cs` needs
  **no change** for this task.

### Expected values (derive at run time, do not hardcode blindly)

- `From` -> earliest transaction date. At time of writing: `2026-01-05` (26 transactions,
  `min(Date)=2026-01-05`, `max(Date)=2026-03-05`).
- `To` -> today. At time of writing: `2026-09-28`.
- Re-read with `sqlite3 MobileShop.db "select min(Date) from Transactions;"` during verification.

---

## ~~[x] Step 1 — Emit a valid ISO `value` on both date inputs, and prove it in the served HTML~~

- **Files**:
  - modify: `src/MobileShop.Web/Pages/Reports/ProfitLoss.cshtml` (lines 30-31 only)
  - modify: `.clinerules/chat/plan.md` (tick this step)
  - do not touch: `ProfitLoss.cshtml.cs` + `ProfitLossTests.cs` (already correct — proven by tests),
    services, repos, DAL, `_ViewImports.cshtml`, `site.css`, DB init, API
- **Symbols**: the two `<input>` elements for `Model.From` and `Model.To` bound via `asp-for`
- **Current -> Desired**:
  - Current: `asp-for` + `asp-format="yyyy-MM-dd"` -> emits `value="yyyy-MM-dd"` or `value=""`
    (Defects 1 and 2) -> browser shows the `mm/dd/yyyy` empty-state placeholder.
  - Desired: a plain HTML date input whose `value` is a guaranteed-valid ISO date string built from
    the model, with **no `asp-for`** on the input, so neither the composite-format path nor the
    `ModelState.AttemptedValue` override can interfere.
- **Change** — replace lines 30-31 with exactly:

      <div class="col-md-3"><label class="form-label" asp-for="From">From</label><input type="date" id="From" name="From" class="form-control" value="@(Model.From?.ToString("yyyy-MM-dd", System.Globalization.CultureInfo.InvariantCulture))" /></div>
      <div class="col-md-3"><label class="form-label" asp-for="To">To</label><input type="date" id="To" name="To" class="form-control" value="@(Model.To?.ToString("yyyy-MM-dd", System.Globalization.CultureInfo.InvariantCulture))" /></div>

  - **Keep** `id="From"` / `id="To"` so the labels' `for` attribute (generated by the separate
    `LabelTagHelper` from `asp-for` on the `<label>`) still resolves, and so `name` still binds to
    the page model on the GET submit.
  - **`asp-format` MUST be deleted** with `asp-for` on the input: it is an `InputTagHelper`-only
    attribute, and if left behind it renders as a stray literal `asp-format="..."` HTML attribute.
  - `System.Globalization.CultureInfo.InvariantCulture` is fully qualified on purpose, to avoid
    editing the shared `_ViewImports.cshtml`. It guards against non-Gregorian server calendars
    (e.g. `fa-IR`) rendering a different year.
- **Order of operations (the baseline capture is mandatory — do it FIRST)**:
  1. `cp MobileShop.db /tmp/MobileShop.db.bak` — safety net (see Execution notes).
  2. Start the app and capture the **current** HTML (recipe in Execution notes).
  3. **Assert the baseline matches the diagnosis** -> expect `value="yyyy-MM-dd"` (Defect 1) and/or
     `value=""` on the `From=`/`To=` round trip (Defect 2). **If neither appears, STOP and report**
     the captured HTML verbatim — the root cause is wrong and the fix must not be applied blind.
  4. Apply the edit above.
  5. `dotnet build src/MobileShop.slnx --nologo` then re-capture the served HTML.
- **Edge cases / error handling**:
  - Empty DB (`earliest == null`): `From` -> today, `To` -> today; the null-conditional renders
    `value=""`, which is correct and still shows as the placeholder. No crash.
  - Null `From`/`To`: `?.ToString(...)` yields nothing -> `value=""`. Valid HTML.
  - Manual mode with explicit dates: the query values are what the model holds, so the emitted
    `value` is the same date, re-rendered ISO. Round-trips exactly.
  - Do **not** remove the `EmptyDatabaseNote` block or change the JS toggle from round 3.
- **Tests**: none to add — this is pure view rendering, which the existing test project cannot
  observe (that gap is exactly why three rounds passed while broken). Regression cover is the
  served-HTML capture below plus the existing `ProfitLossTests` suite.
- **Verify**: (a) focused: `dotnet test src/MobileShop.Tests/MobileShop.Tests.csproj --filter "ProfitLossTests"`
  -> all pass; (b) the decisive check: served HTML contains
  `value="<earliest>"` on `id="From"` and `value="<today>"` on `id="To"`, and contains **no**
  `value="yyyy-MM-dd"`; (c) the round trip: `?Mode=Manual&From=&To=` still renders those same
  two values (proves Defect 2 is gone).
- **Done when**: the served HTML for both `?Mode=Manual` and `?Mode=Manual&From=&To=` carries valid
  ISO `value` attributes; `ProfitLossTests` passes; no `value="yyyy-MM-dd"` anywhere.
- **Risk**: LOW (two lines, one file, no logic change) | **Confidence**: HIGH

---

## [ ] Step 2 — Full-suite regression + stage Definition of Done

- **Files**: none expected to change. If Step 1's build produced no further edits, this step
  commits only the `plan.md` tick.
- **Change**: none. Validation only.
- **Verify**: `dotnet build src/MobileShop.slnx --nologo && dotnet test src/MobileShop.slnx --nologo --no-build`
  (run to completion — see Execution notes about the shell window), plus a final served-HTML capture.
- **Done when**: 0 build warnings/errors, whole suite green, and every Global Definition of Done
  line below is evidenced on the record.
- **Risk**: LOW | **Confidence**: HIGH

---

## Global Definition of Done

- Served HTML for `?Mode=Manual` renders `value="<earliest>"` on `id="From"` and `value="<today>"`
  on `id="To"`, both valid ISO (`yyyy-MM-dd`).
- The string `value="yyyy-MM-dd"` does **not** appear in the served HTML.
- Served HTML for `?Mode=Manual&From=&To=` renders the **same** two values (model wins over the
  empty query string).
- The date pickers are no longer blank as a placeholder on a fresh load; toggling Automatic <-Manual
  is still an instant client-side toggle with no reload (round 3 behaviour preserved).
- Automatic mode still filters by Preset; `EffectiveFrom`/`EffectiveTo` logic untouched.
- `dotnet build src/MobileShop.slnx --nologo` -> 0 errors, 0 warnings.
- `dotnet test src/MobileShop.slnx --nologo --no-build` -> all pass.
- No modifications to `ProfitLoss.cshtml.cs`, services, repos, DAL, the API, the DB init policy, or
  any other page.
- **Owner-visible expectation (state it in the report, do not claim otherwise):** the picker will
  *display* `01/05/2026`-style text, because a native `<input type="date">` always renders its
  value in the browser/OS locale. The emitted and submitted value is `yyyy-MM-dd`. No attribute can
  make the on-screen glyphs read `2026-01-05`.

## Out of scope
- Task 6 (auto-refresh on combobox change / removing Apply) and Task 8 ("All" preset).
- Any datepicker library, any `Html5DateRenderingMode` change, any new dependency.
- Reworking `ProfitLoss.cshtml.cs` bounds logic, which tests already prove correct.
- `DatabaseInitializer` / database seeding policy.

## Execution notes for the Actor

- **Running the app for the HTML capture (both captures, same recipe)**:

      cd /home/mikaeeil/Documents/CSharp/MobileShop
      cp MobileShop.db /tmp/MobileShop.db.bak
      export ASPNETCORE_ENVIRONMENT=Production
      dotnet run --project src/MobileShop.Web --no-build --no-launch-profile --urls http://localhost:5199 > /tmp/app.log 2>&1 &
      APP=$!
      for i in $(seq 1 60); do curl -sf -o /dev/null http://localhost:5199/ && break; sleep 1; done
      curl -s "http://localhost:5199/Reports/ProfitLoss?Mode=Manual" | grep -oE 'id="(From|To)"[^>]*'
      curl -s "http://localhost:5199/Reports/ProfitLoss?Mode=Manual&From=&To=" | grep -oE 'id="(From|To)"[^>]*'
      kill $APP

  - `ASPNETCORE_ENVIRONMENT=Production` is **essential**: `WebApplicationBuilderExtensions.ConfigureApp()`
    only calls `DatabaseInitializer.InitializeForDevelopment` when `IsDevelopment()`, and that method
    **drops and recreates** `MobileShop.db`. Running in Development would wipe the owner's data.
  - `--no-launch-profile` is equally essential: `launchSettings.json` otherwise forces
    `ASPNETCORE_ENVIRONMENT=Development` and re-triggers the drop. `UseApi` is absent from
    `appsettings.json` -> `GetValue("UseApi", false)` -> the DAL path is used.
  - The `&` is only to hold the server open while `curl` runs; it is **killed in the same command**.
    Nothing is left detached (no `nohup`/`disown`), and no success/failure is judged before the
    curls return. If the harness rejects `&`, run the server as one long-lived command and the two
    `curl`s as a separate command in the same tool call.
  - Restore afterwards if needed: `cp /tmp/MobileShop.db.bak MobileShop.db` (mtime will show a
    reseed if anything went wrong).
- Builds and the full test suite exceed the ~30s shell window. Use a "proceed while running" session
  and keep reading it to completion; never background a test run or judge it early.
- Commit with `git add` naming only the files this step touched (never `-A`/`.`).
- **Report honestly.** If the picker still renders blank, say so plainly and attach the captured
  HTML — do not report COMPLETE on a green build. That is the exact failure mode of rounds 1-3.
