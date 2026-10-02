#!/usr/bin/env pwsh
param([Alias('l')][string]$Level,[Alias('h')][switch]$Help,[switch]$Above,[switch]$Below,[Alias('n')][long]$Number,[string]$DateFrom,[string]$DateTo,[string]$DateTimeFrom,[string]$DateTimeTo,[string]$Path)
function Help(){@'
Usage: log.ps1 --Level LEVEL [--Above|--Below] [filters] [-n N] [PATH|-]
LEVEL: debug info warning error fatal. Exact is default; Above/Below include selected and more/less severe.
-n is a positive event count. Dates: YYYY-MM-DD. Datetimes: "YYYY-MM-DD HH:mm:ss".
Date/datetime modes cannot mix. From-only uses current local date/time as to; to-only uses first event as from.
Default: logs/app-*.log. -Path selects one file literally; -Path - reads stdin and pipelines. Results/help stdout; diagnostics stderr; invalid usage is non-zero.
Bounds are inclusive. From-only uses current local date/time; to-only infers the lower bound from the first event; no bounds means no time restriction. Date and datetime modes cannot mix.
Examples: pwsh ./log.ps1 --Level error; pwsh ./log.ps1 --Level error --Above -Number 20; Get-Content logs/app-*.log | pwsh ./log.ps1 --Level error. Windows PowerShell 7 can invoke the .ps1 file directly with -File.
Invalid examples: -DateFrom 2026-02-30, reversed ranges, -Above -Below, and -Number 0 are rejected with stderr diagnostics.
'@}
function Fail($m){[Console]::Error.WriteLine("log.ps1: $m");exit 2}
if($Help -or $args -contains '--help' -or $args -contains '-h'){Help;exit 0}
if(!$Level){Fail '--Level is required (use --Help)'}
$levels=@{debug=0;info=1;warning=2;error=3;fatal=4}
if(!$levels.ContainsKey($Level)){Fail "invalid level '$Level'"}
if($Above -and $Below){Fail '--Above and --Below are mutually exclusive'}
if($PSBoundParameters.ContainsKey('Number') -and $Number -le 0){Fail '--Number must be positive'}
$dm=$PSBoundParameters.ContainsKey('DateFrom') -or $PSBoundParameters.ContainsKey('DateTo');$tm=$PSBoundParameters.ContainsKey('DateTimeFrom') -or $PSBoundParameters.ContainsKey('DateTimeTo');if($dm -and $tm){Fail 'date and datetime filters cannot be mixed'}
function PD($v,$o){$x=[datetime]::MinValue;if(![datetime]::TryParseExact($v,'yyyy-MM-dd',[Globalization.CultureInfo]::InvariantCulture,[Globalization.DateTimeStyles]::None,[ref]$x)){Fail "invalid $o '$v'; expected YYYY-MM-DD"};$x.Date}
function PT($v,$o){$x=[datetime]::MinValue;if(![datetime]::TryParseExact($v,'yyyy-MM-dd HH:mm:ss',[Globalization.CultureInfo]::InvariantCulture,[Globalization.DateTimeStyles]::None,[ref]$x)){Fail "invalid $o '$v'; expected YYYY-MM-DD HH:mm:ss"};$x}
$df=if($dm -and $DateFrom){PD $DateFrom '--DateFrom'}else{$null};$dt=if($dm -and $DateTo){PD $DateTo '--DateTo'}else{$null};$tf=if($tm -and $DateTimeFrom){PT $DateTimeFrom '--DateTimeFrom'}else{$null};$tt=if($tm -and $DateTimeTo){PT $DateTimeTo '--DateTimeTo'}else{$null}
function ReadE([string[]]$ls){$r=[Collections.Generic.List[object]]::new();$e=$null;foreach($x in $ls){if($x -match '^(?<s>\d{4}-\d{2}-\d{2} \d{2}:\d{2}:\d{2}\.\d{3}) [+-]\d{2}:\d{2}\t(?<l>DBG|INF|WRN|ERR|FTL)\t'){if($e){$r.Add($e)};$e=[pscustomobject]@{Text=$x;Timestamp=$Matches.s;Rank=@{DBG=0;INF=1;WRN=2;ERR=3;FTL=4}[$Matches.l]}}elseif($e){$e.Text+=[Environment]::NewLine+$x}};if($e){$r.Add($e)};$r}
if($Path -eq '-'){$lines=@($input)}elseif($Path){if(!(Test-Path -LiteralPath $Path -PathType Leaf)){Fail "input log file not found: $Path"};$lines=Get-Content -LiteralPath $Path}else{$files=@(Get-ChildItem 'logs/app-*.log' -File -ErrorAction SilentlyContinue|Sort-Object Name);if(!$files){Fail 'no input log files found: logs/app-*.log'};$lines=@();foreach($f in $files){$lines+=Get-Content $f.FullName}}
$ev=ReadE $lines;if(($dm -or $tm)-and !$ev.Count -and !$DateFrom -and !$DateTimeFrom){Fail 'cannot infer lower bound from empty input'}
if($dm){if(!$df){$df=[datetime]::ParseExact($ev[0].Timestamp.Substring(0,10),'yyyy-MM-dd',$null)};if(!$dt){$dt=(Get-Date).Date};if($df -gt $dt){Fail 'date range is reversed'}}
if($tm){if(!$tf){$tf=[datetime]::ParseExact($ev[0].Timestamp.Substring(0,19),'yyyy-MM-dd HH:mm:ss',$null)};if(!$tt){$tt=(Get-Date)};if($tf -gt $tt){Fail 'datetime range is reversed'}}
$want=$levels[$Level];$c=0;foreach($e in $ev){$ok=if($Above){$e.Rank-ge$want}elseif($Below){$e.Rank-le$want}else{$e.Rank-eq$want};if(!$ok){continue};$s=[datetime]::ParseExact($e.Timestamp,'yyyy-MM-dd HH:mm:ss.fff',$null);if($dm -and ($s.Date-lt$df -or $s.Date-gt$dt)){continue};if($tm -and ($s-lt$tf -or $s-gt$tt)){continue};Write-Output $e.Text;$c++;if($Number-and$c-ge$Number){break}}
