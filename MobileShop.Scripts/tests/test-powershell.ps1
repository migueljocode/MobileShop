$ErrorActionPreference = 'Stop'
$root = Split-Path -Parent (Split-Path -Parent $PSScriptRoot)
$script = Join-Path $root 'MobileShop.Scripts/PowerShell/log.ps2'
$fixture = Join-Path $root 'MobileShop.Scripts/tests/fixtures/sample.log'
$tmp = Join-Path ([IO.Path]::GetTempPath()) ("MobileShopLogTests-" + [guid]::NewGuid())
New-Item -ItemType Directory -Path $tmp | Out-Null
try {
  function Fail($m) { throw "FAIL: $m" }
  function Lines($o) { @($o | Where-Object { $_ -match '^2026-' }) }
  function Expect-Fail([scriptblock]$Command, [string]$Label) {
    $stdout = & $Command 2>&1
    $code = $LASTEXITCODE
    if ($code -eq 0) { Fail "$Label unexpectedly succeeded" }
    $text = @($stdout) -join [Environment]::NewLine
    if ([string]::IsNullOrWhiteSpace($text)) { Fail "$Label produced no diagnostic" }
  }

  $exact = @(& $script -Level error -Path $fixture)
  if ((Lines $exact).Count -ne 2) { Fail 'exact error count' }
  if (-not ($exact -match 'System.InvalidOperationException: fixture')) { Fail 'multiline event missing' }
  if (-not ($exact -match '   at Fixture.Method\(\)')) { Fail 'multiline continuation missing' }

  if ((Lines @(& $script -Level warning -Above -Path $fixture)).Count -ne 3) { Fail 'above semantics' }
  if ((Lines @(& $script -Level warning -Below -Path $fixture)).Count -ne 3) { Fail 'below semantics' }
  if ((Lines @(& $script -Level error -Number 1 -Path $fixture)).Count -ne 1) { Fail '-n semantics' }

  $dateExact = @( & $script -Level error -DateFrom 2026-01-02 -DateTo 2026-01-02 -Path $fixture )
  if ((Lines $dateExact).Count -ne 1 -or $dateExact -notmatch 'second error') { Fail 'date boundary' }
  $dtExact = @( & $script -Level error -DateTimeFrom '2026-01-01 08:03:00' -DateTimeTo '2026-01-01 08:03:00' -Path $fixture )
  if ((Lines $dtExact).Count -ne 1) { Fail 'datetime boundary' }

  Expect-Fail { & $script -Level nope -Path $fixture } 'invalid level'
  Expect-Fail { & $script -Path $fixture } 'missing level'
  Expect-Fail { & $script -Level error -DateFrom } 'missing date value'
  Expect-Fail { & $script -Level error -DateTimeFrom } 'missing datetime value'
  Expect-Fail { & $script -Level error -DateFrom 2026-02-30 -Path $fixture } 'invalid date'
  Expect-Fail { & $script -Level error -DateTimeFrom '2026-01-01 25:00:00' -Path $fixture } 'invalid datetime'
  Expect-Fail { & $script -Level error -DateFrom 2026-01-03 -DateTo 2026-01-02 -Path $fixture } 'reversed date'
  Expect-Fail { & $script -Level error -DateTimeFrom '2026-01-03 10:00:00' -DateTimeTo '2026-01-02 10:00:00' -Path $fixture } 'reversed datetime'
  Expect-Fail { & $script -Level error -DateFrom 2026-01-01 -DateTimeFrom '2026-01-01 08:00:00' -Path $fixture } 'conflicting filters'
  Expect-Fail { & $script -Level error -Above -Below -Path $fixture } 'conflicting severity modes'
  Expect-Fail { & $script -Level error -Number 0 -Path $fixture } 'bad number'
  Expect-Fail { & $script -Level error -Path (Join-Path $tmp 'no-such.log') } 'missing input'

  $stdinOut = @($null)
  $stdinOut = Get-Content -LiteralPath $fixture | & $script -Level error -Path -
  if (($stdinOut -join [Environment]::NewLine) -ne ($exact -join [Environment]::NewLine)) { Fail 'stdin parity' }

  $spaced = Join-Path $tmp 'log with spaces.log'
  Copy-Item $fixture $spaced
  $spaceOut = @(& $script -Level error -Path $spaced)
  if (($spaceOut -join [Environment]::NewLine) -ne ($exact -join [Environment]::NewLine)) { Fail 'path with spaces' }

  $noMatch = @(& $script -Level fatal -DateFrom 2026-01-03 -DateTo 2026-01-03 -Path $fixture)
  if ($noMatch.Count -ne 0) { Fail 'no-match output' }

  $help = @(& $script -Help) -join [Environment]::NewLine
  if ($help -notmatch '--date-from') { Fail 'help missing date options' }

  $bashScript = Join-Path $root 'MobileShop.Scripts/Bash/log.sh'
  $bashOut = @(& bash $bashScript --level error $fixture)
  if (($bashOut -join [Environment]::NewLine) -ne ($exact -join [Environment]::NewLine)) { Fail 'cross-platform parity' }

  Write-Output 'PASS-PowerShell comprehensive'
}
finally { Remove-Item -LiteralPath $tmp -Recurse -Force -ErrorAction SilentlyContinue }
