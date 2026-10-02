# Plan — Stage R — Cross-platform log level query scripts

## Reviewer Briefing
- Build a small, dependency-light command family under `MobileShop.Scripts/Bash` and `MobileShop.Scripts/PowerShell`.
- All command names use lowercase kebab-case.
- The canonical command contract is exactly:
  - `log-<level>`
  - `log-<level>-and-above`
  - `log-<level>-and-below`
- Supported `<level>` values are exactly `debug`, `info`, `warning`, `error`, and `fatal`.
- Every command supports:
  - `-h` / `--help`
  - `-n N` / `--number N`
  - `--date-from yyyy-MM-dd`
  - `--date-to yyyy-MM-dd`
  - `--date-time-from "yyyy-MM-dd HH:mm:ss"`
  - `--date-time-to "yyyy-MM-dd HH:mm:ss"`
  - explicit file/path input
  - stdin via `-` and normal pipeline input.
- Date/time filters are inclusive.
- `--date-from` / `--date-to` filter by calendar date.
- `--date-time-from` / `--date-time-to` filter by the full timestamp.
- When a `from` option is supplied without its corresponding `to`, `to` defaults to the current local date or local date-time at command start.
- When a `from` option is not supplied, the lower bound defaults to the first log entry in the selected input.
- If neither side of a range is supplied, no artificial date/time restriction is added.
- Do not silently mix date-only and date-time ranges: a date pair and a date-time pair are separate filtering modes and passing options from both modes together is invalid.
- A `to` without its corresponding `from` is valid and uses the first log entry as the lower bound.
- Reject malformed dates/times, impossible calendar values, reversed ranges, missing option values, and invalid `N`.
- Bash must run on macOS and Linux using standard shell utilities only; PowerShell counterparts must provide the same user-visible behavior on Windows.
- Commands must be directly callable by a non-pro user without requiring knowledge of grep/awk syntax.
- Normal results go to stdout; diagnostics/errors go to stderr; invalid usage or unreadable input returns non-zero.
- Preserve original log lines without reformatting.
- Do not add third-party runtime dependencies, application schema changes, authentication changes, or production logging implementation changes.
- Keep the implementation intentionally boring: shared helper logic is encouraged; avoid a framework or opaque all-in-one command.
- Claude verification remains an external review step after Actor implementation and GitHub Actions verification; it does not replace CI.

## Repository Files — Exact Stage R Scope

### Existing files read or directly relevant
- `.clinerules/chat/plan.md` — this Stage R implementation plan.
- `.clinerules/chat/audit.md` — Reviewer Job A/B audit record.
- `.github/workflows/dotnet.yml` — existing CI; add script verification and a Windows PowerShell job while preserving the existing .NET job.
- `src/MobileShop.Services/Logging/Configuration/LoggingsConfiguration.cs` — authoritative rolling-file path and file output format.
- `src/MobileShop.Services/Logging/Configuration/AppLoggingSettings.cs` — existing logging-level configuration defaults.
- `README.md` — modify only if a minimal link to script documentation is useful.

### New files/directories
- `MobileShop.Scripts/README.md`
- `MobileShop.Scripts/Bash/_log-level-query.sh` — shared implementation.
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
- `MobileShop.Scripts/PowerShell/Log-LevelQuery.ps1` — shared implementation.
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
- `MobileShop.Scripts/tests/` — deterministic fixtures and lightweight script tests only if needed.
- No other new repository area is in Stage R scope unless the Reviewer approves it through a plan revision.

## Step 1 — Define the exact cross-platform command and filter contract
- Treat these as the complete command family:
  - `log-debug`, `log-debug-and-above`, `log-debug-and-below`
  - `log-info`, `log-info-and-above`, `log-info-and-below`
  - `log-warning`, `log-warning-and-above`, `log-warning-and-below`
  - `log-error`, `log-error-and-above`, `log-error-and-below`
  - `log-fatal`, `log-fatal-and-above`, `log-fatal-and-below`
