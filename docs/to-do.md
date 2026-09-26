# MobileShop execution checklist

This file is the authoritative work order for the next act-mode agent. Follow it
literally and complete the stages in order.

## Strict rules for the agent

1. Work only on the first unchecked task whose prerequisites are complete. Do not
   skip ahead, combine unrelated stages, or redesign completed work.
2. Read the relevant existing code, tests, configuration, and seed-data structure
   before editing. Follow the repository's existing naming, layering, global-usings,
   repository, service, Razor Pages, and test conventions.
3. The scope is the development Web application only. Do not modify
   `src/MobileShop.Api`, API service stubs, API configuration, or API endpoints
   unless a later task explicitly adds that scope.
4. Do not add production authentication, authorization, cookie middleware, claims,
   login security, or other security boilerplate in these stages. Profile behavior
   is intentionally development-adapted so it can be exercised without real
   authentication.
5. Apple ID inventory passwords are intentionally stored as plaintext because the
   shop must be able to recover them for customers. Do not hash, encrypt, hide from
   the inventory workflow, or otherwise change this requirement. This does not
   change the separate hashed-password behavior for application users.
6. Preserve existing behavior outside the task. Do not perform broad refactors,
   rename public contracts unnecessarily, change database initialization policy, or
   edit generated build output, `bin`, or `obj` files.
7. Use async repository/service methods when an async equivalent already exists.
   Add async methods only when they fit the existing abstraction and can be used
   end-to-end. Use `IAsyncEnumerable` only for genuinely streaming/deferred
   collection flows; do not force it into Razor Page request handlers, small
   in-memory selections, or APIs that require materialized lists.
8. Every behavior change must have focused tests where practical. Run the smallest
   relevant test first, then run:
   `dotnet build src/MobileShop.slnx --nologo`
   and
   `dotnet test src/MobileShop.slnx --nologo`.
   Do not claim a task is complete when validation is failing.
   If a command can exceed the agent's 30-second command window, do not cancel it
   or report a timeout as a test failure: start it as an attached background
   command, keep its returned shell/session identifier, and read that same session
   until it finishes. Do not use shell `&`, `nohup`, `disown`, or detached
   processes. If the command still cannot be completed reliably, run the focused
   test project or several narrow `--filter` commands separately, then run the
   full build/test through the same attached-background procedure before completion.
9. Never silently swallow errors or return success-shaped fallbacks. Preserve the
   repository's logging and user-facing error patterns, and report missing data
   explicitly.
10. Do not delete a checklist item or completion evidence during ordinary stage
    work. The repository owner explicitly requested resetting the previous roadmap
    for this new set of demands; that reset is the only exception. Add future work
    at the end of the appropriate stage.
11. Only after implementation and validation are complete, change that task from
    `- [ ]` to `- [x]` and wrap the complete task text in Markdown strikethrough:
    `- [x] ~~task text~~`. Add a short indented completion note immediately below
    it containing the files changed, validation performed, and any intentional
    limitation.
12. Never check a parent task while any acceptance criterion or dependent task is
    unfinished. Do not check a task merely because the page compiles or a button
    renders; verify the requested behavior.
13. If a requirement is ambiguous or implementation would conflict with these
    rules, stop before editing and report the exact conflict. Do not invent a new
    product decision.
14. After each completed task, create and persist a separate Git commit before
    starting the next task. Use a clear Conventional Commits message in the
    existing project style, for example `fix(web): remove implicit Apple ID model`
    or `feat(web): generate factors for selected transactions`. The commit must
    contain only that task's related changes, must be created only after focused
    validation passes, and must include this trailer unless the user explicitly
    says otherwise:
    `Co-authored-by: Copilot <223556219+Copilot@users.noreply.github.com>`.
15. Do not push, reset, amend, or revert commits unless the user explicitly asks
    for that operation. If unrelated pre-existing changes are present, do not
    include them in the task commit.

## Stage 1 — Use the browser's native print flow for transaction factors

