#!/usr/bin/env bash
# Local stage check: dotnet restore/build/test (same as CI), then graphify update.
# Prints a terse summary to paste back to the chatbot.
#
# Usage: ./check.sh        Env: SOLUTION=<path>  MAX_LINES=<n>
set -uo pipefail

cd "$(git rev-parse --show-toplevel)" || exit 2

SOLUTION="${SOLUTION:-src/MobileShop.slnx}"
MAX_LINES="${MAX_LINES:-20}"
WORKDIR="$(mktemp -d)"
trap 'rm -rf "$WORKDIR"' EXIT
export DOTNET_NOLOGO=1 DOTNET_CLI_TELEMETRY_OPTOUT=1

RESULTS=()
FAILED=0

# --- helpers ---------------------------------------------------------------

# Hash of the working tree (tracked + untracked, not ignored) via a throwaway
# index, so the real index is never touched.
tree_hash() {
  local index="$WORKDIR/index"
  git ls-files -z --cached --others --exclude-standard \
    | GIT_INDEX_FILE="$index" git update-index --add --remove -z --stdin
  GIT_INDEX_FILE="$index" git write-tree
  rm -f "$index"
}

# run_step <name> <command...>: log to $WORKDIR/<name>.log, record PASS/FAIL.
run_step() {
  local name="$1"; shift
  if "$@" >"$WORKDIR/$name.log" 2>&1; then
    RESULTS+=("$name: PASS")
  else
    RESULTS+=("$name: FAIL")
    FAILED=1
    return 1
  fi
}

skip_step() { RESULTS+=("$1: SKIP ($2)"); }

print_errors() {
  echo "--- $1: first $MAX_LINES error lines ---"
  grep -Ehi -A1 ' error |error:|failed[ !:]|exception' "$WORKDIR/$1.log" \
    | grep -v '^--$' | awk '!seen[$0]++' | head -n "$MAX_LINES"
}

# --- steps -----------------------------------------------------------------

check_dotnet() {
  if ! command -v dotnet >/dev/null; then
    skip_step dotnet "not installed"; FAILED=1; return
  fi
  run_step restore dotnet restore "$SOLUTION" -v q || return
  run_step build dotnet build "$SOLUTION" --no-restore -v q || return
  run_step test dotnet test "$SOLUTION" --no-build -v q
}

check_graphify() {
  if ! command -v graphify >/dev/null; then
    skip_step graphify "not installed"; return
  fi
  run_step graphify graphify update .
}

# --- output ----------------------------------------------------------------

print_summary() {
  local tree="$1" result
  echo "STATUS: $([ "$FAILED" -eq 0 ] && echo PASS || echo FAIL)"
  echo "HEAD: $(git rev-parse --short HEAD)  TREE: $tree"
  printf '%s\n' "${RESULTS[@]}"
  for result in "${RESULTS[@]}"; do
    case "$result" in *": FAIL") print_errors "${result%%:*}" ;; esac
  done
  if [ -n "$(git status --porcelain -- graphify-out)" ]; then
    echo "NOTE: graphify-out changed (commit separately: chore: update graph)"
  fi
}

main() {
  local tree
  tree="$(tree_hash)"   # taken before graphify rewrites files
  check_dotnet
  if [ "$FAILED" -eq 0 ]; then
    check_graphify
  else
    skip_step graphify "earlier step failed"
  fi
  print_summary "$tree"
  return "$FAILED"
}

main "$@"
