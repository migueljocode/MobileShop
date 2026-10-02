# Plan — Stage R — Cross-platform log level query scripts

## Reviewer Briefing
- Build a small, dependency-light command family under `MobileShop.Scripts/Bash` and `MobileShop.Scripts/PowerShell`.
- All command names use lowercase kebab-case.
- The canonical command contract is:
  - `log-<level>`
  - `log-<level>-and-above`
  - `log-<level>-and-below`
- Supported `<level>` values are exactly `debug`, `info`, `warning`, `error`, and `fatal`.
- Bash commands must run on macOS and Linux using standard shell utilities only; PowerShell counterparts must provide the same user-facing capabilities on Windows.
- Commands must be directly callable by a non-pro user without requiring knowledge of grep/awk syntax.
- Every command supports `-h`/\`--help\`, `-n N`/\`--number N\`, file input, and stdin/pipeline input.
- Normal results go to stdout; diagnostics/help/errors go to stderr where appropriate; invalid usage or unreadable input returns non-zero.
- Preserve original log lines without reformatting.
- Do not add third-party runtime dependencies, application schema changes, authentication changes, or production logging implementation changes.
- Keep the implementation intentionally boring: shared helper logic is encouraged; avoid a framework or opaque all-in-one command.
- Claude verification remains an external review step after Actor implementation and GitHub Actions verification; it does not replace CI.

## Repository Files — Exact Stage R Scope

### Existing files read or directly relevant
- `.clinerules/chat/plan.md` — this Stage R implementation plan.
- `.clinerules/chat/audit.md` — Reviewer Job A/B audit record; Planner does not use it to mark completion.
- `.github/workflows/dotnet.yml` — existing GitHub Actions workflow; extend it only as needed to verify the scripts.
- `src/MobileShop.Services/Logging/Configuration/LoggingsConfiguration.cs` — authoritative rolling-file log format and source path configuration.
- `src/MobileShop.Services/Logging/Configuration/AppLoggingSettings.cs` — existing logging-level configuration defaults, if unchanged from the current repository layout.
- `README.md` — repository-level documentation; modify only if a concise link to the script documentation is appropriate.

### New files/directories
- `MobileShop.Scripts/README.md`
- `MobileShop.Scripts/Bash/_log-level-query.sh` — shared Bash implementation for argument parsing, input selection, level matching, event grouping, and `-n` limiting.
- `MobileShop.Scripts/Bash/log-debug`
- `MobileShop.Scripts/Bash/log-debug-and-above`
- `MobileShop.Scripts/Bash/log-debug-and-below`
- `MobileShop.Scripts/Bash/log-info`
- `MobileShop.Scripts/Bash/log-info-and-above`
- `MobileShop.Scripts/Bash/log-info-and-below`
- `MobileShop.Scripts/Bash/log-warning`
- `MobileShop.Scripts/Bash/log-warning-and-above`
- `MobileShop.Scripts/Bash/log-warning-and-below`
- `MobileShop.Scripts/Bash/log-error`
- `MobileShop.Scripts/Bash/log-error-and-above`
- `MobileShop.Scripts/Bash/log-error-and-below`
- `MobileShop.Scripts/Bash/log-fatal`
- `MobileShop.Scripts/Bash/log-fatal-and-above`
- `MobileShop.Scripts/Bash/log-fatal-and-below`
- `MobileShop.Scripts/PowerShell/Log-LevelQuery.ps1` — shared PowerShell implementation for argument parsing, input selection, level matching, event grouping, and `-n` limiting.
- `MobileShop.Scripts/PowerShell/log-debug.ps1`
- `MobileShop.Scripts/PowerShell/log-debug-and-above.ps1`
- `MobileShop.Scripts/PowerShell/log-debug-and-below.ps1`
- `MobileShop.Scripts/PowerShell/log-info.ps1`
- `MobileShop.Scripts/PowerShell/log-info-and-above.ps1`
- `MobileShop.Scripts/PowerShell/log-info-and-below.ps1`
- `MobileShop.Scripts/PowerShell/log-warning.ps1`
- `MobileShop.Scripts/PowerShell/log-warning-and-above.ps1`
- `MobileShop.Scripts/PowerShell/log-warning-and-below.ps1`
- `MobileShop.Scripts/PowerShell/log-error.ps1`
- `MobileShop.Scripts/PowerShell/log-error-and-above.ps1`
- `MobileShop.Scripts/PowerShell/log-error-and-below.ps1`
- `MobileShop.Scripts/PowerShell/log-fatal.ps1`
- `MobileShop.Scripts/PowerShell/log-fatal-and-above.ps1`
- `MobileShop.Scripts/PowerShell/log-fatal-and-below.ps1`
- Add only the minimal deterministic script fixtures/tests required by CI; if files are introduced, their exact paths must be recorded in the implementation commit and kept under `MobileShop.Scripts/tests/`.

## Step 1 — Define the exact cross-platform contract
- Treat these as the complete command family:
  - `log-debug`, `log-debug-and-above`, `log-debug-and-below`
  - `log-info`, `log-info-and-above`, `log-info-and-below`
  - `log-warning`, `log-warning-and-above`, `log-warning-and-below`
  - `log-error`, `log-error-and-above`, `log-error-and-below`
  - `log-fatal`, `log-fatal-and-above`, `log-fatal-and-below`
- Map command levels explicitly to Serilog levels:
  - `debug` → Debug
  - `info` → Information
  - `warning` → Warning
  - `error` → Error
  - `fatal` → Fatal
- Exact commands match one level.
- `and-above` includes the selected level and every more severe supported level.
- `and-below` includes the selected level and every less severe supported level.
- Default log source is `logs/app-*.log` when no file is supplied.
- A positional file/path argument selects an explicit input file.
- `-` selects stdin.
- Define deterministic ordering for multiple rolling files and document whether `-n` selects the first or last matching events; prefer the least surprising behavior for terminal use and keep it identical on Bash and PowerShell.
- Reject missing, non-integer, zero, and negative `N`.
- Treat a log entity as a complete event, not a physical line. Exception continuation lines following an event remain attached to that event when selected.

## Step 2 — Implement the Bash command family
- Implement shared behavior once in `MobileShop.Scripts/Bash/_log-level-query.sh`.
- Keep each command wrapper tiny: it selects the level and relationship and delegates to the helper.
- Use only standard macOS/Linux shell tooling.
- Support `-h` and `--help`, `-n N` and `--number N`, explicit file input, and `-` for stdin.
- Preserve event text exactly.
- Avoid unsafe shell evaluation of filenames, arguments, or log contents.
- Ensure wrappers are executable and can be invoked directly from the Bash directory.

## Step 3 — Implement PowerShell parity
- Implement shared behavior once in `MobileShop.Scripts/PowerShell/Log-LevelQuery.ps1`.
- Keep each command wrapper small: it selects the level and relationship and delegates to the helper.
- Use built-in PowerShell capabilities only.
- Match Bash semantics for level matching, event grouping, `-n`, input selection, output, diagnostics, and exit status.
- Support pipeline/stdin input naturally.
- Keep script names in lowercase kebab-case even though the PowerShell files use the `.ps1` extension.
- Invocation must not require module installation or a project-specific dependency.

## Step 4 — Help, streams, and edge cases
- Every command must provide concise help covering synopsis, syntax, level relationship, `-n`/\`--number\`, file/stdin input, examples, and exit behavior.
- Verify:
  - direct file invocation;
  - stdin/pipeline invocation;
  - missing input file;
  - empty input;
  - invalid `-n`/\`--number\`;
  - paths containing spaces and shell-special characters;
  - multiple rolling log files;
  - no matches;
  - exception continuation lines;
  - every supported level and every above/below relationship.
- Normal log results must never be mixed with diagnostics/help text.
- Errors must be actionable and written to stderr.
- Invalid usage/input must return a non-zero exit status.

## Step 5 — CI and focused script verification
- Add deterministic fixtures/tests under `MobileShop.Scripts/tests/` only if needed; do not introduce a new test framework merely for the scripts.
- Exercise representative Debug, Information, Warning, Error, and Fatal events, including multi-line exceptions.
- Extend `.github/workflows/dotnet.yml` only as needed to verify Bash on the CI-supported Unix runner and PowerShell on the CI-supported Windows runner; preserve the existing application build/test behavior.
- Verify Bash and PowerShell produce equivalent result sets for equivalent commands and `-n` values.
- Use GitHub Actions as the final repository verification gate, not local build/test.
- Keep generated logs, temporary files, and unrelated artifacts out of the commit.

## Step 6 — Documentation and usability pass
- Add `MobileShop.Scripts/README.md` with the command family listed explicitly.
- Explain exact/above/below semantics, the Serilog level mapping, default log location, file argument, stdin usage, `-n`/\`--number\`, help, exit codes, and output ordering.
- Provide copy/paste examples for macOS/Linux and Windows PowerShell.
- Prefer examples such as `./log-error`, `./log-error-and-above -n 20`, and stdin pipelines.
- Make the command family understandable without prior shell expertise.
- If `README.md` is updated to point users to the script directory, keep that change minimal and unrelated application documentation unchanged.

## Global Definition of Done
- All 15 Bash commands and all 15 PowerShell counterparts exist at the exact paths listed above.
- Every command name is lowercase kebab-case and follows exactly `log-<level>` or `log-<level>-and-above/below`.
- Every command supports `-h`/\`--help\` and `-n N`/\`--number N`.
- File input and stdin/pipeline input work consistently.
- Normal output uses stdout; diagnostics/errors use stderr; invalid usage/input returns non-zero.
- Multi-line exception events remain intact.
- Bash uses only standard macOS/Linux tooling; PowerShell uses built-in capabilities.
- Existing application logging behavior remains unchanged.
- `.github/workflows/dotnet.yml` verifies the scripts on supported platforms.
- `MobileShop.Scripts/README.md` makes the command family usable by a non-pro user.
- The implementation is limited to the exact files listed here plus explicitly recorded deterministic fixtures/tests.
- After GitHub Actions verification, Actor pushes the implementation commit/PR as required by the workflow; Reviewer/Claude performs the requested external verification.
- Planner does not mark `.clinerules/to-do.md` complete; only Reviewer may do so after final verification.