- Map levels explicitly:
  - `debug` → Debug
  - `info` → Information
  - `warning` → Warning
  - `error` → Error
  - `fatal` → Fatal
- Exact commands match one level.
- `and-above` includes the selected level and every more severe supported level.
- `and-below` includes the selected level and every less severe supported level.
- Default log source is `logs/app-*.log` when no explicit file is supplied.
- A positional file/path argument selects one explicit input file.
- `-` selects stdin.
- For the default rolling-file source, process matching files in deterministic chronological order and document the resulting output order.
- `-n N` counts complete matching log events, not physical lines. Define and document whether the selected N events are the first N or last N; use the same behavior on both platforms.
- Treat a log event as the timestamped header plus its continuation lines. Exception stack-trace lines must stay attached to the selected event.
- Date-only range semantics:
  - `--date-from YYYY-MM-DD` is an inclusive lower calendar-date bound.
  - `--date-to YYYY-MM-DD` is an inclusive upper calendar-date bound.
  - If `--date-from` is present and `--date-to` is absent, the upper bound is the current local date at command start.
  - If `--date-to` is present without `--date-from`, the lower bound is the first log entry's calendar date.
- Date-time range semantics:
  - `--date-time-from "YYYY-MM-DD HH:mm:ss"` is an inclusive lower timestamp bound.
  - `--date-time-to "YYYY-MM-DD HH:mm:ss"` is an inclusive upper timestamp bound.
  - If `--date-time-from` is present and `--date-time-to` is absent, the upper bound is the current local date-time at command start, truncated/formatted to seconds.
  - If `--date-time-to` is present without `--date-time-from`, the lower bound is the first log entry's timestamp.
- Date-only and date-time options are mutually exclusive modes; do not silently reinterpret one as the other.
- Use the timestamp and timezone recorded in the log entry for comparisons; define behavior consistently for entries with offsets and ensure both platforms compare equivalent instants.
- If the input has no log entries and a bound needs the first entry, report a clear diagnostic and return non-zero only when the requested operation cannot be meaningfully evaluated; otherwise an empty result remains successful.
- Preserve original event text exactly.

## Step 2 — Implement the Bash command family
- Implement shared behavior once in `MobileShop.Scripts/Bash/_log-level-query.sh`.
- Keep each public wrapper tiny: select level and relationship, then delegate.
- Use only standard macOS/Linux shell utilities.
- Support all options from Step 1.
- Provide robust quoting and argument handling; never evaluate filenames or log contents as shell code.
- Ensure wrappers are executable and directly callable.
- Keep the helper private/internal by naming convention and document that users should call the public commands.

## Step 3 — Implement PowerShell parity
- Implement shared behavior once in `MobileShop.Scripts/PowerShell/Log-LevelQuery.ps1`.
- Keep each public wrapper tiny: select level and relationship, then delegate.
- Use built-in PowerShell capabilities only.
- Match Bash semantics for level matching, event grouping, all options, date/time parsing, ordering, `-n`, input, output, diagnostics, and exit status.
- Support pipeline/stdin naturally.
- Keep public script names lowercase kebab-case with `.ps1`.
- Do not require module installation or project-specific dependencies.

## Step 4 — Comprehensive per-script help and streams
- Every public Bash and PowerShell script must contain or expose its own complete, command-specific help; users must be able to run the individual script with `-h` or `--help` and understand it without opening another file.
- Help must cover:
  - command purpose and exact level relationship;
  - synopsis and positional file argument;
  - `-` stdin behavior;
  - `-n N` / `--number N`;
  - `--date-from` / `--date-to`;
  - `--date-time-from` / `--date-time-to`;
  - accepted formats with literal examples;
  - default `from`/current `to` behavior;
  - mutual exclusion of date and date-time modes;
  - multiple-filter behavior and inclusive boundaries;
  - default log path;
  - output ordering and what `-n` counts;
  - examples for direct invocation and pipelines;
  - exit codes and common errors.
