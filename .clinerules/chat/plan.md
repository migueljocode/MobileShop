# Plan — Stage R — Cross-platform log query utilities

## Reviewer Briefing
- Replace the previous 30-script design with exactly two user-facing entry points:
  - `MobileShop.Scripts/Bash/log.sh`
  - `MobileShop.Scripts/PowerShell/log.ps2`
- The PowerShell filename `log.ps2` is intentional for this Stage R contract; do not create per-command wrapper scripts.
- Both entry points implement the same behavior and one shared conceptual command model.
- Selection is explicit through options:
  - `--level <debug|info|warning|error|fatal>` (short `-l`)
  - `--above`
  - `--below`
  - default with neither `--above` nor `--below` means exact level.
- `--above` and `--below` are mutually exclusive.
- Supported log levels map exactly: debug→Debug, info→Information, warning→Warning, error→Error, fatal→Fatal.
- Every invocation supports `-h/--help`, `-n/--number N`, date/date-time filters, explicit input path, and stdin via `-`/pipeline.
- Invalid dates/datetimes must be rejected before filtering, with a clear user-facing diagnostic on stderr and non-zero exit.
- Follow DRY, SRP, and KISS strictly: one parsing/validation path, one event reader, one level selector, one range selector, one formatter/output path per implementation; do not duplicate logic across modes or platforms beyond unavoidable shell-language syntax.
- No application schema, authentication, production logging, or third-party runtime dependency changes.

## Exact Repository Scope

### Existing files
- `.clinerules/chat/plan.md` — active Stage R plan.
- `.clinerules/chat/audit.md` — Reviewer Job A/B record.
- `.github/workflows/dotnet.yml` — preserve existing .NET job; add Bash and Windows PowerShell verification.
- `src/MobileShop.Services/Logging/Configuration/LoggingsConfiguration.cs` — authoritative log path/template.
- `src/MobileShop.Services/Logging/Configuration/AppLoggingSettings.cs` — logging defaults.
- `README.md` — optional minimal pointer to script documentation.

### New Stage R files
- `MobileShop.Scripts/Bash/log.sh`
- `MobileShop.Scripts/PowerShell/log.ps2`
- `MobileShop.Scripts/README.md`
- `MobileShop.Scripts/tests/fixtures/` — deterministic log fixtures.
- `MobileShop.Scripts/tests/` — lightweight cross-platform test scripts only; no new test framework unless unavoidable and approved by plan revision.
- No other new repository area is in scope.

## Step 1 — Canonical CLI contract
- Invocation shape:
  - `log.sh --level LEVEL [--above|--below] [filters] [--number N] [PATH|-]`
  - `log.ps2 --level LEVEL [--above|--below] [filters] [--number N] [PATH|-]`
- `--level/-l` is required unless help is requested.
- Exact mode is the default.
- `--above` includes the selected level and every more severe level.
- `--below` includes the selected level and every less severe level.
- Reject missing/unknown levels and simultaneous `--above --below`.
- Default source is `logs/app-*.log`; explicit PATH selects one file; `-` reads stdin.
- Pipeline input is accepted without requiring users to know shell filtering commands.
- Do not execute or interpolate input paths/content as code.

## Step 2 — Date and datetime filtering
- Date-only options:
  - `--date-from YYYY-MM-DD`
  - `--date-to YYYY-MM-DD`
- Date-time options:
  - `--date-time-from "YYYY-MM-DD HH:mm:ss"`
  - `--date-time-to "YYYY-MM-DD HH:mm:ss"`
- Bounds are inclusive.
- Date-only and date-time modes are mutually exclusive; mixing them is an error.
- If a `from` is supplied without its matching `to`, `to` is the current local date/datetime captured once at command start.
- If `from` is omitted, its lower bound is the first parsed log event in the selected input. This applies whether only `to` is supplied or no range is supplied within a chosen mode.
- If neither bound is supplied, do not impose a synthetic time restriction.
- `to` without `from` is valid and derives `from` from the first event.
- Parse formats strictly; reject malformed syntax, impossible calendar/time values, reversed ranges, missing values, and unsupported combinations.
- Date-only comparison uses the event's calendar date; datetime comparison uses the timestamp represented by the log entry, including its recorded offset. Document the local-clock interpretation of generated current bounds and make both platforms equivalent.
- If first-entry inference is required but input contains no events, emit a clear diagnostic to stderr and return non-zero.
- Invalid date/datetime examples must identify the option, expected format, received value, and correction example.

## Step 3 — Log-event parsing and `-n`
- Parse the existing Serilog header format from `LoggingsConfiguration.cs` and attach continuation/exception lines to their preceding event.
- Never split a multiline exception into separate results.
- Preserve event text exactly; do not reformat timestamps/messages.
- `-n N` counts complete matching events, not physical lines.
- Define `-n` as the first N matching events in deterministic output order; `N=0` is invalid.
- Too-large N is valid and simply returns all matching events.
- Reject negative, non-numeric, missing, or overflowed N with stderr diagnostic and non-zero exit.
- Default rolling-file processing is deterministic chronological order; document whether same-day files are ordered by filename/timestamp and use the same rule on both platforms.

