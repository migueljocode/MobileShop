# Audit — Step 5 (Execution Check)

## PASS

Commit `3de5feb` implements exactly what plan.md Step 5 asked, and the step's verification genuinely passes — not just a green build.

Verified against the diff: `ICustomerDataService` / `ISellerDataService` take `sortBy = "Name", bool ascending = true`; both DAL services order via a `(sortBy, ascending)` tuple switch covering `Phone`, `Count` and default `Name`, with invalid values falling back to `Name`; `CustomersModel` / `SellersModel` expose `SortBy` / `Ascending` and pass the params through; both `.cshtml` headers are `asp-route-sortBy` / `asp-route-ascending` toggles that flip direction for the active column.

The tests actually prove the behavior: `GetListRows_sorts_by_name_phone_and_count_in_both_directions` asserts concrete orderings for Name/Phone/Count × asc/desc, the invalid-key fallback, and sync/async parity; the new `CustomersModelTests` / `SellersModelTests` cover the default (`Name`, `true`), explicit passthrough, and the omitted-`ascending` default.

Re-run by the reviewer rather than trusted from act.md: `dotnet build src/MobileShop.slnx --nologo` → 0 Warning(s), 0 Error(s); `dotnet test src/MobileShop.slnx --nologo --filter "CustomerDataServiceTests|SellerDataServiceTests|CustomersModelTests|SellersModelTests"` → 14 passed, 0 failed; full `dotnet test src/MobileShop.slnx --nologo` → 405 passed, 0 failed, 2 skipped (pre-existing QuestPDF skips). Working tree clean, commit message follows Conventional Commits and names the step, no scope creep.

Accepted deviation: the plan's "Api stub classes need no edit" was wrong — both stubs explicitly implement the **sync** `GetListRows`, so the new signature forced a one-line fix (CS0535). Behavior is unchanged (still `NotImplementedException`) and the actor reported the friction. The plan scoped only the async method while the sync one was extended too; that preserves sync/async parity, is source-compatible, and all call sites were updated. Recorded in the Step 5 completion note.

Bookkeeping done in this pass: `to-do.md` Step 5 struck through with its instruction block trimmed to a single completion note; `plan.md` overwritten with Step 6 only.

## Next
## [ ] Step 6 — Reports page: Total Profit sign/color + Distribution sort by Share%
