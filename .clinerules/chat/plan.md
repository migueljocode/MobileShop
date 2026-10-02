# Plan — Stage R — Cross-platform log level query scripts

## Reviewer Briefing
- Build a small, dependency-light command family under `MobileShop.Scripts/Bash` and `MobileShop.Scripts/PowerShell`.
- Bash scripts must run on macOS and Linux using standard shell utilities only; PowerShell counterparts must provide the same user-facing capabilities on Windows.
- Keep commands simple and directly callable by a non-pro user, with representative names such as `log-errors`, `log-errors-and-above`, and `log-errors-and-below`.
- Every command supports useful `-h/--help`, stdin/stdout/stderr correctly, a file input, and `-n/--number N` to select the number of matching log entities printed.
- Preserve Unix pipeline conventions: normal results go to stdout, diagnostics/errors go to stderr, and invalid usage or unreadable input returns a non-zero exit status.
- Do not add a third-party runtime dependency, application schema change, authentication change, or production logging implementation change.
- Keep implementation intentionally boring: shared helper logic is encouraged; avoid a framework or opaque all-in-one command.
- Claude verification remains an external review step after Actor implementation and GitHub Actions verification; it does not replace CI.

## Step 1 — Script layout and shared conventions
- Create `MobileShop.Scripts/Bash` and `MobileShop.Scripts/PowerShell`.
- Define one consistent command contract for both platforms:
  - default log source: `logs/app-*.log` when no file is supplied, with clear behavior when no matching log exists;
  - optional explicit file/path argument;
  - `-` means stdin;
  - `-h` and `--help` show concise usage, command meaning, options, examples, and exit behavior;
  - `-n N` and `--number N` select the number of matching log entities to print;
  - reject missing, non-integer, zero, and negative values for `N`;
  - preserve original log lines without reformatting.
- Treat a log entity as a complete log event; exception continuation lines belonging to a selected event must remain attached to that event where practical.

## Step 2 — Bash log-level command family
For each supported level, provide the same three relationship forms:
- exact level: `log-<level>`
- that level and above: `log-<level>-and-above`
- that level and below: `log-<level>-and-below`

Supported levels:
- `debug`
- `info`
- `warning`
- `error`
- `fatal`

Examples:
- `log-errors` — Error only.
- `log-errors-and-above` — Error and Fatal.
- `log-errors-and-below` — Error, Warning, Information, and Debug.
- `log-warnings` — Warning only.
- `log-warnings-and-above` — Warning, Error, and Fatal.
- `log-warnings-and-below` — Warning, Information, and Debug.
- Apply the same pattern to Debug, Information, and Fatal.
- `-n/--number N` limits the selected matching log entities, with the output order clearly documented.
- Keep command names literal and avoid requiring users to know grep/awk syntax.
- Use a small shared Bash helper for parsing, input handling, level selection, and number limiting instead of duplicating the implementation in every script.

## Step 3 — PowerShell parity
- Implement the same level commands and relationship commands under `MobileShop.Scripts/PowerShell`.
- Match Bash user-visible behavior, including `-n/--number`, file/stdin input, filtering semantics, output, diagnostics, and exit status.
- Use native PowerShell pipeline/input behavior and built-in capabilities only.
- Do not require execution-policy changes or module installation just to use the scripts.
- Keep command names representative and easy to discover from the directory.

## Step 4 — Help, streams, and edge cases
- Give every command a concise help page covering synopsis, syntax, level relationship, `-n/--number`, file/stdin input, examples, and exit codes.
- Verify:
  - direct file invocation;
  - stdin/pipeline invocation;
  - missing file;
  - empty input;
  - invalid `-n/--number`;
  - text containing spaces/special characters in file paths;
  - multiple rolling log files;
  - no matches;
  - exception continuation lines;
  - all supported levels and above/below relationships.
- Ensure normal log results never contain diagnostics/help text.
- Ensure filenames and arguments are handled safely without accidental shell evaluation.

## Step 5 — Focused automated/script verification
- Add deterministic fixtures/tests where appropriate without introducing a new test framework.
- Exercise representative Debug, Information, Warning, Error, and Fatal events plus exception continuation lines.
- Verify Bash syntax and execution on the CI-supported Unix environment and PowerShell syntax/execution on the CI-supported Windows environment.
- Use GitHub Actions as the final repository verification gate, not local build/test.
- Verify Bash and PowerShell return equivalent result sets for equivalent commands and `-n` values.
- Keep generated logs, temporary files, and unrelated artifacts out of the commit.

## Step 6 — Documentation and usability pass
- Add `MobileShop.Scripts/README.md` with the common commands first and copy/paste examples for macOS/Linux and Windows PowerShell.
- Explain the exact/above/below command pattern, default log location, stdin usage, `-n/--number`, exit codes, and `--help`.
- Prefer examples such as `./log-errors`, `./log-errors-and-above -n 20`, and their PowerShell equivalents.
- Make the command family understandable without prior shell expertise.

## Global Definition of Done
- `MobileShop.Scripts/Bash` contains the complete level-based command family.
- `MobileShop.Scripts/PowerShell` provides equivalent level-based commands.
- Every supported level has exact, above, and below commands.
- Every command supports `-h/--help` and `-n/--number N`.
- File input and stdin/pipeline input work consistently.
- Normal output uses stdout; diagnostics use stderr; invalid usage/input returns non-zero.
- Bash uses only standard macOS/Linux tooling; PowerShell uses built-in capabilities.
- Existing application logging behavior remains unchanged.
- CI verifies the scripts on supported platforms.
- `MobileShop.Scripts/README.md` makes the command family usable by a non-pro user.
- After GitHub Actions verification, Actor pushes the implementation commit/PR as required by the workflow; Reviewer/Claude performs the requested final external verification.
