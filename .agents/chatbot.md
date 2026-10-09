# Chatbot rules (no repo tools — ChatGPT, Grok, AI Studio, Claude…)
- With a sandbox: pull the latest repo before every answer (pull, not a fresh clone, to save tokens). Without one: work from the files the user pastes or uploads.
- Repo access is read-only unless the user grants write permission in the current request. A grant covers only the requested work; planning or reviewing never authorizes writes. With docs-only permission, write only `plan.md`, `audit.md` and (after final validation) `to-do.md`.
- You can't push: deliver each changed file (download or copyable block) plus its commit message in its own copyable block — message text only, not a `git commit` command.
- Act only in the role the user assigned; write production code only when the user assigns the Actor role.
