#!/usr/bin/env bash
set -euo pipefail
r="$(cd "$(dirname "$0")/../.."&&pwd)";s="$r/MobileShop.Scripts/Bash/log.sh";f="$r/MobileShop.Scripts/tests/fixtures/sample.log"
fail(){ echo "FAIL: $*" >&2; exit 1; }
[[ "$(bash "$s" --level debug "$f"|grep -c $'\tDBG\t')" == 1 ]]||fail exact-debug
[[ "$(bash "$s" --level info "$f"|grep -c $'\tINF\t')" == 2 ]]||fail exact-info
[[ "$(bash "$s" --level warning "$f"|grep -c $'\tWRN\t')" == 1 ]]||fail exact-warning
[[ "$(bash "$s" --level error "$f"|grep -c $'\tERR\t')" == 2 ]]||fail exact-error
[[ "$(bash "$s" --level fatal "$f"|grep -c $'\tFTL\t')" == 1 ]]||fail exact-fatal
[[ "$(bash "$s" --level debug --above "$f"|grep -E -c $'\t(DBG|INF|WRN|ERR|FTL)\t')" == 7 ]]||fail above
[[ "$(bash "$s" --level warning --below "$f"|grep -E -c $'\t(DBG|INF|WRN)\t')" == 4 ]]||fail below
[[ "$(bash "$s" --level error -n 1 "$f"|grep -c $'\tERR\t')" == 1 ]]||fail number
[[ "$(bash "$s" --level error --date-from 2026-01-02 --date-to 2026-01-02 "$f"|grep -c $'\tERR\t')" == 1 ]]||fail date
[[ "$(bash "$s" --level error --date-time-from '2026-01-01 08:03:00' --date-time-to '2026-01-01 08:03:00' "$f"|grep -c $'\tERR\t')" == 1 ]]||fail datetime
[[ "$(cat "$f"|bash "$s" --level error -|grep -c $'\tERR\t')" == 2 ]]||fail stdin
space="$r/MobileShop.Scripts/tests/fixture with spaces.log";cp "$f" "$space";trap 'rm -f "$space"' EXIT
[[ "$(bash "$s" --level error "$space"|grep -c $'\tERR\t')" == 2 ]]||fail spaces
if bash "$s" --level error --date-from >/tmp/log-test-out 2>/tmp/log-test-err; then fail missing-date; fi
grep -q 'missing value for --date-from' /tmp/log-test-err||fail diagnostic
if bash "$s" --level error --date-from 2026-01-03 --date-to 2026-01-02 "$f" >/tmp/log-test-out 2>/tmp/log-test-err; then fail reversed; fi
if bash "$s" --level error --date-from 2026-99-01 "$f" >/tmp/log-test-out 2>/tmp/log-test-err; then fail invalid-date; fi
if bash "$s" --level error --date-from 2026-01-01 --date-time-from '2026-01-01 08:00:00' "$f" >/tmp/log-test-out 2>/tmp/log-test-err; then fail conflict; fi
printf '%s\n' '2026-01-01 08:03:00.000 +04:00	ERR	Fixture	error'|bash "$s" --level error - >/dev/null||fail pipeline
echo PASS-Bash