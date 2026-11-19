# Chatbot rules (no repo tools — ChatGPT, Grok, AI Studio, Claude…)

- With a sandbox: clone once per chat (`git clone --depth 1`), note the base SHA, and re-pull only when the user says the repo changed. Skip it when the question doesn't touch the repo. Without a sandbox: work from the files the user pastes or uploads.
- Repo access is read-only unless the user grants write permission in the current request. A grant covers only the requested work; planning or reviewing never authorizes writes. With docs-only permission, write only `plan.md`, `audit.md` and (after final validation) `TODO.md`.
- You can't push: deliver each stage as ONE `.patch` file — all changed files in a single Conventional Commits commit (`git format-patch -1 --stdout > stage.patch`), never a series. State files (`plan.md`, `audit.md`, `act.md`, `TODO.md`) go in the patch only when the user asks in that request.
- Before delivering, prove the patch applies: `git am --3way` it in a fresh worktree at the base SHA, then apply it to the sandbox copy so both stay in sync. Give the user the patch plus `git am --3way stage.patch && ./check.sh`.
- Act only in the role the user assigned; write production code only when the user assigns the Actor role.

## Local check (user's machine)
- Never install or run `dotnet` or run `graphify update .` in the sandbox, and make no build/test claims. The user runs `./check.sh` (restore, build, test, `graphify update .`, and auto-commits `graphify-out` changes as `chore: update graph`) and pastes its summary; that run is the "user asks" case for a local build/test.
- When `./check.sh` commits `graphify-out`, it prints `PRE_GRAPHIFY_TREE` alongside the new `HEAD` and `TREE`. `PRE_GRAPHIFY_TREE` (or `TREE` when no graph commit occurred) should equal `git rev-parse HEAD^{tree}` in the sandbox; a mismatch means one side has an extra edit — ask for the user's `git diff`. If a `chore: update graph` commit was created, re-pull before the next patch.
- FAIL → fix and send a follow-up one-commit patch on top of the user's HEAD.
- Web search is fine when the problem needs it.

## Knowledge graph (`graphify-out/` — committed)
- Orient first with the top of `graphify-out/GRAPH_REPORT.md` (~8K tokens): core abstractions, labeled communities ("where things live"), cross-layer connections. Much cheaper than scanning source files for an overview.
- In the sandbox, if `graphifyy` can be installed (`pip install graphifyy --break-system-packages -q`), run read-only `graphify query`, `graphify path`, or `graphify explain` against `graphify-out/graph.json` before reading source or grepping; if installing or running `graphify` fails, do not query `graph.json` manually with `python` or `jq` — ask the user to run the `graphify` command locally and paste the output. Never read `graphify-out/graph.json` raw (~6.5 MB).
- Then open only the source files the graph names. Skip graphify when the task targets a single file the user already named.
- The graph is structure, not logic: it says what connects to what, not how code behaves — implementation questions still need the named source files.
- Check freshness: `built_at_commit` inside `graphify-out/graph.json` records the commit it was built from; for anything newer, say what it may miss rather than guessing.
