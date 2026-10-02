$ErrorActionPreference='Stop'
$r=Split-Path -Parent (Split-Path -Parent $PSScriptRoot);$s=Join-Path $r 'MobileShop.Scripts/PowerShell/log.ps2';$f=Join-Path $r 'MobileShop.Scripts/tests/fixtures/sample.log'
function Assert([bool]$ok,[string]$name){if(!$ok){throw "FAIL: $name"}}
function Run([string[]]$a){& pwsh -NoProfile -File $s @a 2>&1}
Assert ((Run @('-Level','debug','-Path',$f)|Select-String '\tDBG\t').Count -eq 1) 'exact-debug'
Assert ((Run @('-Level','info','-Path',$f)|Select-String '\tINF\t').Count -eq 2) 'exact-info'
Assert ((Run @('-Level','warning','-Path',$f)|Select-String '\tWRN\t').Count -eq 1) 'exact-warning'
Assert ((Run @('-Level','error','-Path',$f)|Select-String '\tERR\t').Count -eq 2) 'exact-error'
Assert ((Run @('-Level','fatal','-Path',$f)|Select-String '\tFTL\t').Count -eq 1) 'exact-fatal'
Assert ((Run @('-Level','debug','-Above','-Path',$f)|Select-String '\t(DBG|INF|WRN|ERR|FTL)\t').Count -eq 7) 'above'
Assert ((Run @('-Level','warning','-Below','-Path',$f)|Select-String '\t(DBG|INF|WRN)\t').Count -eq 3) 'below'
Assert ((Run @('-Level','error','-Number','1','-Path',$f)|Select-String '\tERR\t').Count -eq 1) 'number'
Assert ((Run @('-Level','error','-DateFrom','2026-01-02','-DateTo','2026-01-02','-Path',$f)|Select-String '\tERR\t').Count -eq 1) 'date'
Assert ((Run @('-Level','error','-DateTimeFrom','2026-01-01 08:03:00','-DateTimeTo','2026-01-01 08:03:00','-Path',$f)|Select-String '\tERR\t').Count -eq 1) 'datetime'
Assert ((Get-Content -LiteralPath $f|& pwsh -NoProfile -File $s -Level error -Path -|Select-String '\tERR\t').Count -eq 2) 'pipeline'
$space=Join-Path $r 'MobileShop.Scripts/tests/fixture with spaces.log';Copy-Item -LiteralPath $f -Destination $space;try{Assert ((Run @('-Level','error','-Path',$space)|Select-String '\tERR\t').Count -eq 2) 'spaces'}finally{Remove-Item -LiteralPath $space -Force}
$bad=Run @('-Level','not-a-level','-Path',$f);Assert (($bad -join [Environment]::NewLine) -match "invalid level 'not-a-level'") 'invalid-level'
$bad=Run @('-Level','error','-DateFrom','2026-99-01','-Path',$f);Assert (($bad -join [Environment]::NewLine) -match 'invalid --DateFrom') 'invalid-date'
$bad=Run @('-Level','error','-DateFrom','2026-01-03','-DateTo','2026-01-02','-Path',$f);Assert (($bad -join [Environment]::NewLine) -match 'date range is reversed') 'reversed'
$bad=Run @('-Level','error','-DateFrom','2026-01-01','-DateTimeFrom','2026-01-01 08:00:00','-Path',$f);Assert (($bad -join [Environment]::NewLine) -match 'cannot be mixed') 'conflict'
& pwsh -NoProfile -File $s -h|Out-Null
echo PASS-PowerShell