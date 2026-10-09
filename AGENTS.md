# MobileShop — Agent Rules
**Planner → Job A (review plan) → Actor (one step) → Job B (verify that step) → next step.**
- Your role is what the user assigns (agent, command or first line) — never the tool's built-in mode. Don't mix roles in one pass.
- Then read `.agents/roles/<role>.md` and `.agents/project.md` (opencode/Kilo auto-load the latter; otherwise read it yourself). Chatbots without repo tools also read `.agents/chatbot.md`.
- Finish by naming who goes next. Never switch agent/mode or start another role's work yourself.

## Start — fresh session; these files are your only context
`.agents/to-do.md` → `.agents/chat/plan.md` → `.agents/chat/audit.md` → `.agents/chat/act.md` (Job B only).
Inspect the real code before acting; never infer architecture from filenames. Read targeted slices, not dumps.

## State files (in `.agents/`)
| File | Written by | Notes |
|---|---|---|
| `to-do.md` | Planner adds approved stages (unchecked); Reviewer ticks | Checkbox + one-line title per stage; never reorder or rewrite. Done = `- [x] ~~Stage N — title~~` |
| `chat/plan.md` | Planner; Actor strikes its own step heading | Active stage only; overwritten per stage |
| `chat/audit.md` | Reviewer | One verdict; overwritten each time |
| `chat/act.md` | Actor | One step report; overwritten each time |

An assigned role always writes its own file; nobody edits another role's file beyond this table.

## Commands
`plan` Planner: rewrite `plan.md` for the active stage only, actor-ready · `review` Reviewer Job A: review the PLAN, not code · `verify` Reviewer Job B: the Actor's last step only · `act` Actor: exactly one step · `sign off` Reviewer: only after the final Job B PASS and the Global Definition of Done · `reopen` only when explicitly asked: rewrite the active plan/audit around the new requirements.

## Rules
- **Severity:** only CRITICAL/HIGH findings block or force changes; MEDIUM/LOW are notes. One correction round — never another over MEDIUM/LOW.
- **Minimal change:** no unrelated refactors, renames, dependency bumps or speculative abstractions (mark them OUT OF SCOPE); reuse existing abstractions. Planner and Reviewer never write production code.
- **Conflicts:** if a request, step or plan is ambiguous or contradicts `project.md`, stop and report the exact conflict — never invent a decision.
- **CI is the only build/test gate.** Don't run `dotnet build`/`test` locally unless the user asks or CI is unavailable. The Actor pushes its step commit. CI passes → stop and tell the user to request Job B. CI fails → read the failed job logs, fix, commit, push, wait for the next run. Run not observable → report Pending and wait for the user. Never claim success from pending or interrupted runs.
- **CI evidence:** report `Action: #<run_number> — Success|Failure|Pending` — the human-visible run number, never the run ID, SHA or check-suite ID. Find it with `GET /repos/{owner}/{repo}/actions/runs?head_sha={sha}` → `run_number`, `status`, `conclusion`. List each relevant workflow; no match → `Action: unavailable`; never infer a number. A successful matching run is valid CI evidence for Job B, and every DB/migration/CI claim in a review cites its run number.
- **Commits:** Conventional Commits (`feat fix refactor test docs chore perf build ci`), no `Co-authored-by`. `git add` named files only — never `-A` or `.`. Never amend, reset, revert or force-push unless asked. Reviewer never pushes.
- **Shell (bash):** independent reads in one brace group — `{ git ls-files | head -50; wc -l f.cs; grep -rn "X" src | head; }` (spaces and closing `;` required). Dependent steps with `&&`, ~4 per chain, one purpose each. Around `grep`/`diff` (exit 1 on no match) use `;` or a group, not `&&`. Other directory: `(cd src/Foo && cmd)`. Keep output and reports compact.
