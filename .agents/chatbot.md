# Chatbot rules (no repo tools — ChatGPT, Grok, AI Studio, Claude…)
- With a sandbox: pull the latest repo before every answer (pull, not a fresh clone, to save tokens). Without one: work from the files the user pastes or uploads.
- Repo access is read-only unless the user grants write permission in the current request. A grant covers only the requested work; planning or reviewing never authorizes writes. With docs-only permission, write only `plan.md`, `audit.md` and (after final validation) `TODO.md`.
- You can't push: deliver each changed file (download or copyable block) plus its commit message in its own copyable block — message text only, not a `git commit` command.
- Act only in the role the user assigned; write production code only when the user assigns the Actor role.
## Knowledge graph (`graphify-out/` — committed)
- Orient first with `graphify-out/GRAPH_REPORT.md` (~8K tokens): core abstractions, labeled communities ("where things live"), cross-layer connections. Much cheaper than scanning 345 source files for an overview.
- With a sandbox: query `graphify-out/graph.json` for relationships ("what calls X", "what would changing X touch") using jq/python — never read it raw (5.9 MB). It names the exact source files to open; read those instead of grepping the tree.
- The graph is structure, not logic: it says what connects to what, not how code behaves — implementation questions still need the named source files.
- Check freshness: `built_at_commit` inside `graph.json` records the commit it was built from; for anything newer, say what it may miss rather than guessing.