- [ ] Replace the Transactions Index's separate Print and Download PDF actions
  with one working `Print Factor` action. Keep the current direction, count, order,
  and row-selection behavior, but stop generating a server-side PDF from this page.

  Implementation requirements:
  - Update `src/MobileShop.Web/Pages/Transactions/Index.cshtml` and
    `Index.cshtml.cs`. Remove the `IPdfGenerator` dependency, both old Print and
    Download handlers, and all QuestPDF calls from this page. Do not remove or
    change the separate transaction Details factor flow.
  - Provide one GET page handler for `Print Factor`. Ensure the submitted
    `direction`, `take`, `order`, and repeated `selectedIds` values are bound on
    handler requests; do not depend on implicit binding that the tests do not prove.
  - Load the filtered, ordered, count-limited transaction list once. If no IDs are
    selected, print every transaction in that loaded list. If IDs are selected,
    print exactly those distinct positive IDs, in the submitted selection order,
    only when every ID exists in that loaded snapshot. If any selected ID is
    invalid or absent, redisplay with a visible validation error and do not enter
    print mode or print a partial factor.
  - Add an `IsPrintMode` (or equivalent) page state and an HTML factor layout that
    contains the selected/listed transaction date, direction, product, price, and
    relevant party information. In print mode hide filters, navigation/actions,
    checkboxes, and other screen-only chrome with `@media print`; keep the factor
    readable on paper and in the browser's Save as PDF output.
  - In print mode only, call `window.print()` after the factor HTML has rendered.
    Use the browser's native print dialog for printing or saving as PDF; do not
    return a PDF file, invoke QuestPDF, or create a download response.
  - Give the single button a clear enabled primary style and visible
    `Print Factor` label. Preserve the existing apply-filters and record
    buy/sell actions.

  Acceptance criteria:
  - The single action submits all active filters and all checked IDs.
  - Empty selection uses exactly the currently filtered/ordered/take-limited page
    list; non-empty selection uses only the selected rows from that same snapshot.
  - Invalid or out-of-snapshot IDs produce an error and no printable factor.
  - The response is HTML in print mode and triggers native browser printing;
    server-side PDF generation is absent from the Transactions Index path.
  - Focused page-model tests cover empty selection, one and multiple IDs,
    invalid/missing IDs, handler inputs, and print-mode state/content composition.
    Update `Transactions/IndexModelTests.cs` and add view/render coverage if the
    existing test setup supports it.

## Stage 2 — Show profit/loss percentage and color-coded results

- [ ] Add a `By percent` result column immediately after `Profit / loss` on
  `/Reports/ProfitLoss` and color both result values according to their sign.

  Implementation requirements:
  - Update `src/MobileShop.Models/ViewModels/Web/ProfitLossRowViewModel.cs` with
    a computed `ProfitPercent` property: when `Bought` is non-zero, calculate
    `(Profit / Bought) * 100`; when `Bought` is zero, return `0` to avoid division
    by zero. Preserve the existing `Profit` property and constructor.
  - Update `src/MobileShop.Web/Pages/Reports/ProfitLoss.cshtml`: put the heading
    `By percent` directly after `Profit / loss`; render `ProfitPercent` as a
    percentage with two decimal places and the culture-appropriate number format.
  - Apply Bootstrap `text-success` to both row values when `Profit >= 0` and
    `text-danger` to both when `Profit < 0`. Do not color Bought or Sold columns.

  Acceptance criteria:
  - Positive, zero, and negative profit values render with the specified color;
    the percentage formula uses Bought as its denominator and returns zero when
    Bought is zero.
  - The percentage column is immediately after Profit / loss and shows two
    fractional digits followed by `%`.
  - Add focused unit tests for the computed percentage at positive, zero,
    negative, and zero-Bought cases, and page/render tests for column order and
    both Bootstrap color classes.

## Stage 3 — Add manual and automatic Profit/Loss date ranges

- [ ] Keep the From/To date pickers and add a manual/automatic range selector,
  defaulting to automatic current-month reporting. In manual mode, default missing
  From/To values to the earliest transaction date and today respectively.

  Implementation requirements:
  - Update `src/MobileShop.Web/Pages/Reports/ProfitLoss.cshtml` and
    `ProfitLoss.cshtml.cs`; add focused tests under
    `src/MobileShop.Tests/Web/Pages/Reports/`.
  - Add a bound date mode with `Automatic` and `Manual` values. Missing mode means
    `Automatic`; missing automatic preset means `Month`. Provide automatic presets
    `Today`, `Week`, `Month`, and `Year`, and retain From and To date inputs for
    manual selection.
  - Automatic bounds are inclusive calendar dates: Today is today-to-today;
    Week starts Monday and ends today; Month starts on the first day of this month
    and ends today; Year starts January 1 of this year and ends today. Use the
    current local date, not UTC date truncation. Ignore manual From/To values while
    Automatic mode is selected.
  - In Manual mode, preserve supplied From and To values. For each missing bound,
    default From to the earliest transaction date in the database and To to
    today's local date. Add the smallest appropriate async method to the existing
    transaction service/repository abstraction to get the earliest transaction
    date without loading full transaction entities. Return an explicit nullable
    result for an empty transaction table; in that case default From to today and
    display a clear non-error note that no transactions exist yet.
  - If adding a service interface member, use a default-interface implementation
    that throws the existing API service's standard `NotImplementedException`
    message so API service stub files remain untouched. Do not modify API projects,
    API service stubs, or API configuration.
  - Pass the effective bounds to both profit/loss rows and total calculations.
    Ensure the automatic/manual controls preserve their selected state after Apply
    and after validation/redisplay. Use accessible labels and show the relevant
    controls for the selected mode without removing the manual date pickers.
  - Do not change the transaction filtering's existing inclusive date semantics.

  Acceptance criteria:
  - First visit defaults to Automatic + Month and computes first-of-month through
    today; each other preset returns the exact bounds specified above.
  - Switching to Manual with empty date values defaults to earliest transaction
    date through today. Supplied manual dates are preserved independently, and
    empty-database behavior is explicit and usable.
  - Both report rows and total use identical effective bounds.
  - Tests cover all four automatic presets, default mode/preset, each missing
    manual bound, preserved manual overrides, empty transaction data, and
    inclusive range behavior.

