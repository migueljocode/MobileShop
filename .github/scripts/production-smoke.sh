#!/usr/bin/env bash
set -euo pipefail

ROOT="$(git rev-parse --show-toplevel)"
DB="$ROOT/MobileShop.db"
LOG_DIR="$ROOT/MobileShop.Log"
SMOKE_DIR="$ROOT/TestResults/ProductionSmoke"
DEV_LOG="$SMOKE_DIR/development.log"
PROD_GUARD_LOG="$SMOKE_DIR/production-guard.log"
MIGRATE_LOG="$SMOKE_DIR/migrate.log"
PROD_LOG="$SMOKE_DIR/production.log"

mkdir -p "$SMOKE_DIR"
rm -f "$DB" "$DB"-wal "$DB"-shm
rm -f "$DB".*.bak
rm -rf "$LOG_DIR"

APP_PID=""

cleanup() {
  if [[ -n "${APP_PID:-}" ]] && kill -0 "$APP_PID" 2>/dev/null; then
    kill "$APP_PID" 2>/dev/null || true
    for _ in {1..20}; do
      if ! kill -0 "$APP_PID" 2>/dev/null; then
        break
      fi
      sleep 0.25
    done
    kill -9 "$APP_PID" 2>/dev/null || true
  fi
}
trap cleanup EXIT INT TERM

wait_for_home() {
  local url="$1"
  local log="$2"
  for _ in {1..60}; do
    if curl --silent --show-error --fail --max-time 2 -o /dev/null "$url"; then
      return 0
    fi
    if ! kill -0 "$APP_PID" 2>/dev/null; then
      echo "Application exited before becoming ready." >&2
      tail -n 80 "$log" >&2 || true
      return 1
    fi
    sleep 1
  done
  echo "Application did not become ready within 60 seconds." >&2
  tail -n 80 "$log" >&2 || true
  return 1
}

start_app() {
  local environment="$1"
  local log="$2"
  APP_PID=""
  ASPNETCORE_ENVIRONMENT="$environment"   ASPNETCORE_URLS="http://127.0.0.1:5099"     dotnet run --no-build --no-launch-profile --project src/MobileShop.Web >"$log" 2>&1 &
  APP_PID=$!
}

stop_app() {
  cleanup
  APP_PID=""
}

fingerprint() {
  local output="$1"
  {
    echo "People=$(sqlite3 "$DB" 'SELECT COUNT(*) FROM People;')"
    echo "Products=$(sqlite3 "$DB" 'SELECT COUNT(*) FROM Products;')"
    echo "Phones=$(sqlite3 "$DB" 'SELECT COUNT(*) FROM Phones;')"
    echo "AppleIds=$(sqlite3 "$DB" 'SELECT COUNT(*) FROM AppleIds;')"
    echo "Transactions=$(sqlite3 "$DB" 'SELECT COUNT(*) FROM Transactions;')"
    echo "Categories=$(sqlite3 "$DB" 'SELECT COUNT(*) FROM Categories;')"
    echo "Integrity=$(sqlite3 "$DB" 'PRAGMA integrity_check;')"
  } >"$output"
}

start_app "Development" "$DEV_LOG"
wait_for_home "http://127.0.0.1:5099/" "$DEV_LOG"
stop_app

test -f "$DB"
fingerprint "$SMOKE_DIR/fingerprint-before.txt"
cp "$SMOKE_DIR/fingerprint-before.txt" "$SMOKE_DIR/fingerprint-before-copy.txt"

set +e
ASPNETCORE_ENVIRONMENT=Production ASPNETCORE_URLS="http://127.0.0.1:5099"   dotnet run --no-build --no-launch-profile --project src/MobileShop.Web >"$PROD_GUARD_LOG" 2>&1
GUARD_EXIT=$?
set -e

if [[ "$GUARD_EXIT" -eq 0 ]]; then
  echo "Production startup unexpectedly succeeded against a no-history database." >&2
  cat "$PROD_GUARD_LOG" >&2
  exit 1
fi
grep -F -- "--migrate-database" "$PROD_GUARD_LOG"

ASPNETCORE_ENVIRONMENT=Production   dotnet run --no-build --no-launch-profile --project src/MobileShop.Web -- --migrate-database >"$MIGRATE_LOG" 2>&1

BACKUPS=( "$DB".*.bak )
if [[ "${#BACKUPS[@]}" -ne 1 || ! -f "${BACKUPS[0]}" ]]; then
  echo "Expected exactly one verified database backup." >&2
  printf '%s\n' "${BACKUPS[@]}" >&2
  exit 1
fi

HISTORY_COUNT="$(sqlite3 "$DB" 'SELECT COUNT(*) FROM __EFMigrationsHistory;')"
[[ "$HISTORY_COUNT" == "6" ]]

fingerprint "$SMOKE_DIR/fingerprint-after-migrate.txt"
diff -u "$SMOKE_DIR/fingerprint-before.txt" "$SMOKE_DIR/fingerprint-after-migrate.txt"

start_app "Production" "$PROD_LOG"
wait_for_home "http://127.0.0.1:5099/" "$PROD_LOG"

for route in / /Products /Products/SecondHand /Transactions /People/Customers /People/Sellers /Reports/ProfitLoss; do
  curl --silent --show-error --fail --max-time 10 -o /dev/null "http://127.0.0.1:5099$route"
done

stop_app
fingerprint "$SMOKE_DIR/fingerprint-after-production.txt"
diff -u "$SMOKE_DIR/fingerprint-after-migrate.txt" "$SMOKE_DIR/fingerprint-after-production.txt"

BACKUPS=( "$DB".*.bak )
[[ "${#BACKUPS[@]}" -eq 1 && -f "${BACKUPS[0]}" ]]

{
  echo "Backup:"
  printf '%s\n' "${BACKUPS[@]}"
  echo
  echo "Migration command output:"
  cat "$MIGRATE_LOG"
} >"$SMOKE_DIR/backup-and-migration.txt"

echo "Production smoke passed."
