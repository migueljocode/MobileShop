#!/usr/bin/env bash
set -euo pipefail

root="$(cd "$(dirname "$0")/../.." && pwd)"
script="$root/MobileShop.Scripts/Bash/log.sh"
fixture="$root/MobileShop.Scripts/tests/fixtures/sample.log"
tmp="$(mktemp -d)"
trap 'rm -rf "$tmp"' EXIT

fail(){ printf 'FAIL: %s\n' "$*" >&2; exit 1; }
expect_fail(){
  local label="$1"; shift
  local out="$tmp/out" err="$tmp/err"
  if "$@" >"$out" 2>"$err"; then fail "$label unexpectedly succeeded"; fi
  [[ ! -s "$out" ]] || fail "$label wrote to stdout"
  [[ -s "$err" ]] || fail "$label wrote no diagnostic"
}
count_events(){ grep -c '^2026-' <<<"$1" || true; }

exact="$(bash "$script" --level error "$fixture")"
[[ "$(count_events "$exact")" == 2 ]] || fail "exact error count"
grep -q $'\tERR\tFixture\terror' <<<"$exact" || fail "first error missing"
grep -q $'\tERR\tFixture\tsecond error' <<<"$exact" || fail "second error missing"
grep -q 'System.InvalidOperationException: fixture' <<<"$exact" || fail "multiline event missing"
grep -q '   at Fixture.Method()' <<<"$exact" || fail "multiline continuation missing"

[[ "$(bash "$script" --level warning --above "$fixture" | grep -c '^2026-')" == 3 ]] || fail "above semantics"
[[ "$(bash "$script" --level warning --below "$fixture" | grep -c '^2026-')" == 3 ]] || fail "below semantics"
[[ "$(bash "$script" --level error -n 1 "$fixture" | grep -c '^2026-')" == 1 ]] || fail "-n semantics"

date_exact="$(bash "$script" --level error --date-from 2026-01-02 --date-to 2026-01-02 "$fixture")"
[[ "$(count_events "$date_exact")" == 1 ]] || fail "date boundary"
grep -q 'second error' <<<"$date_exact" || fail "date boundary selected wrong event"
dt_exact="$(bash "$script" --level error --date-time-from '2026-01-01 08:03:00' --date-time-to '2026-01-01 08:03:00' "$fixture")"
[[ "$(count_events "$dt_exact")" == 1 ]] || fail "datetime boundary"

expect_fail "invalid level" bash "$script" --level nope "$fixture"
expect_fail "missing level" bash "$script" "$fixture"
expect_fail "missing date value" bash "$script" --level error --date-from
expect_fail "missing datetime value" bash "$script" --level error --date-time-from
expect_fail "invalid date" bash "$script" --level error --date-from 2026-02-30 "$fixture"
expect_fail "invalid datetime" bash "$script" --level error --date-time-from '2026-01-01 25:00:00' "$fixture"
expect_fail "reversed date" bash "$script" --level error --date-from 2026-01-03 --date-to 2026-01-02 "$fixture"
expect_fail "reversed datetime" bash "$script" --level error --date-time-from '2026-01-03 10:00:00' --date-time-to '2026-01-02 10:00:00' "$fixture"
expect_fail "conflicting filters" bash "$script" --level error --date-from 2026-01-01 --date-time-from '2026-01-01 08:00:00' "$fixture"
expect_fail "conflicting severity modes" bash "$script" --level error --above --below "$fixture"
expect_fail "bad number" bash "$script" --level error -n 0 "$fixture"
expect_fail "unknown option" bash "$script" --level error --wat "$fixture"
expect_fail "missing input" bash "$script" --level error "$tmp/no-such.log"

stdin_out="$(cat "$fixture" | bash "$script" --level error -)"
[[ "$stdin_out" == "$exact" ]] || fail "stdin parity"

cp "$fixture" "$tmp/log with spaces.log"
spaced="$(bash "$script" --level error "$tmp/log with spaces.log")"
[[ "$spaced" == "$exact" ]] || fail "path with spaces"

no_match="$(bash "$script" --level fatal --date-from 2026-01-03 --date-to 2026-01-03 "$fixture")"
[[ -z "$no_match" ]] || fail "no-match output"

help="$(bash "$script" --help)"
grep -q -- '--date-from/--date-to' <<<"$help" || fail "help missing date options"
grep -q -- 'PATH selects one file' <<<"$help" || fail "help missing path contract"

printf 'PASS-Bash comprehensive\n'