- Help text should be concise enough for terminal use but comprehensive enough that a non-pro user does not need to inspect implementation files.
- Normal output goes only to stdout; diagnostics go to stderr. Help may use stdout for successful `-h`/\`--help` and must return zero.
- Invalid options, malformed dates/times, invalid numbers, reversed ranges, unreadable files, and incompatible filter combinations return non-zero.

## Step 5 — CI and focused verification
- Add deterministic fixtures/tests under `MobileShop.Scripts/tests/` only if needed; do not introduce a new test framework merely for scripts.
- Cover:
  - all five levels;
  - exact/above/below relationships;
  - `-n` values including 1, multiple events, too-large values, zero, negative, and malformed values;
  - date-only from/to, from-only, to-only, and no bounds;
  - date-time from/to, from-only, to-only, and no bounds;
  - current-time default for omitted `to`;
  - first-entry default for omitted `from`;
  - date/date-time mode conflicts;
  - malformed/impossible dates and times;
  - reversed ranges;
  - multiple rolling files;
  - empty input and no matches;
  - stdin and explicit paths, including spaces;
  - multi-line exception events;
  - stdout/stderr separation and exit codes;
  - every command's help output.
- Extend `.github/workflows/dotnet.yml` to verify Bash on the existing Ubuntu runner and PowerShell on a Windows runner, while preserving the existing .NET build/test job.
- Verify Bash and PowerShell return equivalent event sets for equivalent inputs/options.
- Use GitHub Actions as the final repository verification gate, not local build/test.
- Keep generated logs, temporary files, and unrelated artifacts out of the commit.

## Step 6 — Documentation and usability pass
- Add `MobileShop.Scripts/README.md` with the complete command family explicitly listed.
- Explain exact/above/below semantics, level mapping, default log location, file argument, stdin, `-n`, both date-filter modes, defaults for omitted bounds, output ordering, exit codes, and help.
- Provide copy/paste examples for macOS/Linux and Windows PowerShell.
- Include examples such as:
  - `./log-error`
  - `./log-error-and-above -n 20`
  - `./log-warning --date-from 2026-01-01 --date-to 2026-01-31`
  - `./log-error --date-time-from "2026-01-01 08:00:00" --date-time-to "2026-01-01 18:00:00"`
  - `cat logs/app-*.log | ./log-error`
- Make the command family understandable without prior shell expertise.
- If root `README.md` is updated, keep the change limited to a concise pointer to `MobileShop.Scripts/README.md`.

## Global Definition of Done
- All 15 Bash commands and all 15 PowerShell counterparts exist at the exact paths listed above.
- Every command name is lowercase kebab-case and follows exactly `log-<level>` or `log-<level>-and-above/below`.
- Every command supports `-h`/\`--help`, `-n N`/\`--number N`, both date-only options, both date-time options, explicit file input, and stdin/pipeline input.
- Date/date-time parsing and default-bound behavior are identical across platforms.
- Every public script has comprehensive self-contained help.
- File input and stdin/pipeline input work consistently.
- Normal output uses stdout; diagnostics/errors use stderr; invalid usage/input returns non-zero.
- Multi-line exception events remain intact.
- Bash uses only standard macOS/Linux tooling; PowerShell uses built-in capabilities.
- Existing application logging behavior remains unchanged.
- `.github/workflows/dotnet.yml` verifies both script platforms.
- `MobileShop.Scripts/README.md` makes the command family usable by a non-pro user.
- The implementation is limited to the exact Stage R paths listed here plus explicitly recorded deterministic fixtures/tests.
- After GitHub Actions verification, Actor pushes the implementation commit/PR as required by the workflow; Reviewer/Claude performs the requested external verification.
- Planner does not mark `.clinerules/to-do.md` complete; only Reviewer may do so after final verification.
