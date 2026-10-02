$ErrorActionPreference = 'Stop'
$root = Split-Path -Parent $PSScriptRoot
$script = Join-Path $root 'PowerShell/log.ps1'
$fixture = Join-Path $root 'tests/fixtures/sample.log'
$tmp = Join-Path ([IO.Path]::GetTempPath()) ("MobileShopLogTests-" + [guid]::NewGuid())
New-Item -ItemType Directory -Path $tmp | Out-Null
$runnable = Join-Path $tmp 'log.ps1'
Copy-Item -LiteralPath $script -Destination $runnable
$scriptBlock = [scriptblock]::Create((Get-Content -Raw -LiteralPath $script))
try {
  function Fail($m) { throw "FAIL: $m" }
  function Lines($o) { [regex]::Matches(($o -join [Environment]::NewLine), '(?m)^2026-') }
  function Expect-Fail([string[]]$Arguments,[string]$Label) {
    $out=Join-Path $tmp 'out';$err=Join-Path $tmp 'err'
    & pwsh -NoProfile -File $runnable @Arguments >$out 2>$err
    if($LASTEXITCODE -eq 0){Fail "$Label unexpectedly succeeded"}
    if((Get-Item $out).Length -ne 0){Fail "$Label wrote stdout"}
    if((Get-Item $err).Length -eq 0){Fail "$Label wrote no stderr"}
  }

  $expected=@{debug=1;info=2;warning=1;error=2;fatal=1}
  foreach($level in $expected.Keys){if((Lines @(& $scriptBlock -Level $level -Path $fixture)).Count -ne $expected[$level]){Fail "exact $level"}}
  $exact = @(& $scriptBlock -Level error -Path $fixture)
  if ((Lines $exact).Count -ne 2) { Fail 'exact error count' }
  if (-not ($exact -match 'System.InvalidOperationException: fixture')) { Fail 'multiline event missing' }
  if (-not ($exact -match '   at Fixture.Method()')) { Fail 'multiline continuation missing' }

  if(-not $IsWindows) {
    $above = @(& $scriptBlock -Level error -Above -Path $fixture) -join [Environment]::NewLine
    if ($above -notmatch '\tERR\t' -or $above -notmatch '\tFTL\t' -or $above -match '\tWRN\t') { Fail 'above semantics' }
    $below = @(& $scriptBlock -Level error -Below -Path $fixture) -join [Environment]::NewLine
    if ($below -notmatch '\tDBG\t' -or $below -notmatch '\tINF\t' -or $below -notmatch '\tWRN\t' -or $below -notmatch '\tERR\t' -or $below -match '\tFTL\t') { Fail 'below semantics' }
  }

  if ((Lines @(& $scriptBlock -Level error -Number 1 -Path $fixture)).Count -ne 1) { Fail '-n semantics' }
  if ((Lines @(& $scriptBlock -Level error -Number 999 -Path $fixture)).Count -ne 2) { Fail 'large number' }
  if ((Lines @(& $scriptBlock -Level error -DateFrom 2026-01-02 -DateTo 2026-01-02 -Path $fixture)).Count -ne 1) { Fail 'date boundary' }
  if ((Lines @(& $scriptBlock -Level error -DateTimeFrom '2026-01-01 08:03:00' -DateTimeTo '2026-01-01 08:03:00' -Path $fixture)).Count -ne 1) { Fail 'datetime boundary' }
  if ((Lines @(& $scriptBlock -Level error -DateFrom 2026-01-01 -Path $fixture)).Count -ne 2) { Fail 'date from-only' }
  if ((Lines @(& $scriptBlock -Level error -DateTo 2026-01-02 -Path $fixture)).Count -ne 2) { Fail 'date to-only' }
  if ((Lines @(& $scriptBlock -Level error -DateTimeFrom '2026-01-01 08:03:00' -Path $fixture)).Count -ne 2) { Fail 'datetime from-only' }
  if ((Lines @(& $scriptBlock -Level error -DateTimeTo '2026-01-01 08:03:00' -Path $fixture)).Count -ne 1) { Fail 'datetime to-only' }

  if(-not $IsWindows) {
    Expect-Fail @('-Level','nope','-Path',$fixture) 'invalid level'
    Expect-Fail @('-Path',$fixture) 'missing level'
    Expect-Fail @('-Level','error','-DateFrom') 'missing date value'
    Expect-Fail @('-Level','error','-DateTimeFrom') 'missing datetime value'
    Expect-Fail @('-Level','error','-DateFrom','2026-02-30','-Path',$fixture) 'invalid date'
    Expect-Fail @('-Level','error','-DateTimeFrom','2026-01-01 25:00:00','-Path',$fixture) 'invalid datetime'
    Expect-Fail @('-Level','error','-DateFrom','2026-01-03','-DateTo','2026-01-02','-Path',$fixture) 'reversed date'
    Expect-Fail @('-Level','error','-DateTimeFrom','2026-01-03 10:00:00','-DateTimeTo','2026-01-02 10:00:00','-Path',$fixture) 'reversed datetime'
    Expect-Fail @('-Level','error','-DateFrom','2026-01-01','-DateTimeFrom','2026-01-01 08:00:00','-Path',$fixture) 'conflicting filters'
    Expect-Fail @('-Level','error','-Above','-Below','-Path',$fixture) 'conflicting severity modes'
    Expect-Fail @('-Level','error','-Number','0','-Path',$fixture) 'zero number'
    Expect-Fail @('-Level','error','-Number','-1','-Path',$fixture) 'negative number'
    Expect-Fail @('-Level','error','-Number','abc','-Path',$fixture) 'non-numeric number'
    Expect-Fail @('-Level','error','-Path',(Join-Path $tmp 'no-such.log')) 'missing input'
  }

  $stdinOut = @(Get-Content -LiteralPath $fixture | & $scriptBlock -Level error -Path -)
  if (($stdinOut -join [Environment]::NewLine) -ne ($exact -join [Environment]::NewLine)) { Fail 'stdin parity' }
  $spaced = Join-Path $tmp 'log with spaces.log';Copy-Item $fixture $spaced
  $spaceOut = @(& $scriptBlock -Level error -Path $spaced)
  if (($spaceOut -join [Environment]::NewLine) -ne ($exact -join [Environment]::NewLine)) { Fail 'path with spaces' }
  if (@(& $scriptBlock -Level fatal -DateFrom 2026-01-03 -DateTo 2026-01-03 -Path $fixture).Count -ne 0) { Fail 'no-match output' }
  $empty=Join-Path $tmp 'empty.log';New-Item -ItemType File -Path $empty | Out-Null
  if(-not $IsWindows){Expect-Fail @('-Level','error','-DateTo','2026-01-02','-Path',$empty) 'empty first-entry inference'}

  $help = @(& $scriptBlock -Help) -join [Environment]::NewLine
  if ($help -notmatch '-DateFrom' -or $help -notmatch '-DateTimeFrom' -or $help -notmatch '-Number') { Fail 'help missing date/number options' }

  $bashScript = Join-Path $root 'Bash/log.sh'
  $bashOut = @(& bash $bashScript --level error $fixture)
  if (($bashOut -join [Environment]::NewLine) -ne ($exact -join [Environment]::NewLine)) { Fail 'cross-platform parity' }

  Write-Output 'PASS-PowerShell comprehensive'
}
finally { Remove-Item -LiteralPath $tmp -Recurse -Force -ErrorAction SilentlyContinue }
