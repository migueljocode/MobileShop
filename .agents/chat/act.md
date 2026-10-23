## Actor report — Stage 20, Step 4 — Render the new columns in the factor table

- Commit: `0d6ec48` feat(pdf) — `RenderFactorTable` renders `model.Lines` with six columns, declared in this order (first declared is rightmost in the RTL content): `فی (ریال)` 105, `تعداد` 40, `مبلغ پرداختی (ریال)` 120, `طرف معامله` 1.15 relative, `محصول` 2.25 relative, `#` 35. `RenderFactorSummary`, the header and `ComposePagePersian` are unchanged.
- Tests: new unit test `GenerateTransactionFactor_renders_identical_rows_as_one_quantity_line`; artifact test gained `quantity-lines-factor.pdf` (20 identical glass rows, one blue and one green phone, one large price).
- CI: run #702 Success on `0d6ec48`. Artifact `factor-pdf-inspection` uploaded.
- Rendered check: PENDING. The artifact cannot be downloaded from the sandbox (the storage host is blocked). The user must open `quantity-lines-factor.pdf` from run #702 and confirm: the 20-row glass line shows `تعداد` 20 and `مبلغ پرداختی` = 20 × `فی`; blue and green phone lines are separate; the columns do not clip; the summary total equals the sum of the raw rows.
- Deviation: none.
