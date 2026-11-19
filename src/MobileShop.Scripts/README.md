# MobileShop log query scripts

Exactly two public entry points:
- `MobileShop.Scripts/Bash/log.sh`
- `MobileShop.Scripts/PowerShell/log.ps1`

Both use the same CLI: `--level debug|info|warning|error|fatal`, optional `--above`/`--below`, `-n`, `-p`/`--path`, file or stdin, date/date-time filters, and self-contained help. The log path can be supplied explicitly (including paths with spaces); `-` reads stdin, and omitting the path uses `logs/app-*.log`.

Date bounds are inclusive. Date-times compare the displayed local wall-clock value from Serilog; the recorded offset is not converted. From-only captures current local date/time once; to-only uses the first event.

Examples:
```bash
./MobileShop.Scripts/Bash/log.sh --level error --path "logs/my app.log"
./MobileShop.Scripts/Bash/log.sh --level error --above -n 20
./MobileShop.Scripts/Bash/log.sh --level warning --date-from 2026-01-01 --date-to 2026-01-31
./MobileShop.Scripts/Bash/log.sh --level error --date-time-from "2026-01-01 08:00:00" --date-time-to "2026-01-01 18:00:00"
cat logs/app-*.log | ./MobileShop.Scripts/Bash/log.sh --level error
```
```powershell
pwsh ./MobileShop.Scripts/PowerShell/log.ps1 --Level error -Path "logs/my app.log"
pwsh ./MobileShop.Scripts/PowerShell/log.ps1 --Level error --Above -Number 20
Get-Content logs/app-*.log | pwsh ./MobileShop.Scripts/PowerShell/log.ps1 --Level error
```
No grep/awk knowledge is required.

# MobileShop local installer and tests

Entry point: `src/MobileShop.Scripts/Bash/install.sh` (also installed as `ROOT/app/bin/mobileshop` and symlinked to `${MOBILESHOP_BIN_DIR:-$HOME/.local/bin}/mobileshop`).

- Bash 3.2-compatible (`#!/usr/bin/env bash`, `set -eu`) and portable across GNU/Linux (`linux-x64`) and macOS Apple Silicon (`osx-arm64`).
- Supports `install`, `update`, `rollback`, `prune`, `adopt`, `backup`, `start`, `stop`, `restart`, `status`, `logs`, `doctor`, `version`, `uninstall`, and `run`.
- Supports environment overrides used in deployments and tests: `MOBILESHOP_ROOT`, `MOBILESHOP_BIN_DIR`, `MOBILESHOP_PORT`, `MOBILESHOP_SERVICE`, `MOBILESHOP_REPO`, `MOBILESHOP_BASE_URL`, `MOBILESHOP_UNAME_S`, and `MOBILESHOP_UNAME_M`.

## Running the installer tests

Run the fake-archive suite (scenarios `T1`–`T15`, using isolated temporary directories and a local Python HTTP redirect server):

```bash
bash src/MobileShop.Scripts/tests/test-install.sh
```

Lint with ShellCheck:

```bash
shellcheck -s bash src/MobileShop.Scripts/Bash/install.sh src/MobileShop.Scripts/tests/test-install.sh
```

To run the release smoke scenarios (`S1`–`S9`) against a packaged `MobileShop-<TAG>-<RID>.tar.gz` archive:

```bash
bash .github/scripts/installer-smoke.sh src/MobileShop.Scripts/Bash/install.sh <path-to-archive.tar.gz>
```

## CI checks (`.github/workflows/dotnet.yml`)

The `Log utility` job matrix runs across three OS legs:

- **`ubuntu-24.04`:** runs `test-bash.sh`, installs `shellcheck` if missing, runs `shellcheck -s bash` on `install.sh` and `test-install.sh`, runs `test-install.sh`, and runs `test-powershell.ps1`.
- **`windows-2025`:** runs `test-powershell.ps1`.
- **`macos-latest`:** installs `shellcheck` if missing, runs `shellcheck -s bash` on `install.sh` and `test-install.sh`, and runs `test-install.sh`.


# Local repository check

Entry point: `src/MobileShop.Scripts/Bash/check.sh` (run from the repository root).

```bash
./src/MobileShop.Scripts/Bash/check.sh
```

The helper runs restore, build and tests, then runs `graphify update .` when available. If the graph changes, it commits only `graphify-out/` as `chore: update graph`. It prints a concise summary suitable for sharing with the chatbot. Optional environment variables: `SOLUTION` (defaults to `src/MobileShop.slnx`) and `MAX_LINES` (defaults to `20`).
