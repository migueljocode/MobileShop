# Act — Stage L — Step 1 — Transactions filters auto-apply

## Summary
Step 1 removes the manual "Apply filters" button and replaces it with automatic form submission on Direction and Order change (immediate) and Count (debounced 300ms). The Download Factor button and row selection checkboxes remain unchanged.

## Changes
- `src/MobileShop.Web/Pages/Transactions/Index.cshtml`:
  - Removed the "Apply filters" submit button (kept Download Factor (PDF) button with its handler semantics intact)
  - Added `@section Scripts` block with vanilla JavaScript that:
    - Submits the GET form on `direction` `change` event via `form.requestSubmit()`
        - Submits the GET form on `order` `change` event via `form.requestSubmit()`
    - Submits the GET form on `take` `input` event after a 300ms debounce (using a timer that clears on each keystroke)

## Verification
- Build: `dotnet build src/MobileShop.slnx --nologo --no-restore` → Build succeeded. 0 Warning(s), 0 Error(s).
- Full suite: `dotnet test src/MobileShop.slnx --no-build --no-restore --nologo` → Passed! Failed: 0, Passed: 245, Skipped: 0, Total: 245.
- Confirmed `_Layout.cshtml` line 75 renders `@RenderSectionAsync("Scripts", required: false)` — the page-local script will be served.
- The form uses `method="get"`, so `requestSubmit()` triggers a GET to `OnGetAsync` with `direction`, `take`, and `order` parameters — exactly matching the existing server-side handling.
- Download Factor button retains `name="handler" value="DownloadFactor"` — auto-submit from JS does not include this (no button clicked), so it falls through to `OnGetAsync` correctly.
- No new JS dependencies introduced (plain browser JS only).

## Changed files
- Modified: `src/MobileShop.Web/Pages/Transactions/Index.cshtml` — removed Apply filters button, added Scripts section with auto-submit logic

## Commit
`pending`

## Limitations
None.

## Friction noted
- The editor tool auto-escaped HTML and introduced indentation issues on the `<a>` tag line and `</form>` line; had to use sed to restore correct indentation.
- Full test suite (~35s) exceeds the 30s tool timeout; ran in background and polled output file.

## Problems
None.

## Status
COMPLETE — Step 1 (Transactions filters auto-apply) is implemented and verified. All 245 tests pass. Stopping for Job B review.
