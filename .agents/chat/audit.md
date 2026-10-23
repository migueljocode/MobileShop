## Reviewer Audit — Stage 20, Job B (Step 4)

**PASS on code, rendered check PENDING.** Verified commit `0d6ec48`: `RenderFactorTable` declares the six columns in the planned order and widths; header and body cell order match; cells use `UnitPrice`, `Quantity`, `PaidAmount`, `PersonLabel`, `ProductText`; the summary and invoice code are untouched. Tests cover one quantity line and the artifact sample. GitHub Actions **#702 — Success**.

**Not signed off for this step:** the Definition of Done needs a rendered-PDF check (column order, no clipping, correct paid amount). The artifact `factor-pdf-inspection` from run #702 contains `quantity-lines-factor.pdf` and must be checked by a person. Until that is recorded, Step 4 stays open and Step 5 (validation) cannot be signed off.