## Stage 4 — Limit profit distribution to Mikaeeil, Anis, and the Shop

- [ ] Configure the Profit/Loss distribution to include only Mikaeeil Jorjany at
  40%, Anis Sahabi at 50%, and the Shop with the remaining 10%, and seed the
  development data to support those rows.

  Implementation requirements:
  - Update `src/MobileShop.Services/Logging/Settings/DistributionSettings.cs` and
    `DistributionCalculator.Calculate` (or the smallest suitable adjacent layer)
    so the distribution result is limited to the two named employees and the
    Shop. Use the exact full names `Mikaeeil Jorjany` and `Anis Sahabi`; exclude
    every other active or inactive employee from the displayed distribution.
  - The two named employees' shares are 40% and 50% respectively; the Shop's
    displayed remainder is 10%. Do not derive shares from unrelated seeded
    employees or display the shop owner as an employee row.
  - For positive profit, calculate the named employee amounts from the total using
    their shares, round down to whole currency units consistently with the current
    calculator, and give the Shop the remainder including rounding differences.
    Preserve the current loss behavior: employees receive zero and the Shop
    absorbs the entire loss. Keep the displayed percentages at 40%, 50%, and 10%
    so the configured shares still sum to 100%.
  - Update `src/MobileShop.Dal/Initialization/sample-data.json` to contain active
    employee records for these names with the stated shares. Retain stable shop,
    seller, customer, product, and transaction IDs/references. Set any unrelated
    seeded employee records inactive or remove them from the employee collection
    so the distribution cannot accidentally include them.
  - Update seed-data validation/tests for changed people/employee counts and
    relationships. Add focused calculator tests proving the exact three rows,
    names, share percentages, amounts/remainder, unrelated-employee exclusion,
    and loss behavior.
  - Preserve the existing development-only seed initialization policy; do not
    change production behavior or add real contact details/credentials for the
    named employees.

  Acceptance criteria:
  - The Distribution tab contains exactly Mikaeeil Jorjany (40%), Anis Sahabi
    (50%), and Shop (10%) for a positive-profit period; no other employee appears.
  - Their calculated amounts plus the Shop amount equal the total profit exactly.
  - Loss periods retain the no-employee-payout behavior, assign the complete loss
    to the Shop, and keep the displayed 40%/50%/10% shares summing to 100%.
  - A fresh development database seed and the focused seed/calculator tests pass.

## Stage 5 — Fix Profile username wrapping and improve layout

- [ ] Improve the Profile page layout so the `Username` label stays on one line
  and profile details remain readable on narrow screens.

  Implementation requirements:
  - Update only the relevant markup in
    `src/MobileShop.Web/Pages/Account/Profile.cshtml` unless tests require a
    directly related change.
  - Give the definition-list label enough responsive width (for example,
    `col-sm-3` with a matching `col-sm-9` value column) and prevent the
    `Username` label itself from wrapping. Keep long username values readable by
    allowing the value column to wrap as needed.
  - Preserve the existing development-only profile/password-change behavior,
    validation feedback, labels, and responsive Bootstrap styling. Do not add
    authentication or security middleware.

  Acceptance criteria:
  - `Username` remains a single-line label at desktop and mobile widths.
  - Username/email values and password form remain readable and responsive.
  - Any existing Profile page tests continue to pass; add markup coverage only if
    the existing test conventions support it.

## Final validation

- [ ] After Stages 1–5 are implemented and committed separately, run the full
  solution build and test commands from the strict rules and inspect all failures.
  Manually verify the changed Web flows in a running development Web app:
  Transactions filter/order/count, no-selection Print Factor, single/multi-select
  Print Factor, invalid selection handling, native print dialog and Save as PDF;
  Profit/Loss percentage/color display, all automatic date presets, manual date
  defaults/overrides, distribution rows/amounts; and Profile layout/password flow.
- [ ] Review the final diff and worktree for accidental API or out-of-scope changes,
  generated files, secrets, plaintext application-user passwords, incorrect
  checklist formatting, and any unchecked acceptance criterion. Mark these final
  tasks complete only when manual and automated validation have both actually
  been performed. If browser verification is unavailable, leave the manual
  verification task unchecked and report the limitation rather than claiming
  completion.

## Checklist completion format

Every completed item must look like this:

```text
- [x] ~~Implement the completed task text.~~
  - Completed: `path/to/file.cs`, `path/to/test.cs`.
  - Validation: `dotnet test ...`; behavior manually verified.
  - Notes: intentional limitations or follow-up.
```

Never use `- [x]` without `~~...~~`, never leave completed text unstruck, and
never remove the completion note except during an explicitly owner-requested
checklist reset.
