#!/usr/bin/env bash
set -euo pipefail
root="$(cd "$(dirname "$0")/../.." && pwd)"
script="$root/MobileShop.Scripts/Bash/log.sh"
ps_source="$root/MobileShop.Scripts/PowerShell/log.ps2"
fixture="$root/MobileShop.Scripts/tests/fixtures/sample.log"
tmp="$(mktemp -d)"
trap 'rm -rf "$tmp"' EXIT

fail(){ printf 'FAIL: %s\n' "$*" >&2; exit 1; }
expect_fail(){ local out="$tmp/out" err="$tmp/err"; if "$@" >"$out" 2>"$err"; then fail "$1 unexpectedly succeeded"; fi; [[ ! -s "$out" ]]||fail "$1 wrote stdout"; [[ -s "$err" ]]||fail "$1 wrote no stderr"; }
headers(){ grep -c '^2026-' <<<"$1" || true; }

for level in debug info warning error fatal; do
  out="$(bash "$script" --level "$level" "$fixture")"
  [[ "$(headers "$out")" -gt 0 ]] || fail "exact $level"
done

above="$(bash "$script" --level error --above "$fixture")"
[[ "$(headers "$above")" == 3 ]] || fail "above count"
! grep -q $'\tWRN\t' <<<"$above" || fail "above included warning"
grep -q $'\tFTL\t' <<<"$above" || fail "above missed fatal"

below="$(bash "$script" --level error --below "$fixture")"
[[ "$(headers "$below")" == 6 ]] || fail "below count"
grep -q $'\tDBG\t' <<<"$below" || fail "below missed debug"
grep -q $'\tINF\t' <<<"$below" || fail "below missed info"
grep -q $'\tWRN\t' <<<"$below" || fail "below missed warning"
! grep -q $'\tFTL\t' <<<"$below" || fail "below included fatal"

exact="$(bash "$script" --level error "$fixture")"
[[ "$(headers "$exact")" == 2 ]] || fail "exact error count"
grep -q 'System.InvalidOperationException: fixture' <<<"$exact" || fail "multiline event"
grep -q '   at Fixture.Method()' <<<"$exact" || fail "multiline continuation"
[[ "$(headers "$(bash "$script" --level error -n 1 "$fixture")")" == 1 ]] || fail "number"

[[ "$(headers "$(bash "$script" --level error --date-from 2026-01-02 --date-to 2026-01-02 "$fixture")")" == 1 ]] || fail "date range"
[[ "$(headers "$(bash "$script" --level error --date-from 2026-01-01 "$fixture")")" == 2 ]] || fail "date from-only"
[[ "$(headers "$(bash "$script" --level error --date-to 2026-01-02 "$fixture")")" == 2 ]] || fail "date to-only"
[[ "$(headers "$(bash "$script" --level error --date-time-from '2026-01-01 08:03:00' --date-time-to '2026-01-01 08:03:00' "$fixture")")" == 1 ]] || fail "datetime range"
[[ "$(headers "$(bash "$script" --level error --date-time-from '2026-01-01 08:03:00' "$fixture")")" == 2 ]] || fail "datetime from-only"
[[ "$(headers "$(bash "$script" --level error --date-time-to '2026-01-01 08:03:00' "$fixture")")" == 1 ]] || fail "datetime to-only"

expect_fail "invalid level" bash "$script" --level nope "$fixture"
expect_fail "missing level" bash "$script" "$fixture"
expect_fail "missing date" bash "$script" --level error --date-from
expect_fail "missing datetime" bash "$script" --level error --date-time-from
expect_fail "invalid date" bash "$script" --level error --date-from 2026-02-30 "$fixture"
expect_fail "invalid datetime" bash "$script" --level error --date-time-from '2026-01-01 25:00:00' "$fixture"
expect_fail "reversed date" bash "$script" --level error --date-from 2026-01-03 --date-to 2026-01-02 "$fixture"
expect_fail "reversed datetime" bash "$script" --level error --date-time-from '2026-01-03 10:00:00' --date-time-to '2026-01-02 10:00:00' "$fixture"
expect_fail "mixed filters" bash "$script" --level error --date-from 2026-01-01 --date-time-from '2026-01-01 08:00:00' "$fixture"
expect_fail "conflicting modes" bash "$script" --level error --above --below "$fixture"
expect_fail "zero number" bash "$script" --level error -n 0 "$fixture"
expect_fail "negative number" bash "$script" --level error -n -1 "$fixture"
expect_fail "unknown option" bash "$script" --level error --wat "$fixture"

stdin_out="$(cat "$fixture" | bash "$script" --level error -)"
[[ "$stdin_out" == "$exact" ]] || fail "stdin parity"
cp "$fixture" "$tmp/log with spaces.log"
[[ "$(bash "$script" --level error "$tmp/log with spaces.log")" == "$exact" ]] || fail "path spaces"
[[ -z "$(bash "$script" --level fatal --date-from 2026-01-03 --date-to 2026-01-03 "$fixture")" ]] || fail "no match"

help="$(bash "$script" --help)"
grep -q -- '--date-from/--date-to' <<<"$help" || fail "help dates"
grep -q -- 'PATH selects one file' <<<"$help" || fail "help path"

ps_run="$tmp/log.ps1"
cp "$ps_source" "$ps_run"
ps_exact="$(pwsh -NoProfile -File "$ps_run" -Level error -Path "$fixture")"
[[ "$ps_exact" == "$exact" ]] || fail "cross-platform parity"

printf 'PASS-Bash comprehensive\n'