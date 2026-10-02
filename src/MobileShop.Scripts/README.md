# MobileShop log query scripts

Exactly two public entry points:
- `MobileShop.Scripts/Bash/log.sh`
- `MobileShop.Scripts/PowerShell/log.ps1`

Both use the same CLI: `--level debug|info|warning|error|fatal`, optional `--above`/`--below`, `-n`, file or stdin, date/date-time filters, and self-contained help.

Date bounds are inclusive. Date-times compare the displayed local wall-clock value from Serilog; the recorded offset is not converted. From-only captures current local date/time once; to-only uses the first event.

Examples:
```bash
./MobileShop.Scripts/Bash/log.sh --level error
./MobileShop.Scripts/Bash/log.sh --level error --above -n 20
./MobileShop.Scripts/Bash/log.sh --level warning --date-from 2026-01-01 --date-to 2026-01-31
./MobileShop.Scripts/Bash/log.sh --level error --date-time-from "2026-01-01 08:00:00" --date-time-to "2026-01-01 18:00:00"
cat logs/app-*.log | ./MobileShop.Scripts/Bash/log.sh --level error
```
```powershell
pwsh ./MobileShop.Scripts/PowerShell/log.ps1 --Level error
pwsh ./MobileShop.Scripts/PowerShell/log.ps1 --Level error --Above -Number 20
Get-Content logs/app-*.log | pwsh ./MobileShop.Scripts/PowerShell/log.ps1 --Level error
```
No grep/awk knowledge is required.
