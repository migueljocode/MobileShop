#!/usr/bin/env bash
set -euo pipefail
r="$(cd "$(dirname "$0")/../.."&&pwd)";s="$r/MobileShop.Scripts/Bash/log.sh";f="$r/MobileShop.Scripts/tests/fixtures/sample.log"
o="$(bash "$s" --level error "$f")";printf '%s\n' "$o";[[ "$(printf '%s\n' "$o"|grep -c $'\tERR\t')" == 2 ]];echo PASS-Bash
