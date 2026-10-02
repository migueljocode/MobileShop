# Plan — Stage R — Cross-platform log query scripts

## Reviewer Briefing
- Build a small, dependency-light command family under `MobileShop.Scripts/Bash` and `MobileShop.Scripts/PowerShell`.
- Bash scripts must run on macOS and Linux using standard shell utilities only; PowerShell counterparts must provide the same user-facing capabilities on Windows.
- Keep each command single-purpose and representative so a non-pro user can discover and invoke it directly, for example `log-errors`, `log-last-errors`, `log-warnings`, and `log-warnings-and-above`.
- Every command must support useful `-h/--help`, stdin/stdout/stderr correctly, a file argument, and simple filtering options where applicable.
- Preserve Unix pipeline conventions: input may come from a file or stdin, normal results go to stdout, diagnostics/errors go to stderr, and exit status is non-zero for invalid usage or unreadable input.
- Do not add a third-party runtime dependency, application schema change, application authentication change, or changes to the production logging implementation.
- Keep the implementation intentionally boring: shared helper logic is acceptable, but avoid a framework or a single opaque "do everything" command.
- Claude verification is an external review step after the Actor implementation and GitHub Actions verification; it is not a substitute for the repository CI gate.

## Step 1 — Script layout and shared conventions
- Create `MobileShop.Scripts/Bash` and `MobileShop.Scripts/PowerShell`.
- Define one consistent command contract for both platforms:
  - default log source: `logs/app-*.log` when a file is not supplied, with clear behavior when no matching file exists;
  - optional explicit file/path argument;
  - `-` means stdin where practical;
  - `-h` and `--help` show concise usage/examples and exit successfully;
  - `--file FILE` / positional FILE compatibility where it improves discoverability;
  - `--since DATE` and `--until DATE` for date filtering;
  - `--match TEXT` for simple message/text filtering;
  - `--tail N` for limiting the final result set.
- Document that commands operate on the existing Serilog file format and preserve the original log lines rather than reformatting them.

## Step 2 — Bash command family
Create representative, directly callable commands:
- `log-errors` — show Error and Fatal entries.
- `log-last-errors` — show the most recent Error/Fatal entries; default to a small useful tail count and allow `--tail N`.
- `log-warnings` — show Warning entries.
- `log-warnings-and-above` — show Warning, Error, and Fatal entries.
- `log-info` — show Information entries.
- `log-debug` — show Debug entries.
- `log-level LEVEL` — show one named level for users who need something outside the common shortcuts.
- `log-search TEXT` — search log messages without requiring knowledge of grep syntax.
- `log-tail` — show the last N log lines, with an easy `--tail N` option.
- Keep command names short and literal; scripts should be thin wrappers over a shared internal Bash helper rather than duplicate parsing/filtering code.

## Step 3 — PowerShell parity
Implement the same command names and user-visible behavior under `MobileShop.Scripts/PowerShell`.
- Use native PowerShell pipeline behavior and standard commands only.
- Accept file input and pipeline/stdin input.
- Send normal log output through the success/output stream and diagnostics to the error stream.
- Keep option names and examples aligned with Bash wherever PowerShell syntax permits.
- Do not require execution-policy changes or installation of modules just to use the scripts.

## Step 4 — Help, stdin/stdout/stderr, and edge-case coverage
- Give every command a concise help page with synopsis, syntax, arguments/options, examples, input/output behavior, and exit codes.
- Verify:
  - direct file invocation;
  - stdin/pipeline invocation;
  - missing file;
  - empty input;
  - invalid level;
  - invalid date;
  - invalid/non-positive tail count;
  - text containing spaces/special characters;
  - multiple rolling log files;
  - no matches.
- Ensure no command accidentally consumes or prints help/errors into the normal result stream.
- Ensure filenames and user search text are handled safely without accidental shell evaluation.

## Step 5 — Focused automated/script verification
- Add repository tests or deterministic script fixtures where appropriate without introducing a new test framework.
- Exercise representative existing log lines, including Debug/Information/Warning/Error/Fatal and exception continuation lines.
- Verify Bash syntax and execution on the CI-supported Unix environment and PowerShell syntax/execution on the CI-supported Windows environment, using GitHub Actions rather than local build/test as the final gate.
- Verify both platforms return equivalent result sets for the same fixture and options.
- Keep generated logs, temporary files, and unrelated artifacts out of the commit.

## Step 6 — Documentation and usability pass
- Add a concise `MobileShop.Scripts/README.md` showing the most common commands first, with copy/paste examples for macOS/Linux and Windows PowerShell.
- Explain the default log location, stdin usage, date/text filters, tailing, exit codes, and how to discover commands through `--help`.
- Prefer examples such as `./log-errors`, `./log-last-errors --tail 20`, and PowerShell equivalents rather than requiring users to understand grep/awk/Select-String.

## Global Definition of Done
- `MobileShop.Scripts/Bash` contains a coherent, executable command family for common log diagnosis.
- `MobileShop.Scripts/PowerShell` provides equivalent commands and behavior.
- Common tasks require no knowledge of grep/awk/findstr/Select-String.
- Every command has useful `-h/--help`, sensible exit codes, and correct stdin/stdout/stderr behavior.
- File, date-range, text-search, and tail-count filtering work consistently.
- Bash uses only standard macOS/Linux tooling; PowerShell uses built-in PowerShell capabilities.
- Existing application logging behavior is unchanged.
- CI passes the script verification on the supported platforms.
- `MobileShop.Scripts/README.md` makes the command family usable by a non-pro user.
- After GitHub Actions verification, Actor pushes the implementation commit/PR as required by the workflow; Reviewer/Claude performs the requested final external verification.
