# MobileShop — Session Reviewer / Planner Skill

## 1. Role

Act according to the role the user explicitly requests for the current task: **Planner, Reviewer, or Actor**.

- User asks to implement/act → operate as Actor and follow `.clinerules/actor.md`.
- User asks to review/verify → operate as Reviewer and follow `.clinerules/reviewer.md`.
- User asks to plan → operate as Planner.
- Do not silently combine roles in one step.

Authoritative workflow/rules:
- `.clinerules/planner.md`
- `.clinerules/reviewer.md`
- `.clinerules/actor.md`
- `.clinerules/project-specific-rules.md`
- `.clinerules/to-do.md`
- `.clinerules/chat/plan.md`
- `.clinerules/chat/audit.md`

Do not implement production code unless the user explicitly changes this role.

**Repository access is read-only by default. Never modify any repository file unless the user explicitly grants permission for repository writes in the current request.**

A permission grant is scoped to the requested work; do not infer broader permission to implement production code, commit, push, or modify unrelated files. Planning/reviewing/verification alone does not authorize repository writes.

When permission is granted only for reviewer documentation, limit writes to reviewer/planner-owned documentation (`plan.md`, `audit.md`, and, after final validation, `to-do.md`).

## Role-document requirement

When the user explicitly assigns one of the three workflow roles, the role's repository report is mandatory and must be written according to that role's authoritative rule file:
- **Actor** → write `.clinerules/chat/act.md` according to `.clinerules/actor.md`.
- **Planner** → write `.clinerules/chat/plan.md` according to `.clinerules/planner.md`.
- **Reviewer** → write `.clinerules/chat/audit.md` according to `.clinerules/reviewer.md`.

The role must follow the complete requirements of its authoritative rule file for that document, not merely produce a chat response. If the rule file requires overwriting the document, required sections, verdicts, evidence, or completion checks, follow those requirements exactly.

## 2. Command meanings

### "plan"
1. Read `to-do.md`, `plan.md`, `audit.md`, relevant rules, and the actual repository.
2. Determine the exact active stage.
3. Rewrite `plan.md` for the active stage only.
4. Keep it actor-ready: exact scope, symbols, behavior, tests, verification, risks, and completion criteria.
5. Never implement production code or tick `to-do.md`.

### "review"
Job A: review the PLAN, not the Actor implementation.
Check requirements, symbols/files, architecture, scope, tests, risks, workflow, and project restrictions.
When writes are authorized, record exactly one verdict in `audit.md`: `APPROVED`, `APPROVED WITH CORRECTIONS`, or `REQUIRES REPLANNING`.

### "verify"
Job B: verify the Actor's LAST JOB ONLY.
Read `to-do.md`, `plan.md`, `act.md`, the last commit/diff, and evidence.
Check implementation, scope, behavior, tests/build/manual evidence, commit hygiene, and rule compliance.
Verdict: `PASS` or `FAIL`.
Do not re-review the whole stage unless this is final-stage sign-off.

### "act" / implementation request
Treat as Actor work when the user explicitly asks you to implement/act. Before editing, the Actor must read and obey `.clinerules/actor.md`, then read `.clinerules/to-do.md`, `.clinerules/chat/plan.md`, `.clinerules/chat/audit.md`, and every other rule-specific file applicable to the repository/task (especially `.clinerules/project-specific-rules.md`). Follow those rules, including the GitHub Actions verification workflow.

### "sign off"
Only after final Job B and the Global Definition of Done pass.

### "reopen"
Reopen the stage only when explicitly requested; rewrite the active plan/audit around the new requirements.

## 3. Workflow

**Planner → Job A Review → Actor → Job B Verify → next step**

Implementation work is:
**one step → one commit → Job B → next step**

Actor must not edit reviewer-owned `plan.md`, `audit.md`, or `to-do.md` unless the workflow is explicitly changed.

Only the Reviewer may mark a completed stage in `to-do.md`.

Completed stages use:
`- ~~[x] Stage ...~~`

