# Audit — Job B (Execution Check): Stage V Step 2 (`88d8ee2`)

**Verdict: PASS. Step 2 is closed; Step 3 is authorized.** One MEDIUM (unintended edits in the smoke script) is appended to the Step 3 block in `plan.md`.

- **CI evidence (actor-recorded):** `Action: #432 — Success` (run `37259629990`): build, tests, Bash/PowerShell checks, factor PDF artifact and Production smoke green. I could not read the run myself (GitHub API rate limit).
- **Page and model (checked):** `IndexModel.Availability` and `AvailabilityRoute`; `OnGetAsync(type, partNumberId, availability)` trims and lowercases, keeps only `"available"`/`"sold"`, and passes `null` for All; the view renders one GET form (hidden `type`, an `asp-for="Availability"` select with the three options and `onchange="this.form.submit()"`, and the unchanged phones-only part-number select) and the four type links carry `asp-route-availability`. Because the options are written in the page itself, the select tag helper marks the posted choice as selected.
- **Tests (checked):** five `IndexModelTests` cover default, `"Sold"`/`" sold "`, `"available"`, unrecognised, and phone + part number + availability together.
- **Smoke (checked):** the sold route must contain `text-bg-secondary">Sold` and no `text-bg-success">Available`, the available route the reverse, and `/Products?type=phone&availability=sold` must return 200, exactly as planned.

## MEDIUM — unintended changes in `.github/scripts/production-smoke.sh` (appended to `plan.md` Step 3)
The diff of `88d8ee2` shows edits outside the planned assertion block: four `printf` format strings changed from `\n` to `\\n` (the local-run guard message and the backup listings, including the `Backup:` list written to the artifact summary), and the file mode changed from `100755` to `100644`. CI stayed green because the workflow runs the script through `bash` and the lines affected are messages and artifact text, but the listings no longer break lines and the executable bit is lost. The actor reports "Problems: None", so this slipped through the report. Step 3 restores the strings and the mode and requires the script diff against `8415a3f` to contain only the planned assertion block.

## LOW
- `act.md` notes the changes were composed through the GitHub Git data API; that tooling is the likely cause of the escaped backslashes and the dropped mode. The plan now asks for a diff check of backslashes and file mode after any such write.

## Gate
Next: the actor does **Stage V Step 3** (script repair first, then the final validation). One step → one commit → green run before merge → report `Action: #<run_number>` → STOP for Job B.
