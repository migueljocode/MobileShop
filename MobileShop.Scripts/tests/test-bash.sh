#!/usr/bin/env bash
set -euo pipefail
r="$(cd "$(dirname "$0")/../.."&&pwd)";s="$r/MobileShop.Scripts/Bash/log.sh";f="$r/MobileShop.Scripts/tests/fixtures/sample.log"
[[ "$(bash "$s" --level error "$f"|grep -c $'\tERR\t')" == 2 ]];[[ "$(bash "$s" --level warning --above "$f"|grep -c $'\t')" == 6 ]];[[ "$(bash "$s" --level warning --below "$f"|grep -c $'\t')" == 5 ]];[[ "$(bash "$s" --level error -n 1 "$f"|grep -c $'\tERR\t')" == 1 ]];[[ "$(bash "$s" --level error --date-from 2026-01-02 "$f"|grep -c $'\tERR\t')" == 1 ]];[[ "$(bash "$s" --level error --date-time-from '2026-01-01 08:03:00' --date-time-to '2026-01-01 08:03:00' "$f"|grep -c $'\tERR\t')" == 1 ]];bash "$s" --help>/dev/null;echo PASS-Bash