## Step 4 — Bash implementation
- `log.sh` is the only public Bash script.
- Keep CLI parsing, validation, event parsing, selection, filtering, limiting, and output as small single-purpose functions.
- Use standard macOS/Linux shell utilities only.
- Avoid `eval`, unsafe word splitting, duplicated option branches, and duplicated level logic.
- Capture current time once.
- Send normal events to stdout; help to stdout; diagnostics/errors to stderr.
- Return meaningful non-zero status for invalid usage/input.
- Keep implementation directly runnable by a non-pro user.

## Step 5 — PowerShell implementation
- `log.ps2` is the only public PowerShell script.
- Mirror the same logical functions and behavior as Bash without copying Bash-specific mechanisms.
- Use built-in PowerShell only.
- Accept both direct file input and pipeline input.
- Keep level/severity mapping centralized rather than repeating conditions.
- Capture current time once and use identical bound semantics.
- stdout contains results/help; stderr contains diagnostics/errors; exit codes match Bash behavior.
- No module installation or project-specific dependency.

## Step 6 — Self-contained help
- `log.sh --help` and `log.ps2 --help` must each be complete enough to use without opening another file.
- Help must include:
  - purpose;
  - required `--level/-l`;
  - exact/above/below semantics;
  - every option and accepted value format;
  - default log path;
  - PATH and `-` stdin behavior;
  - `-n` event semantics;
  - date and datetime examples;
  - omitted-bound defaults;
  - inclusive ranges;
  - mutual exclusion;
  - malformed-input examples and errors;
  - stdout/stderr and exit-code behavior;
  - direct and pipeline examples;
  - macOS/Linux and Windows PowerShell invocation examples.
- Keep help concise but genuinely comprehensive for a non-pro user.

## Step 7 — Tests and CI
- Use deterministic fixtures containing all five levels, multiline exceptions, multiple dates, timezone offsets, and boundary timestamps.
- Verify:
  - exact/above/below for every level;
  - missing/invalid level and conflicting severity flags;
  - all `-n` cases;
  - date from/to, from-only, to-only, no bounds;
  - datetime from/to, from-only, to-only, no bounds;
  - current-bound behavior captured once using the process-local wall-clock representation;
  - first-entry inference;
  - malformed and impossible date/datetime values with stderr diagnostics;
  - reversed ranges and mixed date modes;
  - empty input/no matches;
  - explicit paths with spaces;
  - stdin and pipeline;
  - multiline exceptions;
  - stdout/stderr separation and exit codes;
  - help.
- Compare Bash and PowerShell results for equivalent fixtures/options.
- Update `.github/workflows/dotnet.yml` with Bash verification on the existing Ubuntu job and a Windows runner for PowerShell, while preserving the existing .NET build/test gate.
- GitHub Actions is the final verification gate; local tests do not replace it.
- Do not commit generated logs, temporary files, or unrelated artifacts.

## Step 8 — Documentation
- `MobileShop.Scripts/README.md` documents only the two entry points and their shared CLI contract.
- Include copy/paste examples such as:
  - `./log.sh --level error`
  - `./log.sh --level error --above -n 20`
  - `./log.sh --level warning --date-from 2026-01-01 --date-to 2026-01-31`
  - `./log.sh --level error --date-time-from "2026-01-01 08:00:00" --date-time-to "2026-01-01 18:00:00"`
  - `cat logs/app-*.log | ./log.sh --level error`
  - `./log.ps2 --level error --above -n 20`
- Explain that no grep/awk knowledge is required.
- Root README, if touched, gets only a concise pointer.

## Clean Code / Architecture Rules
- DRY: one source of truth for level ordering, option semantics, date parsing, event parsing, and error wording within each implementation.
- SRP: parsing arguments, parsing timestamps/events, selecting levels, applying ranges, limiting output, and rendering diagnostics are separate responsibilities.
- KISS: prefer straightforward functions and data flow over abstractions, frameworks, generated code, or clever shell tricks.
- Keep public surface minimal: exactly two executable entry points.
- Cross-platform parity is behavioral, not forced through unnatural shared code.
- Any duplicated logic introduced must be justified in the implementation commit/PR.

## Global Definition of Done
- Exactly `MobileShop.Scripts/Bash/log.sh` and `MobileShop.Scripts/PowerShell/log.ps2` are the public scripts.
- No 15-command wrapper family remains.
- CLI level/severity selection, all filters, file/stdin behavior, event grouping, ordering, `-n`, errors, and help are documented and parity-tested.
- Invalid dates/datetimes are caught, clearly explained on stderr, and return non-zero.
- DRY/SRP/KISS constraints are demonstrably followed.
- Existing application logging is unchanged.
- CI verifies both script implementations and the existing .NET job.
- Documentation is usable by a non-pro user.
- Actor follows the one implementation step → one commit → Job B verification workflow, then pushes as required.
- Claude/external verification occurs after GitHub Actions verification.
- Planner does not mark `.clinerules/to-do.md`; only Reviewer may complete the stage after final verification.
