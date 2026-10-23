## Actor report — Stage 20, Step 5 — Full validation

- Source changes: none in this step.
- CI: run #703 Success on `bcf18f6` (the final code). Earlier Step 1 failures (#697, #698) were fixed in #699.
- Rendered check: confirmed by the user on the final layout (`quantity-lines-factor.pdf`, run #703 artifact).
- Global Definition of Done: factor shows `تعداد` and `مبلغ پرداختی (ریال)`; identical-spec products merged with the right quantity; differing specs on separate lines; paid amount = quantity × `فی`; CI green; no schema, migration, entity, form, Api or invoice change.