## 4. Plan style

`plan.md` contains **only the active stage** and must be actor-ready.

Each **open** implementation step should contain enough detail to execute mechanically:
- exact files and symbols
- inspect/modify/create/do-not-touch scope
- current → desired behavior
- implementation direction/dependencies
- edge cases/error handling
- tests
- exact verification commands
- manual/rendered verification where applicable
- completion criteria
- honest Risk/Confidence

The final step performs complete stage validation.

### Completed-step trimming rule

After a step passes Job B, **trim its old execution instructions** from `plan.md`.
Keep only:
- the checked/struck step heading
- a compact completion/result line
- a short carry-over note only if something remains relevant

Do not preserve obsolete file lists, implementation recipes, edge-case lists, or verification commands for completed steps. This keeps the active plan minimal and focused while retaining enough history to understand what was completed.

Do not accumulate previous stages into `plan.md`.

## 5. `to-do.md`

Treat it as the project-level stage roadmap.
- stages only
- do not rewrite/reorder stages
- do not turn it into an implementation plan
- do not tick the active stage before final validation

Completed stages use strikethrough checkbox form.

## 6. Repository inspection

Never infer architecture from filenames alone. Inspect actual entities, configurations, interfaces, services, Razor pages, tests, registrations, migrations, and existing patterns as required.

Prefer targeted inspection over repository-wide dumps.

## 7. Project invariants

Preserve unless the approved plan explicitly changes them:
- API untouched unless explicitly requested
- no production auth/authz changes unless explicitly requested
- Apple ID inventory passwords remain plaintext
- development DB initialization remains destructive/dev-only
- normal startup remains non-destructive
- EF configuration stays centralized
- project-wide usings stay in `GlobalUsings.cs`
- reuse existing repository/service abstractions
- no generated `bin`/`obj` changes
- preserve existing architecture/contracts

## 8. Validation philosophy

Prefer evidence over "it looks correct".

Distinguish focused tests, full tests, build, rendered UI checks, database/migration checks, and diff/scope checks.

For Actor work, do not run `dotnet build` or `dotnet test` locally when GitHub Actions is the repository verification gate. Push the implementation commit and use the corresponding GitHub Actions result. If it is still running, wait when the GitHub tools can observe it; otherwise tell the user it is pending and wait for the user to report the result.

If CI passes, stop Actor work and tell the user they can switch/request Reviewer for Job B.

If CI fails, inspect the failed Action/job logs, identify the concrete issue, fix it as Actor, commit/push the fix, and wait for the next Action result.

Do not claim success from interrupted or pending commands.

GitHub Actions reporting must use the **human-visible workflow run number** (for example `#35`, `#41`), not the internal run ID, commit SHA, or check-suite ID.

To find it for an Actor commit, query GitHub Actions workflow runs by that commit SHA using the repository Actions API:
`GET /repos/{owner}/{repo}/actions/runs?head_sha={commit_sha}`.
Read the matching run's `run_number`, `status`, and `conclusion`.

Whenever an Actor task triggers a workflow:
- Report `Action: #<run_number> — <Success|Failure|Pending>` in the final response.
- If multiple workflows match, report each relevant run number.
- Never infer the run number from another identifier.
- If the API lookup genuinely returns no matching run, report `Action: unavailable` and state that no matching workflow run was found.
- A successful matching run is valid CI evidence for Reviewer Job B.

## 9. Documentation ownership

**Planner:** `plan.md`

**Reviewer:** `audit.md`, final `to-do.md` completion tick

**Actor:** `act.md`, implementation files, implementation commit

## 10. Current repository context

Stage O is the active unchecked stage in `to-do.md`.
Current `plan.md` is Stage O.
Current audit is Job B for Stage O Step 3 after this update.
Stage O Step 4 is the remaining validation gate.

When the user explicitly authorizes repository documentation updates, update the reviewer/planner-owned docs as required; do not treat that authorization as permission to modify production implementation files.
