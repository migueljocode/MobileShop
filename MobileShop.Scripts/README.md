# MobileShop log query scripts

Exactly two public entry points:
- `MobileShop.Scripts/Bash/log.sh`
- `MobileShop.Scripts/PowerShell/log.ps2`

Both use the same CLI: `--level debug|info|warning|error|fatal`, optional `--above`/`--below`, `-n`, file or stdin, date/date-time filters, and self-contained help.

Date bounds are inclusive. Date-times compare the displayed local wall-clock value from Serilog; the recorded offset is not converted. From-only captures current local date/time once; to-only uses the first event.

Examples:
```bash
./MobileShop.Scripts/Bash/log.sh --level error
./MobileShop.Scripts/Bash/log.sh --level error --above -n 20
./MobileShop.Scripts/Bash/log.sh --level warning --date-from 2026-01-01 --date-to 2026-01-31
cat logs/app-*.log | ./MobileShop.Scripts/Bash/log.sh --level error
```
```powershell
pwsh ./MobileShop.Scripts/PowerShell/log.ps2 --Level error
Get-Content logs/app-*.log | pwsh ./MobileShop.Scripts/PowerShell/log.ps2 --Level error
# Windows PowerShell 7: .ps2 is intentionally not a native -File extension; load it as a scriptblock.
pwsh -Command "& ([scriptblock]::Create((Get-Content -Raw -LiteralPath './MobileShop.Scripts/PowerShell/log.ps2'))) -Level error"
```
No grep/awk knowledge is required.
