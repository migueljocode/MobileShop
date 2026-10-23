## Actor report — Stage 20, user-requested follow-up to Step 4 — swap فی and مبلغ پرداختی

- User confirmed the Step 4 render (run #702) and asked to swap the `فی (ریال)` and `مبلغ پرداختی (ریال)` columns.
- Commit: `bcf18f6` refactor(pdf) — `RenderFactorTable` declared order is now `مبلغ پرداختی` 120, `تعداد` 40, `فی` 105, `طرف معامله`, `محصول`, `#`. Reading right to left: `#`, محصول, طرف معامله, فی, تعداد, مبلغ پرداختی. Widths move with their columns. No other change.
- Deviation: the planned order from assumption E is replaced by this user request. Only the two columns named were swapped.
- CI: run #703 Success on `bcf18f6`.
- Rendered check of the swapped order: PENDING (user to confirm in the artifact of run #703, `quantity-lines-factor.pdf`).
